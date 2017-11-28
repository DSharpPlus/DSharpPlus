# I want to diagnose a problem I believe originates from the library, how do?

For situations where, for whatever reason, you need to debug the library, we offer debug symbols.

## Symbol sources

There are 2 symbol sources available:

* Stable: [http://srv.symbolsource.org/pdb/Public](http://srv.symbolsource.org/pdb/Public)
* Nightly: [https://www.myget.org/F/dsharpplus-nightly/symbols/](https://www.myget.org/F/dsharpplus-nightly/symbols/)

## Using the symbols

To use the debug symbols in Visual Studio, go to Tools > Options > Debugging > Symbols, and add the URL in there.

Press OK, then debug your project again, the symbols should be loaded automatically.