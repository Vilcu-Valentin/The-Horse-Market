using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "HorseGame/Tier")]
public class TierDef : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;
    public string ID => id;

    public string TierName = "Tier I";
    public Sprite tierIcon;
    [Min(1)] public int TierIndex = 1; 
    public Color HighlightColor = Color.white; // for shop & UI ribbons
    public Color ForegroundColor = Color.white;
    public Color BackgroundColor = Color.white;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }
#endif

}
