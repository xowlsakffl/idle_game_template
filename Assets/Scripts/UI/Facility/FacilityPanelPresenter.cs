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
        public Dictionary<string, FacilityCardViewState> CachedCardStates;
        public Dictionary<string, string> CachedAssignmentRowTexts;
        public Dictionary<string, FacilityAssignmentSlotViewState> CachedAssignmentSlotStates;
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
                FacilityCardViewState cardState = FacilityPanelStateBuilder.BuildCardState(
                    state,
                    args.BattleManager.GetFacilityProductionPerHour(definition.Id),
                    args.BattleManager.GetFacilityMaxStoredAmount(definition.Id),
                    args.BattleManager.GetFacilityHeroBonusPercent(definition.Id),
                    args.Wallet,
                    args.FormatShortNumber);
                if (IsCachedCardCurrent(args.CachedCardStates, definition.Id, cardState))
                {
                    continue;
                }

                FacilityView.ApplyFacilityCardState(
                    definition.Id,
                    cardState,
                    args.FacilityCardTexts,
                    args.FacilityCollectButtons,
                    args.FacilityUpgradeButtons);
                CacheCardState(args.CachedCardStates, definition.Id, cardState);
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
                string rowText = FacilityUiText.BuildAssignmentRowText(state);
                if (!IsCachedAssignmentRowCurrent(args.CachedAssignmentRowTexts, definition.Id, rowText))
                {
                    FacilityView.ApplyAssignmentRowState(
                        definition.Id,
                        rowText,
                        args.AssignmentRowTexts);
                    CacheAssignmentRowText(args.CachedAssignmentRowTexts, definition.Id, rowText);
                }

                RefreshAssignmentSlotCards(args, state);
            }
        }

        private static void RefreshAssignmentSlotCards(FacilityPanelPresenterArgs args, FacilityState state)
        {
            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                string heroId = state.GetAssignedHeroId(slot);
                HeroState hero = args.FindHeroState != null ? args.FindHeroState(heroId) : null;
                FacilityAssignmentSlotViewState slotState = FacilityPanelStateBuilder.BuildAssignmentSlotState(
                    state,
                    slot,
                    hero,
                    HeroUiText.GetRarityBadge,
                    args.GetShortHeroLabel);
                string cacheKey = BuildAssignmentSlotCacheKey(state.Definition.Id, slot);
                if (IsCachedAssignmentSlotCurrent(args.CachedAssignmentSlotStates, cacheKey, slotState))
                {
                    continue;
                }

                FacilityView.ApplyAssignmentSlotState(
                    state.Definition.Id,
                    slot,
                    slotState,
                    args.AssignmentSlotTexts);
                CacheAssignmentSlotState(args.CachedAssignmentSlotStates, cacheKey, slotState);
            }
        }

        private static bool IsCachedCardCurrent(
            Dictionary<string, FacilityCardViewState> cachedStates,
            string facilityId,
            FacilityCardViewState state)
        {
            return cachedStates != null
                && cachedStates.TryGetValue(facilityId, out FacilityCardViewState cachedState)
                && state != null
                && state.IsSameAs(cachedState);
        }

        private static void CacheCardState(
            Dictionary<string, FacilityCardViewState> cachedStates,
            string facilityId,
            FacilityCardViewState state)
        {
            if (cachedStates != null && !string.IsNullOrEmpty(facilityId) && state != null)
            {
                cachedStates[facilityId] = state;
            }
        }

        private static bool IsCachedAssignmentRowCurrent(Dictionary<string, string> cachedTexts, string facilityId, string text)
        {
            return cachedTexts != null
                && cachedTexts.TryGetValue(facilityId, out string cachedText)
                && cachedText == text;
        }

        private static void CacheAssignmentRowText(Dictionary<string, string> cachedTexts, string facilityId, string text)
        {
            if (cachedTexts != null && !string.IsNullOrEmpty(facilityId))
            {
                cachedTexts[facilityId] = text ?? string.Empty;
            }
        }

        private static bool IsCachedAssignmentSlotCurrent(
            Dictionary<string, FacilityAssignmentSlotViewState> cachedStates,
            string cacheKey,
            FacilityAssignmentSlotViewState state)
        {
            return cachedStates != null
                && cachedStates.TryGetValue(cacheKey, out FacilityAssignmentSlotViewState cachedState)
                && state != null
                && state.IsSameAs(cachedState);
        }

        private static void CacheAssignmentSlotState(
            Dictionary<string, FacilityAssignmentSlotViewState> cachedStates,
            string cacheKey,
            FacilityAssignmentSlotViewState state)
        {
            if (cachedStates != null && !string.IsNullOrEmpty(cacheKey) && state != null)
            {
                cachedStates[cacheKey] = state;
            }
        }

        private static string BuildAssignmentSlotCacheKey(string facilityId, int slot)
        {
            return facilityId + ":" + slot;
        }
    }
}
