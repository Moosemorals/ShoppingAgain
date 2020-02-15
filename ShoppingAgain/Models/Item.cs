using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        Basket,
        Bought
    }

    public static class ItemStateMethods {
        public static ItemState Next(this ItemState s)
        {
            switch (s)
            {
                case ItemState.Basket:
                    return ItemState.Bought;
                case ItemState.Bought:
                    return ItemState.Wanted;
                case ItemState.Wanted:
                    return ItemState.Basket;
                default:
                    return ItemState.Unknown;
            }
        }
    }
}
