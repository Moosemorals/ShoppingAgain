using Microsoft.AspNetCore.Http;
using ShoppingAgain.Classes;
using ShoppingAgain.Database;
using ShoppingAgain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShoppingAgain.Middleware
{
    public class ListsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ShoppingService _lists;

        public ListsMiddleware(RequestDelegate Next, ShoppingService Lists)
        {
            _next = Next;
            _lists = Lists;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User != null)
            {
                context.Items[StaticNames.Lists] = _lists.Get(context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

                if (context.Request.Query.ContainsKey(StaticNames.ListId))
                {
                    context.Items[StaticNames.CurrentList] = context.Request.Query[StaticNames.ListId];
                }
            }

            await _next(context);
        }
    }
}
