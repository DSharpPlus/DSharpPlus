using System;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents user status.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// User is offline.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// User is online.
        /// </summary>
        Online = 1,

        /// <summary>
        /// User is idle.
        /// </summary>
        Idle = 2,

        /// <summary>
        /// User asked not to be disturbed.
        /// </summary>
        DoNotDisturb = 4,

        /// <summary>
        /// User is invisible. They will appear as Offline to anyone but themselves.
        /// </summary>
        Invisible = 5
    }

    /// <summary>
    /// Represents a game that a user is playing.
    /// </summary>
    public class DiscordGame
    {
        /// <summary>
        /// Creates a new, empty instance of a <see cref="DiscordGame"/>.
        /// </summary>
        public DiscordGame()
        {
            StreamType = GameStreamType.NoStream;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordGame"/> with specified name.
        /// </summary>
        /// <param name="name"></param>
        public DiscordGame(string name)
        {
            Name = name;
            StreamType = GameStreamType.NoStream;
        }

        /// <summary>
        /// Gets or sets the name of the game the user is playing.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stream URI, if applicable.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the livesteam type.
        /// </summary>
        public GameStreamType StreamType { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets game state.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the application for which the rich presence is for.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public DiscordApplication Application { get; set; }

        /// <summary>
        /// Gets or sets instance status.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public bool? Instance { get; set; }

        /// <summary>
        /// Gets or sets large image for the rich presence.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public DiscordApplicationAsset LargeImage
        {
            get => _largeImage;
            set
            {
                if (value.Type != ApplicationAssetType.LargeImage)
                {
                    throw new InvalidOperationException("The large image asset needs to be a LargeImage type.");
                }

                _largeImage = value;
            }
        }
        private DiscordApplicationAsset _largeImage;

        /// <summary>
        /// Gets or sets the hovertext for large image.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public string LargeImageText { get; set; }

        /// <summary>
        /// Gets or sets small image for the rich presence.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public DiscordApplicationAsset SmallImage
        {
            get => _smallImage;
            set
            {
                if (value.Type != ApplicationAssetType.SmallImage)
                {
                    throw new InvalidOperationException("The small image asset needs to be a SmallImage type.");
                }

                _smallImage = value;
            }
        }
        private DiscordApplicationAsset _smallImage;

        /// <summary>
        /// Gets or sets the hovertext for small image.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public string SmallImageText { get; set; }

        /// <summary>
        /// Gets or sets current party size.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public int? CurrentPartySize
        {
            get => _partySizeCurrent;
            set
            {
                if (value > _partySizeMax || value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Current party size cannot exceed maximum party size and must be greater than zero.");
                }

                _partySizeCurrent = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum party size.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public int? MaximumPartySize
        {
            get => _partySizeMax;
            set
            {
                if (value < 0 || value < _partySizeCurrent)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Maximum party size must be greater than zero and greater or equal to current party size.");
                }

                _partySizeMax = value;
            }
        }

        private int? _partySizeCurrent;
        private int? _partySizeMax;

        /// <summary>
        /// Gets or sets the party ID.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public ulong? PartyId { get; set; }

        /// <summary>
        /// Gets or sets the game start timestamp.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public DateTimeOffset? StartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the game end timestamp.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public DateTimeOffset? EndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the secret value enabling users to join your game. Note that this works for RPC applications only.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public string JoinSecret { get; set; }

        /// <summary>
        /// Gets or sets the secret value enabling users to receive notifications whenever your game state changes. Requires instance set to true.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public string MatchSecret { get; set; }

        /// <summary>
        /// Gets or sets the secret value enabling users to spectate your game. Note that this works for RPC applications only.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        public string SpectateSecret { get; set; }
    }

    /// <summary>
    /// Determines the livestream type for a game.
    /// </summary>
    public enum GameStreamType
    {
        /// <summary>
        /// The game is not being streamed.
        /// </summary>
        NoStream = 0,

        /// <summary>
        /// The game is being streamed on Twitch.
        /// </summary>
        Twitch = 1
    }
}
