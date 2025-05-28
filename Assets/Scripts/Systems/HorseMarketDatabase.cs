using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseMarketDatabase : MonoBehaviour
{
    public List<VisualDef> _allVisuals;
    public List<TierDef> _allTiers;
    public List<TraitDef> _allTraits;
    public List<CrateDef> _allCrates;

    public static HorseMarketDatabase Instance { get; private set; }

    private void Awake()
    {
        // Enforce the singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
