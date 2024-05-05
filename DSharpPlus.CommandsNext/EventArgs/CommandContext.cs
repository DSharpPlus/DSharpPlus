using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandsNext;

/// <summary>
/// Represents a context in which a command is executed.
/// </summary>
public sealed class CommandContext
{
    /// <summary>
    /// Gets the client which received the message.
    /// </summary>
    public DiscordClient Client { get; internal set; } = null!;

    /// <summary>
    /// Gets the message that triggered the execution.
    /// </summary>
    public DiscordMessage Message { get; internal set; } = null!;

    /// <summary>
    /// Gets the channel in which the execution was triggered,
    /// </summary>
    public DiscordChannel Channel
        => this.Message.Channel;

    /// <summary>
    /// Gets the guild in which the execution was triggered. This property is null for commands sent over direct messages.
    /// </summary>
    public DiscordGuild Guild
        => this.Channel.Guild;

    /// <summary>
    /// Gets the user who triggered the execution.
    /// </summary>
    public DiscordUser User
        => this.Message.Author;

    /// <summary>
    /// Gets the member who triggered the execution. This property is null for commands sent over direct messages.
    /// </summary>
    public DiscordMember? Member
        => this.lazyMember.Value;

    private readonly Lazy<DiscordMember?> lazyMember;

    /// <summary>
    /// Gets the CommandsNext service instance that handled this command.
    /// </summary>
    public CommandsNextExtension CommandsNext { get; internal set; } = null!;

    /// <summary>
    /// Gets the service provider for this CNext instance.
    /// </summary>
    public IServiceProvider Services { get; internal set; } = null!;

    /// <summary>
    /// Gets the command that is being executed.
    /// </summary>
    public Command? Command { get; internal set; }

    /// <summary>
    /// Gets the overload of the command that is being executed.
    /// </summary>
    public CommandOverload Overload { get; internal set; } = null!;

    /// <summary>
    /// Gets the list of raw arguments passed to the command.
    /// </summary>
    public IReadOnlyList<string> RawArguments { get; internal set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the raw string from which the arguments were extracted.
    /// </summary>
    public string RawArgumentString { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the prefix used to invoke the command.
    /// </summary>
    public string Prefix { get; internal set; } = string.Empty;

    internal CommandsNextConfiguration Config { get; set; } = null!;

    internal ServiceContext ServiceScopeContext { get; set; }

    internal CommandContext() => this.lazyMember = new Lazy<DiscordMember?>(() => this.Guild is not null && this.Guild.Members.TryGetValue(this.User.Id, out DiscordMember? member) ? member : this.Guild?.GetMemberAsync(this.User.Id).GetAwaiter().GetResult());

    /// <summary>
    /// Quickly respond to the message that triggered the command.
    /// </summary>
    /// <param name="content">Message to respond with.</param>
    /// <returns></returns>
    public Task<DiscordMessage> RespondAsync(string content)
        => this.Message.RespondAsync(content);

    /// <summary>
    /// Quickly respond to the message that triggered the command.
    /// </summary>
    /// <param name="embed">Embed to attach.</param>
    /// <returns></returns>
    public Task<DiscordMessage> RespondAsync(DiscordEmbed embed)
        => this.Message.RespondAsync(embed);

    /// <summary>
    /// Quickly respond to the message that triggered the command.
    /// </summary>
    /// <param name="content">Message to respond with.</param>
    /// <param name="embed">Embed to attach.</param>
    /// <returns></returns>
    public Task<DiscordMessage> RespondAsync(string content, DiscordEmbed embed)
        => this.Message.RespondAsync(content, embed);

    /// <summary>
    /// Quickly respond to the message that triggered the command.
    /// </summary>
    /// <param name="builder">The Discord Message builder.</param>
    /// <returns></returns>
    public Task<DiscordMessage> RespondAsync(DiscordMessageBuilder builder)
        => this.Message.RespondAsync(builder);

    /// <summary>
    /// Quickly respond to the message that triggered the command.
    /// </summary>
    /// <param name="action">The Discord Message builder.</param>
    /// <returns></returns>
    public Task<DiscordMessage> RespondAsync(Action<DiscordMessageBuilder> action)
        => this.Message.RespondAsync(action);

    /// <summary>
    /// Triggers typing in the channel containing the message that triggered the command.
    /// </summary>
    /// <returns></returns>
    public Task TriggerTypingAsync()
        => this.Channel.TriggerTypingAsync();

    internal readonly struct ServiceContext : IDisposable
    {
        public IServiceProvider Provider { get; }
        public IServiceScope Scope { get; }
        public bool IsInitialized { get; }

        public ServiceContext(IServiceProvider services, IServiceScope scope)
        {
            this.Provider = services;
            this.Scope = scope;
            this.IsInitialized = true;
        }

        public readonly void Dispose() => this.Scope?.Dispose();
    }
}
