// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
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
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    /// <summary>
    /// Converts a string to an enum type.
    /// </summary>
    /// <typeparam name="T">Type of enum to convert.</typeparam>
    public class EnumConverter<T> : IArgumentConverter<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        Task<Optional<T>> IArgumentConverter<T>.ConvertAsync(string value, CommandContext ctx)
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!ti.IsEnum)
                throw new InvalidOperationException("Cannot convert non-enum value to an enum.");

            return Enum.TryParse(value, !ctx.Config.CaseSensitive, out T ev)
                ? Task.FromResult(Optional.FromValue(ev))
                : Task.FromResult(Optional.FromNoValue<T>());
        }
    }
}
