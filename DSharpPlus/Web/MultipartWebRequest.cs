using System.Collections.Generic;
using System.IO;
using DSharpPlus.Web;

namespace DSharpPlus
{
    public class MultipartWebRequest : IWebRequest
    {
        public DiscordClient Discord { get; set; }

        public string URL { get; set; }
        public HttpRequestMethod Method { get; set; }
        public IDictionary<string, string> Headers { get; set; }

        public IDictionary<string, string> Values { get; private set; }
        public IDictionary<string, Stream> Files { get; private set; }

        private MultipartWebRequest() { }

        public static MultipartWebRequest CreateRequest(DiscordClient client, string url, HttpRequestMethod method = HttpRequestMethod.GET, IDictionary<string, string> headers = null,
            IDictionary<string, string> values = null, IDictionary<string, Stream> files = null)
        {
            return new MultipartWebRequest
            {
                Discord = client,
                URL = url,
                Method = method,
                Headers = headers,
                Values = values,
                Files = files
            };
        }
    }
}