using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCord.Commands
{
    public class DiscordCommandParameter
    {
        public string parameterName;
        public string parameterDescription;
        public DiscordCommandParameterType parameterType;

        public DiscordCommandParameter(string parameterName, string parameterDescription="", DiscordCommandParameterType parameterType = DiscordCommandParameterType.Required)
        {
            this.parameterName = parameterName;
            this.parameterDescription = parameterDescription;
            this.parameterType = parameterType;
        }

        public static implicit operator DiscordCommandParameter(string s)
        {
            return new DiscordCommandParameter(s);
        }

        public DiscordCommandParameter Type(DiscordCommandParameterType type)
        {
            this.parameterType = type;
            return this;
        }
    }

    public enum DiscordCommandParameterType
    {
        Required,
        NotRequired,
        Multiple
    }
}
