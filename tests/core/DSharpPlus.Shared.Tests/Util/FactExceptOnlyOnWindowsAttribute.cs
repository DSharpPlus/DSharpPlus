// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using Xunit;

namespace DSharpPlus.Shared.Tests.Util;

internal sealed class FactExceptOnlyOnWindowsAttribute : FactAttribute
{
    public FactExceptOnlyOnWindowsAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Ignored on non-Windows platforms.";
        }
    }
}
