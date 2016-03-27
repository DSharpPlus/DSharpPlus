using DiscordSharp.Commands;
using System;

namespace Luigibot.Modules
{
    public class Holup : IModule
    {
        public Holup()
        {
            Name = "hol up";
            Description = "we dem boyz";
        }
        public override void Install(CommandsManager manager)
        {
            manager.Client.MessageReceived += (sender, e) =>
            {
                if(e.author.ID == "72465239836196864" || e.author.ID == "78703938047582208") //if it's me or uniqu0
                {
                    if(e.message.Content.Trim().ToLower() == "hol up" || e.message.Content.Trim().ToLower() == "holup")
                    {
                        e.Channel.SendMessage("we dem");
                    }
                }
            };
        }

    }
}
