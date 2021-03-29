// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace Website.Services.Authentication
{
    public interface IAssertionGrantHandlerProvider
    {
        IAssertionGrantHandler GetHandler(string grantType);
    }
}
