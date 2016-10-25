﻿using System;
using DataProtection.Azure;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.AspNetCore.DataProtection
{
    /// <summary>
    /// Contains Azure-specific extension methods for modifying a
    /// <see cref="IDataProtectionBuilder"/>.
    /// </summary>
    public static class AzureDataProtectionBuilderExtensions
    {
        /// <summary>
        /// Configures the data protection system to persist keys to the specified path
        /// in Azure Blob Storage.
        /// </summary>
        /// <param name="builder">The builder instance to modify.</param>
        /// <param name="storageAccount">The <see cref="CloudStorageAccount"/> which
        /// should be utilized.</param>
        /// <param name="relativePath">A relative path where the key file should be
        /// stored, generally specified as "/containerName/[subDir/]keys.xml".</param>
        /// <returns>The value <paramref name="config"/>.</returns>
        /// <remarks>
        /// The container referenced by <paramref name="relativePath"/> must already exist.
        /// </remarks>
        public static IDataProtectionBuilder PersistKeysToAzureBlobStorage(this IDataProtectionBuilder builder, CloudStorageAccount storageAccount, string relativePath)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (storageAccount == null)
            {
                throw new ArgumentNullException(nameof(storageAccount));
            }
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            // Simply concatenate the root storage endpoint with the relative path,
            // which includes the container name and blob name.

            UriBuilder uriBuilder = new UriBuilder(storageAccount.BlobEndpoint);
            uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + "/" + relativePath.TrimStart('/');

            // We can create a CloudBlockBlob from the storage URI and the creds.

            Uri blobAbsoluteUri = uriBuilder.Uri;
            var credentials = storageAccount.Credentials;

            return PersistKeystoAzureBlobStorageCore(builder, () => new CloudBlockBlob(blobAbsoluteUri, credentials));
        }

        /// <summary>
        /// Configures the data protection system to persist keys to the specified path
        /// in Azure Blob Storage.
        /// </summary>
        /// <param name="builder">The builder instance to modify.</param>
        /// <param name="blobUri">The full URI where the key file should be stored.
        /// The URI must contain the SAS token as a query string parameter.</param>
        /// <returns>The value <paramref name="builder"/>.</returns>
        /// <remarks>
        /// The container referenced by <paramref name="blobUri"/> must already exist.
        /// </remarks>
        public static IDataProtectionBuilder PersistKeysToAzureBlobStorage(this IDataProtectionBuilder builder, Uri blobUri)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (blobUri == null)
            {
                throw new ArgumentNullException(nameof(blobUri));
            }

            UriBuilder uriBuilder = new UriBuilder(blobUri);

            // The SAS token is present in the query string.

            if (String.IsNullOrEmpty(uriBuilder.Query))
            {
                throw new ArgumentException(
                    message: "URI does not have a SAS token in the query string.",
                    paramName: nameof(blobUri));
            }

            var credentials = new StorageCredentials(uriBuilder.Query);
            uriBuilder.Query = null; // no longer needed
            Uri blobAbsoluteUri = uriBuilder.Uri;

            return PersistKeystoAzureBlobStorageCore(builder, () => new CloudBlockBlob(blobAbsoluteUri, credentials));
        }

        /// <summary>
        /// Configures the data protection system to persist keys to the specified path
        /// in Azure Blob Storage.
        /// </summary>
        /// <param name="builder">The builder instance to modify.</param>
        /// <param name="blobReference">The <see cref="CloudBlockBlob"/> where the
        /// key file should be stored.</param>
        /// <returns>The value <paramref name="builder"/>.</returns>
        /// <remarks>
        /// The container referenced by <paramref name="blobReference"/> must already exist.
        /// </remarks>
        public static IDataProtectionBuilder PersistKeysToAzureBlobStorage(this IDataProtectionBuilder builder, CloudBlockBlob blobReference)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (blobReference == null)
            {
                throw new ArgumentNullException(nameof(blobReference));
            }

            // We're basically just going to make a copy of this blob.
            // Use (container, blobName) instead of (storageuri, creds) since the container
            // is tied to an existing service client, which contains user-settable defaults
            // like retry policy and secondary connection URIs.

            var container = blobReference.Container;
            string blobName = blobReference.Name;

            return PersistKeystoAzureBlobStorageCore(builder, () => container.GetBlockBlobReference(blobName));
        }

        /// <summary>
        /// Configures the data protection system to persist keys to the specified path
        /// in Azure Blob Storage.
        /// </summary>
        /// <param name="builder">The builder instance to modify.</param>
        /// <param name="container">The <see cref="CloudBlobContainer"/> in which the
        /// key file should be stored.</param>
        /// <param name="blobName">The name of the key file, generally specified
        /// as "[subdir/]keys.xml"</param>
        /// <returns>The value <paramref name="builder"/>.</returns>
        /// <remarks>
        /// The container referenced by <paramref name="container"/> must already exist.
        /// </remarks>
        public static IDataProtectionBuilder PersistKeysToAzureBlobStorage(this IDataProtectionBuilder builder, CloudBlobContainer container, string blobName)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            if (blobName == null)
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            return PersistKeystoAzureBlobStorageCore(builder, () => container.GetBlockBlobReference(blobName));
        }

        // important: the Func passed into this method must return a new instance with each call
        private static IDataProtectionBuilder PersistKeystoAzureBlobStorageCore(IDataProtectionBuilder builder, Func<CloudBlockBlob> blobRefFactory)
        {
            builder.Services.Add(ServiceDescriptor.Singleton<IXmlRepository>(services => new AzureBlobXmlRepository(blobRefFactory, services)));
            return builder;
        }
    }
}