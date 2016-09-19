#!/bin/sh

if ! [ command -v nuget ] then
	echo "Nuget not found!" >&2
else
	nuget restore
	xbuild
	cd /home/travis/build/NaamloosDT/DSharpPlus/DSharpPlus/bin
	zip -r build.zip /home/travis/build/NaamloosDT/DSharpPlus/DSharpPlus/bin/*
fi
