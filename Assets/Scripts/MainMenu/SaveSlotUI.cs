using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI saveNameText;
    public TextMeshProUGUI lastSavedText;
    public TextMeshProUGUI emeraldsText;
    public TextMeshProUGUI horsesText;
    public Button loadButton;
    public Button deleteButton;

    private SaveSlotInfo saveSlot;
    private System.Action<SaveSlotInfo> onLoadCallback;
    private System.Action<SaveSlotInfo> onDeleteCallback;

    public void Setup(SaveSlotInfo saveSlotInfo, System.Action<SaveSlotInfo> onLoad, System.Action<SaveSlotInfo> onDelete)
    {
        saveSlot = saveSlotInfo;
        onLoadCallback = onLoad;
        onDeleteCallback = onDelete;

        // Update UI elements
        if (saveNameText != null)
            saveNameText.text = saveSlot.saveName;

        if (lastSavedText != null)
        {
            TimeSpan timeDiff = DateTime.Now - saveSlot.lastSaved;
            string timeText;

            if (timeDiff.TotalMinutes < 1)
                timeText = "Just now";
            else if (timeDiff.TotalHours < 1)
                timeText = $"{(int)timeDiff.TotalMinutes} minutes ago";
            else if (timeDiff.TotalDays < 1)
                timeText = $"{(int)timeDiff.TotalHours} hours ago";
            else if (timeDiff.TotalDays < 7)
                timeText = $"{(int)timeDiff.TotalDays} days ago";
            else
                timeText = saveSlot.lastSaved.ToString("MMM dd, yyyy");

            lastSavedText.text = timeText;
        }

        if (emeraldsText != null)
            emeraldsText.text = $"{saveSlot.emeralds:N0}";

        if (horsesText != null)
            horsesText.text = $"{saveSlot.horseCount}"; 

        // Setup buttons
        if (loadButton != null)
            loadButton.onClick.AddListener(() => onLoadCallback?.Invoke(saveSlot));

        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => ConfirmDelete());
    }

    private void ConfirmDelete()
    {
        if (Application.isEditor || Debug.isDebugBuild)
        {
            Debug.Log($"Deleting save: {saveSlot.saveName}");
            onDeleteCallback?.Invoke(saveSlot);
        }
        else
        {
            // For now, just delete immediately
            onDeleteCallback?.Invoke(saveSlot);
        }
    }
}