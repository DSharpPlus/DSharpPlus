# Halp! I tried ~~hard drugs~~ Mono and my bot crashes on start!

Mono is an open-source implementation of the .NET Framework for non-Windows operating systems. However, it has a large 
number of flaws, some of which break support for DSharpPlus without extra effort. It is generally recommended you stay 
away from it.

## But I cannot use Windows! What do?

A cross-platform alternative, that works on Windows, GNU/Linux, and OS X, is .NET Core. It's lighter, faster, and free 
of Mono's problems.

Compared to Mono, you are not required to use 3rd party WebSocket client implementations, nor does it require any 
changes to existing .NET Core projects. Porting from .NETFX is not hard, although generally requires that you start a 
new project, and copy all existing code over. Then you will need to fix all the build errors, usually with help of 
Google.

This might sound scary at first, but it has huge long-term gains, not the least of which is better performance, less 
resources used, and better portability.

## I must absolutely use Mono!

If you insist on using a broken runtime, I cannot stop you, however with most problems, you will be on your own. Before 
you begin, you will need to perform some setup steps on the target system, as well as make some changes to your 
application. Note that due to the nature of these changes, your application might be compromised through a 
man-in-the-middle attacks much easier, as it's required you override certificate validation (as Mono lacks support for 
modern certificate signatures).

### Install certificates

Once you have Mono installed, you will need to populate its certificate cache. This is done by doing 
`cert-sync --user /etc/ssl/certs/ca-certificates.crt`. Once this is done, you should see a bunch of certificates being 
installed.

### Install WebSocketSharp client implementation

Mono lacks support for most of the HTTP-related code from .NETFX Base Class Library. Due to that, it is required that 
you install an alternative WebSocket client implementation.

To achieve that, follow the instructions outlined in the 
[Alternate WebSocket client implementations](/articles/getting_started/alternate_ws.html "Alternate WebSocket client implementations") 
section of the guide.

### Place validation callback override in your code

In your entry class, you will need to place the following method:

```cs
private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
{
    bool isOk = true;

    if (sslPolicyErrors != SslPolicyErrors.None)
    {
        foreach (X509ChainStatus cs in chain.ChainStatus)
        {
            if (cs.Status != X509ChainStatusFlags.RevocationStatusUnknown)
            {
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                }
            }
        }
    }

    return isOk;
}
```

Next up, in your entry point method, before anything else, you will need to place the following code:

```cs
ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
```

This will override all certificate validation callbacks. Be aware that this will drastically reduce the security of 
your application's traffic.

The above code is courtesy of Hawx#1545 (89600512793530368) from Discord API.