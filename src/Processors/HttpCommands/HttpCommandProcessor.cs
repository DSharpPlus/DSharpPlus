using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Exceptions;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.CommandAll.Processors.HttpCommands
{
    public sealed class HttpCommandProcessor : ICommandProcessor<InteractionCreateEventArgs>
    {
        public IReadOnlyDictionary<Type, ConverterDelegate<InteractionCreateEventArgs>> Converters => _slashCommandProcessor?.Converters ?? new Dictionary<Type, ConverterDelegate<InteractionCreateEventArgs>>();
        public HttpCommandConfiguration Configuration { get; init; }

        private CommandAllExtension? _extension;
        private SlashCommandProcessor? _slashCommandProcessor;
        private readonly HttpListener _httpListener;

        public HttpCommandProcessor(HttpCommandConfiguration configuration)
        {
            Configuration = configuration;

            _httpListener = new HttpListener();
            foreach (string prefix in configuration.Prefixes)
            {
                _httpListener.Prefixes.Add(prefix);
            }
        }

        public async Task ConfigureAsync(CommandAllExtension extension)
        {
            _extension = extension;
            _slashCommandProcessor = _extension.GetProcessor<SlashCommandProcessor>() ?? new SlashCommandProcessor();
            await _slashCommandProcessor.ConfigureAsync(_extension);
            _ = ListenAsync();
        }

        private async Task ListenAsync()
        {
            if (_extension is null || _slashCommandProcessor is null)
            {
                return;
            }

            _httpListener.Start();
            while (true)
            {
                HttpListenerContext httpContext = await _httpListener.GetContextAsync();
                if (!HttpDiscordHeaderVerifier.TryVerify(httpContext, Configuration.PublicKey))
                {
                    // The discord header verifier will respond with the appropriate error message.
                    // This should probably be done here instead and the verifier should be modified to be easily resused by other API's.
                    continue;
                }

                AsyncServiceScope scope = _extension.ServiceProvider.CreateAsyncScope();
                InteractionCreateEventArgs eventArgs = DiscordJson.ToDiscordObject<InteractionCreateEventArgs>(JToken.ReadFrom(new JsonTextReader(new StreamReader(httpContext.Request.InputStream))));

                // TODO: Find a more creative implementation that doesn't duplicate code.
                if (!_slashCommandProcessor.TryFindCommand(eventArgs.Interaction, out Command? command, out IEnumerable<DiscordInteractionDataOption>? options))
                {
                    await _extension._commandErrored.InvokeAsync(_extension, new CommandErroredEventArgs()
                    {
                        Context = new HttpCommandContext()
                        {
                            Arguments = new Dictionary<CommandArgument, object?>(),
                            Channel = eventArgs.Interaction.Channel,
                            Command = null!,
                            Extension = _extension,
                            ServiceScope = scope,
                            User = eventArgs.Interaction.User,
                            Interaction = eventArgs.Interaction,
                            Options = eventArgs.Interaction.Data.Options ?? [],
                            HttpContext = httpContext
                        },
                        CommandObject = null,
                        Exception = new CommandNotFoundException(eventArgs.Interaction.Data.Name)
                    });

                    await scope.DisposeAsync();
                    return;
                }

                HttpConverterContext converterContext = new()
                {
                    Channel = eventArgs.Interaction.Channel,
                    Command = command,
                    Extension = _extension,
                    Interaction = eventArgs.Interaction,
                    Options = options,
                    ServiceScope = scope,
                    User = eventArgs.Interaction.User,
                    HttpContext = httpContext
                };

                if (eventArgs.Interaction.Type == InteractionType.AutoComplete)
                {
                    AutoCompleteContext? autoCompleteContext = await ParseAutoCompletesAsync(converterContext, eventArgs);
                    if (autoCompleteContext is not null)
                    {
                        IEnumerable<DiscordAutoCompleteChoice> choices = await converterContext.Argument.Attributes.OfType<SlashAutoCompleteProviderAttribute>().First().AutoCompleteAsync(autoCompleteContext);
                        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, new DiscordInteractionResponseBuilder().AddAutoCompleteChoices(choices));
                    }

                    await converterContext.ServiceScope.DisposeAsync();
                }
                else
                {
                    CommandContext? commandContext = await ParseArgumentsAsync(converterContext, eventArgs);
                    if (commandContext is null)
                    {
                        await converterContext.ServiceScope.DisposeAsync();
                        return;
                    }

                    await _extension.CommandExecutor.ExecuteAsync(commandContext);
                }
            }
        }

        private async Task<CommandContext?> ParseArgumentsAsync(HttpConverterContext converterContext, InteractionCreateEventArgs eventArgs)
        {
            if (_extension is null)
            {
                return null;
            }

            Dictionary<CommandArgument, object?> parsedArguments = [];
            try
            {
                while (converterContext.NextArgument())
                {
                    IOptional optional = await Converters[converterContext.Argument.Type](converterContext, eventArgs);
                    if (!optional.HasValue)
                    {
                        break;
                    }

                    parsedArguments.Add(converterContext.Argument, optional.RawValue);
                }
            }
            catch (Exception error)
            {
                await _extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                {
                    Context = new HttpCommandContext()
                    {
                        Arguments = parsedArguments,
                        Channel = eventArgs.Interaction.Channel,
                        Command = converterContext.Command,
                        Extension = converterContext.Extension,
                        Interaction = eventArgs.Interaction,
                        Options = converterContext.Options,
                        ServiceScope = converterContext.ServiceScope,
                        User = eventArgs.Interaction.User,
                        HttpContext = converterContext.HttpContext
                    },
                    Exception = new ParseArgumentException(converterContext.Argument, error),
                    CommandObject = null
                });

                return null;
            }

            return new HttpCommandContext()
            {
                Arguments = parsedArguments,
                Channel = eventArgs.Interaction.Channel,
                Command = converterContext.Command,
                Extension = converterContext.Extension,
                Interaction = eventArgs.Interaction,
                Options = converterContext.Options,
                ServiceScope = converterContext.ServiceScope,
                User = eventArgs.Interaction.User,
                HttpContext = converterContext.HttpContext
            };
        }

        private async Task<AutoCompleteContext?> ParseAutoCompletesAsync(HttpConverterContext converterContext, InteractionCreateEventArgs eventArgs)
        {
            if (_extension is null)
            {
                return null;
            }

            Dictionary<CommandArgument, object?> parsedArguments = [];
            try
            {
                while (converterContext.NextArgument() && !converterContext.Options.ElementAt(converterContext.ArgumentIndex).Focused)
                {
                    IOptional optional = await Converters[converterContext.Argument.Type](converterContext, eventArgs);
                    if (!optional.HasValue)
                    {
                        break;
                    }

                    parsedArguments.Add(converterContext.Argument, optional.RawValue);
                }
            }
            catch (Exception error)
            {
                await _extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                {
                    Context = new HttpCommandContext()
                    {
                        Arguments = parsedArguments,
                        Channel = eventArgs.Interaction.Channel,
                        Command = converterContext.Command,
                        Extension = converterContext.Extension,
                        Interaction = eventArgs.Interaction,
                        Options = converterContext.Options,
                        ServiceScope = converterContext.ServiceScope,
                        User = eventArgs.Interaction.User,
                        HttpContext = converterContext.HttpContext
                    },
                    Exception = new ParseArgumentException(converterContext.Argument, error),
                    CommandObject = null
                });

                return null;
            }

            return new AutoCompleteContext()
            {
                Arguments = parsedArguments,
                AutoCompleteArgument = converterContext.Argument,
                Channel = eventArgs.Interaction.Channel,
                Command = converterContext.Command,
                Extension = converterContext.Extension,
                Interaction = eventArgs.Interaction,
                Options = converterContext.Options,
                ServiceScope = converterContext.ServiceScope,
                User = eventArgs.Interaction.User,
                UserInput = converterContext.Options.ElementAt(converterContext.ArgumentIndex).Value
            };
        }
    }
}
