using System;
using System.IO;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace SnapchatLib.Encryption;

public static class Aes
{
    public static byte[] DecryptMedia(byte[] input, string key, string iv)
    {
        var _key = Convert.FromBase64String(key);
        var _iv = Convert.FromBase64String(iv);

        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = _key;
        return aes.DecryptCbc(input, _iv);
    }
    internal static async Task<Stream> EncryptMedia(this SnapchatClient client, byte[] data) //byte[] key, byte[] iv
    {
        // AES algorthim with CBC cipher & PKCS5 padding...
        var engine = new AesEngine();
        var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine), new Pkcs7Padding());

        var randomkey = new SecureRandom();
        var keyBytes = new byte[32]; //key = 32 Bytes = 256 Bits || iv = 16 bytes = 128 bits
        randomkey.NextBytes(keyBytes);
        var kgpkey = new KeyGenerationParameters(randomkey, 256);

        var randomiv = new SecureRandom();
        var ivBytes = new byte[16]; //key = 32 Bytes = 256 Bits || iv = 16 bytes = 128 bits
        randomiv.NextBytes(ivBytes);
        var kgpiv = new KeyGenerationParameters(randomiv, 128);

        var kgkey = new CipherKeyGenerator();
        kgkey.Init(kgpkey);

        var kgiv = new CipherKeyGenerator();
        kgiv.Init(kgpiv);


        var key = kgkey.GenerateKey();
        var iv = kgiv.GenerateKey();

        client.IV = Convert.ToBase64String(iv);
        client.KEY = Convert.ToBase64String(key);

        var keyParam = new KeyParameter(key);

        var keyParamWithIv = new ParametersWithIV(keyParam, iv);

        cipher.Init(true, keyParamWithIv);

        // Encrypt the data and write the 'final' byte stream...
        var encryptionBytes = cipher.ProcessBytes(data);
        var encryptedFinal = cipher.DoFinal();

        cipher.Reset();
        // Write the encrypt bytes & final to memory...
        var enryptedStream = new MemoryStream(encryptionBytes.Length);
        await enryptedStream.WriteAsync(encryptionBytes, 0, encryptionBytes.Length);
        await enryptedStream.WriteAsync(encryptedFinal, 0, encryptedFinal.Length);
        enryptedStream.Seek(0, SeekOrigin.Begin);
        return enryptedStream;
    }
}