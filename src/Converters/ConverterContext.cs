using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.Converters
{
    public abstract record ConverterContext : AbstractContext
    {
        public int ArgumentIndex { get; private set; } = -1;
        public CommandArgument Argument => Command.Arguments[ArgumentIndex];

        public bool NextArgument()
        {
            if (ArgumentIndex + 1 >= Command.Arguments.Count)
            {
                return false;
            }

            ArgumentIndex++;
            return true;
        }

        public T As<T>() where T : ConverterContext => (T)this;
    }
}
