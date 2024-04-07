using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Bitmoji.Api;

namespace SnapchatLib.REST.Endpoints;

public interface ICreateAvatarDataEndpoint
{
    Task CreateBitmoji(bool male, int style, int body, int bottom, int bottom_tone1, int bottom_tone10, int bottom_tone2, int bottom_tone3, int bottom_tone4, int bottom_tone5, int bottom_tone6, int bottom_tone7, int bottom_tone8, int bottom_tone9, int brow, int clothing_type, int ear, int eyelash, int face_proportion, int footwear, int footwear_tone1, int footwear_tone10, int footwear_tone2, int footwear_tone3, int footwear_tone4, int footwear_tone5, int footwear_tone6, int footwear_tone7, int footwear_tone8, int footwear_tone9, int hair, int hair_tone, int is_tucked, int jaw, int mouth, int nose, int pupil, int pupil_tone, int skin_tone, int sock, int sock_tone1, int sock_tone2, int sock_tone3, int sock_tone4, int top, int top_tone1, int top_tone10, int top_tone2, int top_tone3, int top_tone4, int top_tone5, int top_tone6, int top_tone7, int top_tone8, int top_tone9);
}

internal class CreateAvatarDataEndpoint : EndpointAccessor, ICreateAvatarDataEndpoint
{
    public static readonly EndpointInfo CreateAvatarData_Endpoint = new()
    {
        Url = "/bitmoji-api/avatar-service/create-avatar-data",
        BaseEndpoint = RequestConfigurator.ApiAWSEast1Endpoint,
        Requirements = EndpointRequirements.AcceptEncoding | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.OSUserAgent | EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID
    };
    public CreateAvatarDataEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    /*
     * 
     * Needs to be moved to GRPC
     * 
     */

    public async Task CreateBitmoji(bool male, int style, int body, int bottom, int bottom_tone1, int bottom_tone10, int bottom_tone2, int bottom_tone3, int bottom_tone4, int bottom_tone5, int bottom_tone6, int bottom_tone7, int bottom_tone8, int bottom_tone9, int brow, int clothing_type, int ear, int eyelash, int face_proportion, int footwear, int footwear_tone1, int footwear_tone10, int footwear_tone2, int footwear_tone3, int footwear_tone4, int footwear_tone5, int footwear_tone6, int footwear_tone7, int footwear_tone8, int footwear_tone9, int hair, int hair_tone, int is_tucked, int jaw, int mouth, int nose, int pupil, int pupil_tone, int skin_tone, int sock, int sock_tone1, int sock_tone2, int sock_tone3, int sock_tone4, int top, int top_tone1, int top_tone10, int top_tone2, int top_tone3, int top_tone4, int top_tone5, int top_tone6, int top_tone7, int top_tone8, int top_tone9)
    {
        int gender;
        if (male)
        {
            gender = 1;
        }
        else
        {
            gender = 2;
        }
        AvatarData avatarData = new AvatarData();
        avatarData.Gender = gender;
        avatarData.Style = style;
        avatarData.OptionIds.Add("body", body);
        avatarData.OptionIds.Add("bottom", bottom);
        avatarData.OptionIds.Add("bottom_tone1", bottom_tone1);
        avatarData.OptionIds.Add("bottom_tone10", bottom_tone10);
        avatarData.OptionIds.Add("bottom_tone2", bottom_tone2);
        avatarData.OptionIds.Add("bottom_tone3", bottom_tone3);
        avatarData.OptionIds.Add("bottom_tone4", bottom_tone4);
        avatarData.OptionIds.Add("bottom_tone5", bottom_tone5);
        avatarData.OptionIds.Add("bottom_tone6", bottom_tone6);
        avatarData.OptionIds.Add("bottom_tone7", bottom_tone7);
        avatarData.OptionIds.Add("bottom_tone8", bottom_tone8);
        avatarData.OptionIds.Add("bottom_tone9", bottom_tone9);
        avatarData.OptionIds.Add("brow", brow);
        avatarData.OptionIds.Add("clothing_type", clothing_type);
        avatarData.OptionIds.Add("ear", ear);
        avatarData.OptionIds.Add("eyelash", eyelash);
        avatarData.OptionIds.Add("face_proportion", face_proportion);
        avatarData.OptionIds.Add("footwear", footwear);
        avatarData.OptionIds.Add("footwear_tone1", footwear_tone1);
        avatarData.OptionIds.Add("footwear_tone10", footwear_tone10);
        avatarData.OptionIds.Add("footwear_tone2", footwear_tone2);
        avatarData.OptionIds.Add("footwear_tone3", footwear_tone3);
        avatarData.OptionIds.Add("footwear_tone4", footwear_tone4);
        avatarData.OptionIds.Add("footwear_tone5", footwear_tone5);
        avatarData.OptionIds.Add("footwear_tone6", footwear_tone6);
        avatarData.OptionIds.Add("footwear_tone7", footwear_tone7);
        avatarData.OptionIds.Add("footwear_tone8", footwear_tone8);
        avatarData.OptionIds.Add("footwear_tone9", footwear_tone9);
        avatarData.OptionIds.Add("hair", hair);
        avatarData.OptionIds.Add("hair_tone", hair_tone);
        avatarData.OptionIds.Add("is_tucked", is_tucked);
        avatarData.OptionIds.Add("jaw", jaw);
        avatarData.OptionIds.Add("mouth", mouth);
        avatarData.OptionIds.Add("nose", nose);
        avatarData.OptionIds.Add("pupil_tone", pupil_tone);
        avatarData.OptionIds.Add("skin_tone", skin_tone);
        avatarData.OptionIds.Add("sock", sock);
        avatarData.OptionIds.Add("sock_tone1", sock_tone1);
        avatarData.OptionIds.Add("sock_tone2", sock_tone2);
        avatarData.OptionIds.Add("sock_tone3", sock_tone3);
        avatarData.OptionIds.Add("sock_tone4", sock_tone4);
        avatarData.OptionIds.Add("top", top);
        avatarData.OptionIds.Add("top_tone1", top_tone1);
        avatarData.OptionIds.Add("top_tone10", top_tone10);
        avatarData.OptionIds.Add("top_tone2", top_tone2);
        avatarData.OptionIds.Add("top_tone3", top_tone3);
        avatarData.OptionIds.Add("top_tone4", top_tone4);
        avatarData.OptionIds.Add("top_tone5", top_tone5);
        avatarData.OptionIds.Add("top_tone6", top_tone6);
        avatarData.OptionIds.Add("top_tonep7", top_tone7);
        avatarData.OptionIds.Add("top_tone8", top_tone8);
        avatarData.OptionIds.Add("top_tone9", top_tone9);
        var CreateAvatarDataRequest_pb2 = new CreateAvatarDataRequest
        {
            TouVersion = -1,
            AvatarData = avatarData,
        };

        // send data via HTTP
        using var ByteArrayContent = new ByteArrayContent(CreateAvatarDataRequest_pb2.ToByteArray());
        ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");

        await Send(CreateAvatarData_Endpoint, ByteArrayContent);
    }
}