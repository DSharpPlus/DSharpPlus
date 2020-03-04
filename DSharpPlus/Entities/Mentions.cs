using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Interface for mentionables
    /// </summary>
    internal interface IMention { }

    /// <summary>
    /// A everyone mention
    /// </summary>
    public readonly struct EveryoneMention : IMention { }

    /// <summary>
    /// A user mention
    /// </summary>
    public readonly struct UserMention : IMention
    {
        /// <summary>
        /// Id of the user that is allowed to be mentioned
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// Creates a new user mention based of the id
        /// </summary>
        /// <param name="id"></param>
        public UserMention(ulong id) { this.Id = id;  }

        /// <summary>
        /// Creates a new user mention based of the user
        /// </summary>
        /// <param name="user"></param>
        public UserMention(DiscordUser user) : this(user.Id) { }

        public static implicit operator UserMention(DiscordUser user) => new UserMention(user.Id);
    }

    /// <summary>
    /// A role mention
    /// </summary>
    public readonly struct RoleMention : IMention
    {
        /// <summary>
        /// Id of the role that is allowed to be mentioned
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// Creates a new role mention based of the id
        /// </summary>
        /// <param name="id"></param>
        public RoleMention(ulong id) { this.Id = id; }

        /// <summary>
        /// Creates a new role mention based of the id
        /// </summary>
        /// <param name="role"></param>
        public RoleMention(DiscordRole role) : this(role.Id) { }

        public static implicit operator RoleMention(DiscordRole role) => new RoleMention(role.Id);
    }


}
