---
uid: articles.misc.debug_symbols
title: Debug Symbols
---

# I want to diagnose a problem I believe originates from the library, how do?
In the event you need to debug DSharpPlus, we offer debug symbols. They are available at the following locations:

## Symbol sources
All of our symbols can be found on [Nuget](https://www.nuget.org/packages/DSharpPlus/). Nightly builds have symbols and source included inside of the packages, while release builds will only contain the symbols.

## Using the symbols
In Visual Studio:
1. Go to Tools > Options > Debugging and make sure "Just My Code" is disabled and "Source Server Support" is enabled.
2. Go to Tools > Options > Debugging > Symbols, and add the URL in there.
