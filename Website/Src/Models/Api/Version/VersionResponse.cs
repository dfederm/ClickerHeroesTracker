// <copyright file="VersionResponse.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Api.Version
{
    using System.Collections.Generic;

    public sealed class VersionResponse
    {
        public string Environment { get; set; }

        public string Changelist { get; set; }

        public string BuildUrl { get; set; }

        public IDictionary<string, string> Webclient { get; set; }
    }
}
