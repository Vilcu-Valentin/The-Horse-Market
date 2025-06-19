using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Player Resources")]
    public long emeralds;

    [Header("Owned Horses")]
    public List<Horse> horses = new List<Horse>();

    // Lookup table for quick ID-based access
    private Dictionary<Guid, Horse> _lookup = new Dictionary<Guid, Horse>();

    private void OnEnable() => RebuildLookup();
    private void OnValidate() => RebuildLookup();

    /// <summary>
    /// Get a horse by its GUID.
    /// </summary>
    public Horse GetHorse(Guid id) => _lookup.TryGetValue(id, out var h) ? h : null;

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
    }
}
