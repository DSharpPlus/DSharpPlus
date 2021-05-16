using System;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Sets a IChoiceProvider for a command options. ChoiceProviders can be used to provide
    /// DiscordApplicationCommandOptionChoice from external sources such as a database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ChoiceProviderAttribute : Attribute
    {

        public Type ProviderType { get; }
        
        public ChoiceProviderAttribute(Type providerType)
        {
            ProviderType = providerType;
        }
    }
}