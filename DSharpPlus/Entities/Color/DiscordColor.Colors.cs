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

namespace DSharpPlus.Entities
{
    public partial struct DiscordColor
    {
        #region Black and White
        /// <summary>
        /// Represents no color, or integer 0;
        /// </summary>
        public static DiscordColor None { get; } = new DiscordColor(0);

        /// <summary>
        /// A near-black color. Due to API limitations, the color is #010101, rather than #000000, as the latter is treated as no color.
        /// </summary>
        public static DiscordColor Black { get; } = new DiscordColor(0x010101);

        /// <summary>
        /// White, or #FFFFFF.
        /// </summary>
        public static DiscordColor White { get; } = new DiscordColor(0xFFFFFF);

        /// <summary>
        /// Gray, or #808080.
        /// </summary>
        public static DiscordColor Gray { get; } = new DiscordColor(0x808080);

        /// <summary>
        /// Dark gray, or #A9A9A9.
        /// </summary>
        public static DiscordColor DarkGray { get; } = new DiscordColor(0xA9A9A9);

        /// <summary>
        /// Light gray, or #808080.
        /// </summary>
        public static DiscordColor LightGray { get; } = new DiscordColor(0xD3D3D3);

        // dev-approved
        /// <summary>
        /// Very dark gray, or #666666.
        /// </summary>
        public static DiscordColor VeryDarkGray { get; } = new DiscordColor(0x666666);
        #endregion

        #region Discord branding colors
        // https://discord.com/branding

        /// <summary>
        /// Discord Blurple, or #5865F2.
        /// </summary>
        public static DiscordColor Blurple { get; } = new DiscordColor(0x5865F2);

        /// <summary>
        /// Discord Grayple, or #99AAB5.
        /// </summary>
        public static DiscordColor Grayple { get; } = new DiscordColor(0x99AAB5);

        /// <summary>
        /// Discord Dark, But Not Black, or #2C2F33.
        /// </summary>
        public static DiscordColor DarkButNotBlack { get; } = new DiscordColor(0x2C2F33);

        /// <summary>
        /// Discord Not QuiteBlack, or #23272A.
        /// </summary>
        public static DiscordColor NotQuiteBlack { get; } = new DiscordColor(0x23272A);
        #endregion

        #region Other colors
        /// <summary>
        /// Red, or #FF0000.
        /// </summary>
        public static DiscordColor Red { get; } = new DiscordColor(0xFF0000);

        /// <summary>
        /// Dark red, or #7F0000.
        /// </summary>
        public static DiscordColor DarkRed { get; } = new DiscordColor(0x7F0000);

        /// <summary>
        /// Green, or #00FF00.
        /// </summary>
        public static DiscordColor Green { get; } = new DiscordColor(0x00FF00);

        /// <summary>
        /// Dark green, or #007F00.
        /// </summary>
        public static DiscordColor DarkGreen { get; } = new DiscordColor(0x007F00);

        /// <summary>
        /// Blue, or #0000FF.
        /// </summary>
        public static DiscordColor Blue { get; } = new DiscordColor(0x0000FF);

        /// <summary>
        /// Dark blue, or #00007F.
        /// </summary>
        public static DiscordColor DarkBlue { get; } = new DiscordColor(0x00007F);

        /// <summary>
        /// Yellow, or #FFFF00.
        /// </summary>
        public static DiscordColor Yellow { get; } = new DiscordColor(0xFFFF00);

        /// <summary>
        /// Cyan, or #00FFFF.
        /// </summary>
        public static DiscordColor Cyan { get; } = new DiscordColor(0x00FFFF);

        /// <summary>
        /// Magenta, or #FF00FF.
        /// </summary>
        public static DiscordColor Magenta { get; } = new DiscordColor(0xFF00FF);

        /// <summary>
        /// Teal, or #008080.
        /// </summary>
        public static DiscordColor Teal { get; } = new DiscordColor(0x008080);

        // meme
        /// <summary>
        /// Aquamarine, or #00FFBF.
        /// </summary>
        public static DiscordColor Aquamarine { get; } = new DiscordColor(0x00FFBF);

        /// <summary>
        /// Gold, or #FFD700.
        /// </summary>
        public static DiscordColor Gold { get; } = new DiscordColor(0xFFD700);

