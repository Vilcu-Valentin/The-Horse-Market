using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public static class StatArrayExtensions
{
    public static int Get(this Stat[] stats, StatType type)
        => stats[(int)type].Value;

    public static void Set(this Stat[] stats, StatType type, int value)
        => stats[(int)type].Value = value;

    public static int Get(this List<Stat> stats, StatType type)
        => stats[(int)type].Value;

}