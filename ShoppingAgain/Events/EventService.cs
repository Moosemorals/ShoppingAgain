using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingAgain.Events
{
    public class EventService
    {
        private readonly EventContext _db;
        public event EventHandler<ShoppingEvent> ShoppingEvents;

        public EventService()
        {
            _db = new EventContext();
        }

        public void ListCreated(Guid listId, string Name)
        {
            SendEvent(listId, new ListCreatedEvent(listId, Name));
        }

        public void ListNameChanged(Guid ListId, string oldName, string newName)
        {
            SendEvent(ListId, new ListRenamedEvent(ListId, oldName, newName));
        }

        public void ListDeleted(Guid listId)
        {
            SendEvent(listId, new ListDeletedEvent(listId));
        }

        public void ItemCreated(Guid listId, Guid itemId, string Name)
        {
            SendEvent(listId, new ItemCreatedEvent(listId, itemId, Name));
        }

        public void ItemNameChanged(Guid listId, Guid itemId, string oldName, string newName)
        {
            SendEvent(listId, new ItemRenamedEvent(listId, itemId, oldName, newName));
        }
        public void ItemStateChanged(Guid listId, Guid itemId, ItemState oldState, ItemState newState)
        {
            SendEvent(listId, new ItemStateChangedEvent(listId, itemId, oldState, newState));
        }
        public void ItemDeleted(Guid listId, Guid itemId)
        {
            SendEvent(listId, new ItemDeletedEvent(listId, itemId));
        }
        public void UserNotFound(string username)
        {
            SendEvent(Guid.Empty, new UserNotFoundEvent(username));
        }

        public void LoginFailed(User user)
        {
            SendEvent(user.ID, new UserLoginFailedEvent(user.ID));
        }

        public void LoginSuccesfull(User user)
        {
            SendEvent(user.ID, new UserLoginSucceededEvent(user.ID));
        }

        public void UserCreated(User user)
        {
            SendEvent(user.ID, new UserCreatedEvent(user.ID, user.Name));
        }

        private void SendEvent(Guid EventSource, ShoppingEvent e)
        {
            LogEvent(EventSource, JsonConvert.SerializeObject(e));
            ShoppingEvents?.Invoke(this, e);
        }

        private State GetState(Guid EventSource)
        {
            return _db.CurrentState.FirstOrDefault(s => s.EventSource == EventSource);
        }

        internal void Replay(Guid ListId)
        {
            IEnumerable<DBEvent> events = _db.EventLog
                .Where(d => d.EventSource == ListId)
                .OrderBy(d => d.Version);

            foreach (DBEvent e in events)
            {
                ShoppingListEvent se = JsonConvert.DeserializeObject<ShoppingListEvent>(e.Payload);
                SendEvent(ListId, se);
            }
        }

        private void LogEvent(Guid id, string payload)
        {
            State s = GetState(id);
            if (s == null)
            {
                s = new State
                {
                    EventSource = id,
                    Version = 0,
                };
                _db.CurrentState.Add(s);
            }
            else
            {
                s.Version += 1;
                _db.CurrentState.Update(s);
            }

            DBEvent e = new DBEvent
            {
                EventSource = id,
                Version = s.Version,
                Payload = payload,
                When = DateTimeOffset.UtcNow,
            };

            _db.EventLog.Add(e);
            _db.SaveChanges();
        }

    }

    public abstract class ShoppingEvent
    {
        [JsonConverter(typeof(StringEnumConverter)), JsonProperty("eventType")]
        public ShoppingEventType Type { get; }

        public ShoppingEvent(ShoppingEventType type)
        {
            Type = type;
        }
    }

    public abstract class ShoppingUserEvent : ShoppingEvent
    {
        [JsonProperty("userId")]
        public Guid UserId { get; }
        public ShoppingUserEvent(ShoppingEventType type, Guid userId) : base(type) {
            UserId = userId;
        }
    }

    public class UserCreatedEvent : ShoppingUserEvent
    {
        [JsonProperty("username")]
        public string Username { get; }

        public UserCreatedEvent(Guid userId, string username) : base(ShoppingEventType.UserCreated, userId)
        {
            Username = username;
        }
    }

    public class UserNotFoundEvent : ShoppingUserEvent
    {
        public UserNotFoundEvent(string username) : base(ShoppingEventType.UserNotFound, Guid.Empty)
        {
            Username = username;
        }

        [JsonProperty("username")]
        public string Username { get; }
    }

    public class UserLoginFailedEvent : ShoppingUserEvent
    {
        public UserLoginFailedEvent(Guid UserId) : base(ShoppingEventType.UserLoginFailed, UserId) { }
    }

    public class UserLoginSucceededEvent : ShoppingUserEvent
    {
        public UserLoginSucceededEvent(Guid UserId) : base(ShoppingEventType.UserLogin, UserId) { }
    }

    public class UserLogoutEvent : ShoppingUserEvent
    { 
        public UserLogoutEvent(Guid UserId) : base(ShoppingEventType.UserLogout, UserId) { }
    }

    public abstract class ShoppingListEvent : ShoppingEvent
    {
        [JsonProperty("listId")]
        public Guid ListId { get; }
        public ShoppingListEvent(ShoppingEventType type, Guid listId) : base(type)
        {
            ListId = listId;
        }
    }

    public class ListCreatedEvent : ShoppingListEvent
    {
        [JsonProperty("listName")]
        public string Name { get; }
        public ListCreatedEvent(Guid listId, string name) : base(ShoppingEventType.ListCreated, listId)
        {
            Name = name;
        }
    }

    public class ListRenamedEvent : ShoppingListEvent
    {
        [JsonProperty("oldListName")]
        public string OldName { get; }
        [JsonProperty("newListName")]
        public string NewName { get; }
        public ListRenamedEvent(Guid listId, string oldName, string newName) : base(ShoppingEventType.ListRenamed, listId)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    public class ListDeletedEvent : ShoppingListEvent
    {
        public ListDeletedEvent(Guid listId) : base(ShoppingEventType.ListDeleted, listId)
        {
        }
    }

    public abstract class ItemEvent : ShoppingListEvent
    {
        [JsonProperty("itemId")]
        public Guid ItemId { get; }

        public ItemEvent(Guid listId, Guid itemId, ShoppingEventType type) : base(type, listId)
        {
            ItemId = itemId;
        }
    }

    public class ItemCreatedEvent : ItemEvent
    {
        [JsonProperty("itemName")]
        public string Name { get; }
        public ItemCreatedEvent(Guid listId, Guid itemId, string name) : base(listId, itemId, ShoppingEventType.ItemCreated)
        {
            Name = name;
        }
    }

    public class ItemRenamedEvent : ItemEvent
    {
        [JsonProperty("oldItemName")]
        public string OldName { get; }

        [JsonProperty("newItemName")]
        public string NewName { get; }

        public ItemRenamedEvent(Guid listId, Guid itemId, string oldName, string newName) : base(listId, itemId, ShoppingEventType.ItemRenamed)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    public class ItemStateChangedEvent : ItemEvent
    {
        [JsonProperty("oldItemState")]
        public ItemState OldState { get; }

        [JsonProperty("newItemState")]
        public ItemState NewState { get; }

        public ItemStateChangedEvent(Guid listId, Guid itemId, ItemState oldState, ItemState newState) : base(listId, itemId, ShoppingEventType.ItemStateChanged)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    public class ItemDeletedEvent : ItemEvent
    {
        public ItemDeletedEvent(Guid listId, Guid itemId) : base(listId, itemId, ShoppingEventType.ItemDeleted) { }
    }


    public enum ShoppingEventType
    {
        ItemCreated,
        ItemDeleted,
        ItemRenamed,
        ItemStateChanged,
        ListCreated,
        ListDeleted,
        ListRenamed,
        UserCreated,
        UserDeleted,
        UserDisabled,
        UserFriendRequestAccepted,
        UserFriendRequestRejected,
        UserFriendRequestSent,
        UserLogin,
        UserLoginFailed,
        UserLogout,
        UserNotFound,
        UserRoleAssigned,
        UserRoleRemoved,
    }
}
