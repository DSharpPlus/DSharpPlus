using System.Net;
using DSharpPlus.CommandAll.Processors.SlashCommands;

namespace DSharpPlus.CommandAll.Processors.HttpCommands
{
    public record HttpConverterContext : SlashConverterContext
    {
        public required HttpListenerContext HttpContext { get; init; }
    }
}
