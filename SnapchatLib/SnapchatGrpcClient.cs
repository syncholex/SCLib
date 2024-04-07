using System;
using System.Collections.Generic;
using System.Net.Http;
using Grpc.Core;
using System.Threading.Tasks;
using Grpc.Net.Client;
using SnapProto.Snapchat.Friending;
using SnapProto.Snapchat.Activation.Api;
using SnapProto.Snapchat.Abuse.Support;
using SnapProto.Snapchat.Content.V2;
using SnapProto.Snapchat.Messaging;
using SnapProto.Snap.Security;
using SnapProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Cdp.Cof;
using SnapProto.Services.Snapchat.Activation.Api;
using SnapProto.Services.Snapchat.Friending.Server;
using SnapProto.Services.Snapchat.Cdp.Cof;
using SnapProto.Services.Snapchat.Janus.Api;
using SnapProto.Services.Snap.Security;
using SnapProto.Services.Snapchat.Abuse.Support;
using SnapProto.Services.Snapchat.Content.V2;
using SnapProto.Services.Messagingcoreservice;
using SnapProto.Services.Com.Snapchat.Deltaforce.External;
using SnapProto.Services.Snapchat.Notification.Notificationdata;
using SnapProto.Snapchat.Notification.Notificationdata;
using SnapProto.Com.Snapchat.Proto.Security;
using SnapProto.Services.Com.Snapchat.Proto.Security;
using DeltaSyncRequest = SnapProto.Snapchat.Messaging.DeltaSyncRequest;
using DeltaSyncResponse = SnapProto.Snapchat.Messaging.DeltaSyncResponse;
using System.Net;
using Google.Protobuf;
using SnapProto.Com.Snapchat.Deltaforce;
using SnapchatLib.Models.SignJson;
using SnapchatLib.Extras;
using SnapchatLibProto.Snapchat.Activation.Api;
using Grpc.Net.Compression;
using SnapProto.Services.Snapchat.Fidelius;
using SnapProto.Snapchat.Fidelius;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SnapchatLib;

internal readonly struct GrpcSignResult
{
    public string Attestation { get; }

    public GrpcSignResult(string attestation)
    {
        Attestation = attestation;
    }
}

