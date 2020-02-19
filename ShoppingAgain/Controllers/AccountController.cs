using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ShoppingAgain.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("Account/Login")]
        public IActionResult Login(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost("Account/Login"),ValidateAntiForgeryToken]
        public async Task< IActionResult > Login(string name, string ReturnUrl)
        {
            ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(ClaimTypes.Name, name));
            identity.AddClaim(new Claim(ClaimTypes.Role, "visitor"));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (ReturnUrl != null)
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToRoute("ListIndex");
        }


        [HttpPost("Account/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToRoute("ListIndex");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}