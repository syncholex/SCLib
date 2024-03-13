using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace SnapchatLib.Crypto
{
    public static class FideliusUtils
    {
        private static readonly RNGCryptoServiceProvider Generator = new RNGCryptoServiceProvider();
        
        public static FideliusData CreateData()
        {
            var data = new byte[32];
            
            Generator.GetNonZeroBytes(data);
            
            // Create secp256r1 keypair.
            var curve = SecNamedCurves.GetByName("secp256r1");
            var keyGenerator = new ECKeyPairGenerator();
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            
            keyGenerator.Init(new ECKeyGenerationParameters(domainParams, new SecureRandom()));
            
            var keyPair = keyGenerator.GenerateKeyPair();
            
            // Create correct asn1 formatted keys.
            var keyPublic = GetPublicKey((ECPublicKeyParameters) keyPair.Public);
            var keyPrivate = GetPrivateKey((ECPrivateKeyParameters) keyPair.Private);
            
            return DoStuff(data, keyPublic, keyPrivate);
        }

        private static byte[] GetPublicKey(AsymmetricKeyParameter publicKey)
        {
            var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            
            using (var memory = new MemoryStream())
            using (var asn1 = new Asn1OutputStream(memory))
            {
                asn1.WriteObject(new DerSequence(new Asn1EncodableVector
                {
                    new DerSequence(new Asn1EncodableVector
                    {
                        X9ObjectIdentifiers.IdECPublicKey, 
                        X9ObjectIdentifiers.Prime256v1
                    }),
                    publicKeyInfo.PublicKeyData
                }));
                
                asn1.Flush();

                return memory.ToArray();
            }
        }

        private static byte[] GetPrivateKey(ECPrivateKeyParameters privateKey)
        {
            using (var memory = new MemoryStream())
            using (var asn1 = new Asn1OutputStream(memory))
            {
                asn1.WriteObject(new DerSequence(new Asn1EncodableVector
                {
                    new DerInteger(0),
                    new DerSequence(new Asn1EncodableVector
                    {
                        X9ObjectIdentifiers.IdECPublicKey, 
                        X9ObjectIdentifiers.Prime256v1
                    }),
                    new DerOctetString(new DerSequence(new Asn1EncodableVector
                    {
                        new DerInteger(1),
                        new DerOctetString(BigIntegers.AsUnsignedByteArray((privateKey.D.BitLength + 7) / 8, privateKey.D))
                    }))
                }));
                
                asn1.Flush();

                return memory.ToArray();
            }
        }

        public static BigInteger UnwrapPrivateKey(byte[] data)
        {
            using (var memory = new MemoryStream(data))
            using (var asn1 = new Asn1InputStream(memory))
            {
                var sequence = (DerSequence) asn1.ReadObject();
                var octetString = (DerOctetString) sequence[2];

                using (var asn1InsideOctet = octetString.Parser.GetOctetStream())
                using (var asn1Inside = new Asn1InputStream(asn1InsideOctet))
                {
                    var sequenceTwo = (DerSequence) asn1Inside.ReadObject();
                    var octetStringTwo = (DerOctetString) sequenceTwo[1];

                    using (var stream = octetStringTwo.Parser.GetOctetStream())
                    {
                        var fullData = new byte[stream.Length];
                        var read = stream.Read(fullData, 0, (int) stream.Length);
                        if (read != stream.Length)
                        {
                            throw new Exception("Read invalid amount.");
                        }
                        
                        return new BigInteger(1, fullData);
                    }
                }
            }
        }

        private static byte[] DoHmac(byte[] key, byte[] data)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }
        
        private static FideliusData DoStuff(byte[] iwekBytes, byte[] publicKey, byte[] privateKey)
        {
            var iwekStr = Convert.ToBase64String(iwekBytes);
            var publicKeyStr = Convert.ToBase64String(publicKey);
            var privateKeyStr = Convert.ToBase64String(DoHmac(iwekBytes, publicKey));

            return new FideliusData(iwekBytes, iwekStr, publicKey, publicKeyStr, privateKey, privateKeyStr);
        }
    }
}