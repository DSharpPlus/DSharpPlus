using DSharpPlus.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public interface IWebRequest
    {
        DiscordClient Discord { get; set; }

        string URL { get; set; }
        HttpRequestMethod Method { get; set; }
        IDictionary<string, string> Headers { get; set; }
    }
}
