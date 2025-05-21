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

    [Header("Initial Horse Setup")]
    [Min(0)] public int MinTraitRolls = 1; 
    [Min(1)] public int MaxTraitRolls = 2;   
    // Visual picking
    [Range(0, 0.5f)]
    public float MaxStatVariance = 0.10f;              // ±10 % wiggle room around tier cap
}


[Serializable]
public struct WeightedTier
{
    public TierDef Tier;
    [Min(0)] public float Weight;        // relative probability
}