internal interface ISnapchatGrpcClient
{
    Task<List<DeliveryDestination>> CreateDestinations(HashSet<string> users);
    Task<QueryConversationsResponse> QueryConversations(QueryConversationsRequest request);
    AsyncUnaryCall<SCFideliusPollRecryptResponse> PollRecrypt(SCFideliusPollRecryptRequest request);
    void SetupServiceClients();
    Task<GrpcSignResult> Sign(string url);
    Task<DeltaSyncResponse> DeltaSyncAsync(DeltaSyncRequest request);
    Task<SendTypingNotificationResponse> SendTypingNotification(SendTypingNotificationRequest request);
    Task<ConversationUpdateResponse> ConversationUpdateRequest(ConversationUpdateRequest request);
    Task<QueryMessagesResponse> QueryMessages(QueryMessagesRequest request);
    AsyncUnaryCall<SCSuggestUsernamePbCheckUsernameResponseV2> CheckUsernameAsync(SCSuggestUsernamePbCheckUsernameRequestV2 request);
    AsyncUnaryCall<SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsernameAsync(SCSuggestUsernamePbSuggestUsernameRequestV2 request);
    AsyncUnaryCall<SCCofConfigTargetingResponse> CofAsync(SCCofConfigTargetingRequest request, bool useaccesstoken);
    AsyncUnaryCall<BatchDeltaSyncResponse> BatchDeltaSyncAsync(BatchDeltaSyncRequest request);
    AsyncUnaryCall<ConditionalPutResponse> ConditionalPutAsync(ConditionalPutRequest request);
    AsyncUnaryCall<SCJanusRegisterWithUsernamePasswordResponse> RegisterAsync(SCJanusRegisterWithUsernamePasswordRequest request);
    AsyncUnaryCall<SnapchatLibProto.Snapchat.Janus.Api.SCJanusRegisterWithUsernamePasswordResponse> RegisterAsyncV2(SnapchatLibProto.Snapchat.Janus.Api.SCJanusRegisterWithUsernamePasswordRequest request);
    AsyncUnaryCall<SnapchatLibProto.Snapchat.Janus.Api.SCJanusLoginWithPasswordResponse> LoginAsync(SnapchatLibProto.Snapchat.Janus.Api.SCJanusLoginWithPasswordRequest request);
    AsyncUnaryCall<SCJanusSendODLVCodeResponse> SendEmail2FA(SCJanusSendODLVCodeRequest request);
    AsyncUnaryCall<SCJanusVerifyODLVResponse> LoginEmail2FA(SCJanusVerifyODLVRequest request);
    Task<SCPBSecurityGetUrlReputationResponse> CheckUrl(SCPBSecurityGetUrlReputationRequest request);
    AsyncUnaryCall<ArgosGetTokensResponse> GetTokensAsync(ArgosGetTokensRequest request);
    AsyncUnaryCall<SyncConversationsResponse> SyncConversationsAsync(SyncConversationsRequest request);
    AsyncUnaryCall<CreateContentMessageResponse> CreateContentMessageAsync(CreateContentMessageRequest request);
    AsyncUnaryCall<SCBoltv2GetUploadLocationsResponse> GetUploadLocationsAsync(SCBoltv2GetUploadLocationsRequest request);
    AsyncUnaryCall<SCChangeUsernamePbChangeUsernameResponse> ChangeUsernameAsync(SCChangeUsernamePbChangeUsernameRequest request);
    Task<SCReportSendReportResponse> ReportUserAsync(SCReportSendReportRequest request);
    Task<SCFriendingFriendsActionResponse> AddFriendAsync(SCFriendingFriendsAddRequest request);
    Task<SCFriendingFriendsActionResponse> RemoveFriendAsync(SCFriendingFriendsRemoveRequest request);
    Task<SCFriendingFriendsActionResponse> BlockFriendsAsync(SCFriendingFriendsBlockRequest request);
    Task<SCFriendingFriendsActionResponse> UnblockFriendsAsync(SCFriendingFriendsUnblockRequest request);
    Task<SCFriendingContactBookUploadResponse> FullSyncContactBookUploadAsync(SCFriendingContactBookUploadRequest request);
    Task<SCFriendingFriendsActionResponse> ChangeDisplayNameForFriendAsync(SCFriendingFriendsDisplayNameChangeRequest request);
    AsyncUnaryCall<SCNotificationRegisterDeviceResponse> RegisterDeviceAsync(SCNotificationRegisterDeviceRequest request);
    AsyncUnaryCall<UpdateContentMessageResponse> UpdateContentMessageAsync(UpdateContentMessageRequest request);
    Task<UpdateEmailResponse> UpdateEmail(UpdateEmailRequest request);
    AsyncUnaryCall<SCChangeUsernamePbGetLatestUsernameChangeDateResponse> GetLatestUsernameChangeDate(SCChangeUsernamePbGetLatestUsernameChangeDateRequest request);
}

internal class SnapchatGrpcClient : ISnapchatGrpcClient
{
    private HttpClient AndroidHttpClient
    {
        get
        {
            if (m_AndroidHttpClient != null) ConfigureClient(m_AndroidHttpClient);
            else m_AndroidHttpClient = CreateHttpClient();
            return m_AndroidHttpClient;
        }
    }

    private HttpClient m_AndroidHttpClient;
    private HttpClient HttpClient => AndroidHttpClient;

    private readonly SnapchatClient m_SnapchatClient;
    private readonly IUtilities m_Utilities;
    private int ConfigTimeout => m_SnapchatClient.SnapchatConfig.Timeout;

