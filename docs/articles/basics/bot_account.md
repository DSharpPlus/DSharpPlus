---
uid: articles.basics.bot_account
title: Creating a Bot Account
---

# Creating a Bot Account

## Create an Application

Before you're able to create a [bot account][0] to interact with the Discord API, you'll need to create a new OAuth2
application. Go to the [Discord Developer Portal][1] and click `New Application` at the top right of the page.

![Discord Developer Portal][2]

You'll then be prompted to enter a name for your application.

![Naming Application][3]

The name of your application will be the name displayed to users when they add your bot to their Discord server. With
that in mind, it would be a good idea for your application name to match the desired name of your bot.

Enter your desired application name into the text box, then hit the `Create` button.

After you hit `Create`, you'll be taken to the application page for your newly created application.

![Application Page][4]

That was easy, wasn't it?

Before you move on, you may want to upload an icon for your application and provide a short description of what your bot
will do. As with the name of your application, the application icon and description will be displayed to users when
adding your bot.

If you want your bot to be private so that only you can add it to servers, make sure to set `Install Link` to `None` on the
installation page. Otherwise, you will get an error when turning off `Public Bot` on the bot page.

![Installation Page][5]

# Using Your Bot Account

## Invite Your Bot

Now that you have a bot account, you'll probably want to invite it to a server!

A bot account joins a server through a special invite link that'll take users through the OAuth2 flow; you'll probably
be familiar with this if you've ever added a public Discord bot to a server. To get the invite link for your bot, head
on over to the OAuth2 page of your application.

![OAuth2][6]

<br/>
We'll be using the *OAuth2 URL Generator* on this page. Simply tick `bot` under the *scopes* panel; your bot invite link
will be generated directly below.

![OAuth2 Scopes][7]

<br/>
By default, the generated link will not grant any permissions to your bot when it joins a new server. If your bot
requires specific permissions to function, you'd select them in the *bot permissions* panel.

![Permissions][8]

The invite link in the *scopes* panel will update each time you change the permissions. Be sure to copy it again after
any changes!

## Get Bot Token

Instead of logging in to Discord with a username and password, bot accounts use a long string called a *token* to
authenticate. You'll want to retrieve the token for your bot account so you can use it with DSharpPlus.

Head back to the bot page and click on `Reset Token`.

![Token Reset][9]

Confirm that you want to reset the token and enter your 2FA code when prompted.

![Token Confirmation][10]

Go ahead and copy your bot token and save it somewhere. You'll be using it soon!

![Token Copy][11]

>[!IMPORTANT]
> Handle your bot token with care! Anyone who has your token will have access to your bot account.
> Be sure to store it in a secure location and *never* give it to *anybody*.
>
> If you ever believe your token has been compromised, be sure to hit the `Reset Token` button (as seen above) to
> invalidate your old token and get a brand new token.

## Write Some Code

You've got a bot account set up and a token ready for use. Sounds like it's time for you to [write your first bot][12]!

<!-- LINKS -->
[0]: https://discord.com/developers/docs/topics/oauth2#bot-users
[1]: https://discord.com/developers/applications
[2]: ../../images/basics_bot_account_01.png "Creating an Application"
[3]: ../../images/basics_bot_account_02.png "Naming our new Application"
[4]: ../../images/basics_bot_account_03.png "Opening the Bot Page"
[5]: ../../images/basics_bot_account_04.png "Making the Bot Private"
[6]: ../../images/basics_bot_account_05.png "The OAuth2 Page"
[7]: ../../images/basics_bot_account_06.png "Scopes Panel"
[8]: ../../images/basics_bot_account_07.png "Permissions Panel"
[9]: ../../images/basics_bot_account_08.png "Resetting the Token"
[10]: ../../images/basics_bot_account_09.png "Confirming the Reset"
[11]: ../../images/basics_bot_account_10.png "Copying the new Token"
[12]: xref:articles.basics.first_bot
