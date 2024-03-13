using System.Collections.Generic;
using Newtonsoft.Json;

namespace SnapchatLib.Crypto
{
    public class FideliusClientInit
    {
        public FideliusClientInit(FideliusData fidelius)
        {
            NewOutBeta = fidelius.OutBeta;
            NewHashedOutBeta = fidelius.HashedOutBeta;
            NewIwek = fidelius.Iwek;
            NewFideliusVersion = 9;
        }

        [JsonProperty("hashed_out_betas", Required = Required.Always)]
        public List<string> HashedOutBetas { get; set; } = new List<string>();
        
        [JsonProperty("new_out_beta")]
        public string NewOutBeta { get; set; }
        
        [JsonProperty("new_hashed_out_beta")]
        public string NewHashedOutBeta { get; set; }
        
        [JsonProperty("new_iwek")]
        public string NewIwek { get; set; }
        
        [JsonProperty("new_fidelius_version")]
        public int NewFideliusVersion { get; set; }
    }
}