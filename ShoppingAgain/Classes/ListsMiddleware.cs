using Microsoft.AspNetCore.Http;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Events;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShoppingAgain.Classes
{
    public class ListsMiddleware
    {
        private readonly RequestDelegate _next;

        public ListsMiddleware(RequestDelegate Next)
        {
            _next = Next;
        }

        public async Task Invoke(HttpContext context, ShoppingService lists)
        {

            // Set the default to not logged in 

            context.Items.Add(Names.IsLoggedIn, false);
            if (context.User.Identity.IsAuthenticated)
            {
                string userIdString = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                User user = lists.GetUser(userIdString); 
                if (user != null)
                {
                    context.Items[Names.IsLoggedIn] = true;
                    context.Items.Add(Names.User, user);
                    context.Items.Add(Names.CurrentList, user.CurrentList);
                    context.Items.Add(Names.Lists, user.Lists.Select(ul => ul.List));
                } else
                {
                    // TODO: Log an error
                }

            }
            await _next(context);
        }
    }
}
