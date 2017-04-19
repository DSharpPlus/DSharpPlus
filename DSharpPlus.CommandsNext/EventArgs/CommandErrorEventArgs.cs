using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.CommandsNext
{
    public class CommandErrorEventArgs : CommandEventArgs
    {
        public Exception Exception { get; private set; }


    }
}
