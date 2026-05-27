using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Facility
{
    public sealed class FacilityPanelPresenterArgs
    {
        public BattleManager BattleManager;
        public CurrencyWallet Wallet;
        public bool AssignmentModalOpen;
        public Text SummaryText;
        public GameObject AssignmentModal;
        public Dictionary<string, Text> FacilityCardTexts;
        public Dictionary<string, Button> FacilityUpgradeButtons;
        public Dictionary<string, Button> FacilityCollectButtons;
        public Dictionary<string, Text> AssignmentRowTexts;
        public Dictionary<string, List<Text>> AssignmentSlotTexts;
        public Func<GameNumber, string> FormatShortNumber;
        public Func<long, string> FormatCountNumber;
        public Func<string, HeroState> FindHeroState;
        public Func<HeroDefinition, string> GetShortHeroLabel;
    }

    public static class FacilityPanelPresenter
    {
        public static void Refresh(FacilityPanelPresenterArgs args)
        {
            if (args == null || args.BattleManager == null || args.Wallet == null)
            {
                return;
            }

            if (args.SummaryText != null)
            {
                args.SummaryText.text = FacilityUiText.BuildSummary(args.Wallet, args.FormatCountNumber);
            }

            RefreshFacilityCards(args);
            RefreshAssignmentModal(args);
        }

        private static void RefreshFacilityCards(FacilityPanelPresenterArgs args)
        {
            foreach (FacilityState state in args.BattleManager.Facilities)
            {
                if (state == null)
                {
                    continue;
                }

                FacilityDefinition definition = state.Definition;
                FacilityView.ApplyFacilityCardState(
                    definition.Id,
                    FacilityPanelStateBuilder.BuildCardState(
                        state,
                        args.BattleManager.GetFacilityProductionPerHour(definition.Id),
                        args.BattleManager.GetFacilityMaxStoredAmount(definition.Id),
                        args.BattleManager.GetFacilityHeroBonusPercent(definition.Id),
                        args.Wallet,
                        args.FormatShortNumber),
                    args.FacilityCardTexts,
                    args.FacilityCollectButtons,
                    args.FacilityUpgradeButtons);
            }
        }

        private static void RefreshAssignmentModal(FacilityPanelPresenterArgs args)
        {
            FacilityView.SetAssignmentModalOpen(args.AssignmentModal, args.AssignmentModalOpen);
            if (!args.AssignmentModalOpen)
            {
                return;
            }

            foreach (FacilityState state in args.BattleManager.Facilities)
            {
                if (state == null)
                {
                    continue;
                }

                FacilityDefinition definition = state.Definition;
                FacilityView.ApplyAssignmentRowState(
                    definition.Id,
                    FacilityUiText.BuildAssignmentRowText(state),
                    args.AssignmentRowTexts);

                RefreshAssignmentSlotCards(args, state);
            }
        }

        private static void RefreshAssignmentSlotCards(FacilityPanelPresenterArgs args, FacilityState state)
        {
            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                string heroId = state.GetAssignedHeroId(slot);
                HeroState hero = args.FindHeroState != null ? args.FindHeroState(heroId) : null;
                FacilityView.ApplyAssignmentSlotState(
                    state.Definition.Id,
                    slot,
                    FacilityPanelStateBuilder.BuildAssignmentSlotState(
                        state,
                        slot,
                        hero,
                        HeroUiText.GetRarityBadge,
                        args.GetShortHeroLabel),
                    args.AssignmentSlotTexts);
            }
        }
    }
}
