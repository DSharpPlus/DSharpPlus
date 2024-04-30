using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Defines various extension methods for slash commands.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Enables slash commands on this <see cref="DiscordClient"/>.
    /// </summary>
    /// <param name="client">Client to enable slash commands for.</param>
    /// <param name="config">Configuration to use.</param>
    /// <returns>Created <see cref="SlashCommandsExtension"/>.</returns>
    public static SlashCommandsExtension UseSlashCommands(this DiscordClient client, SlashCommandsConfiguration config = null)
    {
        if (client.GetExtension<SlashCommandsExtension>() != null)
        {
            throw new InvalidOperationException("Slash commands are already enabled for that client.");
        }

        SlashCommandsExtension scomm = new SlashCommandsExtension(config);
        client.AddExtension(scomm);
        return scomm;
    }

    /// <summary>
    /// Gets the slash commands module for this client.
    /// </summary>
    /// <param name="client">Client to get slash commands for.</param>
    /// <returns>The module, or null if not activated.</returns>
    public static SlashCommandsExtension GetSlashCommands(this DiscordClient client)
        => client.GetExtension<SlashCommandsExtension>();

    /// <summary>
    /// Enables slash commands on this <see cref="DiscordShardedClient"/>.
    /// </summary>
    /// <param name="client">Client to enable slash commands on.</param>
    /// <param name="config">Configuration to use.</param>
    /// <returns>A dictionary of created <see cref="SlashCommandsExtension"/> with the key being the shard id.</returns>
    public static async Task<IReadOnlyDictionary<int, SlashCommandsExtension>> UseSlashCommandsAsync(this DiscordShardedClient client, SlashCommandsConfiguration config = null)
    {
        Dictionary<int, SlashCommandsExtension> modules = new Dictionary<int, SlashCommandsExtension>();
        await client.InitializeShardsAsync();
        foreach (DiscordClient shard in client.ShardClients.Values)
        {
            SlashCommandsExtension? scomm = shard.GetSlashCommands() ?? shard.UseSlashCommands(config);
            modules[shard.ShardId] = scomm;
        }

        return modules;
    }

    /// <summary>
    /// Registers a commands class.
    /// </summary>
    /// <typeparam name="T">The command class to register.</typeparam>
    /// <param name="modules">The modules to register it on.</param>
    /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
    public static void RegisterCommands<T>(this IReadOnlyDictionary<int, SlashCommandsExtension> modules, ulong? guildId = null) where T : ApplicationCommandModule
    {
        foreach (SlashCommandsExtension module in modules.Values)
        {
            module.RegisterCommands<T>(guildId);
        }
    }

    /// <summary>
    /// Registers a command class.
    /// </summary>
    /// <param name="modules">The modules to register it on.</param>
    /// <param name="type">The <see cref="Type"/> of the command class to register.</param>
    /// <param name="guildId">The guild id to register it on. If you want global commands, leave it null.</param>
    public static void RegisterCommands(this IReadOnlyDictionary<int, SlashCommandsExtension> modules, Type type, ulong? guildId = null)
    {
        foreach (SlashCommandsExtension module in modules.Values)
        {
            module.RegisterCommands(type, guildId);
        }
    }

    /// <summary>
    /// Gets the name from the <see cref="ChoiceNameAttribute"/> for this enum value.
    /// </summary>
    /// <returns>The name.</returns>
    public static string GetName<T>(this T e) where T : IConvertible
    {
        if (e is Enum)
        {
            Type type = e.GetType();
            Array values = Enum.GetValues(type);

            foreach (int val in values)
            {
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    System.Reflection.MemberInfo[] memInfo = type.GetMember(type.GetEnumName(val));

                    return memInfo[0].GetCustomAttributes(typeof(ChoiceNameAttribute), false).FirstOrDefault() is ChoiceNameAttribute nameAttribute
                        ? nameAttribute.Name
                        : type.GetEnumName(val);
                }
            }
        }
        return null;
    }
}
