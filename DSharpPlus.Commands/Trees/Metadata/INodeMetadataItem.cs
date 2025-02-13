//
// SPDX-FileCopyrightText: Copyright (c) 2024 DPlayer234/Vamplay
// SPDX-License-Identifier: MIT
//
// This logic is taken from DPlayer234/Vamplay's command handling infrastructure licensed under MIT:
// https://github.com/DPlayer234/celestia-lib/blob/9747011b63a168b11103988b16aed111572df113/src/CelestiaCS.Lib.Services/Commands/INodeAttribute.cs
//

namespace DSharpPlus.Commands.Trees.Metadata;

/// <summary>
/// Represents a metadata item for a command or parameter node.
/// </summary>
public interface INodeMetadataItem
{
    /// <summary>
    /// Gets whether this metadata item spreads to child nodes on build.
    /// </summary>
    /// <remarks>
    /// Regardless of the value, metadata will not spread to <see cref="ParameterNode"/>s.
    /// </remarks>
    public bool SpreadsToChildren => false;
}
