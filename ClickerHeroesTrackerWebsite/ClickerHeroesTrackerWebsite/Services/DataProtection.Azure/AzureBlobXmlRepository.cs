using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNet.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DataProtection.Azure
{
    /// <summary>
    /// An <see cref="IXmlRepository"/> which is backed by Azure Blob Storage.
    /// </summary>
    /// <remarks>
    /// Instances of this type are thread-safe.
    /// </remarks>
    public sealed class AzureBlobXmlRepository : IXmlRepository
    {
        private static readonly TimeSpan ConflictBackoffPeriod = TimeSpan.FromMilliseconds(200);
        private const int ConflictMaxRetries = 5;
        private static readonly XName RepositoryElementName = "repository";

        private readonly Func<ICloudBlob> _blobRefFactory;
        private BlobData _cachedBlobData;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="AzureBlobXmlRepository"/>.
        /// </summary>
        /// <param name="blobRefFactory">A factory which can create <see cref="ICloudBlob"/>
        /// instances. The factory must be thread-safe for invocation by multiple
        /// concurrent threads, and each invocation must return a new object.</param>
        /// <param name="services">Optional services consumed by this instance.</param>
        public AzureBlobXmlRepository(Func<ICloudBlob> blobRefFactory, IServiceProvider services)
        {
            if (blobRefFactory == null)
            {
                throw new ArgumentNullException(nameof(blobRefFactory));
            }

            _blobRefFactory = blobRefFactory;
            _logger = services?.GetLogger<AzureBlobXmlRepository>();
        }

        private XDocument CreateDocumentFromBlob(byte[] blob)
        {
            // Use an XmlReader with safe settings to process the document

            return XDocument.Load(XmlReader.Create(new MemoryStream(blob), new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreProcessingInstructions = true,
                XmlResolver = null
            }));
        }

        private ICloudBlob CreateFreshBlobRef()
        {
            // ICloudBlob instances aren't thread-safe, so we need to make sure we're working
            // with a fresh instance that won't be mutated by another thread.

            ICloudBlob blobRef = _blobRefFactory();
            if (blobRef == null)
            {
                throw new InvalidOperationException("The ICloudBlob factory method returned null.");
            }

            return blobRef;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            ICloudBlob blobRef = CreateFreshBlobRef();

            // Shunt the work onto a ThreadPool thread so that it's independent of any
            // existing sync context or other potentially deadlock-causing items.

            IList<XElement> elements = Task.Run(() => GetAllElementsAsync(blobRef)).SyncAwait();
            return new ReadOnlyCollection<XElement>(elements);
        }

        private async Task<IList<XElement>> GetAllElementsAsync(ICloudBlob blobRef)
        {
            BlobData data = await GetLatestDataAsync(blobRef);

            if (data == null)
            {
                // no data in blob storage
                return new XElement[0];
            }

            // The document will look like this:
            //
            // <root>
            //   <child />
            //   <child />
            //   ...
            // </root>
            //
            // We want to return the first-level child elements to our caller.

            XDocument doc = CreateDocumentFromBlob(data.BlobContents);
            return doc.Root.Elements().ToList();
        }

        private async Task<BlobData> GetLatestDataAsync(ICloudBlob blobRef)
        {
            // Set the appropriate AccessCondition based on what we believe the latest
            // file contents to be, then make the request.

            BlobData latestCachedData = Volatile.Read(ref _cachedBlobData); // local ref so field isn't mutated under our feet
            AccessCondition accessCondition = (latestCachedData != null)
                ? AccessCondition.GenerateIfNoneMatchCondition(latestCachedData.ETag)
                : null;

            MemoryStream memStream = new MemoryStream();
            try
            {
                await blobRef.DownloadToStreamAsync(
                    target: memStream,
                    accessCondition: accessCondition,
                    options: null,
                    operationContext: null);

                // At this point, our original cache either didn't exist or was outdated.
                // We'll update it now and return the updated value;

                latestCachedData = new BlobData()
                {
                    BlobContents = memStream.ToArray(),
                    ETag = blobRef.Properties.ETag
                };

                Volatile.Write(ref _cachedBlobData, latestCachedData);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 304)
                {
                    // 304 Not Modified
                    // Thrown when we already have the latest cached data.
                    // This isn't an error; we'll return our cached copy of the data.
                }
                else if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    // 404 Not Found
                    // Thrown when no file exists in storage.
                    // This isn't an error; we'll delete our cached copy of data.

                    latestCachedData = null;
                    Volatile.Write(ref _cachedBlobData, latestCachedData);
                }
                else
                {
                    throw; // unhandled
                }
            }

            return latestCachedData;
        }

        private static TimeSpan GetRandomizedBackoffPeriod()
        {
            // returns a TimeSpan in the range [0.8, 1.0) * ConflictBackoffPeriod

            Random random = new Random(); // not used for crypto purposes
            var multiplier = 0.8 + (random.NextDouble() * 0.2);
            return TimeSpan.FromTicks((long)(multiplier * ConflictBackoffPeriod.Ticks));
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            ICloudBlob blobRef = CreateFreshBlobRef();

            // Shunt the work onto a ThreadPool thread so that it's independent of any
            // existing sync context or other potentially deadlock-causing items.
            Task.Run(() => StoreElementAsync(blobRef, element)).SyncAwait();
        }

        private async Task StoreElementAsync(ICloudBlob blobRef, XElement element)
        {
            // holds the last error in case we need to rethrow it
            ExceptionDispatchInfo lastError = null;

            for (int i = 0; i < ConflictMaxRetries; i++)
            {
                if (i > 1)
                {
                    // If multiple conflicts occurred, wait a small period of time before retrying
                    // the operation so that other writers can make forward progress.
                    await Task.Delay(GetRandomizedBackoffPeriod());
                }

                if (i > 0)
                {
                    // If at least one conflict occurred, make sure we have an up-to-date
                    // view of the blob contents.
                    await GetLatestDataAsync(blobRef);
                }

                // Merge the new element into the document. If no document exists,
                // create a new default document and inject this element into it.

                BlobData latestData = Volatile.Read(ref _cachedBlobData);
                XDocument doc = (latestData != null)
                    ? CreateDocumentFromBlob(latestData.BlobContents)
                    : new XDocument(new XElement(RepositoryElementName));
                doc.Root.Add(element);

                // Turn this document back into a byte[].

                MemoryStream serializedDoc = new MemoryStream();
                doc.Save(serializedDoc, SaveOptions.None);

                // Generate the appropriate precondition header based on whether or not
                // we believe data already exists in storage.

                AccessCondition accessCondition;
                if (latestData != null)
                {
                    accessCondition = AccessCondition.GenerateIfMatchCondition(blobRef.Properties.ETag);
                }
                else
                {
                    accessCondition = AccessCondition.GenerateIfNotExistsCondition();
                    blobRef.Properties.ContentType = "application/xml; charset=utf-8"; // set content type on first write
                }

                try
                {
                    // Send the request up to the server.

                    byte[] serializedDocAsByteArray = serializedDoc.ToArray();

                    await blobRef.UploadFromByteArrayAsync(
                        buffer: serializedDocAsByteArray,
                        index: 0,
                        count: serializedDocAsByteArray.Length,
                        accessCondition: accessCondition,
                        options: null,
                        operationContext: null);

                    // If we got this far, success!
                    // We can update the cached view of the remote contents.

                    Volatile.Write(ref _cachedBlobData, new BlobData()
                    {
                        BlobContents = serializedDocAsByteArray,
                        ETag = blobRef.Properties.ETag // was updated by Upload routine
                    });

                    return;
                }
                catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 409 || ex.RequestInformation.HttpStatusCode == 412)
                {
                    // 409 Conflict
                    // This error is rare but can be thrown in very special circumstances,
                    // such as if the blob in the process of being created. We treat it
                    // as equivalent to 412 for the purposes of retry logic.

                    // 412 Precondition Failed 
                    // We'll get this error if another writer updated the repository and we
                    // have an outdated view of its contents. If this occurs, we'll just
                    // refresh our view of the remote contents and try again up to the max
                    // retry limit.

                    lastError = ExceptionDispatchInfo.Capture(ex);
                    continue;
                }
            }

            // if we got this far, something went awry
            lastError.Throw();
        }

        private sealed class BlobData
        {
            internal byte[] BlobContents;
            internal string ETag;
        }
    }
}