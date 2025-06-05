using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class EnergyBarUI : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Number of total blocks (max energy).")]
    [Min(1)]
    public int maxEnergy = 5;

    [Tooltip("Current filled energy (clamped between 0 and maxEnergy).")]
    [Min(0)]
    public int currentEnergy = 0;

    [Tooltip("If true, blocks lay out left→right; if false, bottom→top.")]
    public bool horizontal = true;

    [Tooltip("Spacing in pixels between each block along the chosen axis.")]
    [Min(0f)]
    public float spacing = 0f;

    [Header("Sprites")]
    [Tooltip("Sprite to show when a block is filled.")]
    public Sprite filledSprite;
    public Color filledColor = Color.white;

    [Tooltip("Sprite to show when a block is empty/background.")]
    public Sprite emptySprite;
    public Color emptyColor = Color.white;

    [Header("References")]
    [Tooltip("The RectTransform that will contain all block-images. If left blank, this GameObject's RectTransform is used.")]
    public RectTransform containerRect;

    // Internal list of all block Image components
    private List<Image> blockImages = new List<Image>();

    // Keep track of last maxEnergy and last spacing to know when to rebuild
    private int lastMaxEnergy = -1;
    private float lastSpacing = -1f;
    private bool lastDirection = true;

    private void OnValidate()
    {
        // Whenever something changes in the Inspector (maxEnergy, direction, spacing, sprites, etc.), rebuild
        // Delay until after script recompiles so that everything is initialized.
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;

            // If maxEnergy, direction or spacing has changed, force a rebuild
            if (lastMaxEnergy != maxEnergy || lastDirection != horizontal || Mathf.Abs(lastSpacing - spacing) > Mathf.Epsilon)
            {
                RebuildAllBlocks();
            }
            UpdateVisuals();
        };
    }

    private void Awake()
    {
        if (containerRect == null)
            containerRect = GetComponent<RectTransform>();

        // Build only once at runtime start
        RebuildAllBlocks();
        UpdateVisuals();
    }

    /// <summary>
    /// Generates (or regenerates) the child Image blocks under containerRect so that there are exactly maxEnergy of them,
    /// spaced by 'spacing' pixels across width (if horizontal) or height (if vertical).
    /// </summary>
    private void RebuildAllBlocks()
    {
        if (containerRect == null)
        {
            Debug.LogError("EnergyBarUI: No RectTransform assigned or found on this GameObject.");
            return;
        }

        // Only rebuild if maxEnergy, direction, or spacing has changed
        if (lastMaxEnergy == maxEnergy && lastDirection == horizontal && Mathf.Abs(lastSpacing - spacing) < Mathf.Epsilon
            && blockImages.Count == maxEnergy)
        {
            return;
        }

        lastMaxEnergy = maxEnergy;
        lastDirection = horizontal;
        lastSpacing = spacing;

        // Destroy any existing children immediately (in editor also)
        for (int i = containerRect.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(containerRect.GetChild(i).gameObject);
        }
        blockImages.Clear();

        // For each block index, create a new UI Image and set anchors appropriately
        for (int i = 0; i < maxEnergy; i++)
        {
            GameObject go = new GameObject("Block_" + i, typeof(RectTransform));
            go.transform.SetParent(containerRect, false);

            Image img = go.AddComponent<Image>();
            img.sprite = emptySprite;
            img.type = Image.Type.Sliced;
            img.preserveAspect = false;
            img.color = emptyColor;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Compute anchorMin and anchorMax so that each block occupies 1/maxEnergy of the parent,
            // then shrink each block by 'spacing' pixels along the chosen axis.
            if (horizontal)
            {
                float widthSlice = 1f / maxEnergy;
                float xMin = i * widthSlice;
                float xMax = xMin + widthSlice;

                rt.anchorMin = new Vector2(xMin, 0f);
                rt.anchorMax = new Vector2(xMax, 1f);

                // Subtract 'spacing' pixels from the total width of this slice.
                // Because pivot is centered, the block will be shrunk equally from left and right,
                // resulting in exactly 'spacing' pixels between adjacent blocks.
                rt.sizeDelta = new Vector2(-spacing, 0f);
            }
            else
            {
                float heightSlice = 1f / maxEnergy;
                float yMin = i * heightSlice;
                float yMax = yMin + heightSlice;

                rt.anchorMin = new Vector2(0f, yMin);
                rt.anchorMax = new Vector2(1f, yMax);

                // Subtract 'spacing' pixels from the total height of this slice.
                // Because pivot is centered, the block will be shrunk equally from top and bottom,
                // resulting in exactly 'spacing' pixels between adjacent blocks.
                rt.sizeDelta = new Vector2(0f, -spacing);
            }

            blockImages.Add(img);
        }
    }

    /// <summary>
    /// Updates sprites on each block to match currentEnergy.
    /// </summary>
    private void UpdateVisuals()
    {
        if (blockImages == null || blockImages.Count != maxEnergy)
        {
            // If counts mismatch, rebuild first
            RebuildAllBlocks();
        }

        int clamped = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        for (int i = 0; i < blockImages.Count; i++)
        {
            if (i < clamped)
            {
                blockImages[i].sprite = filledSprite;
                blockImages[i].color = filledColor;
            }
            else
            {
                blockImages[i].sprite = emptySprite;
                blockImages[i].color = emptyColor;
            }
        }
    }

    /// <summary>
    /// Public API to set the current energy. Automatically clamps and updates visuals.
    /// </summary>
    /// <param name="value">New energy value.</param>
    public void SetEnergy(int value)
    {
        currentEnergy = Mathf.Clamp(value, 0, maxEnergy);
        UpdateVisuals();
    }

    /// <summary>
    /// Public API to change maxEnergy at runtime. This will rebuild all blocks.
    /// After calling this, you can immediately call SetEnergy().  
    /// </summary>
    /// <param name="newMax">New maximum energy (must be ≥ 1).</param>
    public void SetMaxEnergy(int newMax)
    {
        maxEnergy = Mathf.Max(1, newMax);
        RebuildAllBlocks();
        SetEnergy(currentEnergy); // reapply clamping & visuals
    }
}
