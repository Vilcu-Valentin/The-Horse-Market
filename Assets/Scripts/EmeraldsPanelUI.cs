using UnityEngine;
using TMPro;

public class EmeraldsPanelUI : MonoBehaviour
{
    [Header("Emeralds Counter")]
    public TextMeshProUGUI coinCounter;
    [Header("Animation Settings")]
    public float animationDuration = 1.5f;

    //–– internal state for the tween ––
    private long initialNumber;     // where the animation started
    private long targetNumber;      // the value we’re animating to
    private float animationTimer;    // how far through the animation we are (0 → 1)

    //–– cached “last seen” emerald count ––
    private long lastSavedEmeralds;

    void Start()
    {
        // initialize everything to whatever is in the save file right now
        lastSavedEmeralds = SaveSystem.Instance.Current.emeralds;
        initialNumber = lastSavedEmeralds;
        targetNumber = lastSavedEmeralds;
        animationTimer = 1f;   // already “complete”

        coinCounter.text = lastSavedEmeralds.ToShortString();
    }

    void Update()
    {
        // 1) Poll the save system
        var saved = SaveSystem.Instance.Current.emeralds;
        if (saved != lastSavedEmeralds)
        {
            // emerald count changed in the save!
            // reset animation from whatever we’re _currently_ displaying
            initialNumber = lastSavedEmeralds;
            targetNumber = saved;
            animationTimer = 0f;
            lastSavedEmeralds = saved;
        }

        // 2) If we’re mid-animation, advance it
        if (animationTimer < 1f)
        {
            animationTimer += Time.deltaTime / animationDuration;
            animationTimer = Mathf.Min(animationTimer, 1f);

            // simple ease-out (optional)
            float t = 1f - Mathf.Pow(1f - animationTimer, 3f);

            long newDisplay = (long)Mathf.LerpUnclamped(
                initialNumber,
                targetNumber,
                t
            );
            coinCounter.text = newDisplay.ToShortString();

            // if we’ve reached the end, snap exactly
            if (animationTimer >= 1f)
                coinCounter.text = targetNumber.ToShortString();
        }
    }
}
