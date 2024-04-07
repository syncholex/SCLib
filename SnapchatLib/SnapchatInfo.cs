using System.Collections.Generic;

namespace SnapchatLib;

public enum SnapchatVersion
{
    V12_76_0_38,
}

public enum OS
{
    android
}

internal class SnapchatInfo
{
    public string Version;
    internal OS OS;
    private static readonly Dictionary<SnapchatVersion, SnapchatInfo> VersionKeyMap = new()
    {
        //Android
        { SnapchatVersion.V12_76_0_38, new SnapchatInfo { Version = "12.76.0.38", OS = OS.android }},
    };

    public static SnapchatInfo GetInfo(SnapchatVersion version)
    {
        return VersionKeyMap[version];
    }
}