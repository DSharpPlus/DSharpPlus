using Newtonsoft.Json.Linq;
using System;

namespace DSharpPlus
{
    public class RateLimitException : Exception
    {
        /// <summary>
        /// Request that caused the exception
        /// </summary>
        public IWebRequest WebRequest { get; internal set; }
        /// <summary>
        /// Response from server
        /// </summary>
        public WebResponse WebResponse { get; internal set; }
        /// <summary>
        /// Received json error
        /// </summary>
        public string JsonMessage { get; internal set; }

        public RateLimitException(IWebRequest request, WebResponse response) : base("Rate limited: " + response.ResponseCode)
        {
            this.WebRequest = request;
            this.WebResponse = response;

            JObject j = JObject.Parse(response.Response);

            if (j["message"] != null)
                JsonMessage = j["message"].ToString();
        }
    }
}
