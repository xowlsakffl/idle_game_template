using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public static class GameData
    {
        public const string FirstStageId = "1-1";
        public const string ChapterOneBossStageId = "1-20";
        public const string BossFallbackStageId = "1-19";
        public const int NormalStageRequiredKills = 100;
        public const int MaxVisibleEnemies = 12;
        public const int MaxPartyHeroes = 8;
        public const int MaxHeroPresets = 3;
        public const int MaxTotemSlots = 2;
        public const int InitialUnlockedTotemSlots = 1;
        public const int MaxRuneSlots = 4;
        public const int StagesPerChapter = 20;
        public const int MaxIntBalanceValue = int.MaxValue / 4;
        private const double NormalStageHpGrowth = 1.055d;
        private const double NormalStageGoldGrowth = 1.032d;
        private const double GeneratedStageHpBase = 26d;
        private const double GeneratedStageGoldBase = 3d;
        private const double GeneratedStageHpOffset = 2d;
        private const double GeneratedStageGoldOffset = 1d;
        private const double BossHpMultiplier = 8d;
        private const double BossGoldMultiplier = 8d;

        private static readonly HeroDefinition[] heroes =
        {
            new HeroDefinition("H001", "기사 아렌", "균형형", HeroRarity.Uncommon, HeroTrait.Melee, 10, 130, 0.85f, 3.4f, 5, 26, HeroPassiveStat.AttackPower, 8f),
            new HeroDefinition("H002", "궁수 리나", "빠른 공격", HeroRarity.Rare, HeroTrait.Ranged, 7, 92, 1.25f, 3.9f, 3, 18, HeroPassiveStat.AttackSpeed, 10f),
            new HeroDefinition("H003", "마법사 노아", "강한 한방", HeroRarity.Epic, HeroTrait.Ranged, 18, 80, 0.56f, 3.0f, 8, 16, HeroPassiveStat.AttackPower, 15f),
            new HeroDefinition("H004", "성기사 카일", "안정형", HeroRarity.Legendary, HeroTrait.Defense, 12, 190, 0.72f, 2.8f, 6, 38, HeroPassiveStat.MaxHp, 18f),
            new HeroDefinition("H005", "도적 세라", "연속 공격", HeroRarity.Mythic, HeroTrait.Melee, 6, 105, 1.82f, 4.6f, 2, 20, HeroPassiveStat.MoveSpeed, 14f),
            new HeroDefinition("H006", "사제 미나", "회복 지원", HeroRarity.Rare, HeroTrait.Support, 5, 115, 0.67f, 3.2f, 3, 24, HeroPassiveStat.MaxHp, 10f),
            new HeroDefinition("H007", "방패 로크", "전열 방어", HeroRarity.Epic, HeroTrait.Defense, 14, 230, 0.63f, 2.5f, 5, 42, HeroPassiveStat.MaxHp, 15f),
            new HeroDefinition("H008", "정령 루미", "보조 공격", HeroRarity.Legendary, HeroTrait.Support, 9, 120, 1.00f, 3.7f, 4, 22, HeroPassiveStat.AttackSpeed, 12f),
            new HeroDefinition("H009", "창병 토르", "돌격형", HeroRarity.Uncommon, HeroTrait.Melee, 11, 145, 0.92f, 3.2f, 5, 28, HeroPassiveStat.AttackPower, 7f),
            new HeroDefinition("H010", "폭탄 몰리", "광역 견제", HeroRarity.Rare, HeroTrait.Ranged, 13, 86, 0.74f, 3.1f, 6, 17, HeroPassiveStat.AttackPower, 11f),
            new HeroDefinition("H011", "드루이드 엘린", "자연 지원", HeroRarity.Epic, HeroTrait.Support, 8, 140, 0.88f, 3.5f, 4, 27, HeroPassiveStat.MaxHp, 13f),
            new HeroDefinition("H012", "철벽 브론", "피해 흡수", HeroRarity.Uncommon, HeroTrait.Defense, 9, 250, 0.52f, 2.3f, 4, 46, HeroPassiveStat.MaxHp, 9f),
            new HeroDefinition("H013", "검무 유리", "치명 연계", HeroRarity.Legendary, HeroTrait.Melee, 16, 118, 1.38f, 4.1f, 6, 23, HeroPassiveStat.AttackSpeed, 14f),
            new HeroDefinition("H014", "저격수 베른", "단일 저격", HeroRarity.Epic, HeroTrait.Ranged, 22, 74, 0.48f, 2.9f, 9, 15, HeroPassiveStat.AttackPower, 16f),
            new HeroDefinition("H015", "연금술사 포포", "물약 지원", HeroRarity.Rare, HeroTrait.Support, 6, 128, 0.78f, 3.3f, 3, 25, HeroPassiveStat.MoveSpeed, 8f),
            new HeroDefinition("H016", "수호자 제드", "후열 보호", HeroRarity.Rare, HeroTrait.Defense, 10, 210, 0.66f, 2.7f, 5, 39, HeroPassiveStat.MaxHp, 12f),
            new HeroDefinition("H017", "화염 이프", "폭발 화력", HeroRarity.Mythic, HeroTrait.Ranged, 25, 95, 0.70f, 3.4f, 10, 19, HeroPassiveStat.AttackPower, 20f),
            new HeroDefinition("H018", "음유시인 라온", "공속 지원", HeroRarity.Uncommon, HeroTrait.Support, 5, 118, 1.05f, 3.8f, 3, 23, HeroPassiveStat.AttackSpeed, 7f),
            new HeroDefinition("H019", "망치 그룬", "둔중한 일격", HeroRarity.Epic, HeroTrait.Melee, 20, 165, 0.50f, 2.6f, 8, 32, HeroPassiveStat.AttackPower, 13f),
            new HeroDefinition("H020", "그림자 렌", "기습 암살", HeroRarity.Legendary, HeroTrait.Melee, 12, 102, 1.68f, 4.8f, 4, 20, HeroPassiveStat.MoveSpeed, 18f),
            new HeroDefinition("H021", "견습 기사 딘", "기본 전열", HeroRarity.Common, HeroTrait.Melee, 7, 118, 0.80f, 3.0f, 3, 22, HeroPassiveStat.AttackPower, 4f, true),
            new HeroDefinition("H022", "마을 궁수 봄", "기본 사격", HeroRarity.Common, HeroTrait.Ranged, 6, 82, 1.02f, 3.4f, 2, 16, HeroPassiveStat.AttackSpeed, 4f, true),
            new HeroDefinition("H023", "초보 사제 나리", "기본 지원", HeroRarity.Common, HeroTrait.Support, 4, 104, 0.62f, 3.0f, 2, 20, HeroPassiveStat.MaxHp, 5f, true),
            new HeroDefinition("H024", "나무방패 폴", "기본 방어", HeroRarity.Common, HeroTrait.Defense, 6, 175, 0.50f, 2.4f, 3, 33, HeroPassiveStat.MaxHp, 6f, true),
            new HeroDefinition("H025", "새총 피코", "빠른 견제", HeroRarity.Common, HeroTrait.Ranged, 5, 76, 1.20f, 3.7f, 2, 15, HeroPassiveStat.MoveSpeed, 4f, true),
            new HeroDefinition("H026", "수련생 로나", "보조 전투", HeroRarity.Common, HeroTrait.Support, 5, 96, 0.90f, 3.2f, 2, 18, HeroPassiveStat.AttackSpeed, 5f, true),
            new HeroDefinition("H027", "쌍검 마루", "근접 연타", HeroRarity.Uncommon, HeroTrait.Melee, 9, 122, 1.18f, 3.9f, 4, 24, HeroPassiveStat.AttackSpeed, 8f),
            new HeroDefinition("H028", "돌갑옷 바크", "전열 유지", HeroRarity.Uncommon, HeroTrait.Defense, 8, 215, 0.56f, 2.2f, 4, 41, HeroPassiveStat.MaxHp, 8f),
            new HeroDefinition("H029", "빙결 소녀 아이샤", "감속 사격", HeroRarity.Rare, HeroTrait.Ranged, 12, 90, 0.92f, 3.2f, 5, 18, HeroPassiveStat.AttackPower, 10f),
            new HeroDefinition("H030", "태엽 의무병 코코", "기계 지원", HeroRarity.Epic, HeroTrait.Support, 10, 150, 0.82f, 3.1f, 5, 29, HeroPassiveStat.MaxHp, 14f),
            new HeroDefinition("H031", "번개 창 아스", "돌파 공격", HeroRarity.Legendary, HeroTrait.Melee, 18, 132, 1.12f, 4.2f, 7, 26, HeroPassiveStat.AttackPower, 17f),
            new HeroDefinition("H032", "성녀 이리스", "고급 지원", HeroRarity.Legendary, HeroTrait.Support, 11, 170, 0.76f, 3.4f, 5, 34, HeroPassiveStat.AttackSpeed, 16f),
            new HeroDefinition("H033", "용기사 라그", "신화 돌격", HeroRarity.Mythic, HeroTrait.Melee, 28, 180, 0.95f, 3.8f, 11, 36, HeroPassiveStat.AttackPower, 22f),
            new HeroDefinition("H034", "별마녀 셀린", "신화 원거리", HeroRarity.Mythic, HeroTrait.Ranged, 30, 105, 0.86f, 3.5f, 12, 21, HeroPassiveStat.AttackPower, 24f),
            new HeroDefinition("H035", "불멸 수호자 오르", "신화 방어", HeroRarity.Mythic, HeroTrait.Defense, 18, 320, 0.60f, 2.8f, 8, 58, HeroPassiveStat.MaxHp, 24f),
            new HeroDefinition("H036", "시간술사 네브", "신화 지원", HeroRarity.Mythic, HeroTrait.Support, 16, 160, 1.05f, 3.9f, 7, 32, HeroPassiveStat.AttackSpeed, 22f),
            new HeroDefinition("H037", "천공검 아리아", "신화 연격", HeroRarity.Mythic, HeroTrait.Melee, 26, 145, 1.32f, 4.4f, 10, 29, HeroPassiveStat.AttackSpeed, 23f),
            new HeroDefinition("H038", "공허 현자 벨", "신화 증폭", HeroRarity.Mythic, HeroTrait.Support, 18, 185, 0.88f, 3.6f, 8, 37, HeroPassiveStat.MaxHp, 21f)
        };

        private static readonly EnemyDefinition[] enemies =
        {
            new EnemyDefinition("E001", "슬라임", 30, 5),
            new EnemyDefinition("E002", "고블린", 70, 9),
            new EnemyDefinition("E003", "늑대", 140, 15),
            new EnemyDefinition("E004", "골렘", 260, 25),
            new EnemyDefinition("E005", "악마병", 480, 40)
        };

        private static readonly BossDefinition[] bosses =
        {
            new BossDefinition("B001", "타락한 기사", 9000, 30f, 500)
        };

        private static readonly StageDefinition[] stages =
        {
            Normal("1-1", 1, "E001", 1.00f, 1.00f),
            Normal("1-2", 2, "E001", 1.30f, 1.20f),
            Normal("1-3", 3, "E001", 1.70f, 1.45f),
            Normal("1-4", 4, "E001", 2.20f, 1.75f),
            Normal("1-5", 5, "E002", 1.00f, 1.00f),
            Normal("1-6", 6, "E002", 1.35f, 1.25f),
            Normal("1-7", 7, "E002", 1.80f, 1.55f),
            Normal("1-8", 8, "E002", 2.35f, 1.90f),
            Normal("1-9", 9, "E003", 1.00f, 1.00f),
            Normal("1-10", 10, "E003", 1.40f, 1.30f),
            Normal("1-11", 11, "E003", 1.90f, 1.65f),
            Normal("1-12", 12, "E003", 2.50f, 2.05f),
            Normal("1-13", 13, "E004", 1.00f, 1.00f),
            Normal("1-14", 14, "E004", 1.45f, 1.35f),
            Normal("1-15", 15, "E004", 2.00f, 1.75f),
            Normal("1-16", 16, "E004", 2.65f, 2.20f),
            Normal("1-17", 17, "E005", 1.00f, 1.00f),
            Normal("1-18", 18, "E005", 1.50f, 1.40f),
            Normal("1-19", 19, "E005", 2.10f, 1.85f),
            new StageDefinition("1-20", 1, 20, StageType.Boss, "B001", 1.00f, 1.00f, 1, BossFallbackStageId)
        };

        private static readonly AbilityDefinition[] abilities =
        {
            new AbilityDefinition(AbilityKind.AttackPower, "공격력 증가", "최대 +3,000", 3d, 0d, 1000, 10, 1.12f, AbilityDisplayKind.Flat),
            new AbilityDefinition(AbilityKind.MaxHp, "체력 증가", "최대 +18,000", 18d, 0d, 1000, 12, 1.12f, AbilityDisplayKind.Flat),
            new AbilityDefinition(AbilityKind.CriticalChance, "치명타 확률 증가", "최대 50.0%", 0.1d, 0d, 500, 120, 1.28f, AbilityDisplayKind.Percent),
            new AbilityDefinition(AbilityKind.CriticalDamage, "치명타 데미지 증가", "기본 5.0% + 레벨당 0.2%", 0.2d, 5d, 1000, 18, 1.10f, AbilityDisplayKind.Percent),
            new AbilityDefinition(AbilityKind.DoubleCriticalChance, "더블 치명타 확률 증가", "최대 50.0%", 0.1d, 0d, 500, 180, 1.30f, AbilityDisplayKind.Percent),
            new AbilityDefinition(AbilityKind.DoubleCriticalBonusDamage, "더블 치명타 추가 데미지 증가", "기본 5.0% + 레벨당 0.2%", 0.2d, 5d, 1000, 24, 1.10f, AbilityDisplayKind.Percent),
            new AbilityDefinition(AbilityKind.FinalDamage, "최종 데미지 증가", "최대 +100.0%", 0.1d, 0d, 1000, 32, 1.12f, AbilityDisplayKind.Percent)
        };

        private static readonly CombatSkillDefinition[] skills =
        {
            new CombatSkillDefinition("S001", "유성 낙하", 8f, 1.8f),
            new CombatSkillDefinition("S002", "전장의 함성", 14f, 1.2f)
        };

        private static readonly PetDefinition[] pets =
        {
            new PetDefinition("P001", "여우 루루", 8, 1.5f, 0.05f)
        };

        private static readonly TotemDefinition[] totems =
        {
            new TotemDefinition("TOTEM_COMBAT", "전투 토템", "⚔", "공격력, 치명타, 보스 피해", TotemArchetype.Combat),
            new TotemDefinition("TOTEM_GUARDIAN", "수호 토템", "◆", "체력, 피해 감소, 방어형 보너스", TotemArchetype.Guardian),
            new TotemDefinition("TOTEM_SUPPORT", "지원 토템", "✦", "골드, 경험치책, 계정 경험치", TotemArchetype.Support),
            new TotemDefinition("TOTEM_ARCANE", "비전 토템", "✹", "스킬 피해, 스킬 쿨타임, 자동 스킬", TotemArchetype.Arcane),
            new TotemDefinition("TOTEM_STORM", "폭풍 토템", "↯", "공속, 이속, 원거리 보너스", TotemArchetype.Storm),
            new TotemDefinition("TOTEM_COMMAND", "지휘 토템", "⚑", "파티 공격력, 체력, 스킬 피해", TotemArchetype.Command)
        };

        private static readonly RuneDefinition[] runes =
        {
            new RuneDefinition("RUNE_STRIKE", "검격 룬", "◇", "기본 공격 보정", RuneEffectKind.Strike),
            new RuneDefinition("RUNE_EXECUTE", "처형 룬", "◆", "최종 피해 보정", RuneEffectKind.Execute),
            new RuneDefinition("RUNE_BARRIER", "방벽 룬", "⬟", "체력과 피해 감소", RuneEffectKind.Barrier),
            new RuneDefinition("RUNE_HARVEST", "수확 룬", "✦", "골드와 경험치책 획득", RuneEffectKind.Harvest),
            new RuneDefinition("RUNE_ARCANE", "비전 룬", "✧", "스킬 피해와 쿨타임", RuneEffectKind.Arcane),
            new RuneDefinition("RUNE_STORM", "질풍 룬", "↯", "공속과 이속", RuneEffectKind.Storm),
            new RuneDefinition("RUNE_FOCUS", "정밀 룬", "◎", "치명타 확률", RuneEffectKind.Focus),
            new RuneDefinition("RUNE_VITALITY", "생명 룬", "✚", "파티 체력", RuneEffectKind.Vitality),
            new RuneDefinition("RUNE_COMMAND", "지휘 룬", "☷", "공격력과 계정 경험치", RuneEffectKind.Command),
            new RuneDefinition("RUNE_REGEN", "재생 룬", "♢", "지속 전투 안정성", RuneEffectKind.Regeneration)
        };

        private static readonly EquipmentDefinition[] equipments =
        {
            new EquipmentDefinition("EQ019", "연습용 검", EquipmentSlot.Weapon, HeroRarity.Common, 4, 0),
            new EquipmentDefinition("EQ020", "천 모자", EquipmentSlot.Hat, HeroRarity.Common, 0, 18),
            new EquipmentDefinition("EQ021", "수습 갑옷", EquipmentSlot.Armor, HeroRarity.Common, 0, 30),
            new EquipmentDefinition("EQ022", "낡은 부적", EquipmentSlot.Accessory, HeroRarity.Common, 2, 10),
            new EquipmentDefinition("EQ023", "작은 포션", EquipmentSlot.Potion, HeroRarity.Common, 1, 24),
            new EquipmentDefinition("EQ001", "낡은 검", EquipmentSlot.Weapon, HeroRarity.Uncommon, 8, 0),
            new EquipmentDefinition("EQ002", "가죽 모자", EquipmentSlot.Hat, HeroRarity.Uncommon, 0, 35),
            new EquipmentDefinition("EQ003", "천 갑옷", EquipmentSlot.Armor, HeroRarity.Uncommon, 0, 55),
            new EquipmentDefinition("EQ004", "작은 반지", EquipmentSlot.Accessory, HeroRarity.Uncommon, 4, 20),
            new EquipmentDefinition("EQ005", "초급 포션", EquipmentSlot.Potion, HeroRarity.Uncommon, 2, 45),
            new EquipmentDefinition("EQ006", "기사의 장검", EquipmentSlot.Weapon, HeroRarity.Rare, 18, 0),
            new EquipmentDefinition("EQ007", "마법 챙모자", EquipmentSlot.Hat, HeroRarity.Rare, 5, 85),
            new EquipmentDefinition("EQ008", "강철 갑옷", EquipmentSlot.Armor, HeroRarity.Rare, 0, 140),
            new EquipmentDefinition("EQ009", "수호 목걸이", EquipmentSlot.Accessory, HeroRarity.Rare, 10, 55),
            new EquipmentDefinition("EQ010", "집중 포션", EquipmentSlot.Potion, HeroRarity.Rare, 8, 80),
            new EquipmentDefinition("EQ011", "별빛 지팡이", EquipmentSlot.Weapon, HeroRarity.Epic, 42, 0),
            new EquipmentDefinition("EQ012", "왕실 투구", EquipmentSlot.Hat, HeroRarity.Epic, 12, 190),
            new EquipmentDefinition("EQ013", "용비늘 갑옷", EquipmentSlot.Armor, HeroRarity.Epic, 0, 320),
            new EquipmentDefinition("EQ014", "현자의 장신구", EquipmentSlot.Accessory, HeroRarity.Epic, 25, 120),
            new EquipmentDefinition("EQ015", "대형 포션", EquipmentSlot.Potion, HeroRarity.Epic, 18, 210),
            new EquipmentDefinition("EQ016", "태양검", EquipmentSlot.Weapon, HeroRarity.Legendary, 95, 0),
            new EquipmentDefinition("EQ017", "불멸의 왕관", EquipmentSlot.Hat, HeroRarity.Legendary, 35, 520),
            new EquipmentDefinition("EQ018", "신화의 성배", EquipmentSlot.Accessory, HeroRarity.Mythic, 120, 900)
        };

        private static readonly HeroTranscendOptionDefinition[] heroTranscendOptions = BuildHeroTranscendOptions();
        private static readonly FacilityDefinition[] facilities = BuildFacilities();
        private static readonly Dictionary<string, HeroDefinition> heroesById = BuildHeroMap();
        private static readonly Dictionary<string, EquipmentDefinition> equipmentsById = BuildEquipmentMap();
        private static readonly Dictionary<string, TotemDefinition> totemsById = BuildTotemMap();
        private static readonly Dictionary<string, RuneDefinition> runesById = BuildRuneMap();
        private static readonly Dictionary<string, FacilityDefinition> facilitiesById = BuildFacilityMap();
        private static readonly Dictionary<string, HeroTranscendOptionDefinition> heroTranscendOptionsById = BuildHeroTranscendOptionMap();
        private static readonly Dictionary<string, EnemyDefinition> enemiesById = BuildEnemyMap();
        private static readonly Dictionary<string, BossDefinition> bossesById = BuildBossMap();
        private static readonly Dictionary<string, StageDefinition> stagesById = BuildStageMap();
        private static readonly Dictionary<string, int> stageIndexesById = BuildStageIndexMap();

        public static IReadOnlyList<HeroDefinition> Heroes => heroes;
        public static IReadOnlyList<StageDefinition> Stages => stages;
        public static IReadOnlyList<AbilityDefinition> Abilities => abilities;
        public static IReadOnlyList<CombatSkillDefinition> Skills => skills;
        public static IReadOnlyList<PetDefinition> Pets => pets;
        public static IReadOnlyList<TotemDefinition> Totems => totems;
        public static IReadOnlyList<RuneDefinition> Runes => runes;
        public static IReadOnlyList<FacilityDefinition> Facilities => facilities;
        public static IReadOnlyList<EquipmentDefinition> Equipments => equipments;
        public static IReadOnlyList<HeroTranscendOptionDefinition> HeroTranscendOptions => heroTranscendOptions;

        public static HeroDefinition GetHero(string id)
        {
            return heroesById.TryGetValue(id, out HeroDefinition hero) ? hero : heroes[0];
        }

        public static EquipmentDefinition GetEquipment(string id)
        {
            return !string.IsNullOrEmpty(id) && equipmentsById.TryGetValue(id, out EquipmentDefinition equipment) ? equipment : equipments[0];
        }

        public static TotemDefinition GetTotem(string id)
        {
            return !string.IsNullOrEmpty(id) && totemsById.TryGetValue(id, out TotemDefinition totem) ? totem : totems[0];
        }

        public static RuneDefinition GetRune(string id)
        {
            return !string.IsNullOrEmpty(id) && runesById.TryGetValue(id, out RuneDefinition rune) ? rune : runes[0];
        }

        public static FacilityDefinition GetFacility(string id)
        {
            return !string.IsNullOrEmpty(id) && facilitiesById.TryGetValue(id, out FacilityDefinition facility)
                ? facility
                : facilities[0];
        }

        public static int GetRuneSlotUnlockLevel(int slot)
        {
            switch (Mathf.Clamp(slot, 1, MaxRuneSlots))
            {
                case 1:
                    return 1;
                case 2:
                    return 20;
                case 3:
                    return 50;
                case 4:
                    return 100;
                default:
                    return int.MaxValue;
            }
        }

        public static int GetTotemSlotUnlockLevel(int slot)
        {
            switch (Mathf.Clamp(slot, 1, MaxTotemSlots))
            {
                case 1:
                    return 1;
                case 2:
                    return 80;
                default:
                    return int.MaxValue;
            }
        }

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

        public static EnemyDefinition GetEnemy(string id)
        {
            return enemiesById.TryGetValue(id, out EnemyDefinition enemy) ? enemy : enemies[0];
        }

        public static BossDefinition GetBoss(string id)
        {
            return bossesById.TryGetValue(id, out BossDefinition boss) ? boss : bosses[0];
        }

        public static StageDefinition GetStage(string id)
        {
            if (!string.IsNullOrEmpty(id) && stagesById.TryGetValue(id, out StageDefinition stage))
            {
                return stage;
            }

            return TryParseStageId(id, out int chapter, out int number)
                ? GenerateStage(chapter, number)
                : stages[0];
        }

        public static string GetNextStageId(string currentStageId)
        {
            if (!TryParseStageId(currentStageId, out int chapter, out int number))
            {
                int index = GetStageIndex(currentStageId);
                return index >= 0 && index < stages.Length - 1 ? stages[index + 1].Id : null;
            }

            if (number < StagesPerChapter)
            {
                return BuildStageId(chapter, number + 1);
            }

            return BuildStageId(chapter + 1, 1);
        }

        public static string GetPreviousNormalStageId(string currentStageId)
        {
            if (TryParseStageId(currentStageId, out int chapter, out int number))
            {
                if (number > 1)
                {
                    return BuildStageId(chapter, Math.Min(number - 1, StagesPerChapter - 1));
                }

                return chapter > 1 ? BuildStageId(chapter - 1, StagesPerChapter - 1) : FirstStageId;
            }

            return FirstStageId;
        }

        public static bool IsStageUnlocked(string stageId, string highestStageId)
        {
            return GetStageIndex(stageId) <= GetStageIndex(highestStageId);
        }

        public static string MaxStageId(string left, string right)
        {
            return GetStageIndex(left) >= GetStageIndex(right) ? left : right;
        }

        public static int GetStageIndex(string stageId)
        {
            if (TryParseStageId(stageId, out int chapter, out int number))
            {
                return (chapter - 1) * StagesPerChapter + number - 1;
            }

            return stageIndexesById.TryGetValue(stageId ?? string.Empty, out int index) ? index : 0;
        }

        public static GameNumber GetEnemyHp(StageDefinition stage)
        {
            EnemyDefinition enemy = GetEnemy(stage.TargetId);
            return ClampNumber(GameNumber.Floor(enemy.BaseHp * stage.HpMultiplier));
        }

        public static GameNumber GetEnemyGold(StageDefinition stage)
        {
            EnemyDefinition enemy = GetEnemy(stage.TargetId);
            return ClampNumber(GameNumber.Floor(enemy.BaseGold * stage.GoldMultiplier));
        }

        public static GameNumber GetEnemyHeroExpItem(StageDefinition stage)
        {
            return ClampNumber(GameNumber.Ceiling(GetEnemyGold(stage) * 0.20d));
        }

        public static StageClearReward GetStageFirstClearReward(StageDefinition stage)
        {
            if (stage == null)
            {
                return StageClearReward.Empty;
            }

            if (stage.Type == StageType.Boss)
            {
                int chapter = Math.Max(1, stage.Chapter);
                return new StageClearReward(
                    heroSummonTickets: 1,
                    equipmentSummonTickets: 1,
                    ruby: 100,
                    heroExpItems: 30 + chapter * 10,
                    equipmentExpItems: 25 + chapter * 8,
                    heroTranscendStones: chapter % 5 == 0 ? 5 : 1);
            }

            int stageTier = Math.Max(1, stage.Chapter);
            int heroTickets = stage.Number == 10 ? 1 : 0;
            int equipmentTickets = stage.Number == 5 || stage.Number == 15 ? 1 : 0;
            int ruby = stage.Number == 10 ? 25 : 0;
            int heroExp = stage.Number > 0 && stage.Number % 3 == 0 ? 3 + stageTier * 2 : 0;
            int equipmentExp = stage.Number > 0 && stage.Number % 6 == 0 ? 3 + stageTier * 2 : 0;

            return new StageClearReward(heroTickets, equipmentTickets, ruby, heroExp, equipmentExp, 0);
        }

        public static GameNumber GetBossHp(StageDefinition stage)
        {
            BossDefinition boss = GetBoss(stage.TargetId);
            return ClampNumber(GameNumber.Floor(boss.BaseHp * stage.HpMultiplier));
        }

        public static GameNumber GetBossClearGold(StageDefinition stage)
        {
            BossDefinition boss = GetBoss(stage.TargetId);
            return ClampNumber(GameNumber.Floor(boss.ClearGold * stage.GoldMultiplier));
        }

        public static GameNumber GetOfflineGoldPerSecond(string stageId)
        {
            StageDefinition stage = GetStage(stageId);
            if (stage.Type == StageType.Boss)
            {
                stage = GetStage(GetPreviousNormalStageId(stage.Id));
            }

            double multiplier;
            if (stage.Number <= 4)
            {
                multiplier = 0.20d;
            }
            else if (stage.Number <= 8)
            {
                multiplier = 0.25d;
            }
            else if (stage.Number <= 12)
            {
                multiplier = 0.30d;
            }
            else if (stage.Number <= 16)
            {
                multiplier = 0.35d;
            }
            else
            {
                multiplier = 0.40d;
            }

            return ClampNumber(GetEnemyGold(stage) * multiplier);
        }

        public static double ClampVisibleNumber(double value)
        {
            if (double.IsNaN(value))
            {
                return 0d;
            }

            if (double.IsPositiveInfinity(value))
            {
                return double.MaxValue / 1024d;
            }

            if (double.IsNegativeInfinity(value))
            {
                return -double.MaxValue / 1024d;
            }

            return value;
        }

        public static GameNumber ClampNumber(GameNumber value)
        {
            if (double.IsNaN(value.Mantissa))
            {
                return GameNumber.Zero;
            }

            return value;
        }

        public static GameNumber ClampNumber(double value)
        {
            return GameNumber.FromDouble(ClampVisibleNumber(value));
        }

        public static double ClampCombatPower(double value)
        {
            if (double.IsNaN(value) || value <= 1d)
            {
                return 1d;
            }

            return ClampVisibleNumber(value);
        }

        public static long ClampCount(long value)
        {
            return Math.Max(0L, value);
        }

        private static StageDefinition Normal(string id, int number, string enemyId, double hpMultiplier, double goldMultiplier)
        {
            return new StageDefinition(id, 1, number, StageType.Normal, enemyId, hpMultiplier, goldMultiplier, NormalStageRequiredKills, null);
        }

        private static bool TryParseStageId(string id, out int chapter, out int number)
        {
            chapter = 1;
            number = 1;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            string[] parts = id.Split('-');
            if (parts.Length != 2)
            {
                return false;
            }

            return int.TryParse(parts[0], out chapter)
                && int.TryParse(parts[1], out number)
                && chapter >= 1
                && number >= 1
                && number <= StagesPerChapter;
        }

        private static string BuildStageId(int chapter, int number)
        {
            return chapter + "-" + number;
        }

        private static StageDefinition GenerateStage(int chapter, int number)
        {
            int globalIndex = (chapter - 1) * StagesPerChapter + number;
            if (number == StagesPerChapter)
            {
                BossDefinition boss = bosses[0];
                GameNumber targetHp = GenerateTargetHp(globalIndex) * BossHpMultiplier;
                GameNumber targetGold = GenerateTargetGold(globalIndex) * BossGoldMultiplier;
                return new StageDefinition(
                    BuildStageId(chapter, number),
                    chapter,
                    number,
                    StageType.Boss,
                    boss.Id,
                    targetHp / Math.Max(1d, boss.BaseHp),
                    targetGold / Math.Max(1d, boss.ClearGold),
                    1,
                    BuildStageId(chapter, StagesPerChapter - 1));
            }

            EnemyDefinition enemy = GetGeneratedEnemy(globalIndex);
            GameNumber hpMultiplier = GenerateTargetHp(globalIndex) / Math.Max(1d, enemy.BaseHp);
            GameNumber goldMultiplier = GenerateTargetGold(globalIndex) / Math.Max(1d, enemy.BaseGold);
            return new StageDefinition(
                BuildStageId(chapter, number),
                chapter,
                number,
                StageType.Normal,
                enemy.Id,
                hpMultiplier,
                goldMultiplier,
                NormalStageRequiredKills,
                null);
        }

        private static EnemyDefinition GetGeneratedEnemy(int globalIndex)
        {
            int groupIndex = Math.Max(0, (globalIndex - 1) / 4);
            return enemies[groupIndex % enemies.Length];
        }

        private static GameNumber GenerateTargetHp(int globalIndex)
        {
            double growthValue = GeneratedStageHpBase
                * Math.Pow(NormalStageHpGrowth, globalIndex - 1 + GeneratedStageHpOffset)
                + globalIndex * 6d;
            return ClampNumber(growthValue);
        }

        private static GameNumber GenerateTargetGold(int globalIndex)
        {
            double growthValue = GeneratedStageGoldBase
                * Math.Pow(NormalStageGoldGrowth, globalIndex - 1 + GeneratedStageGoldOffset)
                + globalIndex * 0.45d;
            return ClampNumber(growthValue);
        }

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

        private static Dictionary<string, EnemyDefinition> BuildEnemyMap()
        {
            var map = new Dictionary<string, EnemyDefinition>();
            foreach (EnemyDefinition enemy in enemies)
            {
                map[enemy.Id] = enemy;
            }

            return map;
        }

        private static Dictionary<string, BossDefinition> BuildBossMap()
        {
            var map = new Dictionary<string, BossDefinition>();
            foreach (BossDefinition boss in bosses)
            {
                map[boss.Id] = boss;
            }

            return map;
        }

        private static Dictionary<string, StageDefinition> BuildStageMap()
        {
            var map = new Dictionary<string, StageDefinition>();
            foreach (StageDefinition stage in stages)
            {
                map[stage.Id] = stage;
            }

            return map;
        }

        private static Dictionary<string, int> BuildStageIndexMap()
        {
            var map = new Dictionary<string, int>();
            for (int i = 0; i < stages.Length; i++)
            {
                map[stages[i].Id] = i;
            }

            return map;
        }
    }
}
