using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Facility
{
    public static class FacilityPanelStateBuilder
    {
        private static readonly Color CollectReadyColor = new Color(0.45f, 0.62f, 0.22f, 1f);
        private static readonly Color CollectBlockedColor = new Color(0.30f, 0.32f, 0.34f, 1f);
        private static readonly Color UpgradeReadyColor = new Color(0.76f, 0.48f, 0.16f, 1f);
        private static readonly Color UpgradeBlockedColor = new Color(0.36f, 0.32f, 0.28f, 1f);
        private static readonly Color UpgradeMaxColor = new Color(0.34f, 0.35f, 0.36f, 1f);
        private static readonly Color LockedSlotTextColor = new Color(0.74f, 0.78f, 0.84f, 1f);
        private static readonly Color LockedSlotCardColor = new Color(0.13f, 0.15f, 0.20f, 1f);
        private static readonly Color EmptySlotTextColor = new Color(0.68f, 0.74f, 0.82f, 1f);
        private static readonly Color EmptySlotCardColor = new Color(0.16f, 0.20f, 0.30f, 1f);

        public static FacilityCardViewState BuildCardState(
            FacilityState state,
            GameNumber productionPerHour,
            GameNumber maxStored,
            double heroBonusPercent,
            CurrencyWallet wallet,
            Func<GameNumber, string> formatShort)
        {
            if (state == null)
            {
                return null;
            }

            GameNumber storedFloor = GameNumber.Floor(state.StoredAmount);
            double storedHours = maxStored > GameNumber.Zero
                ? Mathf.Clamp01((float)state.StoredAmount.RatioTo(maxStored)) * 12d
                : 0d;
            bool claimable = storedFloor > GameNumber.Zero;
            bool canUpgrade = !state.IsMaxed;
            bool canAfford = canUpgrade && state.UpgradeCost.CanAfford(wallet);

            return new FacilityCardViewState
            {
                Text = FacilityUiText.BuildCardText(
                    state,
                    productionPerHour,
                    maxStored,
                    storedFloor,
                    storedHours,
                    heroBonusPercent,
                    claimable,
                    formatShort),
                CollectInteractable = claimable,
                CollectText = FacilityUiText.BuildCollectButtonText(claimable, storedFloor, formatShort),
                CollectColor = claimable ? CollectReadyColor : CollectBlockedColor,
                UpgradeInteractable = canUpgrade,
                UpgradeText = FacilityUiText.BuildUpgradeButtonText(state),
                UpgradeColor = state.IsMaxed ? UpgradeMaxColor : canAfford ? UpgradeReadyColor : UpgradeBlockedColor
            };
        }

        public static List<string> BuildRewardPopupLines(
            IReadOnlyList<FacilityState> facilities,
            Func<GameNumber, string> formatShort)
        {
            var rewardLines = new List<string>();
            if (facilities == null)
            {
                return rewardLines;
            }

            foreach (FacilityState state in facilities)
            {
                if (state == null)
                {
                    continue;
                }

                GameNumber amount = GameNumber.Floor(state.StoredAmount);
                if (amount <= GameNumber.Zero)
                {
                    continue;
                }

                rewardLines.Add(FacilityUiText.BuildRewardPopupLine(state.Definition, amount, formatShort));
            }

            return rewardLines;
        }

        public static FacilityAssignmentSlotViewState BuildAssignmentSlotState(
            FacilityState state,
            int slot,
            HeroState hero,
            Func<HeroRarity, string> getRarityBadge,
            Func<HeroDefinition, string> getShortHeroLabel)
        {
            if (state == null)
            {
                return null;
            }

            if (slot >= state.UnlockedSlotCount)
            {
                return new FacilityAssignmentSlotViewState
                {
                    Text = FacilityUiText.BuildLockedSlotText(slot),
                    TextColor = LockedSlotTextColor,
                    CardColor = LockedSlotCardColor
                };
            }

            if (hero != null && hero.IsOwned)
            {
                return new FacilityAssignmentSlotViewState
                {
                    Text = FacilityUiText.BuildAssignedHeroSlotText(hero, getRarityBadge, getShortHeroLabel),
                    TextColor = Color.white,
                    CardColor = HeroUiText.GetRarityColor(hero.Definition.Rarity)
                };
            }

            return new FacilityAssignmentSlotViewState
            {
                Text = "빈칸",
                TextColor = EmptySlotTextColor,
                CardColor = EmptySlotCardColor
            };
        }
    }
}
