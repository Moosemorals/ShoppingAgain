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
    public class HomeController : Controller
    {

        private readonly ShoppingService lists;

        public HomeController(ShoppingService shoppingService)
        {
            lists = shoppingService;
        }

        public IActionResult Index()
        {
            return View(lists.GetAll()); 
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create([Bind("Name")]ShoppingList list)
        {
            if (ModelState.IsValid)
            {
                list = lists.Add(list);
                Message("The list '{0}' has been created", list.Name);
                return RedirectToAction("Details", new { id = list.ID });
            }

            return View(list);
        }

        public IActionResult Edit(long id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null) { 
                return View(list);
            }

            return NotFound(); 
        }

        [HttpPost]
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
                return RedirectToAction("Details", new { id = list.ID });
            }

            return View(fromUser);
        } 

        public IActionResult Details(long id)
        {
            ShoppingList list = lists.Get(id);
            if (list != null) { 
                return View(list);
            }

            return NotFound();
        }

        private void Message(string format, params object[] args)
        {
            TempData.Add(StaticNames.Message, string.Format(format, args));
        }
    }
}
