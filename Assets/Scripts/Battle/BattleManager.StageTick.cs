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
        private void TickBattle(float deltaTime)
        {
            if (!IsReady())
            {
                return;
            }

            TickVisibleEnemySpawnGrace(deltaTime);
            TickBattleActorMovement(deltaTime);
            TickEnemyAttacks(deltaTime);
            int currentRunSequence = stageRunSequence;
            TickFortressAttack(deltaTime);
            if (stageRunSequence != currentRunSequence)
            {
                return;
            }

            if (!HasAttackableTarget())
            {
                TickBossTimer(deltaTime);
                return;
            }

            TickHeroes(deltaTime);
            if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
            {
                return;
            }

            TickSkills(deltaTime);
            if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
            {
                return;
            }

            TickPets(deltaTime);
            if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
            {
                return;
            }

            TickBossTimer(deltaTime);
        }

        private void TickBattleActorMovement(float deltaTime)
        {
            EnsureBattleHeroRuntimeStates();

            CombatMovementService.TickHeroMovement(
                deployedHeroes,
                heroRuntimeStates,
                visibleEnemies,
                heroTargetSpawnSequences,
                deltaTime,
                GetHeroBattleSlotPosition,
                GetHeroMoveSpeed,
                HeroSeparationRadius,
                FieldHalfWidth,
                FieldHalfHeight,
                0.15f);
            CombatMovementService.TickEnemyMovement(
                visibleEnemies,
                heroRuntimeStates.Values,
                fortressHp > GameNumber.Zero,
                IsBossFight,
                deltaTime,
                CombatMovementService.GetEnemyMoveSpeed,
                EnemyAttackRange,
                FortressEnemyAttackRange,
                EnemySeparationRadius,
                FieldHalfWidth,
                FieldHalfHeight);
        }

        private void TickEnemyAttacks(float deltaTime)
        {
            MonsterAttackService.TickEnemyAttacks(
                IsBossFight,
                visibleEnemies,
                heroRuntimeStates.Values,
                heroTargetSpawnSequences,
                ref fortressHp,
                FortressMaxHp,
                GetDamageTakenMultiplier(),
                deltaTime,
                EnemyAttackRange,
                FortressEnemyAttackRange,
                EnemyAttackIntervalSeconds,
                0.18f,
                HeroReviveSeconds,
                ApplyMonsterHitResult);
        }

        private void TickFortressAttack(float deltaTime)
        {
            CombatAttackAction attack = FortressAttackService.TickFortressAttack(
                fortressHp,
                HasAttackableTarget(),
                IsBossFight,
                visibleEnemies,
                ref fortressAttackCooldown,
                deltaTime,
                FortressAttackInterval,
                FortressAttackRange,
                FortressAttackPower,
                0.10f);
            if (!attack.IsValid)
            {
                return;
            }

            fortressAttackSequence += 1;
            ApplyAttack(attack);
        }

        private void ApplyMonsterHitResult(CombatHitService.MonsterHitResult hitResult)
        {
            string battleLog = hitFeedback.ApplyMonsterHitResult(hitResult);
            if (!string.IsNullOrEmpty(battleLog))
            {
                LastBattleLog = battleLog;
            }
        }

        private void TickHeroes(float deltaTime)
        {
            int currentRunSequence = stageRunSequence;
            HeroAttackTickService.TickHeroAttacks(
                deployedHeroes,
                readyHeroAttacks,
                recentHeroAttackIds,
                deltaTime,
                IsHeroAlive,
                hero => SelectVisibleEnemyIndexForHero(hero) >= 0,
                hero => hero.AttackInterval / (float)(GetTotemAttackSpeedMultiplier(hero) * GetRuneAttackSpeedMultiplier(hero)),
                () => stageRunSequence == currentRunSequence && HasAttackableTarget(),
                () => HeroAttackBatchSequence += 1,
                DealDamage);
        }

        private void TickBossTimer(float deltaTime)
        {
            StageCombatFlowService.BossTimerResult result = StageCombatFlowService.TickBossTimer(
                IsBossFight,
                TargetHp,
                BossTimeRemaining,
                progressManager.CurrentStageId,
                deltaTime);
            BossTimeRemaining = result.TimeRemaining;
            if (!result.Failed)
            {
                return;
            }

            progressManager.HandleBossFailed();
            LastBattleLog = result.BattleLog;
            NotifyChanged(BattleChangeFlags.Combat);
        }

        private void TickSkills(float deltaTime)
        {
            if (!skillAutoEnabled)
            {
                return;
            }

            CombatTickService.TickReadySkills(
                skills,
                deltaTime,
                HasAttackableTarget,
                (float)(GetTotemSkillCooldownMultiplier() * GetRuneSkillCooldownMultiplier()),
                CastSkill);
        }

        private void TickPets(float deltaTime)
        {
            CombatTickService.TickReadyPets(pets, deltaTime, HasAttackableTarget, AttackWithPet);
        }
    }
}
