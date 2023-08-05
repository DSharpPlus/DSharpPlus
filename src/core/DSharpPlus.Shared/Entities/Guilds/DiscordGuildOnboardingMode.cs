// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Entities;

/// <summary>
/// Defines the criteria used to satisfy Onboarding constraints required for enabling.
/// </summary>
[Flags]
public enum DiscordGuildOnboardingMode
{
    /// <summary>
    /// Counts only default channels towards constraints.
    /// </summary>
    OnboardingDefault,

    /// <summary>
    /// Counts default channels and questions towards constraints.
    /// </summary>
    OnboardingAdvanced
}
