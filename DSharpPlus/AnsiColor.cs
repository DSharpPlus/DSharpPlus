// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
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

namespace DSharpPlus
{
    /// <summary>
    /// A list of ansi colors supported by Discord.
    /// </summary>
    /// <remarks>
    /// Background support in the client is dodgy at best.
    /// These colors are mapped as per the ansi standard, but may not appear correctly in the client.
    /// </remarks>
    public enum AnsiColor
    {
        Reset = 0,
        Bold = 1,
        Underline = 4,

        Black = 30,
        Red = 31,
        Green = 32,
        Yellow = 33,
        Blue = 34,
        Magenta = 35,
        Cyan = 36,
        White = 37,
        LightGray = 38,

        BlackBackground = 40,
        RedBackground = 41,
        GreenBackground = 42,
        YellowBackground = 43,
        BlueBackground = 44,
        MagentaBackground = 45,
        CyanBackground = 46,
        WhiteBackground = 47,

    }
}
