using System;
using Newtonsoft.Json.Linq;
using DSharpPlus.Net;
using System.IO;
using Newtonsoft.Json;

namespace DSharpPlus.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when the request sent to Discord is too large.
    /// </summary>
    public class RequestSizeException : Exception
    {
        /// <summary>
        /// Gets the request that caused the exception.
        /// </summary>
        public BaseRestRequest WebRequest { get; internal set; }

        /// <summary>
        /// Gets the response to the request.
        /// </summary>
        public RestResponse WebResponse { get; internal set; }

        /// <summary>
        /// Gets the JSON received.
        /// </summary>
        public string JsonMessage { get; internal set; }

        internal RequestSizeException(BaseRestRequest request, RestResponse response) : base($"Request entity too large: {response.ResponseCode}. Make sure the data sent is within Discord's upload limit.")
        {
            this.WebRequest = request;
            this.WebResponse = response;

            try
            {
                using var sr = new StreamReader(response.Response);
                using var jr = new JsonTextReader(sr);

                var j = JObject.Load(jr);

                if (j["message"] != null)
                    JsonMessage = j["message"].ToString();
            }
            catch (Exception) { }
        }
    }
}
