using System;
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
    [Route("l/{listId:Guid}/items"), Authorize(Roles = "User")]
    public class ItemController : ShoppingBaseController
    {
        public ItemController(ShoppingService shoppingService) : base(shoppingService)
        {
        }

        [HttpPost("new", Name = Names.ItemCreate), ValidateAntiForgeryToken]
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
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
            }

            Item i = lists.CreateItem(list, fromUser.Name);

            Message("The item {0} has been added to {1}", i.Name, list.Name);
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
        }
        [HttpGet("{itemId:Guid}/delete", Name = Names.ItemDelete)]
        public ActionResult Delete(Guid listId, Guid itemId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find list requested");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            Item item = list.Items.FirstOrDefault(i => i.ID == itemId);
            if (item == null)
            {
                Message("Can't find item requested");
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
            }
 
            return View(new ItemEditVM { 
                Parent = list,
                ItemID = item.ID,
                Name = item.Name,
                Route = Names.ItemDelete,
                ButtonText = "Delete Item",
            });
        }


        [HttpPost("{itemId:Guid}/delete"), ValidateAntiForgeryToken]
        public ActionResult Delete(Guid listId, Guid itemId, ItemEditVM vm)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find list requested");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            Item item = list.Items.FirstOrDefault(i => i.ID == itemId);
            if (item == null)
            {
                Message("Can't find item requested");
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
            }
            string itemName = item.Name;
            lists.RemoveItem(list, item);

            Message("Deleted {0} from {1}", itemName, list.Name);
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
        }

        [HttpPost("{itemId:Guid}/state", Name = Names.ItemNextState), ValidateAntiForgeryToken]
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
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
        }

        [HttpPost("{itemId:Guid}/state/{state}", Name = "ItemChangeState"), ValidateAntiForgeryToken]
        public ActionResult ChangeState(Guid listId, Guid itemId, ItemState newState = ItemState.Unknown)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find the list you asked for");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            Item item = lists.GetItem(list, itemId);
            if (item == null)
            {
                Message("Can't find the item you asked for");
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
            }

            if (newState == ItemState.Unknown)
            {
                Message("Could not work out what state you wanted {0} in", item.Name);
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
            }

            ItemState prev = item.State;
            lists.ChangeItemState(list, item, item.State.Next());

            Message("{0} state changed from {1} to {2}", item.Name, prev, item.State);
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
        }

        [HttpGet("{itemId:Guid}/edit", Name = Names.ItemChangeName)]
        public IActionResult EditItem(Guid listId, Guid itemId)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find the list you asked for");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            Item item = lists.GetItem(list, itemId);
            if (item == null)
            {
                Message("Can't find the item you asked for");
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
            }

            return View(new ItemEditVM
            {
                ItemID = item.ID,
                Parent = list,
                Name = item.Name,
                Route = Names.ItemChangeName,
                ButtonText = "Save Changes",

            });
        }

        [HttpPost("{itemId:Guid}/edit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(Guid listId, Guid itemId, ItemEditVM vm)
        {
            ShoppingList list = lists.Get(GetUser(), listId);
            if (list == null)
            {
                Message("Can't find the list you asked for");
                return RedirectToRoute(Names.ListIndex, Names.ListHash);
            }

            Item item = lists.GetItem(list, itemId);
            if (item == null)
            {
                Message("Can't find the item you asked for");
                return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
            }

            string oldName = item.Name;
            await lists.ChangeItemName(list, item, vm.Name);

            Message("Changed {0} to {1}", oldName, vm.Name);
            return RedirectToRoute(Names.ListDetails, new { listId = list.ID }, Names.ItemsHash);
        }
    }
}