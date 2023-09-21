using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Executors;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// <para>Represents a delegate for a function that takes a message, and returns the position of the start of command invocation in the message. It has to return -1 if prefix is not present.</para>
    /// <para>
    /// It is recommended that helper methods <see cref="CommandsNextUtilities.GetStringPrefixLength(DiscordMessage, string, StringComparison)"/> and <see cref="CommandsNextUtilities.GetMentionPrefixLength(DiscordMessage, DiscordUser)"/>
    /// be used internally for checking. Their output can be passed through.
    /// </para>
    /// </summary>
    /// <param name="msg">Message to check for prefix.</param>
    /// <returns>Position of the command invocation or -1 if not present.</returns>
    public delegate Task<int> PrefixResolverDelegate(DiscordMessage msg);

    /// <summary>
    /// Represents a configuration for <see cref="CommandsNextExtension"/>.
    /// </summary>
    public sealed class CommandsNextConfiguration
    {
        /// <summary>
        /// <para>Sets the string prefixes used for commands.</para>
        /// <para>Defaults to no value (disabled).</para>
        /// </summary>
        public IEnumerable<string> StringPrefixes { internal get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Sets the custom prefix resolver used for commands.</para>
        /// <para>Defaults to none (disabled).</para>
        /// </summary>
        public PrefixResolverDelegate? PrefixResolver { internal get; set; } = null;

        /// <summary>
        /// <para>Sets whether to allow mentioning the bot to be used as command prefix.</para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool EnableMentionPrefix { internal get; set; } = true;

        /// <summary>
        /// <para>Sets whether strings should be matched in a case-sensitive manner.</para>
        /// <para>This switch affects the behaviour of default prefix resolver, command searching, and argument conversion.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool CaseSensitive { internal get; set; } = false;

        /// <summary>
        /// <para>Sets whether to enable default help command.</para>
        /// <para>Disabling this will allow you to make your own help command.</para>
        /// <para>
        /// Modifying default help can be achieved via custom help formatters (see <see cref="BaseHelpFormatter"/> and <see cref="CommandsNextExtension.SetHelpFormatter{T}()"/> for more details).
        /// It is recommended to use help formatter instead of disabling help.
        /// </para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool EnableDefaultHelp { internal get; set; } = true;

        /// <summary>
        /// <para>Controls whether the default help will be sent via DMs or not.</para>
        /// <para>Enabling this will make the bot respond with help via direct messages.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool DmHelp { internal get; set; } = false;

        /// <summary>
        /// <para>Sets the default pre-execution checks for the built-in help command.</para>
        /// <para>Only applicable if default help is enabled.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IEnumerable<CheckBaseAttribute> DefaultHelpChecks { internal get; set; } = Enumerable.Empty<CheckBaseAttribute>();

        /// <summary>
        /// <para>Sets whether commands sent via direct messages should be processed.</para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool EnableDms { internal get; set; } = true;

        /// <summary>
        /// <para>Sets the service provider for this CommandsNext instance.</para>
        /// <para>Objects in this provider are used when instantiating command modules. This allows passing data around without resorting to static members.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IServiceProvider Services { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);

        /// <summary>
        /// <para>Gets whether any extra arguments passed to commands should be ignored or not. If this is set to false, extra arguments will throw, otherwise they will be ignored.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool IgnoreExtraArguments { internal get; set; } = false;

        /// <summary>
        /// <para>Sets the quotation marks on parameters, used to interpret spaces as part of a single argument.</para>
        /// <para>Defaults to a collection of <c>"</c>, <c>«</c>, <c>»</c>, <c>‘</c>, <c>“</c>, <c>„</c> and <c>‟</c>.</para>
        /// </summary>
        public IEnumerable<char> QuotationMarks { internal get; set; } = new[] { '"', '«', '»', '‘', '“', '„', '‟' };

        /// <summary>
        /// <para>Gets or sets whether to automatically enable handling commands.</para>
        /// <para>If this is set to false, you will need to manually handle each incoming message and pass it to CommandsNext.</para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool UseDefaultCommandHandler { internal get; set; } = true;

        /// <summary>
        /// <para>Gets or sets the default culture for parsers.</para>
        /// <para>Defaults to invariant.</para>
        /// </summary>
        public CultureInfo DefaultParserCulture { internal get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// <para>Gets or sets the default command executor.</para>
        /// <para>This alters the behaviour, execution, and scheduling method of command execution.</para>
        /// </summary>
        public ICommandExecutor CommandExecutor { internal get; set; } = new ParallelQueuedCommandExecutor();

        /// <summary>
        /// Creates a new instance of <see cref="CommandsNextConfiguration"/>.
        /// </summary>
        public CommandsNextConfiguration() { }

        /// <summary>
        /// Creates a new instance of <see cref="CommandsNextConfiguration"/>, copying the properties of another configuration.
        /// </summary>
        /// <param name="other">Configuration the properties of which are to be copied.</param>
        public CommandsNextConfiguration(CommandsNextConfiguration other)
        {
            this.CaseSensitive = other.CaseSensitive;
            this.PrefixResolver = other.PrefixResolver;
            this.DefaultHelpChecks = other.DefaultHelpChecks;
            this.EnableDefaultHelp = other.EnableDefaultHelp;
            this.EnableDms = other.EnableDms;
            this.EnableMentionPrefix = other.EnableMentionPrefix;
            this.IgnoreExtraArguments = other.IgnoreExtraArguments;
            this.QuotationMarks = other.QuotationMarks;
            this.UseDefaultCommandHandler = other.UseDefaultCommandHandler;
            this.Services = other.Services;
            this.StringPrefixes = other.StringPrefixes.ToArray();
            this.DmHelp = other.DmHelp;
            this.DefaultParserCulture = other.DefaultParserCulture;
            this.CommandExecutor = other.CommandExecutor;
        }
    }
}
