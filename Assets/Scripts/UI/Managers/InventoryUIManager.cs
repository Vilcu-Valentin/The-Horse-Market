using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the inventory UI with pagination, search, sorting and favorites support.
/// </summary>
public class InventoryUIManager : MonoBehaviour
{
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

    private enum SortField { Name = 0, Tier = 1, Price = 2, Training = 3 }
    private SortField currentSortField = SortField.Name;
    private bool isAscending = true;

    void Start()
    {
        SetupUI();
        horseInfoPanel.OnNameChanged += HandleNameChanged;
        horseInfoPanel.OnSellClicked += SellHorse;
        horseInfoPanel.OnFavoriteClicked += OnFavoriteToggled;
        RefreshList();
    }

    private void SetupUI()
    {
        // Panel events
        foreach (var panel in horsePanels)
        {
            panel.OnClicked += SellHorse;
            panel.InfoClicked += OpenInfoPanel;
            // Make sure HorseInventoryPanelUI has this event and SetFavoriteIndicator()
            panel.FavoriteToggled += OnFavoriteToggled;
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

    private void OnFavoriteToggled(Horse horse, bool fav)
    {
        horse.favorite = fav;
        SaveSystem.Instance.Save(); // Persist favorite change
        RefreshList();
    }

    /// <summary>
    /// Applies filtering, sorting and pagination in sequence.
    /// </summary>
    public void RefreshList()
    {
        ApplyFiltering();
        ApplySorting();
        ApplyPagination();
    }

    private void ApplyFiltering()
    {
        var query = searchInput.text?.ToLower() ?? string.Empty;
        filteredHorses = SaveSystem.Instance.Current.horses.ToList()
            .Where(h => string.IsNullOrEmpty(query) || h.horseName.ToLower().Contains(query))
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
            case SortField.Training:
                return h.currentTrainingEnergy;
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
                horsePanels[i].InitHorseUI(horse);
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
        SaveSystem.Instance.RemoveHorse(horse);
        SaveSystem.Instance.AddEmeralds(horse.GetCurrentPrice());
        RefreshList();
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
