using System;

namespace Lotus.Network.PrivacyPolicy;

public class PrivacyPolicyInfo
{
    public static PrivacyPolicyInfo Instance = null!;
    public PrivacyPolicyInfo(ValueTuple<bool, bool, bool, long> arguments)
    {
        ConnectWithAPI = arguments.Item1;
        LobbyDiscovery = arguments.Item2;
        AnonymousBugReports = arguments.Item3;
        LastAcceptedPrivacyPolicyVersion = arguments.Item4;
        Instance = this;
    }
    public readonly bool ConnectWithAPI;
    public readonly bool AnonymousBugReports;
    public readonly bool LobbyDiscovery;
    public readonly long LastAcceptedPrivacyPolicyVersion;
}