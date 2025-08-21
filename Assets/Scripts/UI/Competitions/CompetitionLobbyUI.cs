using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class CompetitionLobbyUI : MonoBehaviour
{
    public TMP_Text competitionName;

    public InventoryUIManager inventoryManager;
    public Button startCompetition;

    public CompetitionWinnersUI competitionWinners;

    [Header("Horse")]
    public Button selectHorseButton;
    public TMP_Text horsePowerIndex;

    public GameObject selectedHorsePanel;
    public TMP_Text horseName;
    public TMP_Text horseTier;
    public Image horseVisual;
    public Button removeHorseButton;
    private Horse horse = null;

    [Header("Competition")]
    public List<TMP_Text> competitorsName;
    public List<TMP_Text> competitorRating;
    [Tooltip("We need exactly 5 colors, first one is for superior, last one is for outclassed")]
    public List<Color> ratingColors=  new List<Color>(5);

    public DialogPanelUI notEnoughEmeraldsDialog;

    [Header("RewardsInfo")]
    public TMP_Text entryFee;
    public List<TMP_Text> emeraldReward;
    public List<TMP_Text> itemReward;

    [Header("Tier Selector UI")]
    public TMP_Dropdown tierDropdown;
    public TMP_Dropdown leagueDropdown;
    public List<Color> leagueColors;

    private List<TierDef> allTiers;
    private TierDef selectedTier;
    private float leagueModifier;

    private List<HorseAI> aiHorses;
    private CompetitionDef competition;

    private Horse[] previousHorses = new Horse[9]; // index 0 unused

    private void TryAddPreviousHorse()
    {
        if (previousHorses[selectedTier.TierIndex] != null)
        {
            Horse horse = previousHorses[selectedTier.TierIndex];
            if (SaveSystem.Instance.Current.GetHorse(horse.Id) != null)
                if (horse.CanCompete())
                    AddHorse(horse);
        }
    }

    private void OnTierChanged(int index)
    {
        selectedTier = allTiers[index];
        entryFee.text = competition.GetSettingsFor(selectedTier).entryFee.ToShortString();
        RemoveHorse();
        RefreshUI();
        TryAddPreviousHorse();
    }
    private void OnLeagueChanged(int index)
    {
        if (index == 0)
            leagueModifier = 0.05f;
        if (index == 1)
            leagueModifier = 0.2f;
        if (index == 2)
            leagueModifier = 0.45f;
        if (index == 3)
            leagueModifier = 0.7f;
        if (index == 4)
            leagueModifier = 1f;
        if (index == 5)
            leagueModifier = 1.25f;
        if (index == 6)
            leagueModifier = 2f;

        if(horse != null)
        {
            UpdateRatings(horse);
        }
        else
        {
            for (int i = 0; i < competitorsName.Count; i++)
            {
                competitorsName[i].text = "-";
                competitorRating[i].text = "-";
                competitorRating[i].color = ratingColors[2];
            }
        }
        RefreshUI();
    }

    private void Awake()
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
        for (int i = 0; i < 7; i++)
        {
            string entryName = "";
            if (i == 0)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Novice</color>";
            }
            if (i == 1)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Rookie</color>";
            }
            if (i == 2)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Amateur</color>";
            }
            if (i == 3)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Skilled</color>";
            }
            if (i == 4)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Trained</color>";
            }
            if (i == 5)
            {
                entryName = $"<color=#{leagueColors[i].ToHexString()}>Seasoned</color>";
            }
            if (i == 6)
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
        leagueModifier = 0.05f;

        tierDropdown.onValueChanged.RemoveAllListeners();
        tierDropdown.onValueChanged.AddListener(OnTierChanged);

        leagueDropdown.onValueChanged.RemoveAllListeners();
        leagueDropdown.onValueChanged.AddListener(OnLeagueChanged);
    }

    public void InitUI(CompetitionDef competition)
    {
        this.competition = competition;
        competitionName.text = competition.CompetitionName;

        startCompetition.onClick.RemoveAllListeners();

        horse = null;
        aiHorses = new List<HorseAI>();

        startCompetition.interactable = false;

        selectHorseButton.gameObject.SetActive(true);
        selectedHorsePanel.gameObject.SetActive(false);

        selectHorseButton.onClick.RemoveAllListeners();
        selectHorseButton.onClick.AddListener(OpenInventory);

        removeHorseButton.onClick.RemoveAllListeners();
        removeHorseButton.onClick.AddListener(RemoveHorse);

        startCompetition.onClick.RemoveAllListeners();
        startCompetition.onClick.AddListener(StartCompetition);

        horsePowerIndex.text = "-";

        entryFee.text = competition.GetSettingsFor(selectedTier).entryFee.ToShortString();

        for(int i = 0; i < competitorsName.Count; i++)
        {
            competitorsName[i].text = "-";
            competitorRating[i].text = "-";
            competitorRating[i].color = ratingColors[2];
        }

        RefreshUI();
        TryAddPreviousHorse();
    }

    public void RefreshUI()
    {
        var settings = competition.GetSettingsFor(selectedTier);

       entryFee.text = settings.entryFee.ToShortString();

        // Payout ranks
        for (int i = 0; i < settings.placeRewards.Count && i < emeraldReward.Count; i++)
        {
            long modifiedReward = Mathf.FloorToInt(settings.placeRewards[i].emeralds * CalculateRewardMultiplier(leagueModifier));
            if (modifiedReward < 1)
                modifiedReward = 0;
            if (modifiedReward >= 1)
            {
                modifiedReward = modifiedReward.RoundToAdaptiveStep();
                emeraldReward[i].text = modifiedReward.ToShortString();
            }
            else
                emeraldReward[i].text = "-";
        }

        for(int i = 0; i < settings.placeRewards.Count && i < itemReward.Count; i++)
        {
            int modifiedReward = Mathf.RoundToInt(settings.placeRewards[i].itemNumber * CalculateRewardMultiplier(leagueModifier));
            if (modifiedReward < 1)
                modifiedReward = 0;
            if(modifiedReward >= 1)
            {
                itemReward[i].text = ((long)(modifiedReward)).ToShortString();
            }
            else
            {
                itemReward[i].text = "-";
            }
        }
    }

    private void OpenInventory()
    {
        inventoryManager.OpenForSelecting(AddHorse, selectedTier, openForCompetitions: true);
    }

    public void AddHorse(Horse horse)
    {
        this.horse = horse;
        previousHorses[horse.Tier.TierIndex] = horse;

        selectedHorsePanel.gameObject.SetActive(true);
        selectHorseButton.gameObject.SetActive(false);

        horseName.text = horse.horseName;
        horseTier.text = horse.Tier.TierName;
        horseTier.color = horse.Tier.HighlightColor;
        horseVisual.sprite = horse.Visual.sprite2D;

        UpdateRatings(horse);

        startCompetition.interactable = true;
    }

    private void UpdateRatings(Horse horse)
    {
        long powerIndex = CompetitionSystem.CalculateHorseIndex(horse, competition);
        horsePowerIndex.text = powerIndex.ToShortString();

        aiHorses.Clear();
        for (int i = 0; i < competitorsName.Count; i++)
        {
            HorseAI generateHorse = CompetitionSystem.GenerateAIHorse(horse, competition.GetSettingsFor(selectedTier).difficulty, leagueModifier);
            aiHorses.Add(generateHorse);

            competitorsName[i].text = aiHorses[i].horseName;

            string rating;
            int colorIndex;
            (rating, colorIndex) = CompetitionSystem.GetHorseRating(powerIndex, CompetitionSystem.CalculateHorseIndex(aiHorses[i], competition));
            competitorRating[i].text = rating;
            competitorRating[i].color = ratingColors[colorIndex];
        }
    }

    public void RemoveHorse()
    {
        horse = null;

        selectedHorsePanel.gameObject.SetActive(false);
        selectHorseButton.gameObject.SetActive(true);

        for (int i = 0; i < competitorsName.Count; i++)
        {
            competitorRating[i].text = "-";
            competitorRating[i].color = ratingColors[2];
        }

        horsePowerIndex.text = "-";

        for (int i = 0; i < competitorsName.Count; i++)
        {
            competitorsName[i].text = "-";
            competitorRating[i].text = "-";
            competitorRating[i].color = ratingColors[2];
        }

        startCompetition.interactable = false;
    }

    private float CalculateRewardMultiplier(float leagueModifier)
    {
        const float T = 2f;         // endpoint
        const float k = 1.5f;       // “how exponential” you want
        const float m = -1.1f;      // start point modifier

        // if you really want the k→0 limit to be linear:
        if (Mathf.Approximately(k, 0f))
            return leagueModifier;

        float num = Mathf.Exp(k * leagueModifier) - m;
        float den = Mathf.Exp(k * T) - m;
        return T * (num / den);
    }


    public void StartCompetition()
    {
        if (EconomySystem.Instance.EnoughEmeralds(competition.GetSettingsFor(selectedTier).entryFee))
        {
            EconomySystem.Instance.RemoveEmeralds(competition.GetSettingsFor(selectedTier).entryFee);
            gameObject.SetActive(false);
            competitionWinners.gameObject.SetActive(true);

            competitionWinners.InitUI(horse, aiHorses, competition, CalculateRewardMultiplier(leagueModifier));
        }
        else
        {
            notEnoughEmeraldsDialog.Show(
    $"You don't have enough emeralds to enter the competition!",
    () =>
    {
    },
    () => {
    }
);
        }
    }
}
