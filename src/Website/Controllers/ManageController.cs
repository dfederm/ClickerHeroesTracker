// <copyright file="ManageController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Email;
    using ClickerHeroesTrackerWebsite.Utility;
    using ClickerHeroesTrackerWebsite.ViewModels.Manage;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The manage controller allows users to manage their settings.
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        private readonly IUserSettingsProvider userSettingsProvider;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageController"/> class.
        /// </summary>
        public ManageController(
            IUserSettingsProvider userSettingsProvider,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.userSettingsProvider = userSettingsProvider;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        /// <summary>
        /// Represents a status message to the user
        /// </summary>
        /// <remarks>
        /// Although public, this enum is not intended to be used outside the <see cref="ManageController"/>
        /// </remarks>
        public enum ManageMessageId
        {
            /// <summary>
            /// The user's external login was successfully added.
            /// </summary>
            AddLoginSuccess,

            /// <summary>
            /// The user's password was successfully changed.
            /// </summary>
            ChangePasswordSuccess,

            /// <summary>
            /// The user's password was successfully set.
            /// </summary>
            SetPasswordSuccess,

            /// <summary>
            /// The user's external login was successfully removed.
            /// </summary>
            RemoveLoginSuccess,

            /// <summary>
            /// An error happened
            /// </summary>
            Error
        }

        /// <summary>
        /// GET: /Manage/Index
        /// </summary>
        /// <param name="message">The status of the user's last operation</param>
        /// <returns>The user's settings view</returns>
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            this.ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : string.Empty;

            var user = await this.userManager.GetUserAsync(this.User);
            var userSettings = this.userSettingsProvider.Get(user.Id);
            return await this.GetIndexResult(user, userSettings);
        }

        /// <summary>
        /// The user submits a change to their settings
        /// </summary>
        /// <param name="indexViewModel">The user-submitted settings data</param>
        /// <returns>The user's settings view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IndexViewModel indexViewModel)
        {
            this.ViewBag.StatusMessage = "Changes Saved";

            var user = await this.userManager.GetUserAsync(this.User);
            var userSettings = this.userSettingsProvider.Get(user.Id);

            userSettings.AreUploadsPublic = indexViewModel.AreUploadsPublic;
            userSettings.UseReducedSolomonFormula = indexViewModel.SolomonFormula.Equals("Log", StringComparison.OrdinalIgnoreCase);
            userSettings.PlayStyle = indexViewModel.PlayStyle.SafeParseEnum<PlayStyle>();
            userSettings.UseExperimentalStats = indexViewModel.UseExperimentalStats;
            userSettings.UseScientificNotation = indexViewModel.UseScientificNotation;
            userSettings.UseEffectiveLevelForSuggestions = indexViewModel.UseEffectiveLevelForSuggestions;
            userSettings.UseLogarithmicGraphScale = indexViewModel.UseLogarithmicGraphScale;

            if (indexViewModel.ScientificNotationThreshold.HasValue)
            {
                userSettings.ScientificNotationThreshold = indexViewModel.ScientificNotationThreshold.Value;
            }
            else if (userSettings.UseScientificNotation)
            {
                // If they cleared the value, reset to the default.
                userSettings.ScientificNotationThreshold = 1000000;
            }

            if (indexViewModel.LogarithmicGraphScaleThreshold.HasValue)
            {
                userSettings.LogarithmicGraphScaleThreshold = indexViewModel.LogarithmicGraphScaleThreshold.Value;
            }
            else if (userSettings.UseLogarithmicGraphScale)
            {
                // If they cleared the value, reset to the default.
                userSettings.LogarithmicGraphScaleThreshold = 1000000;
            }

            if (indexViewModel.HybridRatio.HasValue)
            {
                userSettings.HybridRatio = indexViewModel.HybridRatio.Value;
            }
            else if (userSettings.UseLogarithmicGraphScale)
            {
                // If they cleared the value, reset to the default.
                userSettings.HybridRatio = 10;
            }

            return await this.GetIndexResult(user, userSettings);
        }

        /// <summary>
        /// POST: /Manage/RemoveLogin
        /// </summary>
        /// <returns>A redirection to the manage logins page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await this.userManager.GetUserAsync(this.User);
            if (user != null)
            {
                var result = await this.userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }

            return this.RedirectToAction(nameof(this.ManageLogins), new { Message = message });
        }

        /// <summary>
        /// GET: /Manage/ChangePassword
        /// </summary>
        /// <returns>The change password view</returns>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Manage/ChangePassword
        /// </summary>
        /// <param name="model">The user-submitted change password data</param>
        /// <returns>A redirect to the settings page or an error view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.userManager.GetUserAsync(this.User);
            if (user != null)
            {
                var result = await this.userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }

                this.AddErrors(result);
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.Error });
        }

        /// <summary>
        /// GET: /Manage/SetPassword
        /// </summary>
        /// <returns>The set password view</returns>
        [HttpGet]
        public IActionResult SetPassword()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Manage/SetPassword
        /// </summary>
        /// <param name="model">The user-submitted password data</param>
        /// <returns>A redirect to the settings page or an error view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.userManager.GetUserAsync(this.User);
            if (user != null)
            {
                var result = await this.userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }

                this.AddErrors(result);
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Index), new { Message = ManageMessageId.Error });
        }

        /// <summary>
        /// GET: /Manage/ManageLogins
        /// </summary>
        /// <param name="message">The status of the user's last operation</param>
        /// <returns>The user's settings view</returns>
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            this.ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : string.Empty;
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.View("Error");
            }

            var userLogins = await this.userManager.GetLoginsAsync(user);
            var otherLogins = this.signInManager
                .GetExternalAuthenticationSchemes()
                .Where(auth => userLogins.All(ul => !string.Equals(auth.AuthenticationScheme, ul.LoginProvider, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            this.ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return this.View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins,
            });
        }

        /// <summary>
        /// POST: /Manage/LinkLogin
        /// </summary>
        /// <param name="provider">The external login provider name</param>
        /// <returns>A challenge result for the link</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = this.Url.Action("LinkLoginCallback", "Manage");
            var userId = this.userManager.GetUserId(this.User);
            var properties = this.signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// GET: /Manage/LinkLoginCallback
        /// </summary>
        /// <returns>A redirect to the external login manage page</returns>
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.View("Error");
            }

            var info = await this.signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info == null)
            {
                return this.RedirectToAction(nameof(this.ManageLogins), new { Message = ManageMessageId.Error });
            }

            var result = await this.userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
            return this.RedirectToAction(nameof(this.ManageLogins), new { Message = message });
        }

        private async Task<ActionResult> GetIndexResult(ApplicationUser user, IUserSettings userSettings)
        {
            var model = new IndexViewModel
            {
                HasPassword = await this.userManager.HasPasswordAsync(user),
                Logins = await this.userManager.GetLoginsAsync(user),
                AreUploadsPublic = userSettings.AreUploadsPublic,
                SolomonFormula = userSettings.UseReducedSolomonFormula ? "Log" : "Ln",
                PlayStyle = userSettings.PlayStyle.ToString(),
                UseExperimentalStats = userSettings.UseExperimentalStats,
                UseScientificNotation = userSettings.UseScientificNotation,
                ScientificNotationThreshold = userSettings.ScientificNotationThreshold,
                UseEffectiveLevelForSuggestions = userSettings.UseEffectiveLevelForSuggestions,
                UseLogarithmicGraphScale = userSettings.UseLogarithmicGraphScale,
                LogarithmicGraphScaleThreshold = userSettings.LogarithmicGraphScaleThreshold,
                HybridRatio = userSettings.HybridRatio,
            };

            return this.View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
