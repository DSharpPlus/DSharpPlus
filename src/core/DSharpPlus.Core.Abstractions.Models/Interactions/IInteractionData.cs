// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents metadata attached to an interaction.
/// </summary>
// This interface, by itself, does nothing. Type has to be resolved based on IInteraction.Type
// and casted appropriately to one of the derived interface types.
public interface IInteractionData;
