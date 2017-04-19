using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext
{
    public class Command
    {
        public static string Name { get; set; }
        public static IReadOnlyCollection<string> Aliases { get; set; }

        public virtual async Task Execute()
        {

        }
    }
}
