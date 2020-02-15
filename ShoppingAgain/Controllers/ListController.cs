using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Events;
using ShoppingAgain.Models;
using ShoppingAgain.Services;
using ShoppingAgain.ViewModels;

namespace ShoppingAgain
{

    [Route("l")]
    public class ListController : Controller
    {
        private readonly ShoppingService lists;

        public ListController(ShoppingService shoppingService)
        {
            lists = shoppingService;
        }

        [HttpGet("", Name = "ListIndex")]
        [Route("/")] // This is also our default route
        public IActionResult Index()
        { 
            ViewBag.Lists = lists.GetAll();
            return View();
        }

        [HttpGet("{listId}", Name = "Selected")]
        public IActionResult Selected(Guid listId)
        { 
            ShoppingList current = lists.Get(listId);
            if (current == null)
            {
                return NotFound();
            }

            ViewBag.Lists = lists.GetAll(); 
            return View(current);
        }

        [HttpGet("new", Name = "ListCreate")]
        public IActionResult Create()
        {
            ViewBag.Lists = lists.GetAll(); 
            return View();
        }

        [HttpPost("new")]
        public IActionResult Create([Bind("Name")]ShoppingList fromUser)
        {
            if (ModelState.IsValid)
            {
                ShoppingList created = lists.CreateList(fromUser);
                Message("The list '{0}' has been created", created.Name);
                return RedirectToRoute("Selected", new { listId = created.ID });
            }

            ViewBag.Lists = lists.GetAll(); 
            return View(fromUser);
        }

        [HttpGet("{listId:Guid}/Edit", Name = "ListEdit")]
        public IActionResult Edit(Guid listId)
        {
            ShoppingList current = lists.Get(listId); 
            if (current != null)
            {
                ViewBag.Lists = lists.GetAll(); 
                return View(current);
            }

            Message("Can't find the list to change it's name");
            return RedirectToRoute("ListIndex");
        }

        [HttpPost("{listId:Guid}/Edit")]
        public IActionResult Edit([Bind("ID, Name")]ShoppingList fromUser)
        {
            if (ModelState.IsValid)
            {
                ShoppingList list = lists.Get(fromUser.ID);
                if (list == null)
                {
                    Message("Couldn't find list to edit");
                    return RedirectToAction("Index");
                }

                list.Name = fromUser.Name;

                lists.Update(list);
                Message("The list '{0}' has been updated", list.Name);
                return RedirectToRoute("ListDetails", new { id = list.ID });
            }

            ViewBag.Lists = lists.GetAll(); 
            return View(fromUser);
        }

        [HttpGet("{id:Guid}/Delete", Name = "ListDelete")]
        public IActionResult Delete(Guid id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null)
            {
                ViewBag.Lists = lists.GetAll(); 
                return View(list);
            }
            return NotFound();
        }

        [HttpPost("{id:Guid}/Delete")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null)
            {
                lists.DeleteList(list);
                return RedirectToRoute("ListIndex");
            }
            ViewBag.Lists = lists.GetAll(); 
            return NotFound();
        }

        private void Message(string format, params object[] args)
        {
            TempData.Add(StaticNames.Message, string.Format(format, args));
        }
    }
}
