// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.RuntimeServices.RuntimeFeatures;

/// <summary>
/// This attribute causes code to be trimmed or ignored at JIT time conditional on the feature switch
/// <c>DSharpPlus.DisableRestRequestLogging</c>.
/// </summary>
/// <inheritdoc cref="RestRequestLogging.IsEnabled"/>
[AttributeUsage(AttributeTargets.All, Inherited = true)]
public sealed class RequiresRestRequestLoggingAttribute : Attribute;
