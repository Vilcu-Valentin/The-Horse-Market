using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("Layout/Auto Resize Grid Layout Group", 152)]
public class AutoResizeGridLayoutGroup : LayoutGroup
{
    [SerializeField] private int m_Columns = 1;
    [SerializeField] private int m_Rows = 1;
    [SerializeField] private Vector2 m_Spacing = Vector2.zero;

    [SerializeField]
    private Vector2 m_CellSize = Vector2.zero;


    public enum Corner { UpperLeft = 0, UpperRight = 1, LowerLeft = 2, LowerRight = 3 }
    public enum Axis { Horizontal = 0, Vertical = 1 }

    [SerializeField] private Corner m_StartCorner = Corner.UpperLeft;
    [SerializeField] private Axis m_StartAxis = Axis.Horizontal;

    // Exposed properties (with change-notification):
    public int columns { get { return m_Columns; } set { SetProperty(ref m_Columns, Mathf.Max(1, value)); } }
    public int rows { get { return m_Rows; } set { SetProperty(ref m_Rows, Mathf.Max(1, value)); } }
    public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }
    public Corner startCorner { get { return m_StartCorner; } set { SetProperty(ref m_StartCorner, value); } }
    public Axis startAxis { get { return m_StartAxis; } set { SetProperty(ref m_StartAxis, value); } }

    protected AutoResizeGridLayoutGroup() { }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        columns = m_Columns;
        rows = m_Rows;
    }
#endif

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        // Compute cell width to exactly fill 'columns' across the container
        float totalSpacingX = m_Spacing.x * (m_Columns - 1);
        float availableW = rectTransform.rect.width - padding.horizontal - totalSpacingX;
        float cellW = availableW / m_Columns;

        // Store it in the inherited cellSize field
        m_CellSize.x = cellW;

        // Tell the layout system our min/preferred width
        float requiredW = padding.horizontal + m_Columns * cellW + totalSpacingX;
        SetLayoutInputForAxis(requiredW, requiredW, -1, 0);
    }

    public override void CalculateLayoutInputVertical()
    {
        // Compute cell height to exactly fill 'rows' down the container
        float totalSpacingY = m_Spacing.y * (m_Rows - 1);
        float availableH = rectTransform.rect.height - padding.vertical - totalSpacingY;
        float cellH = availableH / m_Rows;

        m_CellSize.y = cellH;

        float requiredH = padding.vertical + m_Rows * cellH + totalSpacingY;
        SetLayoutInputForAxis(requiredH, requiredH, -1, 1);
    }

    public override void SetLayoutHorizontal() => SetCellsAlongAxis(0);
    public override void SetLayoutVertical() => SetCellsAlongAxis(1);

    private void SetCellsAlongAxis(int axis)
    {
        if (axis == 0)
        {
            // Size all children
            for (int i = 0; i < rectChildren.Count; i++)
            {
                var child = rectChildren[i];
                m_Tracker.Add(this, child,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.SizeDelta);

                // Anchor at top-left (Vector2.up) to make positioning easier
                child.anchorMin = Vector2.up;
                child.anchorMax = Vector2.up;
                child.sizeDelta = m_CellSize;
            }
            return;
        }

        // Position children now that we know cellSize
        int cellCountX = m_Columns;
        int cellCountY = m_Rows;

        int cornerX = (int)m_StartCorner % 2;
        int cornerY = (int)m_StartCorner / 2;

        Vector2 requiredSpace = new Vector2(
            cellCountX * m_CellSize.x + (cellCountX - 1) * m_Spacing.x,
            cellCountY * m_CellSize.y + (cellCountY - 1) * m_Spacing.y);

        Vector2 startOffset = new Vector2(
            GetStartOffset(0, requiredSpace.x),
            GetStartOffset(1, requiredSpace.y));

        for (int i = 0; i < rectChildren.Count; i++)
        {
            int posX, posY;
            if (m_StartAxis == Axis.Horizontal)
            {
                posX = i % cellCountX;
                posY = i / cellCountX;
            }
            else
            {
                posX = i / cellCountY;
                posY = i % cellCountY;
            }

            if (cornerX == 1) posX = cellCountX - 1 - posX;
            if (cornerY == 1) posY = cellCountY - 1 - posY;

            float x = startOffset.x + (m_CellSize.x + m_Spacing.x) * posX;
            float y = startOffset.y + (m_CellSize.y + m_Spacing.y) * posY;

            SetChildAlongAxis(rectChildren[i], 0, x, m_CellSize.x);
            SetChildAlongAxis(rectChildren[i], 1, y, m_CellSize.y);
        }
    }
}
