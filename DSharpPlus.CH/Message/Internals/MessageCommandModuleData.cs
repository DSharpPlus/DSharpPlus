namespace DSharpPlus.CH.Message.Internals
{
    internal class MessageCommandModuleData
    {
        public Type Type { get; set; }

        public MessageCommandModuleData(Type type) {
            Type = type;
        }
    }
}
