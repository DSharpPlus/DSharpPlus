namespace DSharpPlus.CH.Message.Internals
{
    internal class MessageCommandParameterData
    {
        public MessageCommandParameterDataType Type { get; set; } = MessageCommandParameterDataType.String;
        public object Value { get; set; } = string.Empty;
        public string? ShorthandOptionName { get; set; }
        public string OptionName { get; set; } = string.Empty;
        public bool CanBeNull { get; set; } = false;
        public bool IsArgument { get; set; } = true;
    }
}