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

namespace DSharpPlus.SlashCommands.Attributes
{
    /// <summary>
    /// Intended to be used in conjunction on parameters marked with <see langword="params"/>, this attribute will limit the amount of elements in the array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ParameterLimitAttribute : Attribute
    {
        /// <summary>
        /// The minimum amount of elements required in the parameter array.
        /// </summary>
        public readonly int Min;

        /// <summary>
        /// The maximum amount of elements allowed in the parameter array.
        /// </summary>
        public readonly int Max;

        /// <summary>
        /// Intended to be used in conjunction on parameters marked with <see langword="params"/>, this attribute will limit the amount of elements in the array.
        /// </summary>
        /// <param name="min">The minimum amount of elements required in the parameter array.</param>
        /// <param name="max">The maximum amount of elements allowed in the parameter array.</param>
        public ParameterLimitAttribute(int min, int max)
        {
            if (min < 0 || min > 25)
                throw new ArgumentException("Minimum must be between 0 and 25 inclusive.", nameof(min));
            else if (max < 1 || max > 25)
                throw new ArgumentException("Maximum must be between 1 and 25 inclusive.", nameof(max));
            else if (min > max)
                throw new ArgumentException("Minimum cannot be greater than maximum.", nameof(min));

            this.Min = min;
            this.Max = max;
        }
    }
}
