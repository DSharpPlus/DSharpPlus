namespace DSharpPlus.Objects
{
    public class DiscordUserInformation
    {
        /// <summary>
        /// The user's email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// The user's password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The username of the user
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The avatar of the user
        /// </summary>
        public string Avatar { get; set; }
        public DiscordUserInformation()
        {}

		/// <summary>
		/// Copies the DiscordUserInformation
		/// </summary>
		/// <returns>DiscordUserInformation</returns>
		public DiscordUserInformation Copy()
        {
            return new DiscordUserInformation
            {
                Email = this.Email,
                Password = this.Password,
                Username = this.Username,
                Avatar = this.Avatar
            };
        }
    }
}
