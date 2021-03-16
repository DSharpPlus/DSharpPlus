using System;

namespace DSharpPlus.Entities.Abstractions
{
    /// <summary>
    /// Represents a Role that will be Created or Modified
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RoleBuilder<T>
    {
        /// <summary>
        /// <para>Gets or Sets the Name of the role to be sent.</para>
        /// <para>This must be between 2 and 100 Characters and is Required for Guild Creation.</para>
        /// </summary>
        public string Name
        {
            get => this._name;
            set
            {
                if (value != null && value.Length <= 2 && value.Length >= 100)
                    throw new ArgumentException("Name must be between 2 and 100 Characters.", nameof(value));
                this._name = value;
            }
        }
        private string _name;

        /// <summary>
        /// Gets the Permissions of the role
        /// </summary>
        public Optional<Permissions> Permissions { get; internal set; }

        /// <summary>
        /// Gets the color of the role
        /// </summary>
        public Optional<DiscordColor> Color { get; internal set; }

        /// <summary>
        /// Gets whether the role should be hoisted
        /// </summary>
        public Optional<bool> Hoist { get; internal set; }

        /// <summary>
        /// Gets whether the role should be mentionable
        /// </summary>
        public Optional<bool> Mentionable { get; internal set; }

        /// <summary>
        /// Gets the AuditLog Reason for modifing the channel.
        /// </summary>
        public Optional<string> AuditLogReason { get; internal set; }

        /// <summary>
        /// Sets the name of the role.
        /// </summary>
        /// <param name="name">The name to give.</param>
        /// <returns></returns>
        public T WithName(string name)
        {
            this.Name = name;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Permissions of the role.
        /// </summary>
        /// <param name="permissions">The permissions to give.</param>
        /// <returns></returns>
        public T WithPermissions(Permissions permissions)
        {
            this.Permissions = permissions;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the color of the role.
        /// </summary>
        /// <param name="color">The color to give.</param>
        /// <returns></returns>
        public T WithColor(DiscordColor color)
        {
            this.Color = color;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets if the role is hoisted.
        /// </summary>
        /// <param name="hoist">The hoist value.</param>
        /// <returns></returns>
        public T WithHoist(bool hoist)
        {
            this.Hoist = hoist;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets if the role is mentionable.
        /// </summary>
        /// <param name="mentionable">The mentionable value.</param>
        /// <returns></returns>
        public T WithMentionable(bool mentionable)
        {
            this.Mentionable = mentionable;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the reason for the Change.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public T WithAuditLogReason(string reason)
        {
            this.AuditLogReason = reason;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Clears all the properties in the builder.
        /// </summary>
        public virtual void Clear()
        {
            this._name = "";
            this.Permissions = Optional.FromNoValue<Permissions>();
            this.Color = Optional.FromNoValue<DiscordColor>();
            this.Hoist = Optional.FromNoValue<bool>();
            this.Mentionable = Optional.FromNoValue<bool>();
            this.AuditLogReason = Optional.FromNoValue<string>();
        }

        /// <summary>
        /// Performs validation logic to verify all the input is valid before sending it off to discord.
        /// </summary>
        public abstract void Validate();
    }
}
