---
uid: basics_bot_account
title: Creating a Bot Account
---

# Creating a Bot Account

## Create a Bot Application
Before you can create a bot account to interact with the Discord API, you'll need to create a new OAuth2 application from the [Discord Developer Portal](https://discord.com/developers/applications).
Once there, click `New Application` at the top right of the page.

![Discord Developer Portal](/images/01_01_new_app.png "New Application")

## Name Your Bot Application
You'll be prompted to enter a name for your application.<br/>

![Naming Application](/images/01_02_app_name.png "Naming Application")

The name of your application will be the name displayed to users when they add your bot to their Discord server.<br/>
With that in mind, it would be a good idea for your application name to match the desired name of your bot.<br/>
Enter your desired name into the text box, then hit the `Create` button.

After you hit `Create`, you'll be taken to the application page for your newly created application.

![Application Page](/images/01_03_app_page.png "Application Page")

Before you move on, you may want to upload an icon for your application and provide a short description of what your bot will do.
As with the name of your application, the application icon and description will be displayed to users when adding your bot.


## Add a Bot Account
Now that you have an application created, you'll be able to add a brand new bot account to it.<br/>
From your application page, click on `Bot` in the left panel. You'll be taken to the bot page for your application.

Click on the `Add Bot` button at the top right of the page

![Bot Page](/images/01_04_bot_page.png "Bot Page")

Then confirm the creation of the bot account.

![Creation Confirmation](/images/01_05_confirm_creation.png "Creation Confirmation")


# Using Your Bot Account

## Invite Your Bot
Now that you have a bot account, you'll probably want to invite it to a server. 

Bot accounts do not join servers using the invite links you're familiar with.<br/>
Instead, they join through a special link that'll take your users through the OAuth2 flow.<br/>
To get this special link, head on over to the OAuth2 page of your application.

![OAuth2](/images/01_06_oauth2.png "OAuth2 Page")

We'll be using the *OAuth2 URL Generator* on this page.<br/>
Simply tick `bot` under the *scopes* panel; your bot invite link will be generated directly below.

![OAuth2 Scopes](/images/01_07_scopes.png "OAuth2 Scopes")

By default, the generated link will not grant any permissions to your bot when it joins a new server.<br/>
If your bot requires specific permissions to function, you'd select them in the *bot permissions* panel.

![Permissions](/images/01_08_permissions.png "Permissions Panel")

The invite link in the *scopes* panel will update each time you change the permissions.<br/>
Be sure to copy it again after any changes!

## Retreiving Bot Credentials
Bot accounts authenticate with Discord using a token rather than a username and password.<br/>
To receive your bot token, head back to the bot page of your application and click on `Click to Reveal Token` just above the `Copy` and `Regenerate` buttons.

![Token Reveal](/images/01_09_reveal_token.png "Token Reveal")

You'll want to copy your token and keep it in a secure location; anybody who has access to your token will have access to your bot account.

## Add Functionality
Now that you have a bot account ready to go, you're ready to write your first lines of code!<br/>
Head on over to the [basic bot](xref:basics_basic_bot) article to get started.