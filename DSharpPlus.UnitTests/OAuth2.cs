using DSharpPlus.Entities;
using DSharpPlus.Rest;
using DSharpPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DSharpPlus.UnitTests
{
	public class OAuth2
	{
		private static Scope[] DefaultScopes = new Scope[] { Scope.email, Scope.identify, Scope.messages_read, Scope.guilds_join, Scope.guilds };
		[Fact]
		public async Task GetToken()
		{
			var s = await DiscordRestClient.ClientCredentialsGrantAsync(SECRET.CLIENT_ID, SECRET.CLIENT_SECRET, DefaultScopes);
			Assert.True(s != null && !string.IsNullOrWhiteSpace(s.Token));
		}

		[Fact]
        public async Task Create()
        {
			var client = DiscordRestClient.ClientCredentialsGrantAsync(SECRET.CLIENT_ID, SECRET.CLIENT_SECRET, DefaultScopes);


			//Cleanup
			client.Dispose();
        }

		[Fact]
		public async Task GetCache()
		{
			var client = await DiscordRestClient.ClientCredentialsGrantAsync(SECRET.CLIENT_ID, SECRET.CLIENT_SECRET, DefaultScopes);

			await client.InitializeCacheAsync();

			//Cleanup
			client.Dispose();
		}

		[Fact]
		public async Task AddUserToGuild()
		{
			var client = await DiscordRestClient.ClientCredentialsGrantAsync(SECRET.CLIENT_ID, SECRET.CLIENT_SECRET, DefaultScopes);

			await client.InitializeCacheAsync();

			var bot = new DiscordClient(new DiscordConfiguration()
			{
				TokenType = TokenType.Bot,
				Token = SECRET.BotToken,
				AutomaticGuildSync = true,
				AutoReconnect = true
			});
			bot.Ready += this.Bot_Ready;
			await bot.ConnectAsync();
			Ready.WaitOne();
			Ready.Reset();

			if (bot.Guilds.Count >= 5) //Technically 10, but we limit to 5
			{
				while (bot.Guilds.Count != 0)
					bot.Guilds.First().Value.DeleteAsync().Wait();
			}

			var testG = await bot.CreateGuildAsync("Test Guild");
			var user = client.CurrentUser;
			var adminrole = await testG.CreateRoleAsync("Admin", Permissions.Administrator, DiscordColor.DarkRed);
			await testG.AddMemberAsync(user, client.Token);
			var member = await testG.GetMemberAsync(user.Id);
			await member.GrantRoleAsync(adminrole, "Cause i can");
			//Just Leave it for the User :)
			//await testG.DeleteAsync();

			//Cleanup
			client.Dispose();
			await bot.DisconnectAsync();
			bot.Dispose();
		}
		ManualResetEvent Ready = new ManualResetEvent(false);
		private async Task Bot_Ready(EventArgs.ReadyEventArgs e)
		{
			Ready.Set();
		}

		[Fact]
		public async Task GetUserInfo()
		{
			var client = await DiscordRestClient.ClientCredentialsGrantAsync(SECRET.CLIENT_ID, SECRET.CLIENT_SECRET, DefaultScopes);

			await client.InitializeCacheAsync();

			var user = client.CurrentUser;

			//Cleanup
			client.Dispose();
		}
	}
}
