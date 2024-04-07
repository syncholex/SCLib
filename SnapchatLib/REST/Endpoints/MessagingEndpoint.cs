using Google.Protobuf;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Messaging;
using System;
using static SnapProto.Snapchat.Messaging.ContentEnvelope.Types;
using SnapProto.Snapchat.Content;
using SnapchatLib.Encryption;
using SnapchatLib.Exceptions;
using SnapProto.Snapchat.Content.V2;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace SnapchatLib.REST.Endpoints
{
    internal interface IMessagingEndpoint
    {
        Task<QueryMessagesResponse> QueryMessages(HashSet<string> user, uint messageId);
        Task SendMention(string friend, HashSet<string> users);
        Task SendLink(string link, HashSet<string> users);
        Task SendMessage(string message, HashSet<string> users);
        Task<string> DeleteMessages(string user);
        Task SendVoiceNote(string audiofile, HashSet<string> users);
        Task SendTypingNotification(string user, ulong messageId);
        Task SendReadNotification(string user, ulong currentVersion, ulong messageId, bool media);
        Task SaveMessage(string user, ulong currentVersion, ulong messageId);
    }

    internal class MessagingEndpoint : EndpointAccessor, IMessagingEndpoint
    {

        private readonly ISnapchatGrpcClient m_SnapchatGrpcClient;
        public MessagingEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
            m_SnapchatGrpcClient = grpcClient;
        }

        private CreateContentMessageRequest CreateContentMessageRequest(List<DeliveryDestination> destinations, SCMessagingContents contents, ContentEnvelope.Types.ContentType contentType)
        {
            return new CreateContentMessageRequest
            {
                SenderId = new UUID
                {
                    Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                },
                ClientResolutionId = (ulong)m_Utilities.LongRandom(100000000000000000, 999999999999999999, new Random()),
                Destinations = { destinations },
                Contents = new ContentEnvelope
                {
                    ContentType = contentType,
                    Contents = contents.ToByteString(),
                    SavePolicy = SavePolicy.Lifetime
                }
            };
        }
        private CreateContentMessageRequest CreateContentMessageRequestVoiceNote(List<DeliveryDestination> destinations, SCMessagingContents contents, ContentEnvelope.Types.ContentType contentType, SCBoltv2UploadLocation sCBoltv2UploadLocation)
        {

            var mediaRef = new MediaReference
            {
                ContentObject = sCBoltv2UploadLocation.ContentReference.ContentObject,
                MediaType = MediaReference.Types.SCCMediaType.Audio
            };
            return new CreateContentMessageRequest
            {
                SenderId = new UUID
                {
                    Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                },
                ClientResolutionId = (ulong)m_Utilities.LongRandom(100000000000000000, 999999999999999999, new Random()),
                Destinations = { destinations },
                Contents = new ContentEnvelope
                {
                    MediaReferenceLists =
                    {
                        new MediaReferenceList
                        {
                            MediaReference =
                            {
                                mediaRef
                            }
                        }
                    },
                    ContentType = contentType,
                    Contents = contents.ToByteString(),
                    SavePolicy = SavePolicy.Lifetime
                }
            };
        }

        public async Task SaveMessage(string user, ulong currentVersion, ulong messageId)
        {
            HashSet<string> friendSet = [user];
            var convo = await SnapchatClient.GetConversationID(friendSet);

            var conversationUpdateRequest = new UpdateContentMessageRequest
            {
                ClientResolutionId = (ulong)m_Utilities.LongRandom(100000000000000000, 999999999999999999, new Random()),
                CurrentVersion = currentVersion,
                Update = new UpdateAction
                {
                    SenderId = new UUID
                    {
                        Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                    },
                    MessageId = messageId,
                    ConversationId = convo.FirstOrDefault().ConversationId,
                    Save = new Save
                    {
                    },
                    UpdateTimestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                }
            };

            await SnapchatGrpcClient.UpdateContentMessageAsync(conversationUpdateRequest);

        }

        public async Task SendReadNotification(string user, ulong currentVersion, ulong messageId, bool media)
        {
            HashSet<string> friendSet = [user];
            var convo = await SnapchatClient.GetConversationID(friendSet);

            if (media)
            {
                var updateContentMessageRequest = new UpdateContentMessageRequest
                {
                    ClientResolutionId = (ulong)m_Utilities.LongRandom(100000000000000000, 999999999999999999, new Random()),
                    CurrentVersion = currentVersion,
                    Update = new UpdateAction
                    {
                        SenderId = new UUID
                        {
                            Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                        },
                        MessageId = messageId,
                        ConversationId = convo.FirstOrDefault().ConversationId,
                        Read = new Read { },
                        UpdateTimestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    }
                };

                await SnapchatGrpcClient.UpdateContentMessageAsync(updateContentMessageRequest);
            }
            else
            {
                var conversationUpdateRequest = new ConversationUpdateRequest
                {
                    ConversationId = convo.FirstOrDefault().ConversationId,
                    ClientResolutionId = (ulong)m_Utilities.LongRandom(100000000000000000, 999999999999999999, new Random()),
                    CurrentVersion = currentVersion,
                    UpdateWatermark = new UpdateWatermark
                    {
                        InitiatingUserId = new UUID
                        {
                            Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                        },
                        Watermark = new HighWatermark
                        {
                            MessageId = messageId,
                            HighWatermarkType = HighWatermark.Types.HighWatermarkType.Read
                        }
                    }
                };

                await SnapchatGrpcClient.ConversationUpdateRequest(conversationUpdateRequest);
            }
        }
        public async Task SendTypingNotification(string user, ulong messageId)
        {
            HashSet<string> friendSet = [user];
            var convo = await SnapchatClient.GetConversationID(friendSet);

            var sendTypingNotificationRequest = new SendTypingNotificationRequest
            {
                SendingUserId = new UUID { Id = ByteString.CopyFromUtf8(await SnapchatClient.FindUserFromCache(user)) },
                ConversationId = convo.FirstOrDefault().ConversationId,
                MostRecentKnownMessageId = messageId
            };

            await SnapchatGrpcClient.SendTypingNotification(sendTypingNotificationRequest);
        }
        public async Task SendMention(string username_or_user_id, HashSet<string> users)
        {
            if (users.Count == 0)
            {
                throw new Exception("You Cannot Send mentions to 0 USERS LOL");
            }

            var destinations = await m_SnapchatGrpcClient.CreateDestinations(users);

            var friendUserId = username_or_user_id;

            if (!Guid.TryParse(username_or_user_id, out _))
                friendUserId = await SnapchatClient.FindUserFromCache(username_or_user_id);

            var contents = new SCMessagingContents
            {
                Share = new SCMessagingShare { User = new SCMessagingUserShare { UserId = new SCMessagingUUID { IdP = ByteString.CopyFrom(GuidUtils.ToBytes(friendUserId)) } } },
            };

            var createContentMessageRequest = CreateContentMessageRequest(destinations, contents, ContentType.Share);

            await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
        }

        public async Task<QueryMessagesResponse> QueryMessages(HashSet<string> user, uint messageId)
        {
            var convo = await SnapchatClient.GetConversationID(user);

            return await SnapchatGrpcClient.QueryMessages(new QueryMessagesRequest
            {
                ConversationId = convo.FirstOrDefault().ConversationId,
                SendingUserId = new UUID
                {
                    Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                },
                StartingMessageId = messageId,
                RequestedPageSize = 999999999,
            });

            throw new Exception("Failed to QueryMessages");
        }
        public async Task<string> DeleteMessages(string user)
        {
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

            foreach (var convo in friendsync.Conversations)
            {
                foreach (var participant in convo.Participants)
                {
                    if (participant.Id == ByteString.CopyFrom(GuidUtils.ToBytes(await SnapchatClient.FindUserFromCache(user))))
                    {
                        var messagesResponses = await SnapchatGrpcClient.DeltaSyncAsync(new DeltaSyncRequest
                        {
                            ConversationId = convo.ConversationInfo.ConversationId,
                            SendingUserId = new UUID
                            {
                                Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                            },
                            OtherParticipantUserId = participant
                        });

                        var ContentMessage = messagesResponses.Messages.ToHashSet();
                        foreach (var message in ContentMessage)
                        {
                            if (message.SenderId.Id == ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id)))
                            {
                                await SnapchatGrpcClient.UpdateContentMessageAsync(
                                    new UpdateContentMessageRequest
                                    {
                                        ClientResolutionId = message.ClientResolutionId,
                                        CurrentVersion = convo.ConversationInfo.ConversationVersion,
                                        Update = new UpdateAction
                                        {
                                            SenderId = message.SenderId,
                                            MessageId = message.MessageId,
                                            ConversationId = convo.ConversationInfo.ConversationId,
                                            Erase = new Erase { },
                                            UpdateTimestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                        }
                                    });
                            }
                        }
                    }
                }
            }

            return "Success";
        }

        public async Task SendLink(string link, HashSet<string> users)
        {
            if (users.Count == 0)
            {
                throw new Exception("You Cannot Send links to 0 USERS LOL");
            }

            var destinations = await m_SnapchatGrpcClient.CreateDestinations(users);

            var sCMessagingTextAttribute = new SCMessagingTextAttribute();

            sCMessagingTextAttribute.URLAttribute = new SCMessagingTextUrlAttribute { URL = link };
            var length = link.Length;
            sCMessagingTextAttribute.Range = new SCMessagingRange { Length = (uint)length };

            var contents = new SCMessagingContents
            {
                Text = new SCMessagingText { Text = link, AttributesArray = { sCMessagingTextAttribute } },
            };

            var createContentMessageRequest = CreateContentMessageRequest(destinations, contents, ContentType.Chat);

            await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
            sCMessagingTextAttribute.URLAttribute = new SCMessagingTextUrlAttribute { URL = link };
        }

        public async Task SendMessage(string message, HashSet<string> users)
        {
            if (users.Count == 0)
            {
                throw new Exception("You Cannot Send a message to 0 USERS LOL");
            }

            var destinations = await m_SnapchatGrpcClient.CreateDestinations(users);

            var contents = new SCMessagingContents
            {
                Text = new SCMessagingText { Text = message },
            };

            var createContentMessageRequest = CreateContentMessageRequest(destinations, contents, ContentType.Chat);

            await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
        }

        public async Task SendVoiceNote(string audiofile, HashSet<string> users)
        {
            if (users.Count == 0)
            {
                throw new Exception("You Cannot Send a message to 0 USERS LOL");
            }

            if (users.Count > 1)
            {
                throw new Exception("You Cannot Send a voice note to more then 1 user at a time");
            }

            var fileBytes = await File.ReadAllBytesAsync(audiofile);
            uint MediaDurationMs;

            using (Stream mp3Stream = new MemoryStream(fileBytes))
            {
                using (Mp3FileReader mp3Reader = new Mp3FileReader(mp3Stream))
                {
                    TimeSpan duration = mp3Reader.TotalTime;
                    MediaDurationMs = (uint)duration.TotalMilliseconds;
                }
            }

            m_Logger.Debug("Calling GetUploadUrls");
            var uploadUrls = await SnapchatClient.GetUploadUrls();

            if (uploadUrls == null)
            {
                throw new NoUploadUrlReceivedException();
            }

            m_Logger.Debug($"Upload URL is {uploadUrls.UploadURL}");

            var endpointInfo = new EndpointInfo() { BaseEndpoint = uploadUrls.UploadURL };
            m_Logger.Debug("Sending Put request to Upload URL");
            await SendPut(endpointInfo, await SnapchatClient.EncryptMedia(fileBytes));
            m_Logger.Debug("Put request went through ok");

            var destinations = await m_SnapchatGrpcClient.CreateDestinations(users);

            var contents = new SCMessagingContents
            {
                Note = new SCMessagingNote
                {
                    Audio = new SCMessagingAudioNote
                    {
                        Note = new SCMessagingMediaMetadata
                        {
                            EncryptionInfo = new SCMessagingMediaMetadata.Types.SCMessagingMediaMetadata_MediaEncryptionInfo
                            {
                                Key = ByteString.CopyFromUtf8(SnapchatClient.KEY),
                                Iv = ByteString.CopyFromUtf8(SnapchatClient.IV),
                            },
                            HasSound = true,
                            Type = SCMessagingMediaMetadata.Types.SCMessagingMediaMetadata_MediaType.Audio,
                            Dimensions = new SCMessagingMediaMetadata.Types.SCMessagingMediaMetadata_MediaDimensions { },
                            ContentObjectIndex = 0,
                            MediaDurationMs = MediaDurationMs,
                        },
                    },
                }
            };

            var createContentMessageRequest = CreateContentMessageRequestVoiceNote(destinations, contents, ContentType.Note, uploadUrls);

            await SnapchatGrpcClient.CreateContentMessageAsync(createContentMessageRequest);
        }
    }
}
