using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Core;
using SnapProto.Snapchat.Friending;
using static SnapchatLib.REST.Models.SyncResponse;

namespace SnapchatLib.REST.Endpoints;

public interface IFriendEndpoint
{
    Task<ami_friends> SyncFriends();
    Task<ami_friends> SyncFriends(string added_friends_sync_token, string friends_sync_token);
    Task<SCFriendingFriendsActionResponse> AcceptFriend(string user_id);
    Task<SCFriendingFriendsActionResponse> AddBySearchByUsername(string username);
    Task<SCFriendingFriendsActionResponse> AddByProfileByUsername(string username);
    Task<SCFriendingFriendsActionResponse> AddBySearchByDisplayName(string username);
    Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string user_id, string suggestionToken);
    Task<SCFriendingFriendsActionResponse> BlockFriend(string username_or_user_id);
    Task<SCFriendingFriendsActionResponse> ChangeFriendDisplayName(string user_id, string newname);
    Task<SCFriendingFriendsActionResponse> RemoveFriend(string user_id);
    Task<SCFriendingFriendsActionResponse> SubscribeBySearch(string username);
    Task<SCFriendingFriendsActionResponse> Subscribe(string username);
    Task<SCFriendingFriendsActionResponse> UnBlockFriend(string username_or_user_id);
    Task<SCFriendingFriendsActionResponse> AddByGroupChat(string username);
    Task<SCFriendingFriendsActionResponse> AddByContact(string username);
    Task<SCFriendingFriendsActionResponse> AddByMention(string username);
    Task<SCFriendingFriendsActionResponse> AddByRegister(HashSet<string> user_id, string suggestionToken);
    Task<bool> HasBitmoji(string username);
}
internal class SyncClass
{
    public string friends_sync_token { get; set; }
    public string added_friends_sync_token { get; set; }
    public bool is_request_from_background { get; set; }
}

internal class FriendEndpoint : EndpointAccessor, IFriendEndpoint
{
    internal static readonly EndpointInfo FriendSyncEndpointInfo = new() { Url = "/ami/friends", Requirements = EndpointRequirements.XSnapchatUUID | EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID, BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint };
    public FriendEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {

    }

