using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SnapchatLib.Encryption;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Snapdoc;
using SnapProto.Snapchat.Messaging;
using SnapProto.Snapchat.Content;
using static SnapProto.Snapchat.Snapdoc.SDMAttachments.Types;

namespace SnapchatLib.Exceptions
{
    public class NoUploadUrlReceivedException : ContactJustinException
    {
        public NoUploadUrlReceivedException() : base("Received an empty UploadURL")
        {
        }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    internal interface IDirectSnapEndpoint
    {
        Task PostDirect(string inputfile, HashSet<string> users, string swipeupurl = null, HashSet<string> mentioned = null);
    }

    internal class DirectSnapEndpoint : EndpointAccessor, IDirectSnapEndpoint
    {
        public DirectSnapEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        private MediaReference.Types.SCCMediaType GetMediaType(string filePath)
        {
            if (filePath.Contains(".mp4") || filePath.Contains(".webm") || filePath.Contains(".mov") || filePath.Contains(".m4v"))
            {
                return MediaReference.Types.SCCMediaType.Video;
            }
            else
            {
                return MediaReference.Types.SCCMediaType.Image;
            }
        }

        public async Task PostDirect(string inputfile, HashSet<string> users, string swipeupurl = null, HashSet<string> mentioned = null)
        {

            if(users.Count == 0)
            {
                throw new Exception("You Cannot Post to 0 USERS LOL");
            }

            var fileBytes = await File.ReadAllBytesAsync(inputfile);

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

            var mediaType = GetMediaType(inputfile);

            var destinations = await SnapchatGrpcClient.CreateDestinations(users);

            var mediaRef = new MediaReference
            {
                ContentObject = uploadUrls.ContentReference.ContentObject,
                MediaType = mediaType
            };

 
            var contents = new SCMessagingContents
            {
                Snapdoc = new SDMSnapDoc
                {
                    Playback = new SDMPlayback
                    {
                        PlaybackLayersArray =
                        {
                            new SDMPlaybackLayer
                            {
                                Media = new SDMMediaMetadata
                                {
                                    EncryptionInfoV1 = new SDMMediaMetadata.Types.SDMMediaMetadata_MediaEncryptionInfo
                                    {
                                        Key = ByteString.CopyFromUtf8(SnapchatClient.KEY),
                                        Iv = ByteString.CopyFromUtf8(SnapchatClient.IV),
                                    },
                                    Dimensions = new SDMMediaMetadata.Types.SDMMediaMetadata_MediaDimensions
                                    {
                                        // TODO: Proper values
                                        Width = 1080,
                                        Height = 1920,
                                    },
                                    Type = mediaType == MediaReference.Types.SCCMediaType.Image ? SDMMediaMetadata.Types.SDMMediaMetadata_MediaType.Image : SDMMediaMetadata.Types.SDMMediaMetadata_MediaType.Video,
                                    HasSound = mediaType == MediaReference.Types.SCCMediaType.Video,
                                    MediaDurationMs = 10000,
                                    MediaId = new SDMMediaId
                                    {
                                    },
                                    EncryptionInfoV2 = new SDMMediaMetadata.Types.SDMMediaMetadata_MediaEncryptionInfo
                                    {
                                        Key = ByteString.FromBase64(SnapchatClient.KEY),
                                        Iv = ByteString.FromBase64(SnapchatClient.IV),
                                    }
                                }
                            }
                        },
                        PlaybackCharacteristics = new SDMPlaybackCharacteristics
                        {
                            HasSound = mediaType == MediaReference.Types.SCCMediaType.Video,
                            Infinite = new Empty()
                        }
                    },
                    Attachments = new SDMAttachments
                    {

                    },
                    Timing = new SDMTiming
                    {
                        SnapCreateTimestampMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }
                }
            };

            // Optional.
            if (!string.IsNullOrEmpty(swipeupurl))
            {
                contents.Snapdoc.Attachments.AttachmentsArray.Add(new SDMAttachments_Attachment
                {
                    WebPage = new SDMWebPage
                    {
                        URL = swipeupurl,
                    }
                });
            }

            // - Create message
            var message = new CreateContentMessageRequest
            {
                SenderId = new UUID
                {
                    Id = ByteString.CopyFrom(GuidUtils.ToBytes(Config.user_id))
                },
                ClientResolutionId = (ulong)m_Utilities.LongRandom(100000000000000000, 999999999999999999, new Random()),
                Destinations = { destinations },
                Contents = new ContentEnvelope
                {
                    Contents = contents.ToByteString(),
                    MediaReferenceLists =
                    {
                        new ContentEnvelope.Types.MediaReferenceList
                        {
                            MediaReference =
                            {
                                mediaRef
                            }
                        }
                    },
                    DisplayInfo = new ContentEnvelope.Types.DisplayInfo
                    {
                        HasAudio = mediaType == MediaReference.Types.SCCMediaType.Video,
                    },
                    SavePolicy = ContentEnvelope.Types.SavePolicy.ViewSession
                }
            };

            // Send. Timeout is set internally
            await SnapchatGrpcClient.CreateContentMessageAsync(message);
        }
    }
}