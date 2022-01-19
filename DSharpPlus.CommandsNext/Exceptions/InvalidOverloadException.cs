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
using System.Reflection;

namespace DSharpPlus.CommandsNext.Exceptions
{
    /// <summary>
    /// Thrown when the command service fails to build a command due to a problem with its overload.
    /// </summary>
    public sealed class InvalidOverloadException : Exception
    {
        /// <summary>
        /// Gets the method that caused this exception.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets or sets the argument that caused the problem. This can be null.
        /// </summary>
        public ParameterInfo Parameter { get; }

        /// <summary>
        /// Creates a new <see cref="InvalidOverloadException"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="method">Method that caused the problem.</param>
        /// <param name="parameter">Method argument that caused the problem.</param>
        public InvalidOverloadException(string message, MethodInfo method, ParameterInfo parameter)
            : base(message)
        {
            this.Method = method;
            this.Parameter = parameter;
        }

        /// <summary>
        /// Creates a new <see cref="InvalidOverloadException"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="method">Method that caused the problem.</param>
        public InvalidOverloadException(string message, MethodInfo method)
            : this(message, method, null)
        { }

        /// <summary>
        /// Returns a string representation of this <see cref="InvalidOverloadException"/>.
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            // much like System.ArgumentNullException works
            return this.Parameter == null
                ? $"{this.GetType()}: {this.Message}\nMethod: {this.Method} (declared in {this.Method.DeclaringType})"
                : $"{this.GetType()}: {this.Message}\nMethod: {this.Method} (declared in {this.Method.DeclaringType})\nArgument: {this.Parameter.ParameterType} {this.Parameter.Name}";
        }
    }
}
