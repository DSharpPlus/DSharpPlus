using System;
using System.Diagnostics;

namespace DSharpPlus.Core.Attributes
{
    /// <summary>
    /// Associates a payload with one or more gateway events.
    /// </summary>
    [DebuggerDisplay("Gateway Payloads: {GetDebuggerDisplay()}")]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DiscordGatewayPayloadAttribute : Attribute
    {
        /// <summary>
        /// The payloads that this record is associated with. Names should be in SCREAMING_SNAKE_CASE. See https://discord.com/developers/docs/topics/gateway#commands-and-events for possible values.
        /// </summary>
        public string[] Names { get; }

        /// <summary>
        /// The payloads that this record is associated with. Names should be in SCREAMING_SNAKE_CASE. See https://discord.com/developers/docs/topics/gateway#commands-and-events for possible values.
        /// </summary>
        /// <param name="names">The gateway event names to associate the record with.</param>
        public DiscordGatewayPayloadAttribute(params string[] names) => Names = names;

        /// <summary>
        /// Returns the names of the gateway payloads associated with this record. Not truly required, but nice to have when debugging.
        /// </summary>
        /// <returns>A `, ` delimited string of associated gateway event names.</returns>
        private string GetDebuggerDisplay() => string.Join(", ", Names);
    }
}
