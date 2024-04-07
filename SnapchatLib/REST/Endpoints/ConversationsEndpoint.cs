using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Messaging;
using System.Linq;

namespace SnapchatLib.REST.Endpoints;

internal interface IConversationsEndpoint
{
    Task<HashSet<ConversationInfo>> GetConversationID(HashSet<string> friend);
}

internal class ConversationsEndpoint : EndpointAccessor, IConversationsEndpoint
{
    public ConversationsEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<HashSet<ConversationInfo>> CreateConversation(HashSet<string> friend)
    {
        var conversationInfo = new HashSet<ConversationInfo>();

        var friends = await SnapchatClient.SyncFriends();

        HashSet<string> friendcount = new HashSet<string>();

        foreach (var f in friends.added_friends)
        {
            friendcount.Add(f.mutable_username);
        }

        foreach (var f in friends.friends)
        {
            friendcount.Add(f.mutable_username);
        }

        var friendsync = await SnapchatGrpcClient.QueryConversations(new QueryConversationsRequest
        {
            SendingUserId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id)) },
            SyncToken = ByteString.CopyFromUtf8("UseV3"),
            RequestedPageSize = (uint)friendcount.Count,
        });

        // Create a dictionary to map usernames to UUIDs
        Dictionary<string, ByteString> friendsUuids = friend
            .Where(friendcount.Contains)
            .ToDictionary(
                friendUsername => friendUsername,
                friendUsername => ByteString.CopyFrom(GuidUtils.ToBytes(SnapchatClient.FindUserFromFriendsListCache(friendUsername)))
            );

        foreach (var convo in friendsync.Conversations)
        {
            foreach (var participant in convo.Participants)
            {
                if (friendsUuids.Values.Contains(participant.Id))
                {
                    conversationInfo.Add(convo.ConversationInfo);
                    break;
                }
            }
        }

        return conversationInfo;
    }

    public async Task<HashSet<ConversationInfo>> GetConversationID(HashSet<string> friend)
    {
        var conversations = new HashSet<ConversationInfo>();
        var conversationInfo = await CreateConversation(friend);

        foreach (var convo in conversationInfo)
        {
            if (convo != null && convo.ConversationId != null)
            {
                conversations.Add(convo);
            }
        }

        return conversations;
    }
}