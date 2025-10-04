using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct UnlockedTraits
{
    public TraitDef definition;
    public bool unlocked;
}

[System.Serializable]
public struct UnlockedVisuals
{
    public VisualDef definition;
    public bool unlocked;
}

public class AlmanachSystem : MonoBehaviour
{
    public List<UnlockedTraits> unlockedTraits = new List<UnlockedTraits>();
    public List<UnlockedVisuals> unlockedVisuals = new List<UnlockedVisuals>();

    [Header("UI")]
    public TMP_Text visualButtonNumbers;
    public TMP_Text traitsButtonNumbers;

    public Transform visualsParent;
    public Transform traitsParent;

    public GameObject traitsEntryPrefab;
    public GameObject visualEntryPrefab;

    private Dictionary<string, AlmanachTraitsEntryUI> traitUIMap = new Dictionary<string, AlmanachTraitsEntryUI>();
    private Dictionary<string, AlmanachVisualEntryUI> visualUIMap = new Dictionary<string, AlmanachVisualEntryUI>();


    public static AlmanachSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Ensure the lists exist and are in a known state.
    /// </summary>
    public void SeedLists()
    {
        unlockedTraits.Clear();
        unlockedVisuals.Clear();

        if (HorseMarketDatabase.Instance != null)
        {
            foreach (var trait in HorseMarketDatabase.Instance._allTraits)
            {
                unlockedTraits.Add(new UnlockedTraits { definition = trait, unlocked = false });
            }

            foreach (var visual in HorseMarketDatabase.Instance._allVisuals)
            {
                unlockedVisuals.Add(new UnlockedVisuals { definition = visual, unlocked = false });
            }
        }
        else
        {
            Debug.LogWarning("HorseMarketDatabase.Instance is null in AlmanachSystem.SeedLists()");
        }
    }

    // Unlocks a trait if found in the list
    public void UnlockTrait(TraitDef trait)
    {
        for (int i = 0; i < unlockedTraits.Count; i++)
        {
            if (unlockedTraits[i].definition == trait && !unlockedTraits[i].unlocked)
            {
                var temp = unlockedTraits[i];
                temp.unlocked = true;
                unlockedTraits[i] = temp;

                Debug.Log($"Almanach: unlocked trait {trait.ID}");

                if (traitUIMap.TryGetValue(trait.ID, out var ui))
                {
                    ui.Discover();
                }
                break;
            }
        }
        int unlocked = 0;
        foreach (var v in unlockedTraits)
            if (v.unlocked)
                unlocked++;

        UpdateCounters();
    }

    public void UnlockVisual(VisualDef visual)
    {
        for (int i = 0; i < unlockedVisuals.Count; i++)
        {
            if (unlockedVisuals[i].definition == visual && !unlockedVisuals[i].unlocked)
            {
                var temp = unlockedVisuals[i];
                temp.unlocked = true;
                unlockedVisuals[i] = temp;

                Debug.Log($"Almanach: unlocked visual {visual.ID}");

                if (visualUIMap.TryGetValue(visual.ID, out var ui))
                {
                    ui.Discover();
                }
                break;
            }
        }

        UpdateCounters();
    }

    private void UpdateCounters()
    {
        int traitsUnlocked = unlockedTraits.FindAll(x => x.unlocked).Count;
        int visualsUnlocked = unlockedVisuals.FindAll(x => x.unlocked).Count;

        traitsButtonNumbers.text = $"TRAITS  {traitsUnlocked}/{unlockedTraits.Count}";
        visualButtonNumbers.text = $"VISUALS  {visualsUnlocked}/{unlockedVisuals.Count}";
    }



    public void Initialize(AlmanachDTO dto = null)
    {
        // First: reset all unlocks to false
        for (int i = 0; i < unlockedTraits.Count; i++)
        {
            var temp = unlockedTraits[i];
            temp.unlocked = false;
            unlockedTraits[i] = temp;
        }
        for (int i = 0; i < unlockedVisuals.Count; i++)
        {
            var temp = unlockedVisuals[i];
            temp.unlocked = false;
            unlockedVisuals[i] = temp;
        }

        // Create all UI entries (locked by default)
        foreach (var t in unlockedTraits)
        {
            var tUI = Instantiate(traitsEntryPrefab, traitsParent).GetComponent<AlmanachTraitsEntryUI>();
            tUI.InitUI(t.definition, t.unlocked);
            traitUIMap[t.definition.ID] = tUI;
        }

        foreach (var v in unlockedVisuals)
        {
            var vUI = Instantiate(visualEntryPrefab, visualsParent).GetComponent<AlmanachVisualEntryUI>();
            vUI.InitUI(v.definition, v.unlocked);
            visualUIMap[v.definition.ID] = vUI;
        }

        // If no saved data, leave everything locked but keep UI visible
        if (dto == null)
        {
            Debug.Log("Almanach: no saved data found, showing locked UI.");
            UpdateCounters();
            return;
        }

        // Apply saved unlocked traits
        foreach (var t in dto.unlockedTraits)
        {
            if (!t.unlocked) continue;

            var trait = unlockedTraits.Find(x => x.definition.ID == t.id);
            if (trait.definition != null)
                UnlockTrait(trait.definition);
        }

        // Apply saved unlocked visuals
        foreach (var v in dto.unlockedVisuals)
        {
            if (!v.unlocked) continue;

            var visual = unlockedVisuals.Find(x => x.definition.ID == v.id);
            if (visual.definition != null)
                UnlockVisual(visual.definition);
        }

        UpdateCounters();
        Debug.Log("Almanach: loaded saved data.");
    }



    /// <summary>
    /// Utility check if lists contain any entries (useful for quick checks).
    /// </summary>
    public bool HasSeededData() => unlockedTraits.Count > 0 || unlockedVisuals.Count > 0;
}
