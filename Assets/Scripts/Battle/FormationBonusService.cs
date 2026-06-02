using System;
using System.Collections.Generic;
using IdleGame.Data;

namespace IdleGame.Battle
{
    internal static class FormationBonusService
    {
        public static double GetTotemAttackMultiplier(
            IReadOnlyList<TotemState> totems,
            IReadOnlyList<HeroState> deployedHeroes,
            bool isBossFight,
            HeroState hero)
        {
            double percent = 0d;
            if (totems == null)
            {
                return PercentToMultiplier(percent);
            }

            for (int i = 0; i < totems.Count; i++)
            {
                TotemState state = totems[i];
                if (state == null || !state.Unlocked)
                {
                    continue;
                }

                percent += state.Definition.GetAttackPercent(state.Level, state.Grade, deployedHeroes, isBossFight);
                if (hero != null)
                {
                    percent += state.Definition.GetTraitAttackPercent(state.Level, state.Grade, hero.Definition.Trait, deployedHeroes);
                }
            }

            return PercentToMultiplier(percent);
        }

        public static double GetTotemHpMultiplier(
            IReadOnlyList<TotemState> totems,
            IReadOnlyList<HeroState> deployedHeroes,
            HeroState hero)
        {
            double percent = 0d;
            if (totems == null)
            {
                return PercentToMultiplier(percent);
            }

            for (int i = 0; i < totems.Count; i++)
            {
                TotemState state = totems[i];
                if (state == null || !state.Unlocked)
                {
                    continue;
                }

                percent += state.Definition.GetHpPercent(state.Level, state.Grade, deployedHeroes);
                if (hero != null)
                {
                    percent += state.Definition.GetTraitHpPercent(state.Level, state.Grade, hero.Definition.Trait);
                }
            }

            return PercentToMultiplier(percent);
        }

        public static double GetTotemGoldMultiplier(IReadOnlyList<TotemState> totems)
        {
            return PercentToMultiplier(SumTotemPercent(totems, state => state.Definition.GetGoldGainPercent(state.Level, state.Grade)));
        }

        public static double GetTotemHeroExpMultiplier(IReadOnlyList<TotemState> totems)
        {
            return PercentToMultiplier(SumTotemPercent(totems, state => state.Definition.GetHeroExpGainPercent(state.Level, state.Grade)));
        }

        public static double GetTotemAccountExpMultiplier(IReadOnlyList<TotemState> totems)
        {
            return PercentToMultiplier(SumTotemPercent(totems, state => state.Definition.GetAccountExpGainPercent(state.Level, state.Grade)));
        }

        public static double GetTotemAttackSpeedMultiplier(IReadOnlyList<TotemState> totems, HeroState hero)
        {
            return PercentToMultiplier(SumTotemPercent(totems, state => state.Definition.GetAttackSpeedPercent(state.Level, state.Grade, hero)));
        }

        public static double GetTotemMoveSpeedMultiplier(IReadOnlyList<TotemState> totems)
        {
            return PercentToMultiplier(SumTotemPercent(totems, state => state.Definition.GetMoveSpeedPercent(state.Level, state.Grade)));
        }

        public static double GetTotemSkillDamageMultiplier(IReadOnlyList<TotemState> totems)
        {
            return PercentToMultiplier(SumTotemPercent(totems, state => state.Definition.GetSkillDamagePercent(state.Level, state.Grade)));
        }

        public static double GetTotemSkillCooldownMultiplier(IReadOnlyList<TotemState> totems)
        {
            double reduction = SumTotemPercent(totems, state => state.Definition.GetSkillCooldownReductionPercent(state.Level, state.Grade));
            return Math.Max(0.65d, 1d - Math.Min(35d, reduction) / 100d);
        }

        public static double GetTotemCriticalChanceBonus(IReadOnlyList<TotemState> totems)
        {
            return Math.Max(0d, SumTotemPercent(totems, state => state.Definition.GetCriticalChancePercent(state.Level, state.Grade)));
        }

        public static double GetTotemDamageTakenMultiplier(IReadOnlyList<TotemState> totems)
        {
            double reduction = Math.Min(90d, Math.Max(0d, SumTotemPercent(totems, state => state.Definition.GetDamageReductionPercent(state.Level, state.Grade))));
            return Math.Max(0.1d, 1d - reduction / 100d);
        }

        public static double GetRuneAttackMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetAttackPercent(state.Grade)));
        }

        public static double GetRuneFinalDamageMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetFinalDamagePercent(state.Grade)));
        }

        public static double GetRuneHpMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetHpPercent(state.Grade)));
        }

        public static double GetRuneGoldMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetGoldGainPercent(state.Grade)));
        }

        public static double GetRuneHeroExpMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetHeroExpGainPercent(state.Grade)));
        }

        public static double GetRuneAccountExpMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetAccountExpGainPercent(state.Grade)));
        }

        public static double GetRuneAttackSpeedMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetAttackSpeedPercent(state.Grade)));
        }

        public static double GetRuneMoveSpeedMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetMoveSpeedPercent(state.Grade)));
        }

        public static double GetRuneSkillDamageMultiplier(IReadOnlyList<RuneState> runes)
        {
            return PercentToMultiplier(SumRunePercent(runes, state => state.Definition.GetSkillDamagePercent(state.Grade)));
        }

        public static double GetRuneSkillCooldownMultiplier(IReadOnlyList<RuneState> runes)
        {
            double reduction = SumRunePercent(runes, state => state.Definition.GetSkillCooldownReductionPercent(state.Grade));
            return Math.Max(0.75d, 1d - Math.Min(25d, reduction) / 100d);
        }

        public static double GetRuneCriticalChanceBonus(IReadOnlyList<RuneState> runes)
        {
            return Math.Max(0d, SumRunePercent(runes, state => state.Definition.GetCriticalChancePercent(state.Grade)));
        }

        public static double GetRuneDamageTakenMultiplier(IReadOnlyList<RuneState> runes)
        {
            double reduction = Math.Min(80d, Math.Max(0d, SumRunePercent(runes, state => state.Definition.GetDamageReductionPercent(state.Grade))));
            return Math.Max(0.2d, 1d - reduction / 100d);
        }

        private static double SumTotemPercent(IReadOnlyList<TotemState> totems, Func<TotemState, double> resolvePercent)
        {
            double percent = 0d;
            if (totems == null)
            {
                return percent;
            }

            for (int i = 0; i < totems.Count; i++)
            {
                TotemState state = totems[i];
                if (state == null || !state.Unlocked)
                {
                    continue;
                }

                percent += resolvePercent(state);
            }

            return percent;
        }

        private static double SumRunePercent(IReadOnlyList<RuneState> runes, Func<RuneState, double> resolvePercent)
        {
            double percent = 0d;
            if (runes == null)
            {
                return percent;
            }

            for (int i = 0; i < runes.Count; i++)
            {
                RuneState state = runes[i];
                if (state != null && state.Unlocked)
                {
                    percent += resolvePercent(state);
                }
            }

            return percent;
        }

        private static double PercentToMultiplier(double percent)
        {
            return 1d + Math.Max(0d, percent) / 100d;
        }
    }
}
