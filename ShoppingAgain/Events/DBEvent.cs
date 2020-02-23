using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingAgain.Events
{
    internal class DBEvent
    {
        [Key] 
        public Guid ID { get; set; }
        public Guid EventSource { get; set; }
        [Required]
        public string Payload { get; set; }
        [Required]
        public long Version { get; set; }
        [Required]
        public DateTimeOffset When { get; set; }

    }
}
