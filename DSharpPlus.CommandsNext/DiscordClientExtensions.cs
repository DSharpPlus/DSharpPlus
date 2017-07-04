using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DSharpPlus.CommandsNext.Attributes;
using cn = DSharpPlus.CommandsNext;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Defines various extensions specific to CommandsNext.
    /// </summary>
    public static class DiscordClientExtensions
    {
        private static Dictionary<cn.Permission, string> PermissionStrings { get; set; }
        
        static DiscordClientExtensions()
        {
            PermissionStrings = new Dictionary<cn.Permission, string>();
            var t = typeof(cn.Permission);
            var ti = t.GetTypeInfo();
            var vs = Enum.GetValues(t).Cast<cn.Permission>();

            foreach (var xv in vs)
            {
                var xsv = xv.ToString();
                var xmv = ti.DeclaredMembers.FirstOrDefault(xm => xm.Name == xsv);
                var xav = xmv.GetCustomAttribute<PermissionStringAttribute>();

                PermissionStrings.Add(xv, xav.String);
            }
        }

        /// <summary>
        /// Enables CommandsNext module on this <see cref="DiscordClient"/>.
        /// </summary>
        /// <param name="client">Client to enable CommandsNext for.</param>
        /// <param name="cfg">CommandsNext configuration to use.</param>
        /// <returns>Created <see cref="CommandsNextModule"/>.</returns>
        public static CommandsNextModule UseCommandsNext(this DiscordClient client, CommandsNextConfiguration cfg)
        {
            if (client.GetModule<CommandsNextModule>() != null)
                throw new InvalidOperationException("CommandsNext is already enabled for that client.");

            var cnext = new CommandsNextModule(cfg);
            client.AddModule(cnext);
            return cnext;
        }

        /// <summary>
        /// Enables CommandsNext module on all shards in this <see cref="DiscordShardedClient"/>.
        /// </summary>
        /// <param name="client">Client to enable CommandsNext for.</param>
        /// <param name="cfg">CommandsNext configuration to use.</param>
        /// <returns>A dictionary of created <see cref="CommandsNextModule"/>, indexed by shard id..</returns>
        public static IReadOnlyDictionary<int, CommandsNextModule> UseCommandsNext(this DiscordShardedClient client, CommandsNextConfiguration cfg)
        {
            var modules = new Dictionary<int, CommandsNextModule>();

            client.InitializeShardsAsync().GetAwaiter().GetResult();

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
            {
                var cnext = shard.GetModule<CommandsNextModule>();
                if (cnext == null)
                    cnext = shard.UseCommandsNext(cfg);

                modules.Add(shard.ShardId, cnext);
            }

            return new ReadOnlyDictionary<int, CommandsNextModule>(modules);
        }

        /// <summary>
        /// Gets the active CommandsNext module for this client.
        /// </summary>
        /// <param name="client">Client to get CommandsNext module from.</param>
        /// <returns>The module, or null if not activated.</returns>
        public static CommandsNextModule GetCommandsNext(this DiscordClient client)
        {
            return client.GetModule<CommandsNextModule>();
        }

        /// <summary>
        /// Gets the active CommandsNext modules for all shards in this client.
        /// </summary>
        /// <param name="client">Client to get CommandsNext instances from.</param>
        /// <returns>A dictionary of the modules, indexed by shard id.</returns>
        public static IReadOnlyDictionary<int, CommandsNextModule> GetCommandsNext(this DiscordShardedClient client)
        {
            var modules = new Dictionary<int, CommandsNextModule>();

            client.InitializeShardsAsync().GetAwaiter().GetResult();

            foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
                modules.Add(shard.ShardId, shard.GetModule<CommandsNextModule>());

            return new ReadOnlyDictionary<int, CommandsNextModule>(modules);
        }

        /// <summary>
        /// Converts this <see cref="cn.Permission"/> into human-readable format.
        /// </summary>
        /// <param name="perm">Permission enumeration to convert.</param>
        /// <returns>Human-readable permissions.</returns>
        public static string GetPermissionString(this cn.Permission perm)
        {
            if (perm == cn.Permission.None)
                return PermissionStrings[perm];

            var strs = PermissionStrings
                .Where(xkvp => xkvp.Key != cn.Permission.None && (perm & xkvp.Key) == xkvp.Key)
                .Select(xkvp => xkvp.Value);

            return string.Join(", ", strs);
        }
    }
}
