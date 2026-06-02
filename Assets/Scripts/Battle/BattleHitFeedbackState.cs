using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal sealed class BattleHitFeedbackState
    {
        public int RecentHitEnemyIndex { get; private set; } = -1;
        public int RecentAttackingEnemyIndex { get; private set; } = -1;
        public int RecentDamagedHeroIndex { get; private set; } = -1;
        public int MonsterHitSequence { get; private set; }
        public int EnemyDefeatSequence { get; private set; }
        public Vector2 LastHitPosition { get; private set; }
        public Vector2 LastMonsterHitPosition { get; private set; }
        public Vector2 LastDefeatedEnemyPosition { get; private set; }
        public string LastDamageLog { get; private set; } = string.Empty;
        public string LastHitSourceName { get; private set; } = string.Empty;
        public GameNumber LastHitDamage { get; private set; }
        public bool LastHitWasCritical { get; private set; }
        public int HitSequence { get; private set; }

        public void ResetStage()
        {
            RecentHitEnemyIndex = -1;
            RecentAttackingEnemyIndex = -1;
            RecentDamagedHeroIndex = -1;
        }

        public void ClearRecentHitEnemy()
        {
            RecentHitEnemyIndex = -1;
        }

        public void MarkVisibleEnemyHit(int enemyIndex)
        {
            RecentHitEnemyIndex = enemyIndex;
        }

        public void ApplyHitResult(CombatHitService.HitApplicationResult hitResult)
        {
            LastHitPosition = hitResult.HitPosition;
            LastHitSourceName = hitResult.SourceName;
            LastHitDamage = hitResult.AppliedDamage;
            LastHitWasCritical = hitResult.IsCritical;
            HitSequence += 1;
            LastDamageLog = hitResult.DamageLog;
        }

        public string ApplyMonsterHitResult(CombatHitService.MonsterHitResult hitResult)
        {
            RecentAttackingEnemyIndex = hitResult.EnemyIndex;
            RecentDamagedHeroIndex = hitResult.DamagedHeroIndex;
            LastMonsterHitPosition = hitResult.HitPosition;
            MonsterHitSequence += 1;
            return hitResult.BattleLog;
        }

        public void RegisterEnemyDefeat(Vector2 defeatedPosition)
        {
            LastDefeatedEnemyPosition = defeatedPosition;
            EnemyDefeatSequence += 1;
        }
    }
}
