namespace DSharpPlus.VideoNext
{
    public sealed class VideoNextConfiguration
    {
        public bool EnableIncoming { get; set; } = false;
        
        /// <summary>
        /// A numerical value which determines whether to prioritize quality or bandwidth usage.
        /// </summary>
        public int Quality { get; set; }
        
        public VideoNextConfiguration() { }

        public VideoNextConfiguration(VideoNextConfiguration other)
        {
            this.EnableIncoming = other.EnableIncoming;
            this.Quality = other.Quality;
        }
    }
}