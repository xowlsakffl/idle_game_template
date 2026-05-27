using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Hero.Formation
{
    public sealed class HeroFormationPanelPresenterArgs
    {
        public HeroPageTab ActivePageTab;
        public int SelectedPreset;
        public int DeployedCount;
        public bool HasPendingChanges;
        public string SelectedHeroForPlacement;
        public string PendingRuneEquipId;
        public GameObject FormationContent;
        public GameObject TraitContent;
        public GameObject TotemContent;
        public GameObject RuneContent;
        public Text PlaceholderText;
        public Text SummaryText;
        public Text OwnedEffectText;
        public Button TotemButton;
        public Button TotemSecondButton;
        public Button TotemRemoveButton;
        public Button TotemSecondRemoveButton;
        public Text TotemText;
        public Text TotemSecondText;
        public Dictionary<HeroPageTab, Button> PageTabButtons;
        public Dictionary<int, Button> PresetButtons;
        public IList<string> EditingFormationHeroIds;
        public List<Text> FormationSlotTexts;
        public Dictionary<int, Button> FormationSlotButtons;
        public Dictionary<int, Button> FormationSlotRemoveButtons;
        public Dictionary<int, Text> RuneSlotTexts;
        public Dictionary<int, Button> RuneSlotButtons;
        public Dictionary<int, Button> RuneSlotRemoveButtons;
        public BattleManager BattleManager;
        public Func<string, HeroState> FindHeroState;
        public Func<HeroPageTab, string> GetPageTabLabel;
        public Func<double, string> FormatShortNumber;
        public Func<HeroDefinition, string> GetShortHeroLabel;
    }

    public sealed class HeroFormationPanelRefreshResult
    {
        public bool TraitOpen;
        public bool TotemOpen;
        public bool RuneOpen;
    }

    public static class HeroFormationPanelPresenter
    {
        private static readonly Color SelectedPageTabColor = new Color(0.42f, 0.54f, 0.82f, 1f);
        private static readonly Color NormalPageTabColor = new Color(0.18f, 0.24f, 0.38f, 1f);
        private static readonly Color SelectedPresetColor = new Color(0.50f, 0.64f, 0.96f, 1f);
        private static readonly Color NormalPresetColor = new Color(0.21f, 0.29f, 0.45f, 1f);
        private static readonly Color TotemDisabledColor = new Color(0.20f, 0.28f, 0.42f, 1f);

        public static HeroFormationPanelRefreshResult Refresh(HeroFormationPanelPresenterArgs args)
        {
            var result = new HeroFormationPanelRefreshResult();
            if (args == null)
            {
                return result;
            }

            bool formationOpen = args.ActivePageTab == HeroPageTab.Formation;
            result.TraitOpen = args.ActivePageTab == HeroPageTab.Trait;
            result.TotemOpen = args.ActivePageTab == HeroPageTab.Statue;
            result.RuneOpen = args.ActivePageTab == HeroPageTab.Seal;

            ApplyPageVisibility(args, formationOpen, result);
            ApplyPageButtons(args);
            ApplySummary(args);
            ApplyFormationSlots(args);
            ApplyOwnedEffect(args);
            ApplyTotemSlots(args);
            ApplyRuneSlots(args);

            return result;
        }

        private static void ApplyPageVisibility(
            HeroFormationPanelPresenterArgs args,
            bool formationOpen,
            HeroFormationPanelRefreshResult result)
        {
            string tabLabel = args.GetPageTabLabel != null
                ? args.GetPageTabLabel(args.ActivePageTab)
                : args.ActivePageTab.ToString();

            HeroFormationView.ApplyContentVisibility(
                args.FormationContent,
                args.TraitContent,
                args.TotemContent,
                args.RuneContent,
                args.PlaceholderText,
                formationOpen,
                result.TraitOpen,
                result.TotemOpen,
                result.RuneOpen,
                tabLabel + " 준비 중");
        }

        private static void ApplyPageButtons(HeroFormationPanelPresenterArgs args)
        {
            HeroFormationView.ApplySelectedButtonColors(
                args.PageTabButtons,
                args.ActivePageTab,
                SelectedPageTabColor,
                NormalPageTabColor);

            HeroFormationView.ApplySelectedButtonColors(
                args.PresetButtons,
                args.SelectedPreset,
                SelectedPresetColor,
                NormalPresetColor);
        }

        private static void ApplySummary(HeroFormationPanelPresenterArgs args)
        {
            if (args.SummaryText == null)
            {
                return;
            }

            HeroDefinition selectedHero = string.IsNullOrEmpty(args.SelectedHeroForPlacement)
                ? null
                : GameData.GetHero(args.SelectedHeroForPlacement);
            args.SummaryText.text = HeroFormationStateBuilder.BuildSummary(
                args.DeployedCount,
                GameData.MaxPartyHeroes,
                args.SelectedPreset,
                args.HasPendingChanges,
                selectedHero != null ? selectedHero.DisplayName : string.Empty);
        }

        private static void ApplyFormationSlots(HeroFormationPanelPresenterArgs args)
        {
            if (args.FormationSlotTexts == null)
            {
                return;
            }

            for (int i = 0; i < args.FormationSlotTexts.Count; i++)
            {
                string heroId = args.EditingFormationHeroIds != null && i < args.EditingFormationHeroIds.Count
                    ? args.EditingFormationHeroIds[i]
                    : string.Empty;
                HeroState hero = args.FindHeroState != null ? args.FindHeroState(heroId) : null;
                HeroFormationView.ApplyFormationSlotState(
                    i,
                    HeroFormationStateBuilder.BuildSlotState(
                        hero,
                        !string.IsNullOrEmpty(args.SelectedHeroForPlacement),
                        args.FormatShortNumber,
                        args.GetShortHeroLabel),
                    args.FormationSlotTexts,
                    args.FormationSlotButtons,
                    args.FormationSlotRemoveButtons);
            }
        }

        private static void ApplyOwnedEffect(HeroFormationPanelPresenterArgs args)
        {
            if (args.OwnedEffectText == null || args.BattleManager == null)
            {
                return;
            }

            args.OwnedEffectText.text = "보유 효과 : 공격력+"
                + args.BattleManager.HeroOwnedAttackBonusPercent.ToString("0.##") + "%";
        }

        private static void ApplyTotemSlots(HeroFormationPanelPresenterArgs args)
        {
            if (args.BattleManager == null)
            {
                return;
            }

            ApplyTotemSlot(args.TotemButton, args.TotemText, args.TotemRemoveButton, "토템\n전체 적용\n6종");
            ApplyTotemSlot(args.TotemSecondButton, args.TotemSecondText, args.TotemSecondRemoveButton, "2번째 슬롯\n사용 안함");
        }

        private static void ApplyTotemSlot(Button button, Text text, Button removeButton, string displayText)
        {
            HeroFormationView.ApplyTotemSlotState(
                button,
                text,
                removeButton,
                displayText,
                TotemDisabledColor,
                false,
                false);
        }

        private static void ApplyRuneSlots(HeroFormationPanelPresenterArgs args)
        {
            if (args.BattleManager == null)
            {
                return;
            }

            RuneState pendingState = args.BattleManager.GetRuneState(args.PendingRuneEquipId);
            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                bool unlocked = args.BattleManager.IsRuneSlotUnlocked(slot);
                RuneState state = unlocked
                    ? args.BattleManager.GetRuneState(args.BattleManager.GetEquippedRuneId(args.SelectedPreset, slot))
                    : null;
                HeroFormationView.ApplyRuneSlotState(
                    slot,
                    HeroFormationStateBuilder.BuildRuneSlotState(
                        slot,
                        unlocked,
                        args.BattleManager.GetRuneSlotUnlockLevel(slot),
                        state,
                        pendingState),
                    args.RuneSlotTexts,
                    args.RuneSlotButtons,
                    args.RuneSlotRemoveButtons);
            }
        }
    }
}
