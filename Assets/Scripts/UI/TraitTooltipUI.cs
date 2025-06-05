using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TraitTooltipUI : MonoBehaviour
{
    public TMP_Text traitName;
    public TMP_Text description;

    public void InitTraitUI(TraitDef trait)
    {
        traitName.text = trait.DisplayName;
        description.text = trait.Description;  
    }
}
