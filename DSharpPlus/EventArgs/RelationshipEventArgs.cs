using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    public class RelationshipEventArgs : AsyncEventArgs
    {
        public DiscordRelationship Relationship { get; internal set; }
    }
}