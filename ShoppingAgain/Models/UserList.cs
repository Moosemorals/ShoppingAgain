using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Models
{
    public class UserList
    {
        public Guid UserId { get; set; }
        public User User { get; set; } 
        public Guid ListId { get; set; } 
        public ShoppingList List { get; set; }
    }
}
