using System;
using UnityEngine;

public static class HorseNameGenerator
{
    // List of single-part horse names (no color references)
    private static readonly string[] SingleNames =
    {
        "Caravaggio", "Quincy", "Clover", "Majesty", "Zephyr", "Aurora", "Stella", "Bandit", "Echo", "Harmony",
        "Comet", "Eclipse", "Phoenix", "Mystic", "Rogue", "Valor", "Reign", "Thunder", "Spirit", "Legacy",
        "Valiant", "Quest", "Ranger", "Summit", "Noble", "Arrow", "Bravo", "Caden", "Dante", "Enzo",
        "Finn", "Gatsby", "Hunter", "Jaxon", "Kael", "Leo", "Maverick", "Nico", "Orion", "Pax",
        "Quinn", "Ryder", "Silas", "Titan", "Ursus", "Zenith", "Atlas", "Apollo", "Hercules", "Achilles",
        "Pegasus", "Nimbus", "Everest", "Azrael", "Sirocco", "Cyclone", "Vortex", "Torrent", "Maestro", "Odyssey"
        // (Feel free to add more single names here)
    };

    // Word lists for combined names (no color references)
    private static readonly string[] CombinedFirstParts =
    {
        "Night", "Wind", "Iron", "Star", "Fire", "River", "Shadow", "Storm", "Thunder", "Eclipse",
        "Apex", "Canyon", "Destiny", "Echo", "Falcon", "Gale", "Horizon", "Legend", "Meridian", "Nova",
        "Odyssey", "Phoenix", "Quest", "Ranger", "Summit", "Titan", "Valor", "Whisper", "Zenith", "Cascade"
        // (Feel free to add more first parts here)
    };

    private static readonly string[] CombinedSecondParts =
    {
        "Rider", "Whisper", "Flame", "Runner", "Spirit", "Legend", "Dream", "Chaser", "Dancer", "Strider",
        "Seeker", "Voyager", "Wanderer", "Keeper", "Bound", "Caller", "Breaker", "Charger", "Drifter", "Fighter",
        "Guardian", "Hunter", "Jester", "Maker", "Nomad", "Oracle", "Pioneer", "Raider", "Sentinel", "Tracker"
        // (Feel free to add more second parts here)
    };

    // Syllable pools for on-the-fly generation (expanded)
    private static readonly string[] SyllableStarts =
    {
        "ba", "ka", "le", "sa", "ri", "mo", "ta", "za", "ia", "no",
        "da", "fa", "ha", "jo", "lu", "me", "ni", "pa", "ra", "si"
        // (Feel free to add more possible starting syllables)
    };

    private static readonly string[] SyllableMiddles =
    {
        "lo", "ri", "na", "ve", "ra", "li", "ta", "ne", "on", "ar",
        "di", "mi", "sa", "te", "ul", "van", "zen", "cor", "bel", "dor"
        // (Feel free to add more possible middle syllables)
    };

    private static readonly string[] SyllableEnds =
    {
        "na", "ra", "lio", "ko", "mi", "ti", "la", "do", "pha", "us",
        "ix", "os", "um", "en", "et", "yl", "al", "or", "cy", "ia"
        // (Feel free to add more possible ending syllables)
    };

    /// <summary>
    /// Returns a random horse name: single, combined, or syllable-based.
    /// </summary>
    public static string GetRandomHorseName()
    {
        float roll = UnityEngine.Random.value;
        if (roll < 0.5f)
            return GetSingleName();       // 50% chance: single-part name
        else if (roll < 0.8f)
            return GetCombinedName();     // 30% chance: combined name
        else
            return GetSyllableName(UnityEngine.Random.Range(2, 4)); // 20% chance: 2–3 syllable name
    }

    private static string GetSingleName()
    {
        int idx = UnityEngine.Random.Range(0, SingleNames.Length);
        return SingleNames[idx];
    }

    private static string GetCombinedName()
    {
        string first = CombinedFirstParts[UnityEngine.Random.Range(0, CombinedFirstParts.Length)];
        string second = CombinedSecondParts[UnityEngine.Random.Range(0, CombinedSecondParts.Length)];
        return first + " " + second;
    }

    /// <summary>
    /// Builds a name from random syllables.
    /// </summary>
    /// <param name="syllableCount">Number of syllables (min 2).</param>
    private static string GetSyllableName(int syllableCount)
    {
        syllableCount = Mathf.Max(2, syllableCount);
        // First syllable
        string name = SyllableStarts[UnityEngine.Random.Range(0, SyllableStarts.Length)];

        // Middle syllables
        for (int i = 1; i < syllableCount - 1; i++)
        {
            name += SyllableMiddles[UnityEngine.Random.Range(0, SyllableMiddles.Length)];
        }

        // Last syllable
        name += SyllableEnds[UnityEngine.Random.Range(0, SyllableEnds.Length)];

        // Capitalize
        return char.ToUpper(name[0]) + name.Substring(1);
    }
}
