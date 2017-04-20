using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DSharpPlus.CommandsNext.Converters;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Various CommandsNext-related utilities.
    /// </summary>
    public static class CommandsNextUtilities
    {
        private static Regex UserRegex { get; set; }
        private static Dictionary<Type, IArgumentConverter> ArgumentConverters { get; set; }
        private static MethodInfo ConvertGeneric { get; set; }

        static CommandsNextUtilities()
        {
            UserRegex = new Regex(@"<@\!?(\d+?)> ");

            ArgumentConverters = new Dictionary<Type, IArgumentConverter>
            {
                [typeof(string)] = new StringConverter(),
                [typeof(bool)] = new BoolConverter(),
                [typeof(sbyte)] = new Int8Converter(),
                [typeof(byte)] = new Uint8Converter(),
                [typeof(short)] = new Int16Converter(),
                [typeof(ushort)] = new Uint16Converter(),
                [typeof(int)] = new Int32Converter(),
                [typeof(uint)] = new Uint32Converter(),
                [typeof(long)] = new Int64Converter(),
                [typeof(ulong)] = new Uint64Converter(),
                [typeof(float)] = new Float32Converter(),
                [typeof(double)] = new Float64Converter(),
                [typeof(decimal)] = new Float128Converter(),
                [typeof(DateTime)] = new DateTimeConverter(),
                [typeof(DateTimeOffset)] = new DateTimeOffsetConverter(),
                [typeof(TimeSpan)] = new TimeSpanConverter(),
                [typeof(DiscordUser)] = new DiscordUserConverter(),
                [typeof(DiscordMember)] = new DiscordMemberConverter(),
                [typeof(DiscordRole)] = new DiscordRoleConverter(),
                [typeof(DiscordChannel)] = new DiscordChannelConverter(),
                [typeof(DiscordGuild)] = new DiscordGuildConverter()
            };
            var t = typeof(CommandsNextUtilities);
            var ms = t.GetTypeInfo().DeclaredMethods;
            var m = ms.FirstOrDefault(xm => xm.Name == "ConvertArgument" && xm.ContainsGenericParameters && xm.IsStatic && xm.IsPublic);
            ConvertGeneric = m;
        }

        /// <summary>
        /// Checks whether the message has a specified string prefix.
        /// </summary>
        /// <param name="msg">Message to check.</param>
        /// <param name="str">String to check for.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int HasStringPrefix(this DiscordMessage msg, string str)
        {
            var cnt = msg.Content;
            if (str.Length >= cnt.Length)
                return -1;

            if (cnt.StartsWith(str))
                return str.Length;

            return -1;
        }

        /// <summary>
        /// Checks whether the message contains a specified mention prefix.
        /// </summary>
        /// <param name="msg">Message to check.</param>
        /// <param name="str">User to check for.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int HasMentionPrefix(this DiscordMessage msg, DiscordUser user)
        {
            var cnt = msg.Content;
            if (!cnt.StartsWith("<@"))
                return -1;

            var cni = cnt.IndexOf('>');
            var cnp = cnt.Substring(0, cni);
            var m = UserRegex.Match(cnp);
            if (!m.Success)
                return -1;

            var uid = ulong.Parse(m.Groups[1].Value);
            if (user.ID != uid)
                return -1;

            return m.Value.Length;
        }

        /// <summary>
        /// Converts a string to specified type.
        /// </summary>
        /// <typeparam name="T">Type to convert to.</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <param name="ctx">Context in which to convert to.</param>
        /// <returns>Converted object.</returns>
        public static object ConvertArgument<T>(this string value, CommandContext ctx)
        {
            var t = typeof(T);
            if (!ArgumentConverters.ContainsKey(t))
                throw new ArgumentException("There is no converter specified for given type.", nameof(T));

            var cv = ArgumentConverters[t] as IArgumentConverter<T>;
            if (cv == null)
                throw new ArgumentException("Invalid converter registered for this type.", nameof(T));

            if (!cv.TryConvert(value, ctx, out var result))
                throw new ArgumentException("Could not convert specified value to given type.", nameof(value));

            return result;
        }

        /// <summary>
        /// Converts a string to specified type.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="ctx">Context in which to convert to.</param>
        /// <param name="type">Type to convert to.</param>
        /// <returns>Converted object.</returns>
        public static object ConvertArgument(this string value, CommandContext ctx, Type type)
        {
            var m = ConvertGeneric.MakeGenericMethod(type);
            return m.Invoke(null, new object[] { value, ctx });
        }
        
        /// <summary>
        /// Registers an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to register the converter.</typeparam>
        /// <param name="converter">Converter to register.</param>
        public static void RegisterConverter<T>(IArgumentConverter<T> converter)
        {
            if (converter == null)
                throw new ArgumentNullException("Converter cannot be null.", nameof(converter));

            ArgumentConverters[typeof(T)] = converter;
        }

        /// <summary>
        /// Unregisters an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to unregister the converter.</typeparam>
        public static void UnregisterConverter<T>()
        {
            var t = typeof(T);
            if (ArgumentConverters.ContainsKey(t))
                ArgumentConverters.Remove(t);
        }

        /// <summary>
        /// Parses given argument string into individual strings.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <returns>Enumerator of parsed strings.</returns>
        public static IEnumerable<string> SplitArguments(string str)
        {
            var stra = str.Split(' ');
            var strt = "";
            foreach (var xs in stra)
            {
                if (strt == "")
                {
                    if (xs.StartsWith("\"") && xs.EndsWith("\""))
                    {
                        if (xs[xs.Length - 2] != '\\')
                            yield return xs.Substring(1, xs.Length - 2);
                        else
                            strt = xs.Substring(1).Remove(xs.Length - 3, 1);
                    }
                    else if (xs.StartsWith("\""))
                    {
                        strt = xs.Substring(1);
                    }
                    else
                    {
                        yield return xs;
                    }
                }
                else
                {
                    if (xs.EndsWith("\""))
                    {
                        if (xs[xs.Length - 2] != '\\')
                        {
                            strt = string.Concat(strt, " ", xs.Substring(0, xs.Length - 1));
                            yield return strt;
                            strt = "";
                        }
                        else
                        {
                            strt = string.Concat(strt, " ", xs.Remove(xs.Length - 3, 1));
                        }
                    }
                    else
                    {
                        strt = string.Concat(strt, " ", xs);
                    }
                }
            }
        }
    }
}
