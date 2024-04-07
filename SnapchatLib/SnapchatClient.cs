using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLib.REST;
using SnapchatLib.REST.Models;
using SnapchatLibProto.Snapchat.Activation.Api;
using SnapProto.Com.Snapchat.Proto.Security;
using SnapProto.Ranking.Serving.Jaguar;
using SnapProto.Snapchat.Content.V2;
using SnapProto.Snapchat.Friending;
using SnapProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Messaging;
using SnapProto.Snapchat.Search;
using static SnapchatLib.REST.Models.SyncResponse;

namespace SnapchatLib;

public class SnapchatClient : IDisposable
{
    internal SnapchatClient() { }

    internal IClientLogger m_Logger;

    internal SnapchatClient(SnapchatConfig config, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator)
    {
        SnapchatConfig = new SnapchatLockedConfig(config);
        HttpClient = httpClient;
        GrpcClient = grpcClient;
        m_Logger = logger;
        m_Utilities = utilities;
        m_RequestConfigurator = configurator;

    }
    public SnapchatClient(SnapchatConfig config)
    {
        SnapchatConfig = new SnapchatLockedConfig(config);
        m_Logger = new ClientLogger(SnapchatConfig);
        m_Utilities = config.Utilities;

        GrpcClient = new SnapchatGrpcClient(this, m_Utilities);
        m_RequestConfigurator = new RequestConfigurator(m_Logger, m_Utilities);

        HttpClient = new SnapchatHttpClient(this, GrpcClient, m_Logger, m_Utilities, m_RequestConfigurator);
        m_UserFinder = new UserFinder(HttpClient, m_Utilities);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    // Use C# finalizer syntax for finalization code.
    // This finalizer will run only if the Dispose method
    // does not get called.
    // It gives your base class the opportunity to finalize.
    // Do not provide finalizer in types derived from this class.
    ~SnapchatClient()
    {
        // Do not re-create Dispose clean-up code here.
        // Calling Dispose(disposing: false) is optimal in terms of
        // readability and maintainability.
        Dispose();
    }
    static long GenerateSessionId()
    {
        byte[] randomBytes = new byte[8]; // Array to hold 8 bytes
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes); // Fill the array with 8 random bytes
        }

