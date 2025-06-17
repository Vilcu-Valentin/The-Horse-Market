// HorseMarketDatabase.cs
using UnityEngine;
using System.Collections.Generic;

public class HorseMarketDatabase : MonoBehaviour
{
    public List<VisualDef> _allVisuals;
    public List<TierDef> _allTiers;
    public List<TraitDef> _allTraits;
    public List<CrateDef> _allCrates;
    public List<CompetitionDef> _allCompetitions;

    public static HorseMarketDatabase Instance { get; private set; }

    private Dictionary<string, TierDef> tierMap;
    private Dictionary<string, VisualDef> visualMap;
    private Dictionary<string, TraitDef> traitMap;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        tierMap = new Dictionary<string, TierDef>();
        visualMap = new Dictionary<string, VisualDef>();
        traitMap = new Dictionary<string, TraitDef>();

        foreach (var t in _allTiers) tierMap[t.ID] = t;
        foreach (var v in _allVisuals) visualMap[v.ID] = v;
        foreach (var t in _allTraits) traitMap[t.ID] = t;
    }

    public TierDef GetTier(string id) => tierMap.TryGetValue(id, out var t) ? t : null;
    public VisualDef GetVisual(string id) => visualMap.TryGetValue(id, out var v) ? v : null;
    public TraitDef GetTrait(string id) => traitMap.TryGetValue(id, out var tr) ? tr : null;
}
