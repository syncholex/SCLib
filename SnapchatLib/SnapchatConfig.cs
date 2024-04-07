using System;
using System.Collections.Generic;
using System.Net;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Search;

namespace SnapchatLib;
public class SnapchatConfig
{
    private string _ApiKey;
    private string _User_ID;
    private string _device_id;
    private string _ClientID;
    private string _OldUsername;
    private string _Username;
    private string _dtoken1i;
    private string _BusinessAccessToken;
    private string _refreshToken;
    private string _Access_Token;
    private string _AccountCountryCode;
    private string _IPAPIPROKEY;
    public WebProxy Proxy { get; set; }
    public static bool IsBase64String(string base64)
    {
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
    }
    public string ApiKey
    {
        get
        {
            if (string.IsNullOrEmpty(_ApiKey))
                throw new InvalidOperationException("ApiKey Cannot be empty");

            return _ApiKey;
        }
        set
        {
            _ApiKey = value;
            if (string.IsNullOrEmpty(_ApiKey)) throw new ArgumentNullException("ApiKey Cannot be empty");
        }
    }
    public string OldUsername
    {
        get => _OldUsername;
        set
        {
            _OldUsername = value;
            if (!string.IsNullOrEmpty(_OldUsername))
            {
                if (_OldUsername == Username) throw new Exception("Invalid OldUsername");
            }
        }
    }
    public string Username
    {
        get => _Username;
        set
        {
            _Username = value;
            if (!string.IsNullOrEmpty(_Username))
            {
                if (_Username == OldUsername) throw new Exception("Invalid Username");
            }
        }
    }
    public string dtoken1i
    {
        get => _dtoken1i;
        set
        {
            _dtoken1i = value;
            if (!string.IsNullOrEmpty(_dtoken1i))
            {
                if (!_dtoken1i.Contains("00001:")) throw new Exception("Invalid dtoken1i");
            }
        }
    }

    public string ClientID
    {
        get => _ClientID;
        set
        {
            _ClientID = value;
            if (!string.IsNullOrEmpty(_ClientID))
            {
                if (!Guid.TryParse(_ClientID, out _)) throw new Exception("Invalid client_id");
            }
        }
    }

    public string DeviceID
    {
        get => _device_id;
        set
        {
            _device_id = value;
            if (!string.IsNullOrEmpty(_device_id))
            {
                if (!Guid.TryParse(_device_id, out _)) throw new Exception("Invalid device_id");
            }
        }
    }

    public string dtoken1v { get; set; }
    public string user_id
    {
        get => _User_ID;
        set
        {
            _User_ID = value;
            if (!string.IsNullOrEmpty(_User_ID))
            {
                if (!Guid.TryParse(_User_ID, out _)) throw new Exception("Invalid user_id");
            }
        }
    }

    public string BusinessAccessToken
    {
        get => _BusinessAccessToken;
        set
        {
            _BusinessAccessToken = value;
            if (!string.IsNullOrEmpty(_BusinessAccessToken))
            {
                if(!_BusinessAccessToken.StartsWith("ey")) throw new Exception("Invalid BusinessAccessToken");
            }
        }
    }

    public string refreshToken
    {
        get => _refreshToken;
        set
        {
            _refreshToken = value;
            if (!string.IsNullOrEmpty(_refreshToken))
            {
                if (!_refreshToken.StartsWith("ey")) throw new Exception("Invalid refreshToken");
            }
        }
    }

    public string Access_Token
    {
        get => _Access_Token;
        set
        {
            _Access_Token = value;
            if (!string.IsNullOrEmpty(_Access_Token))
            {
                if (!_Access_Token.StartsWith("gE") && !_Access_Token.StartsWith("hC")) throw new Exception("Invalid Access_Token");
            }
        }
    }

    public string AccountCountryCode
    {
        get => _AccountCountryCode;
        set
        {
            _AccountCountryCode = value;
            if (!string.IsNullOrEmpty(_AccountCountryCode))
            {
                if (_AccountCountryCode.Length != 2) throw new Exception("Invalid AccountCountryCode");
            }
        }
    }

