using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Objects
{
    public class DiscordPermissionOverride
    {
        public enum OverrideType
        {
            role, member
        }

        private uint allow_raw = 0, deny_raw = 0;
        public OverrideType type { get; set; }
        /// <summary>
        /// If the type is role, then this is the id of the role.
        /// If the type is member, then this is the id of the member in the server.
        /// </summary>
        public string id { get; set; }

        internal DiscordPermissionOverride() { }
        internal DiscordPermissionOverride(uint _allow_raw, uint _deny_raw) { allow_raw = _allow_raw; deny_raw = _deny_raw; }
        
        /// <summary>
        /// Checks to see if the role has a specific permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns>True or false.</returns>
        public bool AllowedPermission(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return false;
            return ((allow_raw >> (byte)permission) & 1) != 0;
        }

        /// <summary>
        /// Checks to see if the role has a specific permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns>True or false.</returns>
        public bool DeniedPermission(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return false;
            return ((deny_raw >> (byte)permission) & 1) != 0;
        }

        /// <summary>
        /// Sets a specified permission on a user.
        /// </summary>
        /// <param name="permission"></param>
        public void SetPermissionAllowed(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return;
            allow_raw |= (uint)(1 << (byte)permission);
        }

        /// <summary>
        /// Removes the specified permission on a user.
        /// </summary>
        /// <param name="permission"></param>
        public void RemovePermissionAllowed(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return;
            allow_raw &= (uint)~(1 << (byte)permission);
        }

        /// <summary>
        /// Sets a specified permission on a user.
        /// </summary>
        /// <param name="permission"></param>
        public void SetPermissionDenied(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return;
            deny_raw |= (uint)(1 << (byte)permission);
        }

        /// <summary>
        /// Removes the specified permission on a user.
        /// </summary>
        /// <param name="permission"></param>
        public void RemovePermissionDenied(DiscordSpecialPermissions permission)
        {
            if (permission == DiscordSpecialPermissions.None)
                return;
            deny_raw &= (uint)~(1 << (byte)permission);
        }

        public List<DiscordSpecialPermissions> GetAllAllowedPermissions()
        {
            if (allow_raw == 0)
                return new List<DiscordSpecialPermissions> { DiscordSpecialPermissions.None };

            List<DiscordSpecialPermissions> allowed = new List<DiscordSpecialPermissions>();
            var allPermissions = Enum.GetValues(typeof(DiscordSpecialPermissions));
            foreach(DiscordSpecialPermissions p in allPermissions)
            {
                if (AllowedPermission(p))
                    allowed.Add(p);
            }
            return allowed;
        }

        public List<DiscordSpecialPermissions> GetAllDeniedPermissions()
        {
            if (deny_raw == 0)
                return new List<DiscordSpecialPermissions> { DiscordSpecialPermissions.None };

            List<DiscordSpecialPermissions> allowed = new List<DiscordSpecialPermissions>();
            var allPermissions = Enum.GetValues(typeof(DiscordSpecialPermissions));
            foreach (DiscordSpecialPermissions p in allPermissions)
            {
                if (DeniedPermission(p))
                    allowed.Add(p);
            }
            return allowed;
        }


        public uint GetAllowedRawPermissions() => allow_raw;
        public uint GetDeniedRawPermissions() => deny_raw;
    }
}
