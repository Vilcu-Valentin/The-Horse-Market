using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[AddComponentMenu("Layout/Horizontal Table Layout Group")]
public class HorizontalTableLayoutGroup : HorizontalOrVerticalLayoutGroup
{
    [SerializeField] private int columnCount = 1;
    [SerializeField] private List<float> columnWidths = new List<float>() { 1f };

    /// <summary>
    /// Number of logical columns.
    /// </summary>
    public int ColumnCount
    {
        get => columnCount;
        set => SetProperty(ref columnCount, Mathf.Max(1, value));
    }

    /// <summary>
    /// Normalized widths for each column. Should sum to 1 (or will be normalized internally).
    /// </summary>
    public List<float> ColumnWidths
    {
        get => columnWidths;
        set => SetProperty(ref columnWidths, value);
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        // Force this group to be “flexible” so it always stretches to its parent
        float pad = padding.horizontal;
        SetLayoutInputForAxis(pad, pad, 1, 0);
    }

    public override void CalculateLayoutInputVertical()
    {
        // compute min/preferred/flexible height just like a HorizontalLayoutGroup
        CalcAlongAxis(1, false);
    }

    public override void SetLayoutHorizontal()
    {
        ArrangeCells(0);
    }

    public override void SetLayoutVertical()
    {
        ArrangeCells(1);
    }

    private void ArrangeCells(int axis)
    {
        if (axis == 0)
        {
            // ─── HORIZONTAL ───
            int cols = Mathf.Max(1, columnCount);

            // ensure our list matches the column count
            if (columnWidths.Count != cols)
            {
                // re-build with defaults or old values
                var newList = new List<float>(cols);
                for (int i = 0; i < cols; i++)
                    newList.Add(i < columnWidths.Count ? columnWidths[i] : (1f / cols));
                columnWidths = newList;
            }

            // normalize
            float sum = columnWidths.Sum();
            if (sum <= 0) sum = cols;

            float totalW = rectTransform.rect.width;
            float availW = totalW - padding.horizontal - spacing * (cols - 1);

            // compute each column’s pixel width
            float[] colPx = new float[cols];
            for (int i = 0; i < cols; i++)
                colPx[i] = availW * (columnWidths[i] / sum);

            // lay out each child in its column slot
            int childCount = rectChildren.Count;
            for (int i = 0; i < childCount && i < cols; i++)
            {
                var child = rectChildren[i];
                // start position:
                float x = padding.left + i * spacing + colPx.Take(i).Sum();

                // determine child width
                float cw = colPx[i];
                float childW = childControlWidth
                               ? cw
                               : Mathf.Clamp(LayoutUtility.GetPreferredWidth(child), 0, cw);
                if (childForceExpandWidth)
                    childW = cw;

                SetChildAlongAxis(child, 0, x, childW);
            }
        }
        else
        {
            // ─── VERTICAL ───
            float totalH = rectTransform.rect.height;
            float availH = totalH - padding.vertical;

            // vertical alignment factor from childAlignment
            float alignY = childAlignment switch
            {
                TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight => 1f,
                TextAnchor.MiddleLeft or TextAnchor.MiddleCenter or TextAnchor.MiddleRight => 0.5f,
                _ => 0f
            };

            int childCount = rectChildren.Count;
            for (int i = 0; i < childCount; i++)
            {
                var child = rectChildren[i];
                // choose height
                float ch = childControlHeight
                           ? availH
                           : LayoutUtility.GetPreferredHeight(child);
                if (childForceExpandHeight)
                    ch = availH;

                // position so that 1=top-aligned, 0=bottom-aligned
                float y = padding.top + (availH - ch) * (1f - alignY);
                SetChildAlongAxis(child, 1, y, ch);
            }
        }
    }
}
