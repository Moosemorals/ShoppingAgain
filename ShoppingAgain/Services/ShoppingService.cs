using Microsoft.EntityFrameworkCore;
using ShoppingAgain.Contexts;
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

        public ShoppingService(ShoppingContext db)
        {
            _db = db;
        }

        public IEnumerable<ShoppingList> GetAll()
        {
            return _db.ShoppingLists
                .Include(x => x.Items)
                .Where(x => true);
        }

        public ShoppingList Get(long id)
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

        public ShoppingList Add(ShoppingList list)
        {
            _db.ShoppingLists.Add(list);
            _db.SaveChanges();

            return list;
        }

        public bool ExistsByName(string name)
        {
            return _db.ShoppingLists.Any(l => l.Name == name);
        }

        public void Update(ShoppingList list)
        {
            _db.ShoppingLists.Update(list);
            _db.SaveChanges();
        }

        internal void Delete(ShoppingList list)
        {
            _db.ShoppingLists.Remove(list);
            _db.SaveChanges();
        }
    }
}
