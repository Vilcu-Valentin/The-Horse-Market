using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogPanelUI : MonoBehaviour
{
    public TMP_Text messageText;
    public Button confirmButton;
    public Button cancelButton;

    private Action onConfirm, onCancel;

    public void Show(string message,
                     Action confirmCallback,
                     Action cancelCallback = null)
    {
        gameObject.SetActive(true);
        messageText.text = message;
        onConfirm = confirmCallback;
        onCancel = cancelCallback;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => {
            Hide();
            onConfirm?.Invoke();
        });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => {
            Hide();
            onCancel?.Invoke();
        });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
