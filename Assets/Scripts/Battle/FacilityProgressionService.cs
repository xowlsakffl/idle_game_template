using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static partial class FacilityProgressionService
    {
        internal readonly struct FacilityCollectResult
        {
            public FacilityCollectResult(int collectedCount, string rewardLog, string battleLog)
            {
                CollectedCount = collectedCount;
                RewardLog = rewardLog;
                BattleLog = battleLog;
            }

            public int CollectedCount { get; }
            public string RewardLog { get; }
            public string BattleLog { get; }
        }

        public static bool TryUpgradeFacility(FacilityState state, CurrencyWallet wallet, out string battleLog)
        {
            battleLog = string.Empty;
            if (state == null)
            {
                return false;
            }

            if (state.IsMaxed)
            {
                battleLog = state.Definition.DisplayName + " MAX";
                return false;
            }

            if (wallet == null || !wallet.SpendFacilityMaterials(state.UpgradeCost))
            {
                battleLog = state.Definition.DisplayName + " 업그레이드 실패: 자재 부족";
                return false;
            }

            state.Level += 1;
            battleLog = state.Definition.DisplayName + " Lv." + state.Level;
            return true;
        }

        public static string GrantFacilityReward(
            FacilityState state,
            GameNumber amount,
            CurrencyWallet wallet,
            IReadOnlyList<RuneState> runes,
            Func<int, int> randomIndex,
            Action<RuneState> saveRuneState)
        {
            if (state == null || amount <= GameNumber.Zero || wallet == null)
            {
                return string.Empty;
            }

            GameNumber reward = GameNumber.Floor(amount);
            switch (state.Definition.RewardKind)
            {
                case FacilityRewardKind.Gold:
                    wallet.AddGold(reward);
                    return state.Definition.DisplayName + " +" + NumberFormatter.Format(reward) + " 골드";
                case FacilityRewardKind.HeroExpItem:
                    wallet.AddHeroExpItem(reward);
                    return state.Definition.DisplayName + " +" + NumberFormatter.Format(reward) + " 영웅 경험치책";
                case FacilityRewardKind.EquipmentExpItem:
                    wallet.AddEquipmentExpItem(reward);
                    return state.Definition.DisplayName + " +" + NumberFormatter.Format(reward) + " 장비책";
                case FacilityRewardKind.TotemEssence:
                    {
                        long count = GameNumberToLong(reward);
                        wallet.AddTotemEssence(count);
                        return state.Definition.DisplayName + " +" + count + " 토템 정수";
                    }
                case FacilityRewardKind.RuneBox:
                    {
                        long boxes = GameNumberToLong(reward);
                        GrantRunesFromBoxes(boxes, runes, randomIndex, saveRuneState);
                        return state.Definition.DisplayName + " +" + boxes + " 룬 상자";
                    }
                case FacilityRewardKind.HeroTranscendStone:
                    {
                        long count = GameNumberToLong(reward);
                        wallet.AddHeroTranscendStone(count);
                        return state.Definition.DisplayName + " +" + count + " 초월석";
                    }
                default:
                    return string.Empty;
            }
        }

        public static bool TryCollectFacility(
            FacilityState state,
            Func<FacilityState, GameNumber, string> grantReward,
            long nowTicks,
            out string rewardLog,
            out string battleLog)
        {
            rewardLog = string.Empty;
            battleLog = string.Empty;
            if (state == null)
            {
                return false;
            }

            if (GameNumber.Floor(state.StoredAmount) <= GameNumber.Zero)
            {
                battleLog = state.Definition.DisplayName + " 수령할 보상 없음";
                return false;
            }

            rewardLog = grantReward != null ? grantReward(state, state.StoredAmount) : string.Empty;
            state.StoredAmount = GameNumber.Zero;
            state.LastUpdateUtcTicks = nowTicks;
            battleLog = state.Definition.DisplayName + " 보상 수령";
            return true;
        }

        public static FacilityCollectResult CollectAllFacilities(
            IReadOnlyList<FacilityState> facilities,
            Action<FacilityState> refreshProduction,
            Func<FacilityState, GameNumber, string> grantReward,
            List<FacilityState> changedFacilities,
            long nowTicks)
        {
            changedFacilities?.Clear();
            var rewardParts = new List<string>();
            int collected = 0;
            if (facilities != null)
            {
                foreach (FacilityState state in facilities)
                {
                    refreshProduction?.Invoke(state);
                    if (state == null || GameNumber.Floor(state.StoredAmount) <= GameNumber.Zero)
                    {
                        continue;
                    }

                    string rewardText = grantReward != null ? grantReward(state, state.StoredAmount) : string.Empty;
                    if (!string.IsNullOrEmpty(rewardText))
                    {
                        rewardParts.Add(rewardText);
                    }

                    state.StoredAmount = GameNumber.Zero;
                    state.LastUpdateUtcTicks = nowTicks;
                    changedFacilities?.Add(state);
                    collected += 1;
                }
            }

            return new FacilityCollectResult(
                collected,
                rewardParts.Count > 0 ? string.Join(" / ", rewardParts) : "시설 보상 없음",
                collected > 0 ? "시설 보상 모두 획득" : "수령할 시설 보상 없음");
        }
    }
}
