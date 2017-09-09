// <copyright file="TemporaryDirectory.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Mocks
{
    using System;
    using System.IO;

    internal sealed class TemporaryDirectory : IDisposable
    {
        private const string BaseDir = ".tmp";

        private bool isDisposed;

        public TemporaryDirectory()
        {
            var guid = Guid.NewGuid().ToString();
            this.Location = Path.Combine(BaseDir, guid);

            Directory.CreateDirectory(BaseDir);
            Directory.CreateDirectory(this.Location);
        }

        public string Location { get; }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                try
                {
                    Directory.Delete(this.Location, true);
                }
                catch
                {
                    // Not the end of the world if we couldn't clean up
                }

                this.isDisposed = true;
            }
        }
    }
}
