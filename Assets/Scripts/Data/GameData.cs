using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public const string FirstStageId = "1-1";
    public const string ChapterOneBossStageId = "1-20";
    public const string BossFallbackStageId = "1-19";
    public const int NormalStageRequiredKills = 100;
    public const int MaxVisibleEnemies = 12;
    public const int MaxPartyHeroes = 8;
    public const int MaxHeroPresets = 3;

    private static readonly HeroDefinition[] heroes =
    {
        new HeroDefinition("H001", "기사 아렌", "균형형", HeroRarity.Uncommon, HeroTrait.Melee, 10, 130, 0.85f, 3.4f, 5, 26, HeroPassiveStat.AttackPower, 8f),
        new HeroDefinition("H002", "궁수 리나", "빠른 공격", HeroRarity.Rare, HeroTrait.Ranged, 7, 92, 1.25f, 3.9f, 3, 18, HeroPassiveStat.AttackSpeed, 10f),
        new HeroDefinition("H003", "마법사 노아", "강한 한방", HeroRarity.Epic, HeroTrait.Ranged, 18, 80, 0.56f, 3.0f, 8, 16, HeroPassiveStat.AttackPower, 15f),
        new HeroDefinition("H004", "성기사 카일", "안정형", HeroRarity.Legendary, HeroTrait.Defense, 12, 190, 0.72f, 2.8f, 6, 38, HeroPassiveStat.MaxHp, 18f),
        new HeroDefinition("H005", "도적 세라", "연속 공격", HeroRarity.Mythic, HeroTrait.Melee, 6, 105, 1.82f, 4.6f, 2, 20, HeroPassiveStat.MoveSpeed, 14f),
        new HeroDefinition("H006", "사제 미나", "회복 지원", HeroRarity.Rare, HeroTrait.Support, 5, 115, 0.67f, 3.2f, 3, 24, HeroPassiveStat.MaxHp, 10f),
        new HeroDefinition("H007", "방패 로크", "전열 방어", HeroRarity.Epic, HeroTrait.Defense, 14, 230, 0.63f, 2.5f, 5, 42, HeroPassiveStat.MaxHp, 15f),
        new HeroDefinition("H008", "정령 루미", "보조 공격", HeroRarity.Legendary, HeroTrait.Support, 9, 120, 1.00f, 3.7f, 4, 22, HeroPassiveStat.AttackSpeed, 12f)
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
        new AbilityDefinition(AbilityKind.AttackPower, "공격력 증가", "만렙 1,000,000 기준 4.89C", 4890d, 0d, 1000000, 25, 1.25f, AbilityDisplayKind.Flat),
        new AbilityDefinition(AbilityKind.MaxHp, "체력 증가", "만렙 1,000,000 기준 26.6C", 26600d, 0d, 1000000, 30, 1.25f, AbilityDisplayKind.Flat),
        new AbilityDefinition(AbilityKind.CriticalChance, "치명타 확률 증가", "만렙 500 기준 50.0%", 0.1d, 0d, 500, 2500, 1.75f, AbilityDisplayKind.Percent),
        new AbilityDefinition(AbilityKind.CriticalDamage, "치명타 데미지 증가", "기본 5.0% + 레벨당 0.1%", 0.1d, 5d, 1000000, 75, 1.22f, AbilityDisplayKind.Percent),
        new AbilityDefinition(AbilityKind.DoubleCriticalChance, "더블 치명타 확률 증가", "만렙 500 기준 50.0%", 0.1d, 0d, 500, 5000, 1.78f, AbilityDisplayKind.Percent),
        new AbilityDefinition(AbilityKind.DoubleCriticalBonusDamage, "더블 치명타 추가 데미지 증가", "기본 5.0% + 레벨당 0.1%", 0.1d, 5d, 1000000, 100, 1.22f, AbilityDisplayKind.Percent),
        new AbilityDefinition(AbilityKind.FinalDamage, "최종 데미지 증가", "레벨당 0.5%", 0.5d, 0d, 1000000, 150, 1.28f, AbilityDisplayKind.Percent)
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

    private static readonly Dictionary<string, HeroDefinition> heroesById = BuildHeroMap();
    private static readonly Dictionary<string, EnemyDefinition> enemiesById = BuildEnemyMap();
    private static readonly Dictionary<string, BossDefinition> bossesById = BuildBossMap();
    private static readonly Dictionary<string, StageDefinition> stagesById = BuildStageMap();
    private static readonly Dictionary<string, int> stageIndexesById = BuildStageIndexMap();

    public static IReadOnlyList<HeroDefinition> Heroes => heroes;
    public static IReadOnlyList<StageDefinition> Stages => stages;
    public static IReadOnlyList<AbilityDefinition> Abilities => abilities;
    public static IReadOnlyList<CombatSkillDefinition> Skills => skills;
    public static IReadOnlyList<PetDefinition> Pets => pets;

    public static HeroDefinition GetHero(string id)
    {
        return heroesById.TryGetValue(id, out HeroDefinition hero) ? hero : heroes[0];
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
        return !string.IsNullOrEmpty(id) && stagesById.TryGetValue(id, out StageDefinition stage) ? stage : stages[0];
    }

    public static string GetNextStageId(string currentStageId)
    {
        int index = GetStageIndex(currentStageId);
        return index >= 0 && index < stages.Length - 1 ? stages[index + 1].Id : null;
    }

    public static string GetPreviousNormalStageId(string currentStageId)
    {
        int index = GetStageIndex(currentStageId);
        for (int i = Mathf.Clamp(index - 1, 0, stages.Length - 1); i >= 0; i--)
        {
            if (stages[i].Type == StageType.Normal)
            {
                return stages[i].Id;
            }
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
        return stageIndexesById.TryGetValue(stageId ?? string.Empty, out int index) ? index : 0;
    }

    public static int GetEnemyHp(StageDefinition stage)
    {
        EnemyDefinition enemy = GetEnemy(stage.TargetId);
        return Mathf.FloorToInt(enemy.BaseHp * stage.HpMultiplier);
    }

    public static int GetEnemyGold(StageDefinition stage)
    {
        EnemyDefinition enemy = GetEnemy(stage.TargetId);
        return Mathf.FloorToInt(enemy.BaseGold * stage.GoldMultiplier);
    }

    public static int GetEnemyHeroExpItem(StageDefinition stage)
    {
        int baseReward = Mathf.CeilToInt(GetEnemyGold(stage) * 0.6f);
        return Mathf.Max(1, baseReward);
    }

    public static int GetBossHp(StageDefinition stage)
    {
        BossDefinition boss = GetBoss(stage.TargetId);
        return Mathf.FloorToInt(boss.BaseHp * stage.HpMultiplier);
    }

    public static float GetOfflineGoldPerSecond(string stageId)
    {
        StageDefinition stage = GetStage(stageId);
        if (stage.Type == StageType.Boss)
        {
            stage = GetStage(GetPreviousNormalStageId(stage.Id));
        }

        float multiplier;
        if (stage.Number <= 4)
        {
            multiplier = 0.20f;
        }
        else if (stage.Number <= 8)
        {
            multiplier = 0.25f;
        }
        else if (stage.Number <= 12)
        {
            multiplier = 0.30f;
        }
        else if (stage.Number <= 16)
        {
            multiplier = 0.35f;
        }
        else
        {
            multiplier = 0.40f;
        }

        return GetEnemyGold(stage) * multiplier;
    }

    private static StageDefinition Normal(string id, int number, string enemyId, float hpMultiplier, float goldMultiplier)
    {
        return new StageDefinition(id, 1, number, StageType.Normal, enemyId, hpMultiplier, goldMultiplier, NormalStageRequiredKills, null);
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
