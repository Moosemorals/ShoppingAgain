using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Events
{
    public class State
    {
        [Key]
        public Guid EventSource { get; set; }
        [Required]
        public long Version { get; set; }
    }
}
