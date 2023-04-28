using System.Threading.Tasks;
using DSharpPlus.CH.Message;
using DSharpPlus.CH.Message.Permission;

namespace DSharpPlus.Test;

[MessageModule("test")]
public class CHCommandModuleTest : MessageCommandModule
{
    private readonly string _str;

    public CHCommandModuleTest(string str) => _str = str;

    [MessageCommand("sync")]
    public IMessageCommandModuleResult TestSync() => Reply("Sync works.");

    [MessageCommand("async")]
    public async Task<IMessageCommandModuleResult> TestAsync()
    {
        await PostAsync(Reply("Async works"));
        return Empty();
    }

    [MessageCommand("arg-opt")]
    public IMessageCommandModuleResult TestArgOpt(string argument, [MessageOption("option", "o")] string? option) => Reply(option != null ? $"Argument was {argument} and option was {option}" : $"Argument was {argument} and option wasn't provided.");

    [MessageCommand("permissions")]
    [MessagePermission(Permissions.Administrator)]
    public IMessageCommandModuleResult TestPermissions() => Reply("You are a admin.");

    [MessageCommand("di")]
    public IMessageCommandModuleResult TestDI() => Reply($"DI gave me value `{_str}`.");
}
