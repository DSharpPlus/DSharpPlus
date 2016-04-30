using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Objects
{
    public enum DiscordSpecialPermissions : byte
    {
        //Me, donut use
        None = 255,

        //General
        CreateInstanceInvite = 0,
        KickMembers = 1,
        BanMembers = 2,
        ManageRoles = 3,
        //ManagePermissions = 3,
        ManageChannels = 4,
        //ManageChannel = 4,
        ManageServer = 5,
        //Chat
        ReadMessages = 10,
        SendMessages = 11,
        SendTTSMessages = 12,
        ManageMessages = 13,
        EmbedLinks = 14,
        AttachFiles = 15,
        ReadMessageHistory = 16,
        MentionEveryone = 17,
        //Voice
        VoiceConnect = 20,
        VoiceSpeak = 21,
        VoiceMuteMembers = 22,
        VoiceDeafenMembers = 23,
        VoiceMoveMembers = 24,
        VoiceUseActivationDetection = 25,
        ChangeNickname = 26,
        ManageNicknames = 27
    }
    public class DiscordPermission
    {
        private uint raw = 0;

        internal DiscordPermission() { }
        internal DiscordPermission(uint raw) { this.raw = raw; }

        /// <summary>
        /// Checks to see if the role has a specific permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns>True or false.</returns>
        public bool HasPermission(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return false;
            return ((raw >> (byte)permission)&1) != 0;
        }

        /// <summary>
        /// Sets a specified permission on a user.
        /// </summary>
        /// <param name="permission"></param>
        public void SetPermission(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return;
            raw |= (uint)(1 << (byte)permission);
        }

        public List<DiscordSpecialPermissions> GetAllPermissions()
        {
            if (raw == 0)
                return new List<DiscordSpecialPermissions> { DiscordSpecialPermissions.None };

            List<DiscordSpecialPermissions> perm = new List<DiscordSpecialPermissions>();
            var allPermissions = Enum.GetValues(typeof(DiscordSpecialPermissions));
            foreach(DiscordSpecialPermissions p in allPermissions)
            {
                if (HasPermission(p))
                    perm.Add(p);
            }
            return perm;
        }

        /// <summary>
        /// Removes the specified permission on a user.
        /// </summary>
        /// <param name="permission"></param>
        public void RemovePermission(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return;
            raw &= (uint)~(1 << (byte)permission);
        }

        public uint GetRawPermissions() => raw;

        //From Voltana <3
        //so it's a bitarray - each bit is a bool bit representing if it's true or false
        //(true in an allow array = allow, true in a deny array = deny, false in either = unchanged/inherit)
        //so to check if a user has BanMembers permission you do:
        //if ((permissions >> 2) & 1 != 0)

        //where 2 is the bit position you're checking (2 = ban members)

    }
}
