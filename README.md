# SnapchatLib

```csharp
            SnapchatClient snapchatClient = new SnapchatClient(
             new SnapchatConfig
             {
                 androidIncrementalVersion = device.AndroidIncrementalVersion,
                 androidOsSDK = device.AndroidOsSDK,
                 Debug = true,
                 androidOsVersion = device.AndroidOsVersion,
                 phoneManufacture = device.PhoneManufacture,
                 phoneModel = device.PhoneModel,
                 PhoneId = device.PhoneId,
                 ProxySigner = true,
                 Timeout = 1000,
                 SnapchatVersion = SnapchatVersion.V12_76_0_38,
                 Proxy = new WebProxy("PROXY_HERE", false, null, new NetworkCredential("PROXY_USERNAME_HERE", "PROXY_PASSWORD_HERE")),
                 ApiKey = "KEY_HERE",
                 IPAPIPROKEY = "https://members.ip-api.com/ get a key here",
             });
```
# How to sign?
- https://github.com/piombilubicsr/Signers (NOT FREE)

# Why'd You Release Such a Smexy Lib
- Blame Cleo / Jenildo using my stuff without permission so who knows how many people have access may aswell make it open source so community can use
