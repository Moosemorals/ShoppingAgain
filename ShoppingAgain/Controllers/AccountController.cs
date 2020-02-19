using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Models;
using ShoppingAgain.Services;

namespace ShoppingAgain.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShoppingService _lists;

        public AccountController(ShoppingService Lists)
        {
            _lists = Lists;
        }

        [HttpGet("Account/Login", Name ="UserLogin")]
        public IActionResult Login(string ReturnUrl)
        {
            LoginVM login = new LoginVM
            {
                ReturnUrl = ReturnUrl,
            };
            return View(login);
        }

        [HttpPost("Account/Login"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM login)
        {
            User u = _lists.ValidateLogin(login);
            if (u == null)
            {
                Message("Login failed");
                login.Password = null;
                return View(login);
            }

            ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(ClaimTypes.Name, u.Name));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (login.ReturnUrl != null)
            {
                return Redirect(login.ReturnUrl);
            }

            return RedirectToRoute("ListIndex");
        }

        [HttpPost("Account/Logout", Name = "UserLogout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToRoute("ListIndex");
        }

        [HttpGet("Account/AccessDenied", Name = "UserDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private void Message(string format, params object[] args)
        {
            TempData.Add(StaticNames.Message, string.Format(format, args));
        }

    }
}