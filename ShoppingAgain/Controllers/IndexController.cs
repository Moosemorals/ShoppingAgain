using Microsoft.AspNetCore.Mvc;

namespace ShoppingAgain.Controllers
{

    public class IndexController : Controller
    {

        [HttpGet("/", Name = "TopIndex")]
        public IActionResult Index()
        {
            return View();
        }
    }
}