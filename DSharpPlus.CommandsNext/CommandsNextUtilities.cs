using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

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
        private static Dictionary<Type, string> UserFriendlyTypeNames { get; set; }

        static CommandsNextUtilities()
        {
            UserRegex = new Regex(@"<@\!?(\d+?)> ", RegexOptions.ECMAScript);

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
                [typeof(DiscordGuild)] = new DiscordGuildConverter(),
                [typeof(DiscordMessage)] = new DiscordMessageConverter(),
                [typeof(DiscordEmoji)] = new DiscordEmojiConverter()
            };

            var t = typeof(CommandsNextUtilities);
            var ms = t.GetTypeInfo().DeclaredMethods;
            var m = ms.FirstOrDefault(xm => xm.Name == "ConvertArgument" && xm.ContainsGenericParameters && xm.IsStatic && xm.IsPublic);
            ConvertGeneric = m;

            UserFriendlyTypeNames = new Dictionary<Type, string>()
            {
                [typeof(string)] = "string",
                [typeof(bool)] = "boolean",
                [typeof(sbyte)] = "signed byte",
                [typeof(byte)] = "byte",
                [typeof(short)] = "short",
                [typeof(ushort)] = "unsigned short",
                [typeof(int)] = "int",
                [typeof(uint)] = "unsigned int",
                [typeof(long)] = "long",
                [typeof(ulong)] = "unsigned long",
                [typeof(float)] = "float",
                [typeof(double)] = "double",
                [typeof(decimal)] = "decimal",
                [typeof(DateTime)] = "date and time",
                [typeof(DateTimeOffset)] = "date and time",
                [typeof(TimeSpan)] = "time span",
                [typeof(DiscordUser)] = "user",
                [typeof(DiscordMember)] = "member",
                [typeof(DiscordRole)] = "role",
                [typeof(DiscordChannel)] = "channel",
                [typeof(DiscordGuild)] = "guild",
                [typeof(DiscordMessage)] = "message",
                [typeof(DiscordEmoji)] = "emoji"
            };
        }

        /// <summary>
        /// Checks whether the message has a specified string prefix.
        /// </summary>
        /// <param name="msg">Message to check.</param>
        /// <param name="str">String to check for.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int GetStringPrefixLength(this DiscordMessage msg, string str)
        {
            var cnt = msg.Content;
            if (str.Length >= cnt.Length)
                return -1;

            if (!cnt.StartsWith(str))
                return -1;

            int sn = 0;
            for (var i = str.Length; i < cnt.Length; i++)
                if (char.IsWhiteSpace(cnt[i]))
                    sn++;
                else
                    break;

            return str.Length + sn;
        }

        /// <summary>
        /// Checks whether the message contains a specified mention prefix.
        /// </summary>
        /// <param name="msg">Message to check.</param>
        /// <param name="user">User to check for.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int GetMentionPrefixLength(this DiscordMessage msg, DiscordUser user)
        {
            var cnt = msg.Content;
            if (!cnt.StartsWith("<@"))
                return -1;

            var cni = cnt.IndexOf('>');
            if (cni == -1 || cnt.Length <= cni + 2)
                return -1;

            var cnp = cnt.Substring(0, cni + 2);
            var m = UserRegex.Match(cnp);
            if (!m.Success)
                return -1;

            var uid = ulong.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            if (user.Id != uid)
                return -1;

            int sn = 0;
            for (var i = m.Value.Length; i < cnt.Length; i++)
                if (char.IsWhiteSpace(cnt[i]))
                    sn++;
                else
                    break;

            return m.Value.Length + sn;
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
            try
            {
                return m.Invoke(null, new object[] { value, ctx });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
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
        /// Registers a user-friendly type name.
        /// </summary>
        /// <typeparam name="T">Type to register the name for.</typeparam>
        /// <param name="value">Name to register.</param>
        public static void RegisterUserFriendlyTypeName<T>(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("Name cannot be null or empty.", nameof(value));

            UserFriendlyTypeNames[typeof(T)] = value;
        }

        /// <summary>
        /// Converts a type into user-friendly type name.
        /// </summary>
        /// <param name="t">Type to convert.</param>
        /// <returns>User-friendly type name.</returns>
        public static string ToUserFriendlyName(this Type t)
        {
            if (UserFriendlyTypeNames.ContainsKey(t))
                return UserFriendlyTypeNames[t];
            return t.Name;
        }

        internal static string ExtractNextArgument(string str, out string remainder)
        {
            remainder = null;
            if (string.IsNullOrWhiteSpace(str))
                return null;

            var in_backtick = false;
            var in_triple_backtick = false;
            var in_quote = false;
            var in_escape = false;

            var i = 0;
            for (; i < str.Length; i++)
                if (!char.IsWhiteSpace(str[i]))
                    break;
            if (i > 0)
                str = str.Substring(i);
            var x = i;
            
            var ep = -1;
            for (i = 0; i < str.Length; i++)
            {
                if (char.IsWhiteSpace(str[i]) && !in_quote && !in_triple_backtick && !in_backtick && !in_escape)
                    ep = i;

                if (str[i] == '\\')
                {
                    if (!in_escape && !in_backtick && !in_triple_backtick)
                    {
                        in_escape = true;
                        if (str.IndexOf("\\`", i) == i || str.IndexOf("\\\"", i) == i || str.IndexOf("\\\\", i) == i || (str.Length >= i && char.IsWhiteSpace(str[i + 1])))
                            str = str.Remove(i, 1);
                        else
                            i++;
                    }
                    else if ((in_backtick || in_triple_backtick) && str.IndexOf("\\`", i) == i)
                    {
                        in_escape = true;
                        str = str.Remove(i, 1);
                    }
                }

                if (str[i] == '`' && !in_escape)
                {
                    if (in_triple_backtick && str.IndexOf("```", i) == i)
                    {
                        in_triple_backtick = false;
                        i += 2;
                    }
                    else if (!in_backtick && str.IndexOf("```", i) == i)
                    {
                        in_triple_backtick = true;
                        i += 2;
                    }

                    if (in_backtick && str.IndexOf("```", i) != i)
                        in_backtick = false;
                    else if (!in_triple_backtick && str.IndexOf("```", i) == i)
                        in_backtick = true;
                }

                if (str[i] == '"' && !in_escape && !in_backtick && !in_triple_backtick)
                {
                    str = str.Remove(i, 1);
                    i--;

                    if (!in_quote)
                        in_quote = true;
                    else
                        in_quote = false;
                }

                if (in_escape)
                    in_escape = false;

                if (ep != -1)
                {
                    remainder = str.Substring(ep);
                    return str.Substring(0, ep);
                }
            }
            
            remainder = null;
            return str;
        }

        internal static object[] BindArguments(CommandContext ctx, bool ignore_surplus)
        {
            var cmd = ctx.Command;

            var args = new object[cmd.Arguments.Count + 1];
            args[0] = ctx;

            var argstr = ctx.RawArgumentString;
            var argrmd = "";
            var argv = "";
            for (var i = 0; i < ctx.Command.Arguments.Count; i++)
            {
                var arg = ctx.Command.Arguments[i];
                if (arg.IsCatchAll)
                {
                    if (arg._is_array)
                    {
                        var lst = new List<object>();
                        while (true)
                        {
                            argv = ExtractNextArgument(argstr, out argrmd);
                            if (argv == null)
                                break;

                            argstr = argrmd;
                            lst.Add(ConvertArgument(argv, ctx, arg.Type));
                        }
                        
                        var arr = Array.CreateInstance(arg.Type, lst.Count);
                        (lst as System.Collections.IList).CopyTo(arr, 0);
                        args[i + 1] = arr;

                        argstr = string.Empty;
                        break;
                    }
                    else
                    {
                        if (argstr == null)
                        {
                            args[i + 1] = arg.DefaultValue;
                            break;
                        }

                        var j = 0;
                        for (; j < argstr.Length; j++)
                            if (!char.IsWhiteSpace(argstr[j]))
                                break;
                        if (j > 0)
                            argstr = argstr.Substring(j);
                        
                        argv = argstr;
                        args[i + 1] = ConvertArgument(argv, ctx, arg.Type);

                        argstr = string.Empty;
                        break;
                    }
                }
                else
                {
                    argv = ExtractNextArgument(argstr, out argrmd);
                }

                if (argv == null && !arg.IsOptional && !arg.IsCatchAll)
                    throw new ArgumentException("Not enough arguments supplied to the command.");
                else if (argv == null)
                {
                    //break;
                    args[i + 1] = arg.DefaultValue;
                }
                else
                {
                    args[i + 1] = ConvertArgument(argv, ctx, arg.Type);
                }
                argstr = argrmd;
            }

            if (!ignore_surplus && !string.IsNullOrWhiteSpace(argstr))
                throw new ArgumentException("Too many arguments were supplied to this command.");

            return args;
        }

        internal static bool IsModuleCandidateType(this Type type) =>
            type.GetTypeInfo().IsModuleCandidateType();

        internal static bool IsModuleCandidateType(this TypeInfo ti)
        {
            // check if compiler-generated
            if (ti.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
                return false;

            // check if anonymous
            if (ti.IsGenericType && ti.Name.Contains("AnonymousType") && (ti.Name.StartsWith("<>") || ti.Name.StartsWith("VB$")) && (ti.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
                return false;

            // check if abstract, static, or not a class
            if (!ti.IsClass || ti.IsAbstract)
                return false;

            // check if delegate type
            var dlgt = typeof(Delegate).GetTypeInfo();
            if (dlgt.IsAssignableFrom(ti))
                return false;

            // qualifies
            return true;
        }

        internal static bool IsCommandCandidate(this MethodInfo mi, out ParameterInfo[] ps)
        {
            ps = null;
            // check if exists
            if (mi == null)
                return false;

            // check if static or non-public
            if (mi.IsStatic || !mi.IsPublic)
                return false;

            // check if appropriate return and arguments
            ps = mi.GetParameters();
            if (!ps.Any() || ps.First().ParameterType != typeof(CommandContext) || mi.ReturnType != typeof(Task))
                return false;

            // qualifies
            return true;
        }
    }
}
