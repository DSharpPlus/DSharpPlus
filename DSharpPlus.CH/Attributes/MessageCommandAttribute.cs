namespace DSharpPlus.CH
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageCommandAttribute : Attribute
    {
        public string Name { get; set; }

        public MessageCommandAttribute(string name)
        {
            Name = name;
        }
    }
}
