namespace DSharpPlus.CommandsNext.Converters
{
    public class StringConverter : IArgumentConverter<string>
    {
        public bool TryConvert(string value, CommandContext ctx, out string result)
        {
            result = value;
            return true;
        }
    }
}
