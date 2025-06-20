using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Item")]
public class ItemDef : ScriptableObject
{
    public string ID;

    public string ItemName;
    [TextArea]public string Description;
    public Sprite Icon;
}


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