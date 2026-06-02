using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class StageCombatFlowService
    {
        internal readonly struct StageCompletionResult
        {
            public StageCompletionResult(string battleLog, string rewardLogSuffix)
            {
                BattleLog = battleLog ?? string.Empty;
                RewardLogSuffix = rewardLogSuffix ?? string.Empty;
            }

            public string BattleLog { get; }
            public string RewardLogSuffix { get; }
        }

        internal readonly struct VisibleEnemyDefeatResult
        {
            public VisibleEnemyDefeatResult(bool hasDefeatedPosition, Vector2 defeatedPosition, bool replacedOrRemovedEnemy)
            {
                HasDefeatedPosition = hasDefeatedPosition;
                DefeatedPosition = defeatedPosition;
                ReplacedOrRemovedEnemy = replacedOrRemovedEnemy;
            }

            public bool HasDefeatedPosition { get; }
            public Vector2 DefeatedPosition { get; }
            public bool ReplacedOrRemovedEnemy { get; }
        }

        internal readonly struct BossTimerResult
        {
            public BossTimerResult(bool failed, float timeRemaining, string battleLog)
            {
                Failed = failed;
                TimeRemaining = timeRemaining;
                BattleLog = battleLog;
            }

            public bool Failed { get; }
            public float TimeRemaining { get; }
            public string BattleLog { get; }
        }

        public static BossTimerResult TickBossTimer(
            bool isBossFight,
            GameNumber targetHp,
            float timeRemaining,
            string currentStageId,
            float deltaTime)
        {
            if (!isBossFight || targetHp <= GameNumber.Zero)
            {
                return new BossTimerResult(false, timeRemaining, string.Empty);
            }

            float nextTimeRemaining = Mathf.Max(0f, timeRemaining - deltaTime);
            if (nextTimeRemaining > 0f)
            {
                return new BossTimerResult(false, nextTimeRemaining, string.Empty);
            }

            string fallbackStageId = GameData.GetPreviousNormalStageId(currentStageId);
            return new BossTimerResult(
                true,
                0f,
                "보스 실패: " + fallbackStageId + " 반복 파밍으로 이동");
        }

        public static StageClearRewardService.StageKillResult RegisterEnemyKill(
            StageDefinition stage,
            int currentKills,
            int requiredKills)
        {
            return StageClearRewardService.RegisterKill(stage, currentKills, requiredKills);
        }

        public static string BuildBossClearLog(StageDefinition stage)
        {
            return StageClearRewardService.BuildBossClearLog(stage);
        }

        public static VisibleEnemyDefeatResult ResolveVisibleEnemyDefeat(
            List<VisibleEnemyState> visibleEnemies,
            Dictionary<string, int> heroTargetLocks,
            Dictionary<string, int> skillTargetLocks,
            Dictionary<string, int> petTargetLocks,
            int enemyIndex,
            int defeatedSpawnSequence,
            bool stageComplete,
            ref int nextEnemySpawnSequence,
            int requiredKills,
            GameNumber targetMaxHp,
            float respawnGraceSeconds,
            float enemyAttackIntervalSeconds,
            float fieldHalfWidth,
            float fieldHalfHeight)
        {
            CombatTargetingService.RemoveTargetLocksForSpawn(
                heroTargetLocks,
                skillTargetLocks,
                petTargetLocks,
                defeatedSpawnSequence);

            bool hasDefeatedPosition = MonsterSpawnService.TryGetVisibleEnemyPosition(
                visibleEnemies,
                enemyIndex,
                out Vector2 defeatedPosition);

            bool replacedOrRemovedEnemy = !stageComplete && MonsterSpawnService.ReplaceOrRemoveDefeatedEnemy(
                visibleEnemies,
                enemyIndex,
                ref nextEnemySpawnSequence,
                requiredKills,
                targetMaxHp,
                respawnGraceSeconds,
                enemyAttackIntervalSeconds,
                fieldHalfWidth,
                fieldHalfHeight);

            return new VisibleEnemyDefeatResult(hasDefeatedPosition, defeatedPosition, replacedOrRemovedEnemy);
        }

        public static StageCompletionResult CompleteStage(
            StageDefinition stage,
            CurrencyWallet wallet,
            StageProgressManager progressManager,
            string battleLog)
        {
            string rewardLogSuffix = string.Empty;
            if (progressManager != null)
            {
                rewardLogSuffix = StageClearRewardService.GrantFirstClearReward(
                    stage,
                    wallet,
                    progressManager.HighestStageId,
                    progressManager.ChapterOneBossCleared);
                progressManager.HandleStageCleared();
            }

            return new StageCompletionResult(battleLog, rewardLogSuffix);
        }
    }
}
