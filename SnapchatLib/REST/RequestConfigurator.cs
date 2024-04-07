using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLib.Models.SignJson;

namespace SnapchatLib.REST;

internal struct RequestConfiguration
{
    public string Endpoint;
    public HttpMethod HttpMethod;
    public string TempRequestToken;
    public string Timestamp;
    public bool IsMulti;
    public string Username;
    public string DToken1V;
    public OS OS;
    public string XSnapAccessToken;
    public string XSnapchatUserId;
    public string OSUserAgent;
    public string device_model;
    public string country_code;
}

[Flags]
internal enum EndpointRequirements
{
    Username = 1 << 0,
    SignAndroid = 1 << 1,
    DSIG = 1 << 2,
    XSnapAccessToken = 1 << 3,
    UseTempRequestToken = 1 << 4,
    XSnapchatUserId = 1 << 5,
    XSnapchatUUID = 1 << 6,
    OSUserAgent = 1 << 7,
    AcceptEncoding = 1 << 8,
    AcceptProtoBuf = 1 << 9,
    ParamsAsHeaders = 1 << 10,
    UseOldUsername = 1 << 11,
    UseBusinessAccessToken = 1 << 12,
    ArgosHeader = 1 << 13,
    XSnapRouteTag = 1 << 14,
    XRequestUUID = 1 << 15,
    XBlizzardUploadTimestamp = 1 << 16,
    BlizzardConfigVersion = 1 << 17
}

internal struct EndpointInfo
{
    public string BaseEndpoint;
    public string Url;
    public EndpointRequirements Requirements;
    public string SignUrlOverride = null;

    public EndpointInfo()
    {
        BaseEndpoint = RequestConfigurator.ApiBaseEndpoint;
        Url = "";
        Requirements = 0;
    }
}

internal interface IRequestConfigurator
{
    Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, HttpContent content, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false);
    Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, Dictionary<string, string> parameters, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false);
}

internal class RequestConfigurator : IRequestConfigurator
{
    internal static string ApiBaseEndpoint => "https://app.snapchat.com";
    internal static string MediaApiBaseEndpoint => "https://mvm.snapchat.com";
    internal static string ApiGCPEast4Endpoint => "https://us-east4-gcp.api.snapchat.com";
    internal static string ApiAWSEast1Endpoint => "https://us-east1-aws.api.snapchat.com";
    internal static string ProAccountsEndpoint => "https://pro-accounts.snapchat.com";
    internal static string ProStoriesEndpoint => "https://pro-stories.snapchat.com";
    internal static string ApiGCPEndpoint => "https://gcp.api.snapchat.com";
    internal static string IPV6AuthEndpoint => "https://auth.snapchat.com";
    internal static string SearchBaseEndpoint => "https://aws.api.snapchat.com";
    internal static string AnalyticsV2 => "https://app-analytics-v2.snapchat.com";
    internal static string XSnapAccessTokenHeaderName => "X-Snap-Access-Token";
    internal static string XSnapAccessRouteTagHeaderName => "X-Snap-Route-Tag";
    internal static string XSnapchatUserIdHeaderName => "x-snapchat-user-id";
    internal static string XSnapchatUUIDHeaderName => "X-Snapchat-UUID";
    internal static string XRequestUUIDHeaderName => "X-Request-ID";
    internal static string AcceptEncodingHeaderName => "Accept-Encoding";
    internal static string AcceptHeaderName => "Accept";
    internal static string AcceptLanguageHeaderName => "Accept-Language";
    internal static string AcceptLocaleHeaderName => "Accept-Locale";
    internal static string TimestampHeaderName => "timestamp";
    internal static string UsernameHeaderName => "username";
    internal static string DsigHeaderName => "dsig";
    internal static string ReqTokenHeaderName => "req_token";
    internal static string BlizzardConfigVersionName => "blizzard-config-version";
    internal static string XBlizzardUploadTimestampName => "x-blizzard-upload-timestamp";
    internal static string AcceptLanguageValue => "en";
    internal static string AcceptEncodingValue => "gzip, deflate, br";
    internal static string ApplicationProtobufValue => "application/x-protobuf";
    internal static string ApplicationJsonValue => "application/json";


    private readonly IUtilities m_Utilities;
    private readonly IClientLogger m_Logger;

