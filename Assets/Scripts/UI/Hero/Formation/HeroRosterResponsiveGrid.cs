using UnityEngine.UI;
using UnityEngine;

namespace IdleGame.UI.Hero.Formation
{
    public sealed class HeroRosterResponsiveGrid : MonoBehaviour
    {
        private const float MinCellWidth = 80f;
        private const float TopPadding = 4f;
        private const float SidePadding = 8f;

        private RectTransform viewport;
        private RectTransform content;
        private GridLayoutGroup grid;
        private int columns;
        private float aspectRatio;
        private Vector2 spacing;
        private int lastChildCount = -1;
        private float lastViewportWidth = -1f;

        public void Initialize(
            RectTransform viewportRect,
            RectTransform contentRect,
            GridLayoutGroup gridLayout,
            int columnCount,
            float cellAspectRatio,
            Vector2 cellSpacing)
        {
            viewport = viewportRect;
            content = contentRect;
            grid = gridLayout;
            columns = Mathf.Max(1, columnCount);
            aspectRatio = Mathf.Max(0.1f, cellAspectRatio);
            spacing = cellSpacing;
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        private void Apply()
        {
            if (viewport == null || content == null || grid == null)
            {
                return;
            }

            int childCount = CountActiveChildren(content);
            float viewportWidth = viewport.rect.width;
            if (viewportWidth <= 1f)
            {
                viewportWidth = content.rect.width;
            }

            if (Mathf.Abs(viewportWidth - lastViewportWidth) < 0.5f && childCount == lastChildCount)
            {
                return;
            }

            lastViewportWidth = viewportWidth;
            lastChildCount = childCount;

            float availableWidth = Mathf.Max(columns * MinCellWidth, viewportWidth - SidePadding * 2f);
            float cellWidth = Mathf.Floor((availableWidth - spacing.x * (columns - 1)) / columns);
            cellWidth = Mathf.Max(MinCellWidth, cellWidth);
            float cellHeight = Mathf.Round(cellWidth * aspectRatio);

            grid.cellSize = new Vector2(cellWidth, cellHeight);
            grid.spacing = spacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;

            int rows = Mathf.CeilToInt(childCount / (float)columns);
            float contentWidth = cellWidth * columns + spacing.x * Mathf.Max(0, columns - 1);
            float contentHeight = Mathf.Max(cellHeight, rows * cellHeight + spacing.y * Mathf.Max(0, rows - 1));

            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = new Vector2(SidePadding, -TopPadding);
            content.sizeDelta = new Vector2(contentWidth, contentHeight);
        }

        private static int CountActiveChildren(Transform root)
        {
            int count = 0;
            for (int i = 0; i < root.childCount; i++)
            {
                if (root.GetChild(i).gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
