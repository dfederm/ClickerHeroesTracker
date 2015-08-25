namespace ClickerHeroesTrackerWebsite.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using System;
    using System.Linq;

    public class IndexViewModel
    {
        public static IEnumerable<TimeZoneSelectItem> TimeZones = TimeZoneInfo
            .GetSystemTimeZones()
            .Select(tz => new TimeZoneSelectItem { Id = tz.Id, Name = tz.DisplayName });

        public class TimeZoneSelectItem
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        [Display(Name = "Time Zone")]
        public string TimeZoneId { get; set; }

        [Display(Name = "Public uploads")]
        public bool AreUploadsPublic { get; set; }

        [Display(Name = "Solomon formula")]
        public string SolomonFormula { get; set; }

        [Display(Name = "Play Style")]
        public string PlayStyle { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}