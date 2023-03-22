---
uid: articles.audio.lavalink.setup
title: Lavalink Setup
---

# Lavalink - the newer, better way to do music

[Lavalink][0] is a standalone program, written in Java. It's a lightweight solution for playing music from sources such
as YouTube or Soundcloud. Unlike raw voice solutions, such as VoiceNext, Lavalink can handle hundreds of concurrent
streams, and supports sharding.

## Configuring Java

In order to run Lavalink, you must have Java 13 or greater installed. Certain Java versions may not be functional with
Lavalink, so it is best to check the [requirements][1] before downloading. The latest releases can be found [here][2].

Make sure the location of the newest JRE's bin folder is added to your system variable's path. This will make the `java`
command run from the latest runtime. You can verify that you have the right version by entering `java -version` in your
command prompt or terminal.

## Downloading Lavalink  

Next, head over to the [releases][3] tab on the Lavalink GitHub page and download the Jar file from the latest version.
Alternatively, stable builds with the latest changes can be found on their [CI Server][4].

The program will not be ready to run yet, as you will need to create a configuration file first. To do so, create a new
YAML file called `application.yml`, and use the [example file][5], or copy this text:

```yaml
server: # REST and WS server
  port: 2333
  address: 127.0.0.1
spring:
  main:
    banner-mode: log
lavalink:
  server:
    password: "youshallnotpass"
    sources:
      youtube: true
      bandcamp: true
      soundcloud: true
      twitch: true
      vimeo: true
      mixer: true
      http: true
      local: false
    bufferDurationMs: 400
    youtubePlaylistLoadLimit: 6 # Number of pages at 100 each
    youtubeSearchEnabled: true
    soundcloudSearchEnabled: true
    gc-warnings: true

metrics:
  prometheus:
    enabled: false
    endpoint: /metrics

sentry:
  dsn: ""
#  tags:
#    some_key: some_value
#    another_key: another_value

logging:
  file:
    max-history: 30
    max-size: 1GB
  path: ./logs/

  level:
    root: INFO
    lavalink: INFO
```

YAML is whitespace-sensitive. Make sure you are using a text editor which properly handles this.

There are a few values to keep in mind.

`host` is the IP of the Lavalink host. This will be `0.0.0.0` by default, but it should be changed as it is a security
risk. For this guide, set this to `127.0.0.1` as we will be running Lavalink locally.

`port` is the allowed port for the Lavalink connection. `2333` is the default port, and is what will be used for this
guide.

`password` is the password that you will need to specify when connecting. This can be anything as long as it is a valid
YAML string. Keep it as `youshallnotpass` for this guide.

When you are finished configuring this, save the file in the same directory as your Lavalink executable.

Keep note of your `port`, `address`, and `password` values, as you will need them later for connecting.

## Starting Lavalink

Open your command prompt or terminal and navigate to the directory containing Lavalink. Once there, type
`java -jar Lavalink.jar`. You should start seeing log output from Lavalink.

If everything is configured properly, you should see this appear somewhere in the log output without any errors:

```
[           main] lavalink.server.Launcher                 : Started Launcher in 5.769 seconds (JVM running for 6.758)
```

If it does, congratulations. We are now ready to interact with it using DSharpPlus.

<!-- LINKS -->
[0]:  https://github.com/freyacodes/Lavalink
[1]:  https://github.com/freyacodes/Lavalink#requirements
[2]:  https://adoptium.net/
[3]:  https://github.com/freyacodes/Lavalink/releases
[4]:  https://ci.fredboat.com/viewLog.html?buildId=lastSuccessful&buildTypeId=Lavalink_Build&tab=artifacts&guest=1
[5]:  https://github.com/freyacodes/Lavalink/blob/master/LavalinkServer/application.yml.example
