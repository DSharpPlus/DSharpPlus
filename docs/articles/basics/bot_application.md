# Creating an application for your bot

Before you can begin making your DSharpPlus bots, you need to create OAuth2 applications and bot users for them, so 
they can interact with users.

It is recommended you keep a notepad or any other text editor open so you can note down the important things.

## 1. Creating an OAuth2 application

Head over to [My apps](https://discordapp.com/developers/applications/me "Discord Developers - My Apps") on Discord Developers 
website. Once there, press the big button with a **+** symbol on it. The one that says **New App**.

![Step 1](/images/01_01_new_app.png "New application")

## 2. Giving it an identity

You will be presented with a form that asks for application's name, redirect URIs, description, and avatar.

You want to fill in the name<sup>1</sup> and description<sup>2</sup>. If you want, you can also give it an avatar<sup>3</sup>.

When you're ready, press the **Create App** button below the form.

![Step 2](/images/01_02_app_data.png "Identity")

## 3. Creating a bot user

Once the application is created, you will need to create a bot user for the application, so the users can interact with it.

Press the **Create a Bot User** button, and in the dialog that pops up, select **Yes, do it!**

![Step 3](/images/01_03_make_bot.png "How make bot")

## 4. Bot's credentials

Once this is done, a section called **APP BOT USER** will appear. Find a field called **Token** in it, and press 
**click to reveal** next to it to get your bot's token. **Note this token down**, you will need it later.

Another thing you also want to note down, is the **Client ID**. You will need that in a moment.

You can also check the **Public Bot** checkbox, if you're so inclined. Once this is all done, press the **Save changes** 
button on the bottom. Congratulations. Your bot can now interact with people, although there's still a long way before it 
actually does.

![Step 4](/images/01_04_settings.png "Almost there")

## 5. Inviting it to your guild

So the bot exists, but it can't talk anywhere yet. You will want to invite it to your guild. This is done by following an 
invite link. An invite link generally looks like this:

`https://discordapp.com/oauth2/authorize?client_id=YOUR_CLIENT_ID&scope=bot&permissions=DESIRED_PERMISSIONS`

For now, copy the link, substitute `DESIRED_PERMISSIONS` with 0, and `YOUR_CLIENT_ID` with the Client ID you noted down 
earlier. Paste it in your browser, select your bot from the dropdown, press **Authorize**, and the bot should appear in 
your server.

If you want to calculate the permissions, I recommend using a permission calculator. My personal recommendation is 
[FiniteReality's Permission Calculator](https://finitereality.github.io/permissions/), which can also generate invite 
links.

## 6. Making it come online

Now that this is all completed, head over to [Basic bot](/articles/getting_started/basic_bot.html "Basic bot") to make the bot come online.
