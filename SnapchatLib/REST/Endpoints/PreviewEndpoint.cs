using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Extras;
using SnapProto.Ranking.Serving.Jaguar;

namespace SnapchatLib.REST.Endpoints;

public interface IPreviewEndpoint
{
    Task<SCSSMStoriesBatchResponse> GetStories(string username);
}

internal class PreviewEndpoint : EndpointAccessor, IPreviewEndpoint
{
    internal static readonly EndpointInfo EndpointInfo = new()
    {
        Url = "/df-mixer-prod/soma/batch_stories",
        BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint,
        Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapAccessToken | EndpointRequirements.XRequestUUID
    };

    public PreviewEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<SCSSMStoriesBatchResponse> GetStories(string username)
    {
        m_Logger.Debug("Starting GetAccessToken configuration");
        var sCSSMStoryLookupRequestItems = new List<SCSSMStoryLookupRequestItem>
        {
            new SCSSMStoryLookupRequestItem
            {
                CompositeStoryId = new SnapProto.Ranking.Core.SCCORECompositeStoryId
                {
                    Corpus = SnapProto.Ranking.Core.SCCORECompositeStoryId.Types.SCFEEDStoryCorpus_Corpus.FriendStory,
                    IdP = await SnapchatClient.FindUserFromCache(username)
                },
                DeltaFetchInfo = new SCSSMStoryLookupRequestItem.Types.SCSSMStoryLookupRequestItem_DeltaFetchInfo { SequenceBegin = 2 }

            }
        };
        var _SCSSMStoriesRequest = new SCSSMStoriesRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            RequestTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ClientInfo = new SCSSMClientInfo
            {
                AppInfo = new SCSSMAppInfo
                {
                    AppVersion = Config.SnapchatVersion.ToString().Replace("V", "").Replace("_", "."),
                    OsType = SCSSMAppInfo.Types.SCSCOREOsType_Type.OsAndroid
                },
                IsNewUser = true,
                CameosFeatureRestricted = SCSSMClientInfo.Types.SCSSMCameosFeatureStatus_Enum.Restricted
            },
            Origin = SCSSMStoriesRequest.Types.SCSSMStoriesRequest_Origin.OriginSnapchatApp,
            DeltaFetchInfo = new SCSSMStoriesRequest.Types.SCSSMStoriesRequest_DeltaFetchInfo
            {
                DeltaTokenArray = { sCSSMStoryLookupRequestItems },
            },
            FeedTypesArray = { new List<int> { 5 } },
            RequestSource = SCSSMStoriesRequest.Types.SCSSMStoriesRequest_RequestSource_Enum.DiscoverPage
        };
        // send data via HTTP
        using var _ByteArrayContent = new ByteArrayContent(_SCSSMStoriesRequest.ToByteArray());
        _ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
        var response = await Send(EndpointInfo, _ByteArrayContent);
        var result = SCSSMStoriesBatchResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());

        return result;
    }
}