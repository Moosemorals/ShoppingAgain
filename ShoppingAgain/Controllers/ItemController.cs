using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Models;
using ShoppingAgain.Services;

namespace ShoppingAgain.Controllers
{
    [Route("l/{listId:long:min(1)}/items")]
    public class ItemController : Controller
    {
        private readonly ShoppingService lists;

        public ItemController(ShoppingService shoppingService)
        {
            lists = shoppingService;
        }

        [HttpGet("", Name = "ItemIndex")]
        public IActionResult Index(long listId)
        {
            ShoppingList list = lists.Get(listId);

            if (list != null)
            {
                return View(list);
            }

            return NotFound();
        }

        [HttpGet("new", Name = "ItemCreate")]
        public IActionResult Create(long listId)
        {
            ShoppingList list = lists.Get(listId);
            if (list == null)
            {
                return NotFound();
            }
            return View();
        }

        [HttpPost("new")]
        public IActionResult Create(long listId, [Bind("Name")]Item fromUser)
        {
            ShoppingList list = lists.Get(listId);
            if (list == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(fromUser);
            }

            list.Items.Add( new Item
            {
                Name = fromUser.Name,
                State = ItemState.Wanted,
            });

            lists.Update(list);

            return CreatedAtRoute("ListDetails", new { id = list.ID }, list);
        }
    }
}