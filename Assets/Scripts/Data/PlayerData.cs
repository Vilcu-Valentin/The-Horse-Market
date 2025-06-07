using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/PlayerData")]
public class PlayerData : ScriptableObject
{
    public long emeralds;
    public List<Horse> horses = new();
    // public List<Item> inventory = new(); // Breeding items, maybe crates etc.

    // Utility - lool-up table for quick stable management
    private Dictionary<System.Guid, Horse> _lookup = new();

    public Horse GetHorse(System.Guid id)
        => _lookup.TryGetValue(id, out var horse) ? horse : null;

    private void OnEnable() => RebuildLookup();
    private void OnValidate() => RebuildLookup();

    public int GetHighestHorseTier()
    {
        int maxI = 0;
        foreach(Horse horse in horses)
        {
            if(horse.Tier.TierIndex > maxI)
                maxI = horse.Tier.TierIndex;
        }
        return maxI;
    }

    public void AddHorse(Horse horse)
    {
        horses.Add(horse);
        RebuildLookup();
    }

    public void RemoveHorse(Horse horse)
    {
        horses.Remove(horse);
        RebuildLookup();
    }

    private void RebuildLookup()
    {
        _lookup.Clear();
        foreach (Horse horse in horses) _lookup[horse.Id] = horse;
    }
}
