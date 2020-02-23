using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.Models
{
    public class ShoppingList
    {
        public ShoppingList() => Items = new List<Item>();

        [Key]
        public Guid ID { get; set; }

        [Required, Display(Name = "List Name")]
        public string Name { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        public virtual ICollection<UserList> Users { get; set; }

        public int Count {  get { return Items.Count; } }

    }
}
