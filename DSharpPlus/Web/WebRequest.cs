using System.Collections.Generic;
using System.IO;
using DSharpPlus.Web;

namespace DSharpPlus
{
    public enum ContentType
    {
        Json = 0,
        Multipart = 1
    }

    public class WebRequest
    {
        internal DiscordClient Discord { get; set; }

        public string URL { get; private set; }
        public HttpRequestMethod Method { get; private set; }
        public IDictionary<string, string> Headers { get; private set; }
        
        // Regular request
        public string Payload { get; private set; }

        // Multipart
        public IDictionary<string, string> Values { get; private set; }
        public IDictionary<string, Stream> Files { get; private set; }
        public ContentType ContentType { get; set; }

        private WebRequest() { }

        public static WebRequest CreateRequest(DiscordClient client, string url, HttpRequestMethod method = HttpRequestMethod.GET, IDictionary<string, string> headers = null, string payload = "")
        {
            return new WebRequest
            {
                Discord = client,
                URL = url,
                Method = method,
                Headers = headers,
                Payload = payload,
                ContentType = ContentType.Json
            };
        }

        public static WebRequest CreateMultipartRequest(DiscordClient client, string url, HttpRequestMethod method = HttpRequestMethod.GET, IDictionary<string, string> headers = null,
            IDictionary<string, string> values = null, IDictionary<string, Stream> files = null)
        {
            return new WebRequest
            {
                Discord = client,
                URL = url,
                Method = method,
                Headers = headers,
                Values = values,
                Files = files,
                ContentType = ContentType.Multipart
            };
        }
    }
}
