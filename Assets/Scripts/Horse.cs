using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

[Serializable]
public class Horse
{
    public Guid Id;
    public string horseName;

    // Tier & visuals are references to immutable ScriptableObjects
    public TierDef Tier;
    public VisualDef Visual;

    // Training
    public int currentTrainingEnergy;

    // TRAINABLE STATS  – current value & cap
    public Stat[] Current = new Stat[4];
    public Stat[] Max = new Stat[4];

    [SerializeField] private List<TraitDef> _traits = new();
    public IReadOnlyList<TraitDef> Traits => _traits;   // read-only view

    public int GetCurrent(StatType s) => Current.Get(s);
    public int GetMax(StatType s) => Max.Get(s);
    public void AddCurrent(StatType s, int delta)
    {
        var sb = Current.Get(s);
        sb = Mathf.Clamp(sb + delta, 0, Max.Get(s));
        Current.Set(s, sb);
    }

    /// <summary>
    /// This method will train the horse provided enough energy is left
    /// </summary>
    /// <returns>A bool stating if training was succesfull(true) or not(false)</returns>
    public bool Train()
    {
        if (currentTrainingEnergy <= 0)
            return false;

        int canFailTraining = 0;
        foreach(TraitDef trait in _traits)
            if(trait.canFailTraining == true)
                canFailTraining++;

        if (canFailTraining > 0)
        {
            float roll;
            roll = UnityEngine.Random.value;

            float value = (Mathf.Pow(canFailTraining, 0.8f)) / 3f;
            if (roll < value)
            {
                currentTrainingEnergy--;
                return false;
            }
        }

        int amount = GetTrainingRate();
        foreach (var stat in Current)
            AddCurrent(stat._Stat, amount);

        currentTrainingEnergy--;
        return true;
    }

    public int GetTrainingRate()
    {
        float trainMultiplier = 1f;
        foreach (TraitDef trait in _traits)
            trainMultiplier *= trait.TrainingMult;

        return Mathf.RoundToInt(Mathf.Max(1, Tier.TierIndex * trainMultiplier));
    }

    /// <summary>
    /// Calculates the max amount of training energy a horse has
    /// </summary>
    /// <returns></returns>
    public int GetTrainingEnergy()
    {
        float trainingSpeed = 1f;
        foreach (TraitDef trait in _traits)
            trainingSpeed *= trait.TrainingSpeed;

        return Mathf.RoundToInt(Mathf.Clamp((Tier.TierIndex * 0.88f + 3) * trainingSpeed, 1, 12));
    }

    /// <summary>
    /// Calculates the horse's breeding odds based on the tier and current traits
    /// </summary>
    /// <returns>Triple floats for downgrade, same, upgrade chance</returns>
    public (float, float, float) GetBreedingOdds()
    {
        return BreedingSystem.CalculateUpgradeOdds(this);
    }

    /// <summary>
    /// Calculates the horses max possible market price (when it's fully trained) based on it's stats and traits
    /// </summary>
    /// <returns>An long value representing the price in emerald</returns>
    public long GetMaxPrice()
    {
        // 1) Compute average tiers (ensure no zero‐division)
        float avgCaps = Mathf.Max(1f, (float)Max.Average(t => t.Value));

        // 2) Base price = (0.75 * avgCaps)^2, rounded
        float rawBase = 0.75f * avgCaps;
        long basePrice = (long)Mathf.Round(Mathf.Pow(rawBase, 1.8f));

        // 3) Aggregate all flat multipliers from traits + visual
        float priceMultiplier = 1f;
        float venerableMultiplier = 1f;
        foreach (var trait in _traits)
        {
            priceMultiplier *= 1f + trait.PriceScalar;
            venerableMultiplier *= trait.Venerable;
        }
        priceMultiplier *= Visual.PriceScalar;

        // warp venerableMultiplier from [1 → 1.5] into [0 → 1]
        float vNorm = (venerableMultiplier - 1) / 1.5f;
        float vWarp = Mathf.Pow(vNorm, 1.15f);

        float venerableScalar = 1f + 2f * vWarp;
        priceMultiplier *= venerableScalar;

        Debug.Log("Max Price: " + basePrice * priceMultiplier +
    " BasePrice: " + basePrice + " PriceMult: " + priceMultiplier);

        // 5) Final price
        return (long)Mathf.Round(basePrice * priceMultiplier);
    }

