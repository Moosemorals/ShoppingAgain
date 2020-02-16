using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.Events
{
    public class SSEvent
    {
        public static readonly SSEvent Heartbeat = new SSEvent();

        private SSEvent()
        {
            Field = null;
            Value = null;
        }

        public SSEvent(string value)
        {
            Field = "event";
            Value = value;
        }

        public SSEvent(string field, string value)
        {
            Field = field;
            Value = value;
        }

        private readonly string Field;
        private readonly string Value;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Field))
            {
                return ":\n";
            }
            else if (string.IsNullOrEmpty(Value))
            {
                return Field + ":\n";
            }
            else
            {
                return Field + ":" + Value + "\n";
            }
        }

    }
}
