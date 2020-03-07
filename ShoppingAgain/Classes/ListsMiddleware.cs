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
            if (context.User.Identity.IsAuthenticated)
            {
                context.Items.Add(Names.IsLoggedIn, true);
                string userIdString = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                User user = lists.GetUser(userIdString);

                context.Items.Add(Names.User, user);
                context.Items.Add(Names.CurrentList, user.CurrentList);
                context.Items.Add(Names.Lists, user.Lists.Select(ul => ul.List));
            }
            else
            { 
                context.Items.Add(Names.IsLoggedIn, false);
            }

            await _next(context);
        }
    }
}
