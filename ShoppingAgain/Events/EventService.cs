using Newtonsoft.Json;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public void ListCreated(Guid ListId, string Name)
        {
            SendEvent(ListId, new ShoppingEvent(EventType.ListCreated, ListId, Name, null, null));
        }

        public void ListNameChanged(Guid ListId, string Name)
        {
            SendEvent(ListId, new ShoppingEvent(EventType.ListNameChanged, ListId, Name, null, null));
        }

        public void ListDeleted(Guid ListId)
        {
            SendEvent(ListId, new ShoppingEvent(EventType.ListDeleted, ListId, null, null, null));
        } 

        public void ItemCreated(Guid ListId, Guid ItemId, string Name)
        {
            SendEvent(ListId, new ShoppingEvent(EventType.ItemCreated, ListId, Name, ItemId, null));
        } 

        public void ItemNameChanged(Guid ListId, Guid ItemId, string Name)
        {
            SendEvent(ListId, new ShoppingEvent(EventType.ItemNameChanged, ListId, Name, ItemId, null));
        }
        public void ItemStateChanged(Guid ListId, Guid ItemId, ItemState State)
        {
            SendEvent(ListId, new ShoppingEvent(EventType.ItemNameChanged, ListId, null, ItemId, State));
        }
        public void ItemDeleted(Guid ListId, Guid ItemId)
        {
            SendEvent(ListId, new ShoppingEvent(EventType.ItemDeleted, ListId, null, ItemId, null));
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
                ShoppingEvent se = JsonConvert.DeserializeObject<ShoppingEvent>(e.Payload);
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
            }
            else
            {
                s.Version += 1;
            }
            _db.EventLog.Add(new DBEvent
            {
                EventSource = id,
                Version = s.Version,
                Payload = payload,
                When = DateTimeOffset.UtcNow,
            }) ;
            _db.CurrentState.Update(s);
            _db.SaveChanges();
        }
    }


    public class ShoppingEvent
    {
        public EventType Type { get; }
        public Guid? ListId { get; }
        public Guid? ItemId { get; }
        public string Name { get; }
        public ItemState? ItemState { get; }

        public ShoppingEvent(EventType type, Guid listId, string name, Guid? itemId, ItemState? itemState)
        {
            Type = type;
            ListId = listId;
            ItemId = itemId;
            Name = name;
            ItemState = itemState;
        }
    }

    public enum EventType
    {
        ListCreated,            // ListId 
        ListNameChanged,        // ListId, Name
        ListDeleted,            // ListId
        ItemCreated,            // ListId, ItemId
        ItemNameChanged,        // ListId, ItemId, Name
        ItemStateChanged,       // ListId, ItemId, ItemState
        ItemDeleted             // ListId, ItemId
    }
}
