using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.Models
{
    public class Role
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual ICollection<UserRole> Users { get; set; }
    }
}
