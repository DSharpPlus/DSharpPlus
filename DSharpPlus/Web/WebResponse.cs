using System.Collections.Generic;

namespace DSharpPlus
{
    public class WebResponse
    {
        public int ResponseCode { get; internal set; }
        public Dictionary<string, string> Headers { get; internal set; }
        public string Response { get; internal set; }

        internal WebResponse() { }
    }
}
