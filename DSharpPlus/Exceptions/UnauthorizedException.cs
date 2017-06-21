using Newtonsoft.Json.Linq;
using System;

namespace DSharpPlus
{
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// Request that caused the exception
        /// </summary>
        public WebRequest WebRequest { get; internal set; }
        /// <summary>
        /// Response from server
        /// </summary>
        public WebResponse WebResponse { get; internal set; }
        /// <summary>
        /// Received json error
        /// </summary>
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
