using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;

namespace SnapchatLib.REST.Endpoints;

public interface IPhoneVerifyEndpoint
{
    Task<string> ChangePhoneRegister(string number, string countrycode);
    Task<string> VerifyPhoneRegister(string code);
    Task<string> ChangePhoneSettings(string number, string countrycode, string network_code);
    Task<string> VerifyPhoneSettings(string code);
}

internal class PhoneVerifyEndpoint : EndpointAccessor, IPhoneVerifyEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new () {Url = "/bq/phone_verify", Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XRequestUUID | EndpointRequirements.XSnapRouteTag };

    public PhoneVerifyEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<string> ChangePhoneRegister(string number, string countrycode)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "updatePhoneNumber"},
            {"bypass_user_login_check", "true"},
            {"client_id", Config.ClientID},
            {"config_device_id", Config.DeviceID},
            {"countryCode", countrycode},
            {"method", "text"},
            {"phoneNumber", number},
            {"reset_password_in_app", "false"},
            {"skipConfirmation", "true"},
            {"snapchat_user_id", Config.user_id},
            {"type", "REGISTRATION_TYPE"}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> ChangePhoneSettings(string number, string countrycode, string network_code)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "updatePhoneNumber"},
            {"bypass_user_login_check", "true"},
            {"client_id", Config.ClientID},
            {"client_network_request_id", ""},
            {"config_device_id", Config.DeviceID},
            {"countryCode", countrycode},
            {"method", "text"},
            {"network_code", network_code},
            {"phoneNumber", number},
            {"reset_password_in_app", "false"},
            {"skipConfirmation", "true"},
            {"snapchat_user_id", Config.user_id},
            {"type", "SETTINGS_PHONE_TYPE"}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> VerifyPhoneSettings(string code)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "verifyPhoneNumber"},
            {"client_id", Config.ClientID},
            {"client_network_request_id", ""},
            {"code", code},
            {"config_device_id", Config.DeviceID},
            {"reset_password_in_app", "false"},
            {"snapchat_user_id", Config.user_id},
            {"type", "SETTINGS_PHONE_TYPE"}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> VerifyPhoneRegister(string code)
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "verifyPhoneNumber"},
            {"client_id", Config.ClientID},
            {"config_device_id", Config.DeviceID},
            {"code", code},
            {"reset_password_in_app", "false"},
            {"snapchat_user_id", Config.user_id},
            {"type", "REGISTRATION_TYPE"}
        };
        var response = await Send(EndpointInfo, parameters);
        return await response.Content.ReadAsStringAsync();
    }
}