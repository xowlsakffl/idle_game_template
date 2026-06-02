using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battlefield;
using IdleGame.Speed;
using IdleGame.UI.Common;

namespace IdleGame.UI.Battle
{
    public sealed class BattlePanelViewRefs
    {
        public GameObject Panel;
        public LayoutElement LayoutElement;
        public Text TargetText;
        public Image KillProgressFill;
        public Text KillProgressText;
        public Text ProgressText;
        public Text SupportText;
        public Text LogText;
        public Text RewardText;
        public Button SkillAutoButton;
        public Button FeverAutoButton;
        public Button SpeedCycleButton;
        public RectTransform BattlefieldRect;
        public RawImage BattlefieldWorldImage;
        public Text CenterSpawnText;
        public Text FieldStagePillText;
        public Text DamagePopupText;
        public Text DamageMeterText;
        public Text GuideQuestText;
        public GameObject GuideQuestDot;
    }

    public sealed class BattlePanelViewBuildArgs
    {
        public Transform Parent;
        public BattlefieldWorldView BattlefieldWorldView;
        public BattleHudVisualState VisualState;
        public Dictionary<string, Image> HeroBattleImages;
        public Dictionary<string, Text> HeroBattleTexts;
        public Dictionary<string, RectTransform> HeroBattleRects;
        public List<Image> EnemyBattleImages;
        public List<Text> EnemyBattleTexts;
        public List<RectTransform> EnemyBattleRects;
        public List<GameObject> EnemyHpBarObjects;
        public List<Image> EnemyHpFillImages;
        public List<GameObject> DamageMeterRows;
        public List<Image> DamageMeterFills;
        public List<Text> DamageMeterRowTexts;
        public Action OnToggleSkillAuto;
        public Action OnToggleFeverAuto;
        public Action OnCycleSpeed;
    }

    public static class BattlePanelView
    {
        public static BattlePanelViewRefs Build(BattlePanelViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new BattlePanelViewRefs();
            }

            var refs = new BattlePanelViewRefs();
            refs.Panel = HudUiFactory.CreatePanel("Battle", args.Parent, new Color(0.14f, 0.16f, 0.20f, 1f));
            refs.LayoutElement = HudUiFactory.AddLayoutElement(refs.Panel, -1, HudLayoutConfig.GrowthBattlePanelHeight);

            BattleHudViewRefs battlefieldRefs = BattleHudView.Build(new BattleHudViewBuildArgs
            {
                Parent = refs.Panel.transform,
                BattlefieldWorldView = args.BattlefieldWorldView,
                VisualState = args.VisualState,
                HeroBattleImages = args.HeroBattleImages,
                HeroBattleTexts = args.HeroBattleTexts,
                HeroBattleRects = args.HeroBattleRects,
                EnemyBattleImages = args.EnemyBattleImages,
                EnemyBattleTexts = args.EnemyBattleTexts,
                EnemyBattleRects = args.EnemyBattleRects,
                EnemyHpBarObjects = args.EnemyHpBarObjects,
                EnemyHpFillImages = args.EnemyHpFillImages,
                DamageMeterRows = args.DamageMeterRows,
                DamageMeterFills = args.DamageMeterFills,
                DamageMeterRowTexts = args.DamageMeterRowTexts
            });

            refs.BattlefieldRect = battlefieldRefs.BattlefieldRect;
            refs.BattlefieldWorldImage = battlefieldRefs.BattlefieldWorldImage;
            refs.CenterSpawnText = battlefieldRefs.CenterSpawnText;
            refs.FieldStagePillText = battlefieldRefs.FieldStagePillText;
            refs.DamagePopupText = battlefieldRefs.DamagePopupText;
            refs.DamageMeterText = battlefieldRefs.DamageMeterText;
            refs.GuideQuestText = battlefieldRefs.GuideQuestText;
            refs.GuideQuestDot = battlefieldRefs.GuideQuestDot;

