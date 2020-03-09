using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Models;

namespace ShoppingAgain.Controllers
{

    public class IndexController : ShoppingBaseController
    {

        public IndexController(ShoppingService shoppingService) : base(shoppingService) { }

        [HttpGet("/", Name = "TopIndex")]
        public IActionResult Index()
        {
            bool isLoggedIn = (bool)HttpContext.Items[Names.IsLoggedIn];

            if (isLoggedIn)
            {
                if (HttpContext.Items[Names.CurrentList] is ShoppingList current)
                {
                    return RedirectToRoute(Names.ListDetails, new { listId = current.ID }, Names.ItemsHash);
                } else
                {
                    return RedirectToRoute(Names.ListIndex, Names.ListHash);
                }
            }

            return View();
        }
    }
}