        // To be fair, you have to have a very high IQ to understand Goldenrod .
        // The tones are extremely subtle, and without a solid grasp of artistic
        // theory most of the beauty will go over a typical painter's head.
        // There's also the flower's nihilistic style, which is deftly woven
        // into its characterization - it's pollinated by the Bombus cryptarum
        // bumblebee, for instance. The fans understand this stuff; they have
        // the intellectual capacity to truly appreciate the depth of this
        // flower, to realize that it's not just a color - it says something
        // deep about LIFE. As a consequence people who dislike Goldenrod truly
        // ARE idiots - of course they wouldn't appreciate, for instance, the
        // beauty in the bumblebee species' complex presence in the British Isles,
        // which is cryptically explained by Turgenev's Russian epic Fathers and
        // Sons I'm blushing right now just imagining one of those addlepated
        // simpletons scratching their heads in confusion as nature's genius
        // unfolds itself on their computer screens. What fools... how I pity them.
        // 😂 And yes by the way, I DO have a goldenrod tattoo. And no, you cannot
        // see it. It's for the ladies' eyes only- And even they have to
        // demonstrate that they're within 5 IQ points of my own (preferably lower) beforehand.
        /// <summary>
        /// Goldenrod, or #DAA520.
        /// </summary>
        public static DiscordColor Goldenrod { get; } = new DiscordColor(0xDAA520);

        // emzi's favourite
        /// <summary>
        /// Azure, or #007FFF.
        /// </summary>
        public static DiscordColor Azure { get; } = new DiscordColor(0x007FFF);

        /// <summary>
        /// Rose, or #FF007F.
        /// </summary>
        public static DiscordColor Rose { get; } = new DiscordColor(0xFF007F);

        /// <summary>
        /// Spring green, or #00FF7F.
        /// </summary>
        public static DiscordColor SpringGreen { get; } = new DiscordColor(0x00FF7F);

        /// <summary>
        /// Chartreuse, or #7FFF00.
        /// </summary>
        public static DiscordColor Chartreuse { get; } = new DiscordColor(0x7FFF00);

        /// <summary>
        /// Orange, or #FFA500.
        /// </summary>
        public static DiscordColor Orange { get; } = new DiscordColor(0xFFA500);

        /// <summary>
        /// Purple, or #800080.
        /// </summary>
        public static DiscordColor Purple { get; } = new DiscordColor(0x800080);

        /// <summary>
        /// Violet, or #EE82EE.
        /// </summary>
        public static DiscordColor Violet { get; } = new DiscordColor(0xEE82EE);

        /// <summary>
        /// Brown, or #A52A2A.
        /// </summary>
        public static DiscordColor Brown { get; } = new DiscordColor(0xA52A2A);

        // meme
        /// <summary>
        /// Hot pink, or #FF69B4
        /// </summary>
        public static DiscordColor HotPink { get; } = new DiscordColor(0xFF69B4);

        /// <summary>
        /// Lilac, or #C8A2C8.
        /// </summary>
        public static DiscordColor Lilac { get; } = new DiscordColor(0xC8A2C8);

        /// <summary>
        /// Cornflower blue, or #6495ED.
        /// </summary>
        public static DiscordColor CornflowerBlue { get; } = new DiscordColor(0x6495ED);

        /// <summary>
        /// Midnight blue, or #191970.
        /// </summary>
        public static DiscordColor MidnightBlue { get; } = new DiscordColor(0x191970);

        /// <summary>
        /// Wheat, or #F5DEB3.
        /// </summary>
        public static DiscordColor Wheat { get; } = new DiscordColor(0xF5DEB3);

        /// <summary>
        /// Indian red, or #CD5C5C.
        /// </summary>
        public static DiscordColor IndianRed { get; } = new DiscordColor(0xCD5C5C);

        /// <summary>
        /// Turquoise, or #30D5C8.
        /// </summary>
        public static DiscordColor Turquoise { get; } = new DiscordColor(0x30D5C8);

        /// <summary>
        /// Sap green, or #507D2A.
        /// </summary>
        public static DiscordColor SapGreen { get; } = new DiscordColor(0x507D2A);

        // meme, specifically bob ross
        /// <summary>
        /// Phthalo blue, or #000F89.
        /// </summary>
        public static DiscordColor PhthaloBlue { get; } = new DiscordColor(0x000F89);

        // meme, specifically bob ross
        /// <summary>
        /// Phthalo green, or #123524.
        /// </summary>
        public static DiscordColor PhthaloGreen { get; } = new DiscordColor(0x123524);

        /// <summary>
        /// Sienna, or #882D17.
        /// </summary>
        public static DiscordColor Sienna { get; } = new DiscordColor(0x882D17);
        #endregion
    }
}
