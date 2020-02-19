using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
