
using System.Configuration;

namespace ToffyDiscord.App;

public static class Startup
{
    public static readonly string BotToken;
    public static readonly string Host;
    public static readonly int Port;



    static Startup()
    {

        BotToken = ConfigurationManager.AppSettings["TOFFY_DISCORD_BOT_TOKEN"];
        Host = ConfigurationManager.AppSettings["TOFFY_DISCORD_HOST"];
        Port = Convert.ToInt32(ConfigurationManager.AppSettings["TOFFY_DISCORD_PORT"]);

    }


}
