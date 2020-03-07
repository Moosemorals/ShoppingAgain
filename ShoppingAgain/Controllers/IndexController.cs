using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Models;

namespace ShoppingAgain.Controllers
{

    public class IndexController : ShoppingBaseController
    {

        [HttpGet("/", Name = "TopIndex")]
        public IActionResult Index()
        {
            bool isLoggedIn = (bool)HttpContext.Items[Names.IsLoggedIn];

            if (isLoggedIn)
            {
                if (HttpContext.Items[Names.CurrentList] is ShoppingList current)
                {
                    return RedirectToRoute(Names.ListDetails, new { listId = current.ID });
                } else
                {
                    return RedirectToRoute(Names.ListIndex);
                }
            }

            return View();
        }
    }
}