    private AccountEmailService.AccountEmailServiceClient AccountEmailServiceClient { get; set; }
    private MediaDeliveryService.MediaDeliveryServiceClient MediaDeliveryServiceClient { get; set; }
    private MessagingCoreService.MessagingCoreServiceClient MessagingCoreServiceClient { get; set; }
    private ChangeUsernameService.ChangeUsernameServiceClient ChangeUsernameServiceClient { get; set; }
    private ReportService.ReportServiceClient ReportServiceClient { get; set; }
    private ArgosService.ArgosServiceClient ArgosServiceClient { get; set; }
    private FriendAction.FriendActionClient FriendActionClient { get; set; }
    private LoginService.LoginServiceClient LoginServiceClient { get; set; }
    private SnapchatLibProto.Snapchat.Janus.Api.LoginService.LoginServiceClient LoginServiceClient2 { get; set; }
    private RegistrationService.RegistrationServiceClient RegistrationServiceClient { get; set; }
    private CircumstancesService.CircumstancesServiceClient CircumstancesClient { get; set; }
    private SnapchatLibProto.Snapchat.Janus.Api.RegistrationService.RegistrationServiceClient RegistrationServiceClientV2 { get; set; }
    private SnapchatLibProto.Snapchat.Activation.Api.SuggestUsernameService.SuggestUsernameServiceClient SuggestUsernameServiceClientV2 { get; set; }
    private ContactBook.ContactBookClient ContactBookClient { get; set; }
    private UrlReputationService.UrlReputationServiceClient UrlReputationServiceClient { get; set; }
    private DeltaForce.DeltaForceClient DeltaForceClient { get; set; }
    private PushNotificationDataRegistryService.PushNotificationDataRegistryServiceClient PushNotificationDataRegistryServiceClient { get; set; }
    private FideliusRecryptService.FideliusRecryptServiceClient FideliusRecryptServiceClient { get; set; }
    public SnapchatGrpcClient(SnapchatClient client, IUtilities utilities)
    {
        m_SnapchatClient = client;
        m_Utilities = utilities;
    }

    public void SetupServiceClients()
    {
        var channel = CreateChannel();
        var channel2 = CreateChannel2();
        MediaDeliveryServiceClient ??= new MediaDeliveryService.MediaDeliveryServiceClient(channel);
        MessagingCoreServiceClient ??= new MessagingCoreService.MessagingCoreServiceClient(channel2);
        ChangeUsernameServiceClient ??= new ChangeUsernameService.ChangeUsernameServiceClient(channel);
        ReportServiceClient ??= new ReportService.ReportServiceClient(channel);
        ArgosServiceClient ??= new ArgosService.ArgosServiceClient(channel);
        FriendActionClient ??= new FriendAction.FriendActionClient(channel);
        LoginServiceClient ??= new LoginService.LoginServiceClient(channel);
        LoginServiceClient2 ??= new SnapchatLibProto.Snapchat.Janus.Api.LoginService.LoginServiceClient(channel);
        CircumstancesClient ??= new CircumstancesService.CircumstancesServiceClient(channel);
        SuggestUsernameServiceClientV2 ??= new SnapchatLibProto.Snapchat.Activation.Api.SuggestUsernameService.SuggestUsernameServiceClient(channel);
        ContactBookClient ??= new ContactBook.ContactBookClient(channel);
        RegistrationServiceClient ??= new RegistrationService.RegistrationServiceClient(channel);
        RegistrationServiceClientV2 ??= new SnapchatLibProto.Snapchat.Janus.Api.RegistrationService.RegistrationServiceClient(channel);
        DeltaForceClient ??= new DeltaForce.DeltaForceClient(channel);
        PushNotificationDataRegistryServiceClient ??= new PushNotificationDataRegistryService.PushNotificationDataRegistryServiceClient(channel);
        UrlReputationServiceClient ??= new UrlReputationService.UrlReputationServiceClient(channel);
        AccountEmailServiceClient ??= new AccountEmailService.AccountEmailServiceClient(channel);
        FideliusRecryptServiceClient ??= new FideliusRecryptService.FideliusRecryptServiceClient(channel);
    }

