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
        private void SaveAutoControlState()
        {
            saveManager.SaveBool(SaveKeys.SkillAutoEnabled, skillAutoEnabled);
            saveManager.SaveBool(SaveKeys.FeverAutoEnabled, feverAutoEnabled);
            saveManager.Flush();
        }

        private void ClearTargetLocks()
        {
            heroTargetSpawnSequences.Clear();
            skillTargetSpawnSequences.Clear();
            petTargetSpawnSequences.Clear();
        }

        private void ResetHeroDamageMeter()
        {
            BattleHeroRuntimeService.ResetDamageMeter(heroDamageMeter, deployedHeroes);
        }

        private void ResetBattleHeroRuntimeStates()
        {
            BattleHeroRuntimeService.ResetRuntimeStates(
                heroRuntimeStates,
                deployedHeroes,
                GetHeroBattleSlotPosition,
                CalculateHeroBattleMaxHp);
        }

        private void EnsureBattleHeroRuntimeStates()
        {
            BattleHeroRuntimeService.EnsureRuntimeStates(
                heroRuntimeStates,
                deployedHeroes,
                GetHeroBattleSlotPosition,
                CalculateHeroBattleMaxHp);
        }

        private bool IsHeroAlive(string heroId)
        {
            return BattleHeroRuntimeService.IsHeroAlive(heroRuntimeStates, heroId);
        }

        private static Vector2 GetHeroBattleSlotPosition(HeroState hero, int heroIndex)
        {
            return CombatMovementService.GetHeroBattleSlotPosition(hero, heroIndex);
        }

        private void AddHeroDamage(string heroId, GameNumber damage)
        {
            if (string.IsNullOrEmpty(heroId) || damage <= GameNumber.Zero)
            {
                return;
            }

            if (!heroDamageMeter.ContainsKey(heroId))
            {
                heroDamageMeter[heroId] = GameNumber.Zero;
            }

            heroDamageMeter[heroId] += damage;
        }

        private HeroState FindHero(string heroId)
        {
            if (heroes == null)
            {
                return null;
            }

            foreach (HeroState hero in heroes)
            {
                if (hero.Definition.Id == heroId)
                {
                    return hero;
                }
            }

            return null;
        }

        private void NotifyChanged()
        {
            NotifyChanged(BattleChangeFlags.All);
        }

        private void NotifyChanged(BattleChangeFlags flags)
        {
            ChangedWithFlags?.Invoke(flags);
        }

        private int GetCurrentAccountLevel()
        {
            return accountProgressManager != null ? accountProgressManager.Level : 1;
        }

        private bool IsReady()
        {
            return initialized
                && progressManager != null
                && wallet != null
                && saveManager != null
                && abilityManager != null
                && speedManager != null
                && heroes != null;
        }
    }
}
