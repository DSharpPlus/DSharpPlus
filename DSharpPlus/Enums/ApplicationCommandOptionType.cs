using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus
{
    /// <summary>
    /// Represents the type of parameter when invoking an interaction.
    /// </summary>
    public enum ApplicationCommandOptionType
    {

        SubCommand = 1,
        SubCommandGroup,
        String,
        Integer, 
        Boolean,
        User,
        Channel,
        Role
    }
}
