---
uid: hosting
title: Hosting Solutions
---

# Publishing your Bot

Typically you dont run your bot 365 days a year from within Visual Studio or another IDE cause that makes the bot slower 
by nature.  You will want to publish it somewhere (either local or remote).  With that said, you will have to familiarize
yourself with the vast ways dotnet allows for you to publish your application.  Please see 
[Microsoft's Documenation](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-3.1) for more details.

# Hosting on Dedicated Machine or VM/VPS

When you host your bot on a Dedicated Machine or VM/VPS you will need to pick a service provider.   

## Hosting at Home
Perhaps you own a dedicated server machine, or you just have a computer that you can leave running 24/7. If your residential 
connection is decent enough, you could try hosting your bot at home.  This has the advantage of giving you full access to hardware 
your code runs on, and only costs you as much as your electricity and internet bill.  You will also need to take care to secure your bot against outages.

You need to make sure you have all the latest updates and patches along with all the necessary software installed. 
It's generally recommended your hosting machine has at least 2 CPU cores, and a decent amount of RAM.

If you dont own a dedicated server but you do own a raspberry pi (versions 2 and above at the time of writting), you can host your bot
on there.  Versions 1 and 0 are not supported due to .Net Core and .Net 5 not supporting the ARMv6 architecture.  

## Hosting remotely 

The simplest, and probably most hassle-free (and maybe cheapest in the long run for dedicated machines) option is to find a provider 
that will lend you their machine or a virtual host so you can run your bot in there.

Generally, cheapest hosting options are all GNU/Linux-based, so it's highly recommended you familiarize yourself with the OS and its 
environment, particularly the shell (command line), and concepts such as SSH.

There are several well-known, trusted, and cheap providers:

* [**Scaleway**](https://www.scaleway.com/ "Scaleway"): Cheap, and powerful. Scalway is based in Europe, and has locations 
  in France and Netherlands. They offer x64 and ARM machines and VPSes.
* [**DigitalOcean**](https://www.digitalocean.com/ "DigitalOcean"): Considered the gold standard. DigitalOcean is based in 
  the US. Their offer ranges from standard to more specialized deployments.
* [**OVH**](https://www.ovh.com/us/ "OVH"): Based in EU and US. OVH is cheap, and used by many people. Their offer includes 
  free DDoS protection.
* [**Contabo**](https://contabo.com/ "Contabo"): Based in Germany, they have very good price-to-contents ratio. SSD-based 
  VPS hosting starts at 4.99€/mo at the time of writing.
* [**Vultr**](https://www.vultr.com/ "Vultr"): US-based, with datacenters all over the globe (incl. APAC region). Pricing similar 
  to Digital Ocean (that is, not cheap). Possible to get starting credit when registering, but it requires a bit of poking around to 
  find it. Starting at $2.50/mo or $3.50/mo if you want IPv4.
* [**Linode**](https://www.linode.com/ "linode"): US-based, with datacenters all over the globe (also incl. APAC region, though not to the extent as Vultr).

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


# Hosting on Cloud Native Services
With most bots, unless if you host many of them, they dont require a whole machine to run them, just a slice of a machine.  This is 
where Docker and other cloud native hosting comes into play.  There are many different options available to you and you will need
to chose which one will suit you best.  Here are a few services that offer Docker or other cloud native solutions that are cheaper than running
a whole VM.

* [**Azure App Service**](https://azure.microsoft.com/en-us/services/app-service/ "Azure App Service"):  Allows for Hosting Website, Continous Jobs, 
  and Docker images on a Windows base or Linux base machine.
* [**AWS Fargate**](https://aws.amazon.com/fargate/ "AWS Fargate"):  Allows for hosting Docker images within Amazon Web Services
* [**Jelastic**](https://jelastic.com/docker/ "Jelastic"):  Allows for hosting Docker images.

# Making your publishing life easier
Now that we have covered where you can possibly host your application, now lets cover how to make your life easier publishing it. Many different
source control solutions out there are free and also offer some type of CI/CD intergration (paid and free).  Below are some of the 
solutions that we recommend:

* [**Azure Devops**]("https://azure.microsoft.com/en-us/services/devops/?nav=min"):  Allows for GIT source control hosting along with intergrated CI/CD
  pipelines to auto compile and publish your applications.  You can also use their CI/CD service if your code is hosted in a different source control enviorment like Github.
* [**Github**]("https://github.com/") Allows for GIT source control hosting.  From here you can leverage many different CI/CD options to compile and publish your 
  applications.
* [**Bitbutcket**]("https://bitbucket.org/"):  Allows for GIT source control hosting along with intergrated CI/CD pipelines to auto compile and publish your applications.

# Final notes
* Discord API Servers are located on the East Coast of the United States, so you may want to pick a hosting provider that resides there or is close.
* Hosting Services like Heroku and glitch are for hosting Web Applications and not standalone apps.  They will turn off after a point in time if traffic is
  not sent to them.  So unless if you plan to host your bot in a Web site (btw why would you), stay away from these hosting companies until the formally support non web applications.
