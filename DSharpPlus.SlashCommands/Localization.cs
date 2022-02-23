// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using System.Collections.Generic;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Supported locals for slash command localizations.
    /// </summary>
    public enum Localization
    {
        AmericanEnglish,
        BritishEnglish,
        PeurtoRicoEnglish,
        Bulgarian,
        Chinese,
        Taiwanese,
        Croatian,
        Czech,
        Danish,
        Dutch,
        Finnish,
        French,
        German,
        Greek,
        Hindi,
        Hungarian,
        Italian,
        Japanese,
        Korean,
        Lithuanian,
        Norwegian,
        Polish,
        Portuguese,
        Romanian,
        Spanish,
        Swedish,
        Thai,
        Turkish,
        Ukrainian,
        Vietnamese
    }

    internal static class LocaleHelper
    {
        public static readonly Dictionary<Localization, string> Localizations = new()
        {
            [Localization.AmericanEnglish] = "en-US",
            [Localization.BritishEnglish] = "en-GB",
            [Localization.Bulgarian] = "bg",
            [Localization.Chinese] = "zh-CN",
            [Localization.Taiwanese] = "zh-TW",
            [Localization.Croatian] = "hr",
            [Localization.Czech] = "cs",
            [Localization.Danish] = "da",
            [Localization.Dutch] = "nl",
            [Localization.Finnish] = "fi",
            [Localization.French] = "fr",
            [Localization.German] = "de",
            [Localization.Greek] = "el",
            [Localization.Hindi] = "hi",
            [Localization.Hungarian] = "hu",
            [Localization.Italian] = "it",
            [Localization.Japanese] = "ja",
            [Localization.Korean] = "ko",
            [Localization.Lithuanian] = "lt",
            [Localization.Norwegian] = "no",
            [Localization.Polish] = "pl",
            [Localization.Portuguese] = "pt-BR",
            [Localization.Romanian] = "ro",
            [Localization.Spanish] = "es-ES",
            [Localization.Swedish] = "sv-SE",
            [Localization.Thai] = "th",
            [Localization.Turkish] = "tr",
            [Localization.Ukrainian] = "uk",
            [Localization.Vietnamese] = "vi"
        };
    }
}
