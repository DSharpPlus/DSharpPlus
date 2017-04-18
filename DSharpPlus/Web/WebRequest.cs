﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public enum ContentType
    {
        Json = 0,
        Multipart = 1
    }

    public class WebRequest
    {
        public string URL { get; private set; }
        public WebRequestMethod Method { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        
        // Regular request
        public string Payload { get; private set; }

        // Multipart
        public Dictionary<string, string> Values { get; private set; }
        public string FilePath { get; private set; }
        public string FileName { get; private set; } 
        public ContentType ContentType { get; set; }

        private WebRequest() { }

        public static WebRequest CreateRequest(string url, WebRequestMethod method = WebRequestMethod.GET, Dictionary<string, string> headers = null, string payload = "")
        {
            return new WebRequest
            {
                URL = url,
                Method = method,
                Headers = headers,
                Payload = payload,
                ContentType = ContentType.Json
            };
        }

        public static WebRequest CreateMultipartRequest(string url, WebRequestMethod method = WebRequestMethod.GET, Dictionary<string, string> headers = null,
            Dictionary<string, string> values = null, string filepath = "", string filename = "")
        {
            return new WebRequest
            {
                URL = url,
                Method = method,
                Headers = headers,
                Values = values,
                FilePath = filepath,
                FileName = filename,
                ContentType = ContentType.Multipart
            };
        }

        public async Task<WebResponse> HandleRequestAsync() => await WebWrapper.HandleRequestAsync(this);
    }
}
