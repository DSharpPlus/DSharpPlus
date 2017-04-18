using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class WebSocketMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
