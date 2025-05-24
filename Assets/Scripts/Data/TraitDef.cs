using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "HorseGame/Trait")]
public sealed class TraitDef : ScriptableObject
{
    [Header("Identity")]
    public string DisplayName;
    [TextArea] public string Description;
    [Tooltip("The lower this number, the rarer they are")]
    public int rarityTickets;
    public Sprite Icon;
    public bool IsNegative;

    [Header("Breeding Odds")]
    [Tooltip("Multiplier applied to upgrade probability (<1 = worse, >1 = better)")]
    public float UpgradeMult = 1f;
    [Tooltip("Multiplier applied to downgrade probability (<1 = safer, >1 = riskier)")]
    public float DowngradeMult = 1f;
    [Tooltip("false - it will only move one tier up/down, true - it will randomly(weighted) move between 1-3 tiers")]
    public bool TierVolatility = false;

    [Header("Traits Inheritence")]
    [Tooltip("How reliably this trait is inherited if ONE parent owns it")]
    [Range(0f, 0.5f)] public float BaseInheritChance = 0.25f;
    [Tooltip("Multiplier changes the base mutation chance for a new trait to apper (1 - neutral)")]
    public float MutationMultiplier = 1f;

    [Header("Training")]
    [Tooltip("Multiplier on XP gain during training (<1 = worse, >1 = better)")]
    public float TrainingMult = 1f;
    [Tooltip("Multiplier on training speed (<1 = faster, >1 = slower)")]
    public float TrainingSpeed = 1f;
    [Tooltip("If true training can fail and yield no increase")]
    public bool canFailTraining = false;

    [Header("Stat Adjustments")]
    [Tooltip("Applies a modifier to a specific stat of a horse (0 - neutral)")]
    public List<StatMod> StatCapMods = new();
    [Tooltip("Applies a starting stat bonus (0 - neutral)")]
    public List<StatMod> StartingStats = new();
    [Tooltip("Randomizes the starting bonus values, 0f = no randomness, 0.1f = +- 10%")]
    [Range(0f, 1f)]public float StartingBonusRandomness = 0f;

    [Header("Competitions & Price")]
    [Tooltip("Percent boost to all competition scores (± %)")]
    public float CompetitionBuffPct = 0f;        // e.g. Talented = +5 % Deviant = -5%
    [Tooltip("Scalar on market price (1 = neutral)")] 
    public float PriceScalar = 1f; // such as for Champion or Venerable
    [Tooltip("Gains a higher premium price based on the current stats. 1 - no multiplier")]
    [Range(1f, 3f)]public float Venerable = 1f;

    [Header("Conflicts")]
    [Tooltip("The traits in this list cannot appear on a horse that has the current trait.")]
    public List<TraitDef> conflictedTraits;

    public bool IsConflict(TraitDef other)
    {
        return conflictedTraits.Contains(other);
    }
}

[Serializable]
public struct StatMod            
{
    public StatType Stat;
    public float Modifier;
}

[Serializable]
public struct StatDelta
{
    public StatType Stat;
    public int Delta;
}