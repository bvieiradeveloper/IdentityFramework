using IdentityFramework.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace IdentityFramework.Controllers
{

    public class AccountController : BaseController
    {
        UserManager<UserBase> _userManager;
        IEmailSender _emailSender;
        public AccountController(UserManager<UserBase> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }
        public ActionResult Register()
        {
            return View();
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

            if (userExists) return BadRequest(new { error = "Usuário já existe" });

            var result =  await _userManager.CreateAsync(user, accountRegister.Password);

            if(result.Succeeded)
            {
                
                var token = _userManager.GenerateEmailConfirmationTokenAsync(user);

                var linkCallBak =
                    Url.Action("RegisterConfirmation", "Account", new { userId = user.Id, token = token.Result }, Request.Scheme);

                  await _emailSender.SendEmailAsync(user.Email, "Email de Confirmação", linkCallBak);

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
            if(user is null)
            {
                return View("ErrorEmailConfirm");
            }
           
            var result = await _userManager.ConfirmEmailAsync(user, token);
            
            if(result.Succeeded)
            {
                //Retornar para a tela de login;
            }

            return RedirectToAction("Index","Home");
        }
    }
}
