---
uid: articles.misc.debug_symbols
title: Debug Symbols
---

# I want to diagnose a problem I believe originates from the library, how do?
In the event you need to debug DSharpPlus, we offer debug symbols. They are available at the following locations:

## Symbol sources
There are 2 symbol sources available:

* Stable: [http://srv.symbolsource.org/pdb/Public][0]
* Nightly: [https://nuget.emzi0767.com/api/v3/symbolstore][1]

## Using the symbols
In Visual Studio:
1. Go to Tools > Options > Debugging and make sure "Just My Code" is disabled and "Source Server Support" is enabled.
2. Go to Tools > Options > Debugging > Symbols, and add the URL in there.

<!-- LINKS -->
[0]:  http://srv.symbolsource.org/pdb/Public
[1]:  https://nuget.emzi0767.com/api/v3/symbolstore
