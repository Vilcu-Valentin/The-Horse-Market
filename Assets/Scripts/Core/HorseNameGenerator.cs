using System;
using UnityEngine;

public static class HorseNameGenerator
{
    // List of single-part horse names
    private static readonly string[] SingleNames =
    {
        "Caravaggio",
        "Quincy",
        "Clover",
        "Majesty",
        "Zephyr",
        "Aurora",
        "Stella",
        "Bandit",
        "Echo",
        "Harmony"
        // add more single names here
    };

    // Word lists for combined names
    private static readonly string[] CombinedFirstParts =
    {
        "Night",
        "Wind",
        "Iron",
        "Star",
        "Silver",
        "Fire",
        "River",
        "Shadow",
        "Golden",
        "Storm"
        // add more first parts here
    };

    private static readonly string[] CombinedSecondParts =
    {
        "Rider",
        "Fever",
        "Whisper",
        "Flame",
        "Runner",
        "Spirit",
        "Legend",
        "Dream",
        "Chaser",
        "Dancer"
        // add more second parts here
    };

    // Syllable pools for on-the-fly generation
    private static readonly string[] SyllableStarts =
    {
        "ba", "ka", "le", "sa", "ri", "mo", "ta", "za", "ia", "no"
        // add more possible starting syllables
    };

    private static readonly string[] SyllableMiddles =
    {
        "lo", "ri", "na", "ve", "ra", "li", "ta", "ne", "on", "ar"
        // add more possible middle syllables
    };

    private static readonly string[] SyllableEnds =
    {
        "na", "ra", "lio", "ko", "mi", "ti", "la", "do", "ra", "pha"
        // add more possible ending syllables
    };

    /// <summary>
    /// Returns a random horse name: single, combined, or syllable-based.
    /// </summary>
    public static string GetRandomHorseName()
    {
        float roll = UnityEngine.Random.value;
        if (roll < 0.5f)
            return GetSingleName();       // 50% single-part
        else if (roll < 0.8f)
            return GetCombinedName();     // 30% combined
        else
            return GetSyllableName(UnityEngine.Random.Range(2, 4)); // 20% syllable-based (2–3 syllables)
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
