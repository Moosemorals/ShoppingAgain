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
            return _db.ShoppingLists.Where(x => true);
        }

        public ShoppingList Get(long id)
        {
            return _db.ShoppingLists.FirstOrDefault(x => x.ID == id);
        }

        public ShoppingList Get(string name)
        {
            return _db.ShoppingLists.FirstOrDefault(x => x.Name == name);
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
    }
}
