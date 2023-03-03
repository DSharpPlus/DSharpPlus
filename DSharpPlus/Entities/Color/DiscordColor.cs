// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Globalization;
using System.Linq;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a color used in Discord API.
    /// </summary>
    public partial struct DiscordColor
    {
        private static readonly char[] _hexAlphabet = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// Gets the integer representation of this color.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Gets the red component of this color as an 8-bit integer.
        /// </summary>
        public byte R
            => (byte)((this.Value >> 16) & 0xFF);

        /// <summary>
        /// Gets the green component of this color as an 8-bit integer.
        /// </summary>
        public byte G
            => (byte)((this.Value >> 8) & 0xFF);

        /// <summary>
        /// Gets the blue component of this color as an 8-bit integer.
        /// </summary>
        public byte B
            => (byte)(this.Value & 0xFF);

        /// <summary>
        /// Creates a new color with specified value.
        /// </summary>
        /// <param name="color">Value of the color.</param>
        public DiscordColor(int color)
        {
            this.Value = color;
        }

        /// <summary>
        /// Creates a new color with specified values for red, green, and blue components.
        /// </summary>
        /// <param name="r">Value of the red component.</param>
        /// <param name="g">Value of the green component.</param>
        /// <param name="b">Value of the blue component.</param>
        public DiscordColor(byte r, byte g, byte b)
        {
            this.Value = (r << 16) | (g << 8) | b;
        }

        /// <summary>
        /// Creates a new color with specified values for red, green, and blue components.
        /// </summary>
        /// <param name="r">Value of the red component.</param>
        /// <param name="g">Value of the green component.</param>
        /// <param name="b">Value of the blue component.</param>
        public DiscordColor(float r, float g, float b)
        {
            if (r < 0 || r > 1 || g < 0 || g > 1 || b < 0 || b > 1)
                throw new ArgumentOutOfRangeException("Each component must be between 0.0 and 1.0 inclusive.");

            var rb = (byte)(r * 255);
            var gb = (byte)(g * 255);
            var bb = (byte)(b * 255);

            this.Value = (rb << 16) | (gb << 8) | bb;
        }

        /// <summary>
        /// Creates a new color from specified string representation.
        /// </summary>
        /// <param name="color">String representation of the color. Must be 6 hexadecimal characters, optionally with # prefix.</param>
        public DiscordColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                throw new ArgumentNullException(nameof(color), "Null or empty values are not allowed!");

            if (color.Length != 6 && color.Length != 7)
                throw new ArgumentException(nameof(color), "Color must be 6 or 7 characters in length.");

            color = color.ToUpper();
            if (color.Length == 7 && color[0] != '#')
                throw new ArgumentException(nameof(color), "7-character colors must begin with #.");
            else if (color.Length == 7)
                color = color.Substring(1);

            if (color.Any(xc => !_hexAlphabet.Contains(xc)))
                throw new ArgumentException(nameof(color), "Colors must consist of hexadecimal characters only.");

            this.Value = int.Parse(color, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets a string representation of this color.
        /// </summary>
        /// <returns>String representation of this color.</returns>
        public override string ToString() => $"#{this.Value:X6}";

        public static implicit operator DiscordColor(int value)
            => new(value);
    }
}
