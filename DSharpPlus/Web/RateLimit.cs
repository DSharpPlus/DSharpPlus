using System;

namespace DSharpPlus
{
    public class RateLimit
    {
        public string Url { get; internal set; }
        public int UsesLeft { get; internal set; }
        public int UsesMax { get; internal set; }
        public DateTime Reset { get; internal set; }

        public override string ToString()
        {
            return $"{Url} [{UsesLeft}/{UsesMax}]: {Reset.ToString()}";
        }
    }
}
