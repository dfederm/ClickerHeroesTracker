// <copyright file="JsonWebToken.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Models.Authentication
{
    public sealed class JsonWebToken
    {
        /// <summary>
        /// Gets or sets the principal that issued the JWT
        /// </summary>
        public string Iss { get; set; }

        /// <summary>
        /// Gets or sets the subject of the JWT
        /// </summary>
        public string Sub { get; set; }

        /// <summary>
        /// Gets or sets the recipients ("audience") that the JWT is intended for
        /// </summary>
        public string Aud { get; set; }

        /// <summary>
        /// Gets or sets the time at which the JWT was issued ("issued at")
        /// </summary>
        public string Iat { get; set; }

        /// <summary>
        /// Gets or sets the expiration time on or after which the JWT MUST NOT be accepted for processing.
        /// </summary>
        public string Exp { get; set; }
    }
}
