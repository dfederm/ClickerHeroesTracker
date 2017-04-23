// <copyright file="AccountController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ClickerHeroesTrackerWebsite.Models;
    using ClickerHeroesTrackerWebsite.Services.Email;
    using ClickerHeroesTrackerWebsite.ViewModels.Account;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Account controller is where users register and log in.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        /// <summary>
        /// GET: /Account/Login
        /// </summary>
        /// <param name="returnUrl">The url to redirect back to after login</param>
        /// <returns>The login view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        /// <summary>
        /// POST: /Account/Login
        /// </summary>
        /// <param name="model">The user-submitted login data</param>
        /// <param name="returnUrl">The url to redirect back to after login</param>
        /// <returns>A redirect result if successful, and error view otherwise</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            this.ViewBag.ReturnUrl = returnUrl;

            var userName = model.UserName;

            // Allow the user to log in with their email address too.
            // We already check that usernames are only "word" chars (\w+), so this check is sufficient.
            if (userName.Contains("@"))
            {
                var user = await this.userManager.FindByEmailAsync(userName);
                if (user == null)
                {
                    this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return this.View(model);
                }

                userName = user.UserName;
            }

            // This doesn't count login failures towards account lockout
            var result = await this.signInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return this.RedirectToLocal(returnUrl);
            }

            this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return this.View(model);
        }

        /// <summary>
        /// GET: /Account/Register
        /// </summary>
        /// <returns>The register view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Account/Register
        /// </summary>
        /// <param name="model">The user-submitted registration data</param>
        /// <returns>A redirect result if successful, and error view otherwise</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await this.userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    return this.RedirectToAction(nameof(HomeController.Index), "Home");
                }

                this.AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        /// <summary>
        /// GET: /Account/ConfirmEmail
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="code">The confirmation code sent to the user</param>
        /// <returns>A view with the status</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return this.View("Error");
            }

            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return this.View("Error");
            }

            var result = await this.userManager.ConfirmEmailAsync(user, code);
            return this.View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /// <summary>
        /// GET: /Account/ForgotPassword
        /// </summary>
        /// <returns>The forgot password view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Account/ForgotPassword
        /// </summary>
        /// <param name="model">The user-submitted forgot password data</param>
        /// <returns>The forgot password confirmation view</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                var code = await this.userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = this.Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: this.Request.Scheme);
                await this.emailSender.SendEmailAsync(model.Email, "Reset Password", "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                return this.View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        /// <summary>
        /// GET: /Account/ResetPassword
        /// </summary>
        /// <param name="code">The confirmation code</param>
        /// <returns>The reset password view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? this.View("Error") : this.View();
        }

        /// <summary>
        /// POST: /Account/ResetPassword
        /// </summary>
        /// <param name="model">The user-submitted reset password view</param>
        /// <returns>A redirection to the reset password confirmation view</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return this.RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }

            var result = await this.userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return this.RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }

            this.AddErrors(result);
            return this.View();
        }

        /// <summary>
        /// GET: /Account/ResetPasswordConfirmation
        /// </summary>
        /// <returns>The reset password confirmation view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Account/ExternalLogin
        /// </summary>
        /// <param name="provider">The external login provider</param>
        /// <param name="returnUrl">The url to redirect back to</param>
        /// <returns>The challenge result</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = this.Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = this.signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// GET: /Account/ExternalLoginCallback
        /// </summary>
        /// <param name="returnUrl">The url to redirect back to</param>
        /// <returns>A redirection result if the challenge succeeded, an error view otherwise</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await this.signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return this.RedirectToAction(nameof(this.Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await this.signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return this.RedirectToLocal(returnUrl);
            }

            // If the user does not have an account, then ask the user to create an account.
            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.LoginProvider = info.LoginProvider;
            return this.View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel());
        }

        /// <summary>
        /// POST: /Account/ExternalLoginConfirmation
        /// </summary>
        /// <param name="model">The user-submitted external login confirmation data</param>
        /// <param name="returnUrl">The url to redirect back to</param>
        /// <returns>A redirection result if the challenge succeeded, an error view otherwise</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (this.signInManager.IsSignedIn(this.User))
            {
                return this.RedirectToAction(nameof(ManageController.Index), "Manage");
            }

            if (this.ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await this.signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return this.View("ExternalLoginFailure");
                }

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = new ApplicationUser { UserName = model.UserName, Email = email };
                var result = await this.userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this.userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await this.signInManager.SignInAsync(user, isPersistent: false);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                this.AddErrors(result);
            }

            this.ViewBag.ReturnUrl = returnUrl;
            return this.View(model);
        }

        /// <summary>
        /// POST: /Account/LogOff
        /// </summary>
        /// <returns>A redirection to the homepage</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await this.signInManager.SignOutAsync();
            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// GET: /Account/ExternalLoginFailure
        /// </summary>
        /// <returns>An external login failure view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLoginFailure()
        {
            return this.View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
