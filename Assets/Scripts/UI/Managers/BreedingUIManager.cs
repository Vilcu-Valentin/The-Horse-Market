using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;

public class BreedingUIManager : MonoBehaviour
{
    [Header("HorseA")]
    public Button selectParentAButton;

    public GameObject selectedParentAPanel;
    public TMP_Text parentAName;
    public TMP_Text parentATier;
    public Image parentAVisual;
    public Button removeParentAButton;
    private Horse parentA = null;

    [Header("HorseB")]
    public Button selectParentBButton;

    public GameObject selectedParentBPanel;
    public TMP_Text parentBName;
    public TMP_Text parentBTier;
    public Image parentBVisual;
    public Button removeParentBButton;
    private Horse parentB = null;

    [Header("System")]
    public Button inventoryButton;
    public Button shopButton;
    public Button competitionButton;
    public bool lockWindow = false;
    private bool lockWindowcached = false;


    public InventoryUIManager inventoryManager;

    public DialogPanelUI confirmDialog;
    public DialogPanelUI resultDialog;
    public HorseInfoPanelUI foalInfoPanel;

    public Slider upOdds;
    public Slider sameOdds;

    public TMP_Text upOddsText;
    public TMP_Text sameOddsText;
    public TMP_Text downOddsText;

    public Button breedButton;

    private void Start()
    {
        InitUI();
    }

    private void Update()
    {
        if (lockWindow != lockWindowcached)
            LockButtons();
        lockWindowcached = lockWindow;
    }
    
    private void LockButtons()
    {
        if(lockWindow == true)
        {
            inventoryButton.interactable = false;
            shopButton.interactable = false;
            competitionButton.interactable = false;
        }
        else
        {
            inventoryButton.interactable = true;
            shopButton.interactable = true;
            competitionButton.interactable = true;
        }
    }

    public void InitUI()
    {
        parentA = null;
        parentB = null;
        lockWindow = false;
        breedButton.interactable = false;

        selectParentAButton.gameObject.SetActive(true);
        selectParentBButton.gameObject.SetActive(true);
        selectedParentAPanel.gameObject.SetActive(false);
        selectedParentBPanel.gameObject.SetActive(false);

        selectParentAButton.onClick.RemoveAllListeners();
        selectParentBButton.onClick.RemoveAllListeners();

         // When picking A, filter by parentB.Tier if you already chose B
        selectParentAButton.onClick.AddListener(OpenInventoryA);

        // When picking B, filter by parentA.Tier if you already chose A
        selectParentBButton.onClick.AddListener(OpenInventoryB);

        UpdateChanceValues(0.33f, 0.34f, 0.33f);
    }

    private void OpenInventoryA()
    {
        inventoryManager.OpenForSelecting(
            AddParentA,
            parentB != null ? parentB.Tier : null,
            parentB                              // ← don’t show B in the list
        );
    }

    private void OpenInventoryB()
    {
        inventoryManager.OpenForSelecting(
            AddParentB,
            parentA != null ? parentA.Tier : null,
            parentA                              // ← don’t show A in the list
        );
    }


    public void AddParentA(Horse horse)
    {
        parentA = horse;
        lockWindow = true;

        selectedParentAPanel.gameObject.SetActive(true);
        selectParentAButton.gameObject.SetActive(false);

        parentAName.text = horse.horseName;
        parentATier.text = horse.Tier.TierName;
        parentATier.color = horse.Tier.HighlightColor;
        parentAVisual.sprite = horse.Visual.sprite2D;

        float upChance;
        float sameChance;
        float downChance;

        if (parentB != null)
        { 
            (upChance, sameChance, downChance) = BreedingSystem.CalculateFoalOdds(parentA, parentB);
            breedButton.interactable = true;
        }
        else
            (upChance, sameChance, downChance) = horse.GetBreedingOdds();

        UpdateChanceValues(upChance, sameChance, downChance);
    }
    public void AddParentB(Horse horse)
    {
        parentB = horse;
        lockWindow = true;

        selectedParentBPanel.gameObject.SetActive(true);
        selectParentBButton.gameObject.SetActive(false);

        parentBName.text = horse.horseName;
        parentBTier.text = horse.Tier.TierName;
        parentBTier.color = horse.Tier.HighlightColor;
        parentBVisual.sprite = horse.Visual.sprite2D;

        float upChance;
        float sameChance;
        float downChance;

        if (parentA != null)
        { 
            (upChance, sameChance, downChance) = BreedingSystem.CalculateFoalOdds(parentA, parentB); 
            breedButton.interactable = true;
        }
        else
            (upChance, sameChance, downChance) = horse.GetBreedingOdds();

        UpdateChanceValues(upChance, sameChance, downChance);
    }

