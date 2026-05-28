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
            layout.padding = new RectOffset(24, 24, 22, 22);
            layout.spacing = 6;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = HudUiFactory.CreateText("GrowthTitle", args.Parent, 32, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.text = "성장";
            HudUiFactory.AddLayoutElement(title.gameObject, -1, 38);

            GrowthPanelViewRefs refs = new GrowthPanelViewRefs();
            refs.TotalCombatPowerText = HudUiFactory.CreateText("TotalCombatPower", args.Parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
            refs.TotalCombatPowerText.text = "종합 전투력 0";
            HudUiFactory.AddLayoutElement(refs.TotalCombatPowerText.gameObject, -1, 42);

            refs.GrowthNoticeText = HudUiFactory.CreateText("GrowthNotice", args.Parent, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            refs.GrowthNoticeText.color = new Color(1f, 0.55f, 0.34f, 1f);
            HudUiFactory.AddLayoutElement(refs.GrowthNoticeText.gameObject, -1, 30);

            CreateStepRow(args);
            CreateAbilityRows(args);
            return refs;
        }

        private static void CreateStepRow(GrowthPanelViewBuildArgs args)
        {
            GameObject stepRow = new GameObject("GrowthStepRow", typeof(RectTransform));
            stepRow.transform.SetParent(args.Parent, false);
            HorizontalLayoutGroup stepLayout = stepRow.AddComponent<HorizontalLayoutGroup>();
            stepLayout.spacing = 10;
            stepLayout.childControlWidth = true;
            stepLayout.childControlHeight = true;
            stepLayout.childForceExpandWidth = true;
            stepLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(stepRow, -1, 50);

            int[] steps = { 1, 10, 100, 1000 };
            foreach (int step in steps)
            {
                Button stepButton = HudUiFactory.CreateButton(step + "x", stepRow.transform, 24, new Color(0.18f, 0.24f, 0.38f, 1f));
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
                Button button = HudUiFactory.CreateButton(ability.Definition.DisplayName, args.Parent, 22, new Color(0.48f, 0.54f, 0.66f, 1f));
                HudUiFactory.AddLayoutElement(button.gameObject, -1, 64);

                AbilityKind kind = ability.Definition.Kind;
                HudUiFactory.ConfigureHoldRepeat(button, () => args.OnLevelUpAbility?.Invoke(kind), () => args.CanLevelUpAbility == null || args.CanLevelUpAbility(kind));
                Text rowText = button.GetComponentInChildren<Text>();
                rowText.alignment = TextAnchor.MiddleLeft;
                rowText.color = Color.white;
                RectTransform rowTextRect = rowText.GetComponent<RectTransform>();
                rowTextRect.anchorMin = Vector2.zero;
                rowTextRect.anchorMax = new Vector2(0.70f, 1f);
                rowTextRect.offsetMin = new Vector2(24f, 4f);
                rowTextRect.offsetMax = new Vector2(-8f, -4f);
                args.AbilityButtonTexts[kind] = rowText;

                GameObject costBadge = HudUiFactory.CreatePanel(ability.Definition.Id + "CostBadge", button.transform, new Color(0.56f, 0.88f, 0.24f, 1f));
                RectTransform costBadgeRect = costBadge.GetComponent<RectTransform>();
                costBadgeRect.anchorMin = new Vector2(0.73f, 0.14f);
                costBadgeRect.anchorMax = new Vector2(0.98f, 0.86f);
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
