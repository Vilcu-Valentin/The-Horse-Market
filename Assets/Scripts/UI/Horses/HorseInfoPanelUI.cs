using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Linq;

public class HorseInfoPanelUI : MonoBehaviour
{ 
    [Header("MainInfo")]
    public TMP_Text tierText;
    public Image imageForeground;
    public Image imageBackground;
    public Image horseImage;
    public TMP_Text horseColor;
    public TMP_Text horseColorMultiplier;
    public TMP_InputField horseName;

    [Header("Value")]
    public InfoBarUI emeraldBar;
    public Button sellButton;

    [Header("Stats")]
    public InfoBarUI speedBar;
    public InfoBarUI staminaBar;
    public InfoBarUI jumpBar;
    public InfoBarUI strengthBar;

    [Header("Training")]
    public EnergyBarUI trainingEnergyBar;
    public TMP_Text trainingMultiplierText;
    public Button addTrainingItemButton;
    public Button trainButton;

    [Header("Breeding")]
    public Slider upgradeSlider;
    public Slider sameSlider;
    public TMP_Text upgradeText;
    public TMP_Text sameText;
    public TMP_Text downgradeText;
    public Button breedButton;

    [Header("Traits")]
    public GameObject traitPrefab;
    public Transform traitPanel;

    /// <summary>
    /// Initializes Horse Info UI
    /// </summary>
    /// <param name="horse"></param>
    /// <param name="inventoryMode"></param>
    public void HorseUIInit(Horse horse, bool inventoryMode)
    {
        // MainInfo
        tierText.text = horse.Tier.TierName;
        tierText.color = horse.Tier.HighlightColor;

        imageForeground.color = horse.Tier.ForegroundColor;
        imageBackground.color = horse.Tier.BackgroundColor;
        horseImage.sprite = horse.Visual.sprite2D;

        horseColor.text = horse.Visual.horseColor.ToString();
        horseColor.color = horse.Visual.textColor;
        horseColorMultiplier.text = "x" + horse.Visual.PriceScalar.ToString("#.##");

        horseName.text = horse.horseName;

        //Value
        emeraldBar.UpdateBar("Emeralds", horse.GetCurrentPrice(), horse.GetMinPrice(), horse.GetMaxPrice(), true);
        sellButton.gameObject.SetActive(inventoryMode);

        //Stats
        speedBar.UpdateBar("Speed", horse.GetCurrent(StatType.Speed), 1, horse.GetMax(StatType.Speed), false);
        staminaBar.UpdateBar("Stamina", horse.GetCurrent(StatType.Stamina), 1, horse.GetMax(StatType.Stamina), false);
        jumpBar.UpdateBar("Jump Height", horse.GetCurrent(StatType.JumpHeight), 1, horse.GetMax(StatType.JumpHeight), false);
        strengthBar.UpdateBar("Strength", horse.GetCurrent(StatType.Strength), 1, horse.GetMax(StatType.Strength), false);

        //Training 
        trainingEnergyBar.SetMaxEnergy(horse.GetTrainingEnergy());
        trainingEnergyBar.SetEnergy(horse.currentTrainingEnergy);

        trainingMultiplierText.text = "+" + horse.GetTrainingRate().ToShortString();

        addTrainingItemButton.gameObject.SetActive(inventoryMode);
        trainButton.gameObject.SetActive(inventoryMode);
        trainButton.onClick.RemoveAllListeners();
        if (inventoryMode)
        {
            // Capture both "horse" and "inventoryMode" in the closure.
            trainButton.onClick.AddListener(() =>
            {
                horse.Train();
                HorseUIInit(horse, inventoryMode);
            });
        }

        // Breeding
        float upChance;
        float sameChance;
        float downChance;

        (upChance, sameChance, downChance) = horse.GetBreedingOdds();

        if(inventoryMode)
        {
            breedButton.gameObject.SetActive(true);
        }
        else
        {
            breedButton.gameObject.SetActive(false);
        }
        upgradeSlider.value = upChance;
        sameSlider.value = upChance + sameChance;

        if (upChance > 0)
            upgradeText.text = "UP: " + upChance.ToString("# %");
        else
            upgradeText.text = "UP: 0%";
        sameText.text = "SAME: " + sameChance.ToString("# %");
        if (downChance > 0)
            downgradeText.text = "DOWN: " + downChance.ToString("# %");
        else
            downgradeText.text = "DOWN: 0%";

        // Traits
        foreach (Transform child in traitPanel)
        {
            Destroy(child.gameObject);
        }

        var sortedTraits = horse.Traits.OrderByDescending(t => t.quality);
        foreach (var trait in sortedTraits)
        {
            GameObject tr = Instantiate(traitPrefab, traitPanel);
            tr.GetComponent<TraitUI>().InitTrait(trait);
        }
    }
}
