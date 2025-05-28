using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateSystemUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform crateUIContents;
    [SerializeField] private GameObject crateUIPrefab;
    [SerializeField] private CrateOpeningUI opener;

    void Start()
    {
        PopulateUI();
    }

    /// <summary>
    /// Handles all UI instantiation and wiring of button callbacks.
    /// </summary>
    private void PopulateUI()
    {
        foreach (var crate in HorseMarketDatabase.Instance._allCrates)
        {
            GameObject go = Instantiate(crateUIPrefab, crateUIContents);
            var panel = go.GetComponent<CratePanelUI>();
            panel.InitCrateUI(crate);
            // When this button is clicked, call the logic in CrateSystem
            panel.OnClicked += OpenCrate;
        }
    }

    private void OpenCrate(CrateDef selectedCrate)
    {
        TierDef tier;
        List<(WeightedTier tier, int weight)> values;
        
        // We should check if true (if we have enough money to open it, TODO: Implement)
        (tier, values) = CrateSystem.OpenCrate(selectedCrate);
        opener.StartSpin(tier, values);
    }
}
