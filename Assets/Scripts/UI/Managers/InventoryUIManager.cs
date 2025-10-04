using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryMode { Inventory, Selecting}

/// <summary>
/// Manages the inventory UI with pagination, search, sorting and favorites support.
/// </summary>
public class InventoryUIManager : MonoBehaviour
{
    public GameObject noHorsePanel;

    /// <summary>
    /// Used in SelectingMode
    /// </summary>
    public Button closePanelButton;
    public DialogPanelUI confirmSellDialog;

    [Header("UI Panels")]
    public List<HorseInventoryPanelUI> horsePanels;
    public HorseInfoPanelUI horseInfoPanel;

    [Header("Filter & Sorting UI")]
    public TMP_InputField searchInput;
    public TMP_Dropdown sortDropdown;
    public Toggle ascendingToggle;

    [Header("Pagination UI")]
    public Button prevPageButton;
    public Button nextPageButton;
    public TextMeshProUGUI pageInfoText;

    private List<Horse> filteredHorses = new List<Horse>();
    private int currentPage = 0;
    private int itemsPerPage => horsePanels.Count;

    private enum SortField { Name = 0, Tier = 1, Price = 2, Stats = 3 }
    private SortField currentSortField = SortField.Name;
    private bool isAscending = true;
    private bool openForCompetitions = false;

    private TierDef selectionTier;
    private Horse selectionExcludedHorse;
    private InventoryMode currentMode = InventoryMode.Inventory;
    private Action<Horse> onSelectCallback;

    void Start()
    {
        SetupUI();
        horseInfoPanel.OnNameChanged += HandleNameChanged;
        horseInfoPanel.OnSellClicked += SellHorse;
        horseInfoPanel.OnFavoriteClicked += OnFavoriteToggled;
        horseInfoPanel.OnCloseClicked += RefreshList;
        RefreshList();
    }

    /// <summary>
    /// Call this to show the inventory for picking a breeding candidate.
    /// </summary>
    public void OpenForSelecting(
        Action<Horse> onPick,
        TierDef requiredTier = null,
        Horse excludedHorse = null,
        bool openForCompetitions = false
    )
    {
        closePanelButton.gameObject.SetActive(true);
        currentMode = InventoryMode.Selecting;
        onSelectCallback = onPick;
        selectionTier = requiredTier;
        this.openForCompetitions = openForCompetitions;
        selectionExcludedHorse = excludedHorse;
        gameObject.SetActive(true);
        RefreshList();
    }


    public void OpenForInventory()
    {
        closePanelButton.gameObject.SetActive(false);
        currentMode = InventoryMode.Inventory;
        onSelectCallback = null;
        selectionTier = null;
        openForCompetitions = false;
        gameObject.SetActive(true);
        RefreshList();
    }

    private void SetupUI()
    {
        // Panel events
        foreach (var panel in horsePanels)
        {
            panel.OnClicked += HandleClick;
            panel.InfoClicked += OpenInfoPanel;
            panel.FavoriteToggled += OnFavoriteToggled;
            panel.SelectClicked += HandleSelect;
        }

        // Search input
        searchInput.onValueChanged.AddListener(_ => { currentPage = 0; RefreshList(); });
        // Sort dropdown (0: Name, 1: Tier, 2: Price, 3: Training)
        sortDropdown.onValueChanged.AddListener(idx => { currentSortField = (SortField)idx; RefreshList(); });
        // Ascending toggle
        ascendingToggle.onValueChanged.AddListener(val => { isAscending = val; RefreshList(); });

        // Pagination buttons
        prevPageButton.onClick.AddListener(() => { if (currentPage > 0) { currentPage--; RefreshList(); } });
        nextPageButton.onClick.AddListener(() => { if ((currentPage + 1) * itemsPerPage < filteredHorses.Count) { currentPage++; RefreshList(); } });
    }

    private void HandleClick(Horse horse)
    {
        if (currentMode == InventoryMode.Inventory)
            SellHorse(horse);
    }

    private void HandleSelect(Horse horse)
    {
        // only in a select mode do we honor this
        if (currentMode != InventoryMode.Inventory && onSelectCallback != null)
        {
            onSelectCallback(horse);
            // auto-close or revert to browsing
            currentMode = InventoryMode.Inventory;
            onSelectCallback = null;
            RefreshList();
            gameObject.SetActive(false);
        }
    }

