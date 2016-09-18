using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using System.IO;
using DSharpPlus.Github;
using DSharpPlus.Github.Modules;
using DSharpPlus.GitHub;

namespace DSharpPlus.Github
{
    class TestBot
    {
        static void Main(string[] args)
        {
            string botToken = "";
            DiscordClient client = new DSharpPlus.DiscordClient(botToken, true);

            try
            {
                client.SendLoginRequest();
                client.Connect();

            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            client.MessageReceived += (sender, e) =>
            {
                if (e.MessageText.StartsWith(";repo"))
                {
                    string otherStuff = e.MessageText.Substring(6);
                    string user = otherStuff.Split('/').First();
                    string repo = otherStuff.Split('/').Last();

                    DSharpPlusGithubClient ghclient = new DSharpPlusGithubClient(true, "default-dsharp-client");

                    Issues issues = new Issues(ghclient.ghclient);
                    e.Channel.SendMessageAsync(issues.GetActiveIssuesOfRepo(user, repo).ToString());
                }
                else
                {
                    //Console.WriteLine("I won't be doing anything as I don't understand the message/command");
                }
            };

            Console.ReadLine();
            Environment.Exit(0);

        }
    }
}