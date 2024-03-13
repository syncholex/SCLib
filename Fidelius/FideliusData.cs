using System;

namespace SnapchatLib.Crypto
{
    public class FideliusData
    {
        public FideliusData(byte[] iwekBytes, string iwek, byte[] publicKey, string publicKeyStr, byte[] privateKey, string privateKeyStr)
        {
            Iwek = iwek;
            OutBeta = publicKeyStr;
            HashedOutBeta = privateKeyStr;
            PrivateKey = Convert.ToBase64String(privateKey);
        }
        
        public string Iwek { get; }
        public string OutBeta { get; }
        public string HashedOutBeta { get; }
        public string PrivateKey { get; }
    }
}