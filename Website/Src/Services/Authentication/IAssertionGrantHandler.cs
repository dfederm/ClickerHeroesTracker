// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

using System.Threading.Tasks;

namespace Website.Services.Authentication
{
    public interface IAssertionGrantHandler
    {
        string Name { get; }

        Task<AssertionGrantResult> ValidateAsync(string assertion);
    }
}
