using System;
namespace DSharpPlus.SlashCommands
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SlashCommandPermissionsAttribute : Attribute
    {
        public Permissions Permissions { get; }

        public SlashCommandPermissionsAttribute(Permissions permissions)
        {
            this.Permissions = permissions;
        }
    }
}
