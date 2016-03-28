using DiscordSharp.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YugiohPrices;

namespace Luigibot.Modules
{
    public class YugiohModules : IModule
    {
        public YugiohModules()
        {
            Name = "ygo";
            Description = "Yu-Gi-Oh searcher module.";
        }
        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("ygo", "Retrieves information for a Yu-Gi-Oh card from the YugiohPrices database.",
                "Card names are (unfortunately) case sensitive.\n\n**Valid:** Dark Magician\n**Invalid: **dark magician", PermissionType.User, 1, cmdArgs =>
                {
                    if (cmdArgs.Args.Count > 0)
                    {
                        YugiohPricesSearcher searcher = new YugiohPricesSearcher();
                        try
                        {
                            cmdArgs.Channel.SimulateTyping();
                            var card = searcher.GetCardByName(cmdArgs.Args[0]).Result;
                            if (card.Name != "<NULL CARD>")
                            {
                                card.CardImage.Save("ygotemp.png");
                                string message = $"**{card.Name}**";
                                if (card.Type == CardType.Monster)
                                    message += $" Level: {card.Level} Attribute: {card.Attribute}\n";
                                else
                                    message += "\n";
                                message += $"**Description:** {card.Description}";
                                if (card.Type == CardType.Monster)
                                    message += $"\n**Type:** {card.MonsterType}\n**ATK/DEF:** {card.Attack}/{card.Defense}";

                                manager.Client.AttachFile(cmdArgs.Channel, message, "ygotemp.png");
                            }
                            else
                                cmdArgs.Channel.SendMessage("Couldn't find that specified card!");
                        }
                        catch (AggregateException ex)
                        {
                            ex.Handle((x) =>
                            {
                                cmdArgs.Channel.SendMessage("Couldn't find that specified card! (" + x.Message + ")");
                                return true;
                            });
                        }

                    }
                }), this);
        }
    }
}
