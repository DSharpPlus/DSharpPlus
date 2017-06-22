using System;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines a readable name for this permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PermissionStringAttribute : Attribute
    {
        /// <summary>
        /// Gets the readable name for this permission.
        /// </summary>
        public string String { get; private set; }

        /// <summary>
        /// Defines a readable name for this permission.
        /// </summary>
        /// <param name="str">Readable name for this permission.</param>
        public PermissionStringAttribute(string str)
        {
            this.String = str;
        }
    }
}
