using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShoppingAgain.Models
{
    public class Item
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public ItemState State { get; set; }
    }

    public enum ItemState
    {
        Wanted,
        Basket,
        Bought
    }
}
