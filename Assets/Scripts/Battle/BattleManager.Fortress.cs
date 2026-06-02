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
        public bool TryLevelUpFortress()
        {
            bool leveledUp = FortressCommandService.TryLevelUp(
                fortressLevel,
                fortressExperience,
                FortressMaxLevelValue,
                out int nextLevel,
                out string battleLog);
            return ApplyLoggedChange(
                leveledUp,
                battleLog,
                () =>
                {
                    SetFortressLevel(nextLevel);
                    SaveFortress();
                    StartStage(false);
                    NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.Fortress);
                });
        }

        public void DebugAddFortressExperience(GameNumber amount)
        {
            if (!FortressCommandService.TryDebugAddExperience(amount, out string battleLog))
            {
                return;
            }

            AddFortressExperience(amount);
            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.Fortress);
        }

        public void DebugLevelFortress(int levels)
        {
            if (!FortressCommandService.TryDebugLevel(
                    fortressLevel,
                    levels,
                    FortressMaxLevelValue,
                    out int nextLevel,
                    out string battleLog))
            {
                return;
            }

            SetFortressLevel(nextLevel);
            SaveFortress();
            ApplyBattleLog(battleLog);
            StartStage(false);
            NotifyChanged(BattleChangeFlags.Combat | BattleChangeFlags.Fortress);
        }

        private static GameNumber GetFortressRequiredExperienceForLevel(int level)
        {
            return FortressCombatService.GetRequiredExperienceForLevel(level, FortressMaxLevelValue);
        }

        private static GameNumber CalculateFortressMaxHp(int level)
        {
            return FortressCombatService.CalculateMaxHp(level, FortressMaxLevelValue);
        }

        private static GameNumber CalculateFortressAttackPower(int level)
        {
            return FortressCombatService.CalculateAttackPower(level, FortressMaxLevelValue);
        }

        private static float CalculateFortressAttackInterval(int level)
        {
            return FortressCombatService.CalculateAttackInterval(level, FortressMaxLevelValue);
        }

        private static float CalculateFortressAttackRange(int level)
        {
            return FortressCombatService.CalculateAttackRange(level, FortressMaxLevelValue);
        }

        private static double CalculateFortressCombatPower(int level)
        {
            return FortressCombatService.CalculateCombatPower(level, FortressMaxLevelValue);
        }

        private void LoadFortress()
        {
            BattleStatePersistenceService.FortressSaveState state = BattleStatePersistenceService.LoadFortress(saveManager, FortressMaxLevelValue);
            fortressLevel = state.Level;
            fortressExperience = state.Experience;
            fortressHp = FortressMaxHp;
            fortressAttackCooldown = Mathf.Min(FortressAttackInterval, 0.18f);
        }

        private void SaveFortress()
        {
            BattleStatePersistenceService.SaveFortress(saveManager, fortressLevel, fortressExperience, true);
        }

        private void SetFortressLevel(int level)
        {
            fortressLevel = Mathf.Clamp(level, 1, FortressMaxLevelValue);
            fortressHp = FortressMaxHp;
            fortressAttackCooldown = Mathf.Min(FortressAttackInterval, 0.18f);
        }

        private void AddFortressExperience(GameNumber amount)
        {
            if (amount <= GameNumber.Zero)
            {
                return;
            }

            fortressExperience = GameData.ClampNumber(fortressExperience + GameNumber.Floor(amount));
            BattleStatePersistenceService.SaveFortress(saveManager, fortressLevel, fortressExperience, false);
        }
    }
}
