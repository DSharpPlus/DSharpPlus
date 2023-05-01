using System.Threading.Tasks;
using DSharpPlus.CH.Message;
using DSharpPlus.CH.Message.Permission;
using Microsoft.Extensions.Logging;

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

    [MessageCommand("arg opt")]
    public IMessageCommandModuleResult TestArgOpt(string argument,
        [MessageOption("user", "u")] Entities.DiscordUser? user)
    {
        return Reply(
            user is not null
                ? $"Argument was {argument} and user {user.Username} was given"
                : $"Argument was {argument} and user wasn't provided.");
    }

    [MessageCommand("permissions")]
    [MessagePermission(Permissions.Administrator)]
    public IMessageCommandModuleResult TestPermissions() => Reply("You are a admin.");

    [MessageCommand("di")]
    public IMessageCommandModuleResult TestDi() => Reply($"DI gave me value `{_str}`.");
}
