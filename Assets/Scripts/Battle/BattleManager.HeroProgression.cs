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
        public bool TryLevelUpHero(string heroId)
        {
            HeroState hero = FindHero(heroId);
            bool leveledUp = HeroProgressionService.TryLevelUpHero(hero, wallet, out string battleLog);
            return ApplyLoggedChange(
                leveledUp,
                battleLog,
                () =>
                {
                    SaveHeroState(hero, true);
                    NotifyChanged(BattleChangeFlags.HeroProgression);
                });
        }

        public void AddHeroShards(string heroId, int amount)
        {
            HeroState hero = FindHero(heroId);
            if (!HeroProgressionService.AddHeroShards(hero, amount))
            {
                return;
            }

            SaveHeroState(hero, true);
            NotifyChanged(BattleChangeFlags.HeroProgression);
        }

        public bool TryStarUpHero(string heroId)
        {
            HeroState hero = FindHero(heroId);
            bool starredUp = HeroProgressionService.TryStarUpHero(hero, out string battleLog);
            return ApplyLoggedChange(
                starredUp,
                battleLog,
                () =>
                {
                    SaveHeroState(hero, true);
                    NotifyChanged(BattleChangeFlags.HeroProgression);
                });
        }

        public bool TryRollHeroTranscendOption(string heroId, int slotIndex, bool advanced, out HeroTranscendOptionDefinition option)
        {
            HeroState hero = FindHero(heroId);
            bool rolled = HeroProgressionService.TryRollTranscendOption(
                hero,
                slotIndex,
                advanced,
                GameData.RollHeroTranscendOption,
                out option,
                out string battleLog);
            return ApplyLoggedChange(
                rolled,
                battleLog,
                () =>
                {
                    SaveHeroTranscendOption(hero, slotIndex, true);
                    NotifyChanged(BattleChangeFlags.HeroProgression);
                });
        }

        public int BulkStarUpHeroes()
        {
            if (!IsReady())
            {
                return 0;
            }

            var changedHeroes = new List<HeroState>();
            int totalStarUps = HeroProgressionService.BulkStarUpHeroes(heroes, changedHeroes, out string battleLog);
            if (totalStarUps > 0)
            {
                SaveHeroStates(changedHeroes, true);
            }

            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.HeroProgression);
            return totalStarUps;
        }

        public void DebugLevelAllHeroes(int levels)
        {
            if (!IsReady() || levels <= 0)
            {
                return;
            }

            var changedHeroes = new List<HeroState>();
            int changedCount = HeroProgressionService.DebugLevelAllHeroes(heroes, levels, changedHeroes, out string battleLog);
            if (changedCount <= 0)
            {
                return;
            }

            SaveHeroStates(changedHeroes, true);
            ApplyBattleLog(battleLog);
            NotifyChanged(BattleChangeFlags.HeroProgression);
        }
    }
}