    public async Task<SCFriendingFriendsActionResponse> ChangeFriendDisplayName(string username_or_user_id, string newname)
    {

        var friendUserId = username_or_user_id;

        if (!Guid.TryParse(username_or_user_id, out _))
            friendUserId = await SnapchatClient.FindUserFromCache(username_or_user_id);


        List<SCFriendingFriendDisplayNameParam> _param = new List<SCFriendingFriendDisplayNameParam>
        {
            new SCFriendingFriendDisplayNameParam(new SCFriendingFriendDisplayNameParam { FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(friendUserId), LowBits = GuidUtils.GetLowBits(friendUserId) }, DisplayName = newname })
        };
        var _SCFriendingFriendsDisplayNameChangeRequest = new SCFriendingFriendsDisplayNameChangeRequest
        {
            ParamsArray = { _param }
        };
        return await SnapchatGrpcClient.ChangeDisplayNameForFriendAsync(_SCFriendingFriendsDisplayNameChangeRequest);
    }

    private async Task<SCFriendingFriendsActionResponse> AddFriendGrpcInternalUserID(string user_id, string page, SCFriendingFriendAddParam.Types.SCFriendingAddSource AddSource, string suggestionToken)
    {
        if (string.IsNullOrEmpty(suggestionToken))
        {
            if (page == "add_friends_button_on_top_bar_on_camera")
            {
                throw new Exception(page + " requires a suggestion token");
            }

            List<SCFriendingFriendAddParam> _param = new List<SCFriendingFriendAddParam>
            {
                new SCFriendingFriendAddParam(new SCFriendingFriendAddParam { Source = AddSource, FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(user_id), LowBits = GuidUtils.GetLowBits(user_id) } })
            };
            var _SCFriendingFriendsAddRequest = new SCFriendingFriendsAddRequest
            {
                Page = page,
                ParamsArray = { _param }
            };
            var resp = await SnapchatGrpcClient.AddFriendAsync(_SCFriendingFriendsAddRequest);
            return resp;
        }
        else
        {
            List<SCFriendingFriendAddParam> _param = new List<SCFriendingFriendAddParam>
            {
                new SCFriendingFriendAddParam(new SCFriendingFriendAddParam { Source = AddSource, FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(user_id), LowBits = GuidUtils.GetLowBits(user_id) }, SuggestionToken = suggestionToken })
            };
            var _SCFriendingFriendsAddRequest = new SCFriendingFriendsAddRequest
            {
                Page = page,
                ParamsArray = { _param }
            };
            var resp = await SnapchatGrpcClient.AddFriendAsync(_SCFriendingFriendsAddRequest);
            return resp;
        }
    }

    private async Task<SCFriendingFriendsActionResponse> BulkAddFriendGrpcInternalUserID(HashSet<string> user_id, string page, SCFriendingFriendAddParam.Types.SCFriendingAddSource AddSource, string suggestionToken)
    {
        if (string.IsNullOrEmpty(suggestionToken))
        {
            throw new Exception("Suggestion Token is required for Bulk Add");
        }
        else
        {
            List<SCFriendingFriendAddParam> _param = new List<SCFriendingFriendAddParam>();

            foreach (var UserID in user_id)
            {
                _param.Add(new SCFriendingFriendAddParam(new SCFriendingFriendAddParam { Source = AddSource, FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(UserID), LowBits = GuidUtils.GetLowBits(UserID) }, SuggestionToken = suggestionToken }));
            }

            var _SCFriendingFriendsAddRequest = new SCFriendingFriendsAddRequest
            {
                Page = page,
                ParamsArray = { _param }
            };

            var resp = await SnapchatGrpcClient.AddFriendAsync(_SCFriendingFriendsAddRequest);
            return resp;
        }
    }

    private async Task<SCFriendingFriendsActionResponse> AddFriendGrpcInternalNonUserID(string username, string page, SCFriendingFriendAddParam.Types.SCFriendingAddSource AddSource)
    {
        var friendUserId = await SnapchatClient.FindUserFromCache(username);
        List<SCFriendingFriendAddParam> _param = new List<SCFriendingFriendAddParam>
        {
            new SCFriendingFriendAddParam(new SCFriendingFriendAddParam { Source = AddSource, FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(friendUserId), LowBits = GuidUtils.GetLowBits(friendUserId) } })
        };
        var _SCFriendingFriendsAddRequest = new SCFriendingFriendsAddRequest
        {
            Page = page,
            ParamsArray = { _param }
        };
        var resp = await SnapchatGrpcClient.AddFriendAsync(_SCFriendingFriendsAddRequest);
        return resp;
    }

    public Task<SCFriendingFriendsActionResponse> AddBySearchByUsername(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "search", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByUsername);
    }
    public Task<SCFriendingFriendsActionResponse> AddByProfileByUsername(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "add_friends_profile", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByUsername);
    }
    public Task<SCFriendingFriendsActionResponse> AddBySearchByDisplayName(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "search", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByDisplayName);
    }

    public Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string user_id, string suggestionToken)
    {
        return AddFriendGrpcInternalUserID(user_id, "add_friends_button_on_top_bar_on_camera", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedBySuggested, suggestionToken);
    }

    public Task<SCFriendingFriendsActionResponse> AddByRegister(HashSet<string> user_id, string suggestionToken)
    {
        return BulkAddFriendGrpcInternalUserID(user_id, "register", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedBySuggested, suggestionToken);
    }

    public Task<SCFriendingFriendsActionResponse> AddByGroupChat(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "profile", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByGroupChat);
    }

    public Task<SCFriendingFriendsActionResponse> AddByContact(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "contacts", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByPhone);
    }

    public Task<SCFriendingFriendsActionResponse> AddByMention(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "share_user", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedBySharedUsername);
    }

    public Task<SCFriendingFriendsActionResponse> SubscribeBySearch(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "subscription_user_story_on_discover_feed", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedBySubscription);
    }

    public Task<SCFriendingFriendsActionResponse> Subscribe(string username)
    {
        return AddFriendGrpcInternalNonUserID(username, "subscription_user_story_on_discover_feed", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedFromPublicProfile);
    }

    public Task<SCFriendingFriendsActionResponse> AcceptFriend(string user_id)
    {
        return AddFriendGrpcInternalUserID(user_id, "added_me_notification", SCFriendingFriendAddParam.Types.SCFriendingAddSource.AddedByAddedMeBack, null);
    }

    public async Task<SCFriendingFriendsActionResponse> RemoveFriend(string user_id)
    {

        List<SCFriendingFriendRemoveParam> _param = new List<SCFriendingFriendRemoveParam>
        {
            new SCFriendingFriendRemoveParam(new SCFriendingFriendRemoveParam { FriendId = new SCCOREUUID { HighBits = GuidUtils.GetHighBits(user_id), LowBits = GuidUtils.GetLowBits(user_id) } })
        };
        var _SCFriendingFriendsRemoveRequest = new SCFriendingFriendsRemoveRequest
        {
            ParamsArray = { _param }
        };
        var reply = await SnapchatGrpcClient.RemoveFriendAsync(_SCFriendingFriendsRemoveRequest);
        return reply;
    }

    public async Task<SCFriendingFriendsActionResponse> BlockFriend(string usernameOrUserId)
    {
        var friendUserId = usernameOrUserId;

        if (!Guid.TryParse(usernameOrUserId, out _))
            friendUserId = await SnapchatClient.FindUserFromCache(usernameOrUserId);

        var friendBlockParam = new SCFriendingFriendBlockParam
        {
            FriendId = new SCCOREUUID
            {
                HighBits = GuidUtils.GetHighBits(friendUserId),
                LowBits = GuidUtils.GetLowBits(friendUserId)
            }
        };

        var blockRequest = new SCFriendingFriendsBlockRequest
        {
            ParamsArray = { friendBlockParam }
        };

        var reply = await SnapchatGrpcClient.BlockFriendsAsync(blockRequest);
        return reply;
    }

    public async Task<SCFriendingFriendsActionResponse> UnBlockFriend(string usernameOrUserId)
    {
        var friendUserId = usernameOrUserId;

        if (!Guid.TryParse(usernameOrUserId, out _))
            friendUserId = await SnapchatClient.FindUserFromCache(usernameOrUserId);

        var friendUnblockParam = new SCFriendingFriendUnblockParam
        {
            FriendId = new SCCOREUUID
            {
                HighBits = GuidUtils.GetHighBits(friendUserId),
                LowBits = GuidUtils.GetLowBits(friendUserId)
            }
        };

        var unblockRequest = new SCFriendingFriendsUnblockRequest
        {
            ParamsArray = { friendUnblockParam }
        };

        var reply = await SnapchatGrpcClient.UnblockFriendsAsync(unblockRequest);
        return reply;
    }


    public async Task<ami_friends> SyncFriends()
    {
        var sync = new SyncClass { added_friends_sync_token = "", friends_sync_token = "", is_request_from_background = false };
        var parameters = new Dictionary<string, string> { { "friends_request", m_Utilities.JsonSerializeObject(sync) }, { "is_post_login_request", "false" } };
        var response = await Send(FriendSyncEndpointInfo, parameters);
        var result = m_Utilities.JsonDeserializeObject<ami_friends>(await response.Content.ReadAsStringAsync());
        return result;
    }

    public async Task<ami_friends> SyncFriends(string added_friends_sync_token, string friends_sync_token)
    {
        var sync = new SyncClass { added_friends_sync_token = added_friends_sync_token, friends_sync_token = friends_sync_token, is_request_from_background = false };
        var parameters = new Dictionary<string, string> { { "friends_request", m_Utilities.JsonSerializeObject(sync) }, { "is_post_login_request", "false" } };
        var response = await Send(FriendSyncEndpointInfo, parameters);
        var result = m_Utilities.JsonDeserializeObject<ami_friends>(await response.Content.ReadAsStringAsync());
        return result;
    }


    public async Task<bool> HasBitmoji(string username)
    {
        using (var _c = new HttpClient(new HttpClientHandler { Proxy = SnapchatClient.SnapchatConfig.Proxy }))
        {
            _c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
            var parser = await _c.GetAsync($"https://www.snapchat.com/add/{username}");
            if (parser.IsSuccessStatusCode)
            {
                var content = await parser.Content.ReadAsStringAsync();
                if (content.Contains("https://images.bitmoji.com/3d/avatar/") || content.Contains("https://cf-st.sc-cdn.net/aps/bolt/"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}