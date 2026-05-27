using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Progression;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Trait
{
    public sealed class HeroTraitPanelPresenterArgs
    {
        public AccountProgressManager AccountProgressManager;
        public string SelectedTalentId;
        public Text SummaryText;
        public Text DetailText;
        public Button LevelUpButton;
        public Dictionary<string, Text> ButtonTexts;
        public Dictionary<string, Button> Buttons;
        public Func<GameNumber, string> FormatShortNumber;
    }

    public sealed class HeroTraitPanelPresenterResult
    {
        public string SelectedTalentId;
    }

    public static class HeroTraitPanelPresenter
    {
        private static readonly Color SelectedColor = new Color(0.34f, 0.85f, 0.86f, 1f);
        private static readonly Color LockedColor = new Color(0.24f, 0.27f, 0.32f, 1f);
        private static readonly Color MaxedColor = new Color(0.88f, 0.63f, 0.16f, 1f);
        private static readonly Color AvailableColor = new Color(0.22f, 0.48f, 0.58f, 1f);
        private static readonly Color CanLevelColor = new Color(0.54f, 0.78f, 0.22f, 1f);
        private static readonly Color CannotLevelColor = new Color(0.34f, 0.36f, 0.40f, 1f);

        public static HeroTraitPanelPresenterResult Refresh(HeroTraitPanelPresenterArgs args)
        {
            var result = new HeroTraitPanelPresenterResult
            {
                SelectedTalentId = args != null ? args.SelectedTalentId : string.Empty
            };

            if (args == null)
            {
                return result;
            }

            AccountProgressManager account = args.AccountProgressManager;
            if (account == null)
            {
                if (args.SummaryText != null)
                {
                    args.SummaryText.text = "계정 성장 데이터를 불러오는 중";
                }

                return result;
            }

            TalentDefinition selectedTalent = TalentData.GetTalent(args.SelectedTalentId);
            result.SelectedTalentId = selectedTalent.Id;

            ApplySummary(args, account);
            ApplyTalentButtons(args, account, selectedTalent);
            ApplyDetail(args, account, selectedTalent);
            ApplyLevelUpButton(args, account, selectedTalent);
            return result;
        }

        private static void ApplySummary(HeroTraitPanelPresenterArgs args, AccountProgressManager account)
        {
            if (args.SummaryText == null)
            {
                return;
            }

            Func<GameNumber, string> format = args.FormatShortNumber ?? NumberFormatter.Format;
            args.SummaryText.text = "계정 Lv." + account.Level
                + "  EXP " + format(account.Experience)
                + "/" + format(account.NextLevelExperience)
                + "  특성 포인트 " + account.AvailableTalentPoints
                + "/" + account.TotalTalentPointsEarned;
        }

        private static void ApplyTalentButtons(
            HeroTraitPanelPresenterArgs args,
            AccountProgressManager account,
            TalentDefinition selectedTalent)
        {
            foreach (TalentDefinition talent in TalentData.Talents)
            {
                int level = account.GetTalentLevel(talent.Id);
                bool unlocked = account.IsTalentUnlocked(talent);
                bool maxed = level >= talent.MaxLevel;
                bool selected = selectedTalent != null && talent.Id == selectedTalent.Id;

                if (args.ButtonTexts != null && args.ButtonTexts.TryGetValue(talent.Id, out Text text) && text != null)
                {
                    text.text = talent.Icon
                        + "\n" + talent.DisplayName
                        + "\n" + (maxed ? "MAX" : level + "/" + talent.MaxLevel)
                        + (unlocked ? string.Empty : "\n잠김");
                }

                if (args.Buttons != null && args.Buttons.TryGetValue(talent.Id, out Button button) && button != null)
                {
                    HudUiFactory.SetButtonColor(button, GetNodeColor(unlocked, maxed, selected));
                }
            }
        }

        private static void ApplyDetail(
            HeroTraitPanelPresenterArgs args,
            AccountProgressManager account,
            TalentDefinition selectedTalent)
        {
            if (args.DetailText == null || selectedTalent == null)
            {
                return;
            }

            int selectedLevel = account.GetTalentLevel(selectedTalent.Id);
            bool selectedUnlocked = account.IsTalentUnlocked(selectedTalent);
            bool selectedMaxed = selectedLevel >= selectedTalent.MaxLevel;
            args.DetailText.text = selectedTalent.Icon + " " + selectedTalent.DisplayName
                + " [" + selectedTalent.BranchName + "]"
                + "\n현재: " + selectedTalent.FormatValue(selectedLevel)
                + (selectedMaxed ? "\n다음: MAX" : "\n다음: " + selectedTalent.FormatValue(selectedLevel + 1))
                + "\nLv." + selectedLevel + "/" + selectedTalent.MaxLevel
                + (!selectedUnlocked ? BuildUnlockConditionText(selectedTalent) : string.Empty);
        }

        private static void ApplyLevelUpButton(
            HeroTraitPanelPresenterArgs args,
            AccountProgressManager account,
            TalentDefinition selectedTalent)
        {
            if (args.LevelUpButton == null || selectedTalent == null)
            {
                return;
            }

            int selectedLevel = account.GetTalentLevel(selectedTalent.Id);
            bool selectedUnlocked = account.IsTalentUnlocked(selectedTalent);
            bool selectedMaxed = selectedLevel >= selectedTalent.MaxLevel;
            bool canLevel = selectedUnlocked
                && !selectedMaxed
                && account.AvailableTalentPoints >= selectedTalent.CostPerLevel;
            HudUiFactory.SetButtonText(
                args.LevelUpButton,
                selectedMaxed ? "MAX" : "레벨업\n" + selectedTalent.CostPerLevel + "P");
            HudUiFactory.SetButtonColor(args.LevelUpButton, canLevel ? CanLevelColor : CannotLevelColor);
        }

        private static Color GetNodeColor(bool unlocked, bool maxed, bool selected)
        {
            if (selected)
            {
                return SelectedColor;
            }

            if (!unlocked)
            {
                return LockedColor;
            }

            return maxed ? MaxedColor : AvailableColor;
        }

        private static string BuildUnlockConditionText(TalentDefinition talent)
        {
            IReadOnlyList<TalentDefinition> prerequisites = TalentData.GetPrerequisiteTalents(talent);
            if (prerequisites.Count == 0)
            {
                return string.Empty;
            }

            string label = prerequisites[0].DisplayName;
            for (int i = 1; i < prerequisites.Count; i++)
            {
                label += " / " + prerequisites[i].DisplayName;
            }

            return "\n해금 조건: 연결된 이전 특성 MAX (" + label + ")";
        }
    }
}
