using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class MonsterSpawnService
    {
        public static void FillVisibleEnemies(
            IList<VisibleEnemyState> visibleEnemies,
            ref int nextEnemySpawnSequence,
            int requiredKills,
            GameNumber targetMaxHp,
            float spawnGraceSeconds,
            float enemyAttackIntervalSeconds,
            float fieldHalfWidth,
            float fieldHalfHeight,
            int maxVisibleEnemies)
        {
            if (visibleEnemies == null)
            {
                return;
            }

            while (visibleEnemies.Count < maxVisibleEnemies
                && nextEnemySpawnSequence < requiredKills)
            {
                visibleEnemies.Add(CreateVisibleEnemy(
                    ref nextEnemySpawnSequence,
                    targetMaxHp,
                    spawnGraceSeconds,
                    enemyAttackIntervalSeconds,
                    fieldHalfWidth,
                    fieldHalfHeight));
            }
        }

        public static VisibleEnemyState CreateVisibleEnemy(
            ref int nextEnemySpawnSequence,
            GameNumber targetMaxHp,
            float spawnGraceSeconds,
            float enemyAttackIntervalSeconds,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            int spawnSequence = nextEnemySpawnSequence;
            nextEnemySpawnSequence += 1;
            return new VisibleEnemyState(
                spawnSequence,
                GameNumber.Max(GameNumber.One, targetMaxHp),
                spawnSequence + 1,
                spawnGraceSeconds,
                GetSpawnPosition(spawnSequence, fieldHalfWidth, fieldHalfHeight),
                enemyAttackIntervalSeconds);
        }

        public static bool TryGetVisibleEnemyPosition(
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            int enemyIndex,
            out Vector2 position)
        {
            position = Vector2.zero;
            if (visibleEnemies == null || enemyIndex < 0 || enemyIndex >= visibleEnemies.Count)
            {
                return false;
            }

            position = visibleEnemies[enemyIndex].Position;
            return true;
        }

        public static bool ReplaceOrRemoveDefeatedEnemy(
            IList<VisibleEnemyState> visibleEnemies,
            int enemyIndex,
            ref int nextEnemySpawnSequence,
            int requiredKills,
            GameNumber targetMaxHp,
            float respawnGraceSeconds,
            float enemyAttackIntervalSeconds,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            if (visibleEnemies == null || enemyIndex < 0 || enemyIndex >= visibleEnemies.Count)
            {
                return false;
            }

            if (nextEnemySpawnSequence < requiredKills)
            {
                visibleEnemies[enemyIndex] = CreateVisibleEnemy(
                    ref nextEnemySpawnSequence,
                    targetMaxHp,
                    respawnGraceSeconds,
                    enemyAttackIntervalSeconds,
                    fieldHalfWidth,
                    fieldHalfHeight);
            }
            else
            {
                visibleEnemies.RemoveAt(enemyIndex);
            }

            return true;
        }

        public static Vector2 GetSpawnPosition(int spawnSequence, float fieldHalfWidth, float fieldHalfHeight)
        {
            int side = Mathf.Abs(spawnSequence) % 4;
            float offset = Mathf.Lerp(-2.6f, 2.6f, PseudoRandom01(spawnSequence * 19 + 5));
            switch (side)
            {
                case 0:
                    return new Vector2(-fieldHalfWidth - 0.55f, offset);
                case 1:
                    return new Vector2(fieldHalfWidth + 0.55f, offset);
                case 2:
                    return new Vector2(offset, fieldHalfHeight + 0.55f);
                default:
                    return new Vector2(offset, -fieldHalfHeight - 0.55f);
            }
        }

        private static float PseudoRandom01(int seed)
        {
            unchecked
            {
                uint value = (uint)(seed * 747796405 + 2891336453);
                value = ((value >> ((int)(value >> 28) + 4)) ^ value) * 277803737;
                value = (value >> 22) ^ value;
                return (value & 0xFFFFFF) / 16777215f;
            }
        }
    }
}
