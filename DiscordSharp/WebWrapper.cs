using DiscordSharp.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordSharp
{
    /// <summary>
    /// Convienent wrapper for doing anything web related.
    /// </summary>
    internal class WebWrapper
    {
        static string UserAgentString = $"DiscordBot (http://github.com/Luigifan/DiscordSharp, {typeof(DiscordClient).Assembly.GetName().Version.ToString()})";

        /// <summary>
        /// Sends a DELETE HTTP request to the specified URL using the specified token.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string Delete(string url, string token)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = DiscordClient.IsBotAccount ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "DELETE";
            httpRequest.UserAgent += $" {UserAgentString}";
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        JObject jsonTest = JObject.Parse(result);
                        if (jsonTest != null)
                        {
                            if (!jsonTest["bucket"].IsNullOrEmpty()) //you got rate limited punk
                            {
                                throw new RateLimitException(jsonTest["message"].ToString(), jsonTest["retry_after"].ToObject<int>());
                            }
                        }
                    }
                    if (result != "")
                        return result;
                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            return "";
        }

        /// <summary>
        /// Sends a PUT HTTP request to the specified URL using the specified token.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string Put(string url, string token)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = DiscordClient.IsBotAccount ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PUT";
            httpRequest.UserAgent += $" {UserAgentString}";
            httpRequest.ContentLength = 0;
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (result != "")
                        {
                            JObject jsonTest = JObject.Parse(result);
                            if (jsonTest != null)
                            {
                                if (!jsonTest["bucket"].IsNullOrEmpty()) //you got rate limited punk
                                {
                                    throw new RateLimitException(jsonTest["message"].ToString(), jsonTest["retry_after"].ToObject<int>());
                                }
                            }
                            return result;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                throw e;
            }
            return "";
        }

        /// <summary>
        /// Sends a POST HTTP request to the specified URL, using the specified token, sending the specified message.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Post(string url, string token, string message, bool acceptInviteWorkaround = false)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = DiscordClient.IsBotAccount ? "Bot " + token : token;
            if (acceptInviteWorkaround)
                httpRequest.ContentLength = message.Length;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

            //httpRequest.AllowWriteStreamBuffering = false;
            //httpRequest.KeepAlive = false;
            //httpRequest.ProtocolVersion = HttpVersion.Version10;

            //httpRequest.SendChunked = true;
            //httpRequest.TransferEncoding = "unicode";

            if (!string.IsNullOrEmpty(message))
            {
                using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    sw.Write(message);
                    sw.Flush();
                    sw.Close();
                }
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if(httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("401");
                }
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        JObject jsonTest = JObject.Parse(result);
                        if (jsonTest != null)
                        {
                            if (!jsonTest["bucket"].IsNullOrEmpty()) //you got rate limited punk
                            {
                                throw new RateLimitException(jsonTest["message"].ToString(), jsonTest["retry_after"].ToObject<int>());
                            }
                        }
                    }
                    if (result != "")
                        return result;
                }
            }
            catch (WebException e)
            {
                throw e;
            }
            return "";
        }

        public static string HttpUploadFile(string url, string token, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.Headers["authorization"] = DiscordClient.IsBotAccount ? "Bot " + token : token;
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.UserAgent += UserAgentString;
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            System.IO.Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            if (nvc != null)
            {
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            fileStream.CopyTo(rs);

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            wresp = wr.GetResponse();
            System.IO.Stream stream2 = wresp.GetResponseStream();
            StreamReader reader2 = new StreamReader(stream2);
            string returnVal = reader2.ReadToEnd();

            reader2.Close();
            stream2.Close();
            fileStream.Close();
            return returnVal;
        }

        public static string HttpUploadFile(string url, string token, System.IO.Stream file, string paramName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.Headers["authorization"] = DiscordClient.IsBotAccount ? "Bot " + token : token;
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.UserAgent += UserAgentString;
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            System.IO.Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            if (nvc != null)
            {
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            file.CopyTo(rs);

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            wresp = wr.GetResponse();
            System.IO.Stream stream2 = wresp.GetResponseStream();
            StreamReader reader2 = new StreamReader(stream2);
            string returnVal = reader2.ReadToEnd();

            reader2.Close();
            stream2.Close();
            return returnVal;
        }

        /// <summary>
        /// Sends a POST HTTP request to the specified URL, without a token, sending the specified message.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Post(string url, string message)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(message);
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        JObject jsonTest = JObject.Parse(result);
                        if (jsonTest != null)
                        {
                            if (!jsonTest["bucket"].IsNullOrEmpty()) //you got rate limited punk
                            {
                                throw new RateLimitException(jsonTest["message"].ToString(), jsonTest["retry_after"].ToObject<int>());
                            }
                        }
                        if (result != "")
                            return result;
                    }
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    var result = s.ReadToEnd();
                    return result;
                }
            }
            return "";
        }
        [Obsolete]
        public static string PostWithAttachment(string url, string message, string fileToAttach)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.ContentType = "multipart/form-data";
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (System.IO.Stream s = httpRequest.GetRequestStream())
            {
                byte[] f = new byte[4096];
                var ss = File.Open(fileToAttach, FileMode.Open);
                f = new byte[ss.Length];

                ss.Write(f, 0, (int)ss.Length);
                s.Write(f, 0, f.Length);
                //s.Write(Encoding.ASCII.GetBytes(message), f.Length + 1, message.Length);
                s.Flush();
                s.Write(Encoding.ASCII.GetBytes(message), 0, message.Length * 8);
                s.Flush();
                s.Close();
            }
            ////}
            //using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            //{
            //    sw.Write(message);
            //    sw.Flush();
            //    sw.Close();
            //}
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        JObject jsonTest = JObject.Parse(result);
                        if (jsonTest != null)
                        {
                            if (!jsonTest["bucket"].IsNullOrEmpty()) //you got rate limited punk
                            {
                                throw new RateLimitException(jsonTest["message"].ToString(), jsonTest["retry_after"].ToObject<int>());
                            }
                        }
                    }
                    if (result != "")
                        return result;
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    var result = s.ReadToEnd();
                    return result;
                }
            }
            return "";
        }

        /// <summary>
        /// Sends a POST HTTP request to the specified URL, using the specified token, sending the specified message.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Patch(string url, string token, string message)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = DiscordClient.IsBotAccount ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PATCH";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(message);
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        JObject jsonTest = JObject.Parse(result);
                        if (jsonTest != null)
                        {
                            if (!jsonTest["bucket"].IsNullOrEmpty()) //you got rate limited punk
                            {
                                throw new RateLimitException(jsonTest["message"].ToString(), jsonTest["retry_after"].ToObject<int>());
                            }
                        }
                    }
                    if (result != "")
                        return result;
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    var result = s.ReadToEnd();
                    return result;
                }
            }
            return "";
        }

        /// <summary>
        /// Sends a GET Request to the specified url using the provided token.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns>The raw string returned. Or, an error.</returns>
        public static string Get(string url, string token)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = DiscordClient.IsBotAccount ? "Bot " + token : token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "GET";
            httpRequest.UserAgent += $" {UserAgentString}";

            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if(httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("401");
                }
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        try
                        {
                            JObject jsonTest = JObject.Parse(result);
                            if (jsonTest != null)
                            {
                                if (!jsonTest["bucket"].IsNullOrEmpty()) //you got rate limited punk
                                {
                                    throw new RateLimitException(jsonTest["message"].ToString(), jsonTest["retry_after"].ToObject<int>());
                                }
                            }
                        }
                        catch(Exception ) //must be a jarray
                        {
                            return result;
                        }
                        
                    }
                    return result;
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    var result = s.ReadToEnd();
                    return result;
                }
            }
        }

    }
}