# I want to diagnose a problem I believe originates from the library, how do?

In the event you need to debug DSharpPlus, we offer debug symbols. They are available at the following locations:

## Symbol sources

There are 2 symbol sources available:

* Stable: [http://srv.symbolsource.org/pdb/Public](http://srv.symbolsource.org/pdb/Public)
* Nightly: [https://www.myget.org/F/dsharpplus-nightly/symbols/](https://www.myget.org/F/dsharpplus-nightly/symbols/)

## Using the symbols

In Visual Studio:
1. Go to Tools > Options > Debugging and make sure "Just My Code" is disabled and "Source Server Support" is enabled.
2. Go to Tools > Options > Debugging > Symbols, and add the URL in there.
