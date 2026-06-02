using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class CombatHitService
    {
        internal readonly struct HitApplicationResult
        {
            public HitApplicationResult(
                string sourceName,
                GameNumber appliedDamage,
                bool isCritical,
                Vector2 hitPosition,
                string damageLog,
                GameNumber targetHp,
                bool defeated,
                int targetSpawnSequence)
            {
                SourceName = sourceName;
                AppliedDamage = appliedDamage;
                IsCritical = isCritical;
                HitPosition = hitPosition;
                DamageLog = damageLog;
                TargetHp = targetHp;
                Defeated = defeated;
                TargetSpawnSequence = targetSpawnSequence;
            }

            public string SourceName { get; }
            public GameNumber AppliedDamage { get; }
            public bool IsCritical { get; }
            public Vector2 HitPosition { get; }
            public string DamageLog { get; }
            public GameNumber TargetHp { get; }
            public bool Defeated { get; }
            public int TargetSpawnSequence { get; }
        }

        internal readonly struct MonsterHitResult
        {
            public MonsterHitResult(
                bool applied,
                int enemyIndex,
                int damagedHeroIndex,
                Vector2 hitPosition,
                GameNumber fortressHp,
                bool heroDefeated,
                string heroId,
                string battleLog)
            {
                Applied = applied;
                EnemyIndex = enemyIndex;
                DamagedHeroIndex = damagedHeroIndex;
                HitPosition = hitPosition;
                FortressHp = fortressHp;
                HeroDefeated = heroDefeated;
                HeroId = heroId ?? string.Empty;
                BattleLog = battleLog ?? string.Empty;
            }

            public bool Applied { get; }
            public int EnemyIndex { get; }
            public int DamagedHeroIndex { get; }
            public Vector2 HitPosition { get; }
            public GameNumber FortressHp { get; }
            public bool HeroDefeated { get; }
            public string HeroId { get; }
            public string BattleLog { get; }
        }

        public static HitApplicationResult ApplyTargetDamage(
            GameNumber targetHp,
            GameNumber damage,
            string sourceName,
            bool isCritical,
            Vector2 hitPosition)
        {
            GameNumber appliedDamage = CombatDamageService.NormalizeDamage(damage);
            GameNumber nextHp = GameNumber.Max(GameNumber.Zero, targetHp - appliedDamage);
            return new HitApplicationResult(
                sourceName,
                appliedDamage,
                isCritical,
                hitPosition,
                BuildDamageLog(sourceName, appliedDamage, isCritical),
                nextHp,
                nextHp <= GameNumber.Zero,
                -1);
        }

        public static bool TryApplyVisibleEnemyDamage(
            IList<VisibleEnemyState> visibleEnemies,
            int enemyIndex,
            GameNumber damage,
            string sourceName,
            bool isCritical,
            out HitApplicationResult result)
        {
            result = default;
            if (visibleEnemies == null || enemyIndex < 0 || enemyIndex >= visibleEnemies.Count)
            {
                return false;
            }

            VisibleEnemyState enemy = visibleEnemies[enemyIndex];
            GameNumber appliedDamage = CombatDamageService.NormalizeDamage(damage);
            enemy.Hp = GameNumber.Max(GameNumber.Zero, enemy.Hp - appliedDamage);
            result = new HitApplicationResult(
                sourceName,
                appliedDamage,
                isCritical,
                enemy.Position,
                BuildDamageLog(sourceName, appliedDamage, isCritical),
                enemy.Hp,
                enemy.Hp <= GameNumber.Zero,
                enemy.SpawnSequence);
            return true;
        }

        public static MonsterHitResult ApplyMonsterDamageToFortress(
            int enemyIndex,
            GameNumber fortressHp,
            GameNumber fortressMaxHp)
        {
            if (fortressHp <= GameNumber.Zero)
            {
                return new MonsterHitResult(false, enemyIndex, -1, Vector2.zero, fortressHp, false, string.Empty, string.Empty);
            }

            GameNumber damage = CombatDamageService.NormalizeDamage(GameNumber.Max(GameNumber.One, fortressMaxHp * 0.018d));
            GameNumber nextFortressHp = GameNumber.Max(GameNumber.Zero, fortressHp - damage);
            string battleLog = nextFortressHp <= GameNumber.Zero
                ? "요새 파괴: 영웅이 부활할 때까지 버티는 중"
                : string.Empty;
            return new MonsterHitResult(true, enemyIndex, -1, Vector2.zero, nextFortressHp, false, string.Empty, battleLog);
        }

        public static MonsterHitResult ApplyMonsterDamageToHero(
            int enemyIndex,
            BattleHeroRuntimeState heroState,
            double damageTakenMultiplier,
            float reviveSeconds)
        {
            if (heroState == null || !heroState.IsAlive)
            {
                return new MonsterHitResult(false, enemyIndex, -1, Vector2.zero, GameNumber.Zero, false, string.Empty, string.Empty);
            }

            float damage = Mathf.Max(1f, heroState.MaxHp * 0.035f * (float)damageTakenMultiplier);
            heroState.Hp = Mathf.Max(0f, heroState.Hp - damage);
            bool defeated = heroState.Hp <= 0f;
            string battleLog = string.Empty;
            if (defeated)
            {
                heroState.ReviveRemaining = reviveSeconds;
                battleLog = heroState.Hero.Definition.DisplayName + " 전투불능";
            }

            return new MonsterHitResult(
                true,
                enemyIndex,
                heroState.SlotIndex,
                heroState.Position,
                GameNumber.Zero,
                defeated,
                heroState.Hero.Definition.Id,
                battleLog);
        }

        private static string BuildDamageLog(string sourceName, GameNumber appliedDamage, bool isCritical)
        {
            return sourceName + " -" + NumberFormatter.Format(appliedDamage) + (isCritical ? " CRIT" : string.Empty);
        }
    }
}