    public async Task<GrpcSignResult> Sign(string url)
    {
        var timestamp = m_Utilities.UtcTimestamp();

        var signResult = m_Utilities.JsonDeserializeObject<SignResponseJson>(await m_SnapchatClient.SignRequest(url));
        return new GrpcSignResult(signResult.x_snapchat_att);
    }

    private X509Certificate2 GenerateSelfSignedCertificate()
    {
        using (RSA rsa = RSA.Create(2048)) // using RSA with 2048-bit key
        {
            var request = new CertificateRequest($"cn={m_Utilities.RandomString(12)}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Set the validity period. Here set to one year.
            var start = DateTimeOffset.Now;
            var end = start.AddYears(1);

            // Create a self-signed certificate
            var certificate = request.CreateSelfSigned(start, end);

            // Export the certificate with the private key and import it back to get a 'X509Certificate2' object with a private key

            // Random Password
            var PasswordForCert = m_Utilities.RandomString(12);
            return new X509Certificate2(certificate.Export(X509ContentType.Pfx, PasswordForCert), PasswordForCert, X509KeyStorageFlags.Exportable);
        }
    }
    private HttpClient CreateHttpClient()
    {
        // Hacking TLS Spoofing
        var handler = new HttpClientHandler();
        X509Certificate2 TLSSpoofCert = GenerateSelfSignedCertificate();

        if (m_SnapchatClient.SnapchatConfig.TLSSpoofing)
        {
            handler = new HttpClientHandler()
            {
                Proxy = m_SnapchatClient.SnapchatConfig.Proxy,
                AutomaticDecompression = DecompressionMethods.All,
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    cert = TLSSpoofCert;
                    TlsCipherSuite cipherSuite = EnumerableExtension.RandomEnumValue<TlsCipherSuite>();
                    return cert.Equals(TLSSpoofCert);
                }
            };
        }
        else
        {
            handler = new HttpClientHandler()
            {
                Proxy = m_SnapchatClient.SnapchatConfig.Proxy,
                AutomaticDecompression = DecompressionMethods.All,
            };
        }

        var client = new HttpClient(new CustomGrpcUserAgentHandler(m_SnapchatClient.SnapchatConfig)
        {
            InnerHandler = handler
        });

        client.Timeout = TimeSpan.FromSeconds(m_SnapchatClient.SnapchatConfig.Timeout);
        ConfigureClient(client);
        return client;
    }

