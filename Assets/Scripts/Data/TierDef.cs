using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Tier")]
public class TierDef : ScriptableObject
{
    public string TierName = "Tier I";
    [Min(1)] public int TierIndex = 1; 
    public Color UIColour = Color.white; // for shop & UI ribbons
    [Min(1)] public float BasePriceMultiplier = 1f;

    [Header("Base Stat Caps")]
    [Min(1)] public int SpeedCap = 10;
    [Min(1)] public int StaminaCap = 10;
    [Min(1)] public int JumpHeightCap = 10;
    [Min(1)] public int StrengthCap = 10;

    public int GetCap(StatType s) => s switch
    {
        StatType.Speed => SpeedCap,
        StatType.Stamina => StaminaCap,
        StatType.JumpHeight => JumpHeightCap,
        StatType.Strength => StrengthCap,
        _ => 0
    };

    [Header("Baseline Breeding Odds")]
    [Range(0, 1)] public float UpgradeChance = 0.30f;
    [Range(0, 1)] public float DowngradeChance = 0.20f; 
    [Range(0, 1)] public float MutationChance = 0.02f;    // new trait appears

    // Convenience
    public float SameChance => 1f - UpgradeChance - DowngradeChance;
}
