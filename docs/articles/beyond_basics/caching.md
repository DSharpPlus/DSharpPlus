---
uid: articles.beyond_basics.caching
title: DSharpPlus Caching
---

# Caching

DSharpPlus has a built-in caching system which will cache most objects received from Discord. This is done to reduce
the amount of API calls made to Discord, and to provide a more consistent experience for the end user. 

## Configuration

D#+ caching can be configured via `DiscordConfiguration` object. The following properties are available:

* `CacheConfiguration` - This config is used by the default in-memory cache. The default in-memory cache only provides 
   time-based expiration, and is not persistent. This cache is used by default.
* `CacheProvider` - You can use this property to provide your own cache provider. This provider must implement the
   `IDiscordCache` interface. You can use this to provide your own cache implementation.

The CacheConfiguration has a dictionary of types and their respective cache configurations. You can use this to configure
the cache for specific types. If you want a certain type to not be cached, you can simply remove the type from the dictionary.
In your own cache provider, you can simple do nothing in the set and remove method and return `null` if you dont cache
the type.

## Caching behavior

D#+ will atempt to cache following types:

* `DiscordGuild`
* `DiscordChannel`
* `DiscordMessage`
* `DiscordMember`
* `DiscordUser`
* `DiscordPresence`

## Events

D#+ will use this cache to provide additional information in events. Most events only contains ids of objects, but 
D#+ will attempt to provide the full object if it is available in cache. You will see `CachedEntity<Tkey, TValue>` 
in this case - this will provide you the id of the object, and the full object if it was available in cache when parsing
the event.

## Create your own cache provider

You can create your own cache provider by implementing the `IDiscordCache` interface. D#+ supports asynchronous caching
and will await your cache provider methods. You can use this to provide your own caching implementation.


