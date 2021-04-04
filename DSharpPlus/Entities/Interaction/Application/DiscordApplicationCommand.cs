using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a command that is registered to an application.
    /// </summary>
    public sealed class DiscordApplicationCommand : SnowflakeObject, IEquatable<DiscordApplicationCommand>
    {
        /// <summary>
        /// Gets the unique ID of this command's application.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }

        /// <summary>
        /// Gets the name of this command.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the description of this command.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the potential parameters for this command.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOption> Options { get; internal set; }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordApplicationCommand"/>.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="options">Optional parameters for this command.</param>
        public DiscordApplicationCommand(string name, string description, IEnumerable<DiscordApplicationCommandOption> options = null)
        {
            if(name.Length > 32)
                throw new ArgumentException("Slash command name cannot exceed 32 characters.");
            if (description.Length > 100)
                throw new ArgumentException("Slash command description cannot exceed 100 characters.");

            var optionsList = options != null ? new ReadOnlyCollection<DiscordApplicationCommandOption>(options.ToList()) : null;

            this.Name = name;
            this.Description = description;
            this.Options = optionsList;
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplicationCommand"/> object is equal to another object.
        /// </summary>
        /// <param name="other">The command to compare to.</param>
        /// <returns>Whether the command is equal to this <see cref="DiscordApplicationCommand"/>.</returns>
        public bool Equals(DiscordApplicationCommand other)
            => this.Id == other.Id;

        /// <summary>
        /// Determines if two <see cref="DiscordApplicationCommand"/> objects are equal. 
        /// </summary>
        /// <param name="e1">The first command object.</param>
        /// <param name="e2">The second command object.</param>
        /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are equal.</returns>
        public static bool operator ==(DiscordApplicationCommand e1, DiscordApplicationCommand e2)
            => e1.Equals(e2);

        /// <summary>
        /// Determines if two <see cref="DiscordApplicationCommand"/> objects are not equal. 
        /// </summary>
        /// <param name="e1">The first command object.</param>
        /// <param name="e2">The second command object.</param>
        /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are not equal.</returns>
        public static bool operator !=(DiscordApplicationCommand e1, DiscordApplicationCommand e2)
            => !(e1 == e2);

        /// <summary>
        /// Determines if a <see cref="object"/> is equal to the current <see cref="DiscordApplicationCommand"/>.
        /// </summary>
        /// <param name="other">The object to compare to.</param>
        /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are not equal.</returns>
        public override bool Equals(object other)
        {
            if (other is DiscordApplicationCommand dac)
                return this.Equals(dac);

            return false;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordApplicationCommand"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordApplicationCommand"/>.</returns>
        public override int GetHashCode()
            => this.Id.GetHashCode();
    }
}
