namespace KalkulatorIP.Models;

public class IPCalculatorResult
{
    public string InputIP { get; set; } = "";
    public string NetworkAddress { get; set; } = "";
    public string BroadcastAddress { get; set; } = "";
    public string MinHost { get; set; } = "";
    public string MaxHost { get; set; } = "";
    public long TotalHosts { get; set; }
    public long UsableHosts { get; set; }
    public string NetworkClass { get; set; } = "";
    public int Prefix { get; set; }
    public string SubnetMask { get; set; } = "";
    public string WildcardMask { get; set; } = "";
    public string BinaryMask { get; set; } = "";
    public List<SubnetDivision> PossibleSubnets { get; set; } = new();
}

public class SubnetDivision
{
    public int Prefix { get; set; }
    public long NumberOfSubnets { get; set; }
    public long UsableHostsPerSubnet { get; set; }
    public string SubnetMask { get; set; } = "";
    public List<string> Subnets { get; set; } = new();
    public bool HasMore { get; set; }
    public long TotalSubnets { get; set; }
}
