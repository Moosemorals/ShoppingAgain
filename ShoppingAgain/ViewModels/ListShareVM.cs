using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.ViewModels
{
    public class ListShareVM
    {
        public List<ListShareFriendVM> Friends { get; set; }
        public string ListName { get; set; }
    }

    public class ListShareFriendVM
    {
        [Required, HiddenInput]
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        [Required]
        public bool IsShared { get; set; }
    }
}
