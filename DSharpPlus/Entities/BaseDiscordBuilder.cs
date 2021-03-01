using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    public abstract class BaseDiscordBuilder
    {
        internal virtual void Validate(bool isModify = false)
        {
            throw new Exception("The derived class does not implement a validate method.");
        }
    }
}
