using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Models
{
    public class Option
    {
        [Key, Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
