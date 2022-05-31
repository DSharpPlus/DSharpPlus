using System;

namespace DSharpPlus.Core.JsonConverters.Reflect
{
    /// <summary>
    /// Internal flags used to support ignoring properties.
    /// </summary>
    [Flags]
    internal enum JsonFieldState
    {
        /// <summary>
        /// No relevant state.
        /// </summary>
        None = 0,

        /// <summary>
        /// The property value is <see langword="null"/>.
        /// </summary>
        Null = 1,

        /// <summary>
        /// The property value is equal to the <see langword="default"/> of its type.
        /// </summary>
        Default = 2
    }
}
