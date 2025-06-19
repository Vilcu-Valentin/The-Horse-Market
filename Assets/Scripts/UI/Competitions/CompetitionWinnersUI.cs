using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using Unity.Loading;

public class CompetitionWinnersUI : MonoBehaviour
{
    public TMP_Text competitionName;

    [Header("Rankings")]
    public Color playerHorseNameColor;
    public Color nameBaseColor;
    public List<TMP_Text> horseNames;
    public List<TMP_Text> horseRatings;
    public List<TMP_Text> horseRewards;

    [Tooltip("We need exactly 5 colors, first one is for superior, last one is for outclassed")]
    public List<Color> ratingColors = new List<Color>(5);

    public Button continueButton;

    private long _emeraldsReward;

    public void InitUI(Horse horse, List<HorseAI> aiHorses, CompetitionDef competition, float rewardModifier)
    {
        competitionName.text = competition.CompetitionName + " Result";

        List<int> compIndexes;
        compIndexes = CompetitionSystem.CalculateOutcome(horse, aiHorses, competition);

        for(int i = 0; i < compIndexes.Count; i++)
        {
            horseNames[i].color = nameBaseColor;
            long emeraldsReward = Mathf.FloorToInt(competition.GetSettingsFor(horse.Tier).placeRewards[i].emeralds * rewardModifier);
            if (compIndexes[i] == 7)
            {
                horseNames[i].color = playerHorseNameColor;
                horseNames[i].text = horse.horseName;
                horseRatings[i].text = "-";
                horseRatings[i].color = ratingColors[2];
                _emeraldsReward = emeraldsReward;
            }
            else
            {
                int colorIndex;
                string ratingStr;

                horseNames[i].text = aiHorses[compIndexes[i]].horseName;
                (ratingStr, colorIndex) = CompetitionSystem.GetHorseRating(CompetitionSystem.CalculateHorseIndex(horse, competition),
    CompetitionSystem.CalculateHorseIndex(aiHorses[compIndexes[i]], competition));
                horseRatings[i].text = ratingStr;
                horseRatings[i].color = ratingColors[colorIndex];
            }

            horseRewards[i].text = emeraldsReward.ToShortString();
        }

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            ClaimReward(horse);
        });
    }

    public void ClaimReward(Horse horse)
    {
        if(_emeraldsReward > 0)
            EconomySystem.Instance.AddEmeralds(_emeraldsReward);
        gameObject.SetActive(false);
        horse.RefillEnergy();
        horse.Compete();
        SaveSystem.Instance.Current.UpdateHorseCompetition(horse);
    }
}