    /// <summary>
    /// Calculates the horse’s current market price based on its stats and traits.
    /// </summary>
    /// <returns>Price in emerald (rounded to nearest integer).</returns>
    public long GetCurrentPrice()
    {
        // 1) Compute average tiers (ensure no zero‐division)
        float avgCaps = Mathf.Max(1f, (float)Max.Average(t => t.Value));
        float avgCurrent = Current.Average(t => (float)t.Value);

        // 2) Base price = (0.75 * avgCaps)^2, rounded
        float rawBase = 0.75f * avgCaps;
        long basePrice = (long)Mathf.Round(Mathf.Pow(rawBase, 1.8f));

        // 3) Aggregate all flat multipliers from traits + visual
        float priceMultiplier = 1f;
        float venerableMultiplier = 1f;
        foreach (var trait in _traits)
        {
            priceMultiplier *= 1f + trait.PriceScalar;
            venerableMultiplier *= trait.Venerable;
        }
        priceMultiplier *= Visual.PriceScalar;

        // 4) “Ease‐in” on how much the current tier (relative to cap) and venerable boost the price
        float tierRatio = (float)(avgCurrent - 1) / (Mathf.Max(1, avgCaps - 1));               // now a float in [0,1]
        float tierRatioSq = tierRatio * tierRatio;            // instead of Pow(x,2)

        // warp venerableMultiplier from [1 → 1.5] into [0 → 1]
        float vNorm = (venerableMultiplier - 1) / 1.5f;
        float vWarp = Mathf.Pow(vNorm, 1.15f);

        float venerableScalar = 1f + 2f * tierRatioSq * vWarp;
        priceMultiplier *= venerableScalar;

        float trainingMultiplier = tierRatio * 0.5f + 0.5f;

        Debug.Log("Current Price: " + basePrice * priceMultiplier * trainingMultiplier +
            " BasePrice: " + basePrice + " PriceMult: " + priceMultiplier + "TrainingMult: " + trainingMultiplier + "TrainingR: " + tierRatio);

        // 5) Final price
        return (long)Mathf.Round(basePrice * priceMultiplier * trainingMultiplier);
    }

    /// <summary>
    /// Calculates the minimum possible price of the horse
    /// </summary>
    /// <returns></returns>
    public long GetMinPrice()
    {
        // 1) Compute average tiers (ensure no zero‐division)
        float avgCaps = Mathf.Max(1f, (float)Max.Average(t => t.Value));

        // 2) Base price = (0.75 * avgCaps)^2, rounded
        float rawBase = 0.75f * avgCaps;
        long basePrice = (long)Mathf.Round(Mathf.Pow(rawBase, 1.8f));

        // 3) Aggregate all flat multipliers from traits + visual
        float priceMultiplier = 1f;
        float venerableMultiplier = 1f;
        foreach (var trait in _traits)
        {
            priceMultiplier *= 1f + trait.PriceScalar;
            venerableMultiplier *= trait.Venerable;
        }
        priceMultiplier *= Visual.PriceScalar;

        // 5) Final price
        return (long)Mathf.Round(basePrice * priceMultiplier * 0.5f);
    }

    /// <summary>
    /// Used by the shop, it's the market price of the horse + 35%
    /// </summary>
    /// <returns>Inflated current price</returns>
    public long GetMarketPrice()
    {
        return (long)(GetCurrentPrice() * 1.3);
    }


    internal Horse(
        Guid g,  
        TierDef tier, 
        VisualDef visual,
        List<TraitDef> traits)
    {
        Id = g;
        Tier  = tier;
        Visual = visual;
        _traits = traits;
        BuildStats(tier, traits);
        horseName = HorseNameGenerator.GetRandomHorseName();
    }

