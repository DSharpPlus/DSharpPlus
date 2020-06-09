# Lavalink - the newer, better way to do music
[Lavalink](https://github.com/Frederikam/Lavalink) is a standalone program, written in Java, using JDA. It's a 
lightweight solution for playing music from sources such as YouTube or 
Soundcloud. Unlike raw voice solutions, such as VoiceNext, Lavalink can handle 
hundreds of concurrent streams, and supports sharding.

## Configuring Java
In order to run Lavalink, you must have Java 11 or greater installed.
The latest releases can be found [here](https://www.oracle.com/technetwork/java/javase/downloads/index.html).

Make sure the location of the newest JRE's bin folder is added to your system variable's path. This will make the `java` command run from the latest runtime. You can verify that you have the right version by entering `java -version` in your command prompt or terminal.

## Downloading Lavalink  
Next, head over to the [releases](https://ci.fredboat.com/viewLog.html?buildId=lastSuccessful&buildTypeId=Lavalink_Build&tab=artifacts&guest=1) tab on the Lavalink Github page. and download the Jar file from the latest version.

The program will not be ready to run yet, as you will need to create a configuration file first. To do so, create a new YAML file called `application.yml` and copy this text:

```yaml
server: # REST and WS server
  port: 8080
  address: localhost
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
There are a few values to keep in mind.

`host` is the IP of the Lavalink host, keep this as `localhost` if it is running on a local machine.

`port` is the allowed port for the Lavalink connection. `8080` is open on most machines.

`password` is the password that you will need to specify when connecting. This can be anything.

When you are finished configuring this, save the file in the same directory as your Lavalink executable.

Keep note of your `port`, `address`, and `password` values, as you will need them later for connecting.

## Starting Lavalink

Open your command prompt or terminal and navigate to the directory containing Lavalink. Once there, type `java -jar Lavalink.jar`. You should start seeing log output from Lavalink.

If everything is configured properly, you should see this appear somewhere in the log output without any errors: 
```
[           main] lavalink.server.Launcher                 : Started Launcher in 5.769 seconds (JVM running for 6.758)
```

If it does, congratulations. We are now ready to interact with it using DSharpPlus.
