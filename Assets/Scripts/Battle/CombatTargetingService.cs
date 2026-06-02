using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class CombatTargetingService
    {
        public static int SelectVisibleEnemyIndexForHero(
            HeroState hero,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            Dictionary<string, int> targetLocks,
            Dictionary<string, BattleHeroRuntimeState> heroRuntimeStates,
            float attackRange)
        {
            if (hero == null || visibleEnemies == null || visibleEnemies.Count <= 0)
            {
                return -1;
            }

            string heroId = hero.Definition.Id;
            if (string.IsNullOrEmpty(heroId)
                || heroRuntimeStates == null
                || !heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState heroState)
                || !heroState.IsAlive)
            {
                return -1;
            }

            if (targetLocks != null && targetLocks.TryGetValue(heroId, out int spawnSequence))
            {
                int lockedIndex = FindVisibleEnemyIndexBySpawnSequence(visibleEnemies, spawnSequence);
                if (lockedIndex >= 0 && visibleEnemies[lockedIndex].IsAttackable)
                {
                    float attackRangeSqr = attackRange * attackRange;
                    return (heroState.Position - visibleEnemies[lockedIndex].Position).sqrMagnitude <= attackRangeSqr
                        ? lockedIndex
                        : -1;
                }

                targetLocks.Remove(heroId);
            }

            int targetIndex = FindNearestAttackableEnemyInRange(visibleEnemies, heroState.Position, attackRange);
            if (targetIndex < 0)
            {
                return -1;
            }

            if (targetLocks != null)
            {
                targetLocks[heroId] = visibleEnemies[targetIndex].SpawnSequence;
            }

            return targetIndex;
        }

        public static int SelectVisibleEnemyIndexForLockedSource(
            string sourceId,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            Dictionary<string, int> targetLocks,
            int preferredOffset)
        {
            if (visibleEnemies == null || visibleEnemies.Count <= 0)
            {
                return -1;
            }

            if (!string.IsNullOrEmpty(sourceId)
                && targetLocks != null
                && targetLocks.TryGetValue(sourceId, out int spawnSequence))
            {
                int lockedIndex = FindVisibleEnemyIndexBySpawnSequence(visibleEnemies, spawnSequence);
                if (lockedIndex >= 0 && visibleEnemies[lockedIndex].IsAttackable)
                {
                    return lockedIndex;
                }
            }

            int targetIndex = FindAttackableVisibleEnemyIndex(visibleEnemies, preferredOffset);
            if (targetIndex < 0)
            {
                return -1;
            }

            if (!string.IsNullOrEmpty(sourceId) && targetLocks != null)
            {
                targetLocks[sourceId] = visibleEnemies[targetIndex].SpawnSequence;
            }

            return targetIndex;
        }

        public static int FindFirstAttackableVisibleEnemyIndex(IReadOnlyList<VisibleEnemyState> visibleEnemies)
        {
            return FindAttackableVisibleEnemyIndex(visibleEnemies, 0);
        }

        public static int FindAttackableVisibleEnemyIndex(IReadOnlyList<VisibleEnemyState> visibleEnemies, int preferredOffset)
        {
            if (visibleEnemies == null || visibleEnemies.Count <= 0)
            {
                return -1;
            }

            int startIndex = Mathf.Abs(preferredOffset) % visibleEnemies.Count;
            for (int offset = 0; offset < visibleEnemies.Count; offset++)
            {
                int index = (startIndex + offset) % visibleEnemies.Count;
                if (visibleEnemies[index].IsAttackable)
                {
                    return index;
                }
            }

            return -1;
        }

        public static int FindVisibleEnemyIndexBySpawnSequence(IReadOnlyList<VisibleEnemyState> visibleEnemies, int spawnSequence)
        {
            if (visibleEnemies == null)
            {
                return -1;
            }

            for (int i = 0; i < visibleEnemies.Count; i++)
            {
                if (visibleEnemies[i].SpawnSequence == spawnSequence)
                {
                    return i;
                }
            }

            return -1;
        }

        public static BattleHeroRuntimeState FindNearestLivingHero(
            IEnumerable<BattleHeroRuntimeState> heroStates,
            Vector2 fromPosition)
        {
            BattleHeroRuntimeState nearest = null;
            float nearestDistance = float.MaxValue;
            if (heroStates == null)
            {
                return nearest;
            }

            foreach (BattleHeroRuntimeState heroState in heroStates)
            {
                if (!heroState.IsAlive)
                {
                    continue;
                }

                float distance = (heroState.Position - fromPosition).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = heroState;
                }
            }

            return nearest;
        }

        public static BattleHeroRuntimeState FindNearestMonsterTargetHero(
            IEnumerable<BattleHeroRuntimeState> heroStates,
            Vector2 fromPosition,
            bool fortressAlive)
        {
            BattleHeroRuntimeState nearestFrontline = null;
            float frontlineDistance = float.MaxValue;
            if (heroStates == null)
            {
                return null;
            }

            foreach (BattleHeroRuntimeState heroState in heroStates)
            {
                if (!heroState.IsAlive || !IsFrontlineHero(heroState.Hero))
                {
                    continue;
                }

                float distance = (heroState.Position - fromPosition).sqrMagnitude;
                if (distance < frontlineDistance)
                {
                    frontlineDistance = distance;
                    nearestFrontline = heroState;
                }
            }

            if (nearestFrontline != null)
            {
                return nearestFrontline;
            }

            return fortressAlive ? null : FindNearestLivingHero(heroStates, fromPosition);
        }

        public static int FindNearestVisibleEnemyIndex(
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            Vector2 fromPosition,
            bool attackableOnly)
        {
            int nearestIndex = -1;
            float nearestDistance = float.MaxValue;
            if (visibleEnemies == null)
            {
                return nearestIndex;
            }

            for (int i = 0; i < visibleEnemies.Count; i++)
            {
                VisibleEnemyState enemy = visibleEnemies[i];
                if (enemy.Hp <= GameNumber.Zero || (attackableOnly && !enemy.IsAttackable))
                {
                    continue;
                }

                float distance = (enemy.Position - fromPosition).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        public static int FindNearestAttackableEnemyInRange(
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            Vector2 fromPosition,
            float range)
        {
            int nearestIndex = -1;
            float nearestDistance = range * range;
            if (visibleEnemies == null)
            {
                return nearestIndex;
            }

            for (int i = 0; i < visibleEnemies.Count; i++)
            {
                VisibleEnemyState enemy = visibleEnemies[i];
                if (!enemy.IsAttackable)
                {
                    continue;
                }

                float distance = (enemy.Position - fromPosition).sqrMagnitude;
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

    }
}