        return BitConverter.ToInt64(randomBytes, 0); // Convert to long and return
    }
    private void ClientSetup()
    {
        m_Logger.Debug("//==Debug Info==\\");
        m_Logger.Debug("APIKEY: " + Encoding.UTF8.GetBytes(SnapchatConfig.ApiKey));
        m_Logger.Debug("Username: " + SnapchatConfig.Username);
        m_Logger.Debug("[FLAG] BandwithSaver: " + SnapchatConfig.BandwithSaver);
        m_Logger.Debug("[FLAG] TLSSpoofing: " + SnapchatConfig.TLSSpoofing);
        m_Logger.Debug("Payload: " + payload);
        m_Logger.Debug("Mac: " + mac);
        m_Logger.Debug("IV: " + IV);
        m_Logger.Debug("KEY: " + KEY);
        m_Logger.Debug("User-Id: " + SnapchatConfig.user_id);
        m_Logger.Debug("Environment Version: " + Environment.Version);
    }

    #region Fields
    internal string guid_sessionId { get; set; } = Guid.NewGuid().ToString();
    internal long sessionId { get; set; } = GenerateSessionId();
    internal int sessionQueryId { get; set; } = 0;
    internal long launchTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    internal string IV { get; set; }
    internal string KEY { get; set; }
    public string payload { get; set; }
    public string mac { get; set; }
    internal string odlvToken { get; set; }
    internal bool HasLoggedIn { get; set; }

    internal List<int> cofConfigData_Android = new List<int>();
    internal string loginFlowSessionId { get; set; }
    internal string ConfigResultsEtag { get; set; }
    internal string authenticationSessionId { get; set; }
    internal virtual ISnapchatHttpClient HttpClient { get; }
    internal virtual ISnapchatGrpcClient GrpcClient { get; }
    public virtual SnapchatLockedConfig SnapchatConfig { get; }
    internal int sequenceRequest { get; set; } = 1;

    private UserFinder m_UserFinder;
    internal readonly IUtilities m_Utilities;
    private readonly IRequestConfigurator m_RequestConfigurator;

    #endregion

    public async Task InitClient()
    {
        try
        {
            ClientSetup();

            if (SnapchatConfig.AccessTokenExpirey <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                Console.WriteLine("Access token is still expired renewing.");
                await RenewAccessTokens();
            }
            else
            {
                Console.WriteLine("Access token is still valid.");
            }

            if (string.IsNullOrEmpty(SnapchatConfig.user_id))
            {
                throw new Exception("user_id cannot be null");
            }
            var friendcount = await m_UserFinder.CacheFriendsList();

            GrpcClient.SetupServiceClients();
        }
        catch (Exception ex)
        {
            if (SnapchatConfig.Debug)
            {
                throw new Exception(ex.ToString());
            }

            throw new Exception("Failed to InitClient", ex);
        }
    }

    public void NoSignInit() // Mainly used for only calling Suggest / Check Username
    {
        try
        {
            ClientSetup();
            GrpcClient.SetupServiceClients();
        }
        catch (Exception ex)
        {
            if (SnapchatConfig.Debug)
            {
                throw new Exception(ex.ToString());
            }

            throw new Exception("Failed to NoSignInit", ex);
        }
    }

    #region Settings / Profile

    public virtual async Task ChangeUsername(string username, string password)
    {
        await HttpClient.ChangeUsername.ChangeUsername(username, password);
    }

    public virtual async Task<SCSuggestUsernamePbCheckUsernameResponseV2> CheckUsername(string username)
    {
        return await GrpcClient.CheckUsernameAsync(
                new SCSuggestUsernamePbCheckUsernameRequestV2
                {
                    RequestedUsername = username,
                    ClientId = Guid.NewGuid().ToString(),
                    DeviceId = Guid.NewGuid().ToString(),
                    VersionNumber = 0,
                });
    }

    public virtual async Task GetLatestUsername()
    {
        await HttpClient.ChangeUsername.GetLatestUsernameChangeDate();
    }
    public virtual async Task<string> ChangeEmail(string email)
    {
        return await HttpClient.ChangeEmail.ChangeEmail(email);
    }

    public virtual async Task<string> ResendEmail(string email)
    {
        return await HttpClient.ChangeEmail.ResendEmail(email);
    }
    public virtual async Task<SnapchatLibProto.Snapchat.Janus.Api.SCJanusRegisterWithUsernamePasswordResponse> Register(string username, string password, string firstname, string lastname, string email)
    {
        return await HttpClient.RegisterV2.Register(username, password, firstname, lastname, email);
    }
    public virtual async Task<SnapchatLibProto.Snapchat.Janus.Api.SCJanusRegisterWithUsernamePasswordResponse> RegisterNoIntegrityNoKeyAttest(string username, string password, string firstname, string lastname, string email)
    {
        return await HttpClient.RegisterV2.RegisterNoIntegrityNoKeyAttest(username, password, firstname, lastname, email);
    }
    public virtual async Task<WebCreationStatus> RegisterWeb(string firstname, string lastname, string username, string password, string email, string emailpassword)
    {
        return await HttpClient.RegisterV2.RegisterWeb(firstname, lastname, username, password, email, emailpassword);
    }

    public virtual async Task<ValidationStatus> VerifyEmail(string link)
    {
        return await HttpClient.RegisterV2.VerifyEmail(link);
    }

    public virtual string GetSnapchatConfirmationLink(string body)
    {
        return HttpClient.RegisterV2.GetSnapchatConfirmationLink(body);
    }

    public virtual async Task<string> GetProfileInfo(string username)
    {
        return await HttpClient.SnapchatterPublicInfo.GetProfileInfo(username);
    }

    public virtual async Task<SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsername(string first_name, string last_name)
    {
        return await HttpClient.SuggestUsername.SuggestUsername(first_name, last_name);
    }
    public virtual async Task<SCPBSecurityGetUrlReputationResponse.Types.SCPBSecurityUrlType> CheckUrl(string url)
    {
        return await HttpClient.CheckUrl.CheckUrl(url);
    }

    public virtual async Task<SnapchatLibProto.Snapchat.Janus.Api.SCJanusLoginWithPasswordResponse> Login(string username, string password)
    {
        return await HttpClient.Login.Login(username, password);
    }
    public virtual async Task<SCJanusVerifyODLVResponse> Login2FA(string twofactorcode)
    {
        return await HttpClient.Login.Login2FA(twofactorcode);
    }
    public virtual async Task<string> ReAuth(string password)
    {
        return await HttpClient.Reauth.ReAuth(password);
    }

    public virtual async Task<string> ChangePhoneRegister(string number, string countrycode)
    {
        return await HttpClient.PhoneVerify.ChangePhoneRegister(number, countrycode);
    }

    public virtual async Task<string> VerifyPhoneRegister(string code)
    {
        return await HttpClient.PhoneVerify.VerifyPhoneRegister(code);
    }

    public virtual async Task<string> ChangePhoneSettings(string number, string countrycode, string network_code)
    {
        return await HttpClient.PhoneVerify.ChangePhoneSettings(number, countrycode, network_code);
    }

    public virtual async Task<string> VerifyPhoneSettings(string code)
    {
        return await HttpClient.PhoneVerify.VerifyPhoneSettings(code);
    }

    #endregion

    #region Find / Search

    public string FindUserFromFriendsListCache(string username)
    {
        return m_UserFinder.FindUserFromFriendsListCache(username);
    }

    public virtual async Task<string> FindUserFromCache(string username)
    {
        return await m_UserFinder.FindUserFromCache(username);
    }

    public virtual async Task<SCFriendingContactBookUploadResponse> FindUsersViaPhone(string number, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.FindUsersViaPhone(number, CountryCode, randomfirstname);
    }

    public virtual async Task<SCFriendingContactBookUploadResponse> FindUsersViaEmail(string email, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.FindUsersViaEmail(email, CountryCode, randomfirstname);
    }
    public virtual async Task<List<string>> ReturnUsernameViaEmail(string number, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.ReturnUsernameViaEmail(number, CountryCode, randomfirstname);
    }
    public virtual async Task<List<string>> ReturnUsernameViaPhone(string number, string CountryCode, string randomfirstname)
    {
        return await HttpClient.FindUsers.ReturnUsernameViaPhone(number, CountryCode, randomfirstname);
    }
    public virtual async Task<string> GetUserId(string username)
    {
        return await HttpClient.Search.GetUserId(username);
    }
    public virtual async Task<bool> IsUserActive(string username)
    {
        return await HttpClient.Search.IsUserActive(username);
    }
    public virtual async Task<SCS2SearchResponse> FindUsersViaSearch(string username)
    {
        return await HttpClient.Search.FindUser(username);
    }

    #endregion

    #region Init / Setup
    public virtual async Task Validate()
    {
        await HttpClient.AccessToken.Validate();
    }

    internal virtual async Task RenewAccessTokens()
    {
        await HttpClient.AccessToken.RenewAccessTokens();
    }

    public virtual async Task GetArgosTokenCached()
    {
        await HttpClient.Get_Tokens.GetArgosTokenCached();
    }
    #endregion

    #region CreateAvatarData
    public async Task CreateBitmoji(bool male, int style, int body, int bottom, int bottom_tone1, int bottom_tone10, int bottom_tone2, int bottom_tone3, int bottom_tone4, int bottom_tone5, int bottom_tone6, int bottom_tone7, int bottom_tone8, int bottom_tone9, int brow, int clothing_type, int ear, int eyelash, int face_proportion, int footwear, int footwear_tone1, int footwear_tone10, int footwear_tone2, int footwear_tone3, int footwear_tone4, int footwear_tone5, int footwear_tone6, int footwear_tone7, int footwear_tone8, int footwear_tone9, int hair, int hair_tone, int is_tucked, int jaw, int mouth, int nose, int pupil, int pupil_tone, int skin_tone, int sock, int sock_tone1, int sock_tone2, int sock_tone3, int sock_tone4, int top, int top_tone1, int top_tone10, int top_tone2, int top_tone3, int top_tone4, int top_tone5, int top_tone6, int top_tone7, int top_tone8, int top_tone9)
    {
        await HttpClient.CreateAvatarData.CreateBitmoji(male, style, body, bottom, bottom_tone1, bottom_tone10, bottom_tone2, bottom_tone3, bottom_tone4, bottom_tone5, bottom_tone6, bottom_tone7, bottom_tone8, bottom_tone9, brow, clothing_type, ear, eyelash, face_proportion, footwear, footwear_tone1, footwear_tone10, footwear_tone2, footwear_tone3, footwear_tone4, footwear_tone5, footwear_tone6, footwear_tone7, footwear_tone8, footwear_tone9, hair, hair_tone, is_tucked, jaw, mouth, nose, pupil, pupil_tone, skin_tone, sock, sock_tone1, sock_tone2, sock_tone3, sock_tone4, top, top_tone1, top_tone10, top_tone2, top_tone3, top_tone4, top_tone5, top_tone6, top_tone7, top_tone8, top_tone9);
    }

    #endregion

    #region Settings

    public virtual async Task MakeStoryEveryone()
    {
        await HttpClient.Settings.MakeStoryEveryone();
    }

    public virtual async Task MakeStoryFriendsOnly()
    {
        await HttpClient.Settings.MakeStoryFriendsOnly();
    }
    #endregion

    #region Reporting
    public virtual async Task ReportUserRandom(string username)
    {
        await HttpClient.Reporting.ReportUserRandom(username);
    }
    public virtual async Task ReportBusinessStoryRandom(string username)
    {
        await HttpClient.Reporting.ReportBusinessStoryRandom(username);
    }

    #endregion

    #region Friend
    public virtual async Task<SCFriendingFriendsActionResponse> ChangeFriendDisplayName(string user_id, string newname)
    {
        return await HttpClient.Friend.ChangeFriendDisplayName(user_id, newname);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddBySearchByUsername(string username)
    {
        return await HttpClient.Friend.AddBySearchByUsername(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddBySearchByDisplayName(string username)
    {
        return await HttpClient.Friend.AddBySearchByDisplayName(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddByRegister(HashSet<string> username, string suggestionToken)
    {
        return await HttpClient.Friend.AddByRegister(username, suggestionToken);
    }

    public virtual async Task<bool> HasBitmoji(string username)
    {
        return await HttpClient.Friend.HasBitmoji(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddByGroupChat(string username)
    {
        return await HttpClient.Friend.AddByGroupChat(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddByContact(string username)
    {
        return await HttpClient.Friend.AddByContact(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddByMention(string username)
    {
        return await HttpClient.Friend.AddByMention(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AddByQuickAdd(string user_id, string suggestionToken)
    {
        return await HttpClient.Friend.AddByQuickAdd(user_id, suggestionToken);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> Subscribe(string username)
    {
        return await HttpClient.Friend.Subscribe(username);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> AcceptFriend(string user_id)
    {
        return await HttpClient.Friend.AcceptFriend(user_id);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> RemoveFriend(string user_id)
    {
        return await HttpClient.Friend.RemoveFriend(user_id);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> BlockFriend(string username_or_user_id)
    {
        return await HttpClient.Friend.BlockFriend(username_or_user_id);
    }

    public virtual async Task<SCFriendingFriendsActionResponse> UnBlockFriend(string username_or_user_id)
    {
        return await HttpClient.Friend.UnBlockFriend(username_or_user_id);
    }

    public virtual async Task<ami_friends> SyncFriends()
    {
        return await HttpClient.Friend.SyncFriends();
    }
    public virtual async Task<ami_friends> SyncFriends(string added_friends_sync_token, string friends_sync_token)
    {
        return await HttpClient.Friend.SyncFriends(added_friends_sync_token, friends_sync_token);
    }
    public virtual async Task<suggest_friend> GetSuggestionsHighAvailability()
    {
        return await HttpClient.SuggestFriend.GetSuggestionsHighAvailability();
    }

    public virtual async Task<suggest_friend> GetSuggestionsHighQuality()
    {
        return await HttpClient.SuggestFriend.GetSuggestionsHighQuality();
    }

    public virtual async Task<suggest_friend_on_demand> GetSuggestionsOnDemand()
    {
        return await HttpClient.SuggestFriend.GetSuggestionsOnDemand();
    }

    #endregion

    #region Story
    public virtual async Task PostStory(string inputfile, string swipeupurl = null, List<string> mentioned = null)
    {
        await HttpClient.PostStory.PostStory(inputfile, swipeupurl, mentioned);
    }

    public virtual async Task<SCSSMStoriesBatchResponse> GetStories(string username)
    {
        return await HttpClient.Preview.GetStories(username);
    }
    public virtual async Task ViewBuisnessStory(string username)
    {
        await HttpClient.GetBusinessProfileEndpoint.ViewStory(username);
    }

    #endregion

    #region Sign
    internal virtual async Task<string> SignRequest(string p)
    {
        return await HttpClient.Sign.SignRequest(p);
    }

    #endregion

    #region Messaging

    public virtual async Task SendMention(string username_or_user_id, HashSet<string> users)
    {
        await HttpClient.Messaging.SendMention(username_or_user_id, users);
    }

    public virtual async Task SendLink(string link, HashSet<string> users)
    {
        await HttpClient.Messaging.SendLink(link, users);
    }

    public virtual async Task SendMessage(string message, HashSet<string> users)
    {
        await HttpClient.Messaging.SendMessage(message, users);
    }

    public virtual async Task DeleteMessages(string user)
    {
        await HttpClient.Messaging.DeleteMessages(user);
    }

    public virtual async Task<QueryMessagesResponse> QueryMessages(HashSet<string> user, uint messageId)
    {
        return await HttpClient.Messaging.QueryMessages(user, messageId);
    }

    public virtual async Task SendReadNotification(string user, ulong currentVersion, ulong messageId, bool media = false)
    {
        await HttpClient.Messaging.SendReadNotification(user, currentVersion, messageId, media);
    }

    public virtual async Task SaveMessage(string user, ulong currentVersion, ulong messageId)
    {
        await HttpClient.Messaging.SaveMessage(user, currentVersion, messageId);
    }

    public virtual async Task SendTypingNotification(string username, ulong messageId)
    {
        await HttpClient.Messaging.SendTypingNotification(username, messageId);
    }

    public virtual async Task SendVoiceNote(string audiofile, HashSet<string> users)
    {
        await HttpClient.Messaging.SendVoiceNote(audiofile, users);
    }

    #endregion

    #region DirectSnap

    public virtual async Task PostDirect(string inputfile, HashSet<string> users, string swipeupurl = null, HashSet<string> mentioned = null)
    {
        await HttpClient.DirectSnap.PostDirect(inputfile, users, swipeupurl, mentioned);
    }

    #endregion

    #region GetUploadUrls
    public virtual async Task<SCBoltv2UploadLocation> GetUploadUrls()
    {
        return await HttpClient.GetUploadUrls.GetUploadUrls();
    }

    #endregion

    #region Conversations
    public virtual async Task<HashSet<ConversationInfo>> GetConversationID(HashSet<string> friend)
    {
        return await HttpClient.Conversations.GetConversationID(friend);
    }
    #endregion

}