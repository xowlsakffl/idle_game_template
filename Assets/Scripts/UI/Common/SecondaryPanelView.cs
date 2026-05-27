using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Gacha;
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
