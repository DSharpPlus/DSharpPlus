using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DSharpPlus.Commands.ArgumentModifiers;
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
    /// The current index of the variadic-argument parameter.
    /// </summary>
    public int VariadicArgumentParameterIndex { get; protected set; } = -1;

    /// <summary>
    /// The current variadic-argument parameter.
    /// </summary>
    public VariadicParameterAttribute? VariadicArgumentAttribute { get; protected set; }

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
        this.VariadicArgumentParameterIndex = -1;
        this.VariadicArgumentAttribute = this.Parameter.Attributes.FirstOrDefault(attribute => attribute is VariadicParameterAttribute) as VariadicParameterAttribute;
        return true;
    }

    /// <summary>
    /// Advances to the next argument, returning a value indicating whether there was another argument.
    /// </summary>
    public abstract bool NextArgument();

    /// <summary>
    /// Increments the variadic-argument parameter index.
    /// </summary>
    /// <returns>Whether the current parameter can accept another argument or not.</returns>
    [MemberNotNullWhen(true, nameof(VariadicArgumentAttribute))]
    public virtual bool NextVariadicArgument()
    {
        if (this.VariadicArgumentAttribute is null)
        {
            return false;
        }
        else if (this.VariadicArgumentParameterIndex++ >= this.VariadicArgumentAttribute.MaximumArgumentCount)
        {
            this.VariadicArgumentParameterIndex--;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Short-hand for converting to a more specific converter context type.
    /// </summary>
    public T As<T>() where T : ConverterContext => (T)this;
}
