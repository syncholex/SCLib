using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;

namespace SnapchatLib.Exceptions
{
    public class ProfileNotFoundException : Exception
    {
        public ProfileNotFoundException(string username) : base($"Failed to get user id for username: {username}")
        {
        }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    public interface ISnapchatterPublicInfoEndpoint
    {
        Task<string> GetProfileInfo(string username);
    }

    internal class SnapchatterPublicInfoEndpoint : EndpointAccessor, ISnapchatterPublicInfoEndpoint
    {
        internal static readonly EndpointInfo EndpointInfo = new()
        {
            Url = "/loq/snapchatter_public_info",
            BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint,
            Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XRequestUUID | EndpointRequirements.XSnapchatUUID
        };

        public SnapchatterPublicInfoEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        public async Task<string> GetProfileInfo(string username)
        {
            var userid = await SnapchatClient.GetUserId(username);

            if (string.IsNullOrWhiteSpace(userid)) throw new ProfileNotFoundException(username);
            
            return await GetProfile(userid);
        }
        private async Task<string> GetProfile(string userId)
        {
            var parameters = new Dictionary<string, string>
            {
                {"source", "PROFILE"},
                {"snapchat_user_id", Config.user_id},
                {"user_ids", "[\"" + userId + "\"]"}
            };
            var response = await Send(EndpointInfo, parameters);
            return await response.Content.ReadAsStringAsync();
        }
    }
}