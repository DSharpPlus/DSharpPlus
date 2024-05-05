namespace DSharpPlus.AsyncEvents;


/// <summary>
/// Represents a non-generic base for async events.
/// </summary>
public abstract class AsyncEvent
{
    public string Name { get; }

    protected internal AsyncEvent(string name) => this.Name = name;
}
