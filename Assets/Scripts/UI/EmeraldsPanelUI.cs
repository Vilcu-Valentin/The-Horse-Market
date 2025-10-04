using UnityEngine;
using TMPro;
using System.Collections;

public class EmeraldsPanelUI : MonoBehaviour
{
    public enum CurrencyMode { Emeralds, LE }
    private CurrencyMode currentMode = CurrencyMode.Emeralds;

    [Header("Counter")]
    public TextMeshProUGUI coinCounter;

    [Header("Currency Images")]
    public GameObject emeraldImg;
    public GameObject LE_Img;

    [Header("Animation Settings")]
    public float animationDuration = 1.5f;

    [Header("Insufficient‐Funds Feedback")]
    public GameObject insufficientPopup;
    public Color flashErrorColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashRepeats = 3;

    // internal state
    private long initialNumber;
    private long targetNumber;
    private float animationTimer;
    private long lastSavedValue;
    private Color baseColor;

    void Start()
    {
        // initialize from current mode
        lastSavedValue = GetCurrentCurrency();
        initialNumber = lastSavedValue;
        targetNumber = lastSavedValue;
        animationTimer = 1f;
        coinCounter.text = lastSavedValue.ToShortString();

        baseColor = coinCounter.color;

        if (insufficientPopup != null)
            insufficientPopup.SetActive(false);
    }

    void Update()
    {
        // poll current currency based on mode
        long saved = GetCurrentCurrency();
        if (saved != lastSavedValue)
        {
            initialNumber = lastSavedValue;
            targetNumber = saved;
            animationTimer = 0f;
            lastSavedValue = saved;
        }

        // animate
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
    /// Returns the correct value depending on the active currency.
    /// </summary>
    private long GetCurrentCurrency()
    {
        if (currentMode == CurrencyMode.Emeralds)
            return SaveSystem.Instance.Current.emeralds;
        else
            return SaveSystem.Instance.Current.liquidEmeralds;
    }

    // insufficient feedback
    public void NotifyInsufficientEmeralds()
    {
        if (insufficientPopup != null)
            StartCoroutine(ShowPopupCoroutine());
        else
            StartCoroutine(FlashCounterCoroutine());
    }

    private IEnumerator ShowPopupCoroutine()
    {
        insufficientPopup.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        insufficientPopup.SetActive(false);
    }

    private IEnumerator FlashCounterCoroutine()
    {
        for (int i = 0; i < flashRepeats; i++)
        {
            float half = flashDuration * 0.5f;
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                coinCounter.color = Color.Lerp(baseColor, flashErrorColor, t / half);
                yield return null;
            }
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                coinCounter.color = Color.Lerp(flashErrorColor, baseColor, t / half);
                yield return null;
            }
        }
        coinCounter.color = baseColor;
    }

    // mode switching
    public void ToEmeralds()
    {
        currentMode = CurrencyMode.Emeralds;
        emeraldImg.SetActive(true);
        LE_Img.SetActive(false);
        ForceRefreshCounter();
    }

    public void ToLE()
    {
        currentMode = CurrencyMode.LE;
        emeraldImg.SetActive(false);
        LE_Img.SetActive(true);
        ForceRefreshCounter();
    }

    /// <summary>
    /// Refresh display immediately when switching currencies.
    /// </summary>
    private void ForceRefreshCounter()
    {
        lastSavedValue = GetCurrentCurrency();
        initialNumber = targetNumber = lastSavedValue;
        animationTimer = 1f;
        coinCounter.text = lastSavedValue.ToShortString();
    }
}
