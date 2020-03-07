using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Models;
using ShoppingAgain.ViewModels;

namespace ShoppingAgain.Controllers
{
    [Route("l/{listId:Guid}/items"), Authorize(Roles = "User")]
    public class ItemController : ShoppingBaseController
    {
        public ItemController(ShoppingService shoppingService) : base(shoppingService)
        {
        }

        [HttpPost("new", Name = "ItemCreate"), ValidateAntiForgeryToken]
        public IActionResult Create(Guid listId, [Bind("Name")]Item fromUser)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Message("There was a problem creating that item");
                RedirectToRoute(Names.ListDetails, new { listId = list.ID });
            }

            Item i = lists.CreateItem(list, fromUser.Name);

            Message("The item {0} has been added to {1}", i.Name, list.Name);
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
        }

        [HttpPost("{itemId:Guid}/delete", Name = "ItemDelete"), ValidateAntiForgeryToken]
        public ActionResult Delete(Guid listId, Guid itemId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                return NotFound();
            }

            Item item = list.Items.FirstOrDefault(i => i.ID == itemId);
            if (item == null)
            {
                return NotFound();
            }

            lists.RemoveItem(list, item);

            return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
        }

        [HttpPost("{itemId:Guid}/state", Name = "ItemNextState"), ValidateAntiForgeryToken]
        public ActionResult NextState(Guid listId, Guid itemId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                return NotFound();
            }

            Item item = lists.GetItem(list, itemId);
            if (item == null)
            {
                return NotFound();
            }

            ItemState prev = item.State;
            lists.ChangeItemState(list, item, item.State.Next());

            Message("{0} state changed from {1} to {2}", item.Name, prev, item.State);
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
        }

        [HttpPost("{itemId:Guid}/state/{state}", Name = "ItemChangeState"), ValidateAntiForgeryToken]
        public ActionResult ChangeState(Guid listId, Guid itemId, ItemState newState = ItemState.Unknown)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find the list you asked for");
                return RedirectToRoute(Names.ListIndex);
            }

            Item item = lists.GetItem(list, itemId);
            if (item == null)
            {
                Message("Can't find the item you asked for");
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
            }

            if (newState == ItemState.Unknown)
            {
                Message("Could not work out what state you wanted {0} in", item.Name);
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
            }

            ItemState prev = item.State;
            lists.ChangeItemState(list, item, item.State.Next());

            Message("{0} state changed from {1} to {2}", item.Name, prev, item.State);
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
        }

        [HttpGet("{itemId:Guid}/edit", Name = "ItemChangeName")]
        public IActionResult EditItem(Guid listId, Guid itemId)
        {
            ShoppingList list = lists.Get(GetUser(),listId);
            if (list == null)
            {
                Message("Can't find the list you asked for");
                return RedirectToRoute(Names.ListIndex);
            }

            Item item = lists.GetItem(list, itemId);
            if (item == null)
            {
                Message("Can't find the item you asked for");
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID });
            }

            return View(new ItemEditVM { ItemID = item.ID, Parent = list, Name = list.Name });
        }

    }
}