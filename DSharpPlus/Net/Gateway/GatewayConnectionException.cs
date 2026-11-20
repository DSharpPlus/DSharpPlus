using System;

namespace DSharpPlus.Net.Gateway;

// thrown if the gateway connecting encountered a Discord Moment:tm:
internal sealed class GatewayConnectionException(string message) : Exception(message);
