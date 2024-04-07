using Google.Protobuf;
using Janus.Crypto.Fidelius;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using SnapchatLib.Extras;
using SnapchatLibProto.Snapchat.Activation.Api;
using SnapchatLibProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Cdp.Cof;
using SnapProto.Snapchat.Fidelius;
using SnapProto.Snapchat.Search;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SnapchatLib.Extras.Utilities;
namespace SnapchatLib.REST.Endpoints;
public interface IRegisterV2Endpoint
{
    Task<SCJanusRegisterWithUsernamePasswordResponse> Register(string username, string password, string firstname, string lastname, string email);
    Task<SCJanusRegisterWithUsernamePasswordResponse> RegisterNoIntegrityNoKeyAttest(string username, string password, string firstname, string lastname, string email);
    Task<ValidationStatus> VerifyEmail(string link);
    string GetSnapchatConfirmationLink(string body);
    Task<WebCreationStatus> RegisterWeb(string firstname, string lastname, string username, string password, string email, string emailpassword);
}

internal class RegisterV2Endpoint : EndpointAccessor, IRegisterV2Endpoint
{
    public RegisterV2Endpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    internal static readonly EndpointInfo EndpointInfo = new()
    {
        BaseEndpoint = RequestConfigurator.AnalyticsV2,
        Url = "/analytics/bz2",
        Requirements = EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XBlizzardUploadTimestamp | EndpointRequirements.BlizzardConfigVersion
    };
    SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign zodiac_sign(int day, int month)
    {
        SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Unset;

        // checks month and date within the
        // valid range of a specified zodiac
        if (month == 12)
        {

            if (day < 22)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Sagittarius;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Capricorn;
        }

        else if (month == 1)
        {
            if (day < 20)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Capricorn;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Aquarius;
        }

        else if (month == 2)
        {
            if (day < 19)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Aquarius;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
        }

        else if (month == 3)
        {
            if (day < 21)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
        }
        else if (month == 4)
        {
            if (day < 20)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Taurus;
        }

        else if (month == 5)
        {
            if (day < 21)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Taurus;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Gemini;
        }

        else if (month == 6)
        {
            if (day < 21)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Gemini;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Cancer;
        }

        else if (month == 7)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Cancer;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Leo;
        }

