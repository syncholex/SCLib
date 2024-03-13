using System;
using System.Text;
using SnapchatLib.Exceptions;

namespace SnapchatLib.Crypto
{
    internal class SnapKeyDeriverDigest
    {
        private readonly byte[] _mystique;
        private readonly string _senderId;
        private readonly string _recipientId;
        private readonly int _fideliusVersion;
        private readonly string _type;
        private readonly byte[] _na;

        public SnapKeyDeriverDigest(byte[] mystique, string senderId, string recipientId, int fideliusVersion, string type, byte[] na)
        {
            _mystique = mystique;
            _senderId = senderId;
            _recipientId = recipientId;
            _fideliusVersion = fideliusVersion;
            _type = type;
            _na = na;
        }

        public byte[] GetDigest()
        {
            try
            {
                var data = new StringBuilder();
                data.Append('~');
                data.Append(_senderId);
                data.Append('~');
                data.Append(_recipientId);
                data.Append('~');
                data.Append(_type);
                data.Append('~');
                data.Append(_fideliusVersion);
                var dataBytes = Encoding.UTF8.GetBytes(data.ToString());
                
                // Concat arrays.
                var input = new byte[_mystique.Length + dataBytes.Length];
                Buffer.BlockCopy(_mystique, 0, input, 0, _mystique.Length);
                Buffer.BlockCopy(dataBytes, 0, input, _mystique.Length, dataBytes.Length);
                
                // Generate digest.
                return SnapKeyDeriverUtils.DoHkdf(input, _na);
            }
            catch (Exception e)
            {
                throw new SnApiCryptoException("failure_derived_bytes", e);
            }
        }
    }
}