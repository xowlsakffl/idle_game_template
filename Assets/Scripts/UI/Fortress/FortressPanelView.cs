using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Fortress
{
    public sealed class FortressPanelViewRefs
    {
        public Text SummaryText;
        public Text StatsText;
        public Text ExpText;
        public Text NoticeText;
        public Image ExpFill;
        public Button LevelUpButton;
    }

    public sealed class FortressPanelViewBuildArgs
    {
        public Transform Parent;
        public Action OnLevelUp;
        public Func<bool> CanLevelUp;
    }

    public static class FortressPanelView
    {
        public static void ApplyState(FortressPanelViewRefs refs, FortressPanelViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            if (refs.SummaryText != null)
            {
                refs.SummaryText.text = state.SummaryText ?? string.Empty;
            }

            if (refs.ExpFill != null)
            {
                refs.ExpFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(state.ExpFillRatio), 1f);
            }

            if (refs.ExpText != null)
            {
                refs.ExpText.text = state.ExpText ?? string.Empty;
            }

            if (refs.StatsText != null)
            {
                refs.StatsText.text = state.StatsText ?? string.Empty;
            }

            if (refs.LevelUpButton != null)
            {
                refs.LevelUpButton.interactable = state.LevelUpInteractable;
                HudUiFactory.SetButtonColor(refs.LevelUpButton, state.LevelUpColor);
                SetButtonText(refs.LevelUpButton, state.LevelUpText);
            }
        }

        public static FortressPanelViewRefs Build(FortressPanelViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new FortressPanelViewRefs();
            }

            VerticalLayoutGroup layout = args.Parent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = HudUiFactory.CreateText("FortressTitle", args.Parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.text = "요새";
            HudUiFactory.AddLayoutElement(title.gameObject, -1, 46);

            GameObject card = HudUiFactory.CreatePanel("FortressMainCard", args.Parent, new Color(0.19f, 0.25f, 0.34f, 1f));
            HudUiFactory.AddLayoutElement(card, -1, 210);

            VerticalLayoutGroup cardLayout = card.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(18, 18, 16, 16);
            cardLayout.spacing = 8;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            FortressPanelViewRefs refs = new FortressPanelViewRefs();
            refs.SummaryText = HudUiFactory.CreateText("FortressSummary", card.transform, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, 52);

            GameObject expBar = HudUiFactory.CreatePanel("FortressExpBar", card.transform, new Color(0.06f, 0.08f, 0.12f, 1f));
            HudUiFactory.AddLayoutElement(expBar, -1, 38);
            refs.ExpFill = HudUiFactory.CreatePanel("FortressExpFill", expBar.transform, new Color(0.38f, 0.78f, 1f, 1f)).GetComponent<Image>();
            RectTransform expFillRect = refs.ExpFill.GetComponent<RectTransform>();
            expFillRect.anchorMin = Vector2.zero;
            expFillRect.anchorMax = new Vector2(0f, 1f);
            expFillRect.offsetMin = Vector2.zero;
            expFillRect.offsetMax = Vector2.zero;

            refs.ExpText = HudUiFactory.CreateText("FortressExpText", expBar.transform, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(refs.ExpText.gameObject);

            refs.StatsText = HudUiFactory.CreateText("FortressStats", card.transform, 22, FontStyle.Bold, TextAnchor.UpperLeft);
            HudUiFactory.AddLayoutElement(refs.StatsText.gameObject, -1, 74);

            refs.LevelUpButton = HudUiFactory.CreateButton("레벨업", args.Parent, 30, new Color(0.58f, 0.84f, 0.20f, 1f));
            HudUiFactory.AddLayoutElement(refs.LevelUpButton.gameObject, -1, 74);
            HudUiFactory.ConfigureHoldRepeat(refs.LevelUpButton, args.OnLevelUp, args.CanLevelUp);

            refs.NoticeText = HudUiFactory.CreateText("FortressNotice", args.Parent, 24, FontStyle.Bold, TextAnchor.UpperLeft);
            refs.NoticeText.color = new Color(0.95f, 0.78f, 0.42f, 1f);
            refs.NoticeText.text = "전투 중 요새는 중앙에서 자동 공격합니다. 원거리/지원 영웅은 요새 안쪽에서 공격하고, 근접/방어 영웅만 밖에서 맞붙습니다.";
            HudUiFactory.AddLayoutElement(refs.NoticeText.gameObject, -1, 100);
            return refs;
        }

        private static void SetButtonText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }
    }
}
