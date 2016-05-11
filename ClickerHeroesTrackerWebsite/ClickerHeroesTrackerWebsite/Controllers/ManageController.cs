// <copyright file="ManageController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Mvc;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using ClickerHeroesTrackerWebsite.Services.Email;
    using ClickerHeroesTrackerWebsite.ViewModels.Manage;

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

            var userId = this.User.GetUserId();
            var userSettings = this.userSettingsProvider.Get(userId);
            return await this.GetIndexResult(userId, userSettings);
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

            var userId = this.User.GetUserId();
            var userSettings = this.userSettingsProvider.Get(userId);

            userSettings.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(indexViewModel.TimeZoneId);
            userSettings.AreUploadsPublic = indexViewModel.AreUploadsPublic;
            userSettings.UseReducedSolomonFormula = indexViewModel.SolomonFormula.Equals("Log", StringComparison.OrdinalIgnoreCase);
            userSettings.PlayStyle = indexViewModel.PlayStyle.SafeParseEnum<PlayStyle>();
            userSettings.UseExperimentalStats = indexViewModel.UseExperimentalStats;
            userSettings.UseScientificNotation = indexViewModel.UseScientificNotation;

            if (indexViewModel.ScientificNotationThreshold.HasValue)
            {
                userSettings.ScientificNotationThreshold = indexViewModel.ScientificNotationThreshold.Value;
            }
            else if (userSettings.UseScientificNotation)
            {
                // If they cleared the value, reset to the default.
                userSettings.ScientificNotationThreshold = 1000000;
            }

            return await this.GetIndexResult(userId, userSettings);
        }

        /// <summary>
        /// POST: /Manage/RemoveLogin
        /// </summary>
        /// <param name="loginProvider">The external login provider name</param>
        /// <param name="providerKey">The login provider key</param>
        /// <returns>A redirection to the manage logins page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                var result = await this.userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }

            return this.RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        /// <summary>
        /// GET: /Manage/ChangePassword
        /// </summary>
        /// <returns>The change password view</returns>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await this.userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }

                this.AddErrors(result);
                return this.View(model);
            }

            return this.RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
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

            var user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                var result = await this.userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }

                this.AddErrors(result);
                return this.View(model);
            }

            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
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
            var user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var userLogins = await this.userManager.GetLoginsAsync(user);
            var otherLogins = this.signInManager
                .GetExternalAuthenticationSchemes()
                .Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider))
                .ToList();
            this.ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return this.View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
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
            var properties = this.signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, this.User.GetUserId());
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// GET: /Manage/LinkLoginCallback
        /// </summary>
        /// <returns>A redirect to the external login manage page</returns>
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                return this.View("Error");
            }

            var info = await this.signInManager.GetExternalLoginInfoAsync(this.User.GetUserId());
            if (info == null)
            {
                return this.RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }

            var result = await this.userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
            return this.RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        private async Task<ActionResult> GetIndexResult(string userId, IUserSettings userSettings)
        {
            var user = await this.GetCurrentUserAsync(userId);
            var model = new IndexViewModel
            {
                HasPassword = await this.userManager.HasPasswordAsync(user),
                Logins = await this.userManager.GetLoginsAsync(user),
                TimeZoneId = userSettings.TimeZone.Id,
                AreUploadsPublic = userSettings.AreUploadsPublic,
                SolomonFormula = userSettings.UseReducedSolomonFormula ? "Log" : "Ln",
                PlayStyle = userSettings.PlayStyle.ToString(),
                UseExperimentalStats = userSettings.UseExperimentalStats,
                UseScientificNotation = userSettings.UseScientificNotation,
                ScientificNotationThreshold = userSettings.ScientificNotationThreshold,
            };

            return this.View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<ApplicationUser> GetCurrentUserAsync(string userId = null)
        {
            return await this.userManager.FindByIdAsync(userId ?? this.User.GetUserId());
        }
    }
}
