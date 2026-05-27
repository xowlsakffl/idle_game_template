using System;
using UnityEngine.UI;
using UnityEngine;

namespace IdleGame.UI.Common
{
    public static class HudUiFactory
    {
        private static Sprite roundedPanelSprite;
        private static Sprite roundedButtonSprite;
        private static Sprite roundedPillSprite;

        public static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            ApplyPanelVisualStyle(panel, image, name, color);
            return panel;
        }

        public static Button CreateButton(string label, Transform parent, int fontSize, Color color)
        {
            GameObject buttonObject = CreatePanel(label + "Button", parent, color);
            Button button = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = GetRoundedButtonSprite();
                buttonImage.type = Image.Type.Sliced;
            }

            AddInsetHighlight(buttonObject.transform, 0.18f);
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
            ApplyTextVisualStyle(textObject, fontSize);

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, fontSize + 22f);
            return text;
        }

        public static void SetButtonColor(Button button, Color color)
        {
            if (button == null)
            {
                return;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.12f;
            colors.pressedColor = color * 0.84f;
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
            button.colors = colors;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
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

        private static Sprite GetRoundedPanelSprite()
        {
            if (roundedPanelSprite == null)
            {
                roundedPanelSprite = CreateRoundedRectSprite(64, 14, 16);
            }

            return roundedPanelSprite;
        }

        private static Sprite GetRoundedButtonSprite()
        {
            if (roundedButtonSprite == null)
            {
                roundedButtonSprite = CreateRoundedRectSprite(64, 20, 22);
            }

            return roundedButtonSprite;
        }

        private static Sprite GetRoundedPillSprite()
        {
            if (roundedPillSprite == null)
            {
                roundedPillSprite = CreateRoundedRectSprite(64, 28, 28);
            }

            return roundedPillSprite;
        }

        private static Sprite CreateRoundedRectSprite(int size, int radius, int border)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, radius, size - radius);
                    float cy = Mathf.Clamp(py, radius, size - radius);
                    float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                    float alpha = Mathf.Clamp01(radius + 0.5f - distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static void ApplyPanelVisualStyle(GameObject panel, Image image, string name, Color color)
        {
            if (image == null || color.a <= 0.01f)
            {
                return;
            }

            if (!IsFlatUiPanel(name))
            {
                image.sprite = IsPillUiPanel(name) ? GetRoundedPillSprite() : GetRoundedPanelSprite();
                image.type = Image.Type.Sliced;
            }

            if (ShouldDecorateUiPanel(name))
            {
                Shadow shadow = panel.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.38f);
                shadow.effectDistance = new Vector2(0f, -4f);
                shadow.useGraphicAlpha = true;

                Outline outline = panel.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.58f);
                outline.effectDistance = new Vector2(2f, -2f);
                outline.useGraphicAlpha = true;
            }
        }

        private static bool IsFlatUiPanel(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name == "Root"
                || name.Contains("Viewport", StringComparison.Ordinal)
                || name.Contains("World", StringComparison.Ordinal)
                || name.Contains("ConnectorLine", StringComparison.Ordinal)
                || name.Contains("Projectile", StringComparison.Ordinal)
                || name.Contains("DamagePopup", StringComparison.Ordinal);
        }

        private static bool IsPillUiPanel(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.Contains("Pill", StringComparison.Ordinal)
                || name.Contains("Badge", StringComparison.Ordinal)
                || name.Contains("Bar", StringComparison.Ordinal)
                || name.Contains("Fill", StringComparison.Ordinal)
                || name.Contains("Dot", StringComparison.Ordinal);
        }

        private static bool ShouldDecorateUiPanel(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            return !name.Contains("Fill", StringComparison.Ordinal)
                && !name.Contains("Viewport", StringComparison.Ordinal)
                && !name.Contains("Overlay", StringComparison.Ordinal)
                && !name.Contains("Prompt", StringComparison.Ordinal)
                && !name.Contains("Popup", StringComparison.Ordinal)
                && !name.Contains("World", StringComparison.Ordinal)
                && !name.Contains("Root", StringComparison.Ordinal)
                && !name.Contains("ConnectorLine", StringComparison.Ordinal)
                && !name.Contains("DamagePopup", StringComparison.Ordinal);
        }

        private static void AddInsetHighlight(Transform parent, float alpha)
        {
            GameObject highlight = new GameObject("InsetHighlight", typeof(RectTransform), typeof(Image));
            highlight.transform.SetParent(parent, false);
            Image image = highlight.GetComponent<Image>();
            image.sprite = GetRoundedButtonSprite();
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 1f, 1f, alpha);
            image.raycastTarget = false;

            RectTransform rect = highlight.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.52f);
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(5f, 4f);
            rect.offsetMax = new Vector2(-5f, -5f);
        }

        private static void ApplyTextVisualStyle(GameObject textObject, int fontSize)
        {
            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, fontSize >= 24 ? 0.86f : 0.70f);
            outline.effectDistance = fontSize >= 28 ? new Vector2(2.6f, -2.6f) : new Vector2(1.7f, -1.7f);
            outline.useGraphicAlpha = false;

            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
            shadow.effectDistance = new Vector2(0f, -2f);
            shadow.useGraphicAlpha = false;
        }
    }
}
