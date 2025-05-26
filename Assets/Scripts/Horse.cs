using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            // ---- 1) TRAIT CAP BONUS ----
            var capModList = traits
                .SelectMany(t => t.StatCapMods, (t, m) => new { Trait = t.name, m.Stat, m.Modifier })
                .Where(x => x.Stat == stat)
                .ToList();
            foreach (var m in capModList)
                Debug.Log($"    CapMod from {m.Trait}: rawModifier={m.Modifier:F4} (contribution={m.Modifier - 1f:F4})");
            float capBonusPercent = capModList.Sum(x => x.Modifier);
            float capMultiplier = 1f + capBonusPercent;
            Debug.Log($"    capBonusPercent={capBonusPercent:F4} → capMultiplier={capMultiplier:F4}");

            // ---- 2) RANDOMIZE AROUND 1.0 ± tierRadius ----
            float randomFactor = UnityEngine.Random.Range(1f - tierRadius, 1f + tierRadius);
            Debug.Log($"    randomFactor in [{1f - tierRadius:F4}…{1f + tierRadius:F4}] = {randomFactor:F4}");

            // compute max
            int maxValue = Mathf.RoundToInt(baseCap * randomFactor * capMultiplier);
            maxValue = Mathf.Max(1, maxValue);
            Debug.Log($"    maxValue = RoundToInt({baseCap} * {randomFactor:F4} * {capMultiplier:F4}) = {maxValue}");
            maxStats.Add(new Stat { _Stat = stat, Value = maxValue });

            // ---- 3) STARTING STAT PERCENT ----
            var startModList = traits
                .SelectMany(t => t.StartingStats, (t, m) => new { Trait = t.name, m.Stat, m.Modifier })
                .Where(x => x.Stat == stat)
                .ToList();
            foreach (var m in startModList)
                Debug.Log($"    StartingStatMod from {m.Trait}: modifier={m.Modifier:F4}");
            float startPercent = startModList.Sum(x => x.Modifier);
            float startPercentClamped = Mathf.Clamp(startPercent, 0f, 0.9f);
            Debug.Log($"    startPercent (sum)={startPercent:F4} → clamped to {startPercentClamped:F4}");

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

        Debug.Log($"[BuildStats] Finished. Generated {Max.Length} max-stats and {Current.Length} current-stats.");
    }


}

[Serializable]
public struct Stat
{
    public StatType _Stat;
    public int Value;
}