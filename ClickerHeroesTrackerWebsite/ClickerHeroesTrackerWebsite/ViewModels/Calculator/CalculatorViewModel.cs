// <copyright file="CalculatorViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using ClickerHeroesTrackerWebsite.Models.Game;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// The model for the calculator view.
    /// </summary>
    public class CalculatorViewModel
    {
        private static int[] suggestedAncientIds = new int[]
        {
            AncientIds.Argaiv,
            AncientIds.Atman,
            AncientIds.Bhaal,
            AncientIds.Bubos,
            AncientIds.Chronos,
            AncientIds.Dogcog,
            AncientIds.Dora,
            AncientIds.Fortuna,
            AncientIds.Fragsworth,
            AncientIds.Juggernaut,
            AncientIds.Kumawakamaru,
            AncientIds.Libertas,
            AncientIds.Mammon,
            AncientIds.Mimzee,
            AncientIds.Morgulis,
            AncientIds.Siyalatas,
            AncientIds.Solomon,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorViewModel"/> class.
        /// </summary>
        public CalculatorViewModel(
            IDatabaseCommandFactory databaseCommandFactory,
            IUserSettingsProvider userSettingsProvider,
            int uploadId,
            ClaimsPrincipal user,
            UserManager<ApplicationUser> userManager)
        {
            var userId = userManager.GetUserId(user);

            this.UploadId = uploadId;

            string uploadUserId;

            var parameters = new Dictionary<string, object>
            {
                { "@UploadId", uploadId },
            };
            const string GetUploadUserIdCommandText = @"
	            SELECT UserId
                FROM Uploads
                WHERE Uploads.Id = @UploadId";
            using (var command = databaseCommandFactory.Create(
                GetUploadUserIdCommandText,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    uploadUserId = reader["UserId"].ToString();
                }
                else
                {
                    return;
                }
            }

            var uploadUserSettings = userSettingsProvider.Get(uploadUserId);

            var isUploadAnonymous = string.IsNullOrEmpty(uploadUserId);
            this.IsOwn = string.Equals(userId, uploadUserId, StringComparison.OrdinalIgnoreCase);
            this.IsPublic = isUploadAnonymous || uploadUserSettings.AreUploadsPublic;
            this.IsPermitted = this.IsOwn || this.IsPublic || user.IsInRole("Admin");

            this.SuggestedAncientIds = suggestedAncientIds;

            this.IsValid = true;
        }

        /// <summary>
        /// Gets a value indicating whether the upload is the user's own upload.
        /// </summary>
        public bool IsOwn { get; }

        /// <summary>
        /// Gets a value indicating whether the upload is public.
        /// </summary>
        public bool IsPublic { get; }

        /// <summary>
        /// Gets the upload id.
        /// </summary>
        public int UploadId { get; }

        /// <summary>
        /// Gets a model for the suggested ancient levels view.
        /// </summary>
        public IList<int> SuggestedAncientIds { get; }

        /// <summary>
        /// Gets a value indicating whether the user is permitted to view the upload.
        /// </summary>
        internal bool IsPermitted { get; }

        /// <summary>
        /// Gets a value indicating whether the model is valid
        /// </summary>
        internal bool IsValid { get; }
    }
}