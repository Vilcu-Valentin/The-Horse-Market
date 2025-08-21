using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }
    public List<ItemDef> _allItems;

    private Dictionary<string, ItemDef> map;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            map = _allItems.ToDictionary(d => d.ID, d => d);
        }
    }

    public ItemDef GetItemDef(string id) => map.TryGetValue(id, out var d) ? d : null;
}
