using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "HorseGame/Visual")]
public class VisualDef : ScriptableObject
{
    public HorseColor horseColor;
    public Color textColor;
    public Sprite sprite2D;
    [Tooltip("The lower this number, the rarer they are")]
    public int rarityTickets;

    [Tooltip("Scalar on market price (1 = neutral)")]
    public float PriceScalar = 1f;
}
