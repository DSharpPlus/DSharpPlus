# Pre

First we will start off with the code found in the first tutorial ([found here](http://dhsarpplus.readthedocs.io/))

```
class Program {
  static void Main(string[] args)
    {
        DiscordClient client = new DiscordClient("INSERT YOUR TOKEN HERE", true);

        client.MessageReceived += (sender, e) => // Channel message has been received
        {
            e.Channel.SendMessage(e.MessageText);
        }

        Console.WriteLine("Attempting to connect!");

        try
        {
            client.SendLoginRequest();
            client.Connect();
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        client.Connected += (sender, e) =>
        {
            Console.WriteLine("CLIENT CONNECTED");
        };

        Console.ReadLine();
        Environment.Exit(0);
    }
}
```
