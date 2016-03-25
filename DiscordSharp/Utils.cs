using DiscordSharp.Objects;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace DiscordSharp
{
    internal class Utils
    {
        internal static byte[] ImageToByteArray(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }

        internal static DiscordMessage GenerateMessage(string message)
        {
            //DiscordMessage dm = new DiscordMessage();
            ///Temporarily disabling this
            //if (parse)
            //{
            //    List<string> foundIDS = new List<string>();
            //    Regex r = new Regex("\\@\\w+\\s\\w+");
            //    List<KeyValuePair<string, string>> toReplace = new List<KeyValuePair<string, string>>();
            //    foreach (Match m in r.Matches(message))
            //    {
            //        if (m.Index > 0 && message[m.Index - 1] == '<')
            //            continue;
            //        DiscordMember user = ServersList.Find(x => x.members.Find(y => y.user.username == m.Value.Trim('@')) != null).members.Find(y => y.user.username == m.Value.Trim('@'));
            //        foundIDS.Add(user.user.id);
            //        toReplace.Add(new KeyValuePair<string, string>(m.Value, user.user.id));
            //    }
            //    foreach (var k in toReplace)
            //    {
            //        message = message.Replace(k.Key, "<@" + k.Value + ">");
            //    }
            //}

            //dm.content = message;
            //dm.mentions = foundIDS.ToArray();
            //dm.mentions = new string[] { "" };
            return new DiscordMessage { Content = message };
        }
    }
}
