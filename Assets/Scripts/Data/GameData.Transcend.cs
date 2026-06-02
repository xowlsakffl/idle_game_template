using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public static partial class GameData
    {
        public static HeroTranscendOptionDefinition GetHeroTranscendOption(string id)
        {
            if (!string.IsNullOrEmpty(id) && heroTranscendOptionsById.TryGetValue(id, out HeroTranscendOptionDefinition option))
            {
                return option;
            }

            return null;
        }

        public static string GetDefaultHeroTranscendOptionId(HeroDefinition hero, int slotIndex)
        {
            return string.Empty;
        }

        public static HeroTranscendOptionDefinition RollHeroTranscendOption(HeroDefinition hero, bool advanced)
        {
            HeroTranscendOptionDefinition selectedOption = null;
            float totalWeight = 0f;

            for (int i = 0; i < heroTranscendOptions.Length; i++)
            {
                HeroTranscendOptionDefinition option = heroTranscendOptions[i];
                if (!IsHeroTranscendOptionEligible(hero, option, advanced))
                {
                    continue;
                }

                totalWeight += option.ProbabilityWeight;
            }

            if (totalWeight <= 0f && advanced)
            {
                return RollHeroTranscendOption(hero, false);
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float accumulated = 0f;
            for (int i = 0; i < heroTranscendOptions.Length; i++)
            {
                HeroTranscendOptionDefinition option = heroTranscendOptions[i];
                if (!IsHeroTranscendOptionEligible(hero, option, advanced))
                {
                    continue;
                }

                accumulated += option.ProbabilityWeight;
                if (roll <= accumulated)
                {
                    selectedOption = option;
                    break;
                }
            }

            return selectedOption;
        }
        private static Dictionary<string, HeroTranscendOptionDefinition> BuildHeroTranscendOptionMap()
        {
            var map = new Dictionary<string, HeroTranscendOptionDefinition>();
            foreach (HeroTranscendOptionDefinition option in heroTranscendOptions)
            {
                map[option.Id] = option;
            }

            return map;
        }

        private static HeroTranscendOptionDefinition[] BuildHeroTranscendOptions()
        {
            var options = new List<HeroTranscendOptionDefinition>
            {
                new HeroTranscendOptionDefinition("COMMON_ACCOUNT_EXP_F", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.F, "계정 경험치 획득량+1%", 20f),
                new HeroTranscendOptionDefinition("COMMON_GOLD_E", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.E, "골드 획득량+2%", 18f),
                new HeroTranscendOptionDefinition("COMMON_ATTACK_D", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.D, "공격력+3%", 16f),
                new HeroTranscendOptionDefinition("COMMON_HP_C", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.C, "체력+4%", 14f),
                new HeroTranscendOptionDefinition("COMMON_ASPD_B", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.B, "공격속도+5%", 10f),
                new HeroTranscendOptionDefinition("COMMON_FINAL_A", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.A, "최종 데미지+10%", 5f),
                new HeroTranscendOptionDefinition("COMMON_FINAL_S", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.S, "최종 데미지+20%", 2f),
                new HeroTranscendOptionDefinition("COMMON_FINAL_SS", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.SS, "최종 데미지+40%", 0.35f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_F", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.F, "5% 확률로 영웅 스킬 추가 1회 발동", 1.1658f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_E", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.E, "6% 확률로 영웅 스킬 추가 1회 발동", 1.04926f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_D", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.D, "7% 확률로 영웅 스킬 추가 1회 발동", 0.8161f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_C", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.C, "8% 확률로 영웅 스킬 추가 1회 발동", 0.5829f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_B", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.B, "9% 확률로 영웅 스킬 추가 1회 발동", 0.3498f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_A", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.A, "10% 확률로 영웅 스킬 추가 1회 발동", 0.2332f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_S", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.S, "20% 확률로 영웅 스킬 추가 1회 발동", 0.1166f),
                new HeroTranscendOptionDefinition("COMMON_EXTRA_SKILL_SS", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.SS, "30% 확률로 영웅 스킬 추가 1회 발동", 0.02915f),
                new HeroTranscendOptionDefinition("COMMON_STUN_A", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.A, "스킬 적중 시 10% 확률로 적 기절", 2.3317f),
                new HeroTranscendOptionDefinition("COMMON_STUN_S", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.S, "스킬 적중 시 20% 확률로 적 기절", 1.9819f),
                new HeroTranscendOptionDefinition("COMMON_STUN_SS", string.Empty, HeroTranscendOptionScope.Common, HeroTranscendGrade.SS, "스킬 적중 시 30% 확률로 적 기절", 0.02915f)
            };

            foreach (HeroDefinition hero in heroes)
            {
                string prefix = "EX_" + hero.Id + "_";
                options.Add(new HeroTranscendOptionDefinition(prefix + "HIT_A", hero.Id, HeroTranscendOptionScope.Exclusive, HeroTranscendGrade.A, "마구 때리기 타격횟수+1", 2.4483f));
                options.Add(new HeroTranscendOptionDefinition(prefix + "HIT_S", hero.Id, HeroTranscendOptionScope.Exclusive, HeroTranscendGrade.S, "마구 때리기 타격횟수+3", 1.9819f));
                options.Add(new HeroTranscendOptionDefinition(prefix + "HIT_SS", hero.Id, HeroTranscendOptionScope.Exclusive, HeroTranscendGrade.SS, "마구 때리기 타격횟수+5", 0.02915f));
            }

            return options.ToArray();
        }

        private static bool IsHeroTranscendOptionEligible(HeroDefinition hero, HeroTranscendOptionDefinition option, bool advanced)
        {
            if (option == null)
            {
                return false;
            }

            if (option.IsExclusive && (hero == null || option.HeroId != hero.Id))
            {
                return false;
            }

            return !advanced || option.Grade >= HeroTranscendGrade.A;
        }

    }
}
