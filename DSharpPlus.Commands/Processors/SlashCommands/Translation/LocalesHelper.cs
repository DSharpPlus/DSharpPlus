namespace DSharpPlus.Commands.Processors.SlashCommands.Translation;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

public static class LocalesHelper
{
    public static readonly FrozenDictionary<string, Locales> EnglishToLocale;
    public static readonly FrozenDictionary<string, Locales> NativeToLocale;
    public static readonly FrozenDictionary<Locales, string> LocaleToEnglish;
    public static readonly FrozenDictionary<Locales, string> LocaleToNative;

    static LocalesHelper()
    {
        Dictionary<string, Locales> englishToLocale = new()
        {
            ["Indonesian"] = Locales.id,
            ["Danish"] = Locales.da,
            ["German"] = Locales.de,
            ["English, UK"] = Locales.en_GB,
            ["English, US"] = Locales.en_US,
            ["Spanish"] = Locales.es_ES,
            ["French"] = Locales.fr,
            ["Croatian"] = Locales.hr,
            ["Italian"] = Locales.it,
            ["Lithuanian"] = Locales.lt,
            ["Hungarian"] = Locales.hu,
            ["Dutch"] = Locales.nl,
            ["Norwegian"] = Locales.no,
            ["Polish"] = Locales.pl,
            ["Portuguese"] = Locales.pt_BR,
            ["Romanian"] = Locales.ro,
            ["Finnish"] = Locales.fi,
            ["Swedish"] = Locales.sv_SE,
            ["Vietnamese"] = Locales.vi,
            ["Turkish"] = Locales.tr,
            ["Czech"] = Locales.cs,
            ["Greek"] = Locales.el,
            ["Bulgarian"] = Locales.bg,
            ["Russian"] = Locales.ru,
            ["Ukrainian"] = Locales.uk,
            ["Hindi"] = Locales.hi,
            ["Thai"] = Locales.th,
            ["Chinese, China"] = Locales.zh_CN,
            ["Japanese"] = Locales.ja,
            ["Chinese"] = Locales.zh_TW,
            ["Korean"] = Locales.ko
        };

        Dictionary<string, Locales> nativeToLocale = new()
        {
            ["Bahasa Indonesia"] = Locales.id,
            ["Dansk"] = Locales.da,
            ["Deutsch"] = Locales.de,
            ["English, UK"] = Locales.en_GB,
            ["English, US"] = Locales.en_US,
            ["Español"] = Locales.es_ES,
            ["Français"] = Locales.fr,
            ["Hrvatski"] = Locales.hr,
            ["Italiano"] = Locales.it,
            ["Lietuviškai"] = Locales.lt,
            ["Magyar"] = Locales.hu,
            ["Nederlands"] = Locales.nl,
            ["Norsk"] = Locales.no,
            ["Polski"] = Locales.pl,
            ["Português do Brasil"] = Locales.pt_BR,
            ["Română"] = Locales.ro,
            ["Suomi"] = Locales.fi,
            ["Svenska"] = Locales.sv_SE,
            ["Tiếng Việt"] = Locales.vi,
            ["Türkçe"] = Locales.tr,
            ["Čeština"] = Locales.cs,
            ["Ελληνικά"] = Locales.el,
            ["български"] = Locales.bg,
            ["Pусский"] = Locales.ru,
            ["Українська"] = Locales.uk,
            ["हिन्दी"] = Locales.hi,
            ["ไทย"] = Locales.th,
            ["中文"] = Locales.zh_CN,
            ["日本語"] = Locales.ja,
            ["繁體中文"] = Locales.zh_TW,
            ["한국어"] = Locales.ko
        };

        Dictionary<Locales, string> localeToEnglish = englishToLocale.ToDictionary(x => x.Value, x => x.Key);
        Dictionary<Locales, string> localeToNative = nativeToLocale.ToDictionary(x => x.Value, x => x.Key);

        EnglishToLocale = englishToLocale.ToFrozenDictionary();
        NativeToLocale = nativeToLocale.ToFrozenDictionary();
        LocaleToEnglish = localeToEnglish.ToFrozenDictionary();
        LocaleToNative = localeToNative.ToFrozenDictionary();
    }
}
