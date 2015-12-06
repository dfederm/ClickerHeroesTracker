// <copyright file="DisposableBase.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;

    public abstract class DisposableBase : IDisposable
    {
        private bool isDisposed;

        ~DisposableBase()
        {
            if (!this.isDisposed)
            {
                this.Dispose(false);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Dispose(true);
                this.isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        protected abstract void Dispose(bool isDisposing);

        protected void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("DatabaseCommandFactory");
            }
        }
    }
}