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

    }
}
