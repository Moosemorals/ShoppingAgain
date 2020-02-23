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
    public class UserController : Controller
    {
        private readonly ShoppingService _lists;

        public UserController(ShoppingService Lists)
        {
            _lists = Lists;
        }

        [HttpGet("User/Login", Name ="UserLogin")]
        public IActionResult Login(string ReturnUrl)
        {
            LoginVM login = new LoginVM
            {
                ReturnUrl = ReturnUrl,
            };
            return View(login);
        }

        [HttpPost("User/Login"), ValidateAntiForgeryToken]
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

            return RedirectToRoute("ListIndex");
        }

        [HttpPost("User/Logout", Name = "UserLogout"), Authorize(Roles = "User")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToRoute(StaticNames.TopIndex);
        }

        [HttpGet("User/Denied", Name = "UserDenied")]
        public IActionResult Denied()
        {
            return View();
        }

        private void Message(string format, params object[] args)
        {
            TempData.Add(StaticNames.Message, string.Format(format, args));
        }

    }
}