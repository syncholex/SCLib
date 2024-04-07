using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using Google.Protobuf;
using SnapProto.Snap.Security;

namespace SnapchatLib.REST.Endpoints
{
    internal interface IGetTokens
    {
        Task<string> GetArgosTokenCached();
    }

    internal class GetTokensEndpoint : EndpointAccessor, IGetTokens
    {
        private static readonly EndpointInfo EndpointInfo = new() { Url = "/snap.security.ArgosService/GetTokens", Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapRouteTag | EndpointRequirements.XRequestUUID };
        private class CachedToken
        {
            public string Token { get; set; }
            public DateTime Expiry { get; set; }

            public bool IsValid() => DateTime.UtcNow < Expiry;
        }

        private Lazy<Task<CachedToken>> _tokenInit;
        private CachedToken _cachedToken;

        public GetTokensEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator)
            : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
            _tokenInit = new Lazy<Task<CachedToken>>(() => InitializeToken(), true);
        }

        private async Task<CachedToken> InitializeToken()
        {
            var sign = await SnapchatGrpcClient.Sign(EndpointInfo.Url);
            var req = new ArgosGetTokensRequest
            {
                AttestationToken = ByteString.CopyFrom(Convert.FromBase64String(sign.Attestation.Replace('-', '+').Replace('_', '/')))
            };

            var res = await SnapchatGrpcClient.GetTokensAsync(req);
            return new CachedToken
            {
                Token = Convert.ToBase64String(res.TokenB.Token.Span).Replace('+', '-').Replace('/', '_'),
                Expiry = DateTime.UtcNow.AddSeconds(res.TokenB.ExpirySeconds - 30)
            };
        }

        public async Task<string> GetArgosTokenCached()
        {
            _cachedToken = await _tokenInit.Value;

            if (!_cachedToken.IsValid())
            {
                _tokenInit = new Lazy<Task<CachedToken>>(() => InitializeToken(), true);
                _cachedToken = await _tokenInit.Value;
            }

            return _cachedToken.Token;
        }
    }
}