using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Extras;
using SnapProto.Ranking.Storymanagement;

namespace SnapchatLib.REST.Endpoints;

public interface ISettingsEndpoint
{
    Task MakeStoryEveryone();
    Task MakeStoryFriendsOnly();
}

internal class SettingsEndpoint : EndpointAccessor, ISettingsEndpoint
{
    internal static readonly EndpointInfo EndpointInfo2 = new() { Url = "/story-management-service/update_story_privacy", Requirements = EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID | EndpointRequirements.XSnapAccessToken };
    public SettingsEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    public async Task MakeStoryEveryone()
    {
        var _UpdateStoryPrivacyRequest = new UpdateStoryPrivacyRequest
        {
            RequestMetadata =
            {
                RequestId = Guid.NewGuid().ToString(),
                RequestTimestampMs = m_Utilities.UtcTimestamp(),
                ClientInfo = {
                    UserId = new SnapProto.Ranking.Core.SCSCOREUUID
                    {
                        HighBits = GuidUtils.GetHighBits(Config.user_id), LowBits = GuidUtils.GetLowBits(Config.user_id)
                    },
                    AppInfo =
                    {
                        OsType = SnapProto.Ranking.Core.SCSCOREAppInfo.Types.SCSCOREOsType_Type.OsAndroid
                    },
                    CountryCode = Config.AccountCountryCode,
                }
            },
            StoryPrivacy = UpdateStoryPrivacyRequest.Types.UpdateStoryPrivacyRequest_StoryPrivacy.Everyone
        };
        // send data via HTTP
        using var ByteArrayContent = new ByteArrayContent(_UpdateStoryPrivacyRequest.ToByteArray());
        ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
        var response = await Send(EndpointInfo2, ByteArrayContent);
    }
    public async Task MakeStoryFriendsOnly()
    {
        var _UpdateStoryPrivacyRequest = new UpdateStoryPrivacyRequest
        {
            RequestMetadata =
            {
                RequestId = Guid.NewGuid().ToString(),
                RequestTimestampMs = m_Utilities.UtcTimestamp(),
                ClientInfo = {
                    UserId = new SnapProto.Ranking.Core.SCSCOREUUID
                    {
                        HighBits = GuidUtils.GetHighBits(Config.user_id), LowBits = GuidUtils.GetLowBits(Config.user_id)
                    },
                    AppInfo =
                    {
                        OsType = SnapProto.Ranking.Core.SCSCOREAppInfo.Types.SCSCOREOsType_Type.OsAndroid
                    },
                    CountryCode = Config.AccountCountryCode,
                }
            },
            StoryPrivacy = UpdateStoryPrivacyRequest.Types.UpdateStoryPrivacyRequest_StoryPrivacy.Friends
        };
        // send data via HTTP
        using var ByteArrayContent = new ByteArrayContent(_UpdateStoryPrivacyRequest.ToByteArray());
        ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
        var response = await Send(EndpointInfo2, ByteArrayContent);
    }
}