using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordLoginInformation
    {
        public string[] email { get; set; }
        public string[] password { get; set; }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public DiscordLoginInformation()
        {
            email = new string[1];
            password = new string[1];
        }
    }
}
