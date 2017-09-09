# Yes, this is bad. If you can write your own project parser, please feel free to replace this with something not shit!

dotnet publish $CORESC --output ./travis-bin/ -f $COREAPPTYP -c $COREBTYP DSharpPlus.CommandsNext/DSharpPlus.CommandsNext.csproj || exit 1
dotnet publish $CORESC --output ./travis-bin/ -f $COREAPPTYP -c $COREBTYP DSharpPlus.Interactivity/DSharpPlus.Interactivity.csproj || exit 1
dotnet publish $CORESC --output ./travis-bin/ -f $COREAPPTYP -c $COREBTYP DSharpPlus.Rest/DSharpPlus.Rest.csproj || exit 1
dotnet publish $CORESC --output ./travis-bin/ -f $COREAPPTYP -c $COREBTYP DSharpPlus.VoiceNext/DSharpPlus.VoiceNext.csproj || exit 1
dotnet publish $CORESC --output ./travis-bin/ -f $COREAPPTYP -c $COREBTYP DSharpPlus/DSharpPlus.csproj || exit 1

# WebSocket4NetCore doesn't build on .NET Standard 1.1
# Note: regular WebSocket4Net never builds
if [ "$COREAPPTYP" != "netstandard1.1" ]; then
  dotnet publish $CORESC --output ./travis-bin/ -f $COREAPPTYP -c $COREBTYP DSharpPlus.WebSocket.WebSocket4Net/DSharpPlus.WebSocket.WebSocket4NetCore.csproj || exit 1
fi
dotnet publish $CORESC --output ./travis-bin/ -f netcoreapp1.1 -c $COREBTYP DSharpPlus.Test/DSharpPlus.Test.csproj
