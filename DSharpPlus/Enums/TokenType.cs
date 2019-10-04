using System;

namespace DSharpPlus
{
    /// <summary>
    /// Token type
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// User token type
        /// </summary>
        [Obsolete("Logging in with a user token may result in your account being terminated, and is therefore highly unrecommended." +
            "\nIf anything goes wrong with this, we will not provide any support!", true)]
        User = 0,

        /// <summary>
        /// Bot token type
        /// </summary>
        Bot = 1,

        /// <summary>
        /// Bearer token type (used for oAuth)
        /// </summary>
        Bearer = 2
    }
}
