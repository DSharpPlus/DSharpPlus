using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordRole
    {
        public Color color { get; set; }
        //public Color ccolor { get; set; }
        public bool hoist { get; set; }
        public string name { get; set; }
        public DiscordPermission permissions { get; set; }
        public bool managed { get; set; }
        public int position { get; set; }
        public string id { get; set; }
    }
}