            refs.TargetText = HudUiFactory.CreateText("Target", refs.Panel.transform, 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform targetRect = refs.TargetText.GetComponent<RectTransform>();
            targetRect.anchorMin = new Vector2(0.5f, 1f);
            targetRect.anchorMax = new Vector2(0.5f, 1f);
            targetRect.pivot = new Vector2(0.5f, 1f);
            targetRect.sizeDelta = new Vector2(420f, 48f);
            targetRect.anchoredPosition = new Vector2(0f, -88f);
            refs.TargetText.gameObject.SetActive(false);

            GameObject killProgressBar = HudUiFactory.CreatePanel("KillProgressBar", refs.Panel.transform, new Color(0.03f, 0.04f, 0.05f, 1f));
            RectTransform killProgressRect = killProgressBar.GetComponent<RectTransform>();
            killProgressRect.anchorMin = new Vector2(0.5f, 1f);
            killProgressRect.anchorMax = new Vector2(0.5f, 1f);
            killProgressRect.pivot = new Vector2(0.5f, 1f);
            killProgressRect.sizeDelta = new Vector2(300f, 26f);
            killProgressRect.anchoredPosition = new Vector2(0f, -92f);

            refs.KillProgressFill = HudUiFactory.CreateBarFill("KillProgressFill", killProgressBar.transform, HudSpriteKind.BigBarFill, new Color(0.95f, 0.63f, 0.17f, 1f));
            RectTransform fillRect = refs.KillProgressFill.GetComponent<RectTransform>();
            fillRect.offsetMin = new Vector2(0f, 6f);
            fillRect.offsetMax = new Vector2(0f, -6f);

            refs.KillProgressText = HudUiFactory.CreateText("KillProgressText", killProgressBar.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform killProgressTextRect = refs.KillProgressText.GetComponent<RectTransform>();
            killProgressTextRect.anchorMin = Vector2.zero;
            killProgressTextRect.anchorMax = Vector2.one;
            killProgressTextRect.offsetMin = Vector2.zero;
            killProgressTextRect.offsetMax = Vector2.zero;

            refs.ProgressText = HudUiFactory.CreateText("Progress", refs.Panel.transform, 19, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform progressRect = refs.ProgressText.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.5f, 1f);
            progressRect.anchorMax = new Vector2(0.5f, 1f);
            progressRect.pivot = new Vector2(0.5f, 1f);
            progressRect.sizeDelta = new Vector2(300f, 30f);
            progressRect.anchoredPosition = new Vector2(0f, -120f);

            refs.SupportText = HudUiFactory.CreateText("Support", refs.Panel.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform supportRect = refs.SupportText.GetComponent<RectTransform>();
            supportRect.anchorMin = new Vector2(0f, 1f);
            supportRect.anchorMax = new Vector2(0f, 1f);
            supportRect.pivot = new Vector2(0f, 1f);
            supportRect.sizeDelta = new Vector2(330f, 86f);
            supportRect.anchoredPosition = new Vector2(26f, -214f);

            CreateCombatSpeedControls(args, refs);

            refs.LogText = HudUiFactory.CreateText("Log", refs.Panel.transform, 22, FontStyle.Bold, TextAnchor.LowerLeft);
            RectTransform logRect = refs.LogText.GetComponent<RectTransform>();
            logRect.anchorMin = new Vector2(0f, 0f);
            logRect.anchorMax = new Vector2(0f, 0f);
            logRect.pivot = new Vector2(0f, 0f);
            logRect.sizeDelta = new Vector2(460f, 80f);
            logRect.anchoredPosition = new Vector2(26f, 26f);

            refs.RewardText = HudUiFactory.CreateText("Reward", refs.Panel.transform, 22, FontStyle.Bold, TextAnchor.LowerLeft);
            refs.RewardText.color = new Color(1f, 0.86f, 0.36f, 1f);
            RectTransform rewardRect = refs.RewardText.GetComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0f, 0f);
            rewardRect.anchorMax = new Vector2(0f, 0f);
            rewardRect.pivot = new Vector2(0f, 0f);
            rewardRect.sizeDelta = new Vector2(460f, 38f);
            rewardRect.anchoredPosition = new Vector2(26f, 106f);

            killProgressBar.transform.SetAsLastSibling();
            refs.ProgressText.transform.SetAsLastSibling();
            refs.SupportText.transform.SetAsLastSibling();
            refs.LogText.transform.SetAsLastSibling();
            refs.RewardText.transform.SetAsLastSibling();
            return refs;
        }

        public static void RefreshAutoControlButton(Button button, string label, bool enabled, Color enabledColor, Color disabledColor)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = true;
            HudUiFactory.SetButtonColor(button, enabled ? enabledColor : disabledColor);

            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label + "\nAUTO " + (enabled ? "켜짐" : "꺼짐");
            }
        }

        public static void RefreshSpeedButton(Button button, int currentSpeed)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = true;
            HudUiFactory.SetButtonColor(button, currentSpeed == GameSpeedManager.PremiumSpeed
                ? new Color(0.60f, 0.40f, 0.16f, 1f)
                : currentSpeed == GameSpeedManager.FreeSpeed
                    ? new Color(0.34f, 0.44f, 0.20f, 1f)
                    : new Color(0.18f, 0.24f, 0.32f, 1f));

            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = "가속\n" + currentSpeed + "x";
            }
        }

        private static void CreateCombatSpeedControls(BattlePanelViewBuildArgs args, BattlePanelViewRefs refs)
        {
            GameObject controlRow = new GameObject("BattleAutoControls", typeof(RectTransform));
            controlRow.transform.SetParent(refs.Panel.transform, false);
            RectTransform controlRect = controlRow.GetComponent<RectTransform>();
            controlRect.anchorMin = new Vector2(1f, 0f);
            controlRect.anchorMax = new Vector2(1f, 0f);
            controlRect.pivot = new Vector2(1f, 0f);
            controlRect.sizeDelta = new Vector2(390f, 78f);
            controlRect.anchoredPosition = new Vector2(-26f, 154f);

            HorizontalLayoutGroup row = controlRow.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 10;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;

            refs.SkillAutoButton = CreateAutoControlButton("스킬\nAUTO", controlRow.transform);
            refs.SkillAutoButton.onClick.AddListener(() => args.OnToggleSkillAuto?.Invoke());

            refs.FeverAutoButton = CreateAutoControlButton("피버\nAUTO", controlRow.transform);
            refs.FeverAutoButton.onClick.AddListener(() => args.OnToggleFeverAuto?.Invoke());

            refs.SpeedCycleButton = CreateAutoControlButton("가속\n1x", controlRow.transform);
            refs.SpeedCycleButton.onClick.AddListener(() => args.OnCycleSpeed?.Invoke());
        }

        private static Button CreateAutoControlButton(string label, Transform parent)
        {
            Button button = HudUiFactory.CreateButton(label, parent, 21, new Color(0.25f, 0.25f, 0.20f, 1f));
            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 14;
                text.resizeTextMaxSize = 22;
            }

            return button;
        }
    }
}
