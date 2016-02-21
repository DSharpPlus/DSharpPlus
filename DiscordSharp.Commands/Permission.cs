using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Commands
{
    public enum PermissionType : int
    {
        Owner = 666,
        Admin = 2,
        Mod = 1,
        User = 0,
        None = -666
    }
}
