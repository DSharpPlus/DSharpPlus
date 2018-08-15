using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Various CommandsNext-related utilities.
    /// </summary>
    public static class CommandsNextUtilities
    {
        private static Regex UserRegex { get; } = new Regex(@"<@\!?(\d+?)> ", RegexOptions.ECMAScript);

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

            return m.Value.Length;
        }

        //internal static string ExtractNextArgument(string str, out string remainder)
        internal static string ExtractNextArgument(this string str, ref int startPos)
        {
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
                            remove.Add(i - sp);
                        i++;
                    }
                    else if ((in_backtick || in_triple_backtick) && str.IndexOf("\\`", i) == i)
                    {
                        in_escape = true;
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

        internal static async Task<ArgumentBindingResult> BindArguments(CommandContext ctx, bool ignoreSurplus)
        {
            var cmd = ctx.Command;
            var ovl = ctx.Overload;

            var args = new object[ovl.Arguments.Count + 2];
            args[1] = ctx;
            var argr = new List<string>(ovl.Arguments.Count);

            var argstr = ctx.RawArgumentString;
            var findpos = 0;
            var argv = "";
            for (var i = 0; i < ovl.Arguments.Count; i++)
            {
                var arg = ovl.Arguments[i];
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
                    return new ArgumentBindingResult(new ArgumentException("Not enough arguments supplied to the command."));
                else if (argv == null)
                    argr.Add(null);
            }

            if (!ignoreSurplus && findpos < argstr.Length)
                return new ArgumentBindingResult(new ArgumentException("Too many arguments were supplied to this command."));

            for (var i = 0; i < ovl.Arguments.Count; i++)
            {
                var arg = ovl.Arguments[i];
                if (arg.IsCatchAll && arg._isArray)
                {
                    var arr = Array.CreateInstance(arg.Type, argr.Count - i);
                    var start = i;
                    while (i < argr.Count)
                    {
                        try
                        {
                            arr.SetValue(await ctx.CommandsNext.ConvertArgument(argr[i], ctx, arg.Type).ConfigureAwait(false), i - start);
                        }
                        catch (Exception ex)
                        {
                            return new ArgumentBindingResult(ex);
                        }
                        i++;
                    }

                    args[start + 2] = arr;
                    break;
                }
                else
                {
                    try
                    { 
                        args[i + 2] = argr[i] != null ? await ctx.CommandsNext.ConvertArgument(argr[i], ctx, arg.Type).ConfigureAwait(false) : arg.DefaultValue;
                    }
                    catch (Exception ex)
                    {
                        return new ArgumentBindingResult(ex);
                    }
                }
            }

            return new ArgumentBindingResult(args, argr);
        }

        internal static bool IsModuleCandidateType(this Type type)
            => type.GetTypeInfo().IsModuleCandidateType();

        internal static bool IsModuleCandidateType(this TypeInfo ti)
        {
            // check if compiler-generated
            if (ti.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null)
                return false;

            // check if derives from the required base class
            var tmodule = typeof(BaseCommandModule);
            var timodule = tmodule.GetTypeInfo();
            if (!timodule.IsAssignableFrom(ti))
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

            // check if static, non-public, abstract, a constructor, or a special name
            if (method.IsStatic || method.IsAbstract || method.IsConstructor || method.IsSpecialName)
                return false;

            // check if appropriate return and arguments
            parameters = method.GetParameters();
            if (!parameters.Any() || parameters.First().ParameterType != typeof(CommandContext) || method.ReturnType != typeof(Task))
                return false;

            // qualifies
            return true;
        }

        internal static object CreateInstance(this Type t, IServiceProvider services)
        {
            var ti = t.GetTypeInfo();
            var cs = ti.DeclaredConstructors
                .Where(xci => xci.IsPublic)
                .ToArray();

            if (cs.Length != 1)
                throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");

            var constr = cs[0];
            var prms = constr.GetParameters();
            var args = new object[prms.Length];

            if (prms.Length != 0 && services == null)
                throw new InvalidOperationException("Dependency collection needs to be specified for parametered constructors.");

            // inject via constructor
            if (prms.Length != 0)
                for (var i = 0; i < args.Length; i++)
                    args[i] = services.GetRequiredService(prms[i].ParameterType);

            var module = Activator.CreateInstance(t, args);

            // inject into properties
            var props = ti.DeclaredProperties.Where(xp => xp.CanWrite && xp.SetMethod != null && !xp.SetMethod.IsStatic && xp.SetMethod.IsPublic);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var srv = services.GetService(prop.PropertyType);
                if (srv == null)
                    continue;

                prop.SetValue(module, srv);
            }

            // inject into fields
            var fields = ti.DeclaredFields.Where(xf => !xf.IsInitOnly && !xf.IsStatic && xf.IsPublic);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var srv = services.GetService(field.FieldType);
                if (srv == null)
                    continue;

                field.SetValue(module, srv);
            }

            return module;
        }
    }
}
