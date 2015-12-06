// <copyright file="UploadsController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Database;
    using Microsoft.AspNet.Identity;
    using Models.Api;
    using Models.Api.Uploads;

    [RoutePrefix("api/uploads")]
    public sealed class UploadsController : ApiController
    {
        private readonly IDatabaseCommandFactory databaseCommandFactory;

        public UploadsController(IDatabaseCommandFactory databaseCommandFactory)
        {
            this.databaseCommandFactory = databaseCommandFactory;
        }

        [Route("")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage UploadSummaryList(
            int page = ParameterConstants.UploadSummaryList.Page.Default,
            int count = ParameterConstants.UploadSummaryList.Count.Default)
        {
            // Validate parameters
            if (page < ParameterConstants.UploadSummaryList.Page.Min)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: page");
            }

            if (count < ParameterConstants.UploadSummaryList.Count.Min
                || count > ParameterConstants.UploadSummaryList.Count.Max)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: count");
            }

            var userId = this.User.Identity.GetUserId();
            var model = new UploadSummaryListResponse()
            {
                Uploads = this.FetchUploads(userId, page, count),
                Pagination = this.FetchPagination(userId, page, count),
            };

            return this.Request.CreateResponse(model);
        }

        [Route("{id:int}")]
        [HttpGet]
        public HttpResponseMessage UploadSummary(int id)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage Post(RawUpload rawUpload)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private List<UploadSummary> FetchUploads(string userId, int page, int count)
        {
            const string CommandText = @"
	            SELECT Id, UploadTime
	            FROM Uploads
	            WHERE UserId = @UserId
	            ORDER BY UploadTime DESC
		            OFFSET @Offset ROWS
		            FETCH NEXT @Count ROWS ONLY;";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@Offset", (page - 1) * count },
                { "@Count", count },
            };

            using (var command = this.databaseCommandFactory.Create(CommandText, parameters))
            using (var reader = command.ExecuteReader())
            {
                var uploads = new List<UploadSummary>(count);
                while (reader.Read())
                {
                    uploads.Add(new UploadSummary
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TimeSubmitted = Convert.ToDateTime(reader["UploadTime"])
                    });
                }

                return uploads;
            }
        }

        private PaginationMetadata FetchPagination(string userId, int page, int count)
        {
            const string GetUploadCountCommandText = @"
	            SELECT COUNT(*) AS TotalUploads
		        FROM Uploads
		        WHERE UserId = @UserId";
            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId },
            };

            using (var command = this.databaseCommandFactory.Create(GetUploadCountCommandText, parameters))
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }

                var pagination = new PaginationMetadata
                {
                    Count = Convert.ToInt32(reader["TotalUploads"])
                };

                var currentPath = this.Request.RequestUri.LocalPath;
                if (page > 1)
                {
                    pagination.Previous = string.Format(
                        "{0}?{1}={2}&{3}={4}",
                        currentPath,
                        nameof(page),
                        page - 1,
                        nameof(count),
                        count);
                }

                if (page <= Math.Ceiling((float)pagination.Count / count))
                {
                    pagination.Next = string.Format(
                        "{0}?{1}={2}&{3}={4}",
                        currentPath,
                        nameof(page),
                        page + 1,
                        nameof(count),
                        count);
                }

                return pagination;
            }
        }

        internal static class ParameterConstants
        {
            internal static class UploadSummaryList
            {
                internal static class Page
                {
                    internal const int Min = 1;

                    internal const int Default = 1;
                }

                internal static class Count
                {
                    internal const int Min = 1;

                    internal const int Max = 100;

                    internal const int Default = 10;
                }
            }
        }
    }
}