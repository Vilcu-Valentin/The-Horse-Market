using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Item")]
public class ItemDef : ScriptableObject
{
    public string ID;

    public string DisplayName;
    public Sprite Icon;
    [TextArea]public string Description;
    [TextArea] public string ModifiersDescription;
    [Tooltip("The lower this number, the rarer they are")]
    public int rarityTickets;

    public Sprite BorderIcon;
    [Tooltip("Lower for very bad traits, Higher for luxury traits")]
    public int quality;

    public bool isTrainingItem = false;
    [Header("Training Stats")]
    [Tooltip("Applies a stat bonus per training session (1 - no bonus)")]
    public List<StatDelta> AdditionalStatDelta = new();
    public bool preventFailure = false;
    public bool noEnergyUse = false;

    [Header("Breeding Stats")]
    public bool preventParentConsumption = false;
    public float UpgradeMult = 1.0f;
    public float DowngradeMult = 1.0f;
    public bool guaranteeUpgrade = false;
    public bool guaranteeSameTier = false;
    public bool guaranteeNoDowngrade = false;
    [Tooltip("Applies a modifier to a specific stat of a horse (1 - neutral)")]
    public List<StatMod> StatCapMods = new();

    public bool randomVisuals = false;
    public float mutationMultiplier = 1.0f;
    [Tooltip("No items are kept: 99")]
    public int keepHigherThenQualityItems = 99;
}

[Serializable]
public class Item
{
    public Guid Id { get; }
    public ItemDef Def { get; }
    public int Quantity { get; set; }

    public Item(Guid id, ItemDef def, int qty)
    {
        Id = id;
        Def = def;
        Quantity = qty;
    }
}