using Newtonsoft.Json.Linq;
using System;
using System.IO;
using DSharpPlus.Net;
using Newtonsoft.Json;
using DSharpPlus.Net.Serialization;

namespace DSharpPlus.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a malformed request is sent.
    /// </summary>
    public class BadRequestException : Exception
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
        /// Gets the error code for this exception.
        /// </summary>
        public int Code { get; internal set; }

        /// <summary>
        /// Gets the JSON message received.
        /// </summary>
        public string JsonMessage { get; internal set; }

        /// <summary>
        /// Gets the form error responses in JSON format.
        /// </summary>
        public string Errors { get; internal set; }

        internal BadRequestException(BaseRestRequest request, RestResponse response) : base("Bad request: " + response.ResponseCode)
        {
            this.WebRequest = request;
            this.WebResponse = response;

            try
            {
                var j = DiscordJson.LoadJObject(response.Response);

                if (j["code"] != null)
                    this.Code = (int)j["code"];

                if (j["message"] != null)
                    this.JsonMessage = j["message"].ToString();

                if (j["errors"] != null)
                    this.Errors = j["errors"].ToString();
            }
            catch { }
        }
    }
}
