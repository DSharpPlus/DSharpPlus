using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCord
{
    /// <summary>
    /// Custom color class just because.
    /// </summary>
    public class Color
    {
        public uint R { get; set; }
        public uint G { get; set; }
        public uint B { get; set; }

        private uint raw;

        public Color(string hex)
        {
            uint asActualHex = Convert.ToUInt32(hex, 16);

            //01 23 45 67
            //FF FF FF FF
            R = (uint)(asActualHex >> 16) & 0xFF;
            G = (uint)(asActualHex >> 8) & 0xFF;
            B = (uint)(asActualHex & 0xFF);
            //A = (asActualHex >> 24) & 0xFF
            raw = asActualHex;
        }

        /// <summary>
        /// Converts color to decimal
        /// </summary>
        public int ToDecimal()
        {
            return (int)raw;
        }

        /// <summary>
        /// Converts color to string
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0:X}", raw);
        }
    }
}
