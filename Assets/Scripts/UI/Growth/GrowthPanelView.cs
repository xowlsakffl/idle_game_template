using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Growth
{
    public sealed class GrowthPanelViewRefs
    {
        public Text TotalCombatPowerText;
        public Text GrowthNoticeText;
    }

    public sealed class GrowthPanelViewBuildArgs
    {
        public Transform Parent;
        public IEnumerable<AbilityState> Abilities;
        public Action<int> OnSelectLevelStep;
        public Action<AbilityKind> OnLevelUpAbility;
        public Func<AbilityKind, bool> CanLevelUpAbility;
        public Dictionary<int, Button> GrowthStepButtons;
        public Dictionary<AbilityKind, Text> AbilityButtonTexts;
        public Dictionary<AbilityKind, Text> AbilityCostBadgeTexts;
        public Dictionary<AbilityKind, GameObject> AbilityNotificationDots;
    }

    public static class GrowthPanelView
    {
        public static GrowthPanelViewRefs Build(GrowthPanelViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new GrowthPanelViewRefs();
            }

            VerticalLayoutGroup layout = args.Parent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 20);
            layout.spacing = 7;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            GameObject titleBar = HudUiFactory.CreatePanel("GrowthTitleBar", args.Parent, Color.white);
            HudUiFactory.ApplySprite(titleBar.GetComponent<Image>(), HudSpriteKind.BlueRibbon, new Color(0.88f, 1f, 1f, 1f));
            HudUiFactory.AddLayoutElement(titleBar, -1, 44);

            Text title = HudUiFactory.CreateText("GrowthTitle", titleBar.transform, 30, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.text = "성장";
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(22f, 2f);
            titleRect.offsetMax = new Vector2(-22f, -2f);

            GrowthPanelViewRefs refs = new GrowthPanelViewRefs();
            GameObject powerBar = HudUiFactory.CreatePanel("GrowthPowerBar", args.Parent, Color.white);
            HudUiFactory.ApplySprite(powerBar.GetComponent<Image>(), HudSpriteKind.CarvedPanel, new Color(0.56f, 0.66f, 0.78f, 1f));
            HudUiFactory.AddLayoutElement(powerBar, -1, 44);

            refs.TotalCombatPowerText = HudUiFactory.CreateText("TotalCombatPower", powerBar.transform, 27, FontStyle.Bold, TextAnchor.MiddleLeft);
            refs.TotalCombatPowerText.text = "종합 전투력 0";
            RectTransform powerTextRect = refs.TotalCombatPowerText.GetComponent<RectTransform>();
            powerTextRect.anchorMin = Vector2.zero;
            powerTextRect.anchorMax = Vector2.one;
            powerTextRect.offsetMin = new Vector2(22f, 2f);
            powerTextRect.offsetMax = new Vector2(-22f, -2f);

            refs.GrowthNoticeText = HudUiFactory.CreateText("GrowthNotice", args.Parent, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            refs.GrowthNoticeText.color = new Color(1f, 0.55f, 0.34f, 1f);
            HudUiFactory.AddLayoutElement(refs.GrowthNoticeText.gameObject, -1, 26);

            CreateStepRow(args);
            CreateAbilityRows(args);
            return refs;
        }

        private static void CreateStepRow(GrowthPanelViewBuildArgs args)
        {
            GameObject stepRow = new GameObject("GrowthStepRow", typeof(RectTransform));
            stepRow.transform.SetParent(args.Parent, false);
            HorizontalLayoutGroup stepLayout = stepRow.AddComponent<HorizontalLayoutGroup>();
            stepLayout.spacing = 6;
            stepLayout.childControlWidth = true;
            stepLayout.childControlHeight = true;
            stepLayout.childForceExpandWidth = true;
            stepLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(stepRow, -1, 56);

            int[] steps = { 1, 10, 100, 1000 };
            foreach (int step in steps)
            {
                Button stepButton = HudUiFactory.CreateButton(step + "x", stepRow.transform, 24, Color.white);
                HudUiFactory.ApplySpriteButtonState(
                    stepButton,
                    HudSpriteKind.BlueMenuButton,
                    HudSpriteKind.BlueMenuButtonPressed,
                    false);
                int capturedStep = step;
                stepButton.onClick.AddListener(() => args.OnSelectLevelStep?.Invoke(capturedStep));
                args.GrowthStepButtons[step] = stepButton;
            }
        }

        private static void CreateAbilityRows(GrowthPanelViewBuildArgs args)
        {
            if (args.Abilities == null)
            {
                return;
            }

            foreach (AbilityState ability in args.Abilities)
            {
                Button button = HudUiFactory.CreateButton(ability.Definition.DisplayName, args.Parent, 22, Color.white);
                HudUiFactory.ApplyButtonSprite(button, HudSpriteKind.ParchmentPanel, new Color(0.58f, 0.70f, 0.82f, 1f));
                HudUiFactory.AddLayoutElement(button.gameObject, -1, 70);

                AbilityKind kind = ability.Definition.Kind;
                HudUiFactory.ConfigureHoldRepeat(button, () => args.OnLevelUpAbility?.Invoke(kind), () => args.CanLevelUpAbility == null || args.CanLevelUpAbility(kind));
                Text rowText = button.GetComponentInChildren<Text>();
                rowText.alignment = TextAnchor.MiddleLeft;
                rowText.color = Color.white;
                RectTransform rowTextRect = rowText.GetComponent<RectTransform>();
                rowTextRect.anchorMin = Vector2.zero;
                rowTextRect.anchorMax = new Vector2(0.70f, 1f);
                rowTextRect.offsetMin = new Vector2(28f, 6f);
                rowTextRect.offsetMax = new Vector2(-10f, -6f);
                args.AbilityButtonTexts[kind] = rowText;

                GameObject costBadge = HudUiFactory.CreatePanel(ability.Definition.Id + "CostBadge", button.transform, Color.white);
                HudUiFactory.ApplySprite(costBadge.GetComponent<Image>(), HudSpriteKind.BigBlueButton, new Color(0.96f, 1f, 0.86f, 1f));
                RectTransform costBadgeRect = costBadge.GetComponent<RectTransform>();
                costBadgeRect.anchorMin = new Vector2(0.72f, 0.12f);
                costBadgeRect.anchorMax = new Vector2(0.985f, 0.88f);
                costBadgeRect.offsetMin = Vector2.zero;
                costBadgeRect.offsetMax = Vector2.zero;

                Text costText = HudUiFactory.CreateText(ability.Definition.Id + "CostText", costBadge.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
                costText.color = new Color(0.04f, 0.06f, 0.05f, 1f);
                HudUiFactory.StretchToParent(costText.gameObject);
                args.AbilityCostBadgeTexts[kind] = costText;

                args.AbilityNotificationDots[kind] = HudUiFactory.CreateNotificationDot(button.transform, 40f, new Vector2(-16f, -16f));
            }
        }
    }
}
