using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;

namespace IdleGame.UI.Growth
{
    public sealed class GrowthActionResult
    {
        public bool Success;
        public string Message;
    }

    public static class GrowthActionService
    {
        public static GrowthActionResult TryLevelUpAbility(
            AbilityManager abilityManager,
            CurrencyWallet wallet,
            AbilityKind kind,
            int requestedLevels)
        {
            AbilityState ability = FindAbilityState(abilityManager, kind);
            if (ability == null)
            {
                return new GrowthActionResult();
            }

            if (ability.IsMaxed)
            {
                return new GrowthActionResult
                {
                    Message = "이미 최대 레벨입니다."
                };
            }

            int cappedLevels = abilityManager.GetCappedLevelCount(ability, requestedLevels);
            long cost = abilityManager.GetLevelUpCost(ability, cappedLevels);
            if (cappedLevels <= 0 || cost <= 0 || wallet == null || wallet.Gold < cost)
            {
                return new GrowthActionResult
                {
                    Message = "골드가 부족합니다."
                };
            }

            bool leveled = abilityManager.TryLevelUp(kind, requestedLevels);
            return new GrowthActionResult
            {
                Success = leveled,
                Message = leveled ? string.Empty : "골드가 부족합니다."
            };
        }

        public static bool CanLevelUpAbility(
            AbilityManager abilityManager,
            CurrencyWallet wallet,
            AbilityKind kind,
            int requestedLevels)
        {
            AbilityState ability = FindAbilityState(abilityManager, kind);
            if (ability == null || ability.IsMaxed || wallet == null)
            {
                return false;
            }

            int cappedLevels = abilityManager.GetCappedLevelCount(ability, requestedLevels);
            long cost = abilityManager.GetLevelUpCost(ability, cappedLevels);
            return cappedLevels > 0 && cost > 0 && wallet.Gold >= cost;
        }

        public static GrowthActionResult TryLevelUpTalent(AccountProgressManager accountProgressManager, string talentId)
        {
            if (accountProgressManager == null)
            {
                return new GrowthActionResult();
            }

            TalentDefinition talent = TalentData.GetTalent(talentId);
            if (talent == null)
            {
                return new GrowthActionResult();
            }

            if (!accountProgressManager.IsTalentUnlocked(talent))
            {
                return new GrowthActionResult
                {
                    Message = "선으로 연결된 이전 특성을 MAX 찍어야 합니다."
                };
            }

            if (accountProgressManager.GetTalentLevel(talent.Id) >= talent.MaxLevel)
            {
                return new GrowthActionResult
                {
                    Message = "이미 최대 레벨입니다."
                };
            }

            if (accountProgressManager.AvailableTalentPoints < talent.CostPerLevel)
            {
                return new GrowthActionResult
                {
                    Message = "특성 포인트가 부족합니다."
                };
            }

            if (!accountProgressManager.TryLevelUpTalent(talent.Id))
            {
                return new GrowthActionResult
                {
                    Message = "특성 레벨업에 실패했습니다."
                };
            }

            return new GrowthActionResult
            {
                Success = true,
                Message = talent.DisplayName + " Lv." + accountProgressManager.GetTalentLevel(talent.Id)
            };
        }

        public static bool CanLevelUpTalent(AccountProgressManager accountProgressManager, string talentId)
        {
            if (accountProgressManager == null)
            {
                return false;
            }

            TalentDefinition talent = TalentData.GetTalent(talentId);
            if (talent == null)
            {
                return false;
            }

            int level = accountProgressManager.GetTalentLevel(talent.Id);
            return accountProgressManager.IsTalentUnlocked(talent)
                && level < talent.MaxLevel
                && accountProgressManager.AvailableTalentPoints >= talent.CostPerLevel;
        }

        public static GrowthActionResult TryLevelUpFortress(BattleManager battleManager)
        {
            if (battleManager == null)
            {
                return new GrowthActionResult();
            }

            if (!battleManager.TryLevelUpFortress())
            {
                return new GrowthActionResult
                {
                    Message = battleManager.FortressLevel >= battleManager.FortressMaxLevel
                        ? "요새는 이미 최대 레벨입니다."
                        : "요새 경험치가 부족합니다."
                };
            }

            return new GrowthActionResult
            {
                Success = true,
                Message = "요새 Lv." + battleManager.FortressLevel
            };
        }

        public static bool CanLevelUpFortress(BattleManager battleManager)
        {
            return battleManager != null && battleManager.CanLevelUpFortress;
        }

        private static AbilityState FindAbilityState(AbilityManager abilityManager, AbilityKind kind)
        {
            if (abilityManager == null)
            {
                return null;
            }

            foreach (AbilityState ability in abilityManager.States)
            {
                if (ability.Definition.Kind == kind)
                {
                    return ability;
                }
            }

            return null;
        }
    }
}