    private void BuildStats(TierDef tier, List<TraitDef> traits)
    {
        var maxStats = new List<Stat>();
        var currentStats = new List<Stat>();

        // 0) Log tier basics
        int baseCap = tier.GetCap();
        float tierRadius = 0.18f * Mathf.Pow(1.17f, -tier.TierIndex);
        Debug.Log($"[BuildStats] Tier={tier.name} (Index={tier.TierIndex}) → baseCap={baseCap}, tierRadius={tierRadius:F4}");

        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            Debug.Log($"--- Stat: {stat} ---");

            // ---- 1) TRAIT CAP BONUS (multiplicative) ----
            var capModList = traits
                .SelectMany(t => t.StatCapMods, (t, m) => new { Trait = t.name, m.Stat, m.Modifier })
                .Where(x => x.Stat == stat)
                .ToList();

            // log each one
            foreach (var m in capModList)
                Debug.Log(
                    $"    CapMod from {m.Trait}: " +
                    $"percent={(m.Modifier * 100f):F1}% → factor={(1f + m.Modifier):F4}"
                );

            // multiplicative stack
            float capMultiplier = capModList
                .Aggregate(1f, (acc, m) => acc * (1f + m.Modifier));

            Debug.Log($"    capMultiplier = {capMultiplier:F4}");


            // ---- 2) RANDOMIZE AROUND 1.0 ± tierRadius ----
            // (unchanged)
            float randomFactor = UnityEngine.Random.Range(1f - tierRadius, 1f + tierRadius);
            Debug.Log($"    randomFactor in [{1f - tierRadius:F4}…{1f + tierRadius:F4}] = {randomFactor:F4}");

            // compute max
            int maxValue = Mathf.RoundToInt(baseCap * randomFactor * capMultiplier);
            maxValue = Mathf.Max(1, maxValue);
            Debug.Log($"    maxValue = RoundToInt({baseCap} * {randomFactor:F4} * {capMultiplier:F4}) = {maxValue}");
            maxStats.Add(new Stat { _Stat = stat, Value = maxValue });


            // ---- 3) STARTING STAT PERCENT (multiplicative) ----
            var startModList = traits
                .SelectMany(t => t.StartingStats, (t, m) => new { Trait = t.name, m.Stat, m.Modifier })
                .Where(x => x.Stat == stat)
                .ToList();

            // log each one
            foreach (var m in startModList)
                Debug.Log(
                    $"    StartingStatMod from {m.Trait}: " +
                    $"percent={(m.Modifier * 100f):F1}% → factor={(1f + m.Modifier):F4}"
                );

            // multiplicative stack of starting‐percent
            float startMultiplier = startModList
                .Aggregate(1f, (acc, m) => acc * (1f + m.Modifier));

            // convert back to percent-of-base and clamp (0…90%)
            float startPercent = startMultiplier - 1f;
            float startPercentClamped = Mathf.Clamp(startPercent, 0f, 0.9f);

            Debug.Log(
                $"    startPercent = {startPercent:F4} " +
                $"→ clamped to {startPercentClamped:F4}"
            );


            // ---- 4) STARTING RANDOMNESS ----
            float startRandomness = traits.Sum(t => t.StartingBonusRandomness);
            Debug.Log($"    startRandomness (sum of bonuses)={startRandomness:F4}");

            // baseline start = maxValue * startPercentClamped
            float baselineStart = maxValue * startPercentClamped;
            Debug.Log($"    baselineStart = {maxValue} * {startPercentClamped:F4} = {baselineStart:F4}");

            // random range around that baseline
            float randomStart = UnityEngine.Random.Range(
                baselineStart * (1f - startRandomness),
                baselineStart * (1f + startRandomness)
            );
            Debug.Log($"    randomStart in [{baselineStart * (1f - startRandomness):F4}…{baselineStart * (1f + startRandomness):F4}] = {randomStart:F4}");

            int currentValue = Mathf.RoundToInt(randomStart);
            currentValue = Mathf.Clamp(currentValue, 1, maxValue);
            Debug.Log($"    currentValue = Clamp(RoundToInt({randomStart:F4}), 1, {maxValue}) = {currentValue}");
            currentStats.Add(new Stat { _Stat = stat, Value = currentValue });
        }

        Max = maxStats.ToArray();
        Current = currentStats.ToArray();

        currentTrainingEnergy = GetTrainingEnergy();

        Debug.Log($"[BuildStats] Finished. Generated {Max.Length} max-stats and {Current.Length} current-stats.");
    }
}

[Serializable]
public struct Stat
{
    public StatType _Stat;
    public int Value;
}