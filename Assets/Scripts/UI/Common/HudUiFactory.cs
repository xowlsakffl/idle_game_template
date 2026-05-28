using System;
using UnityEngine.UI;
using UnityEngine;

namespace IdleGame.UI.Common
{
    public static class HudUiFactory
    {
        public static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        public static Button CreateButton(string label, Transform parent, int fontSize, Color color)
        {
            GameObject buttonObject = CreatePanel(label + "Button", parent, color);
            Button button = buttonObject.AddComponent<Button>();
            SetButtonColor(button, color);

            Text text = CreateText(label + "Text", buttonObject.transform, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 6f);
            textRect.offsetMax = new Vector2(-10f, -6f);
            text.text = label;

            return button;
        }

        public static Button CreateButton(string label, Transform parent, HudButtonVisualStyle style)
        {
            return CreateButton(label, parent, style.FontSize, style.Color);
        }

        public static Text CreateText(string name, Transform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = Color.white;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, fontSize + 22f);
            return text;
        }

        public static Button CreateCornerActionButton(
            string label,
            Transform parent,
            HudButtonVisualStyle style,
            float size = 30f,
            float inset = 3f)
        {
            return CreateCornerActionButton(label, parent, style.FontSize, style.Color, size, inset);
        }

        public static Button CreateCornerActionButton(
            string label,
            Transform parent,
            int fontSize,
            Color color,
            float size = 30f,
            float inset = 3f)
        {
            Button button = CreateButton(label, parent, fontSize, color);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(-inset, -inset);
            return button;
        }

        public static GameObject CreateNotificationDot(Transform parent, float size, Vector2 anchoredPosition)
        {
            Text dot = CreateText("RedDot", parent, Mathf.RoundToInt(size), FontStyle.Bold, TextAnchor.MiddleCenter);
            dot.text = "●";
            dot.color = new Color(1f, 0.04f, 0.04f, 1f);
            dot.raycastTarget = false;

            RectTransform rect = dot.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = anchoredPosition;

            dot.gameObject.SetActive(false);
            return dot.gameObject;
        }

        public static void ConfigureBestFitText(Text text, int minSize, int maxSize, float lineSpacing = 1f)
        {
            if (text == null)
            {
                return;
            }

            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = maxSize;
            text.lineSpacing = lineSpacing;
        }

        public static void ConfigureHoldRepeat(Button button, Action action, Func<bool> canRepeat)
        {
            if (button == null)
            {
                return;
            }

            HoldRepeatButton repeatButton = button.GetComponent<HoldRepeatButton>();
            if (repeatButton == null)
            {
                repeatButton = button.gameObject.AddComponent<HoldRepeatButton>();
            }

            repeatButton.Configure(action, canRepeat);
        }

        public static void SetButtonColor(Button button, Color color)
        {
            if (button == null)
            {
                return;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color;
            colors.pressedColor = color;
            colors.selectedColor = color;
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
            button.colors = colors;
            button.transition = Selectable.Transition.None;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        public static void ApplyButtonStyle(Button button, HudButtonVisualStyle style)
        {
            if (button == null)
            {
                return;
            }

            SetButtonColor(button, style.Color);

            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.fontSize = style.FontSize;
            }
        }

        public static void SetButtonText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        public static void StretchToParent(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static LayoutElement AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
        {
            LayoutElement element = target.AddComponent<LayoutElement>();
            if (preferredWidth > 0f)
            {
                element.preferredWidth = preferredWidth;
            }

            if (preferredHeight > 0f)
            {
                element.preferredHeight = preferredHeight;
            }

            return element;
        }
    }
}
