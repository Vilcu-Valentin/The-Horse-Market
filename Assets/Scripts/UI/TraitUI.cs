using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TraitUI : MonoBehaviour
{
    public Image traitImage;
    public Image borderImage;

    public void InitTrait(TraitDef trait)
    {
        traitImage.sprite = trait.Icon;
        borderImage.sprite = trait.BorderIcon;
    }
}
