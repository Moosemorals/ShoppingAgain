using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Models
{
    public class ShoppingList
    {
        public ShoppingList() => Items = new List<Item>();

        [Key]
        public Guid ID { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        [Display(Name = "Items")]
        public int Count {  get { return Items.Count; } }
    }
}
