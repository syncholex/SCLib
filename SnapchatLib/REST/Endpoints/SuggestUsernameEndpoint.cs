using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLibProto.Snapchat.Activation.Api;
using static SnapchatLib.Extras.Utilities;

namespace SnapchatLib.REST.Endpoints;

public interface ISuggestUsernameEndpoint
{
    Task<SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsername(string first_name, string last_name);
}

internal class SuggestUsernameEndpoint : EndpointAccessor, ISuggestUsernameEndpoint
{
    public SuggestUsernameEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    public async Task<SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsername(string first_name, string last_name)
    {
        SnapchatGrpcClient.SetupServiceClients();
        if (string.IsNullOrEmpty(Config.androidId) || Config.install_time == 0)
        {
            if (Config.SnapchatVersion == SnapchatVersion.V12_76_0_38)
            {
                Config.install_time = m_Utilities.GetInstallTimeStamp(1709608343000);
            }
            Config.androidId = AndroidIDGenerator.GenerateAndroidID();
            Config.ClientID = Guid.NewGuid().ToString();
            Config.DeviceID = Guid.NewGuid().ToString();
        }

        var _SCFriendingFriendsRemoveRequest = new SCSuggestUsernamePbSuggestUsernameRequestV2
        {
            NameAndBirthdate = new SCSuggestUsernamePbSuggestUsernameRequestV2.Types.NameAndBirthdate { FirstName = first_name, LastName = last_name},
            VersionNumber = 0,
            ClientId = Config.ClientID,
            DeviceId = Config.DeviceID,
        };

        var reply = await SnapchatGrpcClient.SuggestUsernameAsync(_SCFriendingFriendsRemoveRequest);
        return reply;
    }
}