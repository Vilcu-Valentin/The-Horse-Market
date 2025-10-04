using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "HorseGame/Ascension Trait")]
public class AscensionTraitDef : TraitDef
{
    [Header("Ascensions")]

    public bool usesEnergy = true;

    [Tooltip("Can't be consumed while breeding")]
    public bool immortal = false;

    [Tooltip("After each training the traits get rerolled")]
    public bool harbinger = false;

    [Tooltip("Produces LE after each competition")]
    public bool radiant = false;
}
