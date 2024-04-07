using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;

namespace SnapchatLib.REST.Endpoints;

public interface ISuggestFriendEndpoint
{
    Task<suggest_friend> GetSuggestionsHighAvailability();
    Task<suggest_friend_on_demand> GetSuggestionsOnDemand();
    Task<suggest_friend> GetSuggestionsHighQuality();
}

internal class SuggestFriendEndpoint : EndpointAccessor, ISuggestFriendEndpoint
{
    internal static readonly EndpointInfo SuggestionsHighAvailabilityEndpoint = new ()
    {
        Url = "/suggest_friend_high_availability", 
        BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint, 
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUserId | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID
    };

    internal static readonly EndpointInfo SuggestionsOnDemandEndpointInfo = new()
    {
        Url = "/suggest_friend_on_demand",
        BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUserId | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID
    };

    internal static readonly EndpointInfo SuggestionsHighQualityEndpointInfo = new()
    {
        Url = "/suggest_friend_high_quality",
        BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUserId | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID
    };

    public SuggestFriendEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    public async Task<suggest_friend> GetSuggestionsHighAvailability()
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "list"},
            {"impression_id", "0"},
            {"impression_time_ms", "0"},
            {"last_sync_timestamp", "0"},
            {"suggested_friend_ranking_tweak", "0"},
        };
        var response = await Send(SuggestionsHighAvailabilityEndpoint, parameters);
        return m_Utilities.JsonDeserializeObject<suggest_friend>(await response.Content.ReadAsStringAsync());
    }

    public async Task<suggest_friend_on_demand> GetSuggestionsOnDemand()
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "list"},
            {"impression_id", "0"},
            {"impression_time_ms", "0"},
            {"last_sync_timestamp", "0"},
            {"suggested_friend_ranking_tweak", "0"},
        };
        var response = await Send(SuggestionsOnDemandEndpointInfo, parameters);
        return m_Utilities.JsonDeserializeObject<suggest_friend_on_demand>(await response.Content.ReadAsStringAsync());
    }

    public async Task<suggest_friend> GetSuggestionsHighQuality()
    {
        var parameters = new Dictionary<string, string>
        {
            {"action", "list"},
            {"impression_id", "0"},
            {"impression_time_ms", "0"},
            {"last_sync_timestamp", "0"},
            {"suggested_friend_ranking_tweak", "0"},
        };
        var response = await Send(SuggestionsHighQualityEndpointInfo, parameters);
        return m_Utilities.JsonDeserializeObject<suggest_friend>(await response.Content.ReadAsStringAsync());
    }
}