    public void RemoveParentA()
    {
        if (parentB != null)
        {
            float upChance;
            float sameChance;
            float downChance;
            (upChance, sameChance, downChance) = parentB.GetBreedingOdds();
            UpdateChanceValues(upChance, sameChance, downChance);
            breedButton.interactable = false;
        }
        else
        {
            UpdateChanceValues(0.33f, 0.34f, 0.33f);
            lockWindow = false;
        }

        parentA = null;

        selectedParentAPanel.gameObject.SetActive(false);
        selectParentAButton.gameObject.SetActive(true);
    }

    public void RemoveParentB()
    {
        if (parentA != null)
        {
            float upChance;
            float sameChance;
            float downChance;
            (upChance, sameChance, downChance) = parentA.GetBreedingOdds();
            UpdateChanceValues(upChance, sameChance, downChance);
            breedButton.interactable = false;
        }
        else
        {
            UpdateChanceValues(0.33f, 0.34f, 0.33f);
            lockWindow = false;
        }

        parentB = null;

        selectedParentBPanel.gameObject.SetActive(false);
        selectParentBButton.gameObject.SetActive(true);
    }

    public void Breed()
    {
        if (parentA == null || parentB == null) return;

        // 1) Ask for confirmation
        confirmDialog.Show(
            $"The parent horses will be consumed. Proceed?",
            confirmCallback: () => StartCoroutine(BreedSequence()),
            cancelCallback: () => {/* nothing; just close */});
    }

    private IEnumerator BreedSequence()
    {
        // 2) Perform the actual breeding
        Horse foal = BreedingSystem.Breed(parentA, parentB);

        // 3) Build your result message
        string fromTier = parentA.Tier.TierName;   // e.g. "Tier II"
        string toTier = foal.Tier.TierName;      // e.g. "Tier III"
        bool upgraded = foal.Tier.TierIndex > parentA.Tier.TierIndex;
        bool downgraded = foal.Tier.TierIndex < parentA.Tier.TierIndex;

        string body;
        if (upgraded)
            body = $"<b>UPGRADE!</b>\n<color=#{parentA.Tier.HighlightColor.ToHexString()}>{fromTier}</color> -> <color=#{foal.Tier.HighlightColor.ToHexString()}>{toTier}</color>";
        else if (downgraded)
            body = $"<b>DOWNGRADE :(</b>\n<color=#{parentA.Tier.HighlightColor.ToHexString()}>{fromTier}</color> -> <color=#{foal.Tier.HighlightColor.ToHexString()}>{toTier}</color>";
        else
            body = $"<b>SAME TIER</b>\n<color=#{parentA.Tier.HighlightColor.ToHexString()}>{fromTier}</color> -> <color=#{foal.Tier.HighlightColor.ToHexString()}>{toTier}</color>";

        // 4) Show result dialog
        bool done = false;
        resultDialog.Show(
            body,
            () => done = true,    // confirm
            null                  // no-need to cancel
        );
        // wait until user taps “OK”
        yield return new WaitUntil(() => done);

        // 5) Reset your UI and show the foal info
        InitUI();
        foalInfoPanel.gameObject.SetActive(true);
        foalInfoPanel.HorseUIInit(foal, true);
    }

    private void UpdateChanceValues(float upChance, float sameChance, float downChance)
    {
        upOdds.value = upChance;
        sameOdds.value = upChance + sameChance;

        if (upChance > 0)
            upOddsText.text = "UPGRADE: " + upChance.ToString("# %");
        else
            upOddsText.text = "UPGRADE: 0%";
        sameOddsText.text = "SAME: " + sameChance.ToString("# %");
        if (downChance > 0)
            downOddsText.text = "DOWNGRADE: " + downChance.ToString("# %");
        else
            downOddsText.text = "DOWNGRADE: 0%";
    }
}
