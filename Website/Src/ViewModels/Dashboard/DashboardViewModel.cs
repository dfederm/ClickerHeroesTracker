// <copyright file="DashboardViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Dashboard
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using ClickerHeroesTrackerWebsite.Services.Database;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// The model for the dashboard view.
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel(
            IDatabaseCommandFactory databaseCommandFactory,
            ClaimsPrincipal user,
            UserManager<ApplicationUser> userManager)
        {
            var userId = userManager.GetUserId(user);

            this.Follows = new List<string>();
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };
            const string GetUserFollowsCommandText = @"
	            SELECT UserName
	            FROM UserFollows
	            INNER JOIN AspNetUsers
	            ON UserFollows.FollowUserId = AspNetUsers.Id
	            WHERE UserId = @UserId
	            ORDER BY UserName ASC";
            using (var command = databaseCommandFactory.Create(
                GetUserFollowsCommandText,
                parameters))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    this.Follows.Add(reader["UserName"].ToString());
                }
            }
        }

        /// <summary>
        /// Gets the names of the users the user follows.
        /// </summary>
        public IList<string> Follows { get; }
    }
}