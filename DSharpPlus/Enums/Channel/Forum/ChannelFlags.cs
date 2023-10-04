using System;
namespace DSharpPlus
{
    [Flags]
    public enum ChannelFlags
    {
        /// <summary>
        /// The channel is pinned.
        /// </summary>
        Pinned = 1 << 1,

        /// <summary>
        /// The [forum] channel requires tags to be applied.
        /// </summary>
        RequiresTag = 1 << 4
    }
}
