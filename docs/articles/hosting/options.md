# My ~~body~~ bot is ready, but I can't keep my PC running 24/7!

Typically, hosting a bot means it needs to run 24 hours a day, 7 days a week. While running your computer like that is 
an option, it's generally better to deploy your bot on a machine that can run 24/7.

There are 2 (although really 3) options for that. Unfortunately, none of them is free (or at least not permanently).

## Purchasing a VPS or a dedicated server

The simplest, and probably most hassle-free (and maybe cheapest in the long run) option is to find a provider that will 
lend you their machine or a virtual host so you can run your bot in there.

Generally, cheapest hosting options are all GNU/Linux-based, so it's highly recommended you familiarize yourself with the
OS and its environment, particularly the shell (command line), and concepts such as SSH.

There are several well-known, trusted, and cheap providers:

* [**Scaleway**](https://www.scaleway.com/ "Scaleway"): Cheap, and powerful. Scalway is based in Europe, and has locations 
  in France and Netherlands. They offer x64 and ARM machines and VPSes.
* [**DigitalOcean**](https://www.digitalocean.com/ "DigitalOcean"): Considered the gold standard. DigitalOcean is based in 
  the US. Their offer ranges from standard to more specialized deployments.
* [**OVH**](https://www.ovh.com/us/ "OVH"): Based in EU and US. OVH is cheap, and used by many people. Their offer includes 
  free DDoS protection.

In addition to these, there are several hosting providers that offer free trials or in-service credit:

* [**Microsoft Azure**](https://azure.microsoft.com/en-us/free/?cdn=disable "Microsoft Azure"): $200 in-service credit, 
  to be used within month of registration. Requires credit or debit card for validation. Azure isn't cheap, but it supports 
  both Windows and GNU/Linux-based servers. If you're enrolled in Microsoft Imagine, it's possible to get these cheaper or 
  free.
* [**Amazon Web Services**](https://aws.amazon.com/free/ "AWS"): Free for 12 months (with 750 compute hours per month). Not 
  cheap once the trial runs out, but it's also considered industry standard in cloud services.
* [**Google Cloud Platform**](https://cloud.google.com/free/ "Google Cloud Platform"): $300 in-service credit, to be used 
  within year of registration. GCP is based in the US, and offers very scalable products. Like the above, it's not the 
  cheapest of offerings.

## I want to host at home

Perhaps you own a dedicated server machine, or you just have a computer that you can leave running 24/7. If your residential 
connection is decent enough, you could try hosting your bot at home. This has the advantage of giving you full access to 
hardware your code runs on, and only costs you as much as your electricity and internet bill. You will also need to take care 
to secure your bot against outages.

You need to make sure you have all the latest updates and patches along with all the necessary software installed. It's generally 
recommended your hosting machine has at least 2 CPU cores, and a decent amount of RAM.

### I have a Pi, can't that be used?

We have a [special guide](/articles/hosting/raspberry_pi.html "Hosting on a Rasbperry Pi") for Raspberry Pi.