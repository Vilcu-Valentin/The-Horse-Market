using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UISwitchEntry
{
    public string id;                   // Unique key for this tab (e.g. "Weapons", "Creatures")
    public Button button;               // Button to activate this tab
    public GameObject panel;            // Panel that should be shown
    public UnityEngine.Events.UnityEvent onActivate; // Optional extra logic when activated
}

public class UISwitcher : MonoBehaviour
{
    [Tooltip("List of all tabs/categories in this switcher.")]
    public List<UISwitchEntry> entries = new List<UISwitchEntry>();

    [Tooltip("Play a sound on tab switch (optional).")]
    public AudioClip buttonPress;

    private string currentID;

    public static UISwitcher CreateFromChildren(Transform buttonParent, Transform panelParent)
    {
        // Optional helper: auto-generate from hierarchy
        var switcher = new GameObject("UI Switcher").AddComponent<UISwitcher>();
        for (int i = 0; i < buttonParent.childCount && i < panelParent.childCount; i++)
        {
            var entry = new UISwitchEntry
            {
                id = buttonParent.GetChild(i).name,
                button = buttonParent.GetChild(i).GetComponent<Button>(),
                panel = panelParent.GetChild(i).gameObject
            };
            switcher.entries.Add(entry);
        }
        return switcher;
    }

    private void Start()
    {
        foreach (var entry in entries)
        {
            var capturedEntry = entry;
            entry.button.onClick.AddListener(() =>
            {
                SetActiveTab(capturedEntry.id);
                if (buttonPress) AudioManager.Instance.PlaySound(buttonPress, 0.75f, 0.3f);
            });
        }

        if (entries.Count > 0)
            SetActiveTab(entries[0].id); // Default to first tab
    }

    public void SetActiveTab(string id)
    {
        currentID = id;

        foreach (var entry in entries)
        {
            bool active = entry.id == id;

            if (entry.panel) entry.panel.SetActive(active);
            if (entry.button) entry.button.interactable = !active;

            if (active)
                entry.onActivate?.Invoke();
        }
    }

    public void NextTab()
    {
        int index = entries.FindIndex(e => e.id == currentID);
        if (index >= 0)
            SetActiveTab(entries[(index + 1) % entries.Count].id);
    }

    public void PreviousTab()
    {
        int index = entries.FindIndex(e => e.id == currentID);
        if (index >= 0)
            SetActiveTab(entries[(index - 1 + entries.Count) % entries.Count].id);
    }
}
