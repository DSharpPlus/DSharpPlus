using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public interface IModule
    {
        DiscordClient Client { get; set; }

        void Setup(DiscordClient client);
    }
}
