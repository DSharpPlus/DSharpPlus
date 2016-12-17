using System.Net;

namespace DSharpPlus
{
    public class WebResponse
    {
        public int ResponseCode { get; internal set; }
        public WebHeaderCollection Headers { get; internal set; }
        public string Response { get; internal set; }

        internal WebResponse() { }
    }
}