    public RequestConfigurator(IClientLogger logger, IUtilities utilities)
    {
        m_Logger = logger;
        m_Utilities = utilities;
    }

    public async Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, HttpContent content, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false)
    {
        var config = CreateConfig(endpointInfo, httpMethod, client, isMulti);
        return await GenerateRequest(httpClient, config, endpointInfo, content);
    }

    public async Task<HttpRequestMessage> Configure(EndpointInfo endpointInfo, Dictionary<string, string> parameters, HttpMethod httpMethod, SnapchatClient client, ISnapchatHttpClient httpClient, bool isMulti = false)
    {
        var config = CreateConfig(endpointInfo, httpMethod, client, isMulti);
        return await GenerateRequest(httpClient, config, endpointInfo, parameters, isMulti);
    }

    private RequestConfiguration CreateConfig(EndpointInfo endpointInfo, HttpMethod httpMethod, SnapchatClient client, bool isMulti = false)
    {
        var timestamp = m_Utilities.UtcTimestamp().ToString();
        var static_req_token = m_Utilities.GenerateTemporaryRequestToken(timestamp);
        var accessTokenToUse = endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapAccessToken) ? endpointInfo.Requirements.HasFlag(EndpointRequirements.UseBusinessAccessToken) ? client.SnapchatConfig.BusinessAccessToken : client.SnapchatConfig.Access_Token : null;
        // Create the configuration object that will be used to setup the request parameters
        var config = new RequestConfiguration
        {
            Endpoint = endpointInfo.Url,
            HttpMethod = httpMethod,
            DToken1V = client.SnapchatConfig.dtoken1v,
            IsMulti = isMulti,
            TempRequestToken = static_req_token,
            Timestamp = timestamp,
            Username = endpointInfo.Requirements.HasFlag(EndpointRequirements.UseOldUsername) && client.SnapchatConfig.OldUsername != null ? client.SnapchatConfig.OldUsername : client.SnapchatConfig.Username,
            XSnapAccessToken = accessTokenToUse,
            XSnapchatUserId = client.SnapchatConfig.user_id,
            device_model = client.SnapchatConfig.phoneModel,
            country_code = client.SnapchatConfig.AccountCountryCode
        };

        return config;
    }

    private async Task<HttpRequestMessage> CreateRequest(ISnapchatHttpClient client, RequestConfiguration configuration, EndpointInfo endpointInfo)
    {
        var baseEndpoint = endpointInfo.BaseEndpoint;
        var url = baseEndpoint + configuration.Endpoint;
        var request = new HttpRequestMessage(configuration.HttpMethod, url);
        request.Version = configuration.HttpMethod == HttpMethod.Put ? HttpVersion.Version11 : HttpVersion.Version20;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;

        // Add headers
        request.Headers.TryAddWithoutValidation(AcceptLanguageHeaderName, AcceptLanguageValue);

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapAccessToken))
        {
            request.Headers.TryAddWithoutValidation(XSnapAccessTokenHeaderName, configuration.XSnapAccessToken);
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapRouteTag))
        {
            request.Headers.TryAddWithoutValidation(XSnapAccessRouteTagHeaderName, "");
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.BlizzardConfigVersion))
        {
            request.Headers.TryAddWithoutValidation(BlizzardConfigVersionName, "bzdv2.client");
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XBlizzardUploadTimestamp))
        {
            request.Headers.TryAddWithoutValidation(XBlizzardUploadTimestampName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.ArgosHeader))
        {
            request.Headers.TryAddWithoutValidation("x-snapchat-argos-strict-enforcement", "true");
            request.Headers.TryAddWithoutValidation("x-snapchat-att-token", await client.Get_Tokens.GetArgosTokenCached());
        }

        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapchatUserId))
        {
            if (!string.IsNullOrEmpty(configuration.XSnapchatUserId))
            {
                request.Headers.TryAddWithoutValidation(XSnapchatUserIdHeaderName, configuration.XSnapchatUserId);
            }
            else if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapAccessToken))
            {
                request.Headers.TryAddWithoutValidation(XSnapchatUserIdHeaderName, configuration.XSnapAccessToken);
            }
        }

        var uuid = m_Utilities.NewGuid();
        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XSnapchatUUID)) request.Headers.TryAddWithoutValidation(XSnapchatUUIDHeaderName, uuid);
        if (endpointInfo.Requirements.HasFlag(EndpointRequirements.XRequestUUID)) request.Headers.TryAddWithoutValidation(XRequestUUIDHeaderName, uuid);

        if (configuration.HttpMethod == HttpMethod.Post &&
            configuration.Endpoint != "" &&
            configuration.Endpoint != "/snap_token/pb/snap_session" &&
            configuration.Endpoint != "/search/search" &&
            configuration.Endpoint != "/search/pretype" &&
            configuration.Endpoint != "/readreceipt-indexer/batchuploadreadreceipts" &&
            configuration.Endpoint != "/suggest_friend_high_availability" &&
            configuration.Endpoint != "/ami/friends" &&
            configuration.Endpoint != "/story-management-service/get_active_story_status" &&
            configuration.Endpoint != "/df-mixer-prod/soma/batch_stories" &&
            configuration.Endpoint != "/scauth/validate" &&
            configuration.Endpoint != "/scauth/reauth" &&
            configuration.Endpoint != "/rpc/getBusinessStoryManifest" &&
            configuration.Endpoint != "/suggest_friend_high_quality" &&
            configuration.Endpoint != "/suggest_friend_on_demand" &&
            configuration.Endpoint != "/ph/settings" &&
            configuration.Endpoint != "/bitmoji-api/avatar-service/create-avatar-data" &&
            configuration.Endpoint != "/analytics/bz2" &&
            configuration.Endpoint != "/snap_token/pb/snap_access_tokens" &&
            configuration.Endpoint != "/ami/friends" &&
            configuration.Endpoint != "/readreceipt-indexer/batchuploadreadreceipts")
        {
            if (configuration.OS == OS.android && configuration.Endpoint != "/loq/device_id")
            {

                var signResult = m_Utilities.JsonDeserializeObject<SignResponseJson>(
                                    await client.Sign.SignRequest(configuration.Endpoint));

                request.Headers.UserAgent.Clear();
                request.Headers.TryAddWithoutValidation("x-snapchat-att", signResult.x_snapchat_att);
            }
        }

        return request;
    }
    private void AddExtraData(HttpRequestMessage request, EndpointInfo endpointInfo, RequestConfiguration configuration, IDictionary<string, string> parameters)
    {

        var paramsAsHeaders = endpointInfo.Requirements.HasFlag(EndpointRequirements.ParamsAsHeaders);
        if (paramsAsHeaders)
            request.Headers.TryAddWithoutValidation(TimestampHeaderName, configuration.Timestamp);
        else
            parameters?.TryAdd(TimestampHeaderName, configuration.Timestamp);


        if (endpointInfo.BaseEndpoint != "/snap_token/pb/snap_session")
        {
            // Append the username
            if (endpointInfo.Requirements.HasFlag(EndpointRequirements.Username))
            {
                if (paramsAsHeaders)
                    request.Headers.TryAddWithoutValidation(UsernameHeaderName, configuration.Username);
                else
                    parameters?.TryAdd(UsernameHeaderName, configuration.Username);
            }
        }
    }

    private async Task<HttpRequestMessage> GenerateRequest(ISnapchatHttpClient client, RequestConfiguration configuration, EndpointInfo endpointInfo, HttpContent content)
    {
        var request = await CreateRequest(client, configuration, endpointInfo);

        AddExtraData(request, endpointInfo, configuration, null);

        request.Content = content;
        return request;
    }

    private async Task<HttpRequestMessage> GenerateRequest(ISnapchatHttpClient client, RequestConfiguration configuration, EndpointInfo endpointInfo, Dictionary<string, string> parameters, bool ismulti)
    {
        var request = await CreateRequest(client, configuration, endpointInfo);

        AddExtraData(request, endpointInfo, configuration, parameters);
        if (ismulti)
        {
            var content = new MultipartFormDataContent();
            foreach (var parameter in parameters)
            {
                content.Add(new StringContent(parameter.Value), parameter.Key);
            }
            request.Content = content;
        }
        else
        {
            var content = new FormUrlEncodedContent(parameters);
            content.Headers.ContentType.CharSet = "utf-8";
            request.Content = content;
        }

        return request;
    }
}