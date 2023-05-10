using System.Threading; 
using System;
using DSharpPlus.SlashCommands;

namespace DSharpPlus.SlashCommands;

public interface IInteractionContextAccessor
{
    InteractionContext? InteractionContext { get; set; }

}

public class InteractionContextAccessor : IInteractionContextAccessor
{
    private static readonly AsyncLocal<InteractionContextHolder> _interactionContextCurrent = new AsyncLocal<InteractionContextHolder>();
    private readonly string Id = Guid.NewGuid().ToString();
    public InteractionContext? InteractionContext
    {
        get
        {
            return _interactionContextCurrent.Value?.Context;
        }
        set
        {
            var holder = _interactionContextCurrent.Value;
            if (holder != null)
            {
                holder.Context = null;
            }

            if (value != null)
            {
                _interactionContextCurrent.Value = new InteractionContextHolder { Context = value };
            }
        }
    }

    private sealed class InteractionContextHolder
    {
        public InteractionContext? Context;
    }
}
