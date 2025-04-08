---
uid: articles.beyond_basics.permissions
title: Permissions
---

# Permissions

DSharpPlus implements permissions with two types, `enum DiscordPermission` and `struct DiscordPermissions`, as well as three implementation types we'll talk about in this article. This serves to allow permissions to scale indefinitely, at no impact to you, the user.

## The Difference between `DiscordPermission` and `DiscordPermissions`

`DiscordPermission` is an enum expressing names for specific permissions. It is used for attributes, which generally take an array of `DiscordPermission`, as well as to communicate permissions back to you. Each `DiscordPermission` can represent exactly one permission, and only its name is guaranteed to be constant - you should not rely on their underlying values and should treat them as opaque.

> [!WARNING]
> Since the underlying values may change at any time, performing any sort of math of them should be considered unsafe.

`DiscordPermissions`, on the other hand, expresses a set of permissions wherein each permission is either granted or not granted. It is possible, safe, and encouraged to perform math on this type and this type only. It exposes a number of methods that account for all special behaviour involved with permissions: `HasPermission` will not only check for the specified permission, but also for Administrator.

## Querying and Manipulating Permissions

`DiscordPermissions` exposes three methods to query whether a permission is set: `HasPermission` if you want to find out about a single permission, `HasAnyPermission` and `HasAllPermissions` for querying groups. All of these methods will account for special permissions. If you wish to merely find out whether a specific flag is set, `HasFlag` is provided for advanced purposes.

For editing what permissions are set, `Add`, `Remove` and `Toggle` are provided. Both of them provide overloads for both single permissions and groups of permissions, and additionally `Add` and `Remove` are also provided as operators `+` and `-`. These methods do not account for special behaviour, and as such, revoking a permission may not revoke an administrator's permissions to perform the associated action.

While `Add` and `Remove` merely ensure that at the end of the operation, the specified permissions are added or removed from the set, `Toggle` will always modify the set by flipping the permission. If a permission was previously not granted, this operation will grant it and vice versa.

Furthermore, DSharpPlus provides the bitwise operators `AND`, `OR`, `XOR` and `NOT` on permission sets. For the intents and purposes of these operators, each permission is a bit whose position is not guaranteed. It is not advisable to manually handle these operations instead of the above named, well-defined methods outside of advanced scenarios.

## Enumerating and Printing Permissions

DSharpPlus supports numerous formats for printing permissions. By default, and if no other understood format is specified, DSharpPlus will print an opaque integer representation of permissions that can be round-tripped using `BigInteger.Parse` and the constructor overload accepting a `BigInteger`. It is also the same integer representation used in bot invite links and similar, and also the same representation received from Discord.

If `raw` is passed as a format specifier, DSharpPlus will print the underlying representation. This is mainly intended for debug purposes and not assumed to be useful to users.

If `name` is passed as a format specifier, DSharpPlus will pretty-print the permissions according to no stable order by their English name, separated by commata. Undocumented but set flags will be replaced with their internal number. As a variant of this, format specifiers of the shape `name:custom` will pretty-print permissions by their English name according to the format defined as `custom`, where the provided format string will be copied verbatim and `{permission}` will be replaced by the english name. For example, the following code may result in the following:

~~~cs
permissions.ToString("name: - {permission}\n");

// - Administrator
// - Send Messages
// - Send Text-to-speech Messages
// - 47 
~~~

Note that `{permission}` must be contained as a literal in the string and cannot be interpolated. 

If that does not suffice for your intents, you may also wish to build your own pretty-printer, or do something else entirely. To that end, the method `EnumeratePermissions` is provided, which provides an `IEnumerable<DiscordPermission>` containing all set permissions for the given input set. You can use this as a building block for anything further.

## Other Utilities

Two permission sets may be compared using the `==` and `!=` operators, and permissions have a proper implementation of `GetHashCode` that makes them suitable for use in Dictionary keys and the likes.

The `AllBitsSet` property returns a permission set with all possibly representible, which may be more than Discord supports, whereas `None` returns a permission set with no flags set. `All` returns a set with all permissions documented by Discord and implemented by DSharpPlus set. The values of `AllBitsSet` and `All` may change at any point in time: `AllBitsSet` whenever DSharpPlus changes the size or format of the underlying representation, and `All` whenever Discord adds new permissions that are subsequently implemented by DSharpPlus. They cannot be assumed to be constant.
