// <copyright file="AuthenticationSettings.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Services.Authentication
{
    public sealed class AuthenticationSettings
    {
        public MicrosoftAuthenticationSettings Microsoft { get; set; }

        public FacebookAuthenticationSettings Facebook { get; set; }

        public GoogleAuthenticationSettings Google { get; set; }

        public TwitterAuthenticationSettings Twitter { get; set; }
    }
}
