using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.ViewModels
{
    public class ItemEditVM
    {
        public ShoppingList Parent { get; set; }
        [HiddenInput]
        public Guid ItemID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Route { get; set; }

        public string ButtonText { get; set; }
    }
}
