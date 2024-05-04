using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Converters;

public abstract record ConverterContext : AbstractContext
{
    public virtual object? Argument { get; protected set; }
    public int ParameterIndex { get; private set; } = -1;
    public CommandParameter Parameter => this.Command.Parameters[this.ParameterIndex];

    public bool NextParameter()
    {
        if (this.ParameterIndex + 1 >= this.Command.Parameters.Count)
        {
            return false;
        }

        this.ParameterIndex++;
        return true;
    }

    public abstract bool NextArgument();

    public T As<T>() where T : ConverterContext => (T)this;
}
