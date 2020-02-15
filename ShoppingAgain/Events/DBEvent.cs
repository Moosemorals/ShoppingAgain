using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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
