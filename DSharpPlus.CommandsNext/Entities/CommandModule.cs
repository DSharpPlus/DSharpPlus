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

namespace DSharpPlus.CommandsNext.Entities
{
    /// <summary>
    /// Represents a base interface for all types of command modules.
    /// </summary>
    public interface ICommandModule
    {
        /// <summary>
        /// Gets the type of this module.
        /// </summary>
        Type ModuleType { get; }

        /// <summary>
        /// Returns an instance of this module.
        /// </summary>
        /// <param name="services">Services to instantiate the module with.</param>
        /// <returns>A created instance of this module.</returns>
        BaseCommandModule GetInstance(IServiceProvider services);
    }

    /// <summary>
    /// Represents a transient command module. This type of module is reinstated on every command call.
    /// </summary>
    public class TransientCommandModule : ICommandModule
    {
        /// <summary>
        /// Gets the type of this module.
        /// </summary>
        public Type ModuleType { get; }

        /// <summary>
        /// Creates a new transient module.
        /// </summary>
        /// <param name="t">Type of the module to create.</param>
        internal TransientCommandModule(Type t)
        {
            this.ModuleType = t;
        }

        /// <summary>
        /// Creates a new instance of this module.
        /// </summary>
        /// <param name="services">Services to instantiate the module with.</param>
        /// <returns>Created module.</returns>
        public BaseCommandModule GetInstance(IServiceProvider services) => (BaseCommandModule)this.ModuleType.CreateInstance(services);
    }

    /// <summary>
    /// Represents a singleton command module. This type of module is instantiated only when created.
    /// </summary>
    public class SingletonCommandModule : ICommandModule
    {
        /// <summary>
        /// Gets the type of this module.
        /// </summary>
        public Type ModuleType { get; }

        /// <summary>
        /// Gets this module's instance.
        /// </summary>
        public BaseCommandModule Instance { get; }

        /// <summary>
        /// Creates a new singleton module, and instantiates it.
        /// </summary>
        /// <param name="t">Type of the module to create.</param>
        /// <param name="services">Services to instantiate the module with.</param>
        internal SingletonCommandModule(Type t, IServiceProvider services)
        {
            this.ModuleType = t;
            this.Instance = (BaseCommandModule)t.CreateInstance(services);
        }

        /// <summary>
        /// Returns the instance of this module.
        /// </summary>
        /// <param name="services">Services to instantiate the module with.</param>
        /// <returns>This module's instance.</returns>
        public BaseCommandModule GetInstance(IServiceProvider services) => this.Instance;
    }
}
