using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Classes;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Controllers
{
    public abstract class ShoppingBaseController : Controller
    {
        public ShoppingBaseController() : base() { }

        protected User GetUser()
        {
            return HttpContext.Items[StaticNames.User] as User;
        }

        protected void Message(string format, params object[] args)
        {
            List<string> messages;
            if (TempData.ContainsKey(StaticNames.Message))
            {
                messages = new List<string>((string[])TempData[StaticNames.Message]);
                TempData.Remove(StaticNames.Message);
            }
            else
            {
                messages = new List<string>();
            }
            messages.Add(string.Format(format, args));
            TempData.Add(StaticNames.Message, messages);
        }

    }
}
