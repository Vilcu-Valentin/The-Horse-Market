using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "HorseGame/Visual")]
public class VisualDef : ScriptableObject
{
    public HorseColor horseColor;
    public Sprite sprite2D;
    //Rarity

    [Tooltip("Scalar on market price (1 = neutral)")]
    public float PriceScalar = 1f;
}
