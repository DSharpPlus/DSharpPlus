using System.Collections.Generic;

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
