using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public static partial class GameData
    {
        private static Dictionary<string, HeroDefinition> BuildHeroMap()
        {
            var map = new Dictionary<string, HeroDefinition>();
            foreach (HeroDefinition hero in heroes)
            {
                map[hero.Id] = hero;
            }

            return map;
        }

        private static Dictionary<string, EquipmentDefinition> BuildEquipmentMap()
        {
            var map = new Dictionary<string, EquipmentDefinition>();
            foreach (EquipmentDefinition equipment in equipments)
            {
                map[equipment.Id] = equipment;
            }

            return map;
        }

        private static Dictionary<string, TotemDefinition> BuildTotemMap()
        {
            var map = new Dictionary<string, TotemDefinition>();
            foreach (TotemDefinition totem in totems)
            {
                map[totem.Id] = totem;
            }

            return map;
        }

        private static Dictionary<string, RuneDefinition> BuildRuneMap()
        {
            var map = new Dictionary<string, RuneDefinition>();
            foreach (RuneDefinition rune in runes)
            {
                map[rune.Id] = rune;
            }

            return map;
        }

        private static FacilityDefinition[] BuildFacilities()
        {
            return new[]
            {
                new FacilityDefinition("FAC_REQUEST", "의뢰소", "G", FacilityRewardKind.Gold, "골드", GameNumber.FromDouble(1200d)),
                new FacilityDefinition("FAC_TRAINING", "훈련소", "EXP", FacilityRewardKind.HeroExpItem, "영웅 경험치책", GameNumber.FromDouble(240d)),
                new FacilityDefinition("FAC_FORGE", "대장간", "EQ", FacilityRewardKind.EquipmentExpItem, "장비책", GameNumber.FromDouble(180d)),
                new FacilityDefinition("FAC_TOTEM", "토템 제단", "T", FacilityRewardKind.TotemEssence, "토템 정수", GameNumber.FromDouble(40d)),
                new FacilityDefinition("FAC_RUNE", "룬 공방", "R", FacilityRewardKind.RuneBox, "룬 상자", GameNumber.FromDouble(4d)),
                new FacilityDefinition("FAC_TRANSCEND", "초월 연구소", "TR", FacilityRewardKind.HeroTranscendStone, "초월석", GameNumber.FromDouble(12d))
            };
        }

        private static Dictionary<string, FacilityDefinition> BuildFacilityMap()
        {
            var map = new Dictionary<string, FacilityDefinition>();
            foreach (FacilityDefinition facility in facilities)
            {
                map[facility.Id] = facility;
            }

            return map;
        }
    }
}
