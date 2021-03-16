using System;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Role that will be created.
    /// </summary>
    public sealed class RoleCreateBuilder : Abstractions.RoleBuilder<RoleCreateBuilder>
    {
        /// <summary>
        /// Creates a role utilizing what was specified to the builder.
        /// </summary>
        /// <param name="guild">The guild to create the channel in.</param>
        /// <returns></returns>
        public async Task<DiscordRole> CreateAsync(DiscordGuild guild)
        {
            return await guild.CreateRoleAsync(this).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
                throw new ArgumentException("Name is required to be provided.");
        }
    }

    /// <summary>
    /// Represents the modifications to a role.
    /// </summary>
    public sealed class RoleModifyBuilder : Abstractions.RoleBuilder<RoleModifyBuilder>
    {
        /// <summary>
        /// Modify a role utilizing what was specified to the builder.
        /// </summary>
        /// <param name="role">The role to modift.</param>
        /// <returns></returns>
        public async Task ModifyAsync(DiscordRole role)
        {
            await role.ModifyAsync(this).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override void Validate()
        {
            
        }
    }
}
