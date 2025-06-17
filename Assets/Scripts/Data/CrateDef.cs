using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Crate")]
public class CrateDef : ScriptableObject
{
    public string CrateName = "Basic Crate";
    public Sprite Icon;
    public Color crateColor;
    public int CostInEmeralds = 100;

    [Header("Tier Probabilities")]
    public List<WeightedTier> TierChances = new()
    {
        new WeightedTier { Tier = null, Tickets = 1 }   // assign real tiers in Inspector
    };

    public float getTierChance(WeightedTier weightedTier)
    {
        float ticketSum = 0;
        foreach (var tier in TierChances)
            ticketSum += tier.Tickets;

        return weightedTier.Tickets / ticketSum;
    }
}

[Serializable]
public struct WeightedTier
{
    public TierDef Tier;
    [Min(0)] public int Tickets;        // relative probability
    [Min(0)] public int MinTraits;
    [Min(1)] public int MaxTraits;
}