using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;

namespace SnapchatLib.Exceptions
{
    public class UsernameNotFoundException : Exception
    {
        public UsernameNotFoundException(string username) : base($"User with name \"{username}\" not found")
        {
        }
    }

    public class UserNumberNotFoundException : Exception
    {
        public UserNumberNotFoundException(string number) : base($"User with phone number \"{number}\" not found")
        {
        }
    }
}

namespace SnapchatLib
{
    internal class UserFinder
    {
        private static ConcurrentDictionary<string, string> LookupCache = new();
        private readonly ISnapchatHttpClient m_HttpClient;
        private readonly IUtilities m_Utilities;
        public UserFinder(ISnapchatHttpClient httpClient, IUtilities utilities)
        {
            m_HttpClient = httpClient;
            m_Utilities = utilities;
        }

        internal async Task<int> CacheFriendsList()
        {
            var sync = await m_HttpClient.Friend.SyncFriends();

            HashSet<string> FriendCount = new HashSet<string>();
            foreach (var friend in sync.friends)
            {
                if (friend?.user_id != null)
                {
                    LookupCache.TryAdd(friend.mutable_username, friend.user_id);
                    FriendCount.Add(friend.mutable_username);
                }
            }

            foreach (var friend in sync.added_friends)
            {
                if (friend?.user_id != null)
                {
                    LookupCache.TryAdd(friend.mutable_username, friend.user_id);
                    FriendCount.Add(friend.mutable_username);
                }
            }

            return FriendCount.Count;
        }

        public string FindUserFromFriendsListCache(string username)
        {
            return LookupCache.TryGetValue(username, out var userId) ? userId : throw new UsernameNotFoundException(username);
        }

        public async Task<string> FindUserFromCache(string username)
        {
            if (LookupCache.TryGetValue(username, out var userId)) return userId;

            userId = await m_HttpClient.Search.GetUserId(username);

            if (string.IsNullOrWhiteSpace(userId)) throw new UsernameNotFoundException(username);

            LookupCache.TryAdd(username, userId);
            return userId;
        }
    }
}