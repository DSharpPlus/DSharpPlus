using System;
using System.Globalization;

namespace DSharpPlus.Utility
{
    internal class RFCToDateTime
    {
        public static DateTime Convert(string input)
        {
            // Sorry i know this is ugly code
            int day = int.Parse(input.Substring(5, 2));
            int month = DateTime.UtcNow.Month;
            int year = int.Parse(input.Substring(12, 4));
            int hour = int.Parse(input.Substring(17, 2));
            int min = int.Parse(input.Substring(20, 2));
            int sec = int.Parse(input.Substring(23, 2));
            DateTime dt = new DateTime(year, month, day, hour, min, sec);
            return dt;
        }
    }
}
