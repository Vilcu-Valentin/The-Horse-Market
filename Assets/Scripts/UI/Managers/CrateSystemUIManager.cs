using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CrateSystemUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform crateUIContents;
    [SerializeField] private GameObject crateUIPrefab;
    [SerializeField] private CrateOpeningUI opener;
    [SerializeField] private GameObject horseInfoPanel;

    public CrateInfoUIPanel crateInfoPanel;

    // Keep a reference so we can unsubscribe later:
    private Action _onFinishedHandler;

    void Start()
    {
        PopulateUI();
    }

    /// <summary>
    /// Handles all UI instantiation and wiring of button callbacks.
    /// </summary>
    public void PopulateUI()
    {
        foreach (Transform child in crateUIContents)
            Destroy(child.gameObject);

        foreach (var crate in HorseMarketDatabase.Instance._allCrates)
        {
            if (crate.minHorseTier.TierIndex <= SaveSystem.Instance.Current.GetHighestHorseTier())
            {
                GameObject go = Instantiate(crateUIPrefab, crateUIContents);
                var panel = go.GetComponent<CratePanelUI>();
                panel.InitCrateUI(crate);
                // When this button is clicked, call the logic in CrateSystem
                panel.OnClicked += OpenCrate;
                panel.OnInfoClicked += OpenCrateInfo;
            }
        }
    }

    private void OpenCrate(CrateDef selectedCrate)
    {
        if (EconomySystem.Instance.RemoveEmeralds(selectedCrate.CostInEmeralds))
        {
            Horse horse;
            List<(WeightedTier tier, int weight)> values;
            (horse, values) = CrateSystem.OpenCrate(selectedCrate);

            // Create a one‐time handler that "captures" `horse`:
            _onFinishedHandler = () =>
            {
                ShowHorseInfo(horse);
                opener.OpeningFinished -= _onFinishedHandler;
            };

            opener.OpeningFinished += _onFinishedHandler;

            opener.StartSpin(horse.Tier, values);
        }
    }

    private void ShowHorseInfo(Horse openedHorse)
    {
        horseInfoPanel.SetActive(true);
        horseInfoPanel.GetComponent<HorseInfoPanelUI>().HorseUIInit(openedHorse, true);
    }

    private void OpenCrateInfo(CrateDef crate)
    {
        crateInfoPanel.gameObject.SetActive(true);
        crateInfoPanel.InitUI(crate);
    }
}
