using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Janus.Crypto.Fidelius;

internal record FideliusDevice(byte[] Iwek, byte[] Public, byte[] PublicHash, byte[] Private, byte[][] HashedPublicKeysArray)
{
    public byte[] PublicUnwrapped => FideliusCrypto.UnwrapPublicKey(Public);

    [JsonIgnore]
    public string PublicBase64 => Convert.ToBase64String(Public);

    /// <summary>
    ///     Used for RecipientKey in messages.
    /// </summary>
    [JsonIgnore]
    public string PublicUnwrappedBase64 => Convert.ToBase64String(PublicUnwrapped);

    [JsonIgnore]
    public string PrivateBase64 => Convert.ToBase64String(Private);

    public static FideliusDevice Create(int numberOfAdditionalKeys)
    {
        var data = new byte[32];
        RandomNumberGenerator.Fill(data);

        // Create secp256r1 keypair for the device.
        var curve = SecNamedCurves.GetByName("secp256r1");
        var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
        var keyGenerator = new ECKeyPairGenerator();
        keyGenerator.Init(new ECKeyGenerationParameters(domainParams, new SecureRandom()));

        var keyPair = keyGenerator.GenerateKeyPair();
        var keyPublic = FideliusCrypto.GetPublicKey((ECPublicKeyParameters)keyPair.Public);
        var keyPrivate = FideliusCrypto.GetPrivateKey((ECPrivateKeyParameters)keyPair.Private);

        // Generate additional public keys and hash them with the IWEK.
        var hashedPublicKeysArray = new byte[numberOfAdditionalKeys][];
        for (int i = 0; i < numberOfAdditionalKeys; i++)
        {
            var additionalKeyPair = keyGenerator.GenerateKeyPair();
            var additionalPublicKey = FideliusCrypto.GetPublicKey((ECPublicKeyParameters)additionalKeyPair.Public);
            hashedPublicKeysArray[i] = HMACSHA256.HashData(data, additionalPublicKey);
        }

        return new FideliusDevice(
            data,
            keyPublic,
            HMACSHA256.HashData(data, keyPublic),
            keyPrivate,
            hashedPublicKeysArray
        );
    }
}