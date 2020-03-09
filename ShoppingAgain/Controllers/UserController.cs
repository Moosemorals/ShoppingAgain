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
    [Route("u"), Authorize(Roles = Names.RoleUser)]
    public class UserController : ShoppingBaseController
    {

        public UserController(ShoppingService Lists) : base(Lists)
        {
        }

        [HttpGet("", Name = Names.UserIndex)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Password", Name = Names.UserChangePassword)]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost("Password"), ValidateAntiForgeryToken]
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
            return RedirectToRoute(Names.ListIndex, Names.ListHash);
        }

        [HttpGet("Friends", Name = Names.UserFriends)]
        public IActionResult Friends()
        {
            User current = GetUser();
            FriendsVM vm = new FriendsVM
            {
                Friends = new List<FriendVM>(),
            };

            foreach (User u in lists.GetUsers())
            {
                if (u.ID == current.ID)
                {
                    // Can't be friends with yourself
                    continue;
                }

                vm.Friends.Add(new FriendVM
                {
                    UserId = u.ID,
                    UserName = u.Name,
                    IsFriend = current.Friends.Any(uf => uf.FriendID == u.ID),
                });
            }

            return View(vm);
        }

        [HttpPost("Friends"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Friends(FriendsVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            foreach (FriendVM friendVM in vm.Friends)
            {
                User u = lists.GetUser(friendVM.UserId);
                if (u == null)
                {
                    ModelState.AddModelError(nameof(friendVM.UserId), "UserID isn't valid");
                    continue;
                }
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            User current = GetUser();
            foreach (FriendVM friendVM in vm.Friends)
            {
                User u = lists.GetUser(friendVM.UserId);
                await lists.ManageFriend(current, u, friendVM.IsFriend);
            }

            if (current.Friends.Count > 0)
            {
                Message("You are now friends with {0}", string.Join(", ", current.Friends.Select(uf => uf.Friend.Name)));
            }
            else
            {
                Message("You have no friends");
            }

            return RedirectToRoute(Names.UserIndex, "user-controls");
        }
    }
}