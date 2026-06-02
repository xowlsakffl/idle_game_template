using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class HeroProgressionService
    {
        public static bool TryLevelUpHero(HeroState hero, CurrencyWallet wallet, out string battleLog)
        {
            battleLog = string.Empty;
            if (hero == null)
            {
                return false;
            }

            if (!hero.IsOwned)
            {
                battleLog = hero.Definition.DisplayName + " 레벨업 실패: 미보유 영웅";
                return false;
            }

            if (hero.Level >= hero.MaxLevel)
            {
                battleLog = hero.Definition.DisplayName + " 레벨업 실패: 현재 성급 최대 레벨";
                return false;
            }

            if (wallet == null || !wallet.SpendHeroExpItem(hero.LevelUpCost))
            {
                battleLog = hero.Definition.DisplayName + " 레벨업 실패: EXP 아이템 부족";
                return false;
            }

            hero.Level += 1;
            battleLog = hero.Definition.DisplayName + " Lv." + hero.Level + " 달성";
            return true;
        }

        public static bool AddHeroShards(HeroState hero, int amount)
        {
            if (hero == null || amount <= 0)
            {
                return false;
            }

            hero.Shards += amount;
            return true;
        }

        public static bool TryStarUpHero(HeroState hero, out string battleLog)
        {
            battleLog = string.Empty;
            if (hero == null || hero.IsMaxStars)
            {
                return false;
            }

            int cost = hero.StarUpCost;
            if (hero.Shards < cost)
            {
                battleLog = hero.Definition.DisplayName + " 성급업 실패: 조각 부족";
                return false;
            }

            hero.Shards -= cost;
            hero.Stars += 1;
            battleLog = hero.Definition.DisplayName + " 성급 " + hero.Stars + "/" + HeroDefinition.MaxStars;
            return true;
        }

        public static bool TryRollTranscendOption(
            HeroState hero,
            int slotIndex,
            bool advanced,
            Func<HeroDefinition, bool, HeroTranscendOptionDefinition> rollOption,
            out HeroTranscendOptionDefinition option,
            out string battleLog)
        {
            option = null;
            battleLog = string.Empty;
            if (hero == null || slotIndex < 0 || slotIndex >= HeroDefinition.MaxTranscendSlots)
            {
                return false;
            }

            if (!hero.IsTranscendSlotUnlocked(slotIndex))
            {
                battleLog = hero.Definition.DisplayName + " 초월 실패: " + HeroDefinition.GetTranscendRequiredStars(slotIndex) + "성 필요";
                return false;
            }

            option = rollOption?.Invoke(hero.Definition, advanced);
            if (option == null)
            {
                battleLog = hero.Definition.DisplayName + " 초월 실패: 옵션 없음";
                return false;
            }

            hero.SetTranscendOptionId(slotIndex, option.Id);
            battleLog = hero.Definition.DisplayName + " 초월 " + (slotIndex + 1) + "번: " + option.Grade + " " + option.Description;
            return true;
        }

        public static int BulkStarUpHeroes(
            IReadOnlyList<HeroState> heroes,
            List<HeroState> changedHeroes,
            out string battleLog)
        {
            battleLog = string.Empty;
            changedHeroes?.Clear();
            if (heroes == null)
            {
                battleLog = "일괄 승급 실패: 승급 가능한 영웅 없음";
                return 0;
            }

            int totalStarUps = 0;
            int affectedHeroes = 0;
            foreach (HeroState hero in heroes)
            {
                if (hero == null)
                {
                    continue;
                }

                int heroStarUps = 0;
                while (hero.CanStarUp)
                {
                    int cost = hero.StarUpCost;
                    if (cost <= 0)
                    {
                        break;
                    }

                    hero.Shards -= cost;
                    hero.Stars += 1;
                    heroStarUps += 1;
                    totalStarUps += 1;
                }

                if (heroStarUps > 0)
                {
                    affectedHeroes += 1;
                    changedHeroes?.Add(hero);
                }
            }

            battleLog = totalStarUps > 0
                ? "일괄 승급: " + affectedHeroes + "명, 총 " + totalStarUps + "성 상승"
                : "일괄 승급 실패: 승급 가능한 영웅 없음";
            return totalStarUps;
        }

        public static int DebugLevelAllHeroes(
            IReadOnlyList<HeroState> heroes,
            int levels,
            List<HeroState> changedHeroes,
            out string battleLog)
        {
            battleLog = string.Empty;
            changedHeroes?.Clear();
            if (heroes == null || levels <= 0)
            {
                return 0;
            }

            foreach (HeroState hero in heroes)
            {
                if (hero == null)
                {
                    continue;
                }

                hero.Level = Mathf.Min(hero.MaxLevel, hero.Level + levels);
                changedHeroes?.Add(hero);
            }

            battleLog = "QA: 모든 영웅 레벨 +" + levels;
            return changedHeroes != null ? changedHeroes.Count : heroes.Count;
        }
    }
}
