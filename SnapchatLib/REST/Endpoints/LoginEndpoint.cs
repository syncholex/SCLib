using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using Janus.Crypto.Fidelius;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Cdp.Cof;
using SnapchatLibProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Search;
using static SnapchatLib.Extras.Utilities;
using SnapchatLib.REST.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Text;

namespace SnapchatLib.REST.Endpoints;

public interface ILoginEndpoint
{
    Task<SCJanusLoginWithPasswordResponse> Login(string username, string password);
    Task<SnapProto.Snapchat.Janus.Api.SCJanusVerifyODLVResponse> Login2FA(string twofactorcode);
}

internal class LoginEndpoint : EndpointAccessor, ILoginEndpoint
{
    public LoginEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

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
    public async Task<SCJanusLoginWithPasswordResponse> Login(string username, string password)
    {
        try
        {

            m_Logger.Debug("Setting up Login information");

            if (string.IsNullOrEmpty(Config.androidId) || Config.install_time == 0)
            {
                long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long oneHourAgoTimestamp = currentTimestamp - (60 * 60 * 1000);
                if (Config.SnapchatVersion == SnapchatVersion.V12_76_0_38)
                {
                    Config.install_time = m_Utilities.GetInstallTimeStamp(1709608343000);
                }
                Config.androidId = AndroidIDGenerator.GenerateAndroidID();
                Random rnd = new Random();
                DateTime datetoday = DateTime.Now;
                int rndYear = rnd.Next(1965, 2004);
                int rndMonth = rnd.Next(1, 12);
                int rndDay = rnd.Next(1, 28);
                Config.Age = m_Utilities.GetAge(new DateTime(rndYear, rndMonth, rndDay));

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
                Config.Horoscope = zodiac_sign(rndDay, rndMonth);
                Config.ClientID = Guid.NewGuid().ToString();
                Config.DeviceID = Guid.NewGuid().ToString();
            }

            SnapchatGrpcClient.SetupServiceClients();
            m_Logger.Debug("Device and Install OK");
            Config.Username = username;
            var fidelius = FideliusDevice.Create(5);
            var BlizzardSessionId = "f2." + m_Utilities.RandomString(16);
            Random random = new Random();
            int open_to_trigger_delay = random.Next(117, 125);
            int time_from_open_app = random.Next(open_to_trigger_delay + 50, open_to_trigger_delay + 80);
            long trigger_timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - random.Next(50, 200) - time_from_open_app) / 1000;
            var _SCCofConfigTargetingRequest = new SCCofConfigTargetingRequest
            {
                ScreenHeight = 1440,
                ScreenWidth = 1440,
                Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi },
                MaxVideoHeightPx = 1440,
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
                CofSyncExecutionDelayFromStartupMs = time_from_open_app,
                CofSyncTriggerDelayFromStartupMs = time_from_open_app,
                SyncTriggerTime = trigger_timestamp,
                LenscoreVersion = 237,
                ClientId = Config.ClientID,
            };
            var cof = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequest, false);

            if (string.IsNullOrEmpty(Config.AccountCountryCode))
                Config.AccountCountryCode = cof.Iso3166Alpha2CountryCodeFromRequestIp;

            SnapchatClient.ConfigResultsEtag = cof.ConfigResultsEtag;
            foreach (var Config in cof.ConfigResultsArray)
            {
                switch (Config.SequenceId)
                {
                    case 1381308:
                    case 3570677:
                    case 2163747:
                    case 2635291:
                    case 1358289:
                    case 1819395:
                    case 1266018:
                    case 2476333:
                    case 3135777:
                    case 2283842:
                    case 3552034:
                    case 3322071:
                    case 2653841:
                    case 2283841:
                    case 2928172:
                    case 3008682:
                    case 3164736:
                    case 2122110:
                    case 2928171:
                    case 3552033:
                    case 3534615:
                    case 3432618:
                    case 2286504:
                    case 1750546:
                    case 1946177:
                    case 2153539:
                    case 2153538:
                    case 1752033:
                    case 2653840:
                    case 3322106:
                    case 3345039:
                    case 847914:
                    case 1946178:
                    case 1892595:
                    case 2286503:
                    case 2788280:
                    case 2285945:
                    case 1177612:
                    case 1504319:
                    case 2981485:
                    case 846877:
                    case 1358426:
                    case 3809326:
                    case 2701280:
                    case 1743950:
                    case 2546715:
                    case 1175894:
                    case 2701281:
                    case 2177508:
                    case 3080832:
                    case 3378292:
                    case 3517225:
                    case 2044865:
                    case 1495264:
                    case 1931245:
                    case 1931244:
                    case 1731133:
                    case 2177510:
                    case 844966:
                    case 1495266:
                    case 2625188:
                    case 2493876:
                    case 1715311:
                    case 3671924:
                    case 2692031:
                    case 2663714:
                    case 2177509:
                    case 3040410:
                    case 1663776:
                    case 1813313:
                    case 1495260:
                    case 2472930:
                    case 1931199:
                    case 1971888:
                    case 1715316:
                        SnapchatClient.cofConfigData_Android.Add(Config.SequenceId);
                        break;
                }
            }

            var signlogin = await SnapchatGrpcClient.Sign("/snapchat.janus.api.LoginService/LoginWithPassword");

            SnapchatClient.loginFlowSessionId = Guid.NewGuid().ToString();
            SnapchatClient.authenticationSessionId = Guid.NewGuid().ToString();
            SCJanusLoginWithPasswordRequest _SCJanusLoginWithPasswordRequest = new SCJanusLoginWithPasswordRequest
            {
                Username = username,
                Password = password,
                LoginAttemptNumber = 1,
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
                LoginHeader = new SCJanusLoginHeader { BlizzardClientId = Config.ClientID, LoginFlowSessionId = SnapchatClient.loginFlowSessionId, AuthenticationSessionId = SnapchatClient.authenticationSessionId, NetworkRequestId = Guid.NewGuid().ToString(), ClientAttestationPayload = ByteString.CopyFrom(Convert.FromBase64String(signlogin.Attestation.Replace('-', '+').Replace('_', '/'))), CofDeviceId = Config.DeviceID, CofConfigData = new SnapchatLibProto.Snapchat.Janus.Api.PartialToken { SequenceIdsArray = { SnapchatClient.cofConfigData_Android } } },
                CofTags = new SCJanusCofTags { ETag = SnapchatClient.ConfigResultsEtag, RouteTag = "", LenscoreVersion = 237 },
                UnknownNumber = 1
            };

            var resp = await SnapchatGrpcClient.LoginAsync(_SCJanusLoginWithPasswordRequest);

            if (resp.StatusCode == SCJanusLoginWithPasswordResponse.Types.SCJanusLoginWithPasswordResponse_StatusCode.OdlvRequired)
            {
                var signsend2fa = await SnapchatGrpcClient.Sign("/snapchat.janus.api.LoginService/SendODLVCode");
                var _SCJanusSendODLVCodeRequest = new SnapProto.Snapchat.Janus.Api.SCJanusSendODLVCodeRequest
                {
                    LoginHeader = new SnapProto.Snapchat.Janus.Api.SCJanusLoginHeader { BlizzardClientId = Config.ClientID, LoginFlowSessionId = SnapchatClient.loginFlowSessionId, AuthenticationSessionId = SnapchatClient.authenticationSessionId, NetworkRequestId = Guid.NewGuid().ToString(), ClientAttestationPayload = ByteString.CopyFrom(Convert.FromBase64String(signsend2fa.Attestation.Replace('-', '+').Replace('_', '/'))), CofDeviceId = Config.DeviceID, CofConfigData = new SnapProto.Snapchat.Cdp.Cof.PartialToken { SequenceIdsArray = { SnapchatClient.cofConfigData_Android } } },
                    OdlvToken = resp.OdlvData.OdlvToken,
                    OdlvType = SnapProto.Snapchat.Janus.Api.SCJanusSendODLVCodeRequest.Types.SCJanusODLVType.OdlvTypeEmail
                };
                await SnapchatGrpcClient.SendEmail2FA(_SCJanusSendODLVCodeRequest);
                SnapchatClient.odlvToken = resp.OdlvData.OdlvToken;

                return resp;
            }

            if (resp.StatusCode == SCJanusLoginWithPasswordResponse.Types.SCJanusLoginWithPasswordResponse_StatusCode.AccountLocked)
                throw new DeadAccountException("Account Locked");

            if (resp.StatusCode == SCJanusLoginWithPasswordResponse.Types.SCJanusLoginWithPasswordResponse_StatusCode.AccountDeactivated)
                throw new DeadAccountException("Account Deactivated");


            if (resp.ErrorData != null && !string.IsNullOrEmpty(resp.ErrorData.HumanReadableErrorMessage))
                throw new Exception(resp.ErrorData.HumanReadableErrorMessage);

            if (resp.BootstrapData.UserSession.AuthToken != null)
            {
                Config.dtoken1i = resp.BootstrapData.SecurityData.DeviceToken.IdP;
                Config.dtoken1v = resp.BootstrapData.SecurityData.DeviceToken.Value;
                Config.refreshToken = resp.BootstrapData.UserSession.SnapSessionResponse.RefreshToken;
                Config.user_id = resp.BootstrapData.UserSession.UserId;
                m_Logger.Debug($"User_id: {Config.user_id}");
                m_Logger.Debug("Login complete");

                var cof2result = SCCofConfigTargetingResponse.Parser.ParseFrom(resp.BootstrapData.Cof);
                Config.mcs_cof_ids_bin = new List<int>();
                foreach (var cof2 in cof2result.ConfigResultsArray)
                {
                    if (cof2.SequenceId != 0)
                    {
                        if (cof2.ConfigId == "GROUP_MESSAGE_RETENTION_COF" || cof2.ConfigId == "one_on_one_24_hr_retention_config" || cof2.ConfigId == "NON_FRIEND_MESSAGING" || cof2.ConfigId == "MAX_HOURS_AFTER_STREAK_EXPIRE_TO_ENABLE_RESTORE" || cof2.ConfigId == "MIN_STREAK_COUNT_TO_ENABLE_RESTORE" || cof2.ConfigId == "MERLIN_WELCOME_CARD_ENABLED" || cof2.ConfigId == "MERLIN_WELCOME_CARD_CONFIG" || cof2.ConfigId == "HOURS_AFTER_NO_INTERACTION_TO_SEND_MERLIN_WELCOME_MESSAGE")
                        {
                            Config.mcs_cof_ids_bin.Add(cof2.SequenceId);
                        }
                    }
                }

                foreach (var snapAccessTokensArray in resp.BootstrapData.UserSession.SnapSessionResponse.SnapAccessTokensArray)
                {
                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway")
                        Config.Access_Token = snapAccessTokensArray.AccessToken;

                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts")
                        Config.BusinessAccessToken = snapAccessTokensArray.AccessToken;
                }
                Config.AccessTokenExpirey = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).AddHours(25).ToUnixTimeMilliseconds();
            }
            else
            {
                return resp;
            }
            return resp;
        }
        catch (Exception ex)
        {
            if (Config.Debug)
                throw new Exception(ex.ToString());

            throw new Exception("Failed to login");
        }
    }
    public async Task<SnapProto.Snapchat.Janus.Api.SCJanusVerifyODLVResponse> Login2FA(string twofactorcode)
    {
        try
        {

            m_Logger.Debug("Setting up Login information");

            if (Config.install_time == 0 || string.IsNullOrEmpty(Config.androidId))
                throw new Exception("Must Use The Stuff You Registered With (Device,install_time,Install,DeviceProfile) You Registered With (Sus Login will occur if you don't and 2FA will be prompted)");

            var fidelius = FideliusDevice.Create(5);
            var signlogin = await SnapchatGrpcClient.Sign("/snapchat.janus.api.LoginService/VerifyODLV");
            SnapProto.Snapchat.Janus.Api.SCJanusVerifyODLVRequest _SCJanusVerifyODLVRequest = new SnapProto.Snapchat.Janus.Api.SCJanusVerifyODLVRequest
            {
                OdlvCode = twofactorcode,
                OdlvToken = SnapchatClient.odlvToken,
                OdlvType = SnapProto.Snapchat.Janus.Api.SCJanusVerifyODLVRequest.Types.SCJanusODLVType.OdlvTypeEmail,
                FideliusClientInit = new SnapProto.Snapchat.Janus.Api.SCJanusFideliusClientInit
                {
                    HashedPublicKeysArray =
                    {
                        ByteString.CopyFrom(fidelius.HashedPublicKeysArray[0]),
                        ByteString.CopyFrom(fidelius.HashedPublicKeysArray[1]),
                        ByteString.CopyFrom(fidelius.HashedPublicKeysArray[2]),
                        ByteString.CopyFrom(fidelius.HashedPublicKeysArray[3]),
                        ByteString.CopyFrom(fidelius.HashedPublicKeysArray[4])
                    },
                    TentativeDeviceKey = new SnapProto.Snapchat.Janus.Api.SCJanusFideliusTentativeDeviceKey
                    {
                        PublicKey = ByteString.CopyFrom(fidelius.PublicUnwrapped),
                        HashedPublicKey = ByteString.CopyFrom(fidelius.PublicHash),
                        Iwek = ByteString.CopyFrom(fidelius.Iwek),
                        Version = 9
                    }
                },
                LoginHeader = new SnapProto.Snapchat.Janus.Api.SCJanusLoginHeader { BlizzardClientId = Config.ClientID, LoginFlowSessionId = SnapchatClient.loginFlowSessionId, AuthenticationSessionId = SnapchatClient.authenticationSessionId, NetworkRequestId = Guid.NewGuid().ToString(), ClientAttestationPayload = ByteString.CopyFrom(Convert.FromBase64String(signlogin.Attestation.Replace('-', '+').Replace('_', '/'))), CofDeviceId = Config.DeviceID, CofConfigData = new SnapProto.Snapchat.Cdp.Cof.PartialToken { SequenceIdsArray = { SnapchatClient.cofConfigData_Android } } },
                CofTags = new SnapProto.Snapchat.Janus.Api.SCJanusCofTags { ETag = SnapchatClient.ConfigResultsEtag },
            };
          
            var resp = await SnapchatGrpcClient.LoginEmail2FA(_SCJanusVerifyODLVRequest);
            if (resp.ErrorData != null && !string.IsNullOrEmpty(resp.ErrorData.HumanReadableErrorMessage))
                throw new Exception(resp.ErrorData.HumanReadableErrorMessage);

            if (resp.BootstrapData.UserSession.AuthToken != null)
            {
                Config.dtoken1i = resp.BootstrapData.SecurityData.DeviceToken.IdP;
                Config.dtoken1v = resp.BootstrapData.SecurityData.DeviceToken.Value;
                Config.refreshToken = resp.BootstrapData.UserSession.SnapSessionResponse.RefreshToken;
                Config.user_id = resp.BootstrapData.UserSession.UserId;
                m_Logger.Debug($"User_id: {Config.user_id}");
                m_Logger.Debug("Login complete");

                var cof2result = SCCofConfigTargetingResponse.Parser.ParseFrom(resp.BootstrapData.Cof);
                Config.mcs_cof_ids_bin = new List<int>();
                foreach (var cof2 in cof2result.ConfigResultsArray)
                {
                    if (cof2.SequenceId != 0)
                    {
                        if (cof2.ConfigId == "GROUP_MESSAGE_RETENTION_COF" || cof2.ConfigId == "one_on_one_24_hr_retention_config" || cof2.ConfigId == "NON_FRIEND_MESSAGING" || cof2.ConfigId == "MAX_HOURS_AFTER_STREAK_EXPIRE_TO_ENABLE_RESTORE" || cof2.ConfigId == "MIN_STREAK_COUNT_TO_ENABLE_RESTORE" || cof2.ConfigId == "MERLIN_WELCOME_CARD_ENABLED" || cof2.ConfigId == "MERLIN_WELCOME_CARD_CONFIG" || cof2.ConfigId == "HOURS_AFTER_NO_INTERACTION_TO_SEND_MERLIN_WELCOME_MESSAGE")
                        {
                            Config.mcs_cof_ids_bin.Add(cof2.SequenceId);
                        }
                    }
                }

                foreach (var snapAccessTokensArray in resp.BootstrapData.UserSession.SnapSessionResponse.SnapAccessTokensArray)
                {
                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway")
                        Config.Access_Token = snapAccessTokensArray.AccessToken;

                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts")
                        Config.BusinessAccessToken = snapAccessTokensArray.AccessToken;
                }
                Config.AccessTokenExpirey = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).AddHours(25).ToUnixTimeMilliseconds();
            }
            else
            {
                return resp;
            }
            return resp;
        }
        catch (Exception ex)
        {
            if (Config.Debug)
                throw new Exception(ex.ToString());

            throw new Exception("Failed to login");
        }
    }
}