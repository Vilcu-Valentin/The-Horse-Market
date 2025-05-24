using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Tier")]
public class TierDef : ScriptableObject
{
    public string TierName = "Tier I";
    public Sprite tierIcon;
    [Min(1)] public int TierIndex = 1; 
    public Color UIColour = Color.white; // for shop & UI ribbons
    [Tooltip("Final price multiplier, after all of the traits of the horse are calculated, they get multiplied with this value")]
    [Min(1)] public float BasePriceMultiplier = 1f;

    [Tooltip("Base State Cap for all stats, without any traits")]
    [Min(1)] public int StatCap = 10;

    public int GetCap()
    {
        return StatCap;
    }

    [Header("Baseline Breeding Odds")]
    [Range(0, 1)] public float UpgradeChance = 0.30f;
    [Range(0, 1)] public float DowngradeChance = 0.20f; 
    [Range(0, 1)] public float MutationChance = 0.02f;    // new trait appears

    // Convenience
    public float SameChance => 1f - UpgradeChance - DowngradeChance;
}
