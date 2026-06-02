using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class BattleRuntimeQueryService
    {
        public static GameNumber GetHeroDamageDone(IReadOnlyDictionary<string, GameNumber> heroDamageMeter, string heroId)
        {
            return !string.IsNullOrEmpty(heroId)
                && heroDamageMeter != null
                && heroDamageMeter.TryGetValue(heroId, out GameNumber damage)
                ? damage
                : GameNumber.Zero;
        }

        public static GameNumber GetMaxHeroDamageDone(
            IReadOnlyDictionary<string, GameNumber> heroDamageMeter,
            IReadOnlyList<HeroState> deployedHeroes)
        {
            GameNumber maxDamage = GameNumber.Zero;
            if (deployedHeroes == null)
            {
                return maxDamage;
            }

            foreach (HeroState hero in deployedHeroes)
            {
                if (hero != null)
                {
                    maxDamage = GameNumber.Max(maxDamage, GetHeroDamageDone(heroDamageMeter, hero.Definition.Id));
                }
            }

            return maxDamage;
        }

        public static float GetVisibleEnemyHpRatio(
            bool isBossFight,
            GameNumber targetHp,
            GameNumber targetMaxHp,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            int visualIndex)
        {
            if (isBossFight)
            {
                return visualIndex == 0 && targetMaxHp > GameNumber.Zero ? Mathf.Clamp01((float)targetHp.RatioTo(targetMaxHp)) : 0f;
            }

            if (visibleEnemies == null || visualIndex < 0 || visualIndex >= visibleEnemies.Count)
            {
                return 0f;
            }

            VisibleEnemyState enemy = visibleEnemies[visualIndex];
            return enemy.MaxHp > GameNumber.Zero ? Mathf.Clamp01((float)enemy.Hp.RatioTo(enemy.MaxHp)) : 0f;
        }

        public static int GetVisibleEnemyDisplayNumber(
            bool isBossFight,
            int killsThisStage,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            int visualIndex)
        {
            if (isBossFight)
            {
                return 1;
            }

            if (visibleEnemies == null || visualIndex < 0 || visualIndex >= visibleEnemies.Count)
            {
                return killsThisStage + visualIndex + 1;
            }

            return visibleEnemies[visualIndex].DisplayNumber;
        }

        public static int GetVisibleEnemySpawnSequence(
            bool isBossFight,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            int visualIndex)
        {
            if (isBossFight)
            {
                return -2;
            }

            if (visibleEnemies == null || visualIndex < 0 || visualIndex >= visibleEnemies.Count)
            {
                return -1;
            }

            return visibleEnemies[visualIndex].SpawnSequence;
        }

        public static int GetHeroTargetVisualIndex(
            bool isBossFight,
            IReadOnlyDictionary<string, int> heroTargetSpawnSequences,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            string heroId)
        {
            if (isBossFight)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(heroId)
                || heroTargetSpawnSequences == null
                || !heroTargetSpawnSequences.TryGetValue(heroId, out int spawnSequence))
            {
                return -1;
            }

            return CombatTargetingService.FindVisibleEnemyIndexBySpawnSequence(visibleEnemies, spawnSequence);
        }

        public static int GetHeroTargetSpawnSequence(
            IReadOnlyDictionary<string, int> heroTargetSpawnSequences,
            string heroId)
        {
            return !string.IsNullOrEmpty(heroId)
                && heroTargetSpawnSequences != null
                && heroTargetSpawnSequences.TryGetValue(heroId, out int spawnSequence)
                ? spawnSequence
                : -1;
        }

        public static Vector2 GetHeroBattlePosition(
            IReadOnlyDictionary<string, BattleHeroRuntimeState> heroRuntimeStates,
            string heroId)
        {
            return !string.IsNullOrEmpty(heroId)
                && heroRuntimeStates != null
                && heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
                ? state.Position
                : Vector2.zero;
        }

        public static float GetHeroHpRatio(
            IReadOnlyDictionary<string, BattleHeroRuntimeState> heroRuntimeStates,
            string heroId)
        {
            if (string.IsNullOrEmpty(heroId)
                || heroRuntimeStates == null
                || !heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
                || state.MaxHp <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(state.Hp / state.MaxHp);
        }

        public static bool IsHeroBattleAlive(
            IReadOnlyDictionary<string, BattleHeroRuntimeState> heroRuntimeStates,
            string heroId)
        {
            return !string.IsNullOrEmpty(heroId)
                && heroRuntimeStates != null
                && heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
                && state.IsAlive;
        }

        public static Vector2 GetVisibleEnemyBattlePosition(
            bool isBossFight,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            int visualIndex)
        {
            if (isBossFight)
            {
                return new Vector2(0f, 2.05f);
            }

            if (visibleEnemies == null || visualIndex < 0 || visualIndex >= visibleEnemies.Count)
            {
                return Vector2.zero;
            }

            return visibleEnemies[visualIndex].Position;
        }
    }
}
