using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Competition")]
public class CompetitionDef : ScriptableObject
{
    public string CompetitionName = "Sprint Derby";
    public Sprite Icon;
    public int EntryFee = 25;

    [Header("Scoring")]
    public StatType PrimaryStat = StatType.Speed;
    public StatType SecondaryStat = StatType.Stamina;
    [Range(0, 1)] public float PrimaryWeight = 0.7f;  // 0.7 * P + 0.3 * S
    [Range(0, 1)] public float SecondaryWeight = 0.3f;

    [Header("Payout")]
    public AnimationCurve RewardCurve = AnimationCurve.Linear(0, 10, 1, 100);
    // X-axis = percentile rank (0 worst .. 1 best)
    // Y-axis = emerald reward

    [Header("Cooldown")]
    [Min(0)] public float CooldownMinutes = 5f;        // per horse
}
