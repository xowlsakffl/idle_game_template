using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Battle
{
    public sealed partial class BattleManager
    {
        private void ApplyDamageToVisibleEnemy(int enemyIndex, GameNumber damage, string sourceName, bool isCritical, string heroId = null)
        {
            if (!CombatHitService.TryApplyVisibleEnemyDamage(
                visibleEnemies,
                enemyIndex,
                damage,
                sourceName,
                isCritical,
                out CombatHitService.HitApplicationResult hitResult))
            {
                SyncTargetFromVisibleEnemies();
                return;
            }

            hitFeedback.MarkVisibleEnemyHit(enemyIndex);
            ApplyHitResult(hitResult, heroId);

            if (hitResult.Defeated)
            {
                HandleVisibleEnemyDefeated(enemyIndex, hitResult.TargetSpawnSequence);
                return;
            }

            SyncTargetFromVisibleEnemies();
        }

        private void ApplyDamage(GameNumber damage, string sourceName, bool isCritical, string heroId = null)
        {
            CombatHitService.HitApplicationResult hitResult = CombatHitService.ApplyTargetDamage(
                TargetHp,
                damage,
                sourceName,
                isCritical,
                new Vector2(0f, 2.05f));
            TargetHp = hitResult.TargetHp;
            ApplyHitResult(hitResult, heroId);

            if (hitResult.Defeated)
            {
                HandleTargetDefeated();
            }
        }

        private void ApplyHitResult(CombatHitService.HitApplicationResult hitResult, string heroId)
        {
            hitFeedback.ApplyHitResult(hitResult);
            AddHeroDamage(heroId, hitResult.AppliedDamage);
        }

        private void HandleTargetDefeated()
        {
            StageDefinition stage = progressManager.CurrentStage;

            if (stage.Type == StageType.Boss)
            {
                LastRewardLog = CombatRewardService.ApplyBossClearReward(
                    stage,
                    wallet,
                    amount => accountProgressManager?.AddExperience(amount),
                    AddFortressExperience,
                    GetBossGoldMultiplier(),
                    GetAccountExperienceMultiplier(),
                    random.NextDouble);
                CompleteStage(stage, StageCombatFlowService.BuildBossClearLog(stage), clearVisibleEnemies: false);
                return;
            }

            StageClearRewardService.StageKillResult killResult = ApplyEnemyDefeatRewardAndRegisterKill(stage);

            if (killResult.IsComplete)
            {
                CompleteStage(stage, killResult.BattleLog, clearVisibleEnemies: false);
                return;
            }

            LastBattleLog = killResult.BattleLog;
            SpawnTarget();
        }

        private void HandleVisibleEnemyDefeated(int enemyIndex, int defeatedSpawnSequence)
        {
            StageDefinition stage = progressManager.CurrentStage;
            StageClearRewardService.StageKillResult killResult = ApplyEnemyDefeatRewardAndRegisterKill(stage);
            StageCombatFlowService.VisibleEnemyDefeatResult defeatResult = StageCombatFlowService.ResolveVisibleEnemyDefeat(
                visibleEnemies,
                heroTargetSpawnSequences,
                skillTargetSpawnSequences,
                petTargetSpawnSequences,
                enemyIndex,
                defeatedSpawnSequence,
                killResult.IsComplete,
                ref nextEnemySpawnSequence,
                RequiredKills,
                TargetMaxHp,
                RespawnEnemySpawnGraceSeconds,
                EnemyAttackIntervalSeconds,
                FieldHalfWidth,
                FieldHalfHeight);
            if (defeatResult.HasDefeatedPosition)
            {
                hitFeedback.RegisterEnemyDefeat(defeatResult.DefeatedPosition);
            }

            if (killResult.IsComplete)
            {
                CompleteStage(stage, killResult.BattleLog, clearVisibleEnemies: true);
                return;
            }

            if (defeatResult.ReplacedOrRemovedEnemy)
            {
                hitFeedback.ClearRecentHitEnemy();
            }

            SyncTargetFromVisibleEnemies();
            LastBattleLog = killResult.BattleLog;
            NotifyChanged(BattleChangeFlags.Combat);
        }

        private StageClearRewardService.StageKillResult ApplyEnemyDefeatRewardAndRegisterKill(StageDefinition stage)
        {
            LastRewardLog = CombatRewardService.ApplyEnemyDefeatReward(
                stage,
                wallet,
                amount => accountProgressManager?.AddExperience(amount),
                AddFortressExperience,
                GetEnemyGoldMultiplier(),
                GetEnemyHeroExpMultiplier(),
                GetAccountExperienceMultiplier(),
                random.NextDouble);
            StageClearRewardService.StageKillResult killResult = StageCombatFlowService.RegisterEnemyKill(stage, KillsThisStage, RequiredKills);
            KillsThisStage = killResult.Kills;
            return killResult;
        }

        private void CompleteStage(StageDefinition stage, string battleLog, bool clearVisibleEnemies)
        {
            if (clearVisibleEnemies)
            {
                visibleEnemies.Clear();
                SyncTargetFromVisibleEnemies();
            }

            StageCombatFlowService.StageCompletionResult result = StageCombatFlowService.CompleteStage(stage, wallet, progressManager, battleLog);
            LastRewardLog += result.RewardLogSuffix;
            LastBattleLog = result.BattleLog;
            NotifyChanged(BattleChangeFlags.Combat);
        }
    }
}
