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
        /// <param name="comparisonType">Method of string comparison for the purposes of finding prefixes.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int GetStringPrefixLength(this DiscordMessage msg, string str, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var content = msg.Content;
            if (str.Length >= content.Length)
                return -1;

            return !content.StartsWith(str, comparisonType) ? -1 : str.Length;
        }

        /// <summary>
        /// Checks whether the message contains a specified mention prefix.
        /// </summary>
        /// <param name="msg">Message to check.</param>
        /// <param name="user">User to check for.</param>
        /// <returns>Positive number if the prefix is present, -1 otherwise.</returns>
        public static int GetMentionPrefixLength(this DiscordMessage msg, DiscordUser user)
        {
            var content = msg.Content;
            if (!content.StartsWith("<@"))
                return -1;

            var cni = content.IndexOf('>');
            if (cni == -1 || content.Length <= cni + 2)
                return -1;

            var cnp = content.Substring(0, cni + 2);
            var m = UserRegex.Match(cnp);
            if (!m.Success)
                return -1;

            var userId = ulong.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            return user.Id != userId ? -1 : m.Value.Length;
        }

        //internal static string ExtractNextArgument(string str, out string remainder)
        internal static string? ExtractNextArgument(this string str, ref int startPos, IEnumerable<char> _quoteChars)
        {
            if (string.IsNullOrWhiteSpace(str))
                return null;

            var inBacktick = false;
            var inTripleBacktick = false;
            var inQuote = false;
            var inEscape = false;
            var removeIndices = new List<int>(str.Length - startPos);

            var i = startPos;
            for (; i < str.Length; i++)
                if (!char.IsWhiteSpace(str[i]))
                    break;
            startPos = i;

            var endPosition = -1;
            var startPosition = startPos;
            for (i = startPosition; i < str.Length; i++)
            {
                if (char.IsWhiteSpace(str[i]) && !inQuote && !inTripleBacktick && !inBacktick && !inEscape)
                    endPosition = i;

                if (str[i] == '\\' && str.Length > i + 1)
                {
                    if (!inEscape && !inBacktick && !inTripleBacktick)
                    {
                        inEscape = true;
                        if (str.IndexOf("\\`", i) == i || _quoteChars.Any(c => str.IndexOf($"\\{c}", i) == i) || str.IndexOf("\\\\", i) == i || (str.Length >= i && char.IsWhiteSpace(str[i + 1])))
                            removeIndices.Add(i - startPosition);
                        i++;
                    }
                    else if ((inBacktick || inTripleBacktick) && str.IndexOf("\\`", i) == i)
                    {
                        inEscape = true;
                        removeIndices.Add(i - startPosition);
                        i++;
                    }
                }

                if (str[i] == '`' && !inEscape)
                {
                    var tripleBacktick = str.IndexOf("```", i) == i;
                    if (inTripleBacktick && tripleBacktick)
                    {
                        inTripleBacktick = false;
                        i += 2;
                    }
                    else if (!inBacktick && tripleBacktick)
                    {
                        inTripleBacktick = true;
                        i += 2;
                    }

                    if (inBacktick && !tripleBacktick)
                        inBacktick = false;
                    else if (!inTripleBacktick && tripleBacktick)
                        inBacktick = true;
                }

                if (_quoteChars.Contains(str[i]) && !inEscape && !inBacktick && !inTripleBacktick)
                {
                    removeIndices.Add(i - startPosition);

                    inQuote = !inQuote;
                }

                if (inEscape)
                    inEscape = false;

                if (endPosition != -1)
                {
                    startPos = endPosition;
                    return startPosition != endPosition ? str.Substring(startPosition, endPosition - startPosition).CleanupString(removeIndices) : null;
                }
            }

            startPos = str.Length;
            return startPos != startPosition ? str.Substring(startPosition).CleanupString(removeIndices) : null;
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

        internal static async Task<ArgumentBindingResult> BindArgumentsAsync(CommandContext ctx, bool ignoreSurplus)
        {
            var command = ctx.Command;
            var overload = ctx.Overload;

            var args = new object?[overload.Arguments.Count + 2];
            args[1] = ctx;
            var rawArgumentList = new List<string?>(overload.Arguments.Count);
            var argString = ctx.RawArgumentString;
            var foundAt = 0;

            for (var i = 0; i < overload.Arguments.Count; i++)
            {
                var arg = overload.Arguments[i];
                var argValue = string.Empty;
                if (arg.IsCatchAll)
                {
                    if (arg._isArray)
                    {
                        while (true)
                        {
                            argValue = ExtractNextArgument(argString, ref foundAt, ctx.Config.QuotationMarks);
                            if (argValue == null)
                                break;

                            rawArgumentList.Add(argValue);
                        }

                        break;
                    }
                    else
                    {
                        if (argString == null)
                            break;

                        argValue = argString.Substring(foundAt).Trim();
                        argValue = argValue == "" ? null : argValue;
                        foundAt = argString.Length;

                        rawArgumentList.Add(argValue);
                        break;
                    }
                }
                else
                {
                    argValue = ExtractNextArgument(argString, ref foundAt, ctx.Config.QuotationMarks);
                    rawArgumentList.Add(argValue);
                }

                if (argValue == null && !arg.IsOptional && !arg.IsCatchAll)
                    return new ArgumentBindingResult(new ArgumentException("Not enough arguments supplied to the command."));
                else if (argValue == null)
                    rawArgumentList.Add(null);
            }

            if (!ignoreSurplus && foundAt < (argString?.Length ?? 0))
                return new ArgumentBindingResult(new ArgumentException("Too many arguments were supplied to this command."));

            for (var i = 0; i < overload.Arguments.Count; i++)
            {
                var arg = overload.Arguments[i];
                if (arg.IsCatchAll && arg._isArray)
                {
                    var array = Array.CreateInstance(arg.Type, rawArgumentList.Count - i);
                    var start = i;
                    while (i < rawArgumentList.Count)
                    {
                        try
                        {
                            array.SetValue(await ctx.CommandsNext.ConvertArgument(rawArgumentList[i], ctx, arg.Type).ConfigureAwait(false), i - start);
                        }
                        catch (Exception ex)
                        {
                            return new ArgumentBindingResult(ex);
                        }
                        i++;
                    }

                    args[start + 2] = array;
                    break;
                }
                else
                {
                    try
                    {
                        args[i + 2] = rawArgumentList[i] != null ? await ctx.CommandsNext.ConvertArgument(rawArgumentList[i], ctx, arg.Type).ConfigureAwait(false) : arg.DefaultValue;
                    }
                    catch (Exception ex)
                    {
                        return new ArgumentBindingResult(ex);
                    }
                }
            }

            return new ArgumentBindingResult(args, rawArgumentList.Where(x => x is not null).OfType<string>().ToArray());
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
            var tdelegate = typeof(Delegate).GetTypeInfo();
            if (tdelegate.IsAssignableFrom(ti))
                return false;

            // qualifies if any method or type qualifies
            return ti.DeclaredMethods.Any(xmi => xmi.IsCommandCandidate(out _)) || ti.DeclaredNestedTypes.Any(xti => xti.IsModuleCandidateType());
        }

        internal static bool IsCommandCandidate(this MethodInfo method, out ParameterInfo[] parameters)
        {
            parameters = Array.Empty<ParameterInfo>();

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
            var constructors = ti.DeclaredConstructors
                .Where(xci => xci.IsPublic)
                .ToArray();

            if (constructors.Length != 1)
                throw new ArgumentException("Specified type does not contain a public constructor or contains more than one public constructor.");

            var constructor = constructors[0];
            var constructorArgs = constructor.GetParameters();
            var args = new object[constructorArgs.Length];

            if (constructorArgs.Length != 0 && services == null)
                throw new InvalidOperationException("Dependency collection needs to be specified for parameterized constructors.");

            // inject via constructor
            if (constructorArgs.Length != 0)
                for (var i = 0; i < args.Length; i++)
                    args[i] = services.GetRequiredService(constructorArgs[i].ParameterType);

            var moduleInstance = Activator.CreateInstance(t, args);

            // inject into properties
            var props = t.GetRuntimeProperties().Where(xp => xp.CanWrite && xp.SetMethod != null && !xp.SetMethod.IsStatic && xp.SetMethod.IsPublic);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = services.GetService(prop.PropertyType);
                if (service == null)
                    continue;

                prop.SetValue(moduleInstance, service);
            }

            // inject into fields
            var fields = t.GetRuntimeFields().Where(xf => !xf.IsInitOnly && !xf.IsStatic && xf.IsPublic);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<DontInjectAttribute>() != null)
                    continue;

                var service = services.GetService(field.FieldType);
                if (service == null)
                    continue;

                field.SetValue(moduleInstance, service);
            }

            return moduleInstance;
        }
    }
}
