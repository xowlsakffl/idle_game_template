using System;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.UI.Facility
{
    public static class FacilityUiText
    {
        public static string BuildSummary(CurrencyWallet wallet, Func<long, string> formatCount)
        {
            if (wallet == null)
            {
                return "시설 파견";
            }

            return "시설 파견  목재 " + FormatCount(wallet.Wood, formatCount)
                + " / 벽돌 " + FormatCount(wallet.Brick, formatCount)
                + " / 철재 " + FormatCount(wallet.Iron, formatCount);
        }

        public static string BuildCardText(
            FacilityState state,
            GameNumber productionPerHour,
            GameNumber maxStored,
            GameNumber storedAmount,
            double storedHours,
            double heroBonusPercent,
            bool claimable,
            Func<GameNumber, string> formatShort)
        {
            if (state == null)
            {
                return string.Empty;
            }

            FacilityDefinition definition = state.Definition;
            return GetIcon(definition) + " " + GetDisplayName(definition)
                + "  Lv." + state.Level
                + "\n생산: " + GetRewardLabel(definition) + " " + FormatShort(productionPerHour, formatShort) + "/시간"
                + "\n누적: " + FormatShort(storedAmount, formatShort) + " / " + FormatShort(maxStored, formatShort)
                + "  (" + storedHours.ToString("0.#") + "시간 / 12시간)"
                + "\n배치 인원: " + state.AssignedCount + "/" + state.UnlockedSlotCount
                + "  보너스 +" + heroBonusPercent.ToString("0.#") + "%"
                + (claimable ? "  수령 가능" : "  생산 중");
        }

        public static string BuildCollectButtonText(bool claimable, GameNumber amount, Func<GameNumber, string> formatShort)
        {
            return claimable ? "수령\n" + FormatShort(amount, formatShort) : "생산 중";
        }

        public static string BuildUpgradeButtonText(FacilityState state)
        {
            if (state == null || state.IsMaxed)
            {
                return "MAX";
            }

            return "업그레이드\n" + state.UpgradeCost.Format();
        }

        public static string BuildRewardPopupLine(FacilityDefinition definition, GameNumber amount, Func<GameNumber, string> formatShort)
        {
            return GetRewardLabel(definition) + "  +" + FormatShort(amount, formatShort);
        }

        public static string BuildAssignmentRowText(FacilityState state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            return GetDisplayName(state.Definition)
                + "  Lv." + state.Level
                + "\n" + state.AssignedCount + "/" + state.UnlockedSlotCount;
        }

        public static string BuildLockedSlotText(int slotIndex)
        {
            return "잠김\nLv." + GetSlotUnlockLevel(slotIndex);
        }

        public static string BuildAssignedHeroSlotText(
            HeroState hero,
            Func<HeroRarity, string> getRarityBadge,
            Func<HeroDefinition, string> getShortHeroLabel)
        {
            if (hero == null || !hero.IsOwned)
            {
                return "빈칸";
            }

            string badge = getRarityBadge != null ? getRarityBadge(hero.Definition.Rarity) : hero.Definition.RarityLabel;
            string heroLabel = getShortHeroLabel != null ? getShortHeroLabel(hero.Definition) : hero.Definition.DisplayName;
            return badge
                + "\n" + heroLabel
                + "\nLv." + hero.Level;
        }

        public static int GetSlotUnlockLevel(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0:
                    return 1;
                case 1:
                    return 5;
                case 2:
                    return 10;
                case 3:
                    return 15;
                default:
                    return 20;
            }
        }

        public static string GetDisplayName(FacilityDefinition facility)
        {
            if (facility == null)
            {
                return string.Empty;
            }

            switch (facility.Id)
            {
                case "FAC_REQUEST":
                    return "의뢰소";
                case "FAC_TRAINING":
                    return "훈련소";
                case "FAC_FORGE":
                    return "대장간";
                case "FAC_TOTEM":
                    return "토템 제단";
                case "FAC_RUNE":
                    return "룬 공방";
                case "FAC_TRANSCEND":
                    return "초월 연구소";
                default:
                    return facility.DisplayName;
            }
        }

        public static string GetRewardLabel(FacilityDefinition facility)
        {
            if (facility == null)
            {
                return string.Empty;
            }

            switch (facility.RewardKind)
            {
                case FacilityRewardKind.Gold:
                    return "골드";
                case FacilityRewardKind.HeroExpItem:
                    return "영웅 경험치책";
                case FacilityRewardKind.EquipmentExpItem:
                    return "장비책";
                case FacilityRewardKind.TotemEssence:
                    return "토템 정수";
                case FacilityRewardKind.RuneBox:
                    return "룬 상자";
                case FacilityRewardKind.HeroTranscendStone:
                    return "초월석";
                default:
                    return facility.RewardLabel;
            }
        }

        public static string GetIcon(FacilityDefinition facility)
        {
            if (facility == null)
            {
                return "?";
            }

            switch (facility.RewardKind)
            {
                case FacilityRewardKind.Gold:
                    return "G";
                case FacilityRewardKind.HeroExpItem:
                    return "EXP";
                case FacilityRewardKind.EquipmentExpItem:
                    return "EQ";
                case FacilityRewardKind.TotemEssence:
                    return "T";
                case FacilityRewardKind.RuneBox:
                    return "R";
                case FacilityRewardKind.HeroTranscendStone:
                    return "TR";
                default:
                    return facility.Icon;
            }
        }

        private static string FormatShort(GameNumber amount, Func<GameNumber, string> formatShort)
        {
            return formatShort != null ? formatShort(amount) : amount.ToString();
        }

        private static string FormatCount(long amount, Func<long, string> formatCount)
        {
            return formatCount != null ? formatCount(amount) : amount.ToString();
        }
    }
}
