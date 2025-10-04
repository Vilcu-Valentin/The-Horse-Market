using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Competition")]
public class CompetitionDef : ScriptableObject
{
    [Header("General Info")]
    public string CompetitionName = "Sprint Derby";
    public Sprite Icon;
    public Color backgroundColor;
    public Color foregroundColor;

    [Header("Scoring (stat -> weight)")]
    public List<CompetitionStats> competitionStats;

    [Header("Per-Tier Settings")]
    public List<TierSettings> tierSettings = new();

    /// <summary>
    /// Helper: look up the settings for a given tier
    /// </summary>
    public TierSettings GetSettingsFor(TierDef tier)
    {
        return tierSettings.Find(t => t.tier == tier)
            ?? throw new Exception($"No settings for tier {tier.TierName} in {name}");
    }
}

[Serializable]
public class TierSettings
{
    [Tooltip("Must match one of your TierDef assets")]
    public TierDef tier;

    [Tooltip("Represents percentage of horses over your horse (on average) 0.1 means 10% of the horses will be better than your horse")]
    [Range(0,1f)]public float difficulty;

    [Tooltip("Entry fee for this competition at this tier")]
    public long entryFee;

    [Tooltip("Reward per finishing place (index 0 = 1st place, ..., index 7 = 8th place)")]
    public List<PlaceReward> placeRewards = new List<PlaceReward>(8);
}

[Serializable]
public struct PlaceReward
{
    public long emeralds;   // emerald payout
    public int itemNumber;
}

[Serializable]
public struct CompetitionStats
{
    public StatType Stat;
    [Range(0, 1)] public float weight;
}
