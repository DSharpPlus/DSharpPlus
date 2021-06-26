---
uid: hosting
title: Hosting Solutions
---

## 24/7 Hosting Solutions

### Free hosting
If you're looking for free hosts, you've likely considered using [Heroku](https://www.heroku.com/) or [Glitch](https://glitch.com/). 
We advise against using these platforms as they are designed to host web services, not Discord bots, and instances from either of these companies will shut down if there isn't enough internet traffic.
Save yourself the headache and don't bother.

Alternatively, some providers are offering Free Tiers which also allow application hosting. These services typically have some sort of resource quota, and may charge money on exceeding these quotas. Make sure to carefully review the fine-print, and understand that these services may come with strings attached.

One such provider is:

* [**Oracle Cloud**](https://www.oracle.com/cloud/free/) - Oracle currently offers an Always Free Tier that provides VMs with up to 4 Arm-based cores, 24GB of ram, 200GB of storage and does not have a monthly time limit. However, this service does require a valid credit card and offers no SLA.

### Self Hosting
If you have access to an unused machine, have the technical know-how, and you also have a solid internet connection, you might consider hosting your bot on your own.
Even if you don't have a space PC on hand, parts to build one are fairly cheap in most regions. You could think of it as a one time investment with no monthly server fees.
Any modern hardware will work just fine, new or used. 

Depending on how complex your bot is, you may even consider purchasing a Raspberry Pi ($35).

### Third-Party Hosting
The simplest, and probably most hassle-free (and maybe cheapest in the long run for dedicated machines) option is to find a provider 
that will lend you their machine or a virtual host so you can run your bot in there.

Generally, cheapest hosting options are all GNU/Linux-based, so it's highly recommended you familiarize yourself with the OS and its 
environment, particularly the shell (command line), and concepts such as SSH.

There are several well-known, trusted, and cheap providers:

* [Host Pls](https://host-pls.com/) - A hosting solution made by Discord bot developers. Based in America, starting from $2.49/mo.
* [Vultr](https://www.vultr.com/products/cloud-compute/) - Based in the US with datacenters in many regions, including APAC. Starting at $2.50/mo.
* [DigitalOcean](https://www.digitalocean.com/products/droplets/) - The gold standard, US based. Locations available world wide. Starting from $5.00/mo.
* [Linode](https://www.linode.com/products/shared/) - US based host with many datacenters around the world. Starting at $5.00/mo.
* [OVH](https://www.ovhcloud.com/en/vps/) - Very popular VPS host. Based in Canadian with French locations available. Starting from $6.00/mo.
* [Contabo](https://contabo.com/?show=vps) - Based in Germany; extremely good value for the price. Starting from 4.99€/mo.


Things to keep in mind when looking for a VPS host:
* The majority of cheap VPS hosts will be running some variant of Linux, and not Windows.
* The primary Discord API server is located in East US.
  * If latency matters for you application, choose a host that is closer to this location.


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


### Hosting on Cloud Native Services
With most bots, unless if you host many of them, they dont require a whole machine to run them, just a slice of a machine.  This is 
where Docker and other cloud native hosting comes into play.  There are many different options available to you and you will need
to chose which one will suit you best.  Here are a few services that offer Docker or other cloud native solutions that are cheaper than running
a whole VM.

* [**Azure App Service**](https://azure.microsoft.com/en-us/services/app-service/ "Azure App Service"):  Allows for Hosting Website, Continous Jobs, 
  and Docker images on a Windows base or Linux base machine.
* [**AWS Fargate**](https://aws.amazon.com/fargate/ "AWS Fargate"):  Allows for hosting Docker images within Amazon Web Services
* [**Jelastic**](https://jelastic.com/docker/ "Jelastic"):  Allows for hosting Docker images.

### Making your publishing life easier
Now that we have covered where you can possibly host your application, now lets cover how to make your life easier publishing it. Many different
source control solutions out there are free and also offer some type of CI/CD integration (paid and free).  Below are some of the 
solutions that we recommend:

* [**Azure Devops**](https://azure.microsoft.com/en-us/services/devops/?nav=min "Azure Devops"):  Allows for GIT source control hosting along with integrated CI/CD
  pipelines to auto compile and publish your applications.  You can also use their CI/CD service if your code is hosted in a different source control enviorment like Github.
* [**Github**](https://github.com/ "GitHub") Allows for GIT source control hosting.  From here you can leverage many different CI/CD options to compile and publish your 
  applications.
* [**Bitbucket**](https://bitbucket.org/ "Bitbucket"):  Allows for GIT source control hosting along with integrated CI/CD pipelines to auto compile and publish your applications.
