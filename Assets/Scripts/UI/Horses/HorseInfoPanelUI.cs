using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Linq;
using System;

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
    public DialogPanelUI trainingFailDialog;

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

        //Training 
        trainingEnergyBar.SetMaxEnergy(horse.GetTrainingEnergy());
        trainingEnergyBar.SetEnergy(horse.currentTrainingEnergy);

        trainingMultiplierText.text = "+" + horse.GetTrainingRate().ToShortString();

        addTrainingItemButton.gameObject.SetActive(inventoryMode);
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
                bool success = horse.Train();

                // 2) If it failed, show the failure dialog
                if (!success)
                {
                    trainingFailDialog.Show(
                        "Training session failed, but consumed 1 energy!",
                        () => { /* nothing else to do on OK */ }
                    );
                }

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
}
