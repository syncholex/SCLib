using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapProto.Com.Snapchat.Auth.Proto;
using SnapProto.Com.Snapchat.Proto.Snaptoken;

namespace SnapchatLib.Exceptions
{
    public class NoAccessTokensReceivedException : Exception
    {
        public NoAccessTokensReceivedException() : base("The response contained 0 access token")
        {
        }
    }

    public class NoMatchingAccessTokenException : Exception
    {
        public NoMatchingAccessTokenException() : base("A matching access token could not be found")
        {
        }
    }

    public class EmptyAccessTokenException : Exception
    {
        public EmptyAccessTokenException() : base("An empty access token has been received")
        {
        }
    }

    public class ReloginException : Exception
    {
        public ReloginException() : base("Please Relogin")
        {
        }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    internal interface IAccessTokenEndpoint
    {
        Task GetAccessTokens();
        Task Validate();
        Task RenewAccessTokens();
    }

    internal class AccessTokenEndpoint : EndpointAccessor, IAccessTokenEndpoint
    {
        internal static readonly EndpointInfo EndpointInfo = new()
        {
            BaseEndpoint = RequestConfigurator.ApiGCPEndpoint,
            Url = "/snap_token/pb/snap_session",
            Requirements = EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID
        };

        internal static readonly EndpointInfo EndpointInfo2 = new()
        {
            BaseEndpoint = RequestConfigurator.ApiGCPEndpoint,
            Url = "/scauth/validate",
            Requirements = EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID
        };

        internal static readonly EndpointInfo EndpointInfo3 = new()
        {
            BaseEndpoint = RequestConfigurator.ApiGCPEndpoint,
            Url = "/snap_token/pb/snap_access_tokens",
            Requirements = EndpointRequirements.AcceptProtoBuf | EndpointRequirements.XSnapchatUUID | EndpointRequirements.XRequestUUID
        };

        public AccessTokenEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }

        public async Task GetAccessTokens()
        {
            m_Logger.Debug("Starting GetAccessToken configuration");

            var scopes = new List<SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope>
            {
                SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope.BusinessAccounts,
                SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope.Blizzard,
                SCPBSnaptokenSnapSessionRequest.Types.SCPBSnaptokenSnapTokenScope.ApiGateway
            };

            var sessionRequest = new SCPBSnaptokenSnapSessionRequest
            {
                ScopesAsEnumsArray = { scopes },
                DeviceId = Config.dtoken1i
            };

            // Send data via HTTP
            using var byteArrayContent = new ByteArrayContent(sessionRequest.ToByteArray());
            byteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
            var response = await Send(EndpointInfo, byteArrayContent);
            var result = SCPBSnaptokenSnapSessionResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());

            m_Logger.Debug("Processing tokens from response");
            if (result.SnapAccessTokensArray.Count == 0)
            {
                throw new EmptyAccessTokenException();
            }

            var apiGatewayToken = result.SnapAccessTokensArray.FirstOrDefault(i => i.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway");
            if (apiGatewayToken == null)
            {
                throw new NoMatchingAccessTokenException();
            }

            var businessAccountsToken = result.SnapAccessTokensArray.FirstOrDefault(i => i.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts");
            if (businessAccountsToken == null)
            {
                throw new NoMatchingAccessTokenException();
            }

            Config.Access_Token = apiGatewayToken.AccessToken;
            Config.BusinessAccessToken = businessAccountsToken.AccessToken;
        }

        public async Task Validate()
        {
            m_Logger.Debug("Starting GetAccessToken configuration");

            var _Session = new SCAuthUserSessionValidationRequest
            {
                RefreshToken = Config.refreshToken
            };
            // send data via HTTP
            using var ByteArrayContent = new ByteArrayContent(_Session.ToByteArray());
            ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
            var response = await Send(EndpointInfo2, ByteArrayContent);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Please Relogin");

        }

        public async Task RenewAccessTokens()
        {
            m_Logger.Debug("Starting GetAccessToken configuration");

            var scopes = new List<SCPBSnaptokenSnapAccessTokensRequest.Types.SCPBSnaptokenSnapTokenScope>
            {
                SCPBSnaptokenSnapAccessTokensRequest.Types.SCPBSnaptokenSnapTokenScope.BusinessAccounts,
                SCPBSnaptokenSnapAccessTokensRequest.Types.SCPBSnaptokenSnapTokenScope.Blizzard,
                SCPBSnaptokenSnapAccessTokensRequest.Types.SCPBSnaptokenSnapTokenScope.ApiGateway
            };

            var _Session = new SCPBSnaptokenSnapAccessTokensRequest
            {
                RefreshToken = Config.refreshToken,
                DeviceId = Config.DeviceID,
                ScopesAsEnumsArray = { scopes },

            };
            // send data via HTTP
            using var ByteArrayContent = new ByteArrayContent(_Session.ToByteArray());
            ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
            var response = await Send(EndpointInfo3, ByteArrayContent);
            var result = SCPBSnaptokenSnapSessionResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());
            m_Logger.Debug("Processing tokens from response");
            if (result.SnapAccessTokensArray.Count == 0)
            {
                throw new EmptyAccessTokenException();
            }

            var apiGatewayToken = result.SnapAccessTokensArray.FirstOrDefault(i => i.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway");
            if (apiGatewayToken == null)
            {
                throw new NoMatchingAccessTokenException();
            }

            var businessAccountsToken = result.SnapAccessTokensArray.FirstOrDefault(i => i.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts");
            if (businessAccountsToken == null)
            {
                throw new NoMatchingAccessTokenException();
            }

            Config.Access_Token = apiGatewayToken.AccessToken;
            Config.BusinessAccessToken = businessAccountsToken.AccessToken;
            if (!response.IsSuccessStatusCode)
                throw new ReloginException();

        }
    }
}