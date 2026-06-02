using System;
using System.Collections.Generic;
using IdleGame.Data;

namespace IdleGame.Battle
{
    internal static class HeroAttackTickService
    {
        public static bool TickHeroAttacks(
            IReadOnlyList<HeroState> deployedHeroes,
            IList<HeroState> readyHeroAttacks,
            IList<string> recentHeroAttackIds,
            float deltaTime,
            Func<string, bool> isHeroAlive,
            Func<HeroState, bool> hasAttackTarget,
            Func<HeroState, float> attackInterval,
            Func<bool> shouldContinue,
            Action incrementAttackBatch,
            Action<HeroState> attackHero)
        {
            bool hasReadyHeroAttacks = CombatTickService.CollectReadyHeroAttacks(
                deployedHeroes,
                readyHeroAttacks,
                recentHeroAttackIds,
                deltaTime,
                isHeroAlive,
                hasAttackTarget,
                attackInterval,
                0.08f);
            if (!hasReadyHeroAttacks)
            {
                return false;
            }

            incrementAttackBatch?.Invoke();

            foreach (HeroState hero in readyHeroAttacks)
            {
                if (shouldContinue != null && !shouldContinue())
                {
                    return true;
                }

                attackHero?.Invoke(hero);

                if (shouldContinue != null && !shouldContinue())
                {
                    return true;
                }
            }

            return true;
        }
    }
}
