#if NETSTANDARD1_1

namespace System
{
    public class MissingMethodException : Exception
    {
        public MissingMethodException(string message)
            : base(message)
        { }
    }
}
#endif