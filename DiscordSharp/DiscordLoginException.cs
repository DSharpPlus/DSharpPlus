using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp 
{
    [Serializable]
    public class DiscordLoginException : Exception, ISerializable
    {
        public DiscordLoginException() : base("A generic exception occurred while trying to login to Discord.")
        {}
        public DiscordLoginException(string message) : base(message)
        {}
        public DiscordLoginException(string message, Exception inner) : base(message, inner)
        {}
        protected DiscordLoginException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}
