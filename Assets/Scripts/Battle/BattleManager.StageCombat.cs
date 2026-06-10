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
        public void DebugSimulateSeconds(float seconds, float stepSeconds = 0.1f)
        {
            if (!IsReady() || seconds <= 0f)
            {
                return;
            }

            float remaining = seconds;
            float step = Mathf.Clamp(stepSeconds, 0.02f, 1f);
            while (remaining > 0f)
            {
                float delta = Mathf.Min(step, remaining);
                TickBattle(delta);
                remaining -= delta;
            }
        }

        private void StartStage()
        {
            StartStage(true);
        }

        private void StartStage(bool notifyCombatChanged)
        {
            if (!IsReady())
            {
                return;
            }

            if (dungeonRunActive)
            {
                return;
            }

            StageRuntimeResetService.ResetCombatantCooldowns(
                deployedHeroes,
                skills,
                pets,
                InitialEnemySpawnGraceSeconds,
                GetTotemSkillCooldownMultiplier() * GetRuneSkillCooldownMultiplier(),
                FortressAttackInterval,
                out fortressAttackCooldown);
            fortressHp = FortressMaxHp;
            stageRunSequence += 1;
            ResetHeroDamageMeter();
            ResetBattleHeroRuntimeStates();
            KillsThisStage = 0;
            nextEnemySpawnSequence = 0;
            hitFeedback.ResetStage();
            visibleEnemies.Clear();
            ClearTargetLocks();
            SpawnTarget(notifyCombatChanged);
        }

        public bool TryEnterDungeon(DungeonKind kind, int level, bool repeat)
        {
            if (dungeonRepeatStartCoroutine != null)
            {
                StopCoroutine(dungeonRepeatStartCoroutine);
                dungeonRepeatStartCoroutine = null;
            }

            if (!IsReady() || dungeonProgressManager == null)
            {
                return false;
            }

            int normalizedLevel = dungeonProgressManager.ClampSelectableLevel(kind, level);
            if (!dungeonProgressManager.TryConsumeEntry(kind, normalizedLevel, out DungeonEntryReceipt receipt))
            {
                LastBattleLog = "던전 입장 실패: 무료 횟수와 던전 티켓 부족";
                NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.BattleLog);
                return false;
            }

            activeDungeonKind = kind;
            activeDungeonLevel = normalizedLevel;
            activeDungeonRepeat = repeat;
            activeDungeonStartedWithRepeat = repeat;
            activeDungeonReceipt = receipt;
            dungeonRunActive = true;
            dungeonRepeatWaitingForNextRun = false;
            StartDungeonRun();
            return true;
        }

        private void StartDungeonRun()
        {
            dungeonRepeatWaitingForNextRun = false;
            StageRuntimeResetService.ResetCombatantCooldowns(
                deployedHeroes,
                skills,
                pets,
                InitialEnemySpawnGraceSeconds,
                GetTotemSkillCooldownMultiplier() * GetRuneSkillCooldownMultiplier(),
                FortressAttackInterval,
                out fortressAttackCooldown);
            fortressHp = FortressMaxHp;
            stageRunSequence += 1;
            ResetHeroDamageMeter();
            ResetBattleHeroRuntimeStates();
            KillsThisStage = 0;
            nextEnemySpawnSequence = 0;
            hitFeedback.ResetStage();
            visibleEnemies.Clear();
            ClearTargetLocks();
            if (activeDungeonKind == DungeonKind.TotemEssence)
            {
                StartTotemBossDungeonRun();
            }
            else
            {
                SpawnDungeonNormalWave();
            }

            NotifyChanged(BattleChangeFlags.Combat);
        }

        private void StartTotemBossDungeonRun()
        {
            KillsThisStage = 0;
            RequiredKills = 0;
            activeDungeonLevel = 1;
            BossTimeRemaining = DungeonProgressManager.GetTimeLimitSeconds(activeDungeonKind);
            SpawnTotemDungeonBoss();
            LastBattleLog = "토템석 던전 입장: 보스 Lv.1부터 연속 처치";
        }

        private void SpawnDungeonNormalWave()
        {
            IsBossFight = false;
            RequiredKills = DungeonProgressManager.RequiredNormalKills;
            TargetName = DungeonProgressManager.GetTitle(activeDungeonKind) + " Lv." + activeDungeonLevel;
            TargetMaxHp = GetDungeonEnemyHp(activeDungeonLevel);
            BossTimeRemaining = DungeonProgressManager.GetTimeLimitSeconds(activeDungeonKind);
            visibleEnemies.Clear();
            FillVisibleEnemies();
            SyncTargetFromVisibleEnemies();
            LastBattleLog = TargetName + " 입장: 몬스터 " + RequiredKills + "마리 처치";
        }

        private void SpawnDungeonBoss()
        {
            IsBossFight = true;
            TargetName = DungeonProgressManager.GetTitle(activeDungeonKind) + " 보스 Lv." + activeDungeonLevel;
            TargetMaxHp = GetDungeonBossHp(activeDungeonLevel);
            TargetHp = TargetMaxHp;
            VisibleEnemyCount = 1;
            visibleEnemies.Clear();
            ClearTargetLocks();
            LastBattleLog = "보스 출현: 남은 시간 안에 처치";
            NotifyChanged(BattleChangeFlags.Combat);
        }

        private void SpawnTotemDungeonBoss()
        {
            IsBossFight = true;
            TargetName = "토템석 보스 Lv." + activeDungeonLevel;
            TargetMaxHp = GetTotemDungeonBossHp(activeDungeonLevel);
            TargetHp = TargetMaxHp;
            VisibleEnemyCount = 1;
            visibleEnemies.Clear();
            ClearTargetLocks();
        }

        private void SpawnTarget(bool notifyCombatChanged = true)
        {
            StageDefinition stage = progressManager.CurrentStage;
            StageTargetSetupService.StageTargetSetup setup = StageTargetSetupService.Build(stage);
            IsBossFight = setup.IsBossFight;
            RequiredKills = setup.RequiredKills;
            TargetName = setup.TargetName;
            TargetMaxHp = setup.TargetMaxHp;
            BossTimeRemaining = setup.BossTimeRemaining;

            if (IsBossFight)
            {
                visibleEnemies.Clear();
                TargetHp = setup.TargetHp;
                VisibleEnemyCount = setup.VisibleEnemyCount;
            }
            else
            {
                visibleEnemies.Clear();
                FillVisibleEnemies();
                SyncTargetFromVisibleEnemies();
            }

            LastBattleLog = setup.BattleLog;
            if (notifyCombatChanged)
            {
                NotifyChanged(BattleChangeFlags.Combat);
            }
        }

        private void FillVisibleEnemies()
        {
            if (IsBossFight)
            {
                return;
            }

            MonsterSpawnService.FillVisibleEnemies(
                visibleEnemies,
                ref nextEnemySpawnSequence,
                RequiredKills,
                TargetMaxHp,
                InitialEnemySpawnGraceSeconds,
                EnemyAttackIntervalSeconds,
                FieldHalfWidth,
                FieldHalfHeight,
                GameData.MaxVisibleEnemies);
            VisibleEnemyCount = visibleEnemies.Count;
        }

        private void SyncTargetFromVisibleEnemies()
        {
            if (IsBossFight)
            {
                return;
            }

            VisibleEnemyCount = visibleEnemies.Count;
            if (visibleEnemies.Count <= 0)
            {
                TargetHp = GameNumber.Zero;
                return;
            }

            TargetMaxHp = visibleEnemies[0].MaxHp;
            TargetHp = visibleEnemies[0].Hp;
        }

        private bool HasAttackableTarget()
        {
            return IsBossFight ? TargetHp > GameNumber.Zero : FindFirstAttackableVisibleEnemyIndex() >= 0;
        }

        private void TickVisibleEnemySpawnGrace(float deltaTime)
        {
            CombatTickService.TickVisibleEnemySpawnGrace(IsBossFight, visibleEnemies, deltaTime);
        }

        private static GameNumber GetDungeonEnemyHp(int level)
        {
            int normalizedLevel = Mathf.Max(1, level);
            return GameNumber.FromDouble(38d + normalizedLevel * 14d);
        }

        private static GameNumber GetDungeonBossHp(int level)
        {
            int normalizedLevel = Mathf.Max(1, level);
            return GameNumber.FromDouble(1400d + normalizedLevel * 420d);
        }

        private static GameNumber GetTotemDungeonBossHp(int level)
        {
            int normalizedLevel = Mathf.Max(1, level);
            return GetTotemBossHpTotal(normalizedLevel, normalizedLevel);
        }

        private static int CountDefeatedTotemBosses(int startLevel, GameNumber damage)
        {
            if (damage <= GameNumber.Zero)
            {
                return 0;
            }

            int normalizedStart = Mathf.Clamp(startLevel, 1, int.MaxValue);
            int low = normalizedStart;
            int high = int.MaxValue;
            int best = normalizedStart - 1;
            while (low <= high)
            {
                int mid = low + (int)(((long)high - low) / 2L);
                GameNumber requiredDamage = GetTotemBossHpTotal(normalizedStart, mid);
                if (requiredDamage <= damage)
                {
                    best = mid;
                    if (mid == int.MaxValue)
                    {
                        break;
                    }

                    low = mid + 1;
                    continue;
                }

                high = mid - 1;
            }

            return Mathf.Max(0, best - normalizedStart + 1);
        }

        private static GameNumber GetTotemBossHpTotal(int startLevel, int endLevel)
        {
            if (endLevel < startLevel)
            {
                return GameNumber.Zero;
            }

            double start = Math.Max(1d, startLevel);
            double end = Math.Max(start, endLevel);
            double beforeStart = start - 1d;
            double count = end - start + 1d;
            double levelSum = SumLinear(end) - SumLinear(beforeStart);
            double squareSum = SumSquares(end) - SumSquares(beforeStart);
            return GameNumber.FromDouble(squareSum * 8d + levelSum * 220d + count * 600d);
        }

        private static double SumLinear(double value)
        {
            return value * (value + 1d) * 0.5d;
        }

        private static double SumSquares(double value)
        {
            return value * (value + 1d) * (2d * value + 1d) / 6d;
        }

    }
}
