using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CrateOpeningUI : MonoBehaviour
{
    [Tooltip("Speed in pixels per second (right→left).")]
    public float speed = 100f;

    [Tooltip("Local X‑position at which an item is considered off‑screen on the left.")]
    public float thresholdX = -600f;

    [Tooltip("Minimum spin duration in seconds.")]
    public float minSpinDuration = 3f;

    [Tooltip("Maximum spin duration in seconds.")]
    public float maxSpinDuration = 5f;

    [Tooltip("Max random offset (+/-) from the horizontal center of the final item.")]
    public float stopOffsetRange = 50f;

    [Tooltip("Local X‑position of the selector center (stop line).")]
    public float selectorX = 0f;

    public RectTransform _rect;
    public List<RectTransform> _items;
    public HorizontalLayoutGroup _layoutGroup;

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
    public void StartSpin()
    {
        if (_state != SpinState.Idle) return;

        // Turn off layout to avoid jerks
        if (_layoutGroup != null)
            _layoutGroup.enabled = false;

        // Reset colors: dim all items to HSV value=0.5
        foreach (var rt in _items)
        {
            var img = rt.GetComponent<Image>();
            if (img != null)
            {
                Color.RGBToHSV(img.color, out float h, out float s, out float v);
                img.color = Color.HSVToRGB(h, s, 0.5f);
            }
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
            if (t >= 1f)
            {
                EndSpin();
                return;
            }
            float currentSpeed = Mathf.Lerp(_initialSpeed, 0f, t);
            moveDistance = currentSpeed * dt;
        }

        // Move and wrap all items
        foreach (var rt in _items)
        {
            Vector2 p = rt.anchoredPosition;
            p.x -= moveDistance;
            rt.anchoredPosition = p;

            if (p.x < thresholdX)
            {
                float maxX = _items.Max(x => x.anchoredPosition.x);
                p.x = maxX + _itemWidth + _spacing;
                rt.anchoredPosition = p;
            }
        }

        // Highlight overlap during spin
        foreach (var rt in _items)
        {
            var img = rt.GetComponent<Image>();
            if (img == null) continue;
            float halfW = _itemWidth * 0.5f;
            // If any pixel of item overlaps selectorX
            if (Mathf.Abs(rt.anchoredPosition.x - selectorX) <= halfW)
            {
                Color.RGBToHSV(img.color, out float h, out float s, out float v);
                img.color = Color.HSVToRGB(h, s, 1.0f);
            }
            else
            {
                Color.RGBToHSV(img.color, out float h, out float s, out float v);
                img.color = Color.HSVToRGB(h, s, 0.5f);
            }
        }
    }

    private void BeginSlowdown()
    {
        _state = SpinState.SlowingDown;
        _slowStartTime = Time.time;

        // Pick rightmost existing item
        _stopItem = _items.OrderByDescending(x => x.anchoredPosition.x).First();

        // Determine stop target with optional offset
        float offset = UnityEngine.Random.Range(-stopOffsetRange, stopOffsetRange);
        _stopTargetX = selectorX + offset;

        // Calculate decel time so item eases into place
        float startX = _stopItem.anchoredPosition.x;
        float distance = startX - _stopTargetX;
        _decelDuration = distance > 0f ? (2f * distance) / _initialSpeed : 0.01f;

        // Debug: tint selected item red
        var dbgImg = _stopItem.GetComponent<Image>();
        if (dbgImg != null)
            dbgImg.color = new Color(1f, 0.5f, 0.5f);
    }

    private void EndSpin()
    {
        _state = SpinState.Idle;
        // Optionally re-enable layout group
        // if (_layoutGroup != null)
        //     _layoutGroup.enabled = true;
    }
}
