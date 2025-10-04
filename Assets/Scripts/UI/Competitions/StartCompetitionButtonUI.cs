using UnityEngine;
using UnityEngine.EventSystems;

public class StartCompetitionButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Drag your Fee Panel here")]
    public GameObject feePanel;

    private void Awake()
    {
        // Make sure it's off by default
        if (feePanel != null)
            feePanel.SetActive(false);
    }

    // Called when the mouse (or touch) starts hovering this UI element
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (feePanel != null)
            feePanel.SetActive(true);
    }

    // Called when the mouse (or touch) stops hovering this UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        if (feePanel != null)
            feePanel.SetActive(false);
    }
}
