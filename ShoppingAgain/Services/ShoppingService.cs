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

        public ShoppingService(EventService events)
        {
            _db = new ShoppingContext();
            _events = events;
        }

        public void AddEventListener(EventHandler<ShoppingEvent> handler)
        {
            _events.ShoppingEvents += handler;
        }

        public void RemoveEventListener(EventHandler<ShoppingEvent> handler)
        {
            _events.ShoppingEvents -= handler;
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

        public bool Exists(Guid id)
        {
            return _db.ShoppingLists.Any(l => l.ID == id);
        }

        public bool ExistsByName(string name)
        {
            return _db.ShoppingLists.Any(l => l.Name == name);
        }

        public ShoppingList CreateList(ShoppingList list)
        {
            _db.ShoppingLists.Add(list);
            _db.SaveChanges();
            _events.ListCreated(list.ID, list.Name);
            return list;
        }

        public ShoppingList ChangeName(ShoppingList list, string newName)
        {
            string oldName = list.Name;
            list.Name = newName;
            _db.ShoppingLists.Update(list);
            _db.SaveChanges();
            _events.ListNameChanged(list.ID, oldName, newName);

            return list;
        }

        internal void DeleteList(ShoppingList list)
        {
            _db.ShoppingLists.Remove(list);
            _db.SaveChanges();
            _events.ListDeleted(list.ID);
        }

        public Item CreateItem(ShoppingList list, string itemName)
        {
            Item i = new Item
            {
                Name = itemName,
                State = ItemState.Wanted,
            };
            list.Items.Add(i);
            _db.SaveChanges();
            _events.ItemCreated(list.ID, i.ID, itemName);

            return i;
        }

        public void ChangeItemName(ShoppingList list, Item item, string newName)
        {
            string oldName = item.Name;
            item.Name = newName;
            _db.SaveChanges();
            _events.ItemNameChanged(list.ID, item.ID, oldName, newName);
        }

        public void ChangeItemState(ShoppingList list, Item item, ItemState newState)
        {
            ItemState oldState = item.State;
            item.State = newState;
            _db.SaveChanges();
            _events.ItemStateChanged(list.ID, item.ID,oldState, newState);
        }

        public void RemoveItem(ShoppingList list, Item item)
        {
            list.Items.Remove(item);
            _db.SaveChanges();
            _events.ItemDeleted(list.ID, item.ID);
        }
    }
}
