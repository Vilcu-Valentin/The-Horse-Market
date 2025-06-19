using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class HelpTooltipUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text description;           // Your single TMP_Text field
    public RectTransform backgroundRect;   // The panel/image you want to resize

    [Header("Sizing Settings")]
    public float minWidth = 150f;
    public float maxWidth = 300f;
    public float minHeight = 60f;
    public float panelPadding = 8f;        // Space inside panel around text

    /// <summary>
    /// Call this to set text (with colors & sizes) and resize the panel.
    /// </summary>
    public void InitUI(string information)
    {

        description.text = information;

        // 2) Force TMPro to recalc
        Canvas.ForceUpdateCanvases();

        // 3) Grab TMP margins
        Vector4 m = description.margin;
        float horizMargin = m.x + m.z;
        float vertMargin = m.y + m.w;

        // 4) Compute available text width
        float contentMaxW = maxWidth - (panelPadding * 2) - horizMargin;

        // 5) Measure wrapped text size
        Vector2 textSz = description.GetPreferredValues(
            description.text,
            contentMaxW,
            float.PositiveInfinity
        );

        // 6) Clamp final panel size
        float finalW = Mathf.Clamp(
            textSz.x + (panelPadding * 2) + horizMargin,
            minWidth,
            maxWidth
        );
        float finalH = Mathf.Max(
            textSz.y + (panelPadding * 2) + vertMargin,
            minHeight
        );

        // 7) Apply to panel
        backgroundRect.sizeDelta = new Vector2(finalW, finalH);
    }
}
