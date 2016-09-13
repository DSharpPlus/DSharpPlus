# The Intial Code

We will start with the code from the [homepage](http://dsharpplus.readthedocs.io/):

```
using DSharpPlus;

class Program {
  static void Main(string[] args)
	{
        DiscordClient client = new DiscordClient("INSERT YOUR TOKEN HERE", true);

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
	}
}
``` 

As we stated before, the code only connects the bot to the Discord API servers. But we want to get the bot to send a message. So let's start off with adding a few lines under the `Main` method.

# Closing the bot, correctly

You want to add the following code to the end of your `Main` method

```
		Console.ReadLine();
		Environment.Exit(0);
```

The code above makes it so that the program only closes if you press the `Enter` button. The program should look like this now: 

```
using DSharpPlus;

class Program {
  static void Main(string[] args)
	{
        DiscordClient client = new DiscordClient("INSERT YOUR TOKEN HERE", true);

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

# Echoing messages

Now we will get the bot to repeat any message it recieves on any channel its on.

To start that off, put in the below code under the `Console.WriteLine("Attempting to connect!");` line.

```
		client.MessageReceived += (sender, e) => // Channel message has been received
        {
			e.Channel.SendMessage(e.MessageText);
		}
```

Now lets explain the parts

`client.MessageReceived` is the event that happens whenever a message is received on a server the bot is on. 

The `e` in `(sender, e)` is the individual event.

`e.Channel.SendMessage(e.MessageText);` Sends the string `e.MessageText` synchronously.

`e.MessageText` is the text of the message that was recieved.

Your program should now look like this: 

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

Now anytime you send a message, the bot should repeat what you said.