using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Events;
using ShoppingAgain.Models;
using ShoppingAgain.ViewModels;

namespace ShoppingAgain.Controllers
{

    [Route("l"), Authorize(Roles = Names.RoleUser)]
    public class ListController : ShoppingBaseController
    {

        public ListController(ShoppingService shoppingService) : base(shoppingService)
        {
        }

        [HttpGet("", Name = Names.ListIndex)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{listId}", Name = Names.ListDetails)]
        public IActionResult Details(Guid listId)
        {
            ShoppingList current = lists.Get(GetUser(), listId);
            if (current == null)
            {
                Message("Can't find requested list");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            return View(current);
        }

        [HttpGet("new", Name = Names.ListCreate)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("new"), ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Name")]ListEditVM fromUser)
        {
            if (ModelState.IsValid)
            {
                ShoppingList created = lists.CreateList(GetUser(), fromUser.Name);
                Message("The list '{0}' has been created", created.Name);
                return RedirectToRoute(Names.ListDetails, new { listId = created.ID }, Names.ItemsHash);
            }

            return View(fromUser);
        }

        [HttpGet("{listId:Guid}/Edit", Name = Names.ListEdit)]
        public IActionResult Edit(Guid listId)
        {
            ShoppingList current = lists.Get(GetUser(), listId);
            if (current != null)
            {
                return View(current);
            }

            Message("Can't find the list to change it's name");
            return RedirectToRoute(Names.ListIndex, Names.ListHash);
        }

        [HttpPost("{listId:Guid}/Edit"), ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("ID, Name")]ShoppingList fromUser)
        {
            if (ModelState.IsValid)
            {
                ShoppingList list = lists.Get(GetUser(), fromUser.ID);
                if (list == null)
                {
                    Message("Couldn't find list to edit");
                    return RedirectToAction("Index");
                }

                string oldName = list.Name;
                lists.ChangeName(list, fromUser.Name);

                Message("The list '{0}' has been renamed to {1}", oldName, list.Name);
                return RedirectToRoute(Names.ListDetails, new { id = list.ID }, Names.ItemsHash);
            }

            return View(fromUser);
        }

        [HttpGet("{listId:Guid}/Share", Name = Names.ListShare)]
        public IActionResult Share(Guid listId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find list to share");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            ListShareVM vm = new ListShareVM
            {
                ListName = list.Name,
                ListID = list.ID,
                Friends = new List<ListShareFriendVM>(),
            };

            foreach (User friend in lists.GetFriends(GetUser()))
            {
                vm.Friends.Add(new ListShareFriendVM
                {
                    IsShared = list.Users.Any(u => u.UserId == friend.ID),
                    UserID = friend.ID,
                    UserName = friend.Name,
                });
            }

            return View(vm);
        }

        [HttpPost("{listId:Guid}/Share"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(Guid listId, ListShareVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find list to share");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            User current = GetUser();
            IEnumerable<User> friends = lists.GetFriends(current);

            foreach (ListShareFriendVM userVM in vm.Friends)
            {
                User u = lists.GetUser(userVM.UserID);
                if (u == null)
                {
                    ModelState.AddModelError(nameof(userVM.UserID), "Unknown user ID");
                    continue;
                }

                if (!friends.Contains(u))
                {
                    ModelState.AddModelError(nameof(userVM.UserName), "Can only share lists with friends");
                    continue;
                }
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            foreach (ListShareFriendVM userVM in vm.Friends)
            {
                User u = lists.GetUser(userVM.UserID);
                await lists.ShareList(list, u, userVM.IsShared);
            }

            if (list.Users.Count > 1)
            {
                Message("List {0} shared with {1}", list.Name, string.Join(", ", list.Users.Where(ul => ul.User != current).Select(ul => ul.User.Name)));
            }
            else
            {
                Message("List {0} not shared with anyone", list.Name);
            }

            return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
        }


        [HttpGet("{listId:Guid}/Delete", Name = Names.ListDelete)]
        public IActionResult Delete(Guid listId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list != null)
            {
                return View(list);
            }
            Message("Can't find list to delete");
            return RedirectToRoute(Names.ListIndex, Names.ListHash);
        }

        [HttpPost("{listId:Guid}/Delete"), ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid listId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list != null)
            {
                lists.DeleteList(list);
                Message("List {0} has been deleted", list.Name);
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }
            Message("Couldn't find that list to delete");
            return RedirectToRoute(Names.ListIndex, Names.ListHash);
        }

        [HttpGet("/events")]
        public async Task EventStream()
        {
            CancellationToken token = Request.HttpContext.RequestAborted;
            async void handler(object source, ShoppingEvent e)
            {
                if (token.IsCancellationRequested)
                {
                    lists.RemoveEventListener(handler);
                    return;
                }
                SSEvent SSE = new SSEvent(JsonConvert.SerializeObject(e));

                await Response.WriteAsync(SSE.ToString());
                await Response.Body.FlushAsync(token);
            }

            lists.AddEventListener(handler);
            Response.StatusCode = StatusCodes.Status200OK;
            Response.Headers.Add("Content-Type", "text/event-stream");
            await Response.StartAsync(token);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Response.WriteAsync(SSEvent.Heartbeat.ToString(), token);
                    await Response.Body.FlushAsync(token);
                    await Task.Delay(25 * 1000, token);
                }
            }
            catch (TaskCanceledException)
            {
                // This isn't unexpected, and i don't want to bother the logs
                // so, ignoring the exception
            }
            finally
            {
                lists.RemoveEventListener(handler);
            }
        }
    }
}
