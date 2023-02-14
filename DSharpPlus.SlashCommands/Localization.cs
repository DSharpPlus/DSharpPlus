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
        Vietnamese,
        Russian
    }

    /// <summary>
    /// A helper class that provides a list of supported localizations.
    /// </summary>
    public static class LocaleHelper
    {
        /// <summary>
        /// A dictionary of supported localizations.
        /// </summary>
        public static readonly Dictionary<Localization, string> LocaleToStrings = new()
        {
            [Localization.AmericanEnglish]  = "en-US",
            [Localization.BritishEnglish]   = "en-GB",
            [Localization.Bulgarian]        = "bg",
            [Localization.Chinese]          = "zh-CN",
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
            [Localization.Vietnamese] = "vi",
            [Localization.Russian] = "ru"
        };

        public static readonly Dictionary<string, Localization> StringsToLocale = new()
        {
            ["en-US"]   = Localization.AmericanEnglish,
            ["en-GB"]   = Localization.BritishEnglish,
            ["bg"]      = Localization.Bulgarian,
            ["zh-CN"]   = Localization.Chinese,
            ["zh-TW"]   = Localization.Taiwanese,
            ["hr"]      = Localization.Croatian,
            ["cs"]      = Localization.Czech,
            ["da"]      = Localization.Danish,
            ["nl"]      = Localization.Dutch,
            ["fi"]      = Localization.Finnish,
            ["fr"]      = Localization.French,
            ["de"]      = Localization.German,
            ["el"]      = Localization.Greek,
            ["hi"]      = Localization.Hindi,
            ["hu"]      = Localization.Hungarian,
            ["it"]      = Localization.Italian,
            ["ja"]      = Localization.Japanese,
            ["ko"]      = Localization.Korean,
            ["lt"]      = Localization.Lithuanian,
            ["no"]      = Localization.Norwegian,
            ["pl"]      = Localization.Polish,
            ["pt-BR"]   = Localization.Portuguese,
            ["ro"]      = Localization.Romanian,
            ["es-ES"]   = Localization.Spanish,
            ["sv-SE"]   = Localization.Swedish,
            ["th"]      = Localization.Thai,
            ["tr"]      = Localization.Turkish,
            ["uk"]      = Localization.Ukrainian,
            ["vi"]      = Localization.Vietnamese,
            ["ru"]      = Localization.Russian
        };
    }
}
