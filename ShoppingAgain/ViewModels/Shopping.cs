using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.ViewModels
{
    public class Shopping
    {
        public Shopping()
        {
            Lists = new List<ShoppingList>();
        }

        public IEnumerable<ShoppingList> Lists;

        private Guid _currentId;

        public ShoppingList Current {
            get {
                if (_currentId == null)
                {
                    return null;
                }
                return Lists.FirstOrDefault(l => l.ID == _currentId);
            }
            set {
                _currentId = value.ID;
            }
        }
    }
}
