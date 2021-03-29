// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace Website.Services.Authentication
{
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

            if (AssertionGrantTypeMap.ContainsKey(grantType))
            {
                throw new InvalidOperationException("Grant type already exists: " + grantType);
            }

            AssertionGrantTypeMap.Add(grantType, typeof(THandler));
        }
    }
}
