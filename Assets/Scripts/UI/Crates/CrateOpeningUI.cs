using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class CrateOpeningUI : MonoBehaviour
{
    [Tooltip("Speed in pixels per second (right→left).")]
    public float speed = 100f;

    [Tooltip("Local X-position at which an item is considered off-screen on the left.")]
    public float thresholdX = -600f;

    [Tooltip("Minimum spin duration in seconds.")]
    public float minSpinDuration = 3f;

    [Tooltip("Maximum spin duration in seconds.")]
    public float maxSpinDuration = 5f;

    [Tooltip("Max random offset (+/-) from the horizontal center of the final item.")]
    public float stopOffsetRange = 50f;

    [Tooltip("Local X-position of the selector center (stop line).")]
    public float selectorX = 0f;

    [Tooltip("How long after slowing finishes before the panel closes.")]
    public float panelCloseDelay = 1.5f;

    [Tooltip("Parent panel for activating and deactivating it")]
    public GameObject parentPanel;

    public RectTransform _rect;
    public List<RectTransform> _items;
    public HorizontalLayoutGroup _layoutGroup;

    public event Action OpeningFinished;

    private float _itemWidth;
    private float _spacing;

    private enum SpinState { Idle, Spinning, SlowingDown }
    private SpinState _state = SpinState.Idle;

    private float _spinStartTime;
    private float _spinDuration;
    private float _initialSpeed;
    private float _slowStartTime;
    private float _decelDuration;

    private RectTransform _stopItem;
    private float _stopTargetX;

    private Sprite selectedTier;
    private List<(WeightedTier tier, int weight)> crateValues;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);

        if (_items == null || _items.Count == 0)
        {
            Debug.LogWarning("CrateOpeningUI: No items assigned.");
            return;
        }

        _itemWidth = _items[0].sizeDelta.x;
        _spacing = _layoutGroup != null ? _layoutGroup.spacing : 0f;
    }

    /// <summary>
    /// Call this to begin the spin.
    /// </summary>
    public void StartSpin(TierDef selectedTier, List<(WeightedTier tier, int weight)> values)
    {
        if (_state != SpinState.Idle) return;
        parentPanel.SetActive(true);

        this.selectedTier = selectedTier.tierIcon;
        crateValues = values;

        // Disable layout to avoid jerks
        if (_layoutGroup != null)
            _layoutGroup.enabled = false;

        // Reset all images' brightness to HSV value = 0.5
        foreach (var rt in _items)
        {
            var img = rt.GetComponent<Image>();
            if (img == null) continue;
            Color.RGBToHSV(img.color, out float h, out float s, out float v);
            img.color = Color.HSVToRGB(h, s, 0.5f);
            img.sprite = weightedImageSelector(values);
        }

        // Prepare timing
        _spinDuration = UnityEngine.Random.Range(minSpinDuration, maxSpinDuration);
        _spinStartTime = Time.time;
        _initialSpeed = speed;
        _state = SpinState.Spinning;
    }

    void Update()
    {
        if (_state == SpinState.Idle || _items == null) return;

        float dt = Time.deltaTime;
        float moveDistance = 0f;

        if (_state == SpinState.Spinning)
        {
            if (Time.time - _spinStartTime >= _spinDuration)
                BeginSlowdown();

            moveDistance = speed * dt;
        }
        else if (_state == SpinState.SlowingDown)
        {
            float t = (Time.time - _slowStartTime) / _decelDuration;
            // Compute eased speed, clamped
            float currentSpeed = Mathf.Lerp(_initialSpeed, 0f, Mathf.Min(t, 1f));
            moveDistance = currentSpeed * dt;
        }

        // 1) Move all items and record wraps
        var toWrap = new List<RectTransform>();
        foreach (var rt in _items)
        {
            var p = rt.anchoredPosition;
            p.x -= moveDistance;
            rt.anchoredPosition = p;

            if (p.x < thresholdX)
                toWrap.Add(rt);
        }

        // 2) Compute a stable base maxX _before_ any re‐positioning
        float baseMaxX = _items.Max(x => x.anchoredPosition.x);

        // 3) Reposition each wrapped item in a consistent chain
        foreach (var rt in toWrap)
        {
            baseMaxX += _itemWidth + _spacing;
            rt.anchoredPosition = new Vector2(baseMaxX, rt.anchoredPosition.y);

            // reassign sprite if you need to
            rt.GetComponent<Image>().sprite = weightedImageSelector(crateValues);
        }

        // Highlight current overlap
        foreach (var rt in _items)
        {
            var img = rt.GetComponent<Image>();
            if (img == null) continue;
            float halfW = _itemWidth * 0.5f;
            bool overlap = Mathf.Abs(rt.anchoredPosition.x - selectorX) <= halfW;
            Color.RGBToHSV(img.color, out float h, out float s, out float v);
            img.color = Color.HSVToRGB(h, s, overlap ? 1f : 0.5f);
        }
    }

    private void BeginSlowdown()
    {
        _state = SpinState.SlowingDown;
        _slowStartTime = Time.time;

        // Pick rightmost existing item
        _stopItem = _items.OrderByDescending(x => x.anchoredPosition.x).First();

        // Determine stop target with random offset
        float offset = UnityEngine.Random.Range(-stopOffsetRange, stopOffsetRange);
        _stopTargetX = selectorX + offset;

        // Compute decelDuration so it eases into place
        float startX = _stopItem.anchoredPosition.x;
        float distance = startX - _stopTargetX;
        _decelDuration = distance > 0f ? (2f * distance) / _initialSpeed : 0.01f;

        // Debug: set final sprite
        var dbgImg = _stopItem.GetComponent<Image>();
        if (dbgImg != null)
            dbgImg.sprite = selectedTier;

        // Schedule panel close after slowdown + delay
        Invoke(nameof(EndSpin), _decelDuration + panelCloseDelay);
    }

    public void EndSpin()
    {
        _state = SpinState.Idle;
        CancelInvoke();
        parentPanel.SetActive(false);
        OpeningFinished?.Invoke();
        // Optionally re-enable layout
        // if (_layoutGroup != null) _layoutGroup.enabled = true;
    }

    private Sprite weightedImageSelector(List<(WeightedTier tier, int weight)> values)
    {
        return WeightedSelector<WeightedTier>.Pick(values).Tier.tierIcon;
    }
}