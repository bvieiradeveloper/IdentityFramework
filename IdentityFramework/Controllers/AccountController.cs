using IdentityFramework.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;


namespace IdentityFramework.Controllers
{

    public class AccountController : BaseController
    {
        UserManager<UserBase> _userManager;
        SignInManager<UserBase> _signInManager;
        IEmailSender _emailSender;
        public AccountController(UserManager<UserBase> userManager, SignInManager<UserBase> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public ActionResult Register()
        {
            return View("Register");
        }
        [HttpPost]
        public async Task<IActionResult> Register(AccountRegisterViewModel accountRegister)
        {
            if (!ModelState.IsValid) return View(accountRegister);

            var user = new UserBase
            {
                UserName = accountRegister.UserName,
                Email = accountRegister.Email,

            };
            var userExists = _userManager.FindByEmailAsync(accountRegister.Email) is null;

            if (userExists) return BadRequest(new { error = "User already exists" });

            var result = await _userManager.CreateAsync(user, accountRegister.Password);

            if (result.Succeeded)
            {

                var token = _userManager.GenerateEmailConfirmationTokenAsync(user);

                var linkCallBak =
                    Url.Action("RegisterConfirmation", "Account", new { userId = user.Id, token = token.Result }, Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Email confirm", linkCallBak);

                return View("EmailConfirm");

            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(accountRegister);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> RegisterConfirmation(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return View("ErrorEmailConfirm");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View("Login");
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel accountLoginViewModel)
        {

            var user = await _userManager.FindByEmailAsync(accountLoginViewModel.Email);

            if(user  is null)
            {
                ModelState.AddModelError("", "Login/Password invalid");
            }

            if (!ModelState.IsValid) return View(accountLoginViewModel);

            var result = await _signInManager.PasswordSignInAsync(user,
                                                                  accountLoginViewModel.Password,
                                                                  isPersistent: true,

                                                                  lockoutOnFailure: true);
            if (result.Succeeded)
            {
               
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, accountLoginViewModel.Email),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity),
                                              authProperties);

                RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                if (!(await _userManager.CheckPasswordAsync(user, accountLoginViewModel.Password)))
                {
                    ModelState.AddModelError("", "User/Password invalid");
                    return View("Login");
                }

                ModelState.AddModelError("", "Your account are temporaly bloqued!");
                return View("Login");
            }

            if (!user.EmailConfirmed)
            {
                return View("EmailConfirm");
            }

            return View(accountLoginViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Logoff()
        {
             await _signInManager.SignOutAsync();

            await HttpContext.SignOutAsync(
   CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordViewModel.Email);
            if(user is null)
            {
                ModelState.AddModelError("", "Usuário não encontrado");
            }

            var token  = await _userManager.GeneratePasswordResetTokenAsync(user);

            var linkCallBak =
      Url.Action("PassowordReset", "Account", new { userId = user.Id, token = token, newPassoword = forgotPasswordViewModel.Password }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Reset Password", linkCallBak);

            return View("EmailConfirm");
        }

        [AllowAnonymous]
        [HttpGet]
        public async  Task<IActionResult> PassowordReset(string userId, string token, string newPassoword)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.ResetPasswordAsync(user, token, newPassoword);
            
            if(result.Succeeded)
            {
                return RedirectToAction("Login", "Account", new AccountLoginViewModel
                {
                    Email = user.Email,
                    Password = newPassoword
                });
            }

            return RedirectToAction("Index", "Home");

        }
    }
}
