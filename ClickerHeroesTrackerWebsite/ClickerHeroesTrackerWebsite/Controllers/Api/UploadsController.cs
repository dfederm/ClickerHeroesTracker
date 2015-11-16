namespace ClickerHeroesTrackerWebsite.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Microsoft.AspNet.Identity;
    using Models;
    using Models.Api;
    using Models.Api.Uploads;

    [RoutePrefix("api/uploads")]
    public sealed class UploadsController : ApiController
    {
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
            var model = new UploadSummaryListResponse();

            // Fetch data
            var uploads = new List<UploadSummary>(count);
            using (var command = new DatabaseCommand("GetUserUploads"))
            {
                command.AddParameter("@UserId", userId);
                command.AddParameter("@Offset", (page - 1) * count);
                command.AddParameter("@Count", count);

                var returnParameter = command.AddReturnParameter();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uploads.Add(new UploadSummary
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            TimeSubmitted = Convert.ToDateTime(reader["UploadTime"])
                        });
                    }

                    model.Uploads = uploads;

                    // Move beyond the result above.
                    reader.NextResult();

                    var pagination = new PaginationMetadata
                    {
                        Count = Convert.ToInt32(returnParameter.Value)
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

                    model.Pagination = pagination;
                }
            }

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