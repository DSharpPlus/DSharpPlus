using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Converters
{
    public abstract record ConverterContext : AbstractContext
    {
        public virtual object? Argument { get; protected set; }
        public int ParameterIndex { get; private set; } = -1;
        public CommandParameter Parameter => Command.Parameters[ParameterIndex];

        public bool NextParameter()
        {
            if (ParameterIndex + 1 >= Command.Parameters.Count)
            {
                return false;
            }

            ParameterIndex++;
            return true;
        }

        public abstract bool NextArgument();

        public T As<T>() where T : ConverterContext => (T)this;
    }
}
