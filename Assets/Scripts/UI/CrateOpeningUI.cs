using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class CrateOpeningUI : MonoBehaviour
{
    [Header("References")]
    public GameObject panel;         // Root panel (inactive by default)
    public ScrollRect scrollRect;    // Your ScrollRect
    public RectTransform content;       // Content under ScrollRect.Viewport
    public GameObject itemPrefab;    // Prefab: must have an Image component

    [Header("Animation Settings")]
    [Tooltip("Total spin time before final reel.")]
    public float duration = 5f;
    [Tooltip("Time between item steps.")]
    public float spawnInterval = 0.1f;

    [Header("Data")]
    [Tooltip("Weighted list of possible tier icons.")]
    public List<WeightedSprite> weightedSprites;

    [Header("Events")]
    public UnityEvent onAnimationComplete;

    // internal
    float _itemWidth;
    float _spacing;
    int _visibleCount;
    Coroutine _running;

    void Awake()
    {
        // Measure your slot width + spacing (assumes prefab and layout are set up)
        var rt = itemPrefab.GetComponent<RectTransform>();
        _itemWidth = rt.rect.width;
        var hlg = content.GetComponent<HorizontalLayoutGroup>();
        _spacing = hlg != null ? hlg.spacing : 0f;

        // How many items fit in viewport (plus a bit of buffer)
        float vw = scrollRect.viewport.rect.width;
        _visibleCount = Mathf.CeilToInt(vw / (_itemWidth + _spacing)) + 2;
    }

    /// <summary>
    /// Call this to run the reel. It will spin for `duration` seconds, then land your finalSprite in the center.
    /// </summary>
    public void PlayOpenAnimation(Sprite finalSprite)
    {
        if (_running != null) StopCoroutine(_running);
        panel.SetActive(true);
        _running = StartCoroutine(RunReel(finalSprite));
    }

    private IEnumerator RunReel(Sprite finalSprite)
    {
        // 1) Seed initial items
        ClearContent();
        for (int i = 0; i < _visibleCount; i++)
            SpawnItem(PickRandomSprite());

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // 2) Spin phase: continuously spawn → move → destroy front
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // a) spawn new random at end
            Sprite s = PickRandomSprite();
            SpawnItem(s);

            // b) animate one step left
            yield return AnimateStep(spawnInterval);

            // c) destroy the leftmost
            Destroy(content.GetChild(0).gameObject);
            content.anchoredPosition = Vector2.zero;

            elapsed += spawnInterval;
        }

        // 3) Final step: land on `finalSprite`
        SpawnItem(finalSprite);
        yield return AnimateStep(spawnInterval);
        Destroy(content.GetChild(0).gameObject);
        content.anchoredPosition = Vector2.zero;

        // 4) Pause so the player sees it
        yield return new WaitForSeconds(1f);

        // 5) Tear down
        panel.SetActive(false);
        onAnimationComplete?.Invoke();
        _running = null;
    }

    private void ClearContent()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    private void SpawnItem(Sprite spr)
    {
        var go = Instantiate(itemPrefab, content);
        go.GetComponent<Image>().sprite = spr;
    }

    private IEnumerator AnimateStep(float stepTime)
    {
        // Move content.anchoredPosition.x from 0 → -(itemWidth+spacing) with ease-out
        Vector2 start = content.anchoredPosition;
        Vector2 end = start + Vector2.left * (_itemWidth + _spacing);

        float t = 0f;
        while (t < stepTime)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / stepTime);
            // ease-out quad: f(x)=1-(1-x)^2
            float eased = 1f - (1f - norm) * (1f - norm);
            content.anchoredPosition = Vector2.Lerp(start, end, eased);
            yield return null;
        }
        content.anchoredPosition = end;
    }

    private Sprite PickRandomSprite()
    {
        // Project your List<WeightedSprite> into the form WeightedSelector wants:
        var weightedList = weightedSprites
            .Select(ws => (item: ws, weight: ws.weight));

        // Now call Pick with the tuple sequence
        var chosen = WeightedSelector<WeightedSprite>.Pick(weightedList);

        return chosen.sprite;
    }

    [System.Serializable]
    public struct WeightedSprite
    {
        public Sprite sprite;
        public int weight;
    }
}
