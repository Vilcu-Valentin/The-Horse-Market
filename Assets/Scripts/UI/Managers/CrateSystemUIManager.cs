using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateSystemUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CrateSystem crateSystem;
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
        foreach (var crate in crateSystem.crates)
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
        (tier, values) = crateSystem.OpenCrate(selectedCrate);
        opener.StartSpin(tier, values);
    }
}
