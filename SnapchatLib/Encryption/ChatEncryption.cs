using System;
using System.Security.Cryptography;

namespace SnapchatLib
{
    public static class ChatEncryption
    {
        // Message decryption
        public static string DecryptClearTextEelKeyEncryption(string contents, string cek, string cekIv)
        {
            byte[] key = Convert.FromBase64String(cek);
            byte[] iv = Convert.FromBase64String(cekIv);
            byte[] content = Convert.FromBase64String(contents);

            using (AesGcm aesGcm = new AesGcm(key, 16))
            {
                int tagLength = 16;
                int ciphertextLength = content.Length - tagLength;

                byte[] ciphertext = new byte[ciphertextLength];
                byte[] receivedTag = new byte[tagLength];

                Array.Copy(content, 0, ciphertext, 0, ciphertextLength);
                Array.Copy(content, ciphertextLength, receivedTag, 0, tagLength);

                aesGcm.Decrypt(iv, ciphertext, receivedTag, ciphertext);
                return DecryptedMessage.Parser.ParseFrom(ciphertext).Message.MessageContent;
            }
        }
    }
}
