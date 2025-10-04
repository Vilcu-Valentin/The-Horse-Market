using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AscensionManagerUI : MonoBehaviour
{
    public HorseInfoPanelUI infoPanel;

    [Header("ChargeLevels")]
    public Button chargeButton;
    public TMP_Text chargePriceText;
    public EnergyBarUI chargeBar;

    [Header("Rolls")]
    public TMP_Text oddsText;
    public TMP_Text rollsText;
    public Button rollsButton;

    public DialogPanelUI loseDialogUI;
    public DialogPanelUI winDialogUI;

    [Header("HorseInfo")]
    public Image horseImage;
    public TMP_Text horseName;
    public Button horseInfoButton;

    public void Start()
    {
        chargeButton.onClick.RemoveAllListeners();
        chargeButton.onClick.AddListener(() =>
        {
            AscensionSystem.Instance.Charge();
            RefreshUI();
        });

        rollsButton.onClick.RemoveAllListeners();
        rollsButton.onClick.AddListener(() =>
        {
            HandleDialogPanel(AscensionSystem.Instance.Roll());
            RefreshUI();  
        });

        horseInfoButton.onClick.RemoveAllListeners();
        horseInfoButton.onClick.AddListener(() =>
        {
            infoPanel.gameObject.SetActive(true);
            infoPanel.HorseUIInit(AscensionSystem.Instance.SelectMythical(), false);
        });

        RefreshUI();
    }

    public void RefreshUI()
    {
        chargePriceText.text = AscensionSystem.Instance.GetChargePrice().ToShortString();
        chargeBar.SetEnergy(AscensionSystem.Instance.chargeLevel);

        rollsButton.interactable = true;
        if (AscensionSystem.Instance.currentRolls == 0)
            rollsButton.interactable = false;

        chargeButton.interactable = true;
        if(AscensionSystem.Instance.chargeLevel >= 10)
        {
            chargeButton.interactable = false;
            chargePriceText.text = "-";
        }

        oddsText.text = $"Win chance {AscensionSystem.Instance.GetChance().ToString("P5")}";
        rollsText.text = $"ROLL {AscensionSystem.Instance.currentRolls}/3";

        horseImage.sprite = AscensionSystem.Instance.SelectMythical().Visual.sprite2D;
        horseName.text = AscensionSystem.Instance.SelectMythical().horseName;
    }

    public void HandleDialogPanel(int result)
    {
        if(result == -1)
        {
            winDialogUI.Show(
                        "You have captured a mythical horse!",
                        () => {
                            Horse horse = AscensionSystem.Instance.SelectMythical();
                            SaveSystem.Instance.AddHorse(horse);
                            infoPanel.gameObject.SetActive(true);
                            infoPanel.HorseUIInit(horse, true);
                            AscensionSystem.Instance.GenerateMythical();
                            RefreshUI();
                        }
                    );
        }
        else if(result == 0)
        {
            loseDialogUI.Show(
            "You didn't capture the horse. Try again.",
            () => { /* nothing else to do on OK */ }
        );
        }
    }
}
