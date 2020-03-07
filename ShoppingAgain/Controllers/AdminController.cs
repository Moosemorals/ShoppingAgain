using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Models;
using ShoppingAgain.ViewModels;

namespace ShoppingAgain.Controllers
{
    [Route("a"), Authorize(Roles = Names.RoleAdmin)]
    public class AdminController : ShoppingBaseController
    {
        public AdminController(ShoppingService shoppingService) : base(shoppingService) { }

        [HttpGet("", Name = Names.AdminIndex)]
        public IActionResult Index()
        {
            return View(lists.GetUsers());
        }

        [HttpGet("AddUser", Name = Names.AdminAddUser)]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost("AddUser"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AddUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string password = await lists.AddUser(vm.Name);

            Message("New user {0} created with password {1}", vm.Name, password);
            return RedirectToRoute(Names.AdminIndex);
        }

        [HttpGet("DeleteUser/{userId:Guid}", Name = Names.AdminDelUser)]
        public IActionResult DeleteUser(Guid userId)
        {
            User target = lists.GetUser(userId);
            if (target == null)
            {
                Message("Can't find user requested");
                return RedirectToRoute(Names.AdminIndex);
            }

            User current = GetUser();
            if (current.ID == target.ID)
            {
                Message("Can't delete yourself");
                return RedirectToRoute(Names.AdminIndex);
            }

            return View(new DeleteUserVM { Name = target.Name });
        }

        [HttpPost("DeleteUser/{userId:Guid}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid userId, DeleteUserVM vm)
        {
            User user = lists.GetUser(userId);
            if (user == null)
            {
                Message("Can't find user requested");
                return RedirectToRoute(Names.AdminIndex);
            }

            await lists.RemoveUser(user);

            Message("Deleted user {0}", vm.Name);

            return RedirectToRoute(Names.AdminIndex);
        } 
    }
}