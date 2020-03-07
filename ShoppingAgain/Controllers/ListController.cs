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

namespace ShoppingAgain.Controllers
{

    [Route("l"), Authorize(Roles = "User")]
    public class ListController : ShoppingBaseController
    {
        private readonly ShoppingService lists;

        public ListController(ShoppingService shoppingService)
        {
            lists = shoppingService;
        }

        [HttpGet("", Name = "ListIndex")]
        public IActionResult Index()
        {
            if (HttpContext.Items[Names.CurrentList] is ShoppingList current)
            {
                return RedirectToRoute(Names.ListDetails, new { listId = current.ID });
            }
            return View();
        }

        [HttpGet("{listId}", Name = "ListDetails")]
        public IActionResult Details(Guid listId)
        {
            ShoppingList current = lists.Get(GetUser(), listId);
            if (current == null)
            {
                Message("Can't find requested list");
                return RedirectToRoute(Names.ListIndex);
            }

            return View(current);
        }

        [HttpGet("new", Name = "ListCreate")]
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
                return RedirectToRoute(Names.ListDetails, new { listId = created.ID });
            }

            return View(fromUser);
        }

        [HttpGet("{listId:Guid}/Edit", Name = "ListEdit")]
        public IActionResult Edit(Guid listId)
        {
            ShoppingList current = lists.Get(GetUser(), listId);
            if (current != null)
            {
                return View(current);
            }

            Message("Can't find the list to change it's name");
            return RedirectToRoute("ListIndex");
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
                return RedirectToRoute("ListDetails", new { id = list.ID });
            }

            return View(fromUser);
        }

        [HttpGet("{listId:Guid}/Delete", Name = "ListDelete")]
        public IActionResult Delete(Guid listId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list != null)
            {
                return View(list);
            }
            Message("CAn't find list to deltete");
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
        }

        [HttpPost("{listId:Guid}/Delete"), ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid listId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list != null)
            {
                lists.DeleteList(list);
                Message("List {0} has been deleted", list.Name);
                return RedirectToRoute("ListIndex");
            }
            Message("Couldn't find that list to delete");
            return RedirectToRoute("ListIndex");
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
