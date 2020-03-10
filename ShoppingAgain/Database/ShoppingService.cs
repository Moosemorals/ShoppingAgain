using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public ShoppingService(ShoppingContext db, EventService events)
        {
            _db = db;
            _events = events;
        }

        public async Task Seed()
        {
            if (!await IsOptionSet(Names.InitialSetupComplete))
            {
                IConfiguration conf = new ConfigurationBuilder()
                    .AddJsonFile("secrets.json", false, false)
                    .Build();

                Password p = Password.Generate(conf["DefaultPassword"]);
                User u = new User
                {
                    Name = conf["DefaultUser"],
                    Password = p,
                };

                _db.Passwords.Add(p);
                _db.Users.Add(u);
                _db.UserRoles.Add(new UserRole { User = u, Role = _db.Roles.FirstOrDefault(r => r.Name == Names.RoleUser) });
                _db.UserRoles.Add(new UserRole { User = u, Role = _db.Roles.FirstOrDefault(r => r.Name == Names.RoleAdmin) });
               // _db.Options.Add(new Option { Key = Names.InitialSetupComplete, Value = "true" });

                await _db.SaveChangesAsync();
            }
        }

        public async Task SetOption(string key, String value)
        {
            Option o = await _db.Options.FindAsync(key);
            if (o == null)
            {
                _db.Options.Add(new Option
                {
                    Key = key,
                    Value = value,
                });
            }
            else
            {
                o.Value = value;
            }
            await _db.SaveChangesAsync();
        }

        public async Task<string> GetOption(string key, string @default)
        {
            Option o = await _db.Options.FindAsync(key);
            if (o != null)
            {
                return o.Value;
            }
            return @default;
        }

        public Task<string> GetOption(string key)
        {
            return GetOption(key, null);
        }

        public async Task<bool> IsOptionSet(string key)
        {
            return await _db.Options.AnyAsync(o => o.Key == key);
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
                .Include("Friends")
                .Include("Friends.Friend")
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

        public IEnumerable<User> GetFriends(User user)
        {
            return _db.UserFriends
                .Where(uf => uf.UserID == user.ID || uf.FriendID == user.ID)
                .Select(uf => uf.UserID == user.ID ? uf.Friend : uf.User)
                .Distinct();
        }

        internal async Task ManageFriend(User user, User friend, bool isFriend)
        {
            bool test(UserFriend uf) => (uf.UserID == user.ID && uf.FriendID == friend.ID) || (uf.UserID == friend.ID && uf.FriendID == user.ID);

            if (isFriend && !_db.UserFriends.Any(test))
            {
                _db.UserFriends.Add(new UserFriend { UserID = user.ID, FriendID = friend.ID });
            }
            else if (!isFriend && _db.UserFriends.Any(test))
            {
                _db.UserFriends.RemoveRange(_db.UserFriends.Where(test));
            }
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

        public async Task ChangeItemName(ShoppingList list, Item item, string newName)
        {
            string oldName = item.Name;
            item.Name = newName;
            await _db.SaveChangesAsync();
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

        internal async Task ShareList(ShoppingList list, User u, bool isShared)
        {
            if (isShared && !_db.UserLists.Any(ul => ul.ListId == list.ID && ul.UserId == u.ID))
            {
                _db.UserLists.Add(new UserList { ListId = list.ID, UserId = u.ID });
            }
            else if (!isShared && _db.UserLists.Any(ul => ul.ListId == list.ID && ul.UserId == u.ID))
            {
                _db.UserLists.Remove(_db.UserLists.First(ul => ul.ListId == list.ID && ul.UserId == u.ID));
            }
            await _db.SaveChangesAsync();
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
