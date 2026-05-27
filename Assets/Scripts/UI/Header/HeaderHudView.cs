using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Header
{
    public sealed class HeaderHudViewRefs
    {
        public Text StageText;
        public Text ModeText;
        public Text ResourceText;
        public Text RubyResourceText;
        public Text AccountLevelText;
        public Image AccountExpFill;
    }

    public sealed class HeaderHudViewBuildArgs
    {
        public Transform Parent;
        public bool ShowDebugGrantButton;
        public Action OnDebugGrant;
    }

    public static class HeaderHudView
    {
        public static HeaderHudViewRefs Build(HeaderHudViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new HeaderHudViewRefs();
            }

            HeaderHudViewRefs refs = new HeaderHudViewRefs();
            GameObject panel = HudUiFactory.CreatePanel("Header", args.Parent, new Color(0.06f, 0.19f, 0.33f, 0.98f));
            HudUiFactory.AddLayoutElement(panel, -1, 160);

            GameObject avatar = HudUiFactory.CreatePanel("PlayerAvatar", panel.transform, new Color(0.10f, 0.48f, 0.72f, 1f));
            RectTransform avatarRect = avatar.GetComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0f, 0.5f);
            avatarRect.anchorMax = new Vector2(0f, 0.5f);
            avatarRect.pivot = new Vector2(0f, 0.5f);
            avatarRect.sizeDelta = new Vector2(104f, 104f);
            avatarRect.anchoredPosition = new Vector2(22f, 2f);

            Text avatarText = HudUiFactory.CreateText("AvatarText", avatar.transform, 38, FontStyle.Bold, TextAnchor.MiddleCenter);
            avatarText.text = "G";
            HudUiFactory.StretchToParent(avatarText.gameObject);

            refs.StageText = HudUiFactory.CreateText("Stage", panel.transform, 25, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform stageRect = refs.StageText.GetComponent<RectTransform>();
            stageRect.anchorMin = new Vector2(0f, 0.5f);
            stageRect.anchorMax = new Vector2(0f, 0.5f);
            stageRect.pivot = new Vector2(0f, 0.5f);
            stageRect.sizeDelta = new Vector2(410f, 34f);
            stageRect.anchoredPosition = new Vector2(146f, 42f);

            CreateAnchoredIcon("CombatPowerIcon", panel.transform, HudSpriteFactory.GetPowerIconSprite(), new Vector2(146f, 8f), new Vector2(40f, 40f));

            refs.ModeText = HudUiFactory.CreateText("Mode", panel.transform, 31, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform modeRect = refs.ModeText.GetComponent<RectTransform>();
            modeRect.anchorMin = new Vector2(0f, 0.5f);
            modeRect.anchorMax = new Vector2(0f, 0.5f);
            modeRect.pivot = new Vector2(0f, 0.5f);
            modeRect.sizeDelta = new Vector2(382f, 42f);
            modeRect.anchoredPosition = new Vector2(192f, 8f);

            GameObject accountExpBar = HudUiFactory.CreatePanel("AccountExpBar", panel.transform, new Color(0.03f, 0.08f, 0.15f, 1f));
            RectTransform accountBarRect = accountExpBar.GetComponent<RectTransform>();
            accountBarRect.anchorMin = new Vector2(0f, 0.5f);
            accountBarRect.anchorMax = new Vector2(0f, 0.5f);
            accountBarRect.pivot = new Vector2(0f, 0.5f);
            accountBarRect.sizeDelta = new Vector2(430f, 30f);
            accountBarRect.anchoredPosition = new Vector2(146f, -38f);

            refs.AccountExpFill = HudUiFactory.CreatePanel("AccountExpFill", accountExpBar.transform, new Color(0.10f, 0.79f, 0.96f, 1f)).GetComponent<Image>();
            RectTransform accountFillRect = refs.AccountExpFill.GetComponent<RectTransform>();
            accountFillRect.anchorMin = Vector2.zero;
            accountFillRect.anchorMax = new Vector2(0f, 1f);
            accountFillRect.offsetMin = Vector2.zero;
            accountFillRect.offsetMax = Vector2.zero;

            refs.AccountLevelText = HudUiFactory.CreateText("AccountLevelText", accountExpBar.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(refs.AccountLevelText.gameObject);

            GameObject resourcePill = HudUiFactory.CreatePanel("ResourcePill", panel.transform, new Color(0.03f, 0.09f, 0.16f, 0.96f));
            RectTransform resourceRect = resourcePill.GetComponent<RectTransform>();
            resourceRect.anchorMin = new Vector2(1f, 0.5f);
            resourceRect.anchorMax = new Vector2(1f, 0.5f);
            resourceRect.pivot = new Vector2(1f, 0.5f);
            resourceRect.sizeDelta = new Vector2(args.ShowDebugGrantButton ? 330f : 430f, 58f);
            resourceRect.anchoredPosition = new Vector2(args.ShowDebugGrantButton ? -194f : -112f, 34f);

            CreateHeaderResourceDisplay(
                resourcePill.transform,
                "GoldResource",
                HudSpriteFactory.GetCoinIconSprite(),
                new Vector2(18f, 0f),
                out refs.ResourceText);
            CreateHeaderResourceDisplay(
                resourcePill.transform,
                "RubyResource",
                HudSpriteFactory.GetGemIconSprite(),
                new Vector2(args.ShowDebugGrantButton ? 178f : 226f, 0f),
                out refs.RubyResourceText);

            if (args.ShowDebugGrantButton)
            {
                Button debugGrantButton = HudUiFactory.CreateButton("DBG", panel.transform, 24, new Color(0.26f, 0.18f, 0.12f, 1f));
                RectTransform debugRect = debugGrantButton.GetComponent<RectTransform>();
                debugRect.anchorMin = new Vector2(1f, 0.5f);
                debugRect.anchorMax = new Vector2(1f, 0.5f);
                debugRect.pivot = new Vector2(1f, 0.5f);
                debugRect.sizeDelta = new Vector2(76f, 58f);
                debugRect.anchoredPosition = new Vector2(-108f, 34f);
                debugGrantButton.onClick.AddListener(() => args.OnDebugGrant?.Invoke());
            }

            Button menuButton = HudUiFactory.CreateButton("≡", panel.transform, 36, new Color(0.12f, 0.16f, 0.22f, 1f));
            RectTransform menuRect = menuButton.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(1f, 0.5f);
            menuRect.anchorMax = new Vector2(1f, 0.5f);
            menuRect.pivot = new Vector2(1f, 0.5f);
            menuRect.sizeDelta = new Vector2(76f, 58f);
            menuRect.anchoredPosition = new Vector2(-22f, 34f);

            return refs;
        }

        private static Image CreateAnchoredIcon(string name, Transform parent, Sprite sprite, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject iconObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(parent, false);
            Image image = iconObject.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            return image;
        }

        private static void CreateHeaderResourceDisplay(
            Transform parent,
            string name,
            Sprite iconSprite,
            Vector2 iconPosition,
            out Text valueText)
        {
            CreateAnchoredIcon(name + "Icon", parent, iconSprite, iconPosition, new Vector2(38f, 38f));

            valueText = HudUiFactory.CreateText(name + "Text", parent, 25, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform textRect = valueText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(0f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.sizeDelta = new Vector2(132f, 46f);
            textRect.anchoredPosition = new Vector2(iconPosition.x + 46f, iconPosition.y);
        }
    }
}
