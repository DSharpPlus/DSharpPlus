// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus.Runtime.RuntimeFeatures;

/// <summary>
/// Contains the runtime feature switch for logging rest request data.
/// </summary>
public static class RestRequestLogging
{
    /// <summary>
    /// Indicates to the library whether rest request contents should be logged. This feature switch can be controlled
    /// from csproj files, where it will enable trimming the relevant code, and from runtimeconfig.json.
    /// </summary>
    /// <remarks>
    /// Enabling this switch will have catastrophic consequences for debugging issues related to rest requests and should
    /// only ever be considered if there is concrete evidence to back it up. <br/><br/>
    /// This switch, if enabled in the project file, will enable trimming all related code away: <br/>
    /// <code>
    /// <![CDATA[<RuntimeHostConfigurationOption Include="DSharpPlus.DisableRestRequestLogging" Value="false" Trim="true" />]]>
    /// </code>
    /// </remarks>
    [FeatureSwitchDefinition("DSharpPlus.DisableRestRequestLogging")]
    [FeatureGuard(typeof(RequiresRestRequestLoggingAttribute))]
    public static bool IsEnabled
        => !AppContext.TryGetSwitch("DSharpPlus.DisableRestRequestLogging", out bool enabled) || !enabled;
}
