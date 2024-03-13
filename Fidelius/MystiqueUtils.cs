using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace SnapchatLib.Crypto
{
    public static class MystiqueUtils
    {
        public static byte[] Generate(byte[] privateKey, byte[] publicKey)
        {
            // Setup parameters.
            var curve = SecNamedCurves.GetByName("secp256r1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            
            // Parse keys.
            var privateKeyNum = FideliusUtils.UnwrapPrivateKey(privateKey);
            var privateKeyParams = new ECPrivateKeyParameters(privateKeyNum, domainParams);
            var publicKeyParams = PublicKeyFactory.CreateKey(publicKey);
            
            // Do key agreement.
            var instance = new ECDHBasicAgreement();
            instance.Init(privateKeyParams);
            
            return instance.CalculateAgreement(publicKeyParams).ToByteArrayUnsigned();
        }
    }
}