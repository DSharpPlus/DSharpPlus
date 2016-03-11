#!/bin/sh

if ! [ command -v nuget ] then
	echo "Nuget not found!" >&2
else
	nuget restore
	xbuild
fi
