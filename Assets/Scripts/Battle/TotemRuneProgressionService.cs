using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class TotemRuneProgressionService
    {
        public static bool TryLevelUpTotem(TotemState state, CurrencyWallet wallet, out string battleLog)
        {
            battleLog = string.Empty;
            if (state == null)
            {
                return false;
            }

            if (!state.Unlocked)
            {
                battleLog = state.DisplayName + " 강화 실패: 보유하지 않은 토템";
                return false;
            }

            if (state.IsMaxed)
            {
                battleLog = state.DisplayName + " MAX";
                return false;
            }

            if (wallet == null || !wallet.SpendTotemEssence(state.LevelUpCost))
            {
                battleLog = state.DisplayName + " 강화 실패: 토템 정수 부족";
                return false;
            }

            state.Level += 1;
            state.Unlocked = true;
            battleLog = state.DisplayName + " Lv." + state.Level;
            return true;
        }

        public static bool CanPromoteTotemTier(TotemState state, IReadOnlyList<TotemState> totems)
        {
            return state != null
                && state.Unlocked
                && state.CanPromote
                && AreTotemsReadyToAdvanceGrade(totems, state.Grade);
        }

        public static bool TryPromoteTotem(
            TotemState state,
            IReadOnlyList<TotemState> totems,
            CurrencyWallet wallet,
            List<TotemState> changedTotems,
            out string battleLog)
        {
            battleLog = string.Empty;
            changedTotems?.Clear();
            if (state == null || !state.Unlocked)
            {
                battleLog = "토템 진화 실패: 보유하지 않은 토템";
                return false;
            }

            if (!state.CanPromote)
            {
                battleLog = state.DisplayName + " 진화 불가";
                return false;
            }

            if (!AreTotemsReadyToAdvanceGrade(totems, state.Grade))
            {
                battleLog = "토템 진화 실패: 같은 등급 토템을 모두 Lv." + TotemDefinition.MaxLevel + "까지 강화해야 함";
                return false;
            }

            TotemGrade currentGrade = state.Grade;
            if (wallet == null || !wallet.SpendTotemEssence(state.PromoteCost))
            {
                battleLog = state.DisplayName + " 진화 실패: 토템 정수 부족";
                return false;
            }

            TotemGrade nextGrade = TotemDefinition.GetNextGrade(currentGrade);
            foreach (TotemState totemState in totems)
            {
                if (totemState == null || totemState.Grade != currentGrade)
                {
                    continue;
                }

                totemState.Grade = nextGrade;
                totemState.Level = 1;
                totemState.Unlocked = true;
                changedTotems?.Add(totemState);
            }

            battleLog = TotemDefinition.GetGradeLabel(currentGrade) + " 토템 전체 진화 완료";
            return true;
        }

        public static int DebugUnlockAllTotems(IReadOnlyList<TotemState> totems, List<TotemState> changedTotems, out string battleLog)
        {
            battleLog = "QA: 모든 기본 토템 보유";
            changedTotems?.Clear();
            if (totems == null)
            {
                return 0;
            }

            foreach (TotemState state in totems)
            {
                if (state == null)
                {
                    continue;
                }

                state.Unlocked = true;
                changedTotems?.Add(state);
            }

            return changedTotems != null ? changedTotems.Count : totems.Count;
        }

        public static bool TryPromoteRune(RuneState state, out bool highestGradeChanged, out string battleLog)
        {
            highestGradeChanged = false;
            battleLog = string.Empty;
            if (state == null || !state.Unlocked)
            {
                battleLog = "룬 승급 실패: 보유하지 않은 룬";
                return false;
            }

            if (state.IsMaxGrade)
            {
                battleLog = state.Definition.DisplayName + " MAX";
                return false;
            }

            if (!state.TryFindSynthesizableGrade(out _))
            {
                battleLog = state.Definition.DisplayName + " 합성 실패: 같은 등급 룬 부족";
                return false;
            }

            if (!state.TrySynthesizeOnce(out RuneGrade fromGrade, out RuneGrade toGrade, out highestGradeChanged))
            {
                battleLog = state.Definition.DisplayName + " 합성 실패";
                return false;
            }

            battleLog = state.Definition.DisplayName
                + " " + RuneDefinition.GetGradeLabel(fromGrade)
                + " -> " + RuneDefinition.GetGradeLabel(toGrade)
                + " 합성";
            return true;
        }

        public static int PromoteAllRunes(
            IReadOnlyList<RuneState> runes,
            List<RuneState> changedRunes,
            HashSet<string> highestGradeChangedRuneIds,
            out string battleLog)
        {
            battleLog = string.Empty;
            changedRunes?.Clear();
            highestGradeChangedRuneIds?.Clear();
            if (runes == null)
            {
                battleLog = "합성 가능한 룬이 없습니다.";
                return 0;
            }

            int promotedCount = 0;
            foreach (RuneState state in runes)
            {
                if (state == null)
                {
                    continue;
                }

                bool changed = false;
                while (state.CanPromote)
                {
                    if (!state.TrySynthesizeOnce(out _, out _, out bool highestGradeChanged))
                    {
                        break;
                    }

                    promotedCount += 1;
                    changed = true;
                    if (highestGradeChanged)
                    {
                        highestGradeChangedRuneIds?.Add(state.Definition.Id);
                    }
                }

                if (changed)
                {
                    changedRunes?.Add(state);
                }
            }

            battleLog = promotedCount > 0
                ? "룬 일괄 합성 " + promotedCount + "회"
                : "합성 가능한 룬이 없습니다.";
            return promotedCount;
        }

        public static int DebugUnlockAllRunes(IReadOnlyList<RuneState> runes, List<RuneState> changedRunes, out string battleLog)
        {
            battleLog = "QA: 모든 기본 룬 보유";
            changedRunes?.Clear();
            if (runes == null)
            {
                return 0;
            }

            foreach (RuneState state in runes)
            {
                if (state == null)
                {
                    continue;
                }

                state.Unlocked = true;
                changedRunes?.Add(state);
            }

            return changedRunes != null ? changedRunes.Count : runes.Count;
        }

        public static int DebugAddRuneItems(
            IReadOnlyList<RuneState> runes,
            int commonRunesPerRune,
            List<RuneState> changedRunes,
            out string battleLog)
        {
            battleLog = string.Empty;
            changedRunes?.Clear();
            int amount = Mathf.Max(0, commonRunesPerRune);
            if (runes == null || amount <= 0)
            {
                return 0;
            }

            foreach (RuneState state in runes)
            {
                if (state == null)
                {
                    continue;
                }

                state.AddCount(RuneGrade.Common, amount);
                changedRunes?.Add(state);
            }

            battleLog = "QA: 모든 룬 커먼 +" + amount;
            return changedRunes != null ? changedRunes.Count : runes.Count;
        }

        private static bool AreTotemsReadyToAdvanceGrade(IReadOnlyList<TotemState> totems, TotemGrade grade)
        {
            if (totems == null)
            {
                return false;
            }

            bool hasCurrentGrade = false;
            foreach (TotemState state in totems)
            {
                if (state == null || !state.Unlocked)
                {
                    return false;
                }

                if (state.Grade < grade)
                {
                    return false;
                }

                if (state.Grade == grade)
                {
                    hasCurrentGrade = true;
                    if (!state.IsMaxed)
                    {
                        return false;
                    }
                }
            }

            return hasCurrentGrade;
        }
    }
}
