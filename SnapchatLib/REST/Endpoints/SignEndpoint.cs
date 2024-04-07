using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapchatLib.Models.SignJson;
using static SnapchatLib.Extras.Utilities;

namespace SnapchatLib.REST.Endpoints;

internal struct JInfo
{
    public string Version;
    public string Key;
}

internal interface ISignEndpoint
{
    Task<string> SignRequest(string path);
}

internal class SignEndpoint : EndpointAccessor, ISignEndpoint
{
    internal static string DefaultSignUrl = $""; // Your Signer URL

    public SignEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    internal static HttpResponseMessage response { get; set; }
    internal static Dictionary<string, object> requestData { get; set; } = new Dictionary<string, object>();
    string[] pathsToCheck =
    [
        "/snapchat.janus.api/RegistrationService/RegisterWithUsernamePassword",
        "/snapchat.cdp.cof.CircumstancesService/targetingQuery",
        "/snapchat.janus.api.LoginService/LoginWithPassword",
        "/snapchat.janus.api.LoginService/SendODLVCode",
        "/snapchat.janus.api.LoginService/VerifyODLV",
        "/snapchat.cdp.cof.CircumstancesService/targetingQuery"
    ];
    private void RaiseForInvalidValues(string path)
    {
        // Check if install time is set
        if (Config.install_time == 0)
        {
            throw new SignerException("Install time is not set or is invalid.");
        }

        // Check for required phone information
        if (string.IsNullOrEmpty(Config.phoneModel) ||
            string.IsNullOrEmpty(Config.phoneManufacture) ||
            Config.androidOsSDK == 0 ||
            string.IsNullOrEmpty(Config.androidOsVersion))
        {
            throw new Exception("Phone model, manufacture, Android OS SDK, and version are required.");
        }

        // Check for required tokens based on the path
        if (!pathsToCheck.Contains(path))
        {
            if (string.IsNullOrEmpty(Config.dtoken1i))
                throw new ArgumentNullException("dtoken1i", "dtoken1i cannot be null.");

            if (string.IsNullOrEmpty(Config.dtoken1v))
                throw new ArgumentNullException("dtoken1v", "dtoken1v cannot be null.");

            if (string.IsNullOrEmpty(Config.Access_Token))
                throw new Exception("Access Token is required to protect from ban.");
        }

        // Check for Android ID
        if (string.IsNullOrEmpty(Config.androidId))
        {
            throw new AndroidIDNotSet();
        }

        // Check if path is provided
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new SignerException("Path cannot be null or empty.");
        }
    }

    public async Task<string> SignRequest(string req_path)
    {
        if (Config.dtoken1i == null)
            Config.dtoken1i = "";

        DateTime installTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(Config.install_time);
        DateTime launchTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(SnapchatClient.launchTimestamp);

        if (DateTime.Compare(installTime, launchTime) > 0)
        {
            throw new Exception("install_time incorrect");
        }

        var info = SnapchatInfo.GetInfo(Config.SnapchatVersion);

        RaiseForInvalidValues(req_path);
        var request = new HttpRequestMessage(HttpMethod.Post, DefaultSignUrl);
        var sign = new SigningRequestJson
        {
            snapchat_info = new SnapchatInfo_
            {
                snapchat_version = info.Version,
            },
            session_info = new SessionInfo
            {
                sessionIdent = SnapchatClient.sessionId,
                timeBeginSession = SnapchatClient.launchTimestamp,
                sequenceRequest = SnapchatClient.sequenceRequest,
            },
            persistent_info = new PersistentInfo
            {
                dtoken1i = Config.dtoken1i,
                installTime = Config.install_time,
            },
            phone_info = new PhoneInfo
            {
                androidId = (long)AndroidIDConverter.AndroidIDToLong(Config.androidId),
                androidOsApi = Config.androidOsSDK,
                androidOsVersion = Config.androidOsVersion,
                phoneModel = Config.phoneModel,
                phoneBuild = Config.androidIncrementalVersion,
                phoneManufacture = Config.phoneManufacture,
                phoneId = Config.PhoneId,
            },
            snapchat_request_info = new SnapchatRequestInfo
            {
                path = req_path,
                req_token = "",
            }
        };
        request.Version = HttpVersion.Version20;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
        request.Headers.TryAddWithoutValidation("x-license", Config.ApiKey);
        request.Content = new StringContent(m_Utilities.JsonSerializeObject(sign), Encoding.UTF8, "application/json");

        if (Config.ProxySigner)
        {
            response = await SnapchatHttpClient.Send(DefaultSignUrl, request, true);
        }
        else
        {
            response = await SnapchatHttpClient.Send(DefaultSignUrl, request, false);
        }


        var responseData = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
            throw new SignerException(responseData);

        var signResult = m_Utilities.JsonDeserializeObject<SignResponseJson>(responseData);
        if (signResult.x_snapchat_att != null &&
            (signResult.x_snapchat_att.Contains("CgkYA") ||
             signResult.x_snapchat_att.Contains("CgsYA") ||
             signResult.x_snapchat_att.Contains("Ch0YA")))
        {
            SnapchatClient.sequenceRequest++;
            return responseData;
        }

        throw new SignerException(responseData);
    }
}