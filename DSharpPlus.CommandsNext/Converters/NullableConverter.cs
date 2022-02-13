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
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class NullableConverter<T> : IArgumentConverter<Nullable<T>> where T : struct
    {
        async Task<Optional<Nullable<T>>> IArgumentConverter<Nullable<T>>.ConvertAsync(string value, CommandContext ctx)
        {
            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (value == "null")
                return Optional.FromValue<Nullable<T>>(null);

            if (ctx.CommandsNext.ArgumentConverters.TryGetValue(typeof(T), out var cv))
            {
                var cvx = cv as IArgumentConverter<T>;
                var val = await cvx.ConvertAsync(value, ctx).ConfigureAwait(false);
                return val.HasValue ? Optional.FromValue<Nullable<T>>(val.Value) : Optional.FromNoValue<Nullable<T>>();
            }

            return Optional.FromNoValue<Nullable<T>>();
        }
    }
}
