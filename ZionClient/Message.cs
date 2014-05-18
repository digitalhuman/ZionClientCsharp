using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZionClient
{
    class Message
    {
        public string command { get; set; }
        public string channel { get; set; }
        public string message { get; set; }
        public string message_type { get; set; }
    }
}
