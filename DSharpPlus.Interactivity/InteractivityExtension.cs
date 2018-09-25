using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity
{
    #region Extensions
    public static partial class InteractivityExtensionMethods
    {
        public static InteractivityExtension UseInteractivity(this DiscordClient c, InteractivityConfiguration cfg)
        {
            if (c.GetExtension<InteractivityExtension>() != null)
                throw new Exception("Interactivity module is already enabled for this client!");

            var m = new InteractivityExtension(cfg);
            c.AddExtension(m);
            return m;
        }

        public static async Task<IReadOnlyDictionary<int, InteractivityExtension>> UseInteractivityAsync(this DiscordShardedClient c, InteractivityConfiguration cfg)
        {
            var modules = new Dictionary<int, InteractivityExtension>();
            await c.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in c.ShardClients.Select(xkvp => xkvp.Value))
            {
                var m = shard.GetExtension<InteractivityExtension>();
                if (m == null)
                    m = shard.UseInteractivity(cfg);

                modules[shard.ShardId] = m;
            }

            return new ReadOnlyDictionary<int, InteractivityExtension>(modules);
        }

        public static InteractivityExtension GetInteractivity(this DiscordClient c)
        {
            return c.GetExtension<InteractivityExtension>();
        }

        public static IReadOnlyDictionary<int, InteractivityExtension> GetInteractivity(this DiscordShardedClient c)
        {
            var modules = new Dictionary<int, InteractivityExtension>();

            c.InitializeShardsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (var shard in c.ShardClients.Select(xkvp => xkvp.Value))
                modules.Add(shard.ShardId, shard.GetExtension<InteractivityExtension>());

            return new ReadOnlyDictionary<int, InteractivityExtension>(modules);
        }

        public static IEnumerable<string> Split(this string str, int chunkSize)
        {
            var len = str.Length;
            var i = 0;

            while (i < len)
            {
                var size = Math.Min(len - i, chunkSize);
                yield return str.Substring(i, size);
                i += size;
            }
        }
    }
    #endregion

    public class InteractivityExtension : BaseExtension
	{
		private InteractivityConfiguration Config { get; }

		internal InteractivityExtension(InteractivityConfiguration cfg)
		{
			this.Config = new InteractivityConfiguration(cfg);
		}

		protected internal override void Setup(DiscordClient client)
		{
			this.Client = client;
		}

		#region Message
        /// <summary>
        /// Waits for a message to be received
        /// </summary>
        /// <param name="predicate">Expected predicate</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<MessageContext> WaitForMessageAsync(Func<DiscordMessage, bool> predicate, TimeSpan? timeoutoverride = null)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var tsc = new TaskCompletionSource<MessageContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			try
			{
				this.Client.MessageCreated += Handler;
				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageCreated -= Handler;
			}

			#region Handler
			async Task Handler(MessageCreateEventArgs e)
			{
				await Task.Yield();
				if (predicate(e.Message))
				{
					var mc = new MessageContext()
					{
						Interactivity = this,
						Message = e.Message
					};
					tsc.TrySetResult(mc);
					return;
				}
			}
			#endregion
		}
		#endregion

		#region Reaction
        /// <summary>
        /// Waits for a reaction to be received
        /// </summary>
        /// <param name="predicate">Expected predicate</param>
        /// <param name="timeoutoverride">TImeout override</param>
        /// <returns></returns>
		public async Task<ReactionContext> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, TimeSpan? timeoutoverride = null)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var tsc = new TaskCompletionSource<ReactionContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			try
			{
				this.Client.MessageReactionAdded += Handler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageReactionAdded -= Handler;
			}

			#region Handler
			async Task Handler(MessageReactionAddEventArgs e)
			{
				await Task.Yield();
				if (predicate(e.Emoji))
				{
					var rc = new ReactionContext()
					{
						Channel = e.Channel,
						Emoji = e.Emoji,
						Message = e.Message,
						User = e.User,
						Interactivity = this
					};
					tsc.TrySetResult(rc);
					return;
				}
			}
			#endregion
		}

        /// <summary>
        /// Wait for a reaction by a specific user to be received
        /// </summary>
        /// <param name="predicate">Expected predicate</param>
        /// <param name="user">User that sends the reaction</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<ReactionContext> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordUser user, TimeSpan? timeoutoverride = null)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var user_id = user.Id;
			var tsc = new TaskCompletionSource<ReactionContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			try
			{
				this.Client.MessageReactionAdded += Handler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageReactionAdded -= Handler;
			}

			#region Handler
			async Task Handler(MessageReactionAddEventArgs e)
			{
				await Task.Yield();
				if (predicate(e.Emoji))
				{
					if (e.User.Id == user_id)
					{
						var rc = new ReactionContext()
						{
							Channel = e.Channel,
							Emoji = e.Emoji,
							Message = e.Message,
							User = e.User,
							Interactivity = this
						};
						tsc.TrySetResult(rc);
						return;
					}
				}
			}
			#endregion
		}

        /// <summary>
        /// Wait for a reaction on a specific message by a specific user
        /// </summary>
        /// <param name="predicate">Expected predicate</param>
        /// <param name="message">Message reaction has to be placed on</param>
        /// <param name="user">User reaction was sent by</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<ReactionContext> WaitForMessageReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordMessage message, DiscordUser user = null, TimeSpan? timeoutoverride = null)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var message_id = message.Id;
			var tsc = new TaskCompletionSource<ReactionContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			try
			{
				this.Client.MessageReactionAdded += Handler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageReactionAdded -= Handler;
			}

			#region Handler
			async Task Handler(MessageReactionAddEventArgs e)
			{
				await Task.Yield();
				if (predicate(e.Emoji))
				{
					if (e.Message.Id == message_id)
					{
						if (user == null || e.User.Id == user?.Id)
						{
							var rc = new ReactionContext()
							{
								Channel = e.Channel,
								Emoji = e.Emoji,
								Message = e.Message,
								User = e.User,
								Interactivity = this
							};
							tsc.TrySetResult(rc);
							return;
						}
					}
				}
			}
			#endregion
		}

        /// <summary>
        /// Wait for any reaction on a specific message
        /// </summary>
        /// <param name="message">Message to check</param>
        /// <param name="user">(optional) User override</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<ReactionContext> WaitForMessageReactionAsync(DiscordMessage message, DiscordUser user = null, TimeSpan? timeoutoverride = null)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var message_id = message.Id;
			var tsc = new TaskCompletionSource<ReactionContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			try
			{
				this.Client.MessageReactionAdded += Handler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageReactionAdded -= Handler;
			}

			#region Handler
			async Task Handler(MessageReactionAddEventArgs e)
			{
				await Task.Yield();
				if (e.Message.Id == message_id)
				{
					if (user == null || e.User.Id == user?.Id)
					{
						var rc = new ReactionContext()
						{
							Channel = e.Channel,
							Emoji = e.Emoji,
							Message = e.Message,
							User = e.User,
							Interactivity = this
						};
						tsc.TrySetResult(rc);
						return;
					}
				}
			}
			#endregion
		}

        /// <summary>
        /// Creates a poll
        /// </summary>
        /// <param name="message">Message poll belongs to</param>
        /// <param name="emojis">Emojis to poll</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<ReactionCollectionContext> CreatePollAsync(DiscordMessage message, IEnumerable<DiscordEmoji> emojis, TimeSpan? timeoutoverride = null)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));
			if (emojis == null)
				throw new ArgumentNullException(nameof(emojis));
			if (emojis.Count() < 1)
				throw new InvalidOperationException("A minimum of one emoji is required to execute this method!");

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			foreach (var em in emojis)
			{
				await message.CreateReactionAsync(em).ConfigureAwait(false);
			}

			var rcc = new ReactionCollectionContext();
			var tsc = new TaskCompletionSource<ReactionCollectionContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(rcc));

			try
			{
				this.Client.MessageReactionAdded += ReactionAddHandler;
				this.Client.MessageReactionRemoved += ReactionRemoveHandler;
				this.Client.MessageReactionsCleared += ReactionClearHandler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageReactionAdded -= ReactionAddHandler;
				this.Client.MessageReactionRemoved -= ReactionRemoveHandler;
				this.Client.MessageReactionsCleared -= ReactionClearHandler;
			}

			#region Handlers
			async Task ReactionAddHandler(MessageReactionAddEventArgs e)
			{
				if (e.Message.Id != message.Id || e.Client.CurrentUser.Id == e.User.Id)
					return;

				await Task.Yield();
				if (emojis.Count(x => x == e.Emoji) > 0)
				{
					if (rcc._membersvoted.Contains(e.User.Id)) // don't allow to vote twice
						await e.Message.DeleteReactionAsync(e.Emoji, e.User);
					else
						rcc.AddReaction(e.Emoji, e.User.Id);
				}
				else
				{
					// remove unrelated reactions
					await e.Message.DeleteReactionAsync(e.Emoji, e.User);
				}
			}

			async Task ReactionRemoveHandler(MessageReactionRemoveEventArgs e)
			{
				if (e.Client.CurrentUser.Id != e.User.Id)
				{
					await Task.Yield();
					if (e.Message.Id == message.Id && emojis.Count(x => x == e.Emoji) > 0)
					{
						rcc.RemoveReaction(e.Emoji, e.User.Id);
					}
				}
			}

			async Task ReactionClearHandler(MessageReactionsClearEventArgs e)
			{
				await Task.Yield();
				if (e.Message.Id == message.Id)
				{
					rcc.ClearReactions();
					foreach (var em in emojis)
					{
						await message.CreateReactionAsync(em).ConfigureAwait(false);
					}
				}
			}
			#endregion
		}

        /// <summary>
        /// Collects all reactions on a message
        /// </summary>
        /// <param name="message">Message to check</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<ReactionCollectionContext> CollectReactionsAsync(DiscordMessage message, TimeSpan? timeoutoverride = null)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var rcc = new ReactionCollectionContext();
			var tsc = new TaskCompletionSource<ReactionCollectionContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(rcc));

			try
			{
				this.Client.MessageReactionAdded += ReactionAddHandler;
				this.Client.MessageReactionRemoved += ReactionRemoveHandler;
				this.Client.MessageReactionsCleared += ReactionClearHandler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageReactionAdded -= ReactionAddHandler;
				this.Client.MessageReactionRemoved -= ReactionRemoveHandler;
				this.Client.MessageReactionsCleared -= ReactionClearHandler;
			}

			#region Handlers
			async Task ReactionAddHandler(MessageReactionAddEventArgs e)
			{
				await Task.Yield();
				if (e.Message.Id == message.Id)
				{
					rcc.AddReaction(e.Emoji);
				}
			}

			async Task ReactionRemoveHandler(MessageReactionRemoveEventArgs e)
			{
				await Task.Yield();
				if (e.Message.Id == message.Id)
				{
					rcc.RemoveReaction(e.Emoji, e.User.Id);
				}
			}

			async Task ReactionClearHandler(MessageReactionsClearEventArgs e)
			{
				await Task.Yield();
				if (e.Message.Id == message.Id)
				{
					rcc.ClearReactions();
				}
			}
			#endregion
		}
		#endregion

		#region Typing
		// I don't really know anymore why I added this.. -Naam
		// I think I told you it might be useful, but tbh I have no idea myself -Emzi
		// Did you? I don't remember either. Nice it's there anyway I guess.. -Naam
        /// <summary>
        /// Waits for any user to start typing
        /// </summary>
        /// <param name="channel">Channel to check</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<TypingContext> WaitForTypingUserAsync(DiscordChannel channel, TimeSpan? timeoutoverride = null)
		{
			if (channel == null)
				throw new ArgumentNullException(nameof(channel));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var channel_id = channel.Id;
			var tsc = new TaskCompletionSource<TypingContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			try
			{
				this.Client.TypingStarted += Handler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.TypingStarted -= Handler;
			}

			#region Handler
			async Task Handler(TypingStartEventArgs e)
			{
				await Task.Yield();
				if (e.Channel.Id == channel_id)
				{
					var tc = new TypingContext()
					{
						Channel = e.Channel,
						Interactivity = this,
						StartedAt = e.StartedAt,
						User = e.User
					};
					tsc.TrySetResult(tc);
					return;
				}
			}
			#endregion
		}

        /// <summary>
        /// Waits for a user to type in any channel
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
		public async Task<TypingContext> WaitForTypingChannelAsync(DiscordUser user, TimeSpan? timeoutoverride = null)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			var user_id = user.Id;
			var tsc = new TaskCompletionSource<TypingContext>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			AsyncEventHandler<TypingStartEventArgs> handler = async (e) =>
			{
				await Task.Yield();
				if (e.User.Id == user_id)
				{
					var tc = new TypingContext()
					{
						Channel = e.Channel,
						Interactivity = this,
						StartedAt = e.StartedAt,
						User = e.User
					};
					tsc.TrySetResult(tc);
					return;
				}
			};

			try
			{
				this.Client.TypingStarted += handler;

				var result = await tsc.Task.ConfigureAwait(false);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.TypingStarted -= handler;
			}
		}
		#endregion

		#region Pagination
        /// <summary>
        /// Sends a paginated message
        /// </summary>
        /// <param name="channel">Channel to send message to</param>
        /// <param name="user">User that may interact with this paginated message</param>
        /// <param name="message_pages">Pages for this message</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <param name="timeoutbehaviouroverride">Timeout behaviour override</param>
        /// <param name="emojis">Pagination emoji override</param>
        /// <returns></returns>
		public async Task SendPaginatedMessage(DiscordChannel channel, DiscordUser user, IEnumerable<Page> message_pages, TimeSpan? timeoutoverride = null,
			TimeoutBehaviour? timeoutbehaviouroverride = null, PaginationEmojis emojis = null)
		{

			if (channel == null)
				throw new ArgumentNullException(nameof(channel));
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			if (message_pages == null)
				throw new ArgumentNullException(nameof(message_pages));
			if (message_pages.Count() < 1)
				throw new InvalidOperationException("This method can only be executed with a minimum of one page!");

			if (message_pages.Count() == 1)
			{
				await this.Client.SendMessageAsync(channel, string.IsNullOrEmpty(message_pages.First().Content) ? "" : message_pages.First().Content, embed: message_pages.First().Embed).ConfigureAwait(false);
				return;
			}

			TimeSpan timeout = Config.Timeout;
			if (timeoutoverride != null)
				timeout = (TimeSpan)timeoutoverride;

			TimeoutBehaviour timeout_behaviour = Config.PaginationBehavior;
			if (timeoutbehaviouroverride != null)
				timeout_behaviour = (TimeoutBehaviour)timeoutbehaviouroverride;

			List<Page> pages = message_pages.ToList();

			if (pages.Count() == 0)
				throw new ArgumentException("You need to provide at least 1 page!");

			var tsc = new TaskCompletionSource<string>();
			var ct = new CancellationTokenSource(timeout);
			ct.Token.Register(() => tsc.TrySetResult(null));

			DiscordMessage m = await this.Client.SendMessageAsync(channel, string.IsNullOrEmpty(pages.First().Content) ? "" : pages.First().Content, embed: pages.First().Embed).ConfigureAwait(false);
			PaginatedMessage pm = new PaginatedMessage()
			{
				CurrentIndex = 0,
				Pages = pages,
				Timeout = timeout
			};
			
			emojis = emojis ?? new PaginationEmojis(this.Client);

			await this.GeneratePaginationReactions(m, emojis).ConfigureAwait(false);

			try
			{
				this.Client.MessageReactionsCleared += ReactionClearHandler;
				this.Client.MessageReactionAdded += ReactionAddHandler;
				this.Client.MessageReactionRemoved += ReactionRemoveHandler;

				await tsc.Task.ConfigureAwait(false);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Client.MessageReactionsCleared -= ReactionClearHandler;
				this.Client.MessageReactionAdded -= ReactionAddHandler;
				this.Client.MessageReactionRemoved -= ReactionRemoveHandler;

				switch (timeout_behaviour)
				{
					case TimeoutBehaviour.Ignore:
						break;
					case TimeoutBehaviour.DeleteMessage:
						// deleting a message deletes all reactions anyway
						//await m.DeleteAllReactionsAsync().ConfigureAwait(false);
						await m.DeleteAsync().ConfigureAwait(false);
						break;
					case TimeoutBehaviour.DeleteReactions:
						await m.DeleteAllReactionsAsync().ConfigureAwait(false);
						break;
				}
			}

			#region Handlers
			async Task ReactionAddHandler(MessageReactionAddEventArgs e)
			{
				if (e.Message.Id == m.Id && e.User.Id != this.Client.CurrentUser.Id && e.User.Id == user.Id)
				{
					ct.Dispose();
					ct = new CancellationTokenSource(timeout);
					ct.Token.Register(() => tsc.TrySetResult(null));
					await this.DoPagination(e.Emoji, m, pm, ct, emojis).ConfigureAwait(false);
				}
			}

			async Task ReactionRemoveHandler(MessageReactionRemoveEventArgs e)
			{
				if (e.Message.Id == m.Id && e.User.Id != this.Client.CurrentUser.Id && e.User.Id == user.Id)
				{
					ct.Dispose();
					ct = new CancellationTokenSource(timeout);
					ct.Token.Register(() => tsc.TrySetResult(null));
					await this.DoPagination(e.Emoji, m, pm, ct, emojis).ConfigureAwait(false);
				}
			}

			async Task ReactionClearHandler(MessageReactionsClearEventArgs e)
			{
				await this.GeneratePaginationReactions(m, emojis).ConfigureAwait(false);
			}
			#endregion
		}

        /// <summary>
        /// Generates pages in embeds from a long input string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns></returns>
		public IEnumerable<Page> GeneratePagesInEmbeds(string input)
		{
			if (String.IsNullOrEmpty(input))
				throw new InvalidOperationException("You must provide a string that is not null or empty!");

			List<Page> result = new List<Page>();
			List<string> split = input.Split(2000).ToList();
			int page = 1;
			foreach (string s in split)
			{
				result.Add(new Page()
				{
					Embed = new DiscordEmbed()
					{
						Title = $"Page {page}",
						Description = s
					}
				});
				page++;
			}
			return result;
		}

        /// <summary>
        /// Generates pages in strings from a long input string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns></returns>
		public IEnumerable<Page> GeneratePagesInStrings(string input)
		{
			if (String.IsNullOrEmpty(input))
				throw new InvalidOperationException("You must provide a string that is not null or empty!");

			List<Page> result = new List<Page>();
			List<string> split = input.Split(1900).ToList();
			int page = 1;
			foreach (string s in split)
			{
				result.Add(new Page()
				{
					Content = $"**Page {page}:**\n\n" + s
				});
				page++;
			}
			return result;
		}

        /// <summary>
        /// Generates pagination reactions
        /// </summary>
        /// <param name="message">Message to attach reactions to</param>
        /// <param name="emojis">Emojis to attach</param>
        /// <returns></returns>
		public async Task GeneratePaginationReactions(DiscordMessage message, PaginationEmojis emojis)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			await message.CreateReactionAsync(emojis.SkipLeft).ConfigureAwait(false);
			await message.CreateReactionAsync(emojis.Left).ConfigureAwait(false);
			await message.CreateReactionAsync(emojis.Stop).ConfigureAwait(false);
			await message.CreateReactionAsync(emojis.Right).ConfigureAwait(false);
			await message.CreateReactionAsync(emojis.SkipRight).ConfigureAwait(false);
		}

        /// <summary>
        /// Does pagination (for custom handling)
        /// </summary>
        /// <param name="emoji">Emoji that was received</param>
        /// <param name="message">Message reaction belongs to</param>
        /// <param name="paginatedmessage">Paginated message</param>
        /// <param name="canceltoken">Cancellation token source</param>
        /// <param name="emojis">Pagination emoji collection</param>
        /// <returns></returns>
		public async Task DoPagination(DiscordEmoji emoji, DiscordMessage message, PaginatedMessage paginatedmessage, CancellationTokenSource canceltoken, PaginationEmojis emojis)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));
			if (paginatedmessage == null)
				throw new ArgumentNullException(nameof(paginatedmessage));
			if (emoji == null)
				throw new ArgumentNullException(nameof(emoji));
			if (canceltoken == null)
				throw new ArgumentNullException(nameof(canceltoken));

			#region The "good" shit
			// used brackets everywhere for the sake of consistency
			if (emoji == emojis.SkipLeft)
			{
				paginatedmessage.CurrentIndex = 0;
			}
			else if (emoji == emojis.Left)
			{
				if (paginatedmessage.CurrentIndex != 0)
					paginatedmessage.CurrentIndex--;
			}
			else if (emoji == emojis.Stop)
			{
				canceltoken.Cancel();
			}
			else if (emoji == emojis.Right)
			{
				if (paginatedmessage.CurrentIndex != paginatedmessage.Pages.Count() - 1)
					paginatedmessage.CurrentIndex++;
			}
			else if (emoji == emojis.SkipRight)
			{
				paginatedmessage.CurrentIndex = paginatedmessage.Pages.Count() - 1;
			}
			else
			{
				return;
			}

			await message.ModifyAsync((string.IsNullOrEmpty(paginatedmessage.Pages.ToArray()[paginatedmessage.CurrentIndex].Content)) ? "" : paginatedmessage.Pages.ToArray()[paginatedmessage.CurrentIndex].Content,
				embed: paginatedmessage.Pages.ToArray()[paginatedmessage.CurrentIndex].Embed ?? null).ConfigureAwait(false);
			#endregion
		}
		#endregion
	}

    /// <summary>
    /// Different kinds of behaviour on pagination timeout
    /// </summary>
	public enum TimeoutBehaviour
	{
		// is this actually needed?
		//Default, // ignore

        /// <summary>
        /// Interactivity ignores message after timeout. No actions are performed
        /// </summary>
		Ignore,

        /// <summary>
        /// Interactivity removes all reactions after timeout
        /// </summary>
		DeleteReactions,

        /// <summary>
        /// Interactivity deletes the message after timeout
        /// </summary>
		DeleteMessage
	}

    /// <summary>
    /// Information about the paginated message
    /// </summary>
	public class PaginatedMessage
	{
        /// <summary>
        /// Pages that belong to this message
        /// </summary>
		public IEnumerable<Page> Pages { get; internal set; }

        /// <summary>
        /// Messages current index
        /// </summary>
		public int CurrentIndex { get; internal set; }

        /// <summary>
        /// Messages timeout
        /// </summary>
		public TimeSpan Timeout { get; internal set; }
	}

    /// <summary>
    /// One page. This is essentially the "unpaginated" message. Usually you have multiple of these.
    /// </summary>
	public class Page
	{
        /// <summary>
        /// Regular text content
        /// </summary>
		public string Content { get; set; }

        /// <summary>
        /// Embed content
        /// </summary>
		public DiscordEmbed Embed { get; set; }
	}

}
// send nudes

// wait don't im not 18 yet

// I mean I don't mind..

// comeon send those nudes man

// 2 months and its legal

// send nudes

// nvm let's not
