// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Collections.Generic;

namespace Website.Models.Api.Version
{
    public sealed class VersionResponse
    {
        public string Environment { get; set; }

        public string Changelist { get; set; }

        public string BuildUrl { get; set; }

        public IDictionary<string, string> Webclient { get; set; }
    }
}
