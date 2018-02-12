using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace DSharpPlus
{
	/// <summary>
	/// Token type
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TokenType
    {
        /// <summary>
        /// User token type
        /// </summary>
        [Obsolete("Logging in with a user token may result in your account being terminated, and is therefore highly unrecommended.", false)]
        User = 0,

        /// <summary>
        /// Bot token type
        /// </summary>
        Bot = 1,

        /// <summary>
        /// Bearer token type
        /// </summary>
        Bearer = 2
    }
}
