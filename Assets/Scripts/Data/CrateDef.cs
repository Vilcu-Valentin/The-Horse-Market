using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Crate")]
public class CrateDef : ScriptableObject
{
    public string CrateName = "Basic Stable";
    public Sprite Icon;
    public int CostInEmeralds = 100;

    [Header("Tier Probabilities")]
    public List<WeightedTier> TierChances = new()
    {
        new WeightedTier { Tier = null, Weight = 1 }   // assign real tiers in Inspector
    };

    [Min(0)] public int MinTraits = 1;
    [Min(1)] public int MaxTraits = 2;
}

[Serializable]
public struct WeightedTier
{
    public TierDef Tier;
    [Min(0)] public float Weight;        // relative probability
}