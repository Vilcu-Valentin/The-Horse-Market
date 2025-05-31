using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShopSystem 
{
    private static long refreshOffersIncreases = -1;
    public static void BuyHorse(Horse horse)
    {
        // Check if enough money, if there are enough remove them, otherwise raise money error
        
        refreshOffersIncreases = 0;
        SaveSystem.Instance.Current.horses.Add(horse);
    }

    /// <summary>
    /// Generates 'count' new horse offers and returns them along with the emerald cost for the next refresh.
    /// </summary>
    public static (List<Horse> offers, long nextRefreshPrice) GenerateOffers(int count)
    {
        var offers = new List<Horse>(count);

        // create all offers
        for (int i = 0; i < count; i++)
        {
            TierDef tier = PickHorseTier();
            int traits = UnityEngine.Random.Range(2, 7);
            offers.Add(HorseFactory.CreateRandomHorse(tier, traits));
        }

        // bump the counter and compute price
        refreshOffersIncreases++;
        return (offers, GetRefreshOffer());
    }

    private static TierDef PickHorseTier()
    {
        var values = new List<(TierDef tier, int ticket)>();

        foreach(var tier in HorseMarketDatabase.Instance._allTiers)
        {
            int tickets = TicketDistribution(tier.TierIndex, SaveSystem.Instance.Current.GetHighestHorseTier());
            if(tickets > 0 )
            {
                values.Add((tier, tickets));
            }
        }

        return WeightedSelector<TierDef>.Pick(values);  
    }

    /// <summary>
    /// Gives a number of tickets for a particular Tier based on the max horse tier the player currently has
    /// </summary>
    /// <param name="x">The tier you want to get tickets for</param>
    /// <param name="v">The current max tier</param>
    /// <returns></returns>
    private static int TicketDistribution(int x, int v)
    {
        // constants as floats
        const float a = 50f;
        const float r = 2.8f;
        const float s = 1.26f;
        const float k = -0.3f;

        // do all math in floats
        float diff = x - v;                          // x and v are ints, promoted to float
        float denom = 1f + k * MathF.Sign(diff);     // sgn(x-v)
        float exponent = MathF.Pow(MathF.Abs(diff) / denom, s);
        float value = a * MathF.Pow(r, -exponent);

        // round to nearest int
        return (int)MathF.Round(value);
    }

    /// <summary>
    /// Gives a price for the refresh based on the tier and how many refreshes have been made
    /// </summary>
    /// <returns>A long representing the cost in emeralds of the refresh</returns>
    private static long GetRefreshOffer()
    {
        double u = SaveSystem.Instance.Current.GetHighestHorseTier();
        double x = refreshOffersIncreases;

        double expoExp = Math.Pow(u, 0.95) * (x / 10.0);
        double powPart = Math.Pow(x, expoExp);
        double cubicPart = 50.0 * x * Math.Pow(u, 3.2);

        double rawOffer = powPart + cubicPart;

        // guard against NaN
        if (double.IsNaN(rawOffer) || double.IsInfinity(rawOffer))
            rawOffer = 696969; // error

        long units = (long)Math.Round(rawOffer / 50.0);
        return units * 50L;
    }

    /// <summary>
    /// Debug helper: For each Tier in HorseMarketDatabase.Instance._allTiers,
    /// generate `samplesPerTier` random horses of *that* tier, sum up their Price,
    /// and then print "Tier X – average price: YYY" (one line per tier).
    ///
    /// Call this from anywhere (e.g. inside GenerateOffers, or from an editor button)
    /// to see the empirical average price per tier.
    /// </summary>
    public static void DebugAverageOfferPrices(int samplesPerTier)
    {
        if (HorseMarketDatabase.Instance == null ||
            HorseMarketDatabase.Instance._allTiers == null)
        {
            Debug.LogError("HorseMarketDatabase or its _allTiers list is null. Cannot compute averages.");
            return;
        }

        // Loop over every tier definition in your database
        foreach (TierDef tier in HorseMarketDatabase.Instance._allTiers)
        {
            // Accumulate total price in a 64-bit integer (to avoid overflow)
            long totalPriceForTier = 0;

            for (int i = 0; i < samplesPerTier; i++)
            {
                int traits = UnityEngine.Random.Range(2, 7);

                // Create exactly one horse of the *current* tier
                Horse randomHorse = HorseFactory.CreateRandomHorse(tier, traits);

                totalPriceForTier += randomHorse.GetMaxPrice();
            }

            // Compute the average as a double for accuracy
            double avgPrice = totalPriceForTier / (double)samplesPerTier;

            // Format the tier’s "name"
            string tierName = tier.TierName;

            Debug.Log($"[DEBUG] {tierName} – average price: {avgPrice:F2}");
        }
    }
}
