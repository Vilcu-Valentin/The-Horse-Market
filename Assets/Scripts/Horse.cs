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
        BuildStats(tier, traits, visual);
    }

    private void BuildStats(TierDef tier, List<TraitDef> traits, VisualDef visual)
    {
        List<Stat> max = new List<Stat>();
        List<Stat> current = new List<Stat>(); 
        // Current and Max stats
        foreach (StatType s in Enum.GetValues(typeof(StatType)))
        {
            int maxS = 0, currS = 0;

            // Added tier cap
            maxS += (int)UnityEngine.Random.Range(tier.GetCap() * -(0.35f * Mathf.Pow(1.17f, -tier.TierIndex)), tier.GetCap() * (0.35f * Mathf.Pow(1.17f, -tier.TierIndex)));

            // Added traits cap
            float totalMod = traits
                .SelectMany(t => t.StatCapMods)
                .Where(m => m.Stat == s)
                .Sum(m => m.Modifier);
            maxS = (int)Math.Round(maxS * totalMod);

            max.Add(new Stat { _Stat = s, Value = maxS });

            currS += traits.Sum(t => t.StartingStats
                                  .Where(m => m.Stat == s)
                                  .Sum(m => (int)Math.Round(m.Delta * t.StartingBonusRandomness)));

            current.Add(new Stat { _Stat = s, Value = currS });
        }

        Current = current.ToArray();
        Max = max.ToArray();
    }
}

[Serializable]
public struct Stat
{
    public StatType _Stat;
    public int Value;
}