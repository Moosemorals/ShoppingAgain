using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Models;

namespace ShoppingAgain.Controllers
{
    [Route("u"), Authorize(Roles = Names.RoleUser)]
    public class UserController : ShoppingBaseController
    {

        private readonly ShoppingService lists;
        public UserController(ShoppingService Lists)
        {
            lists = Lists;
        }

        [HttpGet(Names.UserIndexPath, Name  = Names.UserIndex)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet(Names.UserChangePasswordPath, Name = Names.UserChangePassword)]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost(Names.UserChangePasswordPath), ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
        { 
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            User u = GetUser();
            if (!u.Password.Validate(vm.OldPassword))
            {
                ModelState.AddModelError(nameof(vm.OldPassword), "That wasn't your old password");
                return View(vm);
            }

            // At this point we know that the old password is right,
            // the new password is >= 12 chars, and the new and confirm
            // passwords match. So, change the password
            await lists.ChangePassword(u, vm.NewPassword);

            Message("Your password has been changed");
            return RedirectToRoute(Names.ListIndex);
        }
    }
}