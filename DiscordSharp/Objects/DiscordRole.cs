using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Objects
{
    public class DiscordRole
    {
        public DiscordSharp.Color color { get; set; }
        //public Color ccolor { get; set; }
        public bool hoist { get; set; }
        public string name { get; set; }
        public DiscordPermission permissions { get; set; }
        public bool managed { get; set; }
        public int position { get; set; }
        public string id { get; set; }

        public DiscordRole Copy()
        {
            return new DiscordRole
            {
                color = this.color,
                hoist = this.hoist,
                name = this.name,
                permissions = this.permissions,
                managed = this.managed,
                position = this.position,
                id = this.id
            };
        }

        internal DiscordRole() { }
    }
}
