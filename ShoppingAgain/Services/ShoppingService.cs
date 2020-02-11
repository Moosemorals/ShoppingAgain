using Microsoft.EntityFrameworkCore;
using ShoppingAgain.Contexts;
using ShoppingAgain.Events;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Services
{
    public class ShoppingService
    {
        private readonly ShoppingContext _db;
        private readonly EventService _events;

        public ShoppingService()
        {
            _db = new ShoppingContext();
            _events = new EventService();
        }

        public IEnumerable<ShoppingList> GetAll()
        {
            return _db.ShoppingLists
                .Include(x => x.Items)
                .Where(x => true);
        }

        public ShoppingList Get(Guid id)
        {
            return _db.ShoppingLists
                .Include(x => x.Items)
                .FirstOrDefault(x => x.ID == id);
        }

        public ShoppingList Get(string name)
        {
            return _db.ShoppingLists
                .Include(x => x.Items)
                .FirstOrDefault(x => x.Name == name);
        }
        public bool ExistsByName(string name)
        {
            return _db.ShoppingLists.Any(l => l.Name == name);
        }

        public ShoppingList CreateList(ShoppingList list)
        {
            _events.ListCreated(list.ID, list.Name);
            _db.ShoppingLists.Add(list);
            _db.SaveChanges();

            return list;
        }

        public void UpdateList(ShoppingList list)
        {
            _events.ListNameChanged(list.ID, list.Name);
            _db.ShoppingLists.Update(list);
            _db.SaveChanges();
        }

        internal void DeleteList(ShoppingList list)
        {
            _db.ShoppingLists.Remove(list);
            _db.SaveChanges();
            _events.ListDeleted(list.ID);
        }

        public void CreateItem(ShoppingList list, string itemName)
        {
            Item i = new Item
            {
                Name = itemName,
                State = ItemState.Wanted,
            };
            list.Items.Add(i);
            _db.SaveChanges();
            _events.ItemCreated(list.ID, i.ID, itemName);
        }

        public void ChangeItemName(ShoppingList list, Item item, string itemName)
        {
            item.Name = itemName;
            _db.SaveChanges();
            _events.ItemNameChanged(list.ID, item.ID, itemName);
        }

        public void ChangeItemState(ShoppingList list, Item item, ItemState state)
        {
            item.State = state;
            _db.SaveChanges();
            _events.ItemStateChanged(list.ID, item.ID, state);
        }

        public void DeleteItem(ShoppingList list, Item item)
        {
            list.Items.Remove(item);
            _db.SaveChanges();
            _events.ItemDeleted(list.ID, item.ID);
        }
    }
}
