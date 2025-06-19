using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CompetitionUIPanel : MonoBehaviour
{
    [Header("General")]
    public Image competitionIcon;
    public Image IconBackground;
    public Image background;
    public Image foreground;

    public TMP_Text competitionName;
    public TMP_Text entryFee;

    public Button competeButton;

    [Header("Information")]
    public Image infoBackground;

    public TMP_Text speedImportance;
    public TMP_Text staminaImportance;
    public TMP_Text jumpImportance;
    public TMP_Text strengthImportance;

    public Color highImportance;
    public Color lowImportance;
    public Color balancedImportance;
    public Color noImportance;

    public List<TMP_Text> ranks;

    public Action<CompetitionDef> OnCompeteClicked;

    public void InitUI(CompetitionDef competition, TierDef selectedTier, float leagueModifier)
    {
        competitionIcon.sprite = competition.Icon;
        background.color = competition.backgroundColor;
        infoBackground.color = competition.foregroundColor;
        foreground.color = competition.foregroundColor;
        IconBackground.color = competition.backgroundColor;

        competeButton.onClick.RemoveAllListeners();
        competeButton.onClick.AddListener(() =>
        {
            OnCompeteClicked?.Invoke(competition);
        });

        competitionName.text = competition.CompetitionName;
        var settings = competition.GetSettingsFor(selectedTier);
        entryFee.text = settings.entryFee.ToShortString();

        // Payout ranks
        for (int i = 0; i < settings.placeRewards.Count && i < ranks.Count; i++)
            if (settings.placeRewards[i].emeralds > 0)
            {
                long modifiedReward = Mathf.FloorToInt(settings.placeRewards[i].emeralds * leagueModifier);
                ranks[i].text = modifiedReward.ToShortString(); 
            }
            else
                ranks[i].text = "-";

        // Determine stat importance
        var statList = competition.competitionStats;
        int statCount = statList.Count;
        float threshold = (statCount == 4) ? 0.26f : 0.5f;

        // Helper to set each stat label
        void SetImportance(TMP_Text label, StatType statType)
        {
            // Find weight, default 0
            float weight = statList.Where(s => s.Stat == statType)
                                   .Select(s => s.weight)
                                   .FirstOrDefault();
            if (weight <= 0f)
            {
                label.text = "Negligible";
                label.color = noImportance;
            }
            else if (statCount == 4)
            {
                // Four-way: high vs balanced
                if (weight > threshold)
                {
                    label.text = "High";
                    label.color = highImportance;
                }
                else
                {
                    label.text = "Balanced";
                    label.color = balancedImportance;
                }
            }
            else
            {
                // 1-3 stats: high vs low
                if (weight > threshold)
                {
                    label.text = "High";
                    label.color = highImportance;
                }
                else
                {
                    label.text = "Low";
                    label.color = lowImportance;
                }
            }
        }

        // Apply to each stat field
        SetImportance(speedImportance, StatType.Speed);
        SetImportance(staminaImportance, StatType.Stamina);
        SetImportance(jumpImportance, StatType.JumpHeight);
        SetImportance(strengthImportance, StatType.Strength);
    }
}