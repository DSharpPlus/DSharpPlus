---
uid: articles.advanced_topics.trace
title: Trace Logging
---

# Introduction

Trace logs are a very useful debugging feature. They will reveal all information your bot sends to Discord and Discord sends to your bot, both through rest and the real-time gateway, as well as information about the library's internal state and what the library is attempting to do at any given moment.

We recommend you do not enable trace logging with the built-in logger provider, but to register a custom logger and, importantly, to enable logging to files: trace logs are very lengthy and will very quickly hit console length limits. For enabling trace logging, please refer to the documentation of your chosen logger - Serilog, for example, calls it "Verbose" logging.

## Controlling Trace Contents

DSharpPlus offers several options to configure what specific data should be logged to traces. These options live in `runtimeconfig.json`, unlike most other configuration the library offers, due to their nature as hosting-specific rather than development/functionality-specific switches. They may be specified as follows:

1. In `runtimeconfig.json`, switches are specified in the `"configProperties"` section:

```json
{
    "runtimeOptions": {
        "tfm": "net8.0",
        // ...,
        "configProperties": {
            "DSharpPlus.Trace.EnableInboundGatewayLogging": true
        }
    }
}
```

2. In your `.csproj` file:

```xml
<ItemGroup>
    <RuntimeHostConfigurationOption Include="DSharpPlus.Trace.EnableInboundGatewayLogging" Value="true" />
</ItemGroup>
```

DSharpPlus supports the following switches for controlling trace log contents: 
- `DSharpPlus.Trace.EnableRestRequestLogging`, controlling whether payloads from REST should be logged;
- `DSharpPlus.Trace.EnableInboundGatewayLogging`, controlling whether incoming gateway events should be logged;
- `DSharpPlus.Trace.EnableOutboundGatewayLogging`, controlling whether outgoing gateway events should be logged.

Each of these options defaults to `true` - by default, DSharpPlus logs as much information as possible.

### Anonymizing Trace Contents

Trace logs contain huge amounts of potentially sensitive data, such as user IDs, message contents and tokens - everything Discord sends us, and everything we send to Discord. DSharpPlus offers additional feature switches to restrict sensitive information ending up in trace logs:

First, `DSharpPlus.Trace.AnonymizeTokens`. This switch is enabled by default and will hide your bot and webhook tokens in trace logs. As a library consumer, you should typically not turn this off.

Second, `DSharpPlus.Trace.AnonymizeContents`. This switch is disabled by default and will hide snowflake IDs, message contents and usernames in your logs. Since this significantly reduces the quality of debug information in your trace logs, you should evaluate whether you should use this switch on a case-by-case basis.
