using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Models
{
    public class UserFriend
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid FriendId { get; set; }
        public User Friend { get; set; }
    }
}
