using Newtonsoft.Json.Linq;
using System;

namespace DSharpPlus
{
    public class UnauthorizedException : Exception
    {
        public WebRequest WebRequest { get; internal set; }
        public WebResponse WebResponse { get; internal set; }
        public string JsonMessage { get; internal set; }

        public UnauthorizedException(WebRequest request, WebResponse response) : base("Unauthorized: " + response.ResponseCode)
        {
            this.WebRequest = request;
            this.WebResponse = response;
            JObject j = JObject.Parse(response.Response);
            if (j["message"] != null)
                JsonMessage = j["message"].ToString();
        }
    }
}
