namespace DSharpPlus.CH.Internals
{
    internal class MessageCommandMethodData
    {
        public required MessageCommandModuleData Module { get; set; }
        public required System.Reflection.MethodInfo Method { get; set; }
        public List<MessageCommandParameterData> Parameters { get; set; } = new List<MessageCommandParameterData>();
        public bool IsAsync { get; set; } = false;
    }
}