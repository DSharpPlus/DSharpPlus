#if NETSTANDARD1_1

// ReSharper disable once CheckNamespace
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
