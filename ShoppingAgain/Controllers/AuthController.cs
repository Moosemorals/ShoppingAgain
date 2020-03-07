using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Models;

namespace ShoppingAgain.Controllers
{
    public class AuthController : ShoppingBaseController
    {
        private readonly ShoppingService _lists;

        public AuthController(ShoppingService Lists)
        {
            _lists = Lists;
        }

        [HttpGet(Names.UserLoginPath, Name = Names.UserLogin)]
        public IActionResult Login(string ReturnUrl)
        {
            LoginVM login = new LoginVM
            {
                ReturnUrl = ReturnUrl,
            };
            return View(login);
        }

        [HttpPost(Names.UserLoginPath), ValidateAntiForgeryToken]
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
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, u.ID.ToString()));

            foreach (UserRole ur in u.Roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, ur.Role.Name));
            }

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (login.ReturnUrl != null)
            {
                return Redirect(login.ReturnUrl);
            }

            return RedirectToRoute(Names.ListIndex);
        }

        [HttpPost(Names.UserLogoutPath, Name = Names.UserLogout), Authorize(Roles = Names.RoleUser)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToRoute(Names.TopIndex);
        }

        [HttpGet(Names.UserDeniedPath, Name = Names.UserDenied)]
        public IActionResult Denied()
        {
            return View();
        }
    }
}