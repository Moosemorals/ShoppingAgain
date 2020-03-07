using Microsoft.EntityFrameworkCore;
using ShoppingAgain.Classes;
using ShoppingAgain.Events;
using ShoppingAgain.Models;
using ShoppingAgain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public IEnumerable<User> GetUsers()
        {
            return _db.Users;
        }

        public void RemoveEventListener(EventHandler<ShoppingEvent> handler)
        {
            _events.ShoppingEvents -= handler;
        }

        public User GetUserByName(string name)
        {
            return _db.Users.FirstOrDefault(u => u.Name == name);
        }

        public User GetUser(string idString)
        {
            return GetUser(Guid.Parse(idString));
        }

        public User GetUser(Guid userId)
        {
            return _db.Users
                .Include("CurrentList")
                .Include("CurrentList.Items")
                .Include("Lists")
                .Include("Lists.List")
                .Include("Lists.List.Items")
                .Include("Roles")
                .Include("Roles.Role")
                .Include("Password")
                .FirstOrDefault(u => u.ID == userId);
        }

        public async Task<string> AddUser(string name)
        {
            string pwd = await Password.Generate(3);
            Password password = Password.Generate(pwd);
            User user = new User
            {
                Name = name,
                Password = password,
            };
            password.User = user;
            _db.Users.Add(user);
            _db.Passwords.Add(password);
            _db.UserRoles.Add(new UserRole { User = user, Role = _db.Roles.FirstOrDefault(r => r.Name == Names.RoleUser) });
            await _db.SaveChangesAsync(); 
            return pwd;
        }

        public ShoppingList Get(User user, Guid listId)
        {
            return _db.ShoppingLists
                .Include("Items")
                .Include("Users")
                .FirstOrDefault(x => x.ID == listId && x.Users.Any(ul => ul.UserId == user.ID));
        }

        internal async Task RemoveUser(User user)
        {
         //   _db.UserFriends.RemoveRange(_db.UserFriends.Where(uf => uf.UserId == user.ID || uf.FriendId == user.ID));  
            _db.UserLists.RemoveRange(_db.UserLists.Where(ul => ul.UserId == user.ID));
            _db.UserRoles.RemoveRange(_db.UserRoles.Where(ur => ur.UserId == user.ID));
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        internal async Task ChangePassword(User u, string pwd)
        {
            _db.Passwords.Remove(u.Password);
            Password newPassword = Password.Generate(pwd);
            u.Password = newPassword;
            newPassword.UserID = u.ID;
            _db.Passwords.Add(newPassword);
            await _db.SaveChangesAsync();
        }

        public ShoppingList CreateList(User user, string name)
        {
            ShoppingList list = new ShoppingList
            {
                Name = name,
                Users = new List<UserList>(),
            };
            _db.ShoppingLists.Add(list);
            UserList ul = new UserList { ListId = list.ID, UserId = user.ID };
            _db.UserLists.Add(ul);
            user.CurrentList = list;
            _db.Users.Update(user);
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

        public void DeleteList(ShoppingList list)
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
                .Include("Roles")
                .Include("Roles.Role")
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
