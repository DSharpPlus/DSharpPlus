using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCord.Toolbox
{
    public static class Tools
    {
        /// <summary>
        /// Takes a String containing path and converts it in to a Stream.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Stream ToStream(string uri)
        {
            Stream fs = File.OpenRead(uri);
            return fs;
        }
    }
}
