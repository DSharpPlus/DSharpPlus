namespace DSharpPlus.Commands.Processors.SlashCommands.Localization;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

public static class LocalesHelper
{
    public static readonly FrozenDictionary<string, DiscordLocale> EnglishToLocale;
    public static readonly FrozenDictionary<string, DiscordLocale> NativeToLocale;
    public static readonly FrozenDictionary<DiscordLocale, string> LocaleToEnglish;
    public static readonly FrozenDictionary<DiscordLocale, string> LocaleToNative;

    static LocalesHelper()
    {
        Dictionary<string, DiscordLocale> englishToLocale = new()
        {
            ["Indonesian"] = DiscordLocale.id,
            ["Danish"] = DiscordLocale.da,
            ["German"] = DiscordLocale.de,
            ["English, UK"] = DiscordLocale.en_GB,
            ["English, US"] = DiscordLocale.en_US,
            ["Spanish"] = DiscordLocale.es_ES,
            ["French"] = DiscordLocale.fr,
            ["Croatian"] = DiscordLocale.hr,
            ["Italian"] = DiscordLocale.it,
            ["Lithuanian"] = DiscordLocale.lt,
            ["Hungarian"] = DiscordLocale.hu,
            ["Dutch"] = DiscordLocale.nl,
            ["Norwegian"] = DiscordLocale.no,
            ["Polish"] = DiscordLocale.pl,
            ["Portuguese"] = DiscordLocale.pt_BR,
            ["Romanian"] = DiscordLocale.ro,
            ["Finnish"] = DiscordLocale.fi,
            ["Swedish"] = DiscordLocale.sv_SE,
            ["Vietnamese"] = DiscordLocale.vi,
            ["Turkish"] = DiscordLocale.tr,
            ["Czech"] = DiscordLocale.cs,
            ["Greek"] = DiscordLocale.el,
            ["Bulgarian"] = DiscordLocale.bg,
            ["Russian"] = DiscordLocale.ru,
            ["Ukrainian"] = DiscordLocale.uk,
            ["Hindi"] = DiscordLocale.hi,
            ["Thai"] = DiscordLocale.th,
            ["Chinese, China"] = DiscordLocale.zh_CN,
            ["Japanese"] = DiscordLocale.ja,
            ["Chinese"] = DiscordLocale.zh_TW,
            ["Korean"] = DiscordLocale.ko
        };

        Dictionary<string, DiscordLocale> nativeToLocale = new()
        {
            ["Bahasa Indonesia"] = DiscordLocale.id,
            ["Dansk"] = DiscordLocale.da,
            ["Deutsch"] = DiscordLocale.de,
            ["English, UK"] = DiscordLocale.en_GB,
            ["English, US"] = DiscordLocale.en_US,
            ["Español"] = DiscordLocale.es_ES,
            ["Français"] = DiscordLocale.fr,
            ["Hrvatski"] = DiscordLocale.hr,
            ["Italiano"] = DiscordLocale.it,
            ["Lietuviškai"] = DiscordLocale.lt,
            ["Magyar"] = DiscordLocale.hu,
            ["Nederlands"] = DiscordLocale.nl,
            ["Norsk"] = DiscordLocale.no,
            ["Polski"] = DiscordLocale.pl,
            ["Português do Brasil"] = DiscordLocale.pt_BR,
            ["Română"] = DiscordLocale.ro,
            ["Suomi"] = DiscordLocale.fi,
            ["Svenska"] = DiscordLocale.sv_SE,
            ["Tiếng Việt"] = DiscordLocale.vi,
            ["Türkçe"] = DiscordLocale.tr,
            ["Čeština"] = DiscordLocale.cs,
            ["Ελληνικά"] = DiscordLocale.el,
            ["български"] = DiscordLocale.bg,
            ["Pусский"] = DiscordLocale.ru,
            ["Українська"] = DiscordLocale.uk,
            ["हिन्दी"] = DiscordLocale.hi,
            ["ไทย"] = DiscordLocale.th,
            ["中文"] = DiscordLocale.zh_CN,
            ["日本語"] = DiscordLocale.ja,
            ["繁體中文"] = DiscordLocale.zh_TW,
            ["한국어"] = DiscordLocale.ko
        };

        Dictionary<DiscordLocale, string> localeToEnglish = englishToLocale.ToDictionary(x => x.Value, x => x.Key);
        Dictionary<DiscordLocale, string> localeToNative = nativeToLocale.ToDictionary(x => x.Value, x => x.Key);

        EnglishToLocale = englishToLocale.ToFrozenDictionary();
        NativeToLocale = nativeToLocale.ToFrozenDictionary();
        LocaleToEnglish = localeToEnglish.ToFrozenDictionary();
        LocaleToNative = localeToNative.ToFrozenDictionary();
    }
}
