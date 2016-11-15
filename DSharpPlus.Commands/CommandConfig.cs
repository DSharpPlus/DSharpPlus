using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Commands
{
    public class CommandConfig
    {
        public bool SelfBot { get; set; } = false;
        public char Prefix { get; set; } = '.';
    }
}
