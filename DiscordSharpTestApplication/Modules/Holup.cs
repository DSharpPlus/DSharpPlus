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
                if(e.Author.ID == "72465239836196864" || e.Author.ID == "78703938047582208") //if it's me or uniqu0
                {
                    if(e.Message.Content.Trim().ToLower() == "hol up" || e.Message.Content.Trim().ToLower() == "holup")
                    {
                        e.Channel.SendMessage("we dem");
                    }
                }
            };
        }

    }
}
