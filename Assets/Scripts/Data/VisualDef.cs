using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(menuName = "HorseGame/Visual")]
public class VisualDef : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;
    public string ID => id;

    public HorseColor horseColor;
    public Color textColor;
    public Sprite sprite2D;
    [Tooltip("The lower this number, the rarer they are")]
    public int rarityTickets;

    [Tooltip("Scalar on market price (1 = neutral)")]
    public float PriceScalar = 1f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }
#endif
}
