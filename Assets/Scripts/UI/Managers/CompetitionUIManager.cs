using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CompetitionUIManager : MonoBehaviour
{
    public CompetitionLobbyUI competitionLobby;
    [Header("UI Panels")]
    public List<CompetitionUIPanel> competitionPanels;

    [Header("Tier Selector UI")]
    public TMP_Dropdown tierDropdown;
    public TMP_Dropdown leagueDropdown;
    public List<Color> leagueColors;

    private List<TierDef> allTiers;
    private TierDef selectedTier;
    private float leagueModifier;

    private void Start()
    {
        // Load all tiers from the database
        allTiers = HorseMarketDatabase.Instance._allTiers;

        // Prepare dropdown to support rich text
        if (tierDropdown.captionText != null)
            tierDropdown.captionText.richText = true;

        if (leagueDropdown.captionText != null)
            leagueDropdown.captionText.richText = true;

        // Populate the dropdown with tier names colored by highlight color
        tierDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var tier in allTiers)
        {
            // Convert color to hex
            string hex = UnityEngine.ColorUtility.ToHtmlStringRGB(tier.HighlightColor);
            // Wrap the name in a rich-text color tag
            string coloredName = $"<color=#{hex}>{tier.TierName}</color>";
            options.Add(new TMP_Dropdown.OptionData(coloredName));
        }
        tierDropdown.AddOptions(options);

        leagueDropdown.ClearOptions();
        var leagueOptions = new List<TMP_Dropdown.OptionData>();
        for(int i = 0; i < 5; i++)
        {
            string entryName = "";
            if (i == 0)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Novice</color>";
            }
            if (i == 1)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Amateur</color>";
            }
            if (i == 2)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Trained</color>";
            }
            if (i == 3)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Seasoned</color>";
            }
            if (i == 4)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Champion</color>";
            }

            leagueOptions.Add(new TMP_Dropdown.OptionData(entryName));
        }
        leagueDropdown.AddOptions(leagueOptions);

        // Select first tier by default
        tierDropdown.value = 0;
        selectedTier = allTiers[0];

        leagueDropdown.value = 0;
        leagueModifier = 0.1f;

        tierDropdown.onValueChanged.RemoveAllListeners();
        tierDropdown.onValueChanged.AddListener(OnTierChanged);

        leagueDropdown.onValueChanged.RemoveAllListeners();
        leagueDropdown.onValueChanged.AddListener(OnLeagueChanged);

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

    private void OnLeagueChanged(int index)
    {
        if (index == 0)
            leagueModifier = 0.1f;
        if (index == 1)
            leagueModifier = 0.25f;
        if (index == 2)
            leagueModifier = 0.5f;
        if (index == 3)
            leagueModifier = 0.75f;
        if (index == 4)
            leagueModifier = 1f;

        RefreshUI();
    }

    public void RefreshUI()
    {
        var comps = HorseMarketDatabase.Instance._allCompetitions;
        int count = Mathf.Min(comps.Count, competitionPanels.Count);
        for (int i = 0; i < count; i++)
            competitionPanels[i].InitUI(comps[i], selectedTier, leagueModifier);
        // Hide extra panels
        for (int i = count; i < competitionPanels.Count; i++)
            competitionPanels[i].gameObject.SetActive(false);
    }

    public void OpenCompetitionLobby(CompetitionDef competition)
    {
        competitionLobby.gameObject.SetActive(true);
        competitionLobby.InitUI(competition, selectedTier, leagueModifier);
    }
}
