﻿using CIAT.DAPA.USAID.Forecast.Data.Enums;
using CIAT.DAPA.USAID.Forecast.WebAdmin.Models.Account;
using CIAT.DAPA.USAID.Forecast.WebAdmin.Models.Tools;
using CIAT.DAPA.USAID.Forecast.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CIAT.DAPA.USAID.Forecast.WebAdmin.Controllers
{
    [Authorize]
    public class AccountController : WebAdminBaseController
    {
        /// <summary>
        /// Method Construct
        /// </summary>
        /// <param name="settings">Settings options</param>
        /// <param name="hostingEnvironment">Host Enviroment</param>
        public AccountController(IOptions<Settings> settings, IHostingEnvironment hostingEnvironment, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender) :
            base(settings, LogEntity.users, hostingEnvironment, userManager, signInManager, roleManager, emailSender)
        {
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await managerSignIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        await writeEventAsync("User trying login: " + model.Email + " - Succeeded", LogEvent.rea);
                        return RedirectToLocal(returnUrl);
                    }
                    if (result.IsLockedOut)
                    {
                        await writeEventAsync("User trying login: " + model.Email + " - Locked", LogEvent.rea);
                        ModelState.AddModelError(string.Empty, "Usuario bloqueado");
                        return View(model);
                    }
                    else
                    {
                        await writeEventAsync("User trying login: " + model.Email + " - Error", LogEvent.rea);
                        ModelState.AddModelError(string.Empty, "Usuario o password inválidos");
                        return View(model);
                    }
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                await writeExceptionAsync(ex);
                return View(model);
            }
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            try
            {
                await writeEventAsync("Trying confirm email: " + userId ?? string.Empty + " code: " + code ?? string.Empty, LogEvent.rea);
                if (userId == null || code == null)
                {
                    return View("Error");
                }
                var user = await managerUser.FindByIdAsync(userId);
                if (user == null)
                {
                    await writeEventAsync("Trying confirm email: " + userId + " - user not found ", LogEvent.rea);
                    return View("Error");
                }
                var result = await managerUser.ConfirmEmailAsync(user, code);
                await writeEventAsync("Trying confirm email: " + userId ?? string.Empty + " - " + (result.Succeeded ? "Confirmed" : "Not confirmed"), LogEvent.rea);
                return View(result.Succeeded ? "ConfirmEmail" : "Error");
            }
            catch (Exception ex)
            {
                await writeExceptionAsync(ex);
                return View("Error");
            }

        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await writeEventAsync("Forgot Password email: " + model.Email, LogEvent.rea);
                    var user = await managerUser.FindByNameAsync(model.Email);
                    if (user == null || !(await managerUser.IsEmailConfirmedAsync(user)))
                    {
                        await writeEventAsync("Forgot Password email: " + model.Email + " - user not found", LogEvent.rea);
                        // Don't reveal that the user does not exist or is not confirmed
                        return View("ForgotPasswordConfirmation");
                    }

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await managerUser.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await notifyEmail.SendEmailAsync(model.Email, "Se te olvidó tu password",
                       $"<p style=\"text-align:justify;\">Estimado usuario<br/><br/>Por favor cambía tu password haciendo click en el siguiente <a href=\"{callbackUrl}\">link</a></p>");
                    return View("ForgotPasswordConfirmation");
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                await writeExceptionAsync(ex);
                return View("Error");
            }
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/Login
        [HttpGet]
        [Authorize(Roles = "ADMIN,TECH")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await db.user.listAllAsync();
                await writeEventAsync("List all users " + users.Count.ToString(), LogEvent.lis);
                return View(users);
            }
            catch (Exception ex)
            {
                await writeExceptionAsync(ex);
                return View("Error");
            }
        }

        // GET: /Account/Register
        [HttpGet]
        [Authorize(Roles = "ADMIN,TECH")]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Role = Role.ROLES_PLATFORM.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,TECH")]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {
                    await registerUserAsync(model.Email, model.Password, model.Role);
                    return RedirectToAction("Index");
                }
                // If we got this far, something failed, redisplay form
                ViewBag.Role = Role.ROLES_PLATFORM.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                await writeExceptionAsync(ex);
                return View("Error");
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await managerSignIn.SignOutAsync();
            //_logger.LogInformation(4, "User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string code = null)
        {
            try
            {
                await writeEventAsync("User trying reset password [" + code ?? string.Empty + "]", LogEvent.upd);
                return code == null ? View("Error") : View();
            }
            catch (Exception ex)
            {
                await writeExceptionAsync(ex);
                return View("Error");
            }

        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                await writeEventAsync("User trying to change the password", LogEvent.upd);
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var user = await managerUser.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    await writeEventAsync("User trying to change the password. User doesn't exist [" + model.Email + "]", LogEvent.upd);
                    return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
                }
                var result = await managerUser.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    await writeEventAsync("User trying to change the password. Password changed [" + model.Email + "]", LogEvent.upd);
                    return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
                }
                //AddErrors(result);
                return View();
            }
            catch(Exception ex)
            {
                await writeExceptionAsync(ex);
                return View("Error");
            }            
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}