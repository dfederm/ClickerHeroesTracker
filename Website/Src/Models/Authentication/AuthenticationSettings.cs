// Copyright (C) Clicker Heroes Tracker. All Rights Reserved.

namespace Website.Models.Authentication
{
    public sealed class AuthenticationSettings
    {
        public MicrosoftAuthenticationSettings Microsoft { get; set; }

        public FacebookAuthenticationSettings Facebook { get; set; }

        public GoogleAuthenticationSettings Google { get; set; }
    }
}
