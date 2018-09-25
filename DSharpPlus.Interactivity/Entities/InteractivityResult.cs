using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.Entities
{
    public class InteractivityResult
    {
        public InteractivityExtension Interactivity { get; internal set; }
        public DiscordClient Client => Interactivity.Client;

        internal InteractivityResult(InteractivityExtension interactivity)
        {
            this.Interactivity = interactivity;
        }
    }
}
