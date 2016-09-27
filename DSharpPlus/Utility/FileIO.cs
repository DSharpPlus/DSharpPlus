using System.Drawing;
using System.Net;
using System.IO;

namespace DSharpPlus.Utility
{
    public static class FileIO
    {
        public static Bitmap DownloadImage(string url)
        {
            return new Bitmap(WebRequest.Create(url).GetResponse().GetResponseStream());
        }

        public static string DownloadString(string url)
        {
            return new WebClient().DownloadString(url);
        }

        public static string LoadString(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
