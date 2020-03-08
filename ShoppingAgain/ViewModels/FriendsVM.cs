using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.ViewModels
{
    public class FriendsVM
    {
        [Required]
        public List<FriendVM> Friends { get; set; }
    }

    public class FriendVM
    {
        [Required, HiddenInput]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsFriend { get; set; }
    }
}
