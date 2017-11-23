using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private static Regex UserRegex { get; }
        private static Dictionary<Type, IArgumentConverter> ArgumentConverters { get; }
        private static MethodInfo ConvertGeneric { get; }
        private static Dictionary<Type, string> UserFriendlyTypeNames { get; }

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
                [typeof(DiscordEmoji)] = new DiscordEmojiConverter(),
                [typeof(DiscordColor)] = new DiscordColorConverter()
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
                [typeof(DiscordEmoji)] = "emoji",
                [typeof(DiscordColor)] = "color"
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

            //int sn = 0;
            //for (var i = str.Length; i < cnt.Length; i++)
            //    if (char.IsWhiteSpace(cnt[i]))
            //        sn++;
            //    else
            //        break;

            //return str.Length + sn;
            return str.Length;
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

            //int sn = 0;
            //for (var i = m.Value.Length; i < cnt.Length; i++)
            //    if (char.IsWhiteSpace(cnt[i]))
            //        sn++;
            //    else
            //        break;

            //return m.Value.Length + sn;
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

            if (UserFriendlyTypeNames.ContainsKey(t))
                UserFriendlyTypeNames.Remove(t);
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

            var t = typeof(T);
            if (!ArgumentConverters.ContainsKey(t))
                throw new InvalidOperationException("Cannot register a friendly name for a type which has no associated converter.");

            UserFriendlyTypeNames[t] = value;
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

        //internal static string ExtractNextArgument(string str, out string remainder)
        internal static string ExtractNextArgument(string str, ref int startPos)
        {
            //remainder = null;
            if (string.IsNullOrWhiteSpace(str))
                return null;

            var in_backtick = false;
            var in_triple_backtick = false;
            var in_quote = false;
            var in_escape = false;
            var remove = new List<int>(str.Length - startPos);

            var i = startPos;
            for (; i < str.Length; i++)
                if (!char.IsWhiteSpace(str[i]))
                    break;
            startPos = i;

            var ep = -1;
            var sp = startPos;
            for (i = sp; i < str.Length; i++)
            {
                if (char.IsWhiteSpace(str[i]) && !in_quote && !in_triple_backtick && !in_backtick && !in_escape)
                    ep = i;

                if (str[i] == '\\')
                {
                    if (!in_escape && !in_backtick && !in_triple_backtick)
                    {
                        in_escape = true;
                        if (str.IndexOf("\\`", i) == i || str.IndexOf("\\\"", i) == i || str.IndexOf("\\\\", i) == i || (str.Length >= i && char.IsWhiteSpace(str[i + 1])))
                            //str = str.Remove(i, 1);
                            remove.Add(i - sp);
                        i++;
                    }
                    else if ((in_backtick || in_triple_backtick) && str.IndexOf("\\`", i) == i)
                    {
                        in_escape = true;
                        //str = str.Remove(i, 1);
                        remove.Add(i - sp);
                        i++;
                    }
                }

                if (str[i] == '`' && !in_escape)
                {
                    var tritick = str.IndexOf("```", i) == i;
                    if (in_triple_backtick && tritick)
                    {
                        in_triple_backtick = false;
                        i += 2;
                    }
                    else if (!in_backtick && tritick)
                    {
                        in_triple_backtick = true;
                        i += 2;
                    }

                    if (in_backtick && !tritick)
                        in_backtick = false;
                    else if (!in_triple_backtick && tritick)
                        in_backtick = true;
                }

                if (str[i] == '"' && !in_escape && !in_backtick && !in_triple_backtick)
                {
                    //str = str.Remove(i, 1);
                    //i--;
                    remove.Add(i - sp);

                    if (!in_quote)
                        in_quote = true;
                    else
                        in_quote = false;
                }

                if (in_escape)
                    in_escape = false;

                if (ep != -1)
                {
                    startPos = ep;
                    if (sp != ep)
                        return str.Substring(sp, ep - sp).CleanupString(remove);
                    return null;
                }
            }

            startPos = str.Length;
            if (startPos != sp)
                return str.Substring(sp).CleanupString(remove);
            return null;
        }

        internal static string CleanupString(this string s, IList<int> indices)
        {
            if (!indices.Any())
                return s;

            var li = indices.Last();
            var ll = 1;
            for (var x = indices.Count - 2; x >= 0; x--)
            {
                if (li - indices[x] == ll)
                {
                    ll++;
                    continue;
                }

                s = s.Remove(li - ll + 1, ll);
                li = indices[x];
                ll = 1;
            }

            return s.Remove(li - ll + 1, ll);
        }

        internal static object[] BindArguments(CommandContext ctx, bool ignoreSurplus, out IReadOnlyList<string> rawArguments)
        {
            var cmd = ctx.Command;

            var args = new object[cmd.Arguments.Count + 1];
            args[0] = ctx;
            var argr = new List<string>(cmd.Arguments.Count);

            var argstr = ctx.RawArgumentString;
            var findpos = 0;
            var argv = "";
            for (var i = 0; i < ctx.Command.Arguments.Count; i++)
            {
                var arg = ctx.Command.Arguments[i];
                if (arg.IsCatchAll)
                {
                    if (arg._isArray)
                    {
                        while (true)
                        {
                            argv = ExtractNextArgument(argstr, ref findpos);
                            if (argv == null)
                                break;
                            
                            argr.Add(argv);
                        }

                        break;
                    }
                    else
                    {
                        if (argstr == null)
                            break;

                        argv = argstr.Substring(findpos).Trim();
                        argv = argv == "" ? null : argv;
                        findpos = argstr.Length;

                        argr.Add(argv);
                        break;
                    }
                }
                else
                {
                    argv = ExtractNextArgument(argstr, ref findpos);
                    argr.Add(argv);
                }

                if (argv == null && !arg.IsOptional && !arg.IsCatchAll)
                    throw new ArgumentException("Not enough arguments supplied to the command.");
                else if (argv == null)
                    argr.Add(null);
            }

            if (!ignoreSurplus && findpos < argstr.Length)
                throw new ArgumentException("Too many arguments were supplied to this command.");

            for (var i = 0; i < ctx.Command.Arguments.Count; i++)
            {
                var arg = ctx.Command.Arguments[i];
                if (arg.IsCatchAll && arg._isArray)
                {
                    var arr = Array.CreateInstance(arg.Type, argr.Count - i);
                    var start = i;
                    while (i < argr.Count)
                    {
                        arr.SetValue(ConvertArgument(argr[i], ctx, arg.Type), i - start);
                        i++;
                    }

                    args[start + 1] = arr;
                    break;
                }
                else
                {
                    args[i + 1] = argr[i] != null ? ConvertArgument(argr[i], ctx, arg.Type) : arg.DefaultValue;
                }
            }

            rawArguments = new ReadOnlyCollection<string>(argr);
            return args;
        }

        internal static bool IsModuleCandidateType(this Type type)
            => type.GetTypeInfo().IsModuleCandidateType();

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

            // qualifies if any method or type qualifies
            return ti.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _)) || ti.DeclaredNestedTypes.Any(xti => xti.IsModuleCandidateType());
        }

        internal static bool IsCommandCandidate(this MethodInfo method, out ParameterInfo[] parameters)
        {
            parameters = null;
            // check if exists
            if (method == null)
                return false;

            // check if static or non-public
            if (method.IsStatic || !method.IsPublic)
                return false;

            // check if appropriate return and arguments
            parameters = method.GetParameters();
            if (!parameters.Any() || parameters.First().ParameterType != typeof(CommandContext) || method.ReturnType != typeof(Task))
                return false;

            // qualifies
            return true;
        }
    }
}