    public string IPAPIPROKEY
    {
        get
        {
            if (string.IsNullOrEmpty(_IPAPIPROKEY))
                throw new InvalidOperationException("IPAPIPROKEY is not set to a valid value.");

            return _IPAPIPROKEY;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(IPAPIPROKEY), "IPAPIPROKEY cannot be null or empty.");

            _IPAPIPROKEY = value;
        }
    }
    public long install_time { get; set; }
    public SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign Horoscope { get; set; }
    public bool Debug { get; set; } = false;
    public bool ProxySigner { get; set; } = false;
    public string TimeZone { get; set; }
    public bool BandwithSaver { get; set; } = true;
    public bool TLSSpoofing { get; set; } = false;
    public int Age { get; set; }
    public int PhoneId { get; set; }
    public long AccessTokenExpirey { get; set; }
    public string androidOsVersion { get; set; }
    public int androidOsSDK { get; set; }
    public string androidIncrementalVersion { get; set; }
    public string phoneManufacture { get; set; }
    public bool IPAbuseCheck { get; set; }
    public bool IPReuseCheck { get; set; }
    public string phoneModel { get; set; }
    public string androidId { get; set; }
    public string DebugInfo { get; set; }
    public int Timeout { get; set; } = 5;
    public SnapchatVersion SnapchatVersion { get; set; } = SnapchatVersion.V12_76_0_38;

    internal readonly IUtilities Utilities;
    public List<int> mcs_cof_ids_bin { get; set; }

    internal SnapchatConfig(IUtilities utilities)
    {
        Utilities = utilities;
    }

    public SnapchatConfig()
    {
        Utilities = new Utilities();
    }
}

public class SnapchatLockedConfig
{
    public SnapchatLockedConfig(SnapchatConfig config)
    {
        install_time = config.install_time;
        androidId = config.androidId;
        androidIncrementalVersion = config.androidIncrementalVersion;
        phoneManufacture = config.phoneManufacture;
        androidOsSDK = config.androidOsSDK;
        phoneModel = config.phoneModel;
        androidOsVersion = config.androidOsVersion;
        PhoneId = config.PhoneId;
        Username = config.Username;
        ApiKey = config.ApiKey;
        Proxy = config.Proxy;
        Debug = config.Debug;
        OldUsername = config.OldUsername;
        BandwithSaver = config.BandwithSaver;
        Timeout = config.Timeout;
        SnapchatVersion = config.SnapchatVersion;
        dtoken1i = config.dtoken1i;
        dtoken1v = config.dtoken1v;
        user_id = config.user_id;
        Access_Token = config.Access_Token;
        BusinessAccessToken = config.BusinessAccessToken;
        TimeZone = config.TimeZone;
        Horoscope = config.Horoscope;
        AccountCountryCode = config.AccountCountryCode;
        refreshToken = config.refreshToken;
        ClientID = config.ClientID;
        Age = config.Age;
        ProxySigner = config.ProxySigner;
        mcs_cof_ids_bin = config.mcs_cof_ids_bin;
        DeviceID = config.DeviceID;
        AccessTokenExpirey = config.AccessTokenExpirey;
        IPAbuseCheck = config.IPAbuseCheck;
        IPReuseCheck = config.IPReuseCheck;
        DebugInfo = config.DebugInfo;
        TLSSpoofing = config.TLSSpoofing;
        IPAPIPROKEY = config.IPAPIPROKEY;
    }
    public SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign Horoscope { get; set; }
    public long AccessTokenExpirey { get; set; }
    public string DebugInfo { get; set; }
    internal bool IsWindowsOS { get; set; }
    public string AccountCountryCode { get; set; }
    public string refreshToken { get; set; }
    public string TimeZone { get; set; }
    public string dtoken1i { get; set; }
    public int PhoneId { get; set; }
    public string androidOsVersion { get; set; }
    public int androidOsSDK { get; set; }
    public string androidIncrementalVersion { get; set; }
    public string phoneManufacture { get; set; }
    public string phoneModel { get; set; }
    public string dtoken1v { get; set; }
    public int Age { get; set; }
    public WebProxy Proxy { get; set; }
    public string ApiKey { get; set; }
    public string Username { get; set; }
    public string OldUsername { get; set; }
    public string Access_Token { get; set; }
    public string BusinessAccessToken { get; set; }
    public string androidId { get; set; }
    public long install_time { get; set; }
    public bool Debug { get; set; }
    public bool ProxySigner { get; set; }
    public bool BandwithSaver { get; set; }
    public string user_id { get; set; }
    public string ClientID { get; set; }
    public string DeviceID { get; set; }
    public int Timeout { get; set; }
    public bool IPAbuseCheck { get; set; }
    public bool IPReuseCheck { get; set; }
    public bool TLSSpoofing { get; set; }
    public List<int> mcs_cof_ids_bin { get; set; }
    public string IPAPIPROKEY { get; set; }
    public SnapchatVersion SnapchatVersion { get; set; }
}