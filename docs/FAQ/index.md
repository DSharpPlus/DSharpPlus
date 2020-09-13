---
uid: faq
title: FAQs
---

# FAQ's
Below are all the Frequently Asked Questions both within Github issues and the Discord Server

## I upgraded from stable to nightly and my project wont compile due to my Commands not being registered.
The upgrade path from stable to nightly does include many changes.  Luckily all of them have been 
documented in [here](xref:migration_3_4).

## I copied the code from the articles but my project does not compile.
Articles document the code at a point in time.  Due to this, articles should be used as a secondary reference
but if your code does not compile, please refer to the API Documentation.

## I deployed my bot that uses a mono platform and things are not working correctly.
That unfortuantly is a known problem, but is not one that the library will be correcting.  Mono itself is inheritly unstable
and has alot of flaws.  Due to this, please use a known working framework such as dotnet core that is cross-platform and highly stable.

## Im trying to connect to a Voice channel but its hanging or coming back with a `NullReferenceException`
With that you will have to verify that your Natives are correctly installed and VoiceNext/LavaLink is correctly configured.

## I keep getting heartbeat skipped warnings.
Occassionally seeing that can be perfectly okay to see.  However if its frequently happening, and in a consistant manner there is one
of two possible reason for that.  
1. Are your methods that are hooked to events long-lived and you could be blocking something?  If this is a possibility, please run everything in 
a seperate thread by `Task.Run`.
2.  You could have a issue with your connection between you and the Discord API Servers.  This could be your ISP, Discords ISP, or an ISP inbetween.  Please
work with your ISP to resolve the issue.  Nothing that we can do.

## What are the 4xx Errors and what do they mean.
Below are some of the common 400 Errors that Discord could return:
1. 401: The Token that was supplied was not correct.  This could be because your token was pasted incorrectly, your token became invalidated, your config file got blownup,
and many other reasons.  This is easily resolvable.  
2. 403: You do not have permission to do this action.  Please check your bots permissions and/or role heirarchy.
3. 404: The channel/message/user/webhook/etc could not be found.  If that entity really does exists you may want to incorperate a retry mechanism.  

## Why can I not alter x user or x role.
This is due to either Permissions of your bot or the heirarchy of your roles.  In order to alter another role/user, your role must be higher than the user's highest role or the role
that you are modifing.  

## Does CNext utilize Dependancy Injection
Yes and we have a lovely [article](xref:commands_dependency_injection) dedicated to this subject.

## I dont have a "bot" token but I have another token, can I use that
No you cannot.  DSharpPlus will not now nor ever help you break TOS.  In fact we try and help make sure you do not even come close to breaking TOS.  Due to this, things
like user tokens will not be allowed to be used.  

## Can my bot use custom statuses
Well that all depends what you call custom statuses.  With the true definition of it, no that is not available to bots.  However, you can have
the Playing xxxxxxx or Watching yyyyyyyy.  That can be set within @DSharpPlus.DiscordClient.ConnectAsync(DiscordActivity,System.Nullable{UserStatus},System.Nullable{System.DateTimeOffset}) or within @DSharpPlus.DiscordClient.UpdateStatusAsync(DiscordActivity,System.Nullable{UserStatus},System.Nullable{System.DateTimeOffset})

## I see that I can get a role by ID but can I get a role by name
There is no method like GetRoleByName or anything like that.  You can however look at the `Roles` Property on `DiscordGuild` and utilize LINQ to get the name.  

## Where are the awesome pictures of spiderman
![Spiderman](/images/spiderman.png "Spiderman Porn")