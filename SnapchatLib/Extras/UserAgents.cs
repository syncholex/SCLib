using SnapchatLib;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

internal class CustomGrpcUserAgentHandler : DelegatingHandler
{
    private readonly string m_UserAgent;

    public CustomGrpcUserAgentHandler(SnapchatLockedConfig config)
    {
        m_UserAgent = UserAgentCreator.CreateUserAgent(config, true);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove("User-Agent");
        request.Headers.TryAddWithoutValidation("User-Agent", m_UserAgent);
        return base.SendAsync(request, cancellationToken);
    }
}


internal static class UserAgentCreator
{
    internal static string CreateUserAgent(SnapchatLockedConfig config, bool isGrpc)
    {
        var snapchatInfo = SnapchatInfo.GetInfo(config.SnapchatVersion);
        
        return CreateUserAgentAndroid(snapchatInfo.Version, config.phoneModel, config.androidOsVersion, config.androidIncrementalVersion, config.androidOsSDK, isGrpc);
    }

    private static string CreateUserAgentAndroid(string appVersion, string deviceModel, string deviceVersionRelease, string deviceVersionIncremental, int deviceVersionSdk, bool isGrpc)
    {
        if (string.IsNullOrEmpty(appVersion) ||
            string.IsNullOrEmpty(deviceModel) ||
            string.IsNullOrEmpty(deviceVersionRelease) ||
            string.IsNullOrEmpty(deviceVersionIncremental))
        {
            throw new ArgumentNullException("Invalid android user-agent arguments");
        }

        var userAgent = $"Snapchat/{appVersion} ({deviceModel}; Android {deviceVersionRelease}#{deviceVersionIncremental}#{deviceVersionSdk}; gzip) V/MUSHROOM";

        if (isGrpc)
        {
            userAgent = $"{userAgent} grpc-c++/1.48.0 grpc-c/26.0.0 (android; cronet_http)";
        }

        return userAgent;
    }
}

public class DisableActivityHandler : DelegatingHandler
{
    public DisableActivityHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Activity.Current = null;

        return base.SendAsync(request, cancellationToken);
    }
}