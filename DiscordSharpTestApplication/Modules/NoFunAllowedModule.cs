using DiscordSharp.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot.Modules
{
    public class NoFunAllowedModule : IModule
    {
        private string[] KhaledQuotes = new string[]
        {
            "Always have faith. Always have hope.",
            "The key is to make it.",
            "Another one.",
            "Key to success is clean heart and clean face.",
            "Smh they get mad when you have joy.",
            "Baby, you smart. I want you to film me taking a shower.",
            "You smart! You loyal! You a genius!",
            "Give thanks to the most high.",
            "They will try to close the door on you, just open it.",
            "They don’t want you to have the No. 1 record in the country.",
            "Those that weather the storm are the great ones.",
            "The key to success is more cocoa butter.",
            "I changed... a lot.",
            "My fans expect me to be greater and keep being great.",
            "There will be road blocks but we will overcome it.",
            "They don't want you to jet ski.",
            "Them doors that was always closed, I ripped the doors off, took the hinges off. And when I took the hinges off, I put the hinges on the fuckboys’ hands.",
            "Congratulations, you played yourself.",
            "Don't play yourself.",
            "Another one, no. Another two, drop two singles at a time.",
        };
        private string[] EightballMessages = new string[] 
        {
            "Signs point to yes.",
            "Yes.",
            "Reply hazy, try again.",
            "Without a doubt",
            "My sources say no",
            "As I see it, yes.",
            "You may rely on it.",
            "Concentrate and ask again",
            "Outlook not so good",
            "It is decidedly so",
            "Better not tell you now.",
            "Very doubtful",
            "Yes - definitely",
            "It is certain",
            "Cannot predict now",
            "Most likely",
            "Ask again later",
            "My reply is no",
            "Outlook good",
            "Don't count on it"
        };
        private string[] FEmojis = new string[]
        {
            "💩","🍆","👌","`lol`","😛","💀","🎆", "😏", "🖕", "💀🎺🎺"
        };

        public NoFunAllowedModule()
        {
            Name = "fun";
            Description = "Contains what would be considered \"fun\" things. I don't know, I don't have fun.";
        }

        public override void Install(CommandsManager manager)
        {
            //manager.Client will return the associated discord client
            manager.AddCommand(new CommandStub("orange", "Orangifies your text.", "", PermissionType.User, 1, cmdArgs =>
            {
                cmdArgs.Channel.SendMessage($"```fix\n{cmdArgs.Args[0]}\n```");
            }), this);
            manager.AddCommand(new CommandStub("f", "Pay respect.", "Press f", PermissionType.User, cmdArgs =>
            {
                cmdArgs.Channel.SendMessage($"{cmdArgs.Author.Username} has paid their respects. {FEmojis[manager.rng.Next(0, FEmojis.Length - 1)]}");
            }), this);
            manager.AddCommand(new CommandStub("nf", "Pay no respect.", "Press nf", PermissionType.User, cmdArgs =>
            {
                cmdArgs.Channel.SendMessage($"{cmdArgs.Author.Username} refuses to pay respect. {FEmojis[manager.rng.Next(0, FEmojis.Length - 1)]}");
            }), this);
            manager.AddCommand(new CommandStub("8ball", "Have your fortune told.", "8ball <your message here>", PermissionType.User, cmdArgs =>
            {
                manager.rng.Next(0, EightballMessages.Length);
                manager.rng.Next(0, EightballMessages.Length);
                int index = manager.rng.Next(0, EightballMessages.Length);
                cmdArgs.Channel.SendMessage($"<@{cmdArgs.Author.ID}>: **{EightballMessages[index]}**");
            }), this);
            manager.AddCommand(new CommandStub("42", "..", "...", PermissionType.User, cmdArgs =>
            {
                cmdArgs.Channel.SendMessage("The answer to life, the universe, and everything.");
            }), this);
            manager.AddCommand(new CommandStub("khaled", "Anotha one.", "", cmdArgs =>
            {
                if (manager.rng == null)
                {
                    Console.WriteLine("RNG null?!");
                    manager.rng = new Random((int)DateTime.Now.Ticks);
                }
                cmdArgs.Channel.SendMessage($"***{KhaledQuotes[manager.rng.Next(0, KhaledQuotes.Length - 1)]}***");
            }), this);
        }
    }
}
