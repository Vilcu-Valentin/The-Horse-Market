using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject upgradeChancePanel;
    public GameObject upgradeChanceCoverPanel;

    public Button inventoryButton;
    public Button shopButton;
    public Button competitionButton;
    public bool lockWindow = false;
    private bool lockWindowcached = false;

    [Header("Breeding Items")]
    public ItemInventoryUIManager inventoryUIManager;
    public List<Button> addBreedingItemButtons;
    public List<Button> removeSelectedItemButtons;
    public List<ItemSelectedUI> selectedItemUIs;
    private List<Item> selectedBreedingItems;

    public InventoryUIManager inventoryManager;

    public DialogPanelUI confirmDialog;
    public BreedingUpgradeDialogPanelUI resultDialog;
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

        for (int i = 0; i < addBreedingItemButtons.Count; i++) 
        {
            int index = i;
            addBreedingItemButtons[index].onClick.RemoveAllListeners();
            addBreedingItemButtons[index].onClick.AddListener(()=> {
                OpenItemInventory(index);
            });

            removeSelectedItemButtons[index].onClick.RemoveAllListeners();
            removeSelectedItemButtons[index].onClick.AddListener(() =>
            {
                RemoveItem(index, false);
            });
        }

        RemoveAllItems(false);
        selectedBreedingItems = new List<Item>(new Item[addBreedingItemButtons.Count]);

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

        RefreshBreedingOddsAndUI();
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

        RefreshBreedingOddsAndUI();
    }

    public void RemoveParentA()
    {
        parentA = null;

        selectedParentAPanel.gameObject.SetActive(false);
        selectParentAButton.gameObject.SetActive(true);

        RefreshBreedingOddsAndUI();
    }

    public void RemoveParentB()
    {
        parentB = null;

        selectedParentBPanel.gameObject.SetActive(false);
        selectParentBButton.gameObject.SetActive(true);

        RefreshBreedingOddsAndUI();
    }

    public void OpenItemInventory(int index)
    {
        inventoryUIManager.InitUI(item => AddItem(item, index), false);
    }

    public void AddItem(Item item, int index)
    {
        selectedBreedingItems[index] = item;
        selectedItemUIs[index].gameObject.SetActive(true);
        selectedItemUIs[index].InitUI(item);
        removeSelectedItemButtons[index].gameObject.SetActive(true);

        SaveSystem.Instance.RemoveItem(item.Def);

        addBreedingItemButtons[index].gameObject.SetActive(false);
        RefreshBreedingOddsAndUI();
    }

    public void RemoveAllItems(bool consume)
    {
        if(selectedBreedingItems != null)
            for (int i = 0; i < selectedBreedingItems.Count; i++)
                RemoveItem(i, consume);
    }

    public void RemoveItem(int index, bool consume)
    {
        if (selectedBreedingItems[index] != null)
        {
            if(!consume)
                SaveSystem.Instance.AddItem(selectedBreedingItems[index].Def);
            selectedBreedingItems[index] = null;
        }

        selectedItemUIs[index].gameObject.SetActive(false);
        removeSelectedItemButtons[index].gameObject.SetActive(false);

        addBreedingItemButtons[index].gameObject.SetActive(true);
        RefreshBreedingOddsAndUI();
    }

    public void Breed()
    {
        if (parentA == null || parentB == null) return;

        if (!ItemConsumeParents())
        {
            // 1) Ask for confirmation
            confirmDialog.Show(
            $"The parent horses will be consumed. Proceed?",
            confirmCallback: () => StartCoroutine(BreedSequence()),
            cancelCallback: () => {/* nothing; just close */});
        }
        else
        {
            StartCoroutine(BreedSequence());
        }
    }

    private IEnumerator BreedSequence()
    {
        // 2) Perform the actual breeding
        Horse foal = BreedingSystem.Breed(parentA, parentB, selectedBreedingItems);

        RemoveAllItems(true);

        // 3) Build your result message
        string fromTier = parentA.Tier.TierName;   // e.g. "Tier II"
        string toTier = foal.Tier.TierName;      // e.g. "Tier III"
        bool upgraded = foal.Tier.TierIndex > parentA.Tier.TierIndex;
        bool downgraded = foal.Tier.TierIndex < parentA.Tier.TierIndex;

        string body;
        if (upgraded)
            body = $"<b>UPGRADE!</b>";
        else if (downgraded)
            body = $"<b>DOWNGRADE :(</b>";
        else
            body = $"<b>SAME TIER</b>";

        // 4) Show result dialog
        bool done = false;
        resultDialog.Show(
            body,
            oldTier: $"<color=#{parentA.Tier.HighlightColor.ToHexString()}>{fromTier}</color>",
            newTier: $"<color=#{foal.Tier.HighlightColor.ToHexString()}>{toTier}</color>",
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

    private (float, float) GetItemUpgradeModifiers()
    {
        float upgradeModifier = 1.0f;
        float downgradeModifier = 1.0f;

        // Calculate item modifiers
        foreach (var item in selectedBreedingItems)
        {
            if (item == null)
                continue;

            upgradeModifier *= item.Def.UpgradeMult;
            downgradeModifier *= item.Def.DowngradeMult;
        }

        return (upgradeModifier, downgradeModifier);
    }

    private bool ItemConsumeParents()
    {
        foreach(var item in selectedBreedingItems)
        {
            if (item == null)
                continue;
            if (item.Def.preventParentConsumption)
                return true;
        }
        return false;
    }

    private void RefreshBreedingOddsAndUI()
    {
        float upChance;
        float sameChance;
        float downChance;

        if (parentA != null && parentB != null)
        {
            // Both parents present
            var (upItemMod, downItemMod) = GetItemUpgradeModifiers();
            (upChance, sameChance, downChance) = BreedingSystem.CalculateFoalOdds(parentA, parentB, upItemMod, downItemMod);

            upgradeChanceCoverPanel.SetActive(false);
            upgradeChancePanel.SetActive(true);
            breedButton.interactable = true;
        }
        else if (parentA != null)
        {
            // Only parent A
            (upChance, sameChance, downChance) = parentA.GetBreedingOdds();

            upgradeChanceCoverPanel.SetActive(false);
            upgradeChancePanel.SetActive(true);
            breedButton.interactable = false;
        }
        else if (parentB != null)
        {
            // Only parent B
            (upChance, sameChance, downChance) = parentB.GetBreedingOdds();

            upgradeChanceCoverPanel.SetActive(false);
            upgradeChancePanel.SetActive(true);
            breedButton.interactable = false;
        }
        else
        {
            // No parents
            upChance = 0.33f;
            sameChance = 0.34f;
            downChance = 0.33f;

            upgradeChanceCoverPanel.SetActive(true);
            upgradeChancePanel.SetActive(false);
            breedButton.interactable = false;
            lockWindow = false;
        }

        UpdateChanceValues(upChance, sameChance, downChance);
    }


    private void UpdateChanceValues(float upChance, float sameChance, float downChance)
    {
        bool guaranteeUpgrade = false;
        bool guaranteeSameTier = false;
        bool guaranteeNoDowngrade = false;

        // Calculate item modifiers
        foreach (var item in selectedBreedingItems)
        {
            if (item == null)
                continue;

            if (item.Def.guaranteeNoDowngrade)
                guaranteeNoDowngrade = true;
            if (item.Def.guaranteeSameTier)
                guaranteeSameTier = true;
            if (item.Def.guaranteeUpgrade)
                guaranteeUpgrade = true;
        }
        if (guaranteeNoDowngrade)
        {
            sameChance += downChance;
            downChance = 0f;
        }
        if (guaranteeSameTier)
        {
            upChance = 0f;
            sameChance = 1.0f;
            downChance = 0f;
        }
        if (guaranteeUpgrade)
        {
            upChance = 1.0f;
            sameChance = 0f;
            downChance = 0f;
        }



        upOdds.value = upChance;
        sameOdds.value = upChance + sameChance;

        if (upChance > 0)
            upOddsText.text = "UPGRADE: " + upChance.ToString("# %");
        else
            upOddsText.text = "UPGRADE: 0%";
        if (sameChance > 0)
            sameOddsText.text = "SAME: " + sameChance.ToString("# %");
        else
            sameOddsText.text = "SAME: 0%";
        if (downChance > 0)
            downOddsText.text = "DOWNGRADE: " + downChance.ToString("# %");
        else
            downOddsText.text = "DOWNGRADE: 0%";
    }
}
