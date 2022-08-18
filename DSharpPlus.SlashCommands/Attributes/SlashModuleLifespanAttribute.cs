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
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS0618 // Type or member is obsolete
namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Defines this slash command module's lifespan. Module lifespans are transient by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SlashModuleLifespanAttribute : Attribute
    {
        /// <summary>
        /// Gets the lifespan.
        /// </summary>
        public ServiceLifetime Lifespan { get; }

        /// <summary>
        /// Defines this slash command module's lifespan.
        /// </summary>
        /// <param name="lifespan">The lifespan of the module. Module lifespans are transient by default.</param>
        [Obsolete("Please use the " + nameof(ServiceLifetime) + " overload instead. The " + nameof(SlashModuleLifespan) + " overload will be removed by version 4.4.0.")]
        public SlashModuleLifespanAttribute(SlashModuleLifespan lifespan)
        {
            this.Lifespan = (ServiceLifetime)lifespan;
        }

        /// <summary>
        /// Defines this slash command module's lifespan.
        /// </summary>
        /// <param name="serviceLifetime">The lifespan of the module. Module lifespans are transient by default.</param>
        public SlashModuleLifespanAttribute(ServiceLifetime serviceLifetime)
        {
            this.Lifespan = serviceLifetime;
        }
    }

    /// <summary>
    /// Represents a slash command module lifespan.
    /// </summary>
    [Obsolete("Please use " + nameof(ServiceLifetime) + " instead. " + nameof(SlashModuleLifespan) + " will be removed by version 4.4.0.")]
    public enum SlashModuleLifespan
    {
        /// <summary>
        /// Whether this module should be initiated at startup.
        /// </summary>
        Singleton,

        /// <summary>
        /// Whether this module should be initiated every time a command is run, with dependencies injected from a scope.
        /// </summary>
        Scoped,

        /// <summary>
        /// Whether this module should be initiated every time a command is run.
        /// </summary>
        Transient
    }
}
