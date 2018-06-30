# This is not DSharpPlus.

So you've stumbled upon my bastardised, heavily stripped down version of D#+, for use in personal projects. Well done, now, piss off :P

## If you want DSharpPlus, go [here.](https://github.com/DSharpPlus/DSharpPlus)

This is obviously unsupported, for fucks sake don't bother Emzi with this, I'll do that when I can't figure out how the fuck their code works.

# Why?

One or two reasons, this fork is primarily aimed at being kind to UI, specifically UWP and WPF. As such, it features a lot of work with regards to databinding, and usability as view models. Someone changes their username? Everything on screen updates with it, easy.

Because of this, some things have been removed, such as the built in commands system and alternate WebSocket clients, and many things have been reworked, such as most entities inhereting from `PropertyChangedBase`, which allows easy notification of property changes.
