using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Models;

namespace ShoppingAgain.Controllers
{

    public class IndexController : Controller
    {

        [HttpGet("/", Name = "TopIndex")]
        public IActionResult Index()
        {
            bool isLoggedIn = (bool)HttpContext.Items[StaticNames.IsLoggedIn];

            if (isLoggedIn)
            {
                if (HttpContext.Items[StaticNames.CurrentList] is ShoppingList current)
                {
                    return RedirectToRoute(StaticNames.ListDetails, new { listId = current.ID });
                } else
                {
                    return RedirectToRoute(StaticNames.ListIndex);
                }
            }

            return View();
        }
    }
}