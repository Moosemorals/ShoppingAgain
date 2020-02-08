using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Models;
using ShoppingAgain.Services;

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
            return View(lists.GetAll()); 
        }

        [HttpGet("new", Name = "ListCreate")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("new")]
        public IActionResult Create([Bind("Name")]ShoppingList list)
        {
            if (ModelState.IsValid)
            {
                list = lists.Add(list);
                Message("The list '{0}' has been created", list.Name);
                return RedirectToRoute("ListDetails", new { id = list.ID });
            }

            return View(list);
        }

        [HttpGet("{id:long:min(1)}/Edit", Name = "ListEdit")]
        public IActionResult Edit(long id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null) { 
                return View(list);
            }

            return NotFound(); 
        }

        [HttpPost("{id:long:min(1)}")]
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

            return View(fromUser);
        } 

        [HttpGet("{id:long:min(1)}", Name="ListDetails")]
        public IActionResult Details(long id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null) { 
                return View(list);
            }

            return NotFound();
        }

        [HttpGet("{id:long:min(1)}/Delete", Name = "ListDelete")]
        public IActionResult Delete(long id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null)
            {
                return View(list);
            }
            return NotFound();
        }

        [HttpPost("{id:long:min(1)}/Delete")]
        public IActionResult DeleteConfirmed(long id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null)
            {
                lists.Delete(list);
                return RedirectToRoute("ListIndex");
            }
            return NotFound();
        }

        private void Message(string format, params object[] args)
        {
            
            TempData.Add(StaticNames.Message, string.Format(format, args));
        }
    }
}
