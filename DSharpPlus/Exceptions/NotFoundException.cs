using Newtonsoft.Json.Linq;
using System;

namespace DSharpPlus
{
    public class NotFoundException : Exception
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

        public NotFoundException(IWebRequest request, WebResponse response) : base("Not found: " + response.ResponseCode)
        {
            this.WebRequest = request;
            this.WebResponse = response;

            JObject j = JObject.Parse(response.Response);

            if (j["message"] != null)
                JsonMessage = j["message"].ToString();
        }
    }
}
