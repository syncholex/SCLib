using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using SnapchatLib.Encryption;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Content;
using SnapProto.Snapchat.Context;
using SnapProto.Snapchat.Context2.Api;
using SnapProto.Snapchat.Messaging;
using SnapProto.Snapchat.Snapdoc;
using static SnapProto.Snapchat.Snapdoc.SDMAttachments.Types;

namespace SnapchatLib.REST.Endpoints;

public interface IPostStoryEnpoint
{
    Task PostStory(string inputfile, string swipeurl = null, List<string> mentioned = null);
}

internal class PostStoryEnpoint : EndpointAccessor, IPostStoryEnpoint
{
    public PostStoryEnpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }
    public async Task PostStory(string inputfile, string swipeupurl = null, List<string> mentioned = null)
    {

        var filebytes = File.ReadAllBytes(inputfile);

        m_Logger.Debug("Calling GetUploadUrls");
        var URL = await SnapchatClient.GetUploadUrls();

        if (URL == null) throw new NoUploadUrlReceivedException();

        m_Logger.Debug($"Upload URL is {URL.UploadURL}");

        // Generate the endpoint info so that our request goes through the proper configuration
        var endpointInfo = new EndpointInfo() { BaseEndpoint = URL.UploadURL };

        m_Logger.Debug("Sending Put request to Upload URL");
        await SendPut(endpointInfo, await SnapchatClient.EncryptMedia(filebytes));

        m_Logger.Debug("Put request went through ok");
        var type = inputfile.Contains(".mp4") || inputfile.Contains(".webm") || inputfile.Contains(".mov") || inputfile.Contains(".m4v") ? MediaReference.Types.SCCMediaType.Video : MediaReference.Types.SCCMediaType.Image;


        var mediaRef = new MediaReference
        {
            ContentObject = URL.ContentReference.ContentObject,
            MediaType = type,
            VideoDescription = new VideoDescription { VideoPlaybackType = VideoDescription.Types.VideoDescription_VideoPlaybackType.FaststartDisabled }
        };
        var destinations = new List<DeliveryDestination>
        {
            new DeliveryDestination { StoryDestination = new StoryDestination { StoryDestinationId = new UUID { Id = ByteString.CopyFrom(GuidUtils.ToBytes("01010101-0101-0101-0101-010101010101")) }, DestinationMetadata = new SnapProto.Ranking.Core.StoryPostDestinationMetadata { MyStory = new SnapProto.Ranking.Core.MyStoryDestinationMetadata {} } }}
        };
        var incidental_attachment = new List<IncidentalAttachment>
        {
            new IncidentalAttachment { StoryPostInfo = new StoryPostInfo {  StoryMetadata = new SnapProto.Ranking.Core.StoryMetadata {  ClientGeneratedToken  = Config.user_id.ToUpper() + "~" + Guid.NewGuid().ToString(), F12 = "en_US" } } }
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
                                    Width = 1080,
                                    Height = 2205,
                                },
                                Type = type == MediaReference.Types.SCCMediaType.Image ? SDMMediaMetadata.Types.SDMMediaMetadata_MediaType.Image : SDMMediaMetadata.Types.SDMMediaMetadata_MediaType.Video,
                                HasSound = type == MediaReference.Types.SCCMediaType.Video,
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
                        HasSound = type == MediaReference.Types.SCCMediaType.Video,
                        Infinite = new Empty()
                    }
                },
                Attachments = new SDMAttachments
                {
                    AttachmentsArray =
                        {
                            new SDMAttachments_Attachment
                            {
                                Context = new SDMContext
                                {
                                    ContextHint = new SCContextContextHint
                                    {
                                        UnencryptedClientInfo = new SCCTXContextClientInfo
                                        {

                                    }
                                }
                            }
                        }
                    }
                },
                Timing = new SDMTiming
                {
                    SnapCreateTimestampMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                },
                Provenance = new SDMProvenance { AppSource = SDMProvenance.Types.SDMProvenance_AppSource.AppSourceCamera }
            }
        };

        // Optional.
        if (swipeupurl != null)
        {
            contents.Snapdoc.Attachments.AttachmentsArray.Add(new SDMAttachments_Attachment
            {
                WebPage = new SDMWebPage
                {
                    URL = swipeupurl
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
                    HasAudio = type == MediaReference.Types.SCCMediaType.Video,
                },
                SavePolicy = ContentEnvelope.Types.SavePolicy.Prohibited,
                FeedDisplayInfo = new ContentEnvelope.Types.FeedDisplayInfo
                {
                    SnapDisplayInfo = new SnapDisplayInfo { }
                },
            },
            IncidentalAttachment = { incidental_attachment }
        };

        // Send. Timeout is set internally
        await SnapchatGrpcClient.CreateContentMessageAsync(message);
    }
}