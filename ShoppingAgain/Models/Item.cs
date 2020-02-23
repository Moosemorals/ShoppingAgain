using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.Models
{
    public class Item
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public ItemState State { get; set; }
    }

    public enum ItemState
    {
        Unknown,
        Wanted,
        [Display(Name = "In Basket")]
        Basket,
        Bought
    }

    public static class ItemStateMethods
    {
        public static ItemState Next(this ItemState s)
        {
            return s switch
            {
                ItemState.Basket => ItemState.Bought,
                ItemState.Bought => ItemState.Wanted,
                ItemState.Wanted => ItemState.Basket,
                _ => ItemState.Unknown,
            };
        } 
    }
}