        else if (month == 8)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Leo;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Virgo;
        }

        else if (month == 9)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Virgo;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Libra;
        }

        else if (month == 10)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Libra;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Scorpio;
        }

        else if (month == 11)
        {
            if (day < 22)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Scorpio;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Sagittarius;
        }

        return astro_sign;
    }
    public string GetSnapchatConfirmationLink(string body)
    {
        var regex = new Regex(@"https:\/\/accounts\.snapchat\.com\/accounts\/confirm_email\?n=[^\s'""']+");
        return regex.Match(body).Value;
    }
    public virtual async Task<ValidationStatus> VerifyEmail(string url)
    {
        try
        {
            url = url.Replace("</a></span><br/>", "");

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = Config.Proxy,
            };

            using HttpClient client = new HttpClient(handler: httpClientHandler, disposeHandler: true);


            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Version = HttpVersion.Version20;
                request.Headers.Clear();
                request.Headers.TryAddWithoutValidation("sec-ch-ua", @"""Google Chrome"";v=""119"", ""Chromium"";v=""119"", ""Not=A?Brand"";v=""24""");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?1");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", @"""Android""");
                request.Headers.TryAddWithoutValidation("upgrade-insecure-requests", "1");
                request.Headers.TryAddWithoutValidation("User-Agent", @"Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Mobile Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.TryAddWithoutValidation("sec-fetch-site", "none");
                request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
                request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
                request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");

                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (responseBody.Contains("Email Address Verified") || responseBody.Contains("verified") | responseBody.Contains("Thanks!"))
                        return ValidationStatus.Validated;
                }
            }
        }
        catch (Exception)
        {
            return ValidationStatus.FailedValidation;
        }

        return ValidationStatus.FailedValidation;
    }
    private static bool IsVersionHigherOrEqual(string version, string targetVersion)
    {
        var currentVersion = Array.ConvertAll(version.Split('.'), int.Parse);
        var targetVersionArray = Array.ConvertAll(targetVersion.Split('.'), int.Parse);

        for (int i = 0; i < currentVersion.Length; i++)
        {
            if (currentVersion[i] > targetVersionArray[i]) return true;
            if (currentVersion[i] < targetVersionArray[i]) return false;
        }

        return true;
    }
    public virtual async Task<SCJanusRegisterWithUsernamePasswordResponse> RegisterNoIntegrityNoKeyAttest(string username, string password, string firstname, string lastname, string email)
    {
        try
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname))
            {
                throw new Exception("Missing required fields");
            }

            if (IsVersionHigherOrEqual(SnapchatClient.SnapchatConfig.androidOsVersion, "7.0.0"))
            {
                throw new Exception("androidOsVersion must be 5.0 - 6.1.1");
            }

            using (var httpClientIPAPI = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All, Proxy = Config.Proxy }))
            {
                HttpResponseMessage response;

                try
                {
                    response = await httpClientIPAPI.GetAsync($"https://pro.ip-api.com/json?fields=timezone,proxy,hosting,query&key={Config.IPAPIPROKEY}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error fetching IP API data. Status code: {response.StatusCode}");
                    }

                    var result = JsonSerializer.Deserialize<IP_API_PRO>(await response.Content.ReadAsStringAsync());

                    Config.TimeZone = result.timezone;
                    if (Config.IPAbuseCheck || Config.IPReuseCheck)
                    {
                        if (Config.IPReuseCheck)
                        {
                            /*
                             * Implement IP reuse check here
                             */
                        }

                        if (Config.IPAbuseCheck)
                        {
                            /*
                            using (var ipAbuseCheck = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }))
                            {
                                var response2 = await ipAbuseCheck.GetAsync($"https://www.ipqualityscore.com/api/json/ip/key_here/{result.query}?strictness=0&allow_public_access_points=true&fast=true&lighter_penalties=true&mobile=true");

                                if (!response2.IsSuccessStatusCode)
                                {
                                    throw new HttpRequestException($"Error checking IP abuse. Status code: {response2.StatusCode}");
                                }
                                var result2 = JsonSerializer.Deserialize<IPQUALITYSCORE>(await response2.Content.ReadAsStringAsync());

                                if (result.proxy || result.hosting || result2.active_vpn || result2.recent_abuse || result2.bot_status)
                                {
                                    throw new Exception("FLAGGED IP");
                                }
                            }
                            */
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    if (Config.Debug)
                    {
                        // Handle HTTP request exceptions here
                        throw new Exception($"HTTP Request failed: {httpEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (Config.Debug)
                    {
                        // Handle other exceptions here
                        throw new Exception($"An error occurred: {ex.Message}");
                    }
                }
            }

            // Get device information
            m_Logger.Debug("Getting Device Info");
            Config.androidId = AndroidIDGenerator.GenerateAndroidID();

            if (Config.SnapchatVersion == SnapchatVersion.V12_76_0_38)
            {
                Config.install_time = m_Utilities.GetInstallTimeStamp(1709608343000);
            }

            SnapchatGrpcClient.SetupServiceClients();

            // Set up user information
            Config.Username = username;
            Random rnd = new Random();
            DateTime datetoday = DateTime.Now;
            int rndYear = rnd.Next(1965, 2004);
            int rndMonth = rnd.Next(1, 12);
            int rndDay = rnd.Next(1, 28);
            Config.DeviceID = Guid.NewGuid().ToString();
            Config.Age = m_Utilities.GetAge(new DateTime(rndYear, rndMonth, rndDay));
            Config.Horoscope = zodiac_sign(rndDay, rndMonth);
            Config.ClientID = Guid.NewGuid().ToString();
            // Set up targeting information
            SCCofConfigTargetingResponse cof = null;
            var BlizzardSessionId = "f2." + m_Utilities.RandomBlizzardSessionId(16);

            int open_to_trigger_delay = new Random().Next(117, 131);
            int time_from_open_app = new Random().Next(234, 337);
            long trigger_timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - new Random().Next(50, 200) - time_from_open_app) / 1000;
            var _SCCofConfigTargetingRequest = new SCCofConfigTargetingRequest
            {
                ScreenHeight = 1080,
                ScreenWidth = 1080,
                Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi, IsRoaming = false, IsMetered = false },
                MaxVideoHeightPx = 1080,
                MaxVideoWidthPx = 2160,
                DeltaSync = true,
                TriggerEventType = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingTriggerEventType.ForegroundTrigger,
                AppState = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingAppState.Foreground,
                DeviceId = Config.DeviceID,
                IsUnAuthorized = true,
                AppLocale = "en",
                Instrumentation = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingInstrumentation.UserAuthentication,
                SyncTriggerBlizzardSessionId = BlizzardSessionId,
                SyncExecutionBlizzardSessionId = BlizzardSessionId,
                CofSyncExecutionDelayFromStartupMs = open_to_trigger_delay,
                CofSyncTriggerDelayFromStartupMs = time_from_open_app,
                SyncTriggerTime = trigger_timestamp,
                LenscoreVersion = 237,
                ClientId = Config.ClientID,
            };
            cof = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequest, false);
            Config.AccountCountryCode = cof.Iso3166Alpha2CountryCodeFromRequestIp;

            // Set up configuration data for Android
            List<int> cofConfigData_Android = new List<int>();
            foreach (var Config in cof.ConfigResultsArray)
            {
                if (SnapchatClient.SnapchatConfig.SnapchatVersion == SnapchatVersion.V12_76_0_38)
                {
                    switch (Config.ConfigId)
                    {
                        case "ACTIVATION_CORE_DUMMY_CONFIG":
                        case "ADS_MINIMUM_VERSION_WITHOUT_INTEGRITY_REQUIREMENT":
                        case "ADS_USE_SIMPLIFIED_INTEGRITY_LOGIC":
                        case "ALLOW_RECYCLED_USERNAME":
                        case "ALLOW_RECYCLED_USERNAME_REG":
                        case "ANDROID_BACKUP_EXECUTE_BACKUP":
                        case "ANDROID_FORCE_LOGOUT_SESSION_MISMATCH":
                        case "ANDROID_PREPROMPT_SKIP_USER_PROMPT":
                        case "ANDROID_PROCESS_REG_RESPONSE_IN_PARALLEL":
                        case "BILLBOARD_CDCP_CAMPAIGN_CATEGORY_FHP_1_INTERACTION_3_IMPRESSION":
                        case "CHANGE_PASSWORD_TAKEOVER":
                        case "CLIENT_USERNAME_SUGGESTION":
                        case "CLIENT_USERNAME_SUGGESTION_CONFIG":
                        case "DEVICE_ID_SURVEY_DATA_COLLECTION_ENABLED":
                        case "DUAL_SIM_MULTISELECT_UI":
                        case "EMAIL_DOMAIN_SUGGESTIONS_LIST_IN_REG":
                        case "ENABLE_AES_UPDATE_EMAIL_SETTINGS":
                        case "ENABLE_AUTOFILL_FROM_ANY_SIM_WITHOUT_COUNTRY_CODE_CHECK":
                        case "ENABLE_BACK_BUTTON_ON_VERIFY_PHONE":
                        case "ENABLE_COF_BASED_AGE_GATING":
                        case "ENABLE_CONTACT_SYNC_PREPROMPT":
                        case "ENABLE_PHONE_AUTOFILL_FROM_ANY_SIM":
                        case "ENABLE_PHONE_PICKER_CURSOR_FIX":
                        case "ENABLE_SET_EMAIL_RETRYABLE_ERROR":
                        case "ENABLE_SMS_LOGIN_AUTO_ADVANCE":
                        case "FIDELIUS_ASYNC_DELETE_MEDIA_KEYS":
                        case "FIDELIUS_BLOCKSTORE_TIMEOUT":
                        case "FIDELIUS_DEVICE_ORDER_SKIP":
                        case "FIDELIUS_IGNORE_EMPTY_SELF_FRIENDLINK":
                        case "FIDELIUS_KEY_PROVIDER_CACHE":
                        case "FIDELIUS_KEY_PROVIDER_SELF_KEY_LOAD":
                        case "FIDELIUS_KEY_VERSION_BUMP":
                        case "FIDELIUS_MESH_RETRY_ACKNOWLEDGE":
                        case "FIDELIUS_MESH_RETRY_CLEAR":
                        case "FIDELIUS_MESH_RETRY_INIT":
                        case "FIDELIUS_READ_FROM_ARCHIVES":
                        case "FIDELIUS_READ_FROM_BLOCKSTORE":
                        case "FIDELIUS_SERVER_FRIEND_KEY_BACKFILL":
                        case "FIDELIUS_SINGLE_DB_KEYPROVIDER":
                        case "FIDELIUS_SKIP_FETCH_FRIEND_KEYS":
                        case "FIDELIUS_USE_VERSION_FROM_DB":
                        case "FIDELIUS_WRITE_TO_ARCHIVES":
                        case "FIDELIUS_WRITE_TO_BLOCKSTORE":
                        case "FID_ANDROID_BACKFILL_FRIEND_KEYS":
                        case "FIREBASE_INITIALIZATION_LOCATION":
                        case "FRND_SHOW_CONTACT_PROMPT_ON_CONTACT_BOOK_SYNC_DISABLED":
                        case "JANUS_LOGIN_CONFIG":
                        case "KEYBOARD_AUTO_POPUP_ON_DISPLAY_NAME_PAGE":
                        case "MANAGE_SPACE_KEEP_FIDELIUS_DATA":
                        case "NGO_REGISTRATION_NATIVE":
                        case "ONE_TAP_LOGGED_IN_REFRESH_JOB_ENABLED":
                        case "REDIRECT_FROM_LOGIN_TO_REGISTRATION":
                        case "REDIRECT_FROM_LOGIN_TO_REG_AUTOFILL":
                        case "REGISTRATION_ALLOW_RETRY_ERROR_WITHOUT_INTERNET":
                        case "SAAC_MIGRATE_SAFE_URL":
                        case "SAAC_SKIP_DEVICE_ID_FETCH_ANDROID":
                        case "SHOW_OPT_IN_OTL_AT_LOGIN":
                        case "SPLASH_PAGE_LAYOUT":
                        case "SPLASH_PAGE_SIGN_UP_STRING":
                        case "TOS_V12_ENABLED":
                        case "UPDATE_OTL_ON_TFA_UPDATE":
                        case "feature_suggestions_config":
                        case "preauth_attestation_configuration":
                            cofConfigData_Android.Add(Config.SequenceId);
                            break;
                    }
                }
            }

            var nonce = m_Utilities.GenerateRandomBytes(16);
            var cleaned = Convert.ToBase64String(nonce).TrimEnd('=')  // Remove any trailing '='
             .Replace('+', '-')  // '+' => '-'
             .Replace('/', '_');  // '/' => '_'

            await Task.Delay(new Random().Next(15000, 20000));

            var SuggestUsernameResponse = await SnapchatGrpcClient.SuggestUsernameAsync(
            new SCSuggestUsernamePbSuggestUsernameRequestV2
            {
                NameAndBirthdate = new SCSuggestUsernamePbSuggestUsernameRequestV2.Types.NameAndBirthdate { FirstName = firstname + " " + lastname },
                ClientId = Config.ClientID,
                DeviceId = Config.DeviceID,
                VersionNumber = 0,
            });

            await Task.Delay(new Random().Next(15000, 20000));

            // Register the user
            var CheckUsernameResponse = await SnapchatGrpcClient.CheckUsernameAsync(
            new SCSuggestUsernamePbCheckUsernameRequestV2
            {
                RequestedUsername = username,
                ClientId = Config.ClientID,
                DeviceId = Config.DeviceID,
                VersionNumber = 0,
            });

            await Task.Delay(new Random().Next(15000, 20000));

            if (CheckUsernameResponse != null)
            {
                if (CheckUsernameResponse.State != SCSuggestUsernamePbCheckUsernameResponseV2.Types.SCSuggestUsernamePbCheckUsernameResponse_RequestedUsernameState.UsernameAvailable)
                {
                    throw new Exception(CheckUsernameResponse.State.ToString());
                }
            }

            var signregister = await SnapchatGrpcClient.Sign("/snapchat.janus.api/RegistrationService/RegisterWithUsernamePassword");

            await Task.Delay(new Random().Next(15000, 20000));

            var fidelius = FideliusDevice.Create(5);

            var _SCJanusRegisterWithUsernamePasswordRequest = new SCJanusRegisterWithUsernamePasswordRequest
            {
                FirstName = firstname + " " + lastname,
                LastName = "",
                Username = username,
                Password = password,
                Birthdate = new Birthday { Day = rndDay, Month = rndMonth, Year = rndYear },
                PredictedPhoneNumberCountryCode = Config.AccountCountryCode,
                TimeZone = Config.TimeZone,
                AutofillSource = SCJanusRegisterWithUsernamePasswordRequest.Types.SCJanusPhoneAutofillSource.Empty,
                RegistrationHeader = new SCJanusRegistrationHeader
                {
                    AuthenticationSessionId = Guid.NewGuid().ToString(),
                    BlizzardClientId = Config.ClientID,
                    NetworkRequestId = Guid.NewGuid().ToString(),
                    RegistrationFlowSessionId = Guid.NewGuid().ToString(),
                    ClientAttestationPayload = ByteString.CopyFrom(Convert.FromBase64String(signregister.Attestation.Replace('-', '+').Replace('_', '/'))),
                    FideliusClientInit = new SCJanusFideliusClientInit
                    {
                        HashedPublicKeysArray =
                        {
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[0]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[1]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[2]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[3]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[4])
                        },
                        TentativeDeviceKey = new SCJanusFideliusTentativeDeviceKey
                        {
                            PublicKey = ByteString.CopyFrom(fidelius.PublicUnwrapped),
                            HashedPublicKey = ByteString.CopyFrom(fidelius.PublicHash),
                            Iwek = ByteString.CopyFrom(fidelius.Iwek),
                            Version = 9
                        }
                    },
                    AttemptNumber = 1,
                },
                UnknownZero = 0,
            };
            var resp = await SnapchatGrpcClient.RegisterAsyncV2(_SCJanusRegisterWithUsernamePasswordRequest);
            // Check for errors
            if (resp.ErrorData != null && !string.IsNullOrEmpty(resp.ErrorData.HumanReadableErrorMessage))
            {
                throw new Exception(resp.ErrorData.HumanReadableErrorMessage);
            }
            // Set up user authentication tokens and access tokens
            if (resp.BootstrapData.UserSession.AuthToken != null)
            {
                Config.refreshToken = resp.BootstrapData.UserSession.SnapSessionResponse.RefreshToken;
                Config.user_id = resp.BootstrapData.UserSession.UserId;
                Config.dtoken1i = resp.BootstrapData.SecurityData.DeviceToken.IdP;
                Config.dtoken1v = resp.BootstrapData.SecurityData.DeviceToken.Value;
                m_Logger.Debug($"User_id: {Config.user_id}");
                m_Logger.Debug("Registration complete");

                foreach (var snapAccessTokensArray in resp.BootstrapData.UserSession.SnapSessionResponse.SnapAccessTokensArray)
                {

                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway")
                    {
                        Config.Access_Token = snapAccessTokensArray.AccessToken;
                    }
                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts")
                    {
                        Config.BusinessAccessToken = snapAccessTokensArray.AccessToken;
                    }
                }
                Config.AccessTokenExpirey = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).AddHours(25).ToUnixTimeMilliseconds();
                var cofafterregisterresult = SCCofConfigTargetingResponse.Parser.ParseFrom(resp.BootstrapData.Cof);
                int open_to_trigger_delay_2 = new Random().Next(2530, 5600);
                int time_from_open_app_2 = new Random().Next(open_to_trigger_delay_2 + 20, open_to_trigger_delay_2 + 30);
                long trigger_timestamp_2 = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - new Random().Next(50, 200) - time_from_open_app_2) / 1000;
                var _SCCofConfigTargetingRequestAfterRegister = new SCCofConfigTargetingRequest
                {
                    ConfigResultsEtag = cofafterregisterresult.ConfigResultsEtag,
                    ScreenHeight = 1440,
                    ScreenWidth = 1440,
                    Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi, IsRoaming = false, IsMetered = false },
                    MaxVideoHeightPx = 1440,
                    MaxVideoWidthPx = 2910,
                    DeltaSync = true,
                    TriggerEventType = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingTriggerEventType.ForegroundTrigger,
                    AppState = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingAppState.Foreground,
                    DeviceId = Config.DeviceID,
                    AppLocale = "en",
                    SyncTriggerBlizzardSessionId = BlizzardSessionId,
                    SyncExecutionBlizzardSessionId = BlizzardSessionId,
                    CofSyncExecutionDelayFromStartupMs = open_to_trigger_delay_2,
                    CofSyncTriggerDelayFromStartupMs = time_from_open_app_2,
                    SyncTriggerTime = trigger_timestamp_2,
                    LenscoreVersion = 237,
                    ClientId = Config.ClientID,
                };
                var mscofbinresult = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequestAfterRegister, true);
                Config.mcs_cof_ids_bin = new List<int>();
                foreach (var ConfigResults in mscofbinresult.ConfigResultsArray)
                {
                    if (ConfigResults.SequenceId != 0)
                    {
                        if (ConfigResults.ConfigId == "GROUP_MESSAGE_RETENTION_COF" ||
                            ConfigResults.ConfigId == "EEL_RECEIVE_CONFIG" ||
                            ConfigResults.ConfigId == "ALLOW_REPLAY_AGAIN" ||
                            ConfigResults.ConfigId == "MERLIN_WELCOME_CARD_ENABLED" ||
                            ConfigResults.ConfigId == "MERLIN_WELCOME_CARD_CONFIG" ||
                            ConfigResults.ConfigId == "HOURS_AFTER_NO_INTERACTION_TO_SEND_MERLIN_WELCOME_MESSAGE" ||
                            ConfigResults.ConfigId == "MAX_HOURS_AFTER_STREAK_EXPIRE_TO_ENABLE_RESTORE" ||
                            ConfigResults.ConfigId == "STREAK_RESTORE_NO_CAPTURE_ENABLED" ||
                            ConfigResults.ConfigId == "MIN_STREAK_COUNT_TO_ENABLE_RESTORE" ||
                            ConfigResults.ConfigId == "ALLOW_DISABLE_SAVING_FOR_PENDING_CONVERSATION" ||
                            ConfigResults.ConfigId == "ENABLE_STABLE_FEED_ORDER_AFTER_SEND_MASS_SNAP" ||
                            ConfigResults.ConfigId == "MASS_FEED_REORDERING_FEED_DECREMENT_TIMESTAMPS" ||
                            ConfigResults.ConfigId == "TEAMSNAPCHAT_CONV_WALLPAPER" ||
                            ConfigResults.ConfigId == "one_on_one_24_hr_retention_config" ||
                            ConfigResults.ConfigId == "SNAP_SCORE_INCREMENT_VARIATIONS" ||
                            ConfigResults.ConfigId == "TOP_GROUPS_UPDATE_FREQ_IN_MS" ||
                            ConfigResults.ConfigId == "EXPIRE_SNAP_AFTER_VIEWED_BY_CURRENT_USER" ||
                            ConfigResults.ConfigId == "24_HOUR_SNAPS_MODE" ||
                            ConfigResults.ConfigId == "EEL_CLIENT_CONFIG" ||
                            ConfigResults.ConfigId == "NON_FRIEND_MESSAGING")
                        {
                            Config.mcs_cof_ids_bin.Add(ConfigResults.SequenceId);
                        }
                    }
                }
                await SnapchatGrpcClient.PollRecrypt(new SCFideliusPollRecryptRequest { PublicKey = ByteString.CopyFrom(fidelius.PublicUnwrapped) });
                await Task.Delay(new Random().Next(10000, 15000));
                await SnapchatClient.ChangeEmail(email);
                await Task.Delay(new Random().Next(30000, 60000));
                await SnapchatClient.ResendEmail(email);
                await Task.Delay(new Random().Next(10000, 15000));
            }
            return resp;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
    public virtual async Task<SCJanusRegisterWithUsernamePasswordResponse> RegisterTest(string username, string password, string firstname, string lastname, string email)
    {
        try
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname))
            {
                throw new Exception("Missing required fields");
            }

            using (var httpClientIPAPI = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All, Proxy = Config.Proxy }))
            {
                HttpResponseMessage response;

                try
                {
                    response = await httpClientIPAPI.GetAsync($"https://pro.ip-api.com/json?fields=timezone,proxy,hosting,query&key={Config.IPAPIPROKEY}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error fetching IP API data. Status code: {response.StatusCode}");
                    }

                    var result = JsonSerializer.Deserialize<IP_API_PRO>(await response.Content.ReadAsStringAsync());

                    Config.TimeZone = result.timezone;
                    if (Config.IPAbuseCheck || Config.IPReuseCheck)
                    {
                        if (Config.IPReuseCheck)
                        {
                            /*
                             * Implement IP reuse check here
                             */
                        }

                        if (Config.IPAbuseCheck)
                        {
                            /*
                            using (var ipAbuseCheck = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }))
                            {
                                var response2 = await ipAbuseCheck.GetAsync($"https://www.ipqualityscore.com/api/json/ip/key_here/{result.query}?strictness=0&allow_public_access_points=true&fast=true&lighter_penalties=true&mobile=true");

                                if (!response2.IsSuccessStatusCode)
                                {
                                    throw new HttpRequestException($"Error checking IP abuse. Status code: {response2.StatusCode}");
                                }
                                var result2 = JsonSerializer.Deserialize<IPQUALITYSCORE>(await response2.Content.ReadAsStringAsync());

                                if (result.proxy || result.hosting || result2.active_vpn || result2.recent_abuse || result2.bot_status)
                                {
                                    throw new Exception("FLAGGED IP");
                                }
                            }
                            */
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    if (Config.Debug)
                    {
                        // Handle HTTP request exceptions here
                        throw new Exception($"HTTP Request failed: {httpEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (Config.Debug)
                    {
                        // Handle other exceptions here
                        throw new Exception($"An error occurred: {ex.Message}");
                    }
                }
            }

            // Get device information
            m_Logger.Debug("Getting Device Info");
            Config.androidId = AndroidIDGenerator.GenerateAndroidID();

            if (Config.SnapchatVersion == SnapchatVersion.V12_76_0_38)
            {
                Config.install_time = m_Utilities.GetInstallTimeStamp(1709608343000);
            }

            SnapchatGrpcClient.SetupServiceClients();

            // Set up user information
            Config.Username = username;
            Random rnd = new Random();
            DateTime datetoday = DateTime.Now;
            int rndYear = rnd.Next(1965, 2004);
            int rndMonth = rnd.Next(1, 12);
            int rndDay = rnd.Next(1, 28);
            Config.DeviceID = Guid.NewGuid().ToString();
            Config.Age = m_Utilities.GetAge(new DateTime(rndYear, rndMonth, rndDay));
            Config.Horoscope = zodiac_sign(rndDay, rndMonth);
            Config.ClientID = Guid.NewGuid().ToString();
            SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.VendorAttestationLibaryUnknownUnset;
            // Set up targeting information
            SCCofConfigTargetingResponse cof = null;
            var BlizzardSessionId = "f2." + m_Utilities.RandomBlizzardSessionId(16);

            int open_to_trigger_delay = new Random().Next(117, 131);
            int time_from_open_app = new Random().Next(234, 337);
            long trigger_timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - new Random().Next(50, 200) - time_from_open_app) / 1000;
            var _SCCofConfigTargetingRequest = new SCCofConfigTargetingRequest
            {
                ScreenHeight = 1080,
                ScreenWidth = 1080,
                Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi, IsRoaming = false, IsMetered = false },
                MaxVideoHeightPx = 2160,
                MaxVideoWidthPx = 1080,
                DeltaSync = true,
                TriggerEventType = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingTriggerEventType.ForegroundTrigger,
                AppState = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingAppState.Foreground,
                DeviceId = Config.DeviceID,
                IsUnAuthorized = true,
                AppLocale = "en",
                Instrumentation = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingInstrumentation.UserAuthentication,
                SyncTriggerBlizzardSessionId = BlizzardSessionId,
                SyncExecutionBlizzardSessionId = BlizzardSessionId,
                CofSyncExecutionDelayFromStartupMs = open_to_trigger_delay,
                CofSyncTriggerDelayFromStartupMs = time_from_open_app,
                SyncTriggerTime = trigger_timestamp,
                LenscoreVersion = 237,
                ClientId = Config.ClientID,
            };
            cof = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequest, false);
            Config.AccountCountryCode = cof.Iso3166Alpha2CountryCodeFromRequestIp;
            Console.WriteLine(Config.ClientID);
            // Set up configuration data for Android
            List<int> cofConfigData_Android = new List<int>();
            foreach (var Config in cof.ConfigResultsArray)
            {
                if (Config.ConfigId.Trim().Equals("ANDROID_CLIENT_INTEGRITY_ON_REGISTRATION"))
                {
                    if (Config.Value.StringValue.Trim().Equals("PLAY_INTEGRITY_AND_KEY_ATTESTATION"))
                    {
                        Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.GooglePlayIntegrityClassic;
                    }
                    else if (Config.Value.StringValue.Trim().Equals("PLAY_INTEGRITY_STANDARD_AND_KEY_ATTESTATION"))
                    {
                        Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.GooglePlayIntegrityStandard;
                    }
                    else
                    {
                        throw new Exception($"{Config.Value.StringValue} NOT SUPPORTED");
                    }
                }
                
                if (SnapchatClient.SnapchatConfig.SnapchatVersion == SnapchatVersion.V12_76_0_38)
                {
                    switch (Config.ConfigId)
                    {
                        case "ACTIVATION_CORE_DUMMY_CONFIG":
                        case "ADS_MINIMUM_VERSION_WITHOUT_INTEGRITY_REQUIREMENT":
                        case "ADS_USE_SIMPLIFIED_INTEGRITY_LOGIC":
                        case "ALLOW_RECYCLED_USERNAME":
                        case "ALLOW_RECYCLED_USERNAME_REG":
                        case "ANDROID_BACKUP_EXECUTE_BACKUP":
                        case "ANDROID_FORCE_LOGOUT_SESSION_MISMATCH":
                        case "ANDROID_PREPROMPT_SKIP_USER_PROMPT":
                        case "ANDROID_PROCESS_REG_RESPONSE_IN_PARALLEL":
                        case "BILLBOARD_CDCP_CAMPAIGN_CATEGORY_FHP_1_INTERACTION_3_IMPRESSION":
                        case "CHANGE_PASSWORD_TAKEOVER":
                        case "CLIENT_USERNAME_SUGGESTION":
                        case "CLIENT_USERNAME_SUGGESTION_CONFIG":
                        case "DEVICE_ID_SURVEY_DATA_COLLECTION_ENABLED":
                        case "DUAL_SIM_MULTISELECT_UI":
                        case "EMAIL_DOMAIN_SUGGESTIONS_LIST_IN_REG":
                        case "ENABLE_AES_UPDATE_EMAIL_SETTINGS":
                        case "ENABLE_AUTOFILL_FROM_ANY_SIM_WITHOUT_COUNTRY_CODE_CHECK":
                        case "ENABLE_BACK_BUTTON_ON_VERIFY_PHONE":
                        case "ENABLE_COF_BASED_AGE_GATING":
                        case "ENABLE_CONTACT_SYNC_PREPROMPT":
                        case "ENABLE_PHONE_AUTOFILL_FROM_ANY_SIM":
                        case "ENABLE_PHONE_PICKER_CURSOR_FIX":
                        case "ENABLE_SET_EMAIL_RETRYABLE_ERROR":
                        case "ENABLE_SMS_LOGIN_AUTO_ADVANCE":
                        case "FIDELIUS_ASYNC_DELETE_MEDIA_KEYS":
                        case "FIDELIUS_BLOCKSTORE_TIMEOUT":
                        case "FIDELIUS_DEVICE_ORDER_SKIP":
                        case "FIDELIUS_IGNORE_EMPTY_SELF_FRIENDLINK":
                        case "FIDELIUS_KEY_PROVIDER_CACHE":
                        case "FIDELIUS_KEY_PROVIDER_SELF_KEY_LOAD":
                        case "FIDELIUS_KEY_VERSION_BUMP":
                        case "FIDELIUS_MESH_RETRY_ACKNOWLEDGE":
                        case "FIDELIUS_MESH_RETRY_CLEAR":
                        case "FIDELIUS_MESH_RETRY_INIT":
                        case "FIDELIUS_READ_FROM_ARCHIVES":
                        case "FIDELIUS_READ_FROM_BLOCKSTORE":
                        case "FIDELIUS_SERVER_FRIEND_KEY_BACKFILL":
                        case "FIDELIUS_SINGLE_DB_KEYPROVIDER":
                        case "FIDELIUS_SKIP_FETCH_FRIEND_KEYS":
                        case "FIDELIUS_USE_VERSION_FROM_DB":
                        case "FIDELIUS_WRITE_TO_ARCHIVES":
                        case "FIDELIUS_WRITE_TO_BLOCKSTORE":
                        case "FID_ANDROID_BACKFILL_FRIEND_KEYS":
                        case "FIREBASE_INITIALIZATION_LOCATION":
                        case "FRND_SHOW_CONTACT_PROMPT_ON_CONTACT_BOOK_SYNC_DISABLED":
                        case "JANUS_LOGIN_CONFIG":
                        case "KEYBOARD_AUTO_POPUP_ON_DISPLAY_NAME_PAGE":
                        case "MANAGE_SPACE_KEEP_FIDELIUS_DATA":
                        case "NGO_REGISTRATION_NATIVE":
                        case "ONE_TAP_LOGGED_IN_REFRESH_JOB_ENABLED":
                        case "REDIRECT_FROM_LOGIN_TO_REGISTRATION":
                        case "REDIRECT_FROM_LOGIN_TO_REG_AUTOFILL":
                        case "REGISTRATION_ALLOW_RETRY_ERROR_WITHOUT_INTERNET":
                        case "SAAC_MIGRATE_SAFE_URL":
                        case "SAAC_SKIP_DEVICE_ID_FETCH_ANDROID":
                        case "SHOW_OPT_IN_OTL_AT_LOGIN":
                        case "SPLASH_PAGE_LAYOUT":
                        case "SPLASH_PAGE_SIGN_UP_STRING":
                        case "TOS_V12_ENABLED":
                        case "UPDATE_OTL_ON_TFA_UPDATE":
                        case "feature_suggestions_config":
                        case "preauth_attestation_configuration":
                            cofConfigData_Android.Add(Config.SequenceId);
                            break;
                    }
                }
            }

            await Task.Delay(new Random().Next(15000, 20000));

            var SuggestUsernameResponse = await SnapchatGrpcClient.SuggestUsernameAsync(
            new SCSuggestUsernamePbSuggestUsernameRequestV2
            {
                NameAndBirthdate = new SCSuggestUsernamePbSuggestUsernameRequestV2.Types.NameAndBirthdate { FirstName = firstname + " " + lastname },
                ClientId = Config.ClientID,
                DeviceId = Config.DeviceID,
                VersionNumber = 0,
            });

            // Register the user
            var CheckUsernameResponse = await SnapchatGrpcClient.CheckUsernameAsync(
            new SCSuggestUsernamePbCheckUsernameRequestV2
            {
                RequestedUsername = username,
                ClientId = Config.ClientID,
                DeviceId = Config.DeviceID,
                VersionNumber = 0,
            });

            if (CheckUsernameResponse != null)
            {
                if (CheckUsernameResponse.State != SCSuggestUsernamePbCheckUsernameResponseV2.Types.SCSuggestUsernamePbCheckUsernameResponse_RequestedUsernameState.UsernameAvailable)
                {
                    throw new Exception(CheckUsernameResponse.State.ToString());
                }
            }

            var signregister = await SnapchatGrpcClient.Sign("/snapchat.janus.api/RegistrationService/RegisterWithUsernamePassword");
            var fidelius = FideliusDevice.Create(5);
            var _SCJanusRegisterWithUsernamePasswordRequest = new SCJanusRegisterWithUsernamePasswordRequest
            {
                FirstName = firstname + " " + lastname,
                LastName = "",
                Username = username,
                Password = password,
                Birthdate = new Birthday { Day = rndDay, Month = rndMonth, Year = rndYear },
                PredictedPhoneNumberCountryCode = Config.AccountCountryCode,
                TimeZone = Config.TimeZone,
                AutofillSource = SCJanusRegisterWithUsernamePasswordRequest.Types.SCJanusPhoneAutofillSource.Empty,
                RegistrationHeader = new SCJanusRegistrationHeader
                {
                    AuthenticationSessionId = Guid.NewGuid().ToString(),
                    BlizzardClientId = Config.ClientID,
                    NetworkRequestId = Guid.NewGuid().ToString(),
                    CofConfigData = new SnapchatLibProto.Snapchat.Janus.Api.PartialToken { SequenceIdsArray = { cofConfigData_Android } },
                    CofDeviceId = Config.DeviceID,
                    RegistrationFlowSessionId = Guid.NewGuid().ToString(),
                    ClientAttestationPayload = ByteString.CopyFrom(Convert.FromBase64String(signregister.Attestation.Replace('-', '+').Replace('_', '/'))),
                    FideliusClientInit = new SCJanusFideliusClientInit
                    {
                        HashedPublicKeysArray =
                        {
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[0]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[1]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[2]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[3]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[4])
                        },
                        TentativeDeviceKey = new SCJanusFideliusTentativeDeviceKey
                        {
                            PublicKey = ByteString.CopyFrom(fidelius.PublicUnwrapped),
                            HashedPublicKey = ByteString.CopyFrom(fidelius.PublicHash),
                            Iwek = ByteString.CopyFrom(fidelius.Iwek),
                            Version = 10
                        }
                    },
                    AttemptNumber = 1,
                    VendorAttestationPayloadsArray = { new SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation { AndroidPackageName = "com.snapchat.android", Type = Type, StrictEnforcementsRequired = false, Payload = "time-out", StandardErrorCode = 0 }, new SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation { AndroidPackageName = "com.snapchat.android", Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.VendorAttestationLibaryKeyAttestation, AppAttestEnforcement = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.AppAttestEnforcement.AppAttestEnforcementAppAttestDefaultUnset, StrictEnforcementsRequired = false, Error = Config.androidOsSDK < 24 ? "Device too old" : "time-out", StandardErrorCode = 0 } }
                },
                UnknownZero = 0,
            };
            var resp = await SnapchatGrpcClient.RegisterAsyncV2(_SCJanusRegisterWithUsernamePasswordRequest);
            // Check for errors
            if (resp.ErrorData != null && !string.IsNullOrEmpty(resp.ErrorData.HumanReadableErrorMessage))
            {
                throw new Exception(resp.ErrorData.HumanReadableErrorMessage);
            }
            // Set up user authentication tokens and access tokens
            if (!string.IsNullOrEmpty(resp.BootstrapData.UserSession.UserId))
            {
                Config.refreshToken = resp.BootstrapData.UserSession.SnapSessionResponse.RefreshToken;
                Config.user_id = resp.BootstrapData.UserSession.UserId;
                Config.dtoken1i = resp.BootstrapData.SecurityData.DeviceToken.IdP;
                Config.dtoken1v = resp.BootstrapData.SecurityData.DeviceToken.Value;
                m_Logger.Debug($"User_id: {Config.user_id}");
                m_Logger.Debug("Registration complete");

                foreach (var snapAccessTokensArray in resp.BootstrapData.UserSession.SnapSessionResponse.SnapAccessTokensArray)
                {

                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway")
                    {
                        Config.Access_Token = snapAccessTokensArray.AccessToken;
                    }
                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts")
                    {
                        Config.BusinessAccessToken = snapAccessTokensArray.AccessToken;
                    }
                }
                Config.AccessTokenExpirey = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).AddHours(25).ToUnixTimeMilliseconds();
                var cofafterregisterresult = SCCofConfigTargetingResponse.Parser.ParseFrom(resp.BootstrapData.Cof);
                int open_to_trigger_delay_2 = new Random().Next(2530, 5600);
                int time_from_open_app_2 = new Random().Next(open_to_trigger_delay_2 + 20, open_to_trigger_delay_2 + 30);
                long trigger_timestamp_2 = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - new Random().Next(50, 200) - time_from_open_app_2) / 1000;
                var _SCCofConfigTargetingRequestAfterRegister = new SCCofConfigTargetingRequest
                {
                    ConfigResultsEtag = cofafterregisterresult.ConfigResultsEtag,
                    ScreenHeight = 1080,
                    ScreenWidth = 1080,
                    Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi, IsRoaming = false, IsMetered = false },
                    MaxVideoHeightPx = 2160,
                    MaxVideoWidthPx = 1080,
                    DeltaSync = true,
                    TriggerEventType = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingTriggerEventType.ForegroundTrigger,
                    AppState = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingAppState.Foreground,
                    DeviceId = Config.DeviceID,
                    AppLocale = "en",
                    SyncTriggerBlizzardSessionId = BlizzardSessionId,
                    SyncExecutionBlizzardSessionId = BlizzardSessionId,
                    CofSyncExecutionDelayFromStartupMs = open_to_trigger_delay_2,
                    CofSyncTriggerDelayFromStartupMs = time_from_open_app_2,
                    SyncTriggerTime = trigger_timestamp_2,
                    LenscoreVersion = 237,
                    ClientId = Config.ClientID,
                };
                var mscofbinresult = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequestAfterRegister, true);
                Config.mcs_cof_ids_bin = new List<int>();
                foreach (var ConfigResults in mscofbinresult.ConfigResultsArray)
                {
                    if (ConfigResults.SequenceId != 0)
                    {
                        if (ConfigResults.ConfigId == "GROUP_MESSAGE_RETENTION_COF" ||
                            ConfigResults.ConfigId == "EEL_RECEIVE_CONFIG" ||
                            ConfigResults.ConfigId == "ALLOW_REPLAY_AGAIN" ||
                            ConfigResults.ConfigId == "MERLIN_WELCOME_CARD_ENABLED" ||
                            ConfigResults.ConfigId == "MERLIN_WELCOME_CARD_CONFIG" ||
                            ConfigResults.ConfigId == "HOURS_AFTER_NO_INTERACTION_TO_SEND_MERLIN_WELCOME_MESSAGE" ||
                            ConfigResults.ConfigId == "MAX_HOURS_AFTER_STREAK_EXPIRE_TO_ENABLE_RESTORE" ||
                            ConfigResults.ConfigId == "STREAK_RESTORE_NO_CAPTURE_ENABLED" ||
                            ConfigResults.ConfigId == "MIN_STREAK_COUNT_TO_ENABLE_RESTORE" ||
                            ConfigResults.ConfigId == "ALLOW_DISABLE_SAVING_FOR_PENDING_CONVERSATION" ||
                            ConfigResults.ConfigId == "ENABLE_STABLE_FEED_ORDER_AFTER_SEND_MASS_SNAP" ||
                            ConfigResults.ConfigId == "MASS_FEED_REORDERING_FEED_DECREMENT_TIMESTAMPS" ||
                            ConfigResults.ConfigId == "TEAMSNAPCHAT_CONV_WALLPAPER" ||
                            ConfigResults.ConfigId == "one_on_one_24_hr_retention_config" ||
                            ConfigResults.ConfigId == "SNAP_SCORE_INCREMENT_VARIATIONS" ||
                            ConfigResults.ConfigId == "TOP_GROUPS_UPDATE_FREQ_IN_MS" ||
                            ConfigResults.ConfigId == "EXPIRE_SNAP_AFTER_VIEWED_BY_CURRENT_USER" ||
                            ConfigResults.ConfigId == "24_HOUR_SNAPS_MODE" ||
                            ConfigResults.ConfigId == "EEL_CLIENT_CONFIG" ||
                            ConfigResults.ConfigId == "NON_FRIEND_MESSAGING")
                        {
                            Config.mcs_cof_ids_bin.Add(ConfigResults.SequenceId);
                        }
                    }
                }
                await SnapchatClient.ChangeEmail(email);
                await SnapchatClient.ResendEmail(email);
            }
            return resp;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
    public virtual async Task<SCJanusRegisterWithUsernamePasswordResponse> Register(string username, string password, string firstname, string lastname, string email)
    {
        try
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname))
            {
                throw new Exception("Missing required fields");
            }

            using (var httpClientIPAPI = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All, Proxy = Config.Proxy }))
            {
                HttpResponseMessage response;

                try
                {
                    response = await httpClientIPAPI.GetAsync($"https://pro.ip-api.com/json?fields=timezone,proxy,hosting,query&key={Config.IPAPIPROKEY}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error fetching IP API data. Status code: {response.StatusCode}");
                    }

                    var result = JsonSerializer.Deserialize<IP_API_PRO>(await response.Content.ReadAsStringAsync());

                    Config.TimeZone = result.timezone;
                    if (Config.IPAbuseCheck || Config.IPReuseCheck)
                    {
                        if (Config.IPReuseCheck)
                        {
                            /*
                             * Implement IP reuse check here
                             */
                        }

                        if (Config.IPAbuseCheck)
                        {
                            /*
                            using (var ipAbuseCheck = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }))
                            {
                                var response2 = await ipAbuseCheck.GetAsync($"https://www.ipqualityscore.com/api/json/ip/key_here/{result.query}?strictness=0&allow_public_access_points=true&fast=true&lighter_penalties=true&mobile=true");

                                if (!response2.IsSuccessStatusCode)
                                {
                                    throw new HttpRequestException($"Error checking IP abuse. Status code: {response2.StatusCode}");
                                }
                                var result2 = JsonSerializer.Deserialize<IPQUALITYSCORE>(await response2.Content.ReadAsStringAsync());

                                if (result.proxy || result.hosting || result2.active_vpn || result2.recent_abuse || result2.bot_status)
                                {
                                    throw new Exception("FLAGGED IP");
                                }
                            }
                            */
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    if (Config.Debug)
                    {
                        // Handle HTTP request exceptions here
                        throw new Exception($"HTTP Request failed: {httpEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (Config.Debug)
                    {
                        // Handle other exceptions here
                        throw new Exception($"An error occurred: {ex.Message}");
                    }
                }
            }

            // Get device information
            m_Logger.Debug("Getting Device Info");
            Config.androidId = AndroidIDGenerator.GenerateAndroidID();

            if (Config.SnapchatVersion == SnapchatVersion.V12_76_0_38)
            {
                Config.install_time = m_Utilities.GetInstallTimeStamp(1709608343000);
            }

            SnapchatGrpcClient.SetupServiceClients();

            // Set up user information
            Config.Username = username;
            Random rnd = new Random();
            DateTime datetoday = DateTime.Now;
            int rndYear = rnd.Next(1965, 2004);
            int rndMonth = rnd.Next(1, 12);
            int rndDay = rnd.Next(1, 28);
            Config.DeviceID = Guid.NewGuid().ToString();
            Config.Age = m_Utilities.GetAge(new DateTime(rndYear, rndMonth, rndDay));
            Config.Horoscope = zodiac_sign(rndDay, rndMonth);
            Config.ClientID = Guid.NewGuid().ToString();
            SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.VendorAttestationLibaryUnknownUnset;
            // Set up targeting information
            SCCofConfigTargetingResponse cof = null;
            var BlizzardSessionId = "f2." + m_Utilities.RandomBlizzardSessionId(16);

            int open_to_trigger_delay = new Random().Next(117, 131);
            int time_from_open_app = new Random().Next(234, 337);
            long trigger_timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - new Random().Next(50, 200) - time_from_open_app) / 1000;
            var _SCCofConfigTargetingRequest = new SCCofConfigTargetingRequest
            {
                ScreenHeight = 1080,
                ScreenWidth = 1080,
                Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi, IsRoaming = false, IsMetered = false },
                MaxVideoHeightPx = 1080,
                MaxVideoWidthPx = 2160,
                DeltaSync = true,
                TriggerEventType = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingTriggerEventType.ForegroundTrigger,
                AppState = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingAppState.Foreground,
                DeviceId = Config.DeviceID,
                IsUnAuthorized = true,
                AppLocale = "en",
                Instrumentation = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingInstrumentation.UserAuthentication,
                SyncTriggerBlizzardSessionId = BlizzardSessionId,
                SyncExecutionBlizzardSessionId = BlizzardSessionId,
                CofSyncExecutionDelayFromStartupMs = open_to_trigger_delay,
                CofSyncTriggerDelayFromStartupMs = time_from_open_app,
                SyncTriggerTime = trigger_timestamp,
                LenscoreVersion = 237,
                ClientId = Config.ClientID,
            };
            cof = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequest, false);
            Config.AccountCountryCode = cof.Iso3166Alpha2CountryCodeFromRequestIp;

            // Set up configuration data for Android
            List<int> cofConfigData_Android = new List<int>();
            foreach (var Config in cof.ConfigResultsArray)
            {
                if (Config.ConfigId.Trim().Equals("ANDROID_CLIENT_INTEGRITY_ON_REGISTRATION"))
                {
                    if (Config.Value.StringValue.Trim().Equals("PLAY_INTEGRITY_AND_KEY_ATTESTATION"))
                    {
                        Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.GooglePlayIntegrityClassic;
                    }
                    else if (Config.Value.StringValue.Trim().Equals("PLAY_INTEGRITY_STANDARD_AND_KEY_ATTESTATION"))
                    {
                        Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.GooglePlayIntegrityStandard;
                    }
                    else
                    {
                        throw new Exception($"{Config.Value.StringValue} NOT SUPPORTED");
                    }
                }
                if (SnapchatClient.SnapchatConfig.SnapchatVersion == SnapchatVersion.V12_76_0_38)
                {
                    switch (Config.ConfigId)
                    {
                        case "ACTIVATION_CORE_DUMMY_CONFIG":
                        case "ADS_MINIMUM_VERSION_WITHOUT_INTEGRITY_REQUIREMENT":
                        case "ADS_USE_SIMPLIFIED_INTEGRITY_LOGIC":
                        case "ALLOW_RECYCLED_USERNAME":
                        case "ALLOW_RECYCLED_USERNAME_REG":
                        case "ANDROID_BACKUP_EXECUTE_BACKUP":
                        case "ANDROID_FORCE_LOGOUT_SESSION_MISMATCH":
                        case "ANDROID_PREPROMPT_SKIP_USER_PROMPT":
                        case "ANDROID_PROCESS_REG_RESPONSE_IN_PARALLEL":
                        case "BILLBOARD_CDCP_CAMPAIGN_CATEGORY_FHP_1_INTERACTION_3_IMPRESSION":
                        case "CHANGE_PASSWORD_TAKEOVER":
                        case "CLIENT_USERNAME_SUGGESTION":
                        case "CLIENT_USERNAME_SUGGESTION_CONFIG":
                        case "DEVICE_ID_SURVEY_DATA_COLLECTION_ENABLED":
                        case "DUAL_SIM_MULTISELECT_UI":
                        case "EMAIL_DOMAIN_SUGGESTIONS_LIST_IN_REG":
                        case "ENABLE_AES_UPDATE_EMAIL_SETTINGS":
                        case "ENABLE_AUTOFILL_FROM_ANY_SIM_WITHOUT_COUNTRY_CODE_CHECK":
                        case "ENABLE_BACK_BUTTON_ON_VERIFY_PHONE":
                        case "ENABLE_COF_BASED_AGE_GATING":
                        case "ENABLE_CONTACT_SYNC_PREPROMPT":
                        case "ENABLE_PHONE_AUTOFILL_FROM_ANY_SIM":
                        case "ENABLE_PHONE_PICKER_CURSOR_FIX":
                        case "ENABLE_SET_EMAIL_RETRYABLE_ERROR":
                        case "ENABLE_SMS_LOGIN_AUTO_ADVANCE":
                        case "FIDELIUS_ASYNC_DELETE_MEDIA_KEYS":
                        case "FIDELIUS_BLOCKSTORE_TIMEOUT":
                        case "FIDELIUS_DEVICE_ORDER_SKIP":
                        case "FIDELIUS_IGNORE_EMPTY_SELF_FRIENDLINK":
                        case "FIDELIUS_KEY_PROVIDER_CACHE":
                        case "FIDELIUS_KEY_PROVIDER_SELF_KEY_LOAD":
                        case "FIDELIUS_KEY_VERSION_BUMP":
                        case "FIDELIUS_MESH_RETRY_ACKNOWLEDGE":
                        case "FIDELIUS_MESH_RETRY_CLEAR":
                        case "FIDELIUS_MESH_RETRY_INIT":
                        case "FIDELIUS_READ_FROM_ARCHIVES":
                        case "FIDELIUS_READ_FROM_BLOCKSTORE":
                        case "FIDELIUS_SERVER_FRIEND_KEY_BACKFILL":
                        case "FIDELIUS_SINGLE_DB_KEYPROVIDER":
                        case "FIDELIUS_SKIP_FETCH_FRIEND_KEYS":
                        case "FIDELIUS_USE_VERSION_FROM_DB":
                        case "FIDELIUS_WRITE_TO_ARCHIVES":
                        case "FIDELIUS_WRITE_TO_BLOCKSTORE":
                        case "FID_ANDROID_BACKFILL_FRIEND_KEYS":
                        case "FIREBASE_INITIALIZATION_LOCATION":
                        case "FRND_SHOW_CONTACT_PROMPT_ON_CONTACT_BOOK_SYNC_DISABLED":
                        case "JANUS_LOGIN_CONFIG":
                        case "KEYBOARD_AUTO_POPUP_ON_DISPLAY_NAME_PAGE":
                        case "MANAGE_SPACE_KEEP_FIDELIUS_DATA":
                        case "NGO_REGISTRATION_NATIVE":
                        case "ONE_TAP_LOGGED_IN_REFRESH_JOB_ENABLED":
                        case "REDIRECT_FROM_LOGIN_TO_REGISTRATION":
                        case "REDIRECT_FROM_LOGIN_TO_REG_AUTOFILL":
                        case "REGISTRATION_ALLOW_RETRY_ERROR_WITHOUT_INTERNET":
                        case "SAAC_MIGRATE_SAFE_URL":
                        case "SAAC_SKIP_DEVICE_ID_FETCH_ANDROID":
                        case "SHOW_OPT_IN_OTL_AT_LOGIN":
                        case "SPLASH_PAGE_LAYOUT":
                        case "SPLASH_PAGE_SIGN_UP_STRING":
                        case "TOS_V12_ENABLED":
                        case "UPDATE_OTL_ON_TFA_UPDATE":
                        case "feature_suggestions_config":
                        case "preauth_attestation_configuration":
                            cofConfigData_Android.Add(Config.SequenceId);
                            break;
                    }
                }
            }

            await Task.Delay(new Random().Next(15000, 20000));

            var SuggestUsernameResponse = await SnapchatGrpcClient.SuggestUsernameAsync(
            new SCSuggestUsernamePbSuggestUsernameRequestV2
            {
                NameAndBirthdate = new SCSuggestUsernamePbSuggestUsernameRequestV2.Types.NameAndBirthdate { FirstName = firstname + " " + lastname },
                ClientId = Config.ClientID,
                DeviceId = Config.DeviceID,
                VersionNumber = 0,
            });

            await Task.Delay(new Random().Next(15000, 20000));

            // Register the user
            var CheckUsernameResponse = await SnapchatGrpcClient.CheckUsernameAsync(
            new SCSuggestUsernamePbCheckUsernameRequestV2
            {
                RequestedUsername = username,
                ClientId = Config.ClientID,
                DeviceId = Config.DeviceID,
                VersionNumber = 0,
            });

            await Task.Delay(new Random().Next(15000, 20000));

            if (CheckUsernameResponse != null)
            {
                if (CheckUsernameResponse.State != SCSuggestUsernamePbCheckUsernameResponseV2.Types.SCSuggestUsernamePbCheckUsernameResponse_RequestedUsernameState.UsernameAvailable)
                {
                    throw new Exception(CheckUsernameResponse.State.ToString());
                }
            }

            var signregister = await SnapchatGrpcClient.Sign("/snapchat.janus.api/RegistrationService/RegisterWithUsernamePassword");

            await Task.Delay(new Random().Next(15000, 20000));

            var fidelius = FideliusDevice.Create(5);
            var _SCJanusRegisterWithUsernamePasswordRequest = new SCJanusRegisterWithUsernamePasswordRequest
            {
                FirstName = firstname + " " + lastname,
                LastName = "",
                Username = username,
                Password = password,
                Birthdate = new Birthday { Day = rndDay, Month = rndMonth, Year = rndYear },
                PredictedPhoneNumberCountryCode = Config.AccountCountryCode,
                TimeZone = Config.TimeZone,
                AutofillSource = SCJanusRegisterWithUsernamePasswordRequest.Types.SCJanusPhoneAutofillSource.Empty,
                RegistrationHeader = new SCJanusRegistrationHeader
                {
                    AuthenticationSessionId = Guid.NewGuid().ToString(),
                    BlizzardClientId = Config.ClientID,
                    NetworkRequestId = Guid.NewGuid().ToString(),
                    CofConfigData = new SnapchatLibProto.Snapchat.Janus.Api.PartialToken { SequenceIdsArray = { cofConfigData_Android } },
                    CofDeviceId = Config.DeviceID,
                    RegistrationFlowSessionId = Guid.NewGuid().ToString(),
                    ClientAttestationPayload = ByteString.CopyFrom(Convert.FromBase64String(signregister.Attestation.Replace('-', '+').Replace('_', '/'))),
                    FideliusClientInit = new SCJanusFideliusClientInit
                    {
                        HashedPublicKeysArray =
                        {
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[0]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[1]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[2]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[3]),
                            ByteString.CopyFrom(fidelius.HashedPublicKeysArray[4])
                        },
                        TentativeDeviceKey = new SCJanusFideliusTentativeDeviceKey
                        {
                            PublicKey = ByteString.CopyFrom(fidelius.PublicUnwrapped),
                            HashedPublicKey = ByteString.CopyFrom(fidelius.PublicHash),
                            Iwek = ByteString.CopyFrom(fidelius.Iwek),
                            Version = 10
                        }
                    },
                    AttemptNumber = 1,
                    VendorAttestationPayloadsArray = { new SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation { AndroidPackageName = "com.snapchat.android", Type = Type, StrictEnforcementsRequired = false, Payload = "time-out", StandardErrorCode = 0 }, new SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation { AndroidPackageName = "com.snapchat.android", Type = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.VendorAttestationLibary.VendorAttestationLibaryKeyAttestation, AppAttestEnforcement = SnapchatLibProto.Snapchat.Janus.Api.VendorAttestation.Types.AppAttestEnforcement.AppAttestEnforcementAppAttestDefaultUnset, StrictEnforcementsRequired = false, Error = Config.androidOsSDK < 24 ? "Device too old" : "time-out", StandardErrorCode = 0 } }
                },
                UnknownZero = 0,
            };
            var resp = await SnapchatGrpcClient.RegisterAsyncV2(_SCJanusRegisterWithUsernamePasswordRequest);
            // Check for errors
            if (resp.ErrorData != null && !string.IsNullOrEmpty(resp.ErrorData.HumanReadableErrorMessage))
            {
                throw new Exception(resp.ErrorData.HumanReadableErrorMessage);
            }
            // Set up user authentication tokens and access tokens
            if (resp.BootstrapData.UserSession.AuthToken != null)
            {
                Config.refreshToken = resp.BootstrapData.UserSession.SnapSessionResponse.RefreshToken;
                Config.user_id = resp.BootstrapData.UserSession.UserId;
                Config.dtoken1i = resp.BootstrapData.SecurityData.DeviceToken.IdP;
                Config.dtoken1v = resp.BootstrapData.SecurityData.DeviceToken.Value;
                m_Logger.Debug($"User_id: {Config.user_id}");
                m_Logger.Debug("Registration complete");

                foreach (var snapAccessTokensArray in resp.BootstrapData.UserSession.SnapSessionResponse.SnapAccessTokensArray)
                {

                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway")
                    {
                        Config.Access_Token = snapAccessTokensArray.AccessToken;
                    }
                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts")
                    {
                        Config.BusinessAccessToken = snapAccessTokensArray.AccessToken;
                    }
                }
                Config.AccessTokenExpirey = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).AddHours(25).ToUnixTimeMilliseconds();
                var cofafterregisterresult = SCCofConfigTargetingResponse.Parser.ParseFrom(resp.BootstrapData.Cof);
                int open_to_trigger_delay_2 = new Random().Next(2530, 5600);
                int time_from_open_app_2 = new Random().Next(open_to_trigger_delay_2 + 20, open_to_trigger_delay_2 + 30);
                long trigger_timestamp_2 = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - new Random().Next(50, 200) - time_from_open_app_2) / 1000;
                var _SCCofConfigTargetingRequestAfterRegister = new SCCofConfigTargetingRequest
                {
                    ConfigResultsEtag = cofafterregisterresult.ConfigResultsEtag,
                    ScreenHeight = 1440,
                    ScreenWidth = 1440,
                    Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi, IsRoaming = false, IsMetered = false },
                    MaxVideoHeightPx = 1440,
                    MaxVideoWidthPx = 2910,
                    DeltaSync = true,
                    TriggerEventType = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingTriggerEventType.ForegroundTrigger,
                    AppState = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingAppState.Foreground,
                    DeviceId = Config.DeviceID,
                    AppLocale = "en",
                    SyncTriggerBlizzardSessionId = BlizzardSessionId,
                    SyncExecutionBlizzardSessionId = BlizzardSessionId,
                    CofSyncExecutionDelayFromStartupMs = open_to_trigger_delay_2,
                    CofSyncTriggerDelayFromStartupMs = time_from_open_app_2,
                    SyncTriggerTime = trigger_timestamp_2,
                    LenscoreVersion = 237,
                    ClientId = Config.ClientID,
                };
                var mscofbinresult = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequestAfterRegister, true);
                Config.mcs_cof_ids_bin = new List<int>();
                foreach (var ConfigResults in mscofbinresult.ConfigResultsArray)
                {
                    if (ConfigResults.SequenceId != 0)
                    {
                        if (ConfigResults.ConfigId == "GROUP_MESSAGE_RETENTION_COF" ||
                            ConfigResults.ConfigId == "EEL_RECEIVE_CONFIG" ||
                            ConfigResults.ConfigId == "ALLOW_REPLAY_AGAIN" ||
                            ConfigResults.ConfigId == "MERLIN_WELCOME_CARD_ENABLED" ||
                            ConfigResults.ConfigId == "MERLIN_WELCOME_CARD_CONFIG" ||
                            ConfigResults.ConfigId == "HOURS_AFTER_NO_INTERACTION_TO_SEND_MERLIN_WELCOME_MESSAGE" ||
                            ConfigResults.ConfigId == "MAX_HOURS_AFTER_STREAK_EXPIRE_TO_ENABLE_RESTORE" ||
                            ConfigResults.ConfigId == "STREAK_RESTORE_NO_CAPTURE_ENABLED" ||
                            ConfigResults.ConfigId == "MIN_STREAK_COUNT_TO_ENABLE_RESTORE" ||
                            ConfigResults.ConfigId == "ALLOW_DISABLE_SAVING_FOR_PENDING_CONVERSATION" ||
                            ConfigResults.ConfigId == "ENABLE_STABLE_FEED_ORDER_AFTER_SEND_MASS_SNAP" ||
                            ConfigResults.ConfigId == "MASS_FEED_REORDERING_FEED_DECREMENT_TIMESTAMPS" ||
                            ConfigResults.ConfigId == "TEAMSNAPCHAT_CONV_WALLPAPER" ||
                            ConfigResults.ConfigId == "one_on_one_24_hr_retention_config" ||
                            ConfigResults.ConfigId == "SNAP_SCORE_INCREMENT_VARIATIONS" ||
                            ConfigResults.ConfigId == "TOP_GROUPS_UPDATE_FREQ_IN_MS" ||
                            ConfigResults.ConfigId == "EXPIRE_SNAP_AFTER_VIEWED_BY_CURRENT_USER" ||
                            ConfigResults.ConfigId == "24_HOUR_SNAPS_MODE" ||
                            ConfigResults.ConfigId == "EEL_CLIENT_CONFIG" ||
                            ConfigResults.ConfigId == "NON_FRIEND_MESSAGING")
                        {
                            Config.mcs_cof_ids_bin.Add(ConfigResults.SequenceId);
                        }
                    }
                }
                await SnapchatGrpcClient.PollRecrypt(new SCFideliusPollRecryptRequest { PublicKey = ByteString.CopyFrom(fidelius.PublicUnwrapped) });
                await Task.Delay(new Random().Next(10000, 15000));
                await SnapchatClient.ChangeEmail(email);
                await Task.Delay(new Random().Next(30000, 60000));
                await SnapchatClient.ResendEmail(email);
                await Task.Delay(new Random().Next(10000, 15000));
            }
            return resp;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
    public virtual async Task<WebCreationStatus> RegisterWeb(string firstname, string lastname, string username, string password, string email, string emailpassword)
    {
        try
        {
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(emailpassword))
            {
                throw new Exception("Missing required fields");
            }

            if (username.Length > 15)
            {
                throw new Exception("Usernames cannot be longer than 15 characters.");
            }

            using (var httpClientIPAPI = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All, Proxy = Config.Proxy }))
            {
                HttpResponseMessage response;

                try
                {
                    if (Config.IPAbuseCheck || Config.IPReuseCheck)
                    {
                        response = await httpClientIPAPI.GetAsync($"http://ip-api.com/json/?fields=timezone,proxy,hosting,query");

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"Error fetching IP API data. Status code: {response.StatusCode}");
                        }

                        var result = JsonSerializer.Deserialize<IP_API_PRO>(await response.Content.ReadAsStringAsync());

                        Config.TimeZone = result.timezone;

                        if (Config.IPReuseCheck)
                        {
                            /*
                             * Implement IP reuse check here
                             */
                        }

                        if (Config.IPAbuseCheck)
                        {
                            /*
                            using (var ipAbuseCheck = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }))
                            {
                                var response2 = await ipAbuseCheck.GetAsync($"https://www.ipqualityscore.com/api/json/ip/key_here/{result.query}?strictness=0&allow_public_access_points=true&fast=true&lighter_penalties=true&mobile=true");

                                if (!response2.IsSuccessStatusCode)
                                {
                                    throw new HttpRequestException($"Error checking IP abuse. Status code: {response2.StatusCode}");
                                }
                                var result2 = JsonSerializer.Deserialize<IPQUALITYSCORE>(await response2.Content.ReadAsStringAsync());

                                if (result.proxy || result.hosting || result2.active_vpn || result2.recent_abuse || result2.bot_status)
                                {
                                    throw new Exception("FLAGGED IP");
                                }
                            }
                            */
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    if (Config.Debug)
                    {
                        // Handle HTTP request exceptions here
                        Console.WriteLine($"HTTP Request failed: {httpEx.Message}");
                    }

                    return WebCreationStatus.Failed;
                }
                catch (Exception ex)
                {
                    if (Config.Debug)
                    {
                        // Handle other exceptions here
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }

                    return WebCreationStatus.Failed;
                }
            }

            // Get device information
            m_Logger.Debug("Getting Device Info");
            // Set up device information
            // Set up user information
            Config.Username = username;
            Random rnd = new Random();
            DateTime datetoday = DateTime.Now;
            int rndYear = rnd.Next(1965, 2004);
            int rndMonth = rnd.Next(1, 12);
            int rndDay = rnd.Next(1, 28);
            Config.Age = m_Utilities.GetAge(new DateTime(rndYear, rndMonth, rndDay));
            Config.Horoscope = zodiac_sign(rndDay, rndMonth);
            // Check if the proxy has credentials

            ChromeOptions chromeOptions = new ChromeOptions();
            if (Config.Proxy.Credentials != null)
            {
                var proxyExtension = new ProxyExtension(Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "").Split(":")[0], Convert.ToInt32(Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "").Split(":")[1]), Config.Proxy.Credentials.GetCredential(Config.Proxy.Address, "").UserName, Config.Proxy.Credentials.GetCredential(Config.Proxy.Address, "").Password);
                chromeOptions.AddArgument($"--load-extension={proxyExtension.Directory}");
            }

            chromeOptions.AddArgument("--mute-audio");
            chromeOptions.AddArguments("--window-size=1,1");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--disable-software-rasterizer");
            chromeOptions.AddArguments($"--proxy-server=http://{Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "")}");
            using (var driver = UndetectedChromeDriver.Create(chromeOptions, null,
                driverExecutablePath: await new ChromeDriverInstaller().Auto(), null, 0, 0, true))
            {
                try
                {
                    driver.GoToUrl("https://accounts.snapchat.com/accounts/signup?client_id=ads-api&referrer=https%253A%252F%252Fbusiness.snapchat.com%252F");
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(Config.Timeout));

                    Action<string, string> sendKeysToElement = (xpath, text) =>
                    {
                        var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
                        element.Click();
                        element.SendKeys(text);
                    };

                    sendKeysToElement("//input[@name='first_name']", firstname);
                    sendKeysToElement("//input[@name='last_name']", lastname);
                    sendKeysToElement("//input[@name='username']", username);
                    sendKeysToElement("//input[@name='password']", password);
                    sendKeysToElement("//input[@name='email']", email);

                    try
                    {
                        sendKeysToElement("//input[@name='birthday']", $"{rndMonth}{rndDay}{rndYear}");
                    }
                    catch (Exception)
                    {
                        new SelectElement(driver.FindElement(By.ClassName("custom-dropdown"))).SelectByValue(rndMonth.ToString());
                        sendKeysToElement("//input[@name='birth_day']", rndDay.ToString());
                        sendKeysToElement("//input[@name='birth_year']", rndYear.ToString());
                    }

                    var submit = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[normalize-space()='Sign Up & Accept']")));
                    submit.Click();
                    var successful_register = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//button[normalize-space()='Next']")));
                    return successful_register != null ? WebCreationStatus.Created : WebCreationStatus.Failed;
                }
                catch (Exception ex)
                {
                    if (Config.Debug)
                    {
                        throw new Exception(ex.ToString());
                    }
                    return WebCreationStatus.Failed;
                }
                finally
                {
                    driver.Quit();
                }
            }

        }
        catch (Exception ex)
        {
            if (Config.Debug)
            {
                throw new Exception(ex.ToString());
            }
            return WebCreationStatus.Failed;
        }
    }
}