    private void OnFavoriteToggled(Horse horse, bool fav)
    {
        horse.favorite = fav;
        SaveSystem.Instance.Save(); // Persist favorite change
        RefreshList();
    }

    /// <summary>
    /// Applies filtering, sorting and pagination in sequence.
    /// </summary>
    private void RefreshList()
    {
        if(SaveSystem.Instance.Current.horses.Count <= 0)
            noHorsePanel.SetActive(true);
        else
            noHorsePanel.SetActive(false);

        ApplyFiltering();
        ApplySorting();
        ApplyPagination();
    }

    private void ApplyFiltering()
    {
        var query = searchInput.text?.ToLower() ?? string.Empty;

        filteredHorses = SaveSystem.Instance.Current.horses
            // name search
            .Where(h => string.IsNullOrEmpty(query)
                        || h.horseName.ToLower().Contains(query))
            // optional tier filter
            .Where(h => currentMode != InventoryMode.Selecting
                        || selectionTier == null
                        || h.Tier == selectionTier)
            // drop the excluded horse
            .Where(h => currentMode != InventoryMode.Selecting
                        || selectionExcludedHorse == null
                        || h != selectionExcludedHorse)
            .ToList();
    }

    private void ApplySorting()
    {
        // Favorites first (favorite == true => 0)
        IOrderedEnumerable<Horse> ordered = filteredHorses
            .OrderBy(h => h.favorite ? 0 : 1);

        // Then by selected field
        if (isAscending)
            ordered = ordered.ThenBy(h => GetSortKey(h));
        else
            ordered = ordered.ThenByDescending(h => GetSortKey(h));

        filteredHorses = ordered.ToList();
    }

    private object GetSortKey(Horse h)
    {
        switch (currentSortField)
        {
            case SortField.Name:
                return h.horseName;
            case SortField.Tier:
                return h.Tier.TierIndex;
            case SortField.Price:
                return h.GetCurrentPrice();
            case SortField.Stats:
                return h.GetAverageMax();
            default:
                return h.horseName;
        }
    }

    private void ApplyPagination()
    {
        int start = currentPage * itemsPerPage;
        int end = Mathf.Min(start + itemsPerPage, filteredHorses.Count);
        var pageItems = filteredHorses.Skip(start).Take(end - start).ToList();

        // Populate panels
        for (int i = 0; i < itemsPerPage; i++)
        {
            if (i < pageItems.Count)
            {
                var horse = pageItems[i];
                horsePanels[i].gameObject.SetActive(true);
                horsePanels[i].InitHorseUI(horse, currentMode, openForCompetitions);
            }
            else
            {
                horsePanels[i].gameObject.SetActive(false);
            }
        }

        // Update pagination UI
        int totalPages = Mathf.CeilToInt((float)filteredHorses.Count / itemsPerPage);
        pageInfoText.text = $"Page {currentPage + 1}/{Mathf.Max(totalPages, 1)}";
        prevPageButton.interactable = currentPage > 0;
        nextPageButton.interactable = currentPage < totalPages - 1;
    }

    public void SellHorse(Horse horse)
    {
        // 1) Show confirmation dialog
        confirmSellDialog.Show(
            // message
            $"Are you sure you want to sell “<color=#FFFFFF>{horse.horseName}</color>” for <color=#1DB921>{horse.GetCurrentPrice().ToShortString()} emeralds</color>?",
            // onConfirm
            () => {
                // actually remove and give money
                SaveSystem.Instance.RemoveHorse(horse);
                EconomySystem.Instance.AddEmeralds(horse.GetCurrentPrice());
                RefreshList();
            },
            // onCancel (optional—if you don’t need to do anything, you can omit this)
            () => { /* user cancelled; nothing to do */ }
        );
    }

    public void OpenInfoPanel(Horse horse, bool inventoryMode)
    {
        horseInfoPanel.gameObject.SetActive(true);
        horseInfoPanel.HorseUIInit(horse, inventoryMode);
    }

    private void HandleNameChanged(Horse horse)
    {
        SaveSystem.Instance.Save();
        RefreshList();
    }
}
