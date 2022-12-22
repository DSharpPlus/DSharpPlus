// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;

namespace DSharpPlus.Tools.AutoUpdateChannelDescription;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? throw new InvalidOperationException("DISCORD_TOKEN environment variable is not set.");
        string guildId = Environment.GetEnvironmentVariable("DISCORD_GUILD_ID") ?? throw new InvalidOperationException("DISCORD_GUILD_ID environment variable is not set.");
        string channelId = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_ID") ?? throw new InvalidOperationException("DISCORD_CHANNEL_ID environment variable is not set.");
        string channelTopic = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_TOPIC") ?? throw new InvalidOperationException("DISCORD_DESCRIPTION environment variable is not set.");
        string latestStableVersion = args.Length == 1 ? args[0] : throw new InvalidOperationException("LATEST_STABLE_VERSION should be the first argument passed.");
        string nugetUrl = Environment.GetEnvironmentVariable("NUGET_URL") ?? throw new InvalidOperationException("NUGET_URL environment variable is not set.");
        string githubUrl = Environment.GetEnvironmentVariable("GITHUB_URL") ?? throw new InvalidOperationException("GITHUB_URL environment variable is not set.");

        if (latestStableVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            latestStableVersion = latestStableVersion[1..];
        }

        DiscordClient client = new(new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot
        });

        client.GuildDownloadCompleted += (client, eventArgs) =>
        {
            Entities.DiscordGuild guild = client.Guilds[ulong.Parse(guildId, NumberStyles.Number, CultureInfo.InvariantCulture)];
            Entities.DiscordChannel channel = guild.Channels[ulong.Parse(channelId, NumberStyles.Number, CultureInfo.InvariantCulture)];

            // Task.Run in case ratelimit gets hit and event handler is cancelled.
            _ = Task.Run(async () =>
            {
                try
                {
                    await channel.ModifyAsync(channel => channel.Topic = @$"{channelTopic}

{Formatter.Bold("GitHub")}: {githubUrl}
{Formatter.Bold("Latest stable version")}: {nugetUrl}/{latestStableVersion}
{Formatter.Bold("Latest preview version")}: {nugetUrl}/{typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}");
                }
                catch (DiscordException error)
                {
                    Console.WriteLine($"Error: HTTP {error.WebResponse.ResponseCode}, {error.WebResponse.Response}");
                    Environment.Exit(1);
                }
                Environment.Exit(0);
            });

            return Task.CompletedTask;
        };

        await client.ConnectAsync();

        // The program should exit ASAP after the channel description is updated.
        // However it may get caught in a ratelimit, so we'll wait for a bit.
        // The program will exit after 30 seconds no matter what.
        // This includes the time it takes to connect to the Discord gateway.
        await Task.Delay(TimeSpan.FromSeconds(30));
    }
}
