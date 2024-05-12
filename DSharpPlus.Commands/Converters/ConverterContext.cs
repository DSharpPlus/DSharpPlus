using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Converters;

/// <summary>
/// Represents context provided to argument converters.
/// </summary>
public abstract record ConverterContext : AbstractContext
{
    /// <summary>
    /// The value of the current raw argument.
    /// </summary>
    public virtual object? Argument { get; protected set; }

    /// <summary>
    /// The index of the current parameter.
    /// </summary>
    public int ParameterIndex { get; private set; } = -1;

    /// <summary>
    /// The current parameter.
    /// </summary>
    public CommandParameter Parameter => this.Command.Parameters[this.ParameterIndex];

    /// <summary>
    /// Advances to the next parameter, returning a value indicating whether there was another parameter.
    /// </summary>
    public virtual bool NextParameter()
    {
        if (this.ParameterIndex + 1 >= this.Command.Parameters.Count)
        {
            return false;
        }

        this.ParameterIndex++;
        return true;
    }

    /// <summary>
    /// Short-hand for converting to a more specific converter context type.
    /// </summary>
    public T As<T>() where T : ConverterContext => (T)this;
}
