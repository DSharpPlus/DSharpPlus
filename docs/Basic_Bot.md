# The Intial Code

We will start with the code from the [homepage](http://dsharpplus.readthedocs.io/):

```cs
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

```cs
		Console.ReadLine();
		Environment.Exit(0);
```

The code above makes it so that the program only closes if you press the `Enter` button. The program should look like this now: 

```cs
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

# Bots

## Echoing messages

Now we will get the bot to repeat any message it recieves on any channel its on.

To start that off, put in the below code under the `Console.WriteLine("Attempting to connect!");` line.

```cs
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

```cs
class Program {
  static void Main(string[] args)
	{
        DiscordClient client = new DiscordClient("INSERT YOUR TOKEN HERE", true);

		client.MessageReceived += (sender, e) => // Channel message has been received
        {
			if(!e.Message.Author.IsBot) {
				e.Channel.SendMessage(e.MessageText);
			}
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

## Response

So now we will take the code from before we added the echoing.
```cs
class Program {
  static void Main(string[] args)
	{
        DiscordClient client = new DiscordClient("INSERT YOUR TOKEN HERE", true);

	client.MessageReceived += (sender, e) => // Channel message has been received
        {
			
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
Inside the message recieved event, we will add this code:

`if e.MessageText.StartsWith(".ping"`)

If you remember from before, `e` stands for the event while `MessageText` is the text of the sent message. Now, inside that if statement we will enter this code: `e.Channel.SendMessage("Pong, " + e.User.Mention);`. 

The code above has 2 parts. `e.Channel.SendMessage();` sends the message to the same channel as `MessageText`, while `e.User.Mention` is a value that is the mention of the user. It is basically `<@userid>` which is the actual raw text of a mention. Now you have code that will both respond with a combination of a string and an event value. There are multiple event values that are public, these public values can be found on the public XML docs [here](http://afroraydude.pw/sharpcord) (it is out of date).
