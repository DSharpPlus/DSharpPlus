using System.Drawing;
using System.Net;
using System.IO;

namespace DSharpPlus.Utility
{
    public static class FileIO
    {
        /// /// <summary>
        /// Download a Bitmap image
        /// </summary>
        public static Bitmap DownloadImage(string url)
        {
            return new Bitmap(WebRequest.Create(url).GetResponse().GetResponseStream());
        }

        /// <summary>
        /// Get the token of a user, given the email and password
        /// </summary>
        public static string DownloadString(string url)
        {
            return new WebClient().DownloadString(url);
        }

        /// <summary>
        /// Load file as a string
        /// </summary>
        public static string LoadString(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
