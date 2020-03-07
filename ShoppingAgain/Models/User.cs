using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.Models
{
    public class User
    {

        [Key]
        public Guid ID { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual Password Password { get; set; }
        [Required]
        public Guid PasswordID { get; set; }

        public virtual ShoppingList CurrentList { get; set; }
        public Guid? CurrentListID { get; set; }
        
        public virtual ICollection<User> Friends { get; set; }
        public virtual ICollection<UserRole> Roles { get; set; }
        public virtual ICollection<UserList> Lists { get; set; }
    }
}
