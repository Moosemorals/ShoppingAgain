using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.ViewModels
{
    public class ListEditVM
    {
        [Required]
        public string Name { get; set; }
    }
}
