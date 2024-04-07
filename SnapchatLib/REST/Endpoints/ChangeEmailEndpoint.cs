using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using static SnapchatLib.Extras.Utilities;

namespace SnapchatLib.REST.Endpoints
{
    public interface IChangeEmailEndpoint
    {
        Task<string> ChangeEmail(string email);
        Task<string> ResendEmail(string email);
    }

    internal class ChangeEmailEndpoint : EndpointAccessor, IChangeEmailEndpoint
    {
        public ChangeEmailEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        public async Task<string> ChangeEmail(string email)
        {
            var response = await SnapchatGrpcClient.UpdateEmail(new SnapchatLibProto.Snapchat.Activation.Api.UpdateEmailRequest { AndroidIdAsLong = AndroidIDConverter.AndroidIDToLong(Config.androidId).ToString(), BlizzardClientId = Config.ClientID, Action = SnapchatLibProto.Snapchat.Activation.Api.UpdateEmailRequest.Types.SCAccountEmailServicePbUpdateEmailRequest_Action.ChangeEmail, RequestedEmail = email, Uuid = Guid.NewGuid().ToString() });
            return response.StringMessage.Message;
        }

        public async Task<string> ResendEmail(string email)
        {
            var response = await SnapchatGrpcClient.UpdateEmail(new SnapchatLibProto.Snapchat.Activation.Api.UpdateEmailRequest { AndroidIdAsLong = AndroidIDConverter.AndroidIDToLong(Config.androidId).ToString(), BlizzardClientId = Config.ClientID, Action = SnapchatLibProto.Snapchat.Activation.Api.UpdateEmailRequest.Types.SCAccountEmailServicePbUpdateEmailRequest_Action.ResendConfirmationEmail, RequestedEmail = email, Uuid = Guid.NewGuid().ToString() });
            return response.StringMessage.Message;
        }
    }
}