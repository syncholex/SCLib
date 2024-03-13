using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace SnapchatLib.Crypto
{
    internal static class SnapKeyDeriverUtils
    {
        public static byte[] DoHmac(byte[] key, byte[] data)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        public static byte[] DoHkdf(byte[] input, byte[] salt)
        {
            var generator = new HkdfBytesGenerator(new Sha256Digest());
            var output = new byte[80];
            
            generator.Init(new HkdfParameters(input, salt, new byte[0]));
            generator.GenerateBytes(output, 0, output.Length);

            return output;
        }
    }
}