    private void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
    }

    public GrpcChannel CreateChannel()
    {
        return GrpcChannel.ForAddress("https://aws.api.snapchat.com/", new GrpcChannelOptions
        {
            HttpClient = HttpClient,
            CompressionProviders = new List<ICompressionProvider>
            {
                new GzipCompressionProvider(CompressionLevel.Optimal),
                new DeflateCompressionProvider(CompressionLevel.Optimal) // For deflate compression
            }
        });

    }

    public GrpcChannel CreateChannel2()
    {
        return GrpcChannel.ForAddress("https://aws-proxy-gcp.api.snapchat.com/", new GrpcChannelOptions
        {
            HttpClient = HttpClient,
            CompressionProviders = new List<ICompressionProvider> { new GzipCompressionProvider(System.IO.Compression.CompressionLevel.Optimal) },
        });
    }
    public async Task<List<DeliveryDestination>> CreateDestinations(HashSet<string> users)
    {

        var info = await m_SnapchatClient.GetConversationID(users);

        if (info == null)
            throw new Exception($"Conversation ID not found for users: {string.Join(", ", users)}");

        var destinations = new List<DeliveryDestination>();

        // - Create DeliveryDestination
        foreach (var c in info)
        {
            destinations.Add(new DeliveryDestination
            {
                ConversationDestination = new ConversationDestination
                {
                    Id = c.ConversationId,
                    CurrentVersion = c.ConversationVersion
                }
            });
        }

        return destinations;
    }

    private DateTime DeadlineFromTimeout()
    {
        return DateTime.UtcNow.AddSeconds(ConfigTimeout);
    }

    /**
     * 
     * If any GRPC call requires x-snapchat-att, add this:
     * 
     * <code>, metadata: await CreateMetadataAsync(url)</code>
     * 
     */
    public async Task<DeltaSyncResponse> DeltaSyncAsync(DeltaSyncRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() }
        };
        return await MessagingCoreServiceClient.DeltaSyncAsync(request, metadata, DeadlineFromTimeout());
    }

    public async Task<SendTypingNotificationResponse> SendTypingNotification(SendTypingNotificationRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        // TODO
        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() }
        };

        return await MessagingCoreServiceClient.SendTypingNotificationAsync(request, metadata, DeadlineFromTimeout());
    }


    public async Task<ConversationUpdateResponse> ConversationUpdateRequest(ConversationUpdateRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        // TODO
        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() }
        };

        return await MessagingCoreServiceClient.UpdateConversationAsync(request, metadata, DeadlineFromTimeout());
    }

    public async Task<QueryMessagesResponse> QueryMessages(QueryMessagesRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() }
        };

        return await MessagingCoreServiceClient.QueryMessagesAsync(request, metadata, DeadlineFromTimeout());
    }


    /* TODO
    public async Task<> CreateAvatar(SCBitmojiCreateAvatarRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "device_model",m_SnapchatClient.SnapchatConfig.phoneModel }, 
            { "user_id", m_SnapchatClient.SnapchatConfig.user_id },
            { "country_code", m_SnapchatClient.SnapchatConfig.AccountCountryCode }, 
            { "locale", "en_US" }, 
            { "os_type", "1" }
        };
    }
    */

    public async Task<QueryConversationsResponse> QueryConversations(QueryConversationsRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() }
        };

        return await MessagingCoreServiceClient.QueryConversationsAsync(request, metadata, DeadlineFromTimeout());
    }

    public async Task<UpdateEmailResponse> UpdateEmail(UpdateEmailRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "accept-language", "en" },
            { "accept-encoding", "br" },
        };

        return await AccountEmailServiceClient.UpdateEmailAsync(request, metadata, DateTime.UtcNow.AddMilliseconds(45100));
    }

    public AsyncUnaryCall<BatchDeltaSyncResponse> BatchDeltaSyncAsync(BatchDeltaSyncRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() }
        };


        return MessagingCoreServiceClient.BatchDeltaSyncAsync(request, metadata, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<UpdateContentMessageResponse> UpdateContentMessageAsync(UpdateContentMessageRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
        {
            throw new Exception("Access Token was null, preventing from ban.");
        }

        var metadata = new Metadata
        {
            { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token },
            { "x-request-id", Guid.NewGuid().ToString() },
            { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() }
        };


        return MessagingCoreServiceClient.UpdateContentMessageAsync(request, metadata, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SCNotificationRegisterDeviceResponse> RegisterDeviceAsync(SCNotificationRegisterDeviceRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return PushNotificationDataRegistryServiceClient.RegisterDeviceAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "X-Snap-Route-Tag", "" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SCFideliusPollRecryptResponse> PollRecrypt(SCFideliusPollRecryptRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return FideliusRecryptServiceClient.PollRecryptAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<ConditionalPutResponse> ConditionalPutAsync(ConditionalPutRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");
        var x_snap_device_id = m_SnapchatClient.SnapchatConfig.androidId.GetHashCode() % 1000000;
        return DeltaForceClient.ConditionalPutAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-device-id", x_snap_device_id.ToString() } }, deadline: DeadlineFromTimeout());
    }
    public async Task<SCFriendingFriendsActionResponse> AddFriendAsync(SCFriendingFriendsAddRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.AddFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() }, { "accept-language", "en" } }, deadline: DateTime.UtcNow.AddMilliseconds(20100));
    }
    public async Task<SCFriendingFriendsActionResponse> BlockFriendsAsync(SCFriendingFriendsBlockRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.BlockFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public async Task<SCFriendingFriendsActionResponse> UnblockFriendsAsync(SCFriendingFriendsUnblockRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.UnblockFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public async Task<SCFriendingFriendsActionResponse> RemoveFriendAsync(SCFriendingFriendsRemoveRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.RemoveFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public async Task<SCFriendingFriendsActionResponse> ChangeDisplayNameForFriendAsync(SCFriendingFriendsDisplayNameChangeRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await FriendActionClient.ChangeDisplayNameForFriendsAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public async Task<SCFriendingContactBookUploadResponse> FullSyncContactBookUploadAsync(SCFriendingContactBookUploadRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var token = await m_SnapchatClient.HttpClient.Get_Tokens.GetArgosTokenCached();

        if (string.IsNullOrEmpty(token))
            throw new Exception("Argos Token was null protecting from ban");

        return await ContactBookClient.FullSyncContactBookUploadAsync(request, new Metadata { { "x-snapchat-att-token", token }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-snapchat-argos-strict-enforcement", "true" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DateTime.UtcNow.AddMilliseconds(20100));
    }
    public AsyncUnaryCall<ArgosGetTokensResponse> GetTokensAsync(ArgosGetTokensRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return ArgosServiceClient.GetTokensAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "X-Snap-Route-Tag", "" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SyncConversationsResponse> SyncConversationsAsync(SyncConversationsRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return MessagingCoreServiceClient.SyncConversationsAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() }, { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<CreateContentMessageResponse> CreateContentMessageAsync(CreateContentMessageRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return MessagingCoreServiceClient.CreateContentMessageAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() }, { "mcs-cof-ids-bin", new CofIds { Ids = { m_SnapchatClient.SnapchatConfig.mcs_cof_ids_bin } }.ToByteArray() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SCBoltv2GetUploadLocationsResponse> GetUploadLocationsAsync(SCBoltv2GetUploadLocationsRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return MediaDeliveryServiceClient.getUploadLocationsAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SCChangeUsernamePbChangeUsernameResponse> ChangeUsernameAsync(SCChangeUsernamePbChangeUsernameRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return ChangeUsernameServiceClient.ChangeUsernameAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() }, { "accept-language", "en" }, { "allow-recycled-username", "true" } }, deadline: DeadlineFromTimeout());
    }

    public async Task<SCReportSendReportResponse> ReportUserAsync(SCReportSendReportRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return await ReportServiceClient.SendReportAsync(request, new Metadata { { "device_model", m_SnapchatClient.SnapchatConfig.phoneModel }, { "country_code", m_SnapchatClient.SnapchatConfig.AccountCountryCode }, { "locale", "en_US" }, { "os_type", "1" }, { "user_id", m_SnapchatClient.SnapchatConfig.user_id }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DateTime.UtcNow.AddMilliseconds(15100));
    }

    public async Task<SCPBSecurityGetUrlReputationResponse> CheckUrl(SCPBSecurityGetUrlReputationRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        var attestation = await Sign("/com.snapchat.proto.security.UrlReputationService/GetUrlReputation");
        return await UrlReputationServiceClient.GetUrlReputationAsync(request, new Metadata { { "x-snapchat-att", attestation.Attestation }, { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCChangeUsernamePbGetLatestUsernameChangeDateResponse> GetLatestUsernameChangeDate(SCChangeUsernamePbGetLatestUsernameChangeDateRequest request)
    {
        if (string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token))
            throw new Exception("Access Token was null protecting from ban");

        return ChangeUsernameServiceClient.GetLatestUsernameChangeDateAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SnapchatLibProto.Snapchat.Janus.Api.SCJanusLoginWithPasswordResponse> LoginAsync(SnapchatLibProto.Snapchat.Janus.Api.SCJanusLoginWithPasswordRequest request)
    {
        var metadata = new Metadata
        {
            { "x-request-id", Guid.NewGuid().ToString() },
            { "x-snap-janus-request-created-at", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() },
            { "accept-language", "en" },
            { "accept-language", "br" },
        };
        return LoginServiceClient2.LoginWithPasswordAsync(request, metadata, deadline: DateTime.UtcNow.AddMilliseconds(30100));
    }

    public AsyncUnaryCall<SCJanusRegisterWithUsernamePasswordResponse> RegisterAsync(SCJanusRegisterWithUsernamePasswordRequest request)
    {
        return RegistrationServiceClient.RegisterWithUsernamePasswordAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-janus-request-created-at", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }, { "accept-language", "en" }, { "allow-recycled-username", "true" } }, deadline: DeadlineFromTimeout());
    }

    public AsyncUnaryCall<SnapchatLibProto.Snapchat.Janus.Api.SCJanusRegisterWithUsernamePasswordResponse> RegisterAsyncV2(SnapchatLibProto.Snapchat.Janus.Api.SCJanusRegisterWithUsernamePasswordRequest request)
    {
        return RegistrationServiceClientV2.RegisterWithUsernamePasswordAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-janus-request-created-at", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }, { "accept-language", "en" }, { "allow-recycled-username", "true" } }, deadline: DateTime.UtcNow.AddMilliseconds(30100));
    }
    public AsyncUnaryCall<SCJanusSendODLVCodeResponse> SendEmail2FA(SCJanusSendODLVCodeRequest request)
    {
        return LoginServiceClient.SendODLVCodeAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-janus-request-created-at", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }, { "accept-language", "en" } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCJanusVerifyODLVResponse> LoginEmail2FA(SCJanusVerifyODLVRequest request)
    {
        return LoginServiceClient.VerifyODLVAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "x-snap-janus-request-created-at", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }, { "accept-language", "en" } }, deadline: DeadlineFromTimeout());
    }
    public AsyncUnaryCall<SCCofConfigTargetingResponse> CofAsync(SCCofConfigTargetingRequest request, bool useaccesstoken)
    {
        if (!string.IsNullOrEmpty(m_SnapchatClient.SnapchatConfig.Access_Token) && useaccesstoken == true)
        {
            return CircumstancesClient.targetingQueryAsync(request, new Metadata { { "X-Snap-Access-Token", m_SnapchatClient.SnapchatConfig.Access_Token }, { "accept-language", "en_US" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DateTime.UtcNow.AddSeconds(270));
        }
        else
        {
            return CircumstancesClient.targetingQueryAsync(request, new Metadata { { "accept-language", "en_US" }, { "x-request-id", Guid.NewGuid().ToString() } }, deadline: DeadlineFromTimeout());
        }
    }
    public AsyncUnaryCall<SCSuggestUsernamePbSuggestUsernameResponseV2> SuggestUsernameAsync(SCSuggestUsernamePbSuggestUsernameRequestV2 request)
    {
        return SuggestUsernameServiceClientV2.SuggestUsernameAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "allow-recycled-username", "true" } }, deadline: DateTime.UtcNow.AddSeconds(10));
    }

    public AsyncUnaryCall<SCSuggestUsernamePbCheckUsernameResponseV2> CheckUsernameAsync(SCSuggestUsernamePbCheckUsernameRequestV2 request)
    {
        return SuggestUsernameServiceClientV2.CheckUsernameAsync(request, new Metadata { { "x-request-id", Guid.NewGuid().ToString() }, { "accept-language", "en" }, { "allow-recycled-username", "true" } }, deadline: DateTime.UtcNow.AddSeconds(10));
    }
}