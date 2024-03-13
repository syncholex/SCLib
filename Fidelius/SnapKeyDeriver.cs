using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SnapchatLib.Exceptions;

namespace SnapchatLib.Crypto
{
    public class SnapKeyDeriver
    {
        private readonly string _privateKey;
        private readonly string _publicKey;
        
        private readonly string _senderId;
        private readonly string _senderBeta;
        
        private readonly string _recipientId;
        private readonly string _recipientBeta;
        
        private readonly int _fideliusVersion;
        private readonly string _type; // Examples: snap

        public SnapKeyDeriver(
            string recipientPrivateKey, 
            string senderId, 
            string senderBeta, 
            string recipientId, 
            string recipientBeta,
            int fideliusVersion,
            string type = "snap")
        {
            _privateKey = recipientPrivateKey;
            _publicKey = senderBeta;
            _senderId = senderId;
            _senderBeta = senderBeta;
            _recipientId = recipientId;
            _recipientBeta = recipientBeta;
            _fideliusVersion = fideliusVersion;
            _type = type;
        }

        private byte[] GenerateMystique()
        {
            return MystiqueUtils.Generate(
                Convert.FromBase64String(_privateKey), 
                Convert.FromBase64String(_publicKey));
        }
        
        public byte[] RetrieveKey(string na, string phi, string tag)
        {
            var mystique = GenerateMystique();
            
            if (string.IsNullOrEmpty(na) ||
                string.IsNullOrEmpty(phi) ||
                string.IsNullOrEmpty(tag) ||
                string.IsNullOrEmpty(_recipientId) ||
                string.IsNullOrEmpty(_recipientBeta))
            {
                throw new SnApiCryptoException("failure_null_validation");
            }
            
            // derived_bytes
            byte[] salt;

            try
            {
                salt = Convert.FromBase64String(na);
            }
            catch (Exception e)
            {
                throw new SnApiCryptoException("failure_derived_bytes", e);
            }
            
            var digestGen = new SnapKeyDeriverDigest(mystique, _senderId, _recipientId, _fideliusVersion, _type, salt);
            var digest = digestGen.GetDigest();
            
            var copyOfRange = new byte[32];
            var copyOfRange2 = new byte[16];
            var copyOfRange3 = new byte[32];
            
            Buffer.BlockCopy(digest, 0, copyOfRange, 0, copyOfRange.Length);
            Buffer.BlockCopy(digest, copyOfRange.Length, copyOfRange2, 0, copyOfRange2.Length);
            Buffer.BlockCopy(digest, copyOfRange.Length + copyOfRange2.Length, copyOfRange3, 0, copyOfRange3.Length);

            // snap_phi_bytes
            byte[] c3;

            try
            {
                c3 = Convert.FromBase64String(phi);
            }
            catch (Exception e)
            {
                throw new SnApiCryptoException("failure_snap_phi_bytes_null", e);
            }
            
            var sb = new StringBuilder();
            sb.Append('~');
            sb.Append(_senderId);
            sb.Append('~');
            sb.Append(_recipientId);
            sb.Append('~');
            sb.Append(na);
            sb.Append('~');
            sb.Append(_senderBeta);
            sb.Append('~');
            sb.Append(_recipientBeta);
            sb.Append('~');
            sb.Append(_fideliusVersion.ToString());
            
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var input = new byte[c3.Length + bytes.Length];
            Buffer.BlockCopy(c3, 0, input, 0, c3.Length);
            Buffer.BlockCopy(bytes, 0, input, c3.Length, bytes.Length);
            
            var b2 = SnapKeyDeriverUtils.DoHmac(copyOfRange3, input);
            var c4 = Convert.FromBase64String(tag);
            if (!b2.SequenceEqual(c4))
            {
                throw new SnApiCryptoException("failure_macs_mismatch");
            }
            
            // decrypt key
            using (var aes = new AesManaged())
            {
                aes.Key = copyOfRange;
                aes.IV = copyOfRange2;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encrypted = new MemoryStream(c3))
                using (var decrypted = new MemoryStream())
                using (var decryptor = aes.CreateDecryptor())
                using (var cryptoStream = new CryptoStream(encrypted, decryptor, CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(decrypted);
                    
                    return decrypted.ToArray();
                }
            }
        }
    }
}