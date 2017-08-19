using System.Collections.Generic;

namespace DSharpPlus
{
    public class WebRequest : IWebRequest
    {
        public DiscordClient Discord { get; set; }

        public string URL { get; set; }
        public HttpRequestMethod Method { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        
        // Regular request
        public string Payload { get; private set; }

        private WebRequest() { }

        public static WebRequest CreateRequest(DiscordClient client, string url, HttpRequestMethod method = HttpRequestMethod.GET, IDictionary<string, string> headers = null, string payload = "")
        {
            return new WebRequest
            {
                Discord = client,
                URL = url,
                Method = method,
                Headers = headers,
                Payload = payload
            };
        }
    }
}
