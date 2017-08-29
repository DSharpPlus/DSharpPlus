using Newtonsoft.Json.Linq;
using System;

namespace DSharpPlus
{
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Request that caused the exception
        /// </summary>
        public BaseWebRequest WebRequest { get; internal set; }
        /// <summary>
        /// Response from server
        /// </summary>
        public WebResponse WebResponse { get; internal set; }
        /// <summary>
        /// Received json error
        /// </summary>
        public string JsonMessage { get; internal set; }

        public BadRequestException(BaseWebRequest request, WebResponse response) : base("Bad request: " + response.ResponseCode)
        {
            this.WebRequest = request;
            this.WebResponse = response;

            JObject j = JObject.Parse(response.Response);

            if (j["message"] != null)
                JsonMessage = j["message"].ToString();
        }
    }
}
