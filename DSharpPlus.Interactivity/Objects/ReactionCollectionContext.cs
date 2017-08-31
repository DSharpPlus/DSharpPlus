using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    public class ReactionCollectionContext
    {
        public ConcurrentDictionary<DiscordEmoji, int> Reactions = new ConcurrentDictionary<DiscordEmoji, int>();

        public InteractivityModule Interactivity;

        public DiscordClient Client => Interactivity.Client;

        internal void AddReaction(DiscordEmoji dr)
        {
            if (Reactions.ContainsKey(dr))
                Reactions[dr]++;
            else
                Reactions.TryAdd(dr, 1);
        }

        internal void RemoveReaction(DiscordEmoji dr)
        {
            if (Reactions.ContainsKey(dr))
            {
                Reactions[dr]--;
                if (Reactions[dr] == 0)
                    Reactions.TryRemove(dr, out int something);
            }
        }

        internal void ClearReactions()
        {
            Reactions = new ConcurrentDictionary<DiscordEmoji, int>();
        }
    }
}
