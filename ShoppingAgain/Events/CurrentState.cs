using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.Events
{
    public class State
    {
        [Key]
        public Guid ID { get; set; }
        public Guid EventSource { get; set; }
        [Required]
        public long Version { get; set; }
    }
}
