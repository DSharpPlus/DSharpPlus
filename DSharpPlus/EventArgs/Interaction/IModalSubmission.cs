using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents the values submitted to a component from a modal. Cast this object to a typed *ModalSubmission to access the submitted data.
/// </summary>
public interface IModalSubmission
{
    /// <summary>
    /// The type of component this submission represents. Use <see cref="As"/> to cast this object accordingly to access the submitted data. 
    /// </summary>
    public DiscordComponentType ComponentType { get; }

    /// <summary>
    /// The custom ID of this component.
    /// </summary>
    public string CustomId { get; }

    /// <summary>
    /// Casts this modal submission to its concrete type.
    /// </summary>
    public T? As<T>()
        where T : class, IModalSubmission
        => this as T;

    /// <summary>
    /// Checks whether this modal submission is of the specified concrete type.
    /// </summary>
    public bool Is<T>()
        where T : class, IModalSubmission
        => GetType().IsAssignableTo(typeof(T));
}
