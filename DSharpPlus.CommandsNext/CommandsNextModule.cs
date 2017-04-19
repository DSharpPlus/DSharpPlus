using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// This is the class which handles command registration, management, and execution. 
    /// </summary>
    public class CommandsNextModule : IModule
    {
        #region Events
        /// <summary>
        /// Triggered whenever a command executes successfully.
        /// </summary>
        public event AsyncEventHandler<CommandExecutedEventArgs> CommandExecuted
        {
            add { this._executed.Register(value); }
            remove { this._executed.Unregister(value); }
        }
        private AsyncEvent<CommandExecutedEventArgs> _executed = new AsyncEvent<CommandExecutedEventArgs>();

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event AsyncEventHandler<CommandErrorEventArgs> CommandErrored
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<CommandErrorEventArgs> _error = new AsyncEvent<CommandErrorEventArgs>();

        /// <summary>
        /// Triggered whenever a new command is registered.
        /// </summary>
        public event AsyncEventHandler<CommandEventArgs> CommandRegistered
        {
            add { this._registered.Register(value); }
            remove { this._registered.Unregister(value); }
        }
        private AsyncEvent<CommandEventArgs> _registered = new AsyncEvent<CommandEventArgs>();

        private async Task OnCommandExecuted(CommandExecutedEventArgs e) =>
            await this._executed.InvokeAsync(e).ConfigureAwait(false);

        private async Task OnCommandErrored(CommandErrorEventArgs e) =>
            await this._error.InvokeAsync(e).ConfigureAwait(false);

        private async Task OnCommandRegistered(CommandEventArgs e) =>
            await this._registered.InvokeAsync(e).ConfigureAwait(false);
        #endregion

        #region DiscordClient registration
        /// <summary>
        /// Gets the instance of <see cref="DiscordClient"/> for which this module is registered.
        /// </summary>
        public DiscordClient Client { get { return this.Client; } }
        private DiscordClient _client;

        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="InvalidOperationException"/>
        public void Setup(DiscordClient client)
        {
            if (this._client != null)
                throw new InvalidOperationException("What did I tell you?");

            this._client = client;
            this.Client.AddModule(this);
            this.Client.MessageCreated += this.HandleCommands;
        }
        #endregion

        #region Command Handler
        private async Task HandleCommands(MessageCreateEventArgs e)
        {
            // Let the bot do its things
            await Task.Yield();


        }
        #endregion
    }
}
