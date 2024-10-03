// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace DSharpPlus.Serialization;

/// <summary>
/// Contains information about which library component uses which serialization format and how models
/// are bound to their definitions.
/// </summary>
public sealed class SerializationOptions
{
    public const string Json = "json";

    /// <summary>
    /// Well-known formats:
    /// <list type="number">
    ///     <item>"json", on top of System.Text.Json.</item>
    ///     <item>"etf", on top of ETFKit, not installed by default.</item>
    /// </list>
    /// </summary>
    internal Dictionary<Type, string> Formats { get; } = new(4)
    {
        [typeof(SystemTextJsonFormatMarker)] = "json"
    };

    internal Dictionary<Type, Type> InterfacesToConcrete { get; } = new(512);

    internal Dictionary<string, Type> BackendImplementations { get; } = new(2)
    {
        ["json"] = typeof(SystemTextJsonSerializationBackend)
    };

    public void RegisterBackendImplementation<T>()
        where T : ISerializationBackend
        => this.BackendImplementations[T.Id] = typeof(T);

    /// <summary>
    /// Specifies the concrete type used to deserialize an interface.
    /// </summary>
    public void AddModel<TInterface, TModel>()
        where TInterface : notnull
        where TModel : notnull, TInterface
        => this.InterfacesToConcrete[typeof(TInterface)] = typeof(TModel);

    /// <summary>
    /// Removes an interface to concrete type entry.
    /// </summary>
    public void RemoveModel<TInterface>()
        where TInterface : notnull
        => this.InterfacesToConcrete.Remove(typeof(TInterface));

    /// <summary>
    /// Specifies the format to use for a given library component.
    /// </summary>
    /// <typeparam name="TComponent">The interface associated with this component.</typeparam>
    public void SetFormat<TComponent>(string format = "json")
        => this.Formats[typeof(TComponent)] = format;

    /// <summary>
    /// Clears the format associated with the given library component.
    /// </summary>
    /// <typeparam name="TComponent">The interface associated with this component.</typeparam>
    public void ClearFormat<TComponent>()
        => this.Formats.Remove(typeof(TComponent));
}
