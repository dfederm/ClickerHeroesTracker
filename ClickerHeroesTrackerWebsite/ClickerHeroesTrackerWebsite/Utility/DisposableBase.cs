// <copyright file="DisposableBase.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Utility
{
    using System;

    /// <summary>
    /// A base class for the common dispoable pattern.
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="DisposableBase"/> class.
        /// </summary>
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

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <remarks>
        /// The implementor can assume that this will only be called once, even if
        /// <see cref="IDisposable.Dispose"/> is called multiple times.
        /// </remarks>
        /// <param name="isDisposing">A value indicating whether the object is being disposed or finalized.</param>
        protected abstract void Dispose(bool isDisposing);

        /// <summary>
        /// Assert as to whether the object is disposed or not. Subclasses should
        /// call this before performing operations, especially ones that may use
        /// resources which may have been cleaned up in <see cref="Dispose(bool)"/>.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("DatabaseCommandFactory");
            }
        }
    }
}