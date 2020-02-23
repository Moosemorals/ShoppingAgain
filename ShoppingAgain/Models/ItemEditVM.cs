using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.Models
{
    public class ItemEditVM
    {
        public ItemEditVM()
        {
            FormId = Guid.NewGuid();
        }

        public ShoppingList Parent { get; set; }
        [HiddenInput]
        public Guid ItemID { get; set; }

        [Required]
        public string Name { get; set; }


        public Guid FormId { get; }
        public string Route { get; set; }
    }
}
