// <copyright file="IUploadProcessor.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.UploadProcessing
{
    /// <summary>
    /// Handles processing for uploads
    /// </summary>
    public interface IUploadProcessor
    {
        /// <summary>
        /// Start continuously processing
        /// </summary>
        void Start();

        /// <summary>
        /// Stops processing
        /// </summary>
        void Stop();
    }
}
