using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class BreedingUpgradeDialogPanelUI : DialogPanelUI
{
    public TMP_Text oldTierText;
    public TMP_Text newTierText;

    /// <summary>
    /// Shows the dialog with an old‐tier/new‐tier display.
    /// </summary>
    public void Show(string message,
                     string oldTier,   
                     string newTier,        
                     Action confirmCallback,
                     Action cancelCallback = null)
    {
        // call base to do all the standard wiring
        base.Show(message, confirmCallback, cancelCallback);

        // now plug in your extra two text fields
        oldTierText.text = oldTier;
        newTierText.text = newTier;
    }
}
