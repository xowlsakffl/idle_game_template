using UnityEngine;
using UnityEngine.UI;

namespace IdleGame.UI.Hero.Formation
{
    public sealed class HeroFormationSlotGridSizer : MonoBehaviour
    {
        private const float MinCellWidth = 96f;
        private RectTransform rectTransform;
        private GridLayoutGroup grid;
        private int columns;
        private int rows;
        private float aspectRatio;
        private Vector2 spacing;
        private float lastWidth = -1f;
        private float lastHeight = -1f;

        public void Initialize(GridLayoutGroup gridLayout, int columnCount, int rowCount, float cellAspectRatio, Vector2 cellSpacing)
        {
            rectTransform = GetComponent<RectTransform>();
            grid = gridLayout;
            columns = Mathf.Max(1, columnCount);
            rows = Mathf.Max(1, rowCount);
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
            if (rectTransform == null || grid == null)
            {
                return;
            }

            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            if (width <= 1f || height <= 1f)
            {
                return;
            }

            if (Mathf.Abs(width - lastWidth) < 0.5f && Mathf.Abs(height - lastHeight) < 0.5f)
            {
                return;
            }

            lastWidth = width;
            lastHeight = height;

            float widthBound = Mathf.Floor((width - spacing.x * Mathf.Max(0, columns - 1)) / columns);
            float heightBound = Mathf.Floor((height - spacing.y * Mathf.Max(0, rows - 1)) / rows);
            float widthFromHeight = Mathf.Floor(heightBound / aspectRatio);
            float cellWidth = Mathf.Max(MinCellWidth, Mathf.Min(widthBound, widthFromHeight));
            float cellHeight = Mathf.Floor(cellWidth * aspectRatio);

            grid.cellSize = new Vector2(cellWidth, cellHeight);
            grid.spacing = spacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
        }
    }
}
