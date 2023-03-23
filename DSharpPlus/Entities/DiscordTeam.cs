// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a team consisting of users. A team can own an application.
    /// </summary>
    public sealed class DiscordTeam : SnowflakeObject, IEquatable<DiscordTeam>
    {
        /// <summary>
        /// Gets the team's name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the team's icon hash.
        /// </summary>
        public string IconHash { get; internal set; }

        /// <summary>
        /// Gets the team's icon.
        /// </summary>
        public string Icon
            => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/team-icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=1024" : null;

        /// <summary>
        /// Gets the owner of the team.
        /// </summary>
        public DiscordUser Owner { get; internal set; }

        /// <summary>
        /// Gets the members of this team.
        /// </summary>
        public IReadOnlyList<DiscordTeamMember> Members { get; internal set; }

        internal DiscordTeam(TransportTeam tt)
        {
            this.Id = tt.Id;
            this.Name = tt.Name;
            this.IconHash = tt.IconHash;
        }

        /// <summary>
        /// Compares this team to another object and returns whether they are equal.
        /// </summary>
        /// <param name="obj">Object to compare this team to.</param>
        /// <returns>Whether this team is equal to the given object.</returns>
        public override bool Equals(object obj)
            => obj is DiscordTeam other && this == other;

        /// <summary>
        /// Compares this team to another team and returns whether they are equal.
        /// </summary>
        /// <param name="other">Team to compare to.</param>
        /// <returns>Whether the teams are equal.</returns>
        public bool Equals(DiscordTeam other)
            => this == other;

        /// <summary>
        /// Gets the hash code of this team.
        /// </summary>
        /// <returns>Hash code of this team.</returns>
        public override int GetHashCode()
            => this.Id.GetHashCode();

        /// <summary>
        /// Converts this team to its string representation.
        /// </summary>
        /// <returns>The string representation of this team.</returns>
        public override string ToString()
            => $"Team: {this.Name} ({this.Id})";

        public static bool operator ==(DiscordTeam left, DiscordTeam right)
            => left?.Id == right?.Id;

        public static bool operator !=(DiscordTeam left, DiscordTeam right)
            => left?.Id != right?.Id;
    }

    /// <summary>
    /// Represents a member of <see cref="DiscordTeam"/>.
    /// </summary>
    public sealed class DiscordTeamMember : IEquatable<DiscordTeamMember>
    {
        /// <summary>
        /// Gets the member's membership status.
        /// </summary>
        public DiscordTeamMembershipStatus MembershipStatus { get; internal set; }

        /// <summary>
        /// Gets the member's permissions within the team.
        /// </summary>
        public IReadOnlyCollection<string> Permissions { get; internal set; }

        /// <summary>
        /// Gets the team this member belongs to.
        /// </summary>
        public DiscordTeam Team { get; internal set; }

        /// <summary>
        /// Gets the user who is the team member.
        /// </summary>
        public DiscordUser User { get; internal set; }

        internal DiscordTeamMember(TransportTeamMember ttm)
        {
            this.MembershipStatus = (DiscordTeamMembershipStatus)ttm.MembershipState;
            this.Permissions = new ReadOnlySet<string>(new HashSet<string>(ttm.Permissions));
        }

        /// <summary>
        /// Compares this team member to another object and returns whether they are equal.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether this team is equal to given object.</returns>
        public override bool Equals(object obj)
            => obj is DiscordTeamMember other && this == other;

        /// <summary>
        /// Compares this team member to another team member and returns whether they are equal.
        /// </summary>
        /// <param name="other">Team member to compare to.</param>
        /// <returns>Whether this team member is equal to the given one.</returns>
        public bool Equals(DiscordTeamMember other)
            => this == other;

        /// <summary>
        /// Gets a hash code of this team member.
        /// </summary>
        /// <returns>Hash code of this team member.</returns>
        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + this.User.GetHashCode();
            hash = (hash * 7) + this.Team.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Converts this team member to their string representation.
        /// </summary>
        /// <returns>String representation of this team member.</returns>
        public override string ToString()
            => $"Team member: {this.User.Username}#{this.User.Discriminator} ({this.User.Id}), part of team {this.Team.Name} ({this.Team.Id})";

        public static bool operator ==(DiscordTeamMember left, DiscordTeamMember right)
            => left?.Team?.Id == right?.Team?.Id && left?.User?.Id == right?.User?.Id;

        public static bool operator !=(DiscordTeamMember left, DiscordTeamMember right)
            => left?.Team?.Id != right?.Team?.Id || left?.User?.Id != right?.User?.Id;
    }

    /// <summary>
    /// Signifies the status of user's team membership.
    /// </summary>
    public enum DiscordTeamMembershipStatus : int
    {
        /// <summary>
        /// Indicates that this user is invited to the team, and is pending membership.
        /// </summary>
        Invited = 1,

        /// <summary>
        /// Indicates that this user is a member of the team.
        /// </summary>
        Accepted = 2
    }
}
