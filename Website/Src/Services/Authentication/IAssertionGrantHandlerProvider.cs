// <copyright file="IAssertionGrantHandlerProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Authentication
{
    public interface IAssertionGrantHandlerProvider
    {
        IAssertionGrantHandler GetHandler(string grantType);
    }
}
