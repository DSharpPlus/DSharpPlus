# DSharpPlus Core Library

This directory is home to everything needed to build the DSharpPlus core library, that is, the user-facing 
convenience library without extension libraries. This includes the serialization models, the REST abstractions 
and implementation, the Gateway abstractions and implementation, as well as the libraries and tools needed to 
wrap these components into a well-curated, convenient library for end users.

These components are generally included privately in the main library. If end users wish to consume these libraries 
directly, they must reference them directly, not transiently through our main library.

Because it is impossible to type-forward from a privately included library, some libraries must be included
publicly. These libraries are, therefore, versioned with the main library, and they contain a notice mentioning
this fact.
