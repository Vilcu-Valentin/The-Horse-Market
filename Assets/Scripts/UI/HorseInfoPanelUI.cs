using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

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

    [Header("Stats")]
    public InfoBarUI speedBar;
    public InfoBarUI staminaBar;
    public InfoBarUI jumpBar;
    public InfoBarUI strengthBar;

    [Header("Training")]
    // ENERGY BAR NEEDED
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
    // TRAITS GENERATION AND SORTING

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

        //Stats
        speedBar.UpdateBar("Speed", horse.GetCurrent(StatType.Speed), 1, horse.GetMax(StatType.Speed), false);
        staminaBar.UpdateBar("Stamina", horse.GetCurrent(StatType.Stamina), 1, horse.GetMax(StatType.Stamina), false);
        jumpBar.UpdateBar("Jump Height", horse.GetCurrent(StatType.JumpHeight), 1, horse.GetMax(StatType.JumpHeight), false);
        strengthBar.UpdateBar("Strength", horse.GetCurrent(StatType.Strength), 1, horse.GetMax(StatType.Strength), false); 

        //Training TODO: GET THE ACTUAL DATA 
        trainingMultiplierText.text = "100xp";
        if(inventoryMode)
        {
            addTrainingItemButton.gameObject.SetActive(true);
            trainButton.gameObject.SetActive(true);
        }
        else
        {
            addTrainingItemButton.gameObject.SetActive(false);
            trainButton.gameObject.SetActive(false);
        }

        //Breeding TODO: GET THE ACTUAL DATA
        if(inventoryMode)
        {
            breedButton.gameObject.SetActive(true);
        }
        else
        {
            breedButton.gameObject.SetActive(false);
        }
        upgradeSlider.value = 0.3f;
        sameSlider.value = upgradeSlider.value + 0.3f;

        foreach (Transform child in traitPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (var trait in horse.Traits)
        {
            GameObject tr = Instantiate(traitPrefab, traitPanel);
            tr.GetComponent<TraitUI>().InitTrait(trait);
        }
    }
}
