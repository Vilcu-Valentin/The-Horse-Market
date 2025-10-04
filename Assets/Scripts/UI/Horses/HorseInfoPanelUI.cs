using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HorseInfoPanelUI : MonoBehaviour
{
    public AudioClip enterAudio;

    [Header("MainInfo")]
    public TMP_Text tierText;
    public Image imageForeground;
    public Image imageBackground;
    public Image horseImage;
    public TMP_Text horseColor;
    public TMP_Text horseColorMultiplier;
    public TMP_InputField horseName;
    public Button favoriteButton;
    public Button closeButton;
    public EnergyBarUI remainingCompetitions;

    [Header("Value")]
    public InfoBarUI emeraldBar;
    public Button sellButton;

    [Header("Stats")]
    public InfoBarUI speedBar;
    public InfoBarUI staminaBar;
    public InfoBarUI jumpBar;
    public InfoBarUI strengthBar;

    public TMP_Text speedTrainingBoost;
    public TMP_Text staminaTrainingBoost;
    public TMP_Text jumpTrainingBoost;
    public TMP_Text strengthTrainingBoost;

    [Header("Ascensions")]
    public GameObject ascBackground;
    public GameObject ascensionNumber;
    public TMP_Text ascensionNumberText;
    public GameObject ascensionPanel;
    public Button AscendHorseButton;
    public TMP_Text LE_amountText;
    public DialogPanelUI ascensionWarningDialog;

    [Header("Training")]
    public GameObject trainingPanel;
    public EnergyBarUI trainingEnergyBar;
    public TMP_Text trainingMultiplierText;
    public Button trainButton;
    public DialogPanelUI trainingFailDialog;

    public ItemInventoryUIManager inventoryUIManager;
    public Button addTrainingItemButton;
    public ItemSelectedUI itemSelectedUI;
    public Button removeSelectedItemButton;
    private Item selectedTrainingItem = null;

    [Header("Breeding")]
    public Slider upgradeSlider;
    public Slider sameSlider;
    public TMP_Text upgradeText;
    public TMP_Text sameText;
    public TMP_Text downgradeText;
    public Button breedButton;
    public BreedingUIManager breedingUIManager;

    [Header("Traits")]
    public GameObject traitPrefab;
    public Transform traitPanel;

    public event Action<Horse> OnSellClicked;
    public event Action<Horse, bool> OnFavoriteClicked;
    public event Action<Horse> OnNameChanged;
    public event Action OnCloseClicked;

    /// <summary>
    /// Initializes Horse Info UI
    /// </summary>
    /// <param name="horse"></param>
    /// <param name="inventoryMode"></param>
    public void HorseUIInit(Horse horse, bool inventoryMode)
    {
        AudioManager.Instance.PlaySound(enterAudio, 0.65f, 0.25f);
        closeButton.onClick.RemoveAllListeners();
        if(inventoryMode)
        {
            closeButton.onClick.AddListener(() =>
            {
                OnCloseClicked?.Invoke();
            });
        }

        horseName.interactable = inventoryMode;
        horseName.onEndEdit.RemoveAllListeners();
        horseName.text = horse.horseName;
        horseName.onEndEdit.AddListener(newName =>
        {
            if (string.IsNullOrWhiteSpace(newName) || newName == horse.horseName)
                return;
            horse.horseName = newName;
            OnNameChanged?.Invoke(horse);
        });

        favoriteButton.gameObject.SetActive(inventoryMode);
        favoriteButton.onClick.RemoveAllListeners();
        if(inventoryMode)
        {
            favoriteButton.onClick.AddListener(() =>
            {
                bool newFavState = !horse.favorite;
                SetFavoriteIndicator(newFavState);
                OnFavoriteClicked?.Invoke(horse, newFavState);
            });
        }

        SetFavoriteIndicator(horse.favorite);

        // MainInfo
        tierText.text = horse.Tier.TierName;
        tierText.color = horse.Tier.HighlightColor;

        imageForeground.color = horse.Tier.ForegroundColor;
        imageBackground.color = horse.Tier.BackgroundColor;
        horseImage.sprite = horse.Visual.sprite2D;

        horseColor.text = horse.Visual.horseColor.ToString();
        horseColor.color = horse.Visual.textColor;
        horseColorMultiplier.text = "x" + horse.Visual.PriceScalar.ToString("#.##") + " Price";

        remainingCompetitions.SetMaxEnergy(horse.GetMaxCompetitions());
        remainingCompetitions.SetEnergy(horse.remainingCompetitions);

        //Value
        emeraldBar.UpdateBar("Emeralds", horse.GetCurrentPrice(), horse.GetMinPrice(), horse.GetMaxPrice(), true);

        sellButton.onClick.RemoveAllListeners();
        sellButton.gameObject.SetActive(inventoryMode);
        if (inventoryMode)
        {
            sellButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                OnSellClicked?.Invoke(horse);
            });
        }

        //Stats
        speedBar.UpdateBar("Speed", horse.GetCurrent(StatType.Speed), 1, horse.GetMax(StatType.Speed), false);
        staminaBar.UpdateBar("Stamina", horse.GetCurrent(StatType.Stamina), 1, horse.GetMax(StatType.Stamina), false);
        jumpBar.UpdateBar("Jump Height", horse.GetCurrent(StatType.JumpHeight), 1, horse.GetMax(StatType.JumpHeight), false);
        strengthBar.UpdateBar("Strength", horse.GetCurrent(StatType.Strength), 1, horse.GetMax(StatType.Strength), false);


        //Ascension
        trainingPanel.SetActive(true);
        ascensionPanel.SetActive(false);

        ascBackground.SetActive(false);
        ascensionNumber.SetActive(false);

        if(horse.ascensions > 0)
        {
            ascBackground.SetActive(true);
            ascensionNumber.SetActive(true);

            ascensionNumberText.text = horse.ascensions.ToRomanString();
        }

        if(inventoryMode)
        {
            if(horse.CanAscend())
            {
                trainingPanel.SetActive(false);
                ascensionPanel.SetActive(true);

                AscendHorseButton.onClick.RemoveAllListeners();
                AscendHorseButton.onClick.AddListener(() =>
                {
                ascensionWarningDialog.Show("Ascending a horse will reset it's tier!", () =>
                {
                    Horse ascendedHorse = AscensionSystem.Instance.AscendHorse(horse);
                    HorseUIInit(ascendedHorse, inventoryMode);
                });
                });
                   
                LE_amountText.text = horse.GetLE_Reward().ToShortString();
            }
        }

        //Training 
        trainingEnergyBar.SetMaxEnergy(horse.GetTrainingEnergy());
        trainingEnergyBar.SetEnergy(horse.currentTrainingEnergy);

        trainingMultiplierText.text = "+" + horse.GetTrainingRate().ToShortString();

        RemoveItem();
        addTrainingItemButton.gameObject.SetActive(inventoryMode);
        if(inventoryMode)
        {
            addTrainingItemButton.onClick.RemoveAllListeners();
            addTrainingItemButton.onClick.AddListener(OpenItemInventory);

            removeSelectedItemButton.onClick.RemoveAllListeners();
            removeSelectedItemButton.onClick.AddListener(RemoveItem);
        }

        trainButton.gameObject.SetActive(inventoryMode);

        trainButton.onClick.RemoveAllListeners();
        if (horse.currentTrainingEnergy <= 0)
            trainButton.interactable = false;
        else
            trainButton.interactable = true;

        if (horse.IsHorseFullyTrained())
            trainButton.interactable = false;

        if (inventoryMode)
        {
            trainButton.onClick.AddListener(() =>
            {
                // 1) Attempt to train
                int success = horse.Train(selectedTrainingItem);

                // 2) If it failed, show the failure dialog
                if (success == 1)
                {
                    trainingFailDialog.Show(
                        "Training session failed, but consumed 1 energy!",
                        () => { /* nothing else to do on OK */ }
                    );
                }

                if(success == 0)
                {
                    trainingFailDialog.Show(
                       "Training session failed, no energy was consumed!",
                       () => { /* nothing else to do on OK */ }
                   );
                }


                if (selectedTrainingItem != null)
                    RemoveItem();

                // 3) Refresh all fields (energy bar, stats, etc.)
                HorseUIInit(horse, inventoryMode);
            });
        }

        // Breeding
        float upChance;
        float sameChance;
        float downChance;

        (upChance, sameChance, downChance) = horse.GetBreedingOdds();

        breedButton.onClick.RemoveAllListeners();
        breedButton.gameObject.SetActive(inventoryMode);
        if(inventoryMode)
        {
            breedButton.onClick.AddListener(() =>
            {
                MainUIController.Instance.SetState(AppState.Breeding);
                breedingUIManager.AddParentA(horse);
                gameObject.SetActive(false);
            });
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

    public void SetFavoriteIndicator(bool isFav)
    {
        // favoriteButton.image.sprite = isFav ? filledStarSprite : emptyStarSprite;

        // Or if you just want a tint:
        favoriteButton.image.color = isFav ? new Color(1f, 0.84f, 0f)  // gold
                                           : Color.white;
    }

    public void OpenItemInventory()
    {
        inventoryUIManager.InitUI(AddItem, true);
    }

    public void AddItem(Item item)
    {
        selectedTrainingItem = item;
        itemSelectedUI.gameObject.SetActive(true);
        itemSelectedUI.InitUI(item);
        removeSelectedItemButton.gameObject.SetActive(true);

        var speed_delta = item.Def.AdditionalStatDelta[(int)StatType.Speed].Delta;
        if (speed_delta >= 1)
        {
            speedTrainingBoost.gameObject.SetActive(true);
            speedTrainingBoost.text = "+" + speed_delta.ToString();
        }

        var stamina_delta = item.Def.AdditionalStatDelta[(int)StatType.Stamina].Delta;
        if (stamina_delta >= 1)
        {
            staminaTrainingBoost.gameObject.SetActive(true);
            staminaTrainingBoost.text = "+" + stamina_delta.ToString();
        }

        var jump_delta = item.Def.AdditionalStatDelta[(int)StatType.JumpHeight].Delta;
        if (jump_delta >= 1)
        {
            jumpTrainingBoost.gameObject.SetActive(true);
            jumpTrainingBoost.text = "+" + jump_delta.ToString();
        }

        var strength_delta = item.Def.AdditionalStatDelta[(int)StatType.Strength].Delta;
        if (strength_delta >= 1)
        {
            strengthTrainingBoost.gameObject.SetActive(true);
            strengthTrainingBoost.text = "+" + strength_delta.ToString();
        }

        addTrainingItemButton.gameObject.SetActive(false);
    }

    public void RemoveItem()
    {
        if(selectedTrainingItem != null)
            selectedTrainingItem = null;

        itemSelectedUI.gameObject.SetActive(false);
        removeSelectedItemButton.gameObject.SetActive(false);

        speedTrainingBoost.gameObject.SetActive(false);
        staminaTrainingBoost.gameObject.SetActive(false);
        jumpTrainingBoost.gameObject.SetActive(false);
        strengthTrainingBoost.gameObject.SetActive(false);

        addTrainingItemButton.gameObject.SetActive(true);
    }
}
