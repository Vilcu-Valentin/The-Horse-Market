using UnityEngine;
using TMPro;
using System.Collections;

public class EmeraldsPanelUI : MonoBehaviour
{
    [Header("Emeralds Counter")]
    public TextMeshProUGUI coinCounter;

    [Header("Animation Settings")]
    public float animationDuration = 1.5f;

    [Header("Insufficient‐Funds Feedback")]
    [Tooltip("Option A: assign a popup panel you want to show (could be a simple UI panel)")]
    public GameObject insufficientPopup;

    [Tooltip("Option B: flash coinCounter between originalColor and this color")]
    public Color flashErrorColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashRepeats = 3;

    //–– internal state for the tween ––
    private long initialNumber;     // where the animation started
    private long targetNumber;      // the value we’re animating to
    private float animationTimer;   // how far through the animation we are (0 → 1)
    private long lastSavedEmeralds;
    private Color baseColor;

    void Start()
    {
        // init emerald count display
        lastSavedEmeralds = SaveSystem.Instance.Current.emeralds;
        initialNumber = lastSavedEmeralds;
        targetNumber = lastSavedEmeralds;
        animationTimer = 1f;   // already “complete”
        coinCounter.text = lastSavedEmeralds.ToShortString();

        // cache the original text color for flashing
        baseColor = coinCounter.color;

        // hide popup if assigned
        if (insufficientPopup != null)
            insufficientPopup.SetActive(false);
    }

    void Update()
    {
        // 1) Poll the save system
        var saved = SaveSystem.Instance.Current.emeralds;
        if (saved != lastSavedEmeralds)
        {
            initialNumber = lastSavedEmeralds;
            targetNumber = saved;
            animationTimer = 0f;
            lastSavedEmeralds = saved;
        }

        // 2) Animate counter
        if (animationTimer < 1f)
        {
            animationTimer += Time.deltaTime / animationDuration;
            animationTimer = Mathf.Min(animationTimer, 1f);
            float t = 1f - Mathf.Pow(1f - animationTimer, 3f);
            long newDisplay = (long)Mathf.LerpUnclamped(initialNumber, targetNumber, t);
            coinCounter.text = newDisplay.ToShortString();
            if (animationTimer >= 1f)
                coinCounter.text = targetNumber.ToShortString();
        }
    }

    /// <summary>
    /// Call this when the player tries to spend more emeralds than they have.
    /// </summary>
    public void NotifyInsufficientEmeralds()
    {
        // OPTION A: pop up a dialog
        if (insufficientPopup != null)
        {
            StartCoroutine(ShowPopupCoroutine());
        }
        // OPTION B: flash the emerald counter text red
        else
        {
            StartCoroutine(FlashCounterCoroutine());
        }
    }

    // —— OPTION A: Popup Panel —— //
    private IEnumerator ShowPopupCoroutine()
    {
        insufficientPopup.SetActive(true);
        // if your popup has its own animator/fade, you can yield until it's done.
        // Here we'll just show it for 1.5 seconds.
        yield return new WaitForSeconds(1.5f);
        insufficientPopup.SetActive(false);
    }

    // —— OPTION B: Flash Text —— //
    private IEnumerator FlashCounterCoroutine()
    {
        for (int i = 0; i < flashRepeats; i++)
        {
            // lerp to error color
            float half = flashDuration * 0.5f;
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                coinCounter.color = Color.Lerp(baseColor, flashErrorColor, t / half);
                yield return null;
            }
            // lerp back to base
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                coinCounter.color = Color.Lerp(flashErrorColor, baseColor, t / half);
                yield return null;
            }
        }
        // ensure we end on the original color
        coinCounter.color = baseColor;
    }
}
