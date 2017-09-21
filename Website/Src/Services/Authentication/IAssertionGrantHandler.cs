// <copyright file="IAssertionGrantHandler.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.Authentication
{
    using System.Threading.Tasks;

    public interface IAssertionGrantHandler
    {
        string Name { get; }

        Task<AssertionGrantResult> ValidateAsync(string assertion);
    }
}
