namespace KalkulatorIP.Models;

public static class IPCalculator
{
    private const int MaxSubnetsToDisplay = 256;

    public static IPCalculatorResult Calculate(string ipInput, string maskInput)
    {
        string ip = ipInput.Trim();
        string mask = maskInput.Trim();

        // Allow combined format: 192.168.1.0/24
        if (ip.Contains('/'))
        {
            var parts = ip.Split('/', 2);
            ip = parts[0].Trim();
            mask = parts[1].Trim();
        }

        uint ipBits = ParseIP(ip);
        int prefix = ParseMask(mask);

        uint maskBits = prefix == 0 ? 0u
                      : prefix == 32 ? 0xFFFFFFFFu
                      : (0xFFFFFFFFu << (32 - prefix));

        uint networkBits = ipBits & maskBits;
        uint broadcastBits = networkBits | (~maskBits);

        long totalHosts = 1L << (32 - prefix);
        long usableHosts = prefix >= 31 ? totalHosts : Math.Max(0, totalHosts - 2);

        var possibleSubnets = new List<SubnetDivision>();
        for (int p = prefix + 1; p <= 30; p++)
        {
            uint step = 1u << (32 - p);
            long numSubnets = 1L << (p - prefix);
            long usablePerSubnet = (long)step - 2;
            uint subMask = (uint)(0xFFFFFFFFu << (32 - p));

            long displayCount = Math.Min(numSubnets, MaxSubnetsToDisplay);
            var subnets = new List<string>((int)displayCount);

            for (long i = 0; i < displayCount; i++)
            {
                uint net = networkBits + (uint)((long)i * step);
                uint bc  = net + step - 1;
                uint min = net + 1;
                uint max = bc - 1;
                subnets.Add($"{BitsToIP(net)}/{p}   {BitsToIP(min)} – {BitsToIP(max)}   BC: {BitsToIP(bc)}");
            }

            possibleSubnets.Add(new SubnetDivision
            {
                Prefix = p,
                NumberOfSubnets = numSubnets,
                UsableHostsPerSubnet = usablePerSubnet,
                SubnetMask = BitsToIP(subMask),
                Subnets = subnets,
                HasMore = numSubnets > MaxSubnetsToDisplay,
                TotalSubnets = numSubnets
            });
        }

        return new IPCalculatorResult
        {
            InputIP = BitsToIP(ipBits),
            NetworkAddress = BitsToIP(networkBits),
            BroadcastAddress = BitsToIP(broadcastBits),
            MinHost = BitsToIP(networkBits + 1),
            MaxHost = BitsToIP(broadcastBits - 1),
            TotalHosts = totalHosts,
            UsableHosts = usableHosts,
            NetworkClass = GetClass(networkBits),
            Prefix = prefix,
            SubnetMask = BitsToIP(maskBits),
            WildcardMask = BitsToIP(~maskBits),
            BinaryMask = ToBinaryMask(prefix),
            PossibleSubnets = possibleSubnets
        };
    }

    private static uint ParseIP(string ip)
    {
        var parts = ip.Split('.');
        if (parts.Length != 4)
            throw new ArgumentException("Nieprawidłowy format adresu IP — wymagany format: xxx.xxx.xxx.xxx");

        uint result = 0;
        foreach (var part in parts)
        {
            if (!uint.TryParse(part, out uint octet) || octet > 255)
                throw new ArgumentException($"Nieprawidłowy oktet: \"{part}\" (zakres 0–255)");
            result = (result << 8) | octet;
        }
        return result;
    }

    private static int ParseMask(string mask)
    {
        if (mask.StartsWith('/'))
            mask = mask[1..];

        if (int.TryParse(mask, out int prefix))
        {
            if (prefix < 0 || prefix > 32)
                throw new ArgumentException("Prefiks CIDR musi być z zakresu 0–32");
            return prefix;
        }

        // Dotted notation
        var parts = mask.Split('.');
        if (parts.Length != 4)
            throw new ArgumentException("Nieprawidłowy format maski — użyj xx lub xxx.xxx.xxx.xxx");

        uint maskBits = 0;
        foreach (var part in parts)
        {
            if (!uint.TryParse(part, out uint octet) || octet > 255)
                throw new ArgumentException($"Nieprawidłowy oktet maski: \"{part}\"");
            maskBits = (maskBits << 8) | octet;
        }

        // Verify contiguous 1s
        bool seenZero = false;
        int count = 0;
        for (int i = 31; i >= 0; i--)
        {
            bool bit = ((maskBits >> i) & 1u) == 1u;
            if (bit && seenZero)
                throw new ArgumentException("Nieprawidłowa maska podsieci — bity muszą być ciągłe");
            if (bit) count++;
            else seenZero = true;
        }
        return count;
    }

    public static string BitsToIP(uint bits) =>
        $"{(bits >> 24) & 0xFF}.{(bits >> 16) & 0xFF}.{(bits >> 8) & 0xFF}.{bits & 0xFF}";

    private static string GetClass(uint networkBits)
    {
        byte first = (byte)(networkBits >> 24);
        return first switch
        {
            < 128 => "A",
            < 192 => "B",
            < 224 => "C",
            < 240 => "D (Multicast)",
            _     => "E (Zarezerwowana)"
        };
    }

    private static string ToBinaryMask(int prefix)
    {
        string b = new string('1', prefix) + new string('0', 32 - prefix);
        return $"{b[..8]}.{b[8..16]}.{b[16..24]}.{b[24..]}";
    }
}
