using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SnapchatLib.Models.SignJson;

internal class SignResponseJson
{
    [JsonPropertyName("x-snapchat-att")]
    public string x_snapchat_att { get; set; }
}


internal class SigningRequestJson
{
    public PhoneInfo phone_info { get; set; }
    public SessionInfo session_info { get; set; }
    public PersistentInfo persistent_info { get; set; }
    public SnapchatInfo_ snapchat_info { get; set; }
    public SnapchatRequestInfo snapchat_request_info { get; set; }
}

internal class PhoneInfo
{
    public long androidId { get; set; }
    public int androidOsApi { get; set; }
    public string androidOsVersion { get; set; }
    public string phoneBuild { get; set; }
    public string phoneManufacture { get; set; }
    public string phoneModel { get; set; }
    public int phoneId { get; set; }
}
internal class SessionInfo
{
    public long sessionIdent { get; set; }
    public long timeBeginSession { get; set; }
    public int sequenceRequest { get; set; }
}

internal class PersistentInfo
{
    public string dtoken1i { get; set; }
    public long installTime { get; set; }
}

internal class SnapchatInfo_
{
    public string snapchat_version { get; set; }
}

public class PIA
{
    public string snapchat_version { get; set; }
    public string pi_nonce { get; set; }
    public string pi_type { get; set; }
}

internal class SnapchatRequestInfo
{
    public string path { get; set; }
    public string req_token { get; set; }
}


public class KeyAttestationRequest
{
    public string nonce { get; set; }

    public long androidId { get; set; }
}