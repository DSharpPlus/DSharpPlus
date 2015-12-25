using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    /// <summary>
    /// Convienent wrapper for doing anything web related.
    /// </summary>
    public class WebWrapper
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
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "DELETE";
            httpRequest.UserAgent += $" {UserAgentString}";
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    if (result != "")
                        return result;
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = s.ReadToEnd();
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
        public static string Post(string url, string token, string message)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Headers["authorization"] = token;
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

        public static string PostWithAttachment(string url, string message, string fileToAttach)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.ContentType = "multipart/form-data";
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (Stream s = httpRequest.GetRequestStream())
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
                    Console.WriteLine("Result: " + result);
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
            httpRequest.Headers["authorization"] = token;
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
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "GET";
            httpRequest.UserAgent += $" {UserAgentString}";

            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
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