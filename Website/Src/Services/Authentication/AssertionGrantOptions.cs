// <copyright file="AssertionGrantOptions.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Authentication
{
    using System;
    using System.Collections.Generic;

    public sealed class AssertionGrantOptions
    {
        public IDictionary<string, Type> AssertionGrantTypeMap { get; } = new Dictionary<string, Type>(StringComparer.Ordinal);

        public void AddAssertionGrantType<THandler>(string grantType)
            where THandler : IAssertionGrantHandler
        {
            if (grantType == null)
            {
                throw new ArgumentNullException(nameof(grantType));
            }

            if (this.AssertionGrantTypeMap.ContainsKey(grantType))
            {
                throw new InvalidOperationException("Grant type already exists: " + grantType);
            }

            this.AssertionGrantTypeMap.Add(grantType, typeof(THandler));
        }
    }
}
