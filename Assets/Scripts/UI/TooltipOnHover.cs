using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 1) Must be on the same GameObject as some ITooltipProvider implementation.
/// 2) OnPointerEnter: waits `delay` seconds → asks provider for a prefab → instantiates it → calls provider.PopulateTooltip(...) → positions it.
/// 3) Keeps the tooltip on‐screen and makes it follow the mouse.
/// 4) OnPointerExit: destroys the tooltip.
/// 
/// NOTE: This script assumes your Canvas is Screen‐Space — Overlay. If you’re using a Screen‐Space Camera or World Space canvas,
///       you can adapt the “position” logic accordingly (like we did in earlier steps). Here we’ll assume Overlay mode for simplicity.
/// </summary>
public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Basic Settings (for any tooltip)")]
    [Tooltip("This offset is applied to the tooltip’s TOP‐LEFT corner (in screen pixels).")]
    public Vector2 offset = Vector2.zero;

    [Tooltip("Seconds to wait after hover begins before spawning the tooltip.")]
    public float delay = 0.5f;

    // ──────────── Internal State ────────────
    private Canvas _parentCanvas;
    private RectTransform _parentCanvasRect;
    private GameObject _tooltipInstance;
    private RectTransform _tooltipRect;
    private Coroutine _showCoroutine;
    private bool _isHovering;
    private Camera _uiEventCamera;      // the camera used by the UI raycast (null if Overlay)
    private ITooltipProvider _tooltipProvider;   // who supplies prefab + data?

    private void Awake()
    {
        // 1) Find the nearest Canvas up the hierarchy
        _parentCanvas = GetComponentInParent<Canvas>();
        if (_parentCanvas == null)
        {
            Debug.LogWarning($"[{name}] TooltipOnHover: No Canvas found in parents. Tooltips will not work.");
            return;
        }

        _parentCanvasRect = _parentCanvas.GetComponent<RectTransform>();
        if (_parentCanvasRect == null)
        {
            Debug.LogWarning($"[{name}] TooltipOnHover: Canvas has no RectTransform. Tooltips will not work.");
            _parentCanvas = null;
        }

        // 2) Look for any ITooltipProvider on the same GameObject
        _tooltipProvider = GetComponent<ITooltipProvider>();
        if (_tooltipProvider == null)
        {
            Debug.LogWarning($"[{name}] TooltipOnHover: No ITooltipProvider found on this object. ${(name)} will hover but show an empty tooltip.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_parentCanvas == null || _tooltipProvider == null)
            return;

        _isHovering = true;
        _uiEventCamera = eventData.enterEventCamera;
        if (_showCoroutine != null)
            StopCoroutine(_showCoroutine);

        _showCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }
        HideTooltip();
    }

    private System.Collections.IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        if (!_isHovering || _parentCanvas == null)
            yield break;

        // 1) Ask provider which prefab to use.
        GameObject prefab = _tooltipProvider.GetTooltipPrefab();
        if (prefab == null)
        {
            Debug.LogWarning($"[{name}] TooltipOnHover: Provider returned null prefab. No tooltip will be shown.");
            yield break;
        }

        // 2) Instantiate the prefab under the Canvas (worldPositionStays = false so it uses prefab pivot/anchors exactly)
        _tooltipInstance = Instantiate(prefab, _parentCanvas.transform, false);
        _tooltipRect = _tooltipInstance.GetComponent<RectTransform>();
        if (_tooltipRect == null)
        {
            Debug.LogWarning($"[{name}] TooltipOnHover: The prefab you provided has no RectTransform.");
        }

        // 3) Disable raycasts on the entire tooltip so that it never steals pointer events
        var cg = _tooltipInstance.GetComponent<CanvasGroup>();
        if (cg == null) cg = _tooltipInstance.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // 4) Let the provider fill in ALL the UI fields
        _tooltipProvider.PopulateTooltip(_tooltipInstance);

        // 5) Immediately position it once, and then Update() will keep it following
        UpdateTooltipPosition();
    }

    private void Update()
    {
        if (_tooltipInstance != null)
            UpdateTooltipPosition();
    }

    private void UpdateTooltipPosition()
    {
        if (_tooltipRect == null || _parentCanvas == null)
            return;

        // We assume “Screen Space – Overlay.” In that mode, setting tooltipRect.position = (x,y) 
        // directly places its pivot at pixel (x,y). But we want to anchor TOP‐LEFT at (mouse + offset).
        // So we’ll:
        //   1. Read the tooltip’s size in pixels: (widthPx, heightPx) = rect.width*scale, rect.height*scale
        //   2. Let (mouseX+offset.x, mouseY+offset.y) = the SCREEN‐PIXEL coordinate where we want the TOP‐LEFT corner to be.
        //   3. Convert from “TOP‐LEFT pixel” to “pivot pixel” using the tooltip’s pivot. Then assign _tooltipRect.position.

        Vector2 mousePos = Input.mousePosition;
        float offX = offset.x;
        float offY = offset.y;

        // Tooltip size in screen pixels:
        float scaleFactor = _parentCanvas.scaleFactor;
        float widthPx = _tooltipRect.rect.width * scaleFactor;
        float heightPx = _tooltipRect.rect.height * scaleFactor;
        Vector2 pivot = _tooltipRect.pivot;     // e.g. (0,1) if pivot is top-left

        float screenW = Screen.width;
        float screenH = Screen.height;

        // A) Compute the desired TOP‐LEFT coordinate = (mouse + offset)
        float tlX = mousePos.x + offX;
        float tlY = mousePos.y + offY;

        // B) If the right edge would go off the screen, flip to left side of cursor:
        if (tlX + widthPx > screenW)
        {
            // Move top-left so that the tooltip’s right edge = (mouse.x - offX).
            // In other words, place the entire tooltip to the left of the cursor.
            tlX = mousePos.x - offX - widthPx;
        }
        // Clamp to left border if still < 0
        if (tlX < 0f)
            tlX = 0f;

        // C) If the top edge would go off the top of the screen, flip it BELOW the cursor:
        if (tlY > screenH)
        {
            // Place the bottom edge = (mouseY - offY). 
            // That means top-left Y = (mouseY - offY - heightPx).
            tlY = mousePos.y - offY - heightPx;
        }
        // If the bottom edge (tlY - heightPx) < 0, flip it above the cursor:
        if (tlY - heightPx < 0f)
        {
            // Put top‐left’s Y at (mouseY - offY + heightPx).
            tlY = mousePos.y - offY + heightPx;
        }
        // Finally clamp TL Y into [heightPx, screenH]
        tlY = Mathf.Clamp(tlY, heightPx, screenH);

        // D) Convert from “TL = (tlX,tlY)” to “PivotPos = ?”
        //    TL.x = pivotPos.x - (pivot.x * widthPx)   → pivotPos.x = TL.x + pivot.x*widthPx
        //    TL.y = pivotPos.y + ((1 - pivot.y) * heightPx) 
        //         (because Unity's UI Y‐axis goes up, so top‐left corner is +((1 - pivot.y)*height))
        float pivotX = tlX + pivot.x * widthPx;
        float pivotY = tlY - (1f - pivot.y) * heightPx;

        _tooltipRect.position = new Vector3(pivotX, pivotY, 0f);
    }

    private void HideTooltip()
    {
        if (_tooltipInstance != null)
        {
            Destroy(_tooltipInstance);
            _tooltipInstance = null;
            _tooltipRect = null;
        }
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void OnDestroy()
    {
        HideTooltip();
    }

}
