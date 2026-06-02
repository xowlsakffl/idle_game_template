using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail
{
    public static partial class HeroDetailView
    {
        private static ScrollRect ConfigureVerticalScroll(GameObject scrollObject, float sensitivity)
        {
            ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = sensitivity;
            return scrollRect;
        }

        private static RectTransform CreateGridViewport(
            Transform parent,
            string name,
            Vector2 contentSize,
            Vector2 contentPosition,
            Vector2 cellSize,
            Vector2 spacing,
            int columns,
            out Transform gridTransform)
        {
            GameObject viewport = HudUiFactory.CreatePanel(name + "Viewport", parent, new Color(0f, 0f, 0f, 0f));
            HudUiFactory.StretchToParent(viewport);
            viewport.AddComponent<RectMask2D>();
            Image viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.raycastTarget = true;
            }

            GameObject gridObject = new GameObject(name + "Grid", typeof(RectTransform));
            gridObject.transform.SetParent(viewport.transform, false);
            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0f, 1f);
            gridRect.anchorMax = new Vector2(0f, 1f);
            gridRect.pivot = new Vector2(0f, 1f);
            gridRect.sizeDelta = contentSize;
            gridRect.anchoredPosition = contentPosition;
            GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = spacing;
            grid.padding = new RectOffset(0, 0, 0, Mathf.RoundToInt(spacing.y));
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            ContentSizeFitter fitter = gridObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            parent.GetComponent<ScrollRect>().viewport = viewport.GetComponent<RectTransform>();
            gridTransform = gridObject.transform;
            return gridRect;
        }

        private static GameObject CreateActionRow(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, float spacing)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            SetAnchored(row, anchorMin, anchorMax, new Vector2(0.5f, 0f), size, position);
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return row;
        }

        private static Text CreateTextAnchored(
            string name,
            Transform parent,
            int fontSize,
            TextAnchor anchor,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 position)
        {
            Text text = HudUiFactory.CreateText(name, parent, fontSize, FontStyle.Bold, anchor);
            SetAnchored(text.gameObject, anchorMin, anchorMax, pivot, size, position);
            return text;
        }

        private static void SetStretchOffsets(GameObject target, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetOffsets(GameObject target, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetAnchored(GameObject target, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
