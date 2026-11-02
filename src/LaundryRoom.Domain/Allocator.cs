public static class Allocator
{
    public static IReadOnlyList<Money> Allocate(Money total, IReadOnlyList<Kwh> weights)
    {
        var totalWeights = weights.Sum(weight => weight.Value);
        if (totalWeights == 0) throw new ArgumentException("All weights can't be zero.", nameof(weights));

        var exactShares = weights.Select(weight => total.Value * weight.Value / totalWeights).ToList();
        var roundedDownShares = exactShares.Select(share => (long)Math.Floor(share)).ToList();
        var leftover = total.Value - roundedDownShares.Sum();

        AllocateLeftover(leftover, exactShares, roundedDownShares);

        return roundedDownShares.Select(share => Money.From(share)).ToList();
    }

    private static void AllocateLeftover(long leftover, List<decimal> exactShares, List<long> shares)
    {
        while (leftover > 0)
        {
            var i = GetIndexOfLargestCutOff(exactShares, shares);
            shares[i] += 1;
            leftover--;
        }
    }

    private static int GetIndexOfLargestCutOff(List<decimal> exactShares, List<long> shares)
    {
        var cutOffs = exactShares.Select((exactShare, i) => exactShare - shares[i]).ToList();
        return cutOffs.IndexOf(cutOffs.Max());
    }
}