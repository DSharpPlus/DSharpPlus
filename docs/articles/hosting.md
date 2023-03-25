---
uid: articles.hosting
title: Hosting Solutions
---

# 24/7 Hosting Solutions

## Free hosting

If you're looking for free hosts, you've likely considered using [Heroku][0] or [Glitch][1]. We advise against using
these platforms as they are designed to host web services, not Discord bots, and instances from either of these
companies will shut down if there isn't enough internet traffic. Save yourself the headache and don't bother.

Alternatively, some providers are offering Free Tiers which also allow application hosting. These services typically
have some sort of resource quota, and may charge money on exceeding these quotas. Make sure to carefully review the
fine-print, and understand that these services may come with strings attached. You can find examples below.

## Self Hosting

If you have access to an unused machine, have the technical know-how, and you also have a solid internet connection, you
might consider hosting your bot on your own. Even if you don't have a spare PC on hand, parts to build one are fairly
cheap in most regions. You could think of it as a one time investment with no monthly server fees. Any modern hardware
will work just fine, new or used.

Depending on how complex your bot is, you may even consider purchasing a Raspberry Pi ($35).

## Third-Party Hosting

The simplest, and probably most hassle-free (and maybe cheapest in the long run for dedicated machines) option is to
find a provider that will lend you their machine or a virtual host so you can run your bot in there.

Generally, cheapest hosting options are all GNU/Linux-based, so it's highly recommended you familiarize yourself with
the OS and its environment, particularly the shell (command line), and concepts such as SSH.

There are several well-known, trusted, and cheap providers:

* [Xenyth Cloud][2] - A hosting solution made by Discord bot developers. Based in Canada, starting from $2.49/mo.
* [Vultr][3] - Based in the US with datacenters in many regions, including APAC. Starting at $2.50/mo.
* [DigitalOcean][4] - The gold standard, US based. Locations available world wide. Starting from $5.00/mo.
* [Linode][5] - US based host with many datacenters around the world. Starting at $5.00/mo.
* [OVH][6] - Very popular VPS host. Worldwide locations available. Starting from $6.00/mo.
* [Contabo][7] - Based in Germany, US locations available; extremely good value for the price. Starting from 4.99€/mo.

Things to keep in mind when looking for a hosting provider:

* The majority of cheap VPS hosts will be running some variant of Linux, and not Windows.
* The primary Discord API server is located in East US.
  * If latency matters for you application, choose a provider who is closer to this location.

In addition to these, there are several hosting providers who offer free tiers, free trials, or in-service credit:

* [**Microsoft Azure**][8]: $200 in-service credit, to be used within month of registration. There are also several
  always-free services available, including various compute resources. Requires credit or debit card for validation.
  Azure isn't cheap, but it supports both Windows and GNU/Linux-based servers. If you're enrolled in Microsoft Imagine,
  it's possible to get these cheaper or free.
* [**Amazon Web Services**][9]: Free for 12 months (with 750 compute hours per month), with several always-free options
  available. Not cheap once the trial runs out, but it's also considered industry standard in cloud services.
* [**Google Cloud Platform**][10]: $300 in-service credit, to be used within year of registration, and several
  always-free resources available, albeit with heavy restrictions. GCP is based in the US, and offers very scalable
  products. Like the above, it's not the cheapest of offerings.
* [**Oracle Cloud**][11] - $300 credit to be used within a month, and an always-free tier, which provides up to 4 ARM
  cores, 24GB of ram, and 200GB of storage in compute resources, as well as some small x64 instances. There is no
  monthly time limit. This service does require a valid credit card and offers no SLA.

## Hosting on Cloud Native Services

With most bots, unless if you host many of them, they dont require a whole machine to run them, just a slice of a
machine. This is where Docker and other cloud native hosting comes into play. There are many different options available
to you and you will need to chose which one will suit you best. Here are a few services that offer Docker or other cloud
native solutions that are cheaper than running a whole VM.

* [**Azure App Service**][12]: Allows for Hosting Website, Continous Jobs, and Docker images on a Windows base or Linux
  base machine.
* [**AWS Fargate**][13]: Allows for hosting Docker images within Amazon Web Services.
* [**Jelastic**][14]: Allows for hosting Docker images.

## Making your publishing life easier

Now that we have covered where you can possibly host your application, now lets cover how to make your life easier
publishing it. Many different source control solutions out there are free and also offer some type of CI/CD integration
(paid and free). Below are some of the solutions that we recommend:

* [**Github**][15]: Offers Git repository hosting, as well as static page hosting (under \*.github.io domain) and basic
  CI/CD in form of GitHub actions.
* [**GitLab**][16]: Another Git repository hosting, offers a far more advanced and flexible CI/CD. Can be self-hosted.
* [**BitBucket**][17]: Like the previous two, offers Git repository hosting, and CI/CD services.
* [**Azure Devops**][18]: Offers Git and Team Foundation Version Control repository hosting, in addition to full CI/CD
  pipeline, similar to GitHub actions. The CI/CD offering can be attached to other services as well.

<!-- LINKS -->
[0]:  https://www.heroku.com/
[1]:  https://glitch.com/
[2]:  https://xenyth.net/
[3]:  https://www.vultr.com/products/cloud-compute/
[4]:  https://www.digitalocean.com/products/droplets/
[5]:  https://www.linode.com/products/shared/
[6]:  https://www.ovhcloud.com/en/vps/
[7]:  https://contabo.com/?show=vps
[8]:  https://azure.microsoft.com/en-us/free/
[9]:  https://aws.amazon.com/free/
[10]: https://cloud.google.com/free/
[11]: https://www.oracle.com/cloud/free/
[12]: https://azure.microsoft.com/en-us/services/app-service/
[13]: https://aws.amazon.com/fargate/
[14]: https://jelastic.com/docker/
[15]: https://github.com/
[16]: https://gitlab.com/
[17]: https://bitbucket.org/
[18]: https://azure.microsoft.com/en-us/services/devops/?nav=min
