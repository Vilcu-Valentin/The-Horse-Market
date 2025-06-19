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

    private TierDef selectedTier;
    private List<HorseAI> aiHorses;
    private CompetitionDef competition;
    private float leagueModifier;

    public void InitUI(CompetitionDef competition, TierDef selectedTier, float leagueModifier)
    {
        this.selectedTier = selectedTier;
        this.competition = competition;
        this.leagueModifier = leagueModifier;
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

        for(int i = 0; i < competitorsName.Count; i++)
        {
            HorseAI generateHorse = CompetitionSystem.GenerateAIHorse(selectedTier, competition.GetSettingsFor(selectedTier).difficulty, leagueModifier);
            aiHorses.Add(generateHorse);

            competitorsName[i].text = generateHorse.horseName;
            competitorRating[i].text = "-";
            competitorRating[i].color = ratingColors[2];
        }
    }

    private void OpenInventory()
    {
        inventoryManager.OpenForSelecting(AddParent, selectedTier);
    }

    public void AddParent(Horse horse)
    {
        this.horse = horse;

        selectedHorsePanel.gameObject.SetActive(true);
        selectHorseButton.gameObject.SetActive(false);

        horseName.text = horse.horseName;
        horseTier.text = horse.Tier.TierName;
        horseTier.color = horse.Tier.HighlightColor;
        horseVisual.sprite = horse.Visual.sprite2D;

        long powerIndex = CompetitionSystem.CalculateHorseIndex(horse, competition);
        horsePowerIndex.text = powerIndex.ToShortString();
        for (int i = 0; i < competitorsName.Count; i++)
        {
            string rating;
            int colorIndex;
            (rating, colorIndex) = CompetitionSystem.GetHorseRating(powerIndex, CompetitionSystem.CalculateHorseIndex(aiHorses[i], competition));
            competitorRating[i].text = rating;
            competitorRating[i].color = ratingColors[colorIndex];
        }

        startCompetition.interactable = true;
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

        startCompetition.interactable = false;
    }

    public void StartCompetition()
    {
        if (EconomySystem.Instance.EnoughEmeralds(competition.GetSettingsFor(selectedTier).entryFee))
        {
            EconomySystem.Instance.RemoveEmeralds(competition.GetSettingsFor(selectedTier).entryFee);
            gameObject.SetActive(false);
            competitionWinners.gameObject.SetActive(true);
            competitionWinners.InitUI(horse, aiHorses, competition, leagueModifier);
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
