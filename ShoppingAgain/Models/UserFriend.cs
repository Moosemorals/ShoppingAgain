using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Models
{
    public class UserFriend
    {
        public Guid UserID { get; set; }
        public User User { get; set; }

        public Guid FriendID { get; set; }
        public User Friend { get; set; }
    }
}
