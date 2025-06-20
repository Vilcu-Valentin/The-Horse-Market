using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Player Resources")]
    public long emeralds;

    [Header("Owned Horses")]
    public List<Horse> horses = new List<Horse>();

    [Header("Owned Items")]
    public List<Item> items = new List<Item>();

    // Lookup table for quick ID-based access
    private Dictionary<Guid, Horse> _lookup = new Dictionary<Guid, Horse>();
    private Dictionary<Guid, Item> _lookupItem = new Dictionary<Guid, Item>();

    private void OnEnable() => RebuildLookup();
    private void OnValidate() => RebuildLookup();

    /// <summary>
    /// Get a horse by its GUID.
    /// </summary>
    public Horse GetHorse(Guid id) => _lookup.TryGetValue(id, out var h) ? h : null;
    public Item GetItem(Guid id) => _lookupItem.TryGetValue(id, out var i) ? i : null; 

    /// <summary>
    /// Returns the highest tier index among owned horses (minimum 1).
    /// </summary>
    public int GetHighestHorseTier()
    {
        int maxTier = 1;
        foreach (var horse in horses)
        {
            if (horse != null && horse.Tier != null && horse.Tier.TierIndex > maxTier)
                maxTier = horse.Tier.TierIndex;
        }
        return maxTier;
    }

    /// <summary>
    /// Add a new horse and update lookup.
    /// </summary>
    public void AddHorse(Horse horse)
    {
        if (horse == null) return;
        horses.Add(horse);
        RebuildLookup();
    }

    /// <summary>
    /// Remove an existing horse and update lookup.
    /// </summary>
    public void RemoveHorse(Horse horse)
    {
        if (horse == null) return;
        horses.Remove(horse);
        RebuildLookup();
    }

    /// <summary>
    /// Adds quantity of an item. 
    /// If an item with the same definition already exists, just increments its quantity.
    /// Otherwise creates a new Item with that quantity.
    /// </summary>
    public void AddItem(ItemDef def, int quantity = 1)
    {
        if (def == null || quantity <= 0) return;

        // see if there’s already an Item with this Def
        var existing = items.FirstOrDefault(i => i.Def.ID == def.ID);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            var newItem = new Item(Guid.NewGuid(), def, quantity);
            items.Add(newItem);
        }

        RebuildLookup();
    }

    /// <summary>
    /// Removes up to `quantity` from the stack. 
    /// If quantity goes to zero or below, removes the Item entirely.
    /// </summary>
    public void RemoveItem(ItemDef def, int quantity = 1)
    {
        if (def == null || quantity <= 0) return;

        var existing = items.FirstOrDefault(i => i.Def.ID == def.ID);
        if (existing == null) return;

        existing.Quantity -= quantity;
        if (existing.Quantity <= 0)
            items.Remove(existing);

        RebuildLookup();
    }


    /// <summary>
    /// Updates the reamining competitions of all the horses that didn't participate
    /// </summary>
    /// <param name="horse">The horse that participated</param>
    public void UpdateHorseCompetition(Horse horse)
    {
        foreach(var _horse in horses)
        {
            if (_horse != horse)
                _horse.RefillCompetitions();
        }
    }

    /// <summary>
    /// Rebuild the GUID->Horse lookup table.
    /// </summary>
    public void RebuildLookup()
    {
        _lookup.Clear();
        foreach (var horse in horses)
        {
            if (horse != null)
                _lookup[horse.Id] = horse;
        }

        _lookupItem.Clear();
        foreach (var item in items)
            if (item != null)
                _lookupItem[item.Id] = item;
    }
}
