using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompetitionUIManager : MonoBehaviour
{
    public CompetitionLobbyUI competitionLobby;
    [Header("UI Panels")]
    public List<CompetitionUIPanel> competitionPanels;

    [Header("Tier Selector UI")]
    public TMP_Dropdown tierDropdown;

    private List<TierDef> allTiers;
    private TierDef selectedTier;

    private void Start()
    {
        // Load all tiers from the database
        allTiers = HorseMarketDatabase.Instance._allTiers;

        // Prepare dropdown to support rich text
        if (tierDropdown.captionText != null)
            tierDropdown.captionText.richText = true;

        // Populate the dropdown with tier names colored by highlight color
        tierDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var tier in allTiers)
        {
            // Convert color to hex
            string hex = ColorUtility.ToHtmlStringRGB(tier.HighlightColor);
            // Wrap the name in a rich-text color tag
            string coloredName = $"<color=#{hex}>{tier.TierName}</color>";
            options.Add(new TMP_Dropdown.OptionData(coloredName));
        }
        tierDropdown.AddOptions(options);

        // Select first tier by default
        tierDropdown.value = 0;
        selectedTier = allTiers[0];

        tierDropdown.onValueChanged.RemoveAllListeners();
        tierDropdown.onValueChanged.AddListener(OnTierChanged);

        RefreshUI();

        foreach(var uiPanel in  competitionPanels)
        {
            uiPanel.OnCompeteClicked += OpenCompetitionLobby;
        }
    }

    private void OnTierChanged(int index)
    {
        selectedTier = allTiers[index];
        RefreshUI();
    }

    public void RefreshUI()
    {
        var comps = HorseMarketDatabase.Instance._allCompetitions;
        int count = Mathf.Min(comps.Count, competitionPanels.Count);
        for (int i = 0; i < count; i++)
            competitionPanels[i].InitUI(comps[i], selectedTier);
        // Hide extra panels
        for (int i = count; i < competitionPanels.Count; i++)
            competitionPanels[i].gameObject.SetActive(false);
    }

    public void OpenCompetitionLobby(CompetitionDef competition)
    {
        competitionLobby.gameObject.SetActive(true);
        competitionLobby.InitUI(competition, selectedTier);
    }
}
