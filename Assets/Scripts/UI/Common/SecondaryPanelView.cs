using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Gacha;
using IdleGame.Progression;
using IdleGame.UI.Support;

namespace IdleGame.UI.Common
{
    public sealed class SummonPanelViewRefs
    {
        public Text ResultText;
    }

    public sealed class SupportPanelViewRefs
    {
        public Text SummaryText;
    }

    public sealed class StagePanelViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<StageDefinition> Stages;
        public Action OnResumeAutoProgress;
        public Action<string> OnSelectStage;
        public Dictionary<string, Button> StageButtons;
    }

    public sealed class DungeonPanelViewRefs
    {
        public Text SummaryText;
        public readonly Dictionary<DungeonKind, Button> DungeonButtons = new Dictionary<DungeonKind, Button>();
        public readonly Dictionary<DungeonKind, Text> DungeonTexts = new Dictionary<DungeonKind, Text>();
        public GameObject DetailPopupRoot;
        public Text DetailTitleText;
        public Text DetailLevelText;
        public Text DetailRewardText;
        public Text DetailEntryText;
        public Button DetailPrevButton;
        public Button DetailNextButton;
        public Button DetailRepeatButton;
        public Button DetailSweepButton;
        public Button DetailEnterButton;
        public Button DetailCloseButton;
    }

    public sealed class DungeonPanelViewBuildArgs
    {
        public Transform Parent;
        public CurrencyWallet Wallet;
        public DungeonProgressManager DungeonManager;
        public Func<DungeonKind> GetSelectedDungeon;
        public Func<int> GetSelectedDungeonLevel;
        public Func<bool> GetRepeatDungeon;
        public Action<DungeonKind> OnOpenDungeon;
        public Action<int> OnChangeDungeonLevel;
        public Action OnToggleRepeatDungeon;
        public Action OnEnterDungeon;
        public Action OnSweepDungeon;
        public Action OnCloseDungeon;
        public Func<GameNumber, string> FormatGameNumber;
        public Func<long, string> FormatCountNumber;
    }

    public sealed class SummonPanelViewBuildArgs
    {
        public Transform Parent;
        public Action<int> OnRollHeroes;
        public Action<int> OnRollEquipment;
    }

    public sealed class ShopPanelViewBuildArgs
    {
        public Transform Parent;
        public Action OnSelectPremiumSpeed;
    }

    public sealed class SupportPanelViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<CombatSkillState> Skills;
        public IReadOnlyList<PetState> Pets;
        public Dictionary<string, Text> SkillStatusTexts;
        public Dictionary<string, Text> PetStatusTexts;
    }

    public static class SecondaryPanelView
    {
        public static void BuildStagePanel(StagePanelViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return;
            }

            ConfigurePanelLayout(args.Parent, 16);

            Button resumeButton = HudUiFactory.CreateButton("자동 진행 재개", args.Parent, 30, new Color(0.12f, 0.34f, 0.30f, 1f));
            HudUiFactory.AddLayoutElement(resumeButton.gameObject, -1, 86);
            resumeButton.onClick.AddListener(() => args.OnResumeAutoProgress?.Invoke());

            GameObject gridObject = new GameObject("StageGrid", typeof(RectTransform));
            gridObject.transform.SetParent(args.Parent, false);
            GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(228, 72);
            grid.spacing = new Vector2(14, 14);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            HudUiFactory.AddLayoutElement(gridObject, -1, 430);

            if (args.Stages == null)
            {
                return;
            }

            foreach (StageDefinition stage in args.Stages)
            {
                Color buttonColor = stage.Type == StageType.Boss
                    ? new Color(0.42f, 0.18f, 0.16f, 1f)
                    : new Color(0.20f, 0.24f, 0.31f, 1f);
                Button button = HudUiFactory.CreateButton(stage.Id, gridObject.transform, 26, buttonColor);
                string stageId = stage.Id;
                button.onClick.AddListener(() => args.OnSelectStage?.Invoke(stageId));
                args.StageButtons[stage.Id] = button;
            }
        }

        public static DungeonPanelViewRefs BuildDungeonPanel(DungeonPanelViewBuildArgs args)
        {
            var refs = new DungeonPanelViewRefs();
            if (args == null || args.Parent == null)
            {
                return refs;
            }

            ConfigurePanelLayout(args.Parent, 12);
            CreateTitle(args.Parent, "DungeonTitle", "던전", 36, 54);

            refs.SummaryText = HudUiFactory.CreateText("DungeonSummary", args.Parent, 22, FontStyle.Bold, TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, 42);

            GameObject gridObject = new GameObject("DungeonGrid", typeof(RectTransform));
            gridObject.transform.SetParent(args.Parent, false);
            GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(430f, 150f);
            grid.spacing = new Vector2(18f, 18f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            HudUiFactory.AddLayoutElement(gridObject, -1, 330);

            CreateDungeonCard(refs, gridObject.transform, DungeonKind.Ruby, new Color(0.28f, 0.16f, 0.38f, 1f), args);
            CreateDungeonCard(refs, gridObject.transform, DungeonKind.Gold, new Color(0.30f, 0.24f, 0.12f, 1f), args);
            CreateDungeonCard(refs, gridObject.transform, DungeonKind.TotemEssence, new Color(0.24f, 0.20f, 0.32f, 1f), args);
            CreateDungeonCard(refs, gridObject.transform, DungeonKind.HeroTranscendStone, new Color(0.34f, 0.18f, 0.26f, 1f), args);

            BuildDungeonDetailPopup(args.Parent, refs, args);
            ApplyDungeonPanelState(
                refs,
                args.Wallet,
                args.DungeonManager,
                GetSelectedDungeon(args),
                GetSelectedDungeonLevel(args),
                GetRepeatDungeon(args),
                false,
                args.FormatGameNumber,
                args.FormatCountNumber);
            return refs;
        }

        public static SummonPanelViewRefs BuildSummonPanel(SummonPanelViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new SummonPanelViewRefs();
            }

            ConfigurePanelLayout(args.Parent, 16);
            CreateTitle(args.Parent, "SummonTitle", "소환", 36, 58);
            CreateTitle(args.Parent, "HeroSummonTitle", "영웅 뽑기", 27, 36);

            GameObject heroButtonRow = CreateButtonRow(args.Parent, "HeroSummonButtons", 78);
            Button heroRollOne = HudUiFactory.CreateButton("영웅 1회", heroButtonRow.transform, 28, new Color(0.36f, 0.24f, 0.45f, 1f));
            Button heroRollTen = HudUiFactory.CreateButton("영웅 10회", heroButtonRow.transform, 28, new Color(0.36f, 0.24f, 0.45f, 1f));
            heroRollOne.onClick.AddListener(() => args.OnRollHeroes?.Invoke(1));
            heroRollTen.onClick.AddListener(() => args.OnRollHeroes?.Invoke(10));

            CreateTitle(args.Parent, "EquipmentSummonTitle", "장비 뽑기", 27, 36);

            GameObject equipmentButtonRow = CreateButtonRow(args.Parent, "EquipmentSummonButtons", 78);
            Button equipmentRollOne = HudUiFactory.CreateButton("장비 1회", equipmentButtonRow.transform, 28, new Color(0.24f, 0.32f, 0.44f, 1f));
            Button equipmentRollTen = HudUiFactory.CreateButton("장비 10회", equipmentButtonRow.transform, 28, new Color(0.24f, 0.32f, 0.44f, 1f));
            equipmentRollOne.onClick.AddListener(() => args.OnRollEquipment?.Invoke(1));
            equipmentRollTen.onClick.AddListener(() => args.OnRollEquipment?.Invoke(10));

            Text rule = HudUiFactory.CreateText("SummonRule", args.Parent, 25, FontStyle.Normal, TextAnchor.UpperLeft);
            rule.text = "영웅: 뽑기권 우선, 부족분 루비 150개"
                + "\n장비: 장비 뽑기권 우선, 부족분 루비 100개"
                + "\n확률: " + GachaManager.GetRateSummaryText()
                + "\n영웅 조각: 1회당 선택 영웅 조각 1개";
            HudUiFactory.AddLayoutElement(rule.gameObject, -1, 120);

            SummonPanelViewRefs refs = new SummonPanelViewRefs();
            refs.ResultText = HudUiFactory.CreateText("GachaResult", args.Parent, 26, FontStyle.Normal, TextAnchor.UpperLeft);
            HudUiFactory.AddLayoutElement(refs.ResultText.gameObject, -1, 256);
            return refs;
        }

        public static void BuildShopPanel(ShopPanelViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return;
            }

            ConfigurePanelLayout(args.Parent, 16);
            CreateTitle(args.Parent, "ShopTitle", "상점", 36, 58);

            Button speedProduct = HudUiFactory.CreateButton("4x", args.Parent, 30, new Color(0.42f, 0.28f, 0.14f, 1f));
            HudUiFactory.AddLayoutElement(speedProduct.gameObject, -1, 96);
            speedProduct.onClick.AddListener(() => args.OnSelectPremiumSpeed?.Invoke());

            Button rubyProduct = HudUiFactory.CreateButton("Ruby", args.Parent, 30, new Color(0.36f, 0.18f, 0.42f, 1f));
            HudUiFactory.AddLayoutElement(rubyProduct.gameObject, -1, 96);

            Button ticketProduct = HudUiFactory.CreateButton("Ticket", args.Parent, 30, new Color(0.24f, 0.30f, 0.42f, 1f));
            HudUiFactory.AddLayoutElement(ticketProduct.gameObject, -1, 96);
        }

        public static void ApplyDungeonPanelState(
            DungeonPanelViewRefs refs,
            CurrencyWallet wallet,
            DungeonProgressManager dungeonManager,
            DungeonKind selectedDungeon,
            int selectedLevel,
            bool repeat,
            bool detailOpen,
            Func<GameNumber, string> formatGameNumber,
            Func<long, string> formatCountNumber)
        {
            if (refs == null)
            {
                return;
            }

            if (refs.SummaryText != null)
            {
                int freeLeft = dungeonManager != null ? dungeonManager.FreeEntriesRemaining : 0;
                long tickets = wallet != null ? wallet.DungeonTicket : 0;
                refs.SummaryText.text = "오늘 무료 " + freeLeft + "/" + DungeonProgressManager.DailyFreeEntryLimit
                    + "   티켓 " + FormatCount(formatCountNumber, tickets)
                    + "   실패 시 반환";
            }

            ApplyDungeonCardSelection(refs, selectedDungeon);
            SetDungeonText(refs, DungeonKind.Ruby, dungeonManager, "보유 " + FormatCount(formatCountNumber, wallet != null ? wallet.Ruby : 0));
            SetDungeonText(refs, DungeonKind.Gold, dungeonManager, "보유 " + FormatGameNumber(formatGameNumber, wallet != null ? wallet.Gold : GameNumber.Zero));
            SetDungeonText(refs, DungeonKind.TotemEssence, dungeonManager, "보유 " + FormatCount(formatCountNumber, wallet != null ? wallet.TotemEssence : 0));
            SetDungeonText(refs, DungeonKind.HeroTranscendStone, dungeonManager, "보유 " + FormatCount(formatCountNumber, wallet != null ? wallet.HeroTranscendStone : 0));
            ApplyDungeonDetailState(refs, wallet, dungeonManager, selectedDungeon, selectedLevel, repeat, detailOpen, formatCountNumber);
        }

        public static SupportPanelViewRefs BuildSupportPanel(SupportPanelViewBuildArgs args)
        {
            if (args == null || args.Parent == null)
            {
                return new SupportPanelViewRefs();
            }

            ConfigurePanelLayout(args.Parent, 14);
            CreateTitle(args.Parent, "SupportTitle", "지원 - 자동 스킬과 펫", 34, 54);

            SupportPanelViewRefs refs = new SupportPanelViewRefs();
            refs.SummaryText = HudUiFactory.CreateText("PartySupportInfo", args.Parent, 26, FontStyle.Bold, TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, 46);

            CreateTitle(args.Parent, "SkillSupportTitle", "자동 스킬", 28, 42);
            if (args.Skills != null)
            {
                foreach (CombatSkillState skill in args.Skills)
                {
                    Text text = CreateSupportStatusRow(args.Parent, skill.Definition.Id + "SupportRow", skill.Definition.Id + "SupportText", new Color(0.19f, 0.24f, 0.28f, 1f), 96);
                    args.SkillStatusTexts[skill.Definition.Id] = text;
                }
            }

            CreateTitle(args.Parent, "PetSupportTitle", "펫", 28, 42);
            if (args.Pets != null)
            {
                foreach (PetState pet in args.Pets)
                {
                    Text text = CreateSupportStatusRow(args.Parent, pet.Definition.Id + "SupportRow", pet.Definition.Id + "SupportText", new Color(0.18f, 0.26f, 0.22f, 1f), 106);
                    args.PetStatusTexts[pet.Definition.Id] = text;
                }
            }

            return refs;
        }

        public static void ApplySupportPanelState(
            Text summaryText,
            IReadOnlyList<CombatSkillState> skills,
            IReadOnlyList<PetState> pets,
            double partyAttackPower,
            double petGoldBonusPercent,
            Func<double, string> formatShort,
            Dictionary<string, Text> skillStatusTexts,
            Dictionary<string, Text> petStatusTexts)
        {
            if (summaryText != null)
            {
                summaryText.text = SupportPanelStateBuilder.BuildSummary(
                    partyAttackPower,
                    petGoldBonusPercent,
                    formatShort);
            }

            if (skills != null && skillStatusTexts != null)
            {
                for (int i = 0; i < skills.Count; i++)
                {
                    CombatSkillState skill = skills[i];
                    if (skill == null || skill.Definition == null)
                    {
                        continue;
                    }

                    if (skillStatusTexts.TryGetValue(skill.Definition.Id, out Text text))
                    {
                        text.text = SupportPanelStateBuilder.BuildSkillStatus(
                            skill,
                            partyAttackPower,
                            formatShort);
                    }
                }
            }

            if (pets != null && petStatusTexts != null)
            {
                for (int i = 0; i < pets.Count; i++)
                {
                    PetState pet = pets[i];
                    if (pet == null || pet.Definition == null)
                    {
                        continue;
                    }

                    if (petStatusTexts.TryGetValue(pet.Definition.Id, out Text text))
                    {
                        text.text = SupportPanelStateBuilder.BuildPetStatus(
                            pet,
                            petGoldBonusPercent,
                            formatShort);
                    }
                }
            }
        }

        private static void ConfigurePanelLayout(Transform parent, int spacing)
        {
            VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 22);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static Text CreateTitle(Transform parent, string name, string text, int fontSize, float height)
        {
            Text title = HudUiFactory.CreateText(name, parent, fontSize, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.text = text;
            HudUiFactory.AddLayoutElement(title.gameObject, -1, height);
            return title;
        }

        private static void CreateDungeonCard(DungeonPanelViewRefs refs, Transform parent, DungeonKind kind, Color color, DungeonPanelViewBuildArgs args)
        {
            Button card = HudUiFactory.CreateButton(DungeonProgressManager.GetTitle(kind), parent, 22, color);
            HudUiFactory.ApplyButtonSprite(card, HudSpriteKind.BluePanel, color);

            Image icon = HudUiFactory.CreateIcon(kind + "DungeonIcon", card.transform, GetDungeonIcon(kind), new Vector2(64f, 64f));
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(22f, 0f);
            icon.color = Color.white;

            Text text = card.GetComponentInChildren<Text>(true);
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(104f, 12f);
            rect.offsetMax = new Vector2(-16f, -12f);
            text.alignment = TextAnchor.MiddleLeft;
            text.lineSpacing = 0.86f;
            HudUiFactory.ConfigureBestFitText(text, 12, 22, 0.86f);
            DungeonKind selectedKind = kind;
            card.onClick.AddListener(() => args.OnOpenDungeon?.Invoke(selectedKind));
            refs.DungeonButtons[kind] = card;
            refs.DungeonTexts[kind] = text;
        }

        private static void SetDungeonText(DungeonPanelViewRefs refs, DungeonKind kind, DungeonProgressManager dungeonManager, string value)
        {
            if (refs == null || !refs.DungeonTexts.TryGetValue(kind, out Text text) || text == null)
            {
                return;
            }

            int highest = dungeonManager != null ? dungeonManager.GetHighestClearLevel(kind) : 0;
            int next = dungeonManager != null ? dungeonManager.GetMaxSelectableLevel(kind) : 1;
            string reward = dungeonManager != null ? dungeonManager.GetRewardText(kind, next) : "0";
            text.text = DungeonProgressManager.HasSelectableLevel(kind)
                ? DungeonProgressManager.GetTitle(kind)
                    + "\n최고 Lv." + highest + "  다음 Lv." + next
                    + "\n보상 " + reward
                    + "\n" + value
                : DungeonProgressManager.GetTitle(kind)
                    + "\n최고 보스 " + highest
                    + "\n처치 수 누적 보상"
                    + "\n" + value;
        }

        private static void BuildDungeonDetailPopup(Transform root, DungeonPanelViewRefs refs, DungeonPanelViewBuildArgs args)
        {
            refs.DetailPopupRoot = HudUiFactory.CreatePanel("DungeonDetailPopup", root, new Color(0f, 0f, 0f, 0.72f));
            HudUiFactory.StretchToParent(refs.DetailPopupRoot);
            refs.DetailPopupRoot.SetActive(false);

            GameObject modal = HudUiFactory.CreateSpritePanel("DungeonDetailPanel", refs.DetailPopupRoot.transform, HudSpriteKind.BluePanel, new Color(0.35f, 0.44f, 0.62f, 1f));
            SetPopupRect(modal, new Vector2(760f, 820f), new Vector2(0f, -30f));

            GameObject title = HudUiFactory.CreateSpritePanel("DungeonDetailTitle", modal.transform, HudSpriteKind.BlueRibbon, Color.white);
            SetPopupRect(title, new Vector2(560f, 76f), new Vector2(0f, 350f));
            refs.DetailTitleText = HudUiFactory.CreateText("DungeonDetailTitleText", title.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(refs.DetailTitleText.gameObject);

            refs.DetailPrevButton = HudUiFactory.CreateButton("<", modal.transform, 42, new Color(0.36f, 0.58f, 0.78f, 1f));
            SetPopupRect(refs.DetailPrevButton.gameObject, new Vector2(88f, 96f), new Vector2(-260f, 115f));
            refs.DetailPrevButton.onClick.AddListener(() => args.OnChangeDungeonLevel?.Invoke(-1));

            refs.DetailNextButton = HudUiFactory.CreateButton(">", modal.transform, 42, new Color(0.36f, 0.58f, 0.78f, 1f));
            SetPopupRect(refs.DetailNextButton.gameObject, new Vector2(88f, 96f), new Vector2(260f, 115f));
            refs.DetailNextButton.onClick.AddListener(() => args.OnChangeDungeonLevel?.Invoke(1));

            GameObject levelBadge = HudUiFactory.CreateSpritePanel("DungeonLevelBadge", modal.transform, HudSpriteKind.SmallRedSquareButton, new Color(0.82f, 0.18f, 0.82f, 1f));
            SetPopupRect(levelBadge, new Vector2(190f, 190f), new Vector2(0f, 115f));
            refs.DetailLevelText = HudUiFactory.CreateText("DungeonLevelText", levelBadge.transform, 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(refs.DetailLevelText.gameObject);

            GameObject rewardBox = HudUiFactory.CreateSpritePanel("DungeonRewardBox", modal.transform, HudSpriteKind.CarvedPanel, new Color(0.27f, 0.33f, 0.46f, 1f));
            SetPopupRect(rewardBox, new Vector2(640f, 170f), new Vector2(0f, -95f));
            refs.DetailRewardText = HudUiFactory.CreateText("DungeonRewardText", rewardBox.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(refs.DetailRewardText.gameObject);

            refs.DetailEntryText = HudUiFactory.CreateText("DungeonEntryText", modal.transform, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetPopupRect(refs.DetailEntryText.gameObject, new Vector2(640f, 64f), new Vector2(0f, -220f));
            refs.DetailEntryText.lineSpacing = 0.86f;
            HudUiFactory.ConfigureBestFitText(refs.DetailEntryText, 16, 25, 0.86f);

            refs.DetailRepeatButton = HudUiFactory.CreateButton("☐ 연속 도전", modal.transform, 24, new Color(0.25f, 0.32f, 0.46f, 1f));
            SetPopupRect(refs.DetailRepeatButton.gameObject, new Vector2(430f, 62f), new Vector2(0f, -285f));
            refs.DetailRepeatButton.onClick.AddListener(() => args.OnToggleRepeatDungeon?.Invoke());

            refs.DetailSweepButton = HudUiFactory.CreateButton("소탕하기", modal.transform, 30, new Color(0.38f, 0.38f, 0.38f, 1f));
            SetPopupRect(refs.DetailSweepButton.gameObject, new Vector2(270f, 84f), new Vector2(-165f, -380f));
            refs.DetailSweepButton.onClick.AddListener(() => args.OnSweepDungeon?.Invoke());

            refs.DetailEnterButton = HudUiFactory.CreateButton("입장하기", modal.transform, 30, new Color(0.48f, 0.78f, 0.12f, 1f));
            SetPopupRect(refs.DetailEnterButton.gameObject, new Vector2(270f, 84f), new Vector2(165f, -380f));
            refs.DetailEnterButton.onClick.AddListener(() => args.OnEnterDungeon?.Invoke());

            refs.DetailCloseButton = HudUiFactory.CreateButton("X", refs.DetailPopupRoot.transform, 34, new Color(0.35f, 0.46f, 0.66f, 1f));
            SetPopupRect(refs.DetailCloseButton.gameObject, new Vector2(86f, 76f), new Vector2(0f, -470f));
            refs.DetailCloseButton.onClick.AddListener(() => args.OnCloseDungeon?.Invoke());
        }

        private static void ApplyDungeonCardSelection(DungeonPanelViewRefs refs, DungeonKind selectedDungeon)
        {
            if (refs == null)
            {
                return;
            }

            foreach (KeyValuePair<DungeonKind, Button> pair in refs.DungeonButtons)
            {
                bool selected = pair.Key == selectedDungeon;
                Color color = GetDungeonCardColor(pair.Key, selected);
                HudUiFactory.ApplyButtonSprite(pair.Value, selected ? HudSpriteKind.BluePanel : HudSpriteKind.CarvedPanel, color);
            }
        }

        private static Color GetDungeonCardColor(DungeonKind kind, bool selected)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return selected ? new Color(1f, 0.78f, 0.30f, 1f) : new Color(0.72f, 0.52f, 0.22f, 1f);
                case DungeonKind.TotemEssence:
                    return selected ? new Color(0.34f, 0.82f, 0.72f, 1f) : new Color(0.24f, 0.54f, 0.50f, 1f);
                case DungeonKind.HeroTranscendStone:
                    return selected ? new Color(0.76f, 0.62f, 1f, 1f) : new Color(0.48f, 0.36f, 0.72f, 1f);
                default:
                    return selected ? new Color(1f, 0.50f, 0.72f, 1f) : new Color(0.68f, 0.30f, 0.46f, 1f);
            }
        }

        private static HudSpriteKind GetDungeonIcon(DungeonKind kind)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return HudSpriteKind.IconGold;
                case DungeonKind.TotemEssence:
                    return HudSpriteKind.IconTotemEssence;
                case DungeonKind.HeroTranscendStone:
                    return HudSpriteKind.IconTranscendStone;
                default:
                    return HudSpriteKind.IconRuby;
            }
        }

        private static void ApplyDungeonDetailState(
            DungeonPanelViewRefs refs,
            CurrencyWallet wallet,
            DungeonProgressManager dungeonManager,
            DungeonKind selectedDungeon,
            int selectedLevel,
            bool repeat,
            bool detailOpen,
            Func<long, string> formatCountNumber)
        {
            if (refs.DetailPopupRoot != null)
            {
                refs.DetailPopupRoot.SetActive(detailOpen);
            }

            if (!detailOpen)
            {
                return;
            }

            bool hasSelectableLevel = DungeonProgressManager.HasSelectableLevel(selectedDungeon);
            int level = dungeonManager != null ? dungeonManager.ClampSelectableLevel(selectedDungeon, selectedLevel) : Mathf.Max(1, selectedLevel);
            bool canEnter = dungeonManager != null && dungeonManager.CanEnter;
            bool canSweep = hasSelectableLevel && dungeonManager != null && dungeonManager.CanSweep(selectedDungeon, level);
            int maxLevel = dungeonManager != null ? dungeonManager.GetMaxSelectableLevel(selectedDungeon) : 1;
            if (refs.DetailTitleText != null)
            {
                refs.DetailTitleText.text = DungeonProgressManager.GetTitle(selectedDungeon);
            }

            if (refs.DetailLevelText != null)
            {
                refs.DetailLevelText.text = hasSelectableLevel
                    ? "레벨\n" + level
                    : "보스\n무한";
            }

            if (refs.DetailRewardText != null)
            {
                string reward = dungeonManager != null ? dungeonManager.GetRewardText(selectedDungeon, level) : "0";
                refs.DetailRewardText.text = hasSelectableLevel
                    ? "클리어 보상\n" + reward
                    : "제한시간 누적 보상\n처치한 보스만큼 토템석 지급";
            }

            if (refs.DetailEntryText != null)
            {
                int freeLeft = dungeonManager != null ? dungeonManager.FreeEntriesRemaining : 0;
                long tickets = wallet != null ? wallet.DungeonTicket : 0;
                string nextCost = freeLeft > 0 ? "이번 입장 무료" : tickets > 0 ? "이번 입장 티켓 1장" : "입장권 부족";
                if (hasSelectableLevel)
                {
                    string sweepState = canSweep ? "소탕 가능" : "클리어한 레벨만 소탕";
                    refs.DetailEntryText.text = "오늘 무료 " + freeLeft + "/" + DungeonProgressManager.DailyFreeEntryLimit
                        + "   티켓 " + FormatCount(formatCountNumber, tickets)
                        + "\n" + nextCost + "   " + sweepState;
                }
                else
                {
                    int best = dungeonManager != null ? dungeonManager.GetHighestClearLevel(selectedDungeon) : 0;
                    refs.DetailEntryText.text = "오늘 무료 " + freeLeft + "/" + DungeonProgressManager.DailyFreeEntryLimit
                        + "   티켓 " + FormatCount(formatCountNumber, tickets)
                        + "\n" + nextCost + "   제한시간 " + Mathf.RoundToInt(DungeonProgressManager.GetTimeLimitSeconds(selectedDungeon))
                        + "초   최고 보스 " + best;
                }
            }

            SetButtonVisible(refs.DetailPrevButton, hasSelectableLevel);
            SetButtonVisible(refs.DetailNextButton, hasSelectableLevel);
            SetButtonVisible(refs.DetailSweepButton, hasSelectableLevel);
            if (refs.DetailEnterButton != null)
            {
                SetPopupRect(
                    refs.DetailEnterButton.gameObject,
                    hasSelectableLevel ? new Vector2(270f, 84f) : new Vector2(430f, 84f),
                    hasSelectableLevel ? new Vector2(165f, -380f) : new Vector2(0f, -380f));
            }

            SetButtonEnabled(refs.DetailPrevButton, hasSelectableLevel && level > 1);
            SetButtonEnabled(refs.DetailNextButton, hasSelectableLevel && level < maxLevel);
            SetButtonEnabled(refs.DetailEnterButton, canEnter);
            SetButtonEnabled(refs.DetailSweepButton, canSweep);
            HudUiFactory.SetButtonText(refs.DetailEnterButton, canEnter ? (dungeonManager != null && dungeonManager.FreeEntriesRemaining > 0 ? "무료 입장" : "티켓 입장") : "입장 불가");
            HudUiFactory.SetButtonText(refs.DetailSweepButton, canSweep ? "소탕하기" : "소탕 잠김");
            HudUiFactory.SetButtonText(refs.DetailRepeatButton, repeat ? "☑ 연속 도전" : "☐ 연속 도전");
        }

        private static DungeonKind GetSelectedDungeon(DungeonPanelViewBuildArgs args)
        {
            return args != null && args.GetSelectedDungeon != null ? args.GetSelectedDungeon() : DungeonKind.Ruby;
        }

        private static int GetSelectedDungeonLevel(DungeonPanelViewBuildArgs args)
        {
            return args != null && args.GetSelectedDungeonLevel != null ? args.GetSelectedDungeonLevel() : 1;
        }

        private static bool GetRepeatDungeon(DungeonPanelViewBuildArgs args)
        {
            return args != null && args.GetRepeatDungeon != null && args.GetRepeatDungeon();
        }

        private static void SetButtonEnabled(Button button, bool enabled)
        {
            if (button != null)
            {
                button.interactable = enabled;
            }
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }

        private static void SetPopupRect(GameObject target, Vector2 size, Vector2 position)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static string FormatGameNumber(Func<GameNumber, string> formatter, GameNumber value)
        {
            return formatter != null ? formatter(value) : NumberFormatter.Format(value);
        }

        private static string FormatCount(Func<long, string> formatter, long value)
        {
            return formatter != null ? formatter(value) : GameData.ClampCount(value).ToString("#,0");
        }

        private static GameObject CreateButtonRow(Transform parent, string name, float height)
        {
            GameObject buttonRow = new GameObject(name, typeof(RectTransform));
            buttonRow.transform.SetParent(parent, false);
            HorizontalLayoutGroup row = buttonRow.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 16;
            row.childControlWidth = true;
            row.childForceExpandWidth = true;
            HudUiFactory.AddLayoutElement(buttonRow, -1, height);
            return buttonRow;
        }

        private static Text CreateSupportStatusRow(Transform parent, string rowName, string textName, Color color, float height)
        {
            GameObject row = HudUiFactory.CreatePanel(rowName, parent, color);
            HudUiFactory.AddLayoutElement(row, -1, height);
            Text text = HudUiFactory.CreateText(textName, row.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18, 8);
            textRect.offsetMax = new Vector2(-18, -8);
            return text;
        }
    }
}
