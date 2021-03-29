// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.IO;

namespace UnitTests.Mocks
{
    internal sealed class TemporaryDirectory : IDisposable
    {
        private const string BaseDir = ".tmp";

        private bool _isDisposed;

        public TemporaryDirectory()
        {
            string guid = Guid.NewGuid().ToString();
            Location = Path.Combine(BaseDir, guid);

            Directory.CreateDirectory(BaseDir);
            Directory.CreateDirectory(Location);
        }

        public string Location { get; }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    Directory.Delete(Location, true);
                }
                catch
                {
                    // Not the end of the world if we couldn't clean up
                }

                _isDisposed = true;
            }
        }
    }
}
