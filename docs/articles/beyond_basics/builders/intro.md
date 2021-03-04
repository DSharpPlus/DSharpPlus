---
uid: beyond_basics_builder_intro
title: Builder Intro
---

## Background
Prior to 4.x and even through development of 4.x, we reconized that we had a problem.  This was that
many of our methods were not consistant and/or had too many params to them (even the optional params).
Due to this, we decided to clean both our public API and internal API by switching to using the builder 
pattern.  With this we can be consistant on what our Discord facing methods will accept and when we introduce
features that Discord has also implemented, we can do so in a clean manner.

## Current Builders
All of our Builders are dedicated to either Create or Modify and will be named as such.  However, both of these builders
will share a common base for code that is shared between the two.  Below is a list of our current builders:

1.  [Message Builder](xref:beyond_basics_builder_messagebuilder)
	1.  There is a dedicated @DSharpPlus.Entities.DiscordMessageCreateBuilder and a @DSharpPlus.Entities.DiscordMessageModifyBuilder
2.  [Guild Builder](xref:beyond_basics_builder_guildbuilder)
	1.  There is a dedicated @DSharpPlus.Entities.DiscordGuildCreateBuilder and a @DSharpPlus.Entities.DiscordGuildModifyBuilder
3.  [Channel Builder](xref:beyond_basics_builder_channelbuilder)
	1.  There is a dedicated @DSharpPlus.Entities.DiscordChannelCreateBuilder and a @DSharpPlus.Entities.DiscordChannelModifyBuilder