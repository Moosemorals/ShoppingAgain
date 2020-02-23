using Microsoft.EntityFrameworkCore;
using ShoppingAgain.Events;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingAgain.Database
{
    public class ShoppingService : IDisposable
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

        public User GetUser(string idString)
        {
            return GetUser(Guid.Parse(idString));
        }

        public User GetUser(Guid userId)
        {
            return _db.Users.Find(userId);
        }

        public IEnumerable<ShoppingList> Get(string userIdString)
        {
            User user = GetUser(userIdString);
            if (user == null)
            {
                // Probably shouldn't happen
                throw new ArgumentException("Can't find user with id " + userIdString);
            }
            return user.Lists.Select(ul => ul.List);
        }

        public ShoppingList Get(Guid listId)
        {
            return _db.ShoppingLists
                .Include(x => x.Items)
                .FirstOrDefault(x => x.ID == listId);
        }

        public bool Exists(Guid listId)
        {
            return _db.ShoppingLists.Any(l => l.ID == listId);
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
            _events.ItemStateChanged(list.ID, item.ID, oldState, newState);
        }

        internal Item GetItem(ShoppingList list, Guid itemId)
        {
            return list.Items.FirstOrDefault(i => i.ID == itemId);
        }

        public void RemoveItem(ShoppingList list, Item item)
        {
            list.Items.Remove(item);
            _db.SaveChanges();
            _events.ItemDeleted(list.ID, item.ID);
        }

        public User ValidateLogin(LoginVM login)
        {
            User u = _db.Users
                .Include("Password")
                .Include("UserRoles")
                .Include("UserRoles.Role")
                .FirstOrDefault(u => u.Name == login.Username);

            if (u == null)
            {
                _events.UserNotFound(login.Username);
                return null;
            }

            if (!u.Password.Validate(login.Password))
            {
                _events.LoginFailed(u);
                return null;
            }

            _events.LoginSuccesfull(u);
            return u;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
