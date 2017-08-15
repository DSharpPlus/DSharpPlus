using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordColor
    {
        internal int _color;

        public int Color {
            get { return _color; }
            set { _color = value; }
        }

        public int R
        {
            get { return (_color >> 16); }
            set { _color = (value << 16) | (G << 8) | (B << 0); }
        }

        public int G
        {
            get { return (_color >> 8); }
            set { _color = (R << 16) | (value << 8) | (B << 0); }
        }

        public int B
        {
            get { return (_color >> 0); }
            set { _color = (R << 16) | (G << 8) | (value << 0); }
        }

        public DiscordColor()
        {
            _color = 0;
        }

        public DiscordColor(int color)
        {
            _color = color;
        }

        public DiscordColor(int r, int g, int b)
        {
            if (r > 255 || g > 255 || b > 255 || r < 0 || g < 0 || b < 0)
                throw new ArgumentException("R, G and B should each be under 255 and above -1!");
            _color = (r << 16) | (g << 8) | b;
        }

        public DiscordColor(string color)
        {
            if (color == null)
                throw new ArgumentException("Null values are not allowed!");
            string TrimmedColor = color.Trim('#');
            _color = int.Parse(TrimmedColor, System.Globalization.NumberStyles.HexNumber);
        }

        public int ToInt()
        {
            return _color;
        }

        public override string ToString()
        {
            return Convert.ToString(_color, 16);
        }
    }
}
