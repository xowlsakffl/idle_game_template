using System;
using UnityEngine;
using UnityEngine.UI;

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

        public static GameObject CreateSpritePanel(string name, Transform parent, HudSpriteKind spriteKind, Color fallbackColor)
        {
            GameObject panel = CreatePanel(name, parent, fallbackColor);
            ApplySprite(panel.GetComponent<Image>(), spriteKind, fallbackColor);
            return panel;
        }

        public static Button CreateButton(string label, Transform parent, int fontSize, Color color)
        {
            GameObject buttonObject = CreatePanel(label + "Button", parent, color);
            Button button = buttonObject.AddComponent<Button>();

            Text text = CreateText(label + "Text", buttonObject.transform, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 6f);
            textRect.offsetMax = new Vector2(-10f, -6f);
            text.text = label;

            ApplyButtonSprite(button, HudSpriteKind.BlueMenuButton, color);
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
            HudFontProvider.Apply(text);
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = Color.white;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.03f, 0.04f, 0.06f, 0.92f);
            outline.effectDistance = new Vector2(2f, -2f);

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
            GameObject dot = CreatePanel("RedDot", parent, new Color(1f, 0.04f, 0.04f, 1f));
            Image image = dot.GetComponent<Image>();
            ApplySprite(image, HudSpriteKind.TinyRoundRedButton, Color.white);
            image.raycastTarget = false;

            RectTransform rect = dot.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = anchoredPosition;

            dot.SetActive(false);
            return dot;
        }

        public static Image CreateIcon(string name, Transform parent, HudSpriteKind spriteKind, Vector2 size)
        {
            GameObject iconObject = CreateSpritePanel(name, parent, spriteKind, Color.white);
            Image image = iconObject.GetComponent<Image>();
            image.raycastTarget = false;

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            return image;
        }

        public static Image CreateBarFill(string name, Transform parent, HudSpriteKind spriteKind, Color fallbackColor)
        {
            GameObject fillObject = CreatePanel(name, parent, fallbackColor);
            Image fill = fillObject.GetComponent<Image>();
            ApplySprite(fill, spriteKind, fallbackColor);
            RectTransform rect = fill.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            fill.raycastTarget = false;
            return fill;
        }

        public static void ApplySprite(Image image, HudSpriteKind spriteKind, Color color)
        {
            if (image == null)
            {
                return;
            }

            if (IsFlatHudSurface(spriteKind))
            {
                ApplyFlatSurface(image, spriteKind, color);
                return;
            }

            if (HudSpriteCatalog.TryGetNinePatchDefinition(spriteKind, out _))
            {
                HudNinePatchPanel patch = image.GetComponent<HudNinePatchPanel>();
                if (patch == null)
                {
                    patch = image.gameObject.AddComponent<HudNinePatchPanel>();
                }

                patch.Configure(spriteKind, color, image.GetComponent<Selectable>() != null);
                return;
            }

            HudNinePatchPanel ninePatch = image.GetComponent<HudNinePatchPanel>();
            if (ninePatch != null)
            {
                ninePatch.HidePatch();
                image.enabled = true;
            }

            Sprite sprite = HudSpriteCatalog.Get(spriteKind);
            if (sprite == null)
            {
                image.sprite = null;
                image.color = color;
                return;
            }

            image.sprite = sprite;
            image.type = HasBorder(sprite) ? Image.Type.Sliced : Image.Type.Simple;
            image.preserveAspect = false;
            image.color = color;
        }

        public static void ApplyNinePatchPanel(GameObject target, HudSpriteKind spriteKind, Color color)
        {
            if (target == null)
            {
                return;
            }

            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                ApplySprite(image, spriteKind, color);
            }
        }

        public static void ApplyNinePatchButton(Button button, HudSpriteKind spriteKind, Color color)
        {
            if (button == null)
            {
                return;
            }

            ApplyButtonSprite(button, spriteKind, color);
        }

        public static void ApplyUntintedButtonSprite(Button button, HudSpriteKind spriteKind)
        {
            ApplyButtonSprite(button, spriteKind, Color.white);
        }

        public static void ApplyButtonSprite(Button button, HudSpriteKind spriteKind, Color color)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            ApplySprite(image, spriteKind, color);
            ColorBlock colors = button.colors;
            if (IsFlatHudSurface(spriteKind))
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
                colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
                colors.selectedColor = new Color(1.04f, 1.04f, 1.04f, 1f);
                colors.disabledColor = new Color(0.52f, 0.52f, 0.52f, 0.55f);
                colors.colorMultiplier = 1f;
                button.transition = Selectable.Transition.ColorTint;
            }
            else
            {
                colors.normalColor = color;
                colors.highlightedColor = color;
                colors.pressedColor = color;
                colors.selectedColor = color;
                colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
                button.transition = Selectable.Transition.None;
            }

            button.colors = colors;

            HudNinePatchPanel patch = image.GetComponent<HudNinePatchPanel>();
            if (patch == null || !patch.IsConfigured)
            {
                image.color = IsFlatHudSurface(spriteKind) ? ResolveFlatSurfaceColor(spriteKind, color) : color;
            }
        }

        public static void ApplySpriteButtonState(Button button, HudSpriteKind normalKind, HudSpriteKind pressedKind, bool pressed)
        {
            if (button == null)
            {
                return;
            }

            ApplyUntintedSpriteSwapColors(button);

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            HudSpriteKind activeKind = pressed ? pressedKind : normalKind;
            if (IsFlatHudSurface(activeKind))
            {
                ApplyFlatSurface(image, activeKind, Color.white);
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
                colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
                colors.selectedColor = new Color(1.04f, 1.04f, 1.04f, 1f);
                colors.disabledColor = new Color(0.52f, 0.52f, 0.52f, 0.55f);
                colors.colorMultiplier = 1f;
                button.colors = colors;
                button.transition = Selectable.Transition.ColorTint;
                return;
            }

            if (HudSpriteCatalog.TryGetNinePatchDefinition(activeKind, out _))
            {
                HudNinePatchPanel patch = button.GetComponent<HudNinePatchPanel>();
                if (patch == null)
                {
                    patch = button.gameObject.AddComponent<HudNinePatchPanel>();
                }

                patch.Configure(activeKind, Color.white, true);
                button.transition = Selectable.Transition.None;
                return;
            }

            HudNinePatchPanel ninePatch = button.GetComponent<HudNinePatchPanel>();
            if (ninePatch != null)
            {
                ninePatch.HidePatch();
                image.enabled = true;
            }

            Sprite normalSprite = HudSpriteCatalog.Get(normalKind);
            Sprite pressedSprite = HudSpriteCatalog.Get(pressedKind);
            Sprite activeSprite = pressed && pressedSprite != null ? pressedSprite : normalSprite;
            if (activeSprite == null)
            {
                image.sprite = null;
                image.type = Image.Type.Simple;
                image.color = pressed
                    ? new Color(0.28f, 0.42f, 0.55f, 1f)
                    : new Color(0.31f, 0.63f, 0.69f, 1f);
                button.transition = Selectable.Transition.None;
                return;
            }

            image.sprite = activeSprite;
            image.type = HasBorder(activeSprite) ? Image.Type.Sliced : Image.Type.Simple;
            image.preserveAspect = false;
            image.color = Color.white;

            SpriteState spriteState = button.spriteState;
            spriteState.highlightedSprite = activeSprite;
            spriteState.pressedSprite = pressedSprite != null ? pressedSprite : activeSprite;
            spriteState.selectedSprite = activeSprite;
            button.spriteState = spriteState;

            button.transition = Selectable.Transition.SpriteSwap;
        }

        public static void MoveButtonText(Button button, Vector2 delta)
        {
            if (button == null)
            {
                return;
            }

            Text text = button.GetComponentInChildren<Text>(true);
            if (text == null)
            {
                return;
            }

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.offsetMin += delta;
            rect.offsetMax += delta;
        }

        private static void ApplyUntintedSpriteSwapColors(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
        }

        private static bool HasBorder(Sprite sprite)
        {
            Vector4 border = sprite.border;
            return border.x > 0f || border.y > 0f || border.z > 0f || border.w > 0f;
        }

        private static bool IsFlatHudSurface(HudSpriteKind spriteKind)
        {
            switch (spriteKind)
            {
                case HudSpriteKind.BigBlueButton:
                case HudSpriteKind.BlueMenuButton:
                case HudSpriteKind.BlueMenuButtonPressed:
                case HudSpriteKind.BigRedButton:
                case HudSpriteKind.BigRedButtonPressed:
                case HudSpriteKind.SmallBlueSquareButton:
                case HudSpriteKind.SmallBlueSquareButtonPressed:
                case HudSpriteKind.SmallRedSquareButton:
                case HudSpriteKind.SmallRedSquareButtonPressed:
                case HudSpriteKind.DisabledPanel:
                case HudSpriteKind.BluePanel:
                case HudSpriteKind.CarvedPanel:
                case HudSpriteKind.ParchmentPanel:
                case HudSpriteKind.SpecialPaperPanel:
                case HudSpriteKind.WoodPanel:
                case HudSpriteKind.Banner:
                case HudSpriteKind.WoodTable:
                case HudSpriteKind.BlueRibbon:
                case HudSpriteKind.RedRibbon:
                case HudSpriteKind.YellowRibbon:
                    return true;
                default:
                    return false;
            }
        }

        private static void ApplyFlatSurface(Image image, HudSpriteKind spriteKind, Color color)
        {
            HudNinePatchPanel ninePatch = image.GetComponent<HudNinePatchPanel>();
            if (ninePatch != null)
            {
                ninePatch.HidePatch();
            }

            image.enabled = true;
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = ResolveFlatSurfaceColor(spriteKind, color);
            image.raycastTarget = true;

            Outline outline = image.GetComponent<Outline>();
            if (outline == null)
            {
                outline = image.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = IsButtonSurface(spriteKind)
                ? new Color(0.02f, 0.03f, 0.04f, 0.78f)
                : new Color(0.02f, 0.025f, 0.03f, 0.64f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static bool IsButtonSurface(HudSpriteKind spriteKind)
        {
            switch (spriteKind)
            {
                case HudSpriteKind.BigBlueButton:
                case HudSpriteKind.BlueMenuButton:
                case HudSpriteKind.BlueMenuButtonPressed:
                case HudSpriteKind.BigRedButton:
                case HudSpriteKind.BigRedButtonPressed:
                case HudSpriteKind.SmallBlueSquareButton:
                case HudSpriteKind.SmallBlueSquareButtonPressed:
                case HudSpriteKind.SmallRedSquareButton:
                case HudSpriteKind.SmallRedSquareButtonPressed:
                case HudSpriteKind.DisabledPanel:
                    return true;
                default:
                    return false;
            }
        }

        private static Color ResolveFlatSurfaceColor(HudSpriteKind spriteKind, Color requested)
        {
            Color fallback = GetDefaultFlatSurfaceColor(spriteKind);
            bool useFallback = requested.a <= 0f
                || (Mathf.Abs(requested.r - 1f) < 0.01f
                    && Mathf.Abs(requested.g - 1f) < 0.01f
                    && Mathf.Abs(requested.b - 1f) < 0.01f);
            Color result = useFallback ? fallback : Color.Lerp(fallback, requested, IsButtonSurface(spriteKind) ? 0.58f : 0.36f);
            result.a = requested.a > 0f ? requested.a : fallback.a;
            return result;
        }

        private static Color GetDefaultFlatSurfaceColor(HudSpriteKind spriteKind)
        {
            switch (spriteKind)
            {
                case HudSpriteKind.BlueMenuButtonPressed:
                case HudSpriteKind.SmallBlueSquareButtonPressed:
                    return new Color(0.10f, 0.30f, 0.40f, 1f);
                case HudSpriteKind.BigBlueButton:
                case HudSpriteKind.BlueMenuButton:
                case HudSpriteKind.SmallBlueSquareButton:
                    return new Color(0.15f, 0.43f, 0.52f, 1f);
                case HudSpriteKind.BigRedButton:
                case HudSpriteKind.BigRedButtonPressed:
                case HudSpriteKind.SmallRedSquareButton:
                case HudSpriteKind.SmallRedSquareButtonPressed:
                    return new Color(0.50f, 0.16f, 0.14f, 1f);
                case HudSpriteKind.DisabledPanel:
                    return new Color(0.20f, 0.21f, 0.24f, 1f);
                case HudSpriteKind.BlueRibbon:
                case HudSpriteKind.Banner:
                    return new Color(0.12f, 0.28f, 0.38f, 0.96f);
                case HudSpriteKind.RedRibbon:
                    return new Color(0.42f, 0.12f, 0.12f, 0.96f);
                case HudSpriteKind.YellowRibbon:
                    return new Color(0.50f, 0.36f, 0.12f, 0.96f);
                case HudSpriteKind.ParchmentPanel:
                    return new Color(0.16f, 0.17f, 0.18f, 0.94f);
                case HudSpriteKind.CarvedPanel:
                case HudSpriteKind.WoodPanel:
                case HudSpriteKind.WoodTable:
                case HudSpriteKind.SpecialPaperPanel:
                    return new Color(0.10f, 0.13f, 0.17f, 0.94f);
                case HudSpriteKind.BluePanel:
                default:
                    return new Color(0.09f, 0.13f, 0.19f, 0.94f);
            }
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

            if (HudPrefabStyleLock.BlocksColors(button))
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
                HudNinePatchPanel patch = image.GetComponent<HudNinePatchPanel>();
                if (patch != null && patch.IsConfigured)
                {
                    patch.SetTint(color);
                    return;
                }

                if (image.sprite == null)
                {
                    ApplySprite(image, HudSpriteKind.BlueMenuButton, color);
                    patch = image.GetComponent<HudNinePatchPanel>();
                    if (patch != null && patch.IsConfigured)
                    {
                        return;
                    }
                }

                image.color = color;
            }
        }

        public static void ApplyButtonStyle(Button button, HudButtonVisualStyle style)
        {
            if (button == null)
            {
                return;
            }

            if (!HudPrefabStyleLock.BlocksColors(button))
            {
                SetButtonColor(button, style.Color);
            }

            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null && !HudPrefabStyleLock.BlocksTextStyle(label))
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
