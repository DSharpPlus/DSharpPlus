---
uid: hosting
title: Hosting Solutions
---

## 24/7 Hosting Solutions

### Free hosting
Outside of persuading somebody to host your bot, you won't find any good free hosting solutions.

If you're looking for free hosts, you've likely considered using [Heroku](https://www.heroku.com/) or [Glitch](https://glitch.com/). 
We advise against using these platforms as they are designed to host web services, not Discord bots, and instances from either of these companies will shut down if there isn't enough internet traffic.
Save youself the headache and don't bother.

You'll be better off [renting](#third-party-hosting) a cheap VPS.

### Self Hosting
If you have access to an unused machine, have the technical know-how, and you also have a solid internet connection, you might consider hosting your bot on your own.
Even if you don't have a space PC on hand, parts to build one are fairly cheap in most regions. You could think of it as a one time investment with no monthly server fees.
Any modern hardware will work just fine, new or used. 

Depending on how complex your bot is, you may even consider purchasing a Raspberry Pi ($35).

### Third-Party Hosting
A [VPS](https://en.wikipedia.org/wiki/Virtual_private_server) is the easiest (and usually the least painless) way to host your bot.

* [Host Pls](https://host-pls.com/) - A hosting solution made by Discord bot developers. Based in America, starting from $2.49/mo.
* [Vultr](https://www.vultr.com/products/cloud-compute/) - Based in the US with datacenters in many regions, including APAC. Starting at $2.50/mo.
* [DigitalOcean](https://www.digitalocean.com/products/droplets/) - The gold standard, US based. Locations available world wide. Starting from $5.00/mo.
* [Linode](https://www.linode.com/products/shared/) - US based host with many datacenters around the world. Starting at $5.00/mo.
* [OVH](https://www.ovhcloud.com/en/vps/) - Very popular VPS host. Based in Canadian with French locations available. Starting from $6.00/mo.
* [Contabo](https://contabo.com/?show=vps) - Based in Germany; extremely good value for the price. Starting from 4.99€/mo.

<br/>
Things to keep in mind when looking for a VPS host:
* The majority of cheap VPS hosts will be running some variant of Linux, and not Windows.
* The primary Discord API server is located in East US.
  * If latency matters for you application, choose a host that is closer to this location.