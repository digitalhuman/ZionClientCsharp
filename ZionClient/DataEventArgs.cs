using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZionClient
{
    class DataEventArgs : EventArgs
    {
        public string Data { get; set; }

        public DataEventArgs(string data)
        {
            Data = data;
        }
    }
}
