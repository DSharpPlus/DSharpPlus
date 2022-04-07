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

#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable CS8019 // Unnecessary using directive.
global using static DSharpPlus.Test.DSharpTestHelpers;
using System;
#pragma warning restore CS8019 // Unnecessary using directive.
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace DSharpPlus.Test
{
    public static class DSharpTestHelpers
    {
        /// <summary>
        /// Removes an element from an array using a zero based index.
        /// </summary>
        /// <param name="array">The array to modify.</param>
        /// <param name="index">The zero based index to remove.</param>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <returns>An array without the removed element.</returns>
        public static T[] RemoveAt<T>(this T[] array, int index)
        {
            T[] result = new T[array.Length - 1];
            if (index > 0)
            {
                Array.Copy(array, 0, result, 0, index);
            }

            if (index < array.Length - 1)
            {
                Array.Copy(array, index + 1, result, index, array.Length - index - 1);
            }

            return result;
        }
    }
}
