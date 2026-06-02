using System;
using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class CombatMovementService
    {
        private static void TickHeroMovement(
            HeroState hero,
            BattleHeroRuntimeState heroState,
            int heroIndex,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            IDictionary<string, int> heroTargetSpawnSequences,
            float deltaTime,
            Func<HeroState, int, Vector2> getSlotPosition,
            Func<HeroState, float> getMoveSpeed,
            float fieldHalfWidth,
            float fieldHalfHeight,
            float reviveAttackCooldown)
        {
            heroState.SlotIndex = heroIndex;
            Vector2 slotPosition = getSlotPosition?.Invoke(hero, heroIndex) ?? GetHeroBattleSlotPosition(hero, heroIndex);
            float moveSpeed = getMoveSpeed?.Invoke(hero) ?? hero.MoveSpeed;
            if (!heroState.IsAlive)
            {
                TickHeroRevive(hero, heroState, slotPosition, deltaTime, reviveAttackCooldown);
                return;
            }

            int targetIndex = GetOrAssignHeroMovementTarget(
                hero,
                heroState,
                visibleEnemies,
                heroTargetSpawnSequences);
            if (targetIndex < 0 || IsFortressProtectedHero(hero))
            {
                heroState.Position = Vector2.MoveTowards(
                    heroState.Position,
                    slotPosition,
                    moveSpeed * deltaTime);
                return;
            }

            VisibleEnemyState enemy = visibleEnemies[targetIndex];
            float attackRange = GetHeroAttackRange(hero);
            heroState.Position = MoveTowardCombatRange(
                heroState.Position,
                enemy.Position,
                attackRange * 0.74f,
                moveSpeed,
                deltaTime,
                fieldHalfWidth,
                fieldHalfHeight);
        }

        private static void TickHeroRevive(
            HeroState hero,
            BattleHeroRuntimeState heroState,
            Vector2 slotPosition,
            float deltaTime,
            float reviveAttackCooldown)
        {
            heroState.ReviveRemaining = Mathf.Max(0f, heroState.ReviveRemaining - deltaTime);
            if (heroState.ReviveRemaining > 0f)
            {
                return;
            }

            heroState.Hp = heroState.MaxHp;
            heroState.Position = slotPosition;
            hero.AttackCooldown = Mathf.Min(hero.AttackInterval, reviveAttackCooldown);
        }

        private static int GetOrAssignHeroMovementTarget(
            HeroState hero,
            BattleHeroRuntimeState heroState,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            IDictionary<string, int> heroTargetSpawnSequences)
        {
            if (visibleEnemies == null || visibleEnemies.Count <= 0)
            {
                return -1;
            }

            string heroId = hero.Definition.Id;
            if (heroTargetSpawnSequences != null
                && heroTargetSpawnSequences.TryGetValue(heroId, out int lockedSpawnSequence))
            {
                int lockedIndex = CombatTargetingService.FindVisibleEnemyIndexBySpawnSequence(visibleEnemies, lockedSpawnSequence);
                if (lockedIndex >= 0 && visibleEnemies[lockedIndex].Hp > GameNumber.Zero)
                {
                    return lockedIndex;
                }

                heroTargetSpawnSequences.Remove(heroId);
            }

            int targetIndex = CombatTargetingService.FindNearestVisibleEnemyIndex(visibleEnemies, heroState.Position, false);
            if (targetIndex >= 0 && heroTargetSpawnSequences != null)
            {
                heroTargetSpawnSequences[heroId] = visibleEnemies[targetIndex].SpawnSequence;
            }

            return targetIndex;
        }
    }
}
