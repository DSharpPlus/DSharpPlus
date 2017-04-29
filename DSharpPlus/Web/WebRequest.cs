using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
        public Dictionary<string, string> Headers { get; private set; }
        
        // Regular request
        public string Payload { get; private set; }

        // Multipart
        public Dictionary<string, string> Values { get; private set; }
        public Dictionary<string, Stream> Files { get; private set; }
        public ContentType ContentType { get; set; }

        private WebRequest() { }

        public static WebRequest CreateRequest(DiscordClient client, string url, HttpRequestMethod method = HttpRequestMethod.GET, Dictionary<string, string> headers = null, string payload = "")
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

        public static WebRequest CreateMultipartRequest(DiscordClient client, string url, HttpRequestMethod method = HttpRequestMethod.GET, Dictionary<string, string> headers = null,
            Dictionary<string, string> values = null, Dictionary<string, Stream> files = null)
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
