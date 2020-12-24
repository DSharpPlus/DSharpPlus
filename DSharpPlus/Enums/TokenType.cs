﻿using System;

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
