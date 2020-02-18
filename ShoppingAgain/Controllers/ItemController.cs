using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Models;
using ShoppingAgain.Services;

namespace ShoppingAgain.Controllers
{
    [Route("l/{listId:Guid}/items")]
    public class ItemController : Controller
    {
        private readonly ShoppingService lists;

        public ItemController(ShoppingService shoppingService)
        {
            lists = shoppingService;
        }

        [HttpPost("new", Name = "ItemCreate"), ValidateAntiForgeryToken]
        public IActionResult Create(Guid listId, [Bind("Name")]Item fromUser)
        {
            ShoppingList list = lists.Get(listId);
            if (list == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Message("There was a problem creating that item");
                RedirectToRoute(StaticNames.ListDetails, new { listId = list.ID });
            }

            Item i = lists.CreateItem(list, fromUser.Name);

            Message("The item {0} has been added to {1}", i.Name, list.Name);
            return RedirectToRoute(StaticNames.ListDetails, new { listId = list.ID });
        }

        [HttpPost("{itemId:Guid}/delete", Name = "ItemDelete"), ValidateAntiForgeryToken]
        public ActionResult Delete(Guid listId, Guid itemId)
        {
            ShoppingList list = lists.Get(listId);
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

            return RedirectToRoute(StaticNames.ListDetails, new { listId = list.ID });
        }

        [HttpPost("{itemId:Guid}/state", Name = "ItemNextState"), ValidateAntiForgeryToken]
        public ActionResult NextState(Guid listId, Guid itemId)
        {
            ShoppingList list = lists.Get(listId);
            if (list == null)
            {
                return NotFound();
            }

            Item item = list.Items.FirstOrDefault(i => i.ID == itemId);
            if (item == null)
            {
                return NotFound();
            }

            ItemState prev = item.State;
            lists.ChangeItemState(list, item, item.State.Next()); 

            Message("{0} state changed from {1} to {2}", item.Name, prev, item.State);
            return RedirectToRoute(StaticNames.ListDetails, new { listId = list.ID });
        }

        [HttpPost("{itemId:Guid}/state/{state}", Name = "ItemChangeState"), ValidateAntiForgeryToken]
        public ActionResult ChangeState(Guid listId, Guid itemId, ItemState newState = ItemState.Unknown)
        {
            ShoppingList list = lists.Get(listId);
            if (list == null)
            {
                return NotFound();
            }

            Item item = list.Items.FirstOrDefault(i => i.ID == itemId);
            if (item == null)
            {
                return NotFound();
            }

            if (newState == ItemState.Unknown)
            {
                Message("Could not work out what state you wanted {0} in", item.Name);
                return RedirectToRoute(StaticNames.ListDetails, new { listId = list.ID });
            }

            ItemState prev = item.State;
            lists.ChangeItemState(list, item, item.State.Next());

            Message("{0} state changed from {1} to {2}", item.Name, prev, item.State);
            return RedirectToRoute(StaticNames.ListDetails, new { listId = list.ID });
        } 

        private void Message(string format, params object[] args)
        {
            TempData.Add(StaticNames.Message, string.Format(format, args));
        }

    }
}