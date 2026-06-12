using System.Collections.Generic;
using System.Collections;
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
            if (dungeonRunActive && activeDungeonKind == DungeonKind.TotemEssence)
            {
                ApplyDamageToTotemDungeonBossChain(damage, sourceName, isCritical, heroId);
                return;
            }

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
            if (dungeonRunActive)
            {
                if (activeDungeonKind == DungeonKind.TotemEssence)
                {
                    HandleTotemDungeonBossDefeated();
                    return;
                }

                CompleteDungeonBoss();
                return;
            }

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
            if (dungeonRunActive)
            {
                HandleDungeonVisibleEnemyDefeated(enemyIndex, defeatedSpawnSequence);
                return;
            }

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

        private void HandleDungeonVisibleEnemyDefeated(int enemyIndex, int defeatedSpawnSequence)
        {
            KillsThisStage = Mathf.Clamp(KillsThisStage + 1, 0, RequiredKills);
            bool waveComplete = KillsThisStage >= RequiredKills;
            StageCombatFlowService.VisibleEnemyDefeatResult defeatResult = StageCombatFlowService.ResolveVisibleEnemyDefeat(
                visibleEnemies,
                heroTargetSpawnSequences,
                skillTargetSpawnSequences,
                petTargetSpawnSequences,
                enemyIndex,
                defeatedSpawnSequence,
                waveComplete,
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

            if (waveComplete)
            {
                if (activeDungeonKind == DungeonKind.Gold)
                {
                    CompleteDungeonRun();
                    return;
                }

                SpawnDungeonBoss();
                return;
            }

            if (defeatResult.ReplacedOrRemovedEnemy)
            {
                hitFeedback.ClearRecentHitEnemy();
            }

            SyncTargetFromVisibleEnemies();
            LastBattleLog = DungeonProgressManager.GetTitle(activeDungeonKind) + " " + KillsThisStage + " / " + RequiredKills;
            NotifyChanged(BattleChangeFlags.Combat);
        }

        private void CompleteDungeonBoss()
        {
            CompleteDungeonRun();
        }

        private void HandleTotemDungeonBossDefeated()
        {
            AdvanceTotemDungeonBosses(1);
            NotifyChanged(BattleChangeFlags.Combat);
        }

        private void ApplyDamageToTotemDungeonBossChain(GameNumber damage, string sourceName, bool isCritical, string heroId)
        {
            GameNumber remainingDamage = CombatDamageService.NormalizeDamage(damage);
            GameNumber totalAppliedDamage = GameNumber.Zero;
            int defeatedBefore = KillsThisStage;

            if (TargetHp > GameNumber.Zero)
            {
                GameNumber appliedToCurrent = GameNumber.Min(remainingDamage, TargetHp);
                TargetHp = GameNumber.Max(GameNumber.Zero, TargetHp - appliedToCurrent);
                remainingDamage = GameNumber.Max(GameNumber.Zero, remainingDamage - appliedToCurrent);
                totalAppliedDamage += appliedToCurrent;
                if (TargetHp <= GameNumber.Zero)
                {
                    AdvanceTotemDungeonBosses(1);
                }
            }

            if (remainingDamage > GameNumber.Zero)
            {
                int startLevel = Mathf.Max(1, activeDungeonLevel);
                int defeatedCount = CountDefeatedTotemBosses(startLevel, remainingDamage);
                if (defeatedCount > 0)
                {
                    int defeatedEndLevel = (int)Math.Min(int.MaxValue, (long)startLevel + defeatedCount - 1L);
                    GameNumber defeatedHp = GetTotemBossHpTotal(startLevel, defeatedEndLevel);
                    remainingDamage = GameNumber.Max(GameNumber.Zero, remainingDamage - defeatedHp);
                    totalAppliedDamage += defeatedHp;
                    AdvanceTotemDungeonBosses(defeatedCount);
                }
            }

            if (remainingDamage > GameNumber.Zero && TargetHp > GameNumber.Zero)
            {
                GameNumber appliedToCurrent = GameNumber.Min(remainingDamage, TargetHp);
                TargetHp = GameNumber.Max(GameNumber.Zero, TargetHp - appliedToCurrent);
                totalAppliedDamage += appliedToCurrent;
            }

            if (totalAppliedDamage <= GameNumber.Zero)
            {
                return;
            }

            var hitResult = new CombatHitService.HitApplicationResult(
                sourceName,
                totalAppliedDamage,
                isCritical,
                new Vector2(0f, 2.05f),
                sourceName + " -" + NumberFormatter.Format(totalAppliedDamage) + (isCritical ? " CRIT" : string.Empty),
                TargetHp,
                false,
                -1);
            ApplyHitResult(hitResult, heroId);

            int defeatedNow = KillsThisStage - defeatedBefore;
            if (defeatedNow > 1)
            {
                LastBattleLog = "토템석 보스 " + defeatedNow + "마리 연속 처치: 다음 보스 Lv." + activeDungeonLevel;
            }

            NotifyChanged(BattleChangeFlags.Combat);
        }

        private void AdvanceTotemDungeonBosses(int defeatedCount)
        {
            if (defeatedCount <= 0)
            {
                return;
            }

            long nextKills = (long)KillsThisStage + defeatedCount;
            KillsThisStage = nextKills >= int.MaxValue ? int.MaxValue : (int)nextKills;
            activeDungeonLevel = KillsThisStage < int.MaxValue ? KillsThisStage + 1 : int.MaxValue;
            SpawnTotemDungeonBoss();
            LastBattleLog = "토템석 보스 Lv." + KillsThisStage + " 처치: 다음 보스 Lv." + activeDungeonLevel;
        }

        private void CompleteTotemBossDungeonRun(string resultTitle)
        {
            int defeatedBossLevel = Mathf.Max(0, KillsThisStage);
            string rewardText = dungeonProgressManager != null
                ? dungeonProgressManager.CompleteTotemBossDungeon(defeatedBossLevel)
                : string.Empty;
            LastRewardLog = rewardText;
            bool repeat = activeDungeonRepeat;
            bool wasRepeat = activeDungeonStartedWithRepeat || activeDungeonRepeat;
            DungeonKind completedKind = activeDungeonKind;
            activeDungeonReceipt = default;
            string resultLog = resultTitle + ": 보스 " + defeatedBossLevel + " 처치 / " + rewardText;

            if (repeat && TryPrepareNextDungeonRepeat(completedKind, defeatedBossLevel))
            {
                RegisterDungeonClearResult(completedKind, defeatedBossLevel, rewardText, false, true);
                LastBattleLog = resultLog;
                NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.BattleLog);
                dungeonRepeatStartCoroutine = StartCoroutine(StartNextDungeonAfterRewardNotice());
                return;
            }

            dungeonRunActive = false;
            activeDungeonRepeat = false;
            activeDungeonStartedWithRepeat = false;
            dungeonRepeatWaitingForNextRun = false;
            visibleEnemies.Clear();
            ClearTargetLocks();
            StartStage(false);
            RegisterDungeonClearResult(completedKind, defeatedBossLevel, rewardText, wasRepeat, false);
            LastBattleLog = resultLog;
            NotifyChanged(BattleChangeFlags.Combat);
        }

        private void CompleteDungeonRun()
        {
            string rewardText = dungeonProgressManager != null
                ? dungeonProgressManager.CompleteDungeon(activeDungeonKind, activeDungeonLevel)
                : string.Empty;
            LastRewardLog = rewardText;
            LastBattleLog = DungeonProgressManager.GetTitle(activeDungeonKind) + " 클리어: " + rewardText;
            bool repeat = activeDungeonRepeat;
            bool wasRepeat = activeDungeonStartedWithRepeat || activeDungeonRepeat;
            DungeonKind completedKind = activeDungeonKind;
            int completedLevel = activeDungeonLevel;
            activeDungeonReceipt = default;

            if (repeat && TryPrepareNextDungeonRepeat(completedKind, completedLevel))
            {
                RegisterDungeonClearResult(completedKind, completedLevel, rewardText, false, true);
                NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.BattleLog);
                dungeonRepeatStartCoroutine = StartCoroutine(StartNextDungeonAfterRewardNotice());
                return;
            }

            dungeonRunActive = false;
            activeDungeonRepeat = false;
            activeDungeonStartedWithRepeat = false;
            dungeonRepeatWaitingForNextRun = false;
            visibleEnemies.Clear();
            SyncTargetFromVisibleEnemies();
            StartStage(false);
            RegisterDungeonClearResult(completedKind, completedLevel, rewardText, wasRepeat, false);
            LastBattleLog = DungeonProgressManager.GetTitle(completedKind) + " 클리어: " + rewardText;
            NotifyChanged(BattleChangeFlags.Combat);
        }

        private bool TryPrepareNextDungeonRepeat(DungeonKind kind, int completedLevel)
        {
            if (dungeonProgressManager == null)
            {
                return false;
            }

            int nextLevel = kind == DungeonKind.TotemEssence
                ? 1
                : dungeonProgressManager.ClampSelectableLevel(kind, completedLevel + 1);
            if (!dungeonProgressManager.TryConsumeEntry(kind, nextLevel, out DungeonEntryReceipt receipt))
            {
                return false;
            }

            activeDungeonKind = kind;
            activeDungeonLevel = nextLevel;
            activeDungeonRepeat = true;
            activeDungeonStartedWithRepeat = true;
            activeDungeonReceipt = receipt;
            dungeonRunActive = true;
            dungeonRepeatWaitingForNextRun = true;
            bool isTotemBossDungeon = kind == DungeonKind.TotemEssence;
            bool isSingleBossDungeon = IsSingleBossDungeon(kind);
            bool isBossDungeon = isTotemBossDungeon || isSingleBossDungeon;
            IsBossFight = isBossDungeon;
            RequiredKills = isBossDungeon
                ? 0
                : DungeonProgressManager.RequiredNormalKills;
            TargetName = isTotemBossDungeon
                ? "토템석 보스 Lv.1"
                : isSingleBossDungeon
                ? DungeonProgressManager.GetTitle(kind) + " 보스 Lv." + nextLevel
                : DungeonProgressManager.GetTitle(kind) + " Lv." + nextLevel;
            TargetMaxHp = isTotemBossDungeon
                ? GetTotemDungeonBossHp(1)
                : isSingleBossDungeon
                ? GetDungeonBossHp(nextLevel)
                : GetDungeonEnemyHp(nextLevel);
            TargetHp = GameNumber.Zero;
            BossTimeRemaining = DungeonProgressManager.GetTimeLimitSeconds(kind);
            VisibleEnemyCount = 0;
            visibleEnemies.Clear();
            ClearTargetLocks();
            LastBattleLog = TargetName + " 준비 중";
            return true;
        }

        private IEnumerator StartNextDungeonAfterRewardNotice()
        {
            yield return new WaitForSecondsRealtime(DungeonRepeatRewardPauseSeconds);
            dungeonRepeatStartCoroutine = null;
            if (!dungeonRunActive || !dungeonRepeatWaitingForNextRun)
            {
                yield break;
            }

            if (!activeDungeonRepeat)
            {
                DungeonEntryReceipt receipt = activeDungeonReceipt;
                DungeonKind stoppedKind = activeDungeonKind;
                dungeonRunActive = false;
                activeDungeonStartedWithRepeat = false;
                activeDungeonReceipt = default;
                dungeonRepeatWaitingForNextRun = false;
                dungeonProgressManager?.RefundEntry(receipt);
                visibleEnemies.Clear();
                ClearTargetLocks();
                StartStage(false);
                LastRewardLog = "입장 비용 반환";
                LastBattleLog = DungeonProgressManager.GetTitle(stoppedKind) + " 연속 도전 중단: 다음 입장 취소";
                NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.BattleLog);
                yield break;
            }

            StartDungeonRun();
        }

        private void RegisterDungeonClearResult(DungeonKind kind, int level, string rewardText, bool endedRepeat, bool continuesRepeat)
        {
            lastDungeonClearKind = kind;
            lastDungeonClearLevel = kind == DungeonKind.TotemEssence
                ? Mathf.Max(0, level)
                : Mathf.Max(1, level);
            lastDungeonClearRewardText = rewardText ?? string.Empty;
            lastDungeonClearEndedRepeat = endedRepeat;
            lastDungeonClearContinuesRepeat = continuesRepeat;
            dungeonClearResultSequence += 1;
        }
    }
}
