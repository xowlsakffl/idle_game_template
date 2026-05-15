using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleManager : MonoBehaviour
{
    private readonly System.Random random = new System.Random();
    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private SaveManager saveManager;
    private AbilityManager abilityManager;
    private GameSpeedManager speedManager;
    private List<HeroState> heroes;
    private static readonly IReadOnlyList<HeroState> EmptyHeroes = Array.Empty<HeroState>();
    private readonly List<CombatSkillState> skills = new List<CombatSkillState>();
    private readonly List<PetState> pets = new List<PetState>();
    private bool initialized;

    public event Action Changed;

    public IReadOnlyList<HeroState> Heroes => heroes != null ? heroes : EmptyHeroes;
    public IReadOnlyList<CombatSkillState> Skills => skills;
    public IReadOnlyList<PetState> Pets => pets;
    public string TargetName { get; private set; } = string.Empty;
    public int TargetHp { get; private set; }
    public int TargetMaxHp { get; private set; }
    public int KillsThisStage { get; private set; }
    public int RequiredKills { get; private set; } = GameData.NormalStageRequiredKills;
    public int VisibleEnemyCount { get; private set; }
    public float BossTimeRemaining { get; private set; }
    public bool IsBossFight { get; private set; }
    public string LastBattleLog { get; private set; } = "전투 준비 중";
    public string LastDamageLog { get; private set; } = string.Empty;
    public string LastRewardLog { get; private set; } = string.Empty;
    public string LastHitSourceName { get; private set; } = string.Empty;
    public int LastHitDamage { get; private set; }
    public bool LastHitWasCritical { get; private set; }
    public int HitSequence { get; private set; }
    public string SupportStatusText => BuildSupportStatusText();
    public double PartyAttackPower => GetPartyAttackPower();
    public double TotalCombatPower => abilityManager != null ? abilityManager.GetTotalCombatPower(Heroes) : 1d;
    public float PetGoldBonusPercent => (GetPetGoldBonusMultiplier() - 1f) * 100f;

    public void Initialize(
        StageProgressManager progress,
        CurrencyWallet currency,
        SaveManager save,
        AbilityManager abilities,
        GameSpeedManager speed)
    {
        if (progressManager != null)
        {
            progressManager.Changed -= StartStage;
        }

        progressManager = progress;
        wallet = currency;
        saveManager = save;
        abilityManager = abilities;
        speedManager = speed;
        heroes = saveManager.LoadHeroes() ?? new List<HeroState>();
        skills.Clear();
        pets.Clear();

        foreach (CombatSkillDefinition skill in GameData.Skills)
        {
            skills.Add(new CombatSkillState(skill));
        }

        foreach (PetDefinition pet in GameData.Pets)
        {
            pets.Add(new PetState(pet));
        }

        progressManager.Changed += StartStage;
        initialized = true;
        StartStage();
    }

    private void Update()
    {
        if (!IsReady() || TargetMaxHp <= 0)
        {
            return;
        }

        float battleDeltaTime = Time.deltaTime * Mathf.Max(1, speedManager.CurrentMultiplier);
        TickBattle(battleDeltaTime);
    }

    private void OnDestroy()
    {
        if (progressManager != null)
        {
            progressManager.Changed -= StartStage;
        }
    }

    public bool TryLevelUpHero(string heroId)
    {
        HeroState hero = FindHero(heroId);
        if (hero == null)
        {
            return false;
        }

        int cost = hero.LevelUpCost;
        if (!wallet.SpendHeroExpItem(cost))
        {
            LastBattleLog = hero.Definition.DisplayName + " 레벨업 실패: EXP 아이템 부족";
            NotifyChanged();
            return false;
        }

        hero.Level += 1;
        saveManager.SaveHero(hero);
        saveManager.Flush();

        LastBattleLog = hero.Definition.DisplayName + " Lv." + hero.Level + " 달성";
        NotifyChanged();
        return true;
    }

    public void AddHeroShards(string heroId, int amount)
    {
        HeroState hero = FindHero(heroId);
        if (hero == null || amount <= 0)
        {
            return;
        }

        hero.Shards += amount;
        saveManager.SaveHero(hero);
        saveManager.Flush();
        NotifyChanged();
    }

    public bool TryStarUpHero(string heroId)
    {
        HeroState hero = FindHero(heroId);
        if (hero == null || hero.IsMaxStars)
        {
            return false;
        }

        int cost = hero.StarUpCost;
        if (hero.Shards < cost)
        {
            LastBattleLog = hero.Definition.DisplayName + " 성급업 실패: 조각 부족";
            NotifyChanged();
            return false;
        }

        hero.Shards -= cost;
        hero.Stars += 1;
        saveManager.SaveHero(hero);
        saveManager.Flush();

        LastBattleLog = hero.Definition.DisplayName + " 성급 " + hero.Stars + "/" + HeroDefinition.MaxStars;
        NotifyChanged();
        return true;
    }

    public void DebugLevelAllHeroes(int levels)
    {
        if (!IsReady() || levels <= 0)
        {
            return;
        }

        foreach (HeroState hero in heroes)
        {
            hero.Level += levels;
            saveManager.SaveHero(hero);
        }

        saveManager.Flush();
        LastBattleLog = "QA: 모든 히어로 레벨 +" + levels;
        NotifyChanged();
    }

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
        if (!IsReady())
        {
            return;
        }

        foreach (HeroState hero in heroes)
        {
            hero.AttackCooldown = 0f;
        }

        foreach (CombatSkillState skill in skills)
        {
            skill.CooldownRemaining = skill.Definition.CooldownSeconds;
        }

        foreach (PetState pet in pets)
        {
            pet.AttackCooldown = 0f;
        }

        KillsThisStage = 0;
        SpawnTarget();
    }

    private void SpawnTarget()
    {
        StageDefinition stage = progressManager.CurrentStage;
        IsBossFight = stage.Type == StageType.Boss;
        RequiredKills = stage.RequiredKills;

        if (IsBossFight)
        {
            BossDefinition boss = GameData.GetBoss(stage.TargetId);
            TargetName = boss.DisplayName;
            TargetMaxHp = GameData.GetBossHp(stage);
            TargetHp = TargetMaxHp;
            BossTimeRemaining = boss.TimeLimitSeconds;
            VisibleEnemyCount = 1;
            LastBattleLog = stage.Id + " 보스전 시작";
        }
        else
        {
            EnemyDefinition enemy = GameData.GetEnemy(stage.TargetId);
            TargetName = enemy.DisplayName + " 무리";
            TargetMaxHp = GameData.GetEnemyHp(stage);
            TargetHp = TargetMaxHp;
            BossTimeRemaining = 0f;
            VisibleEnemyCount = Mathf.Clamp(RequiredKills - KillsThisStage, 1, GameData.MaxVisibleEnemies);
            LastBattleLog = stage.Id + " 전투 중";
        }

        NotifyChanged();
    }

    private void TickBattle(float deltaTime)
    {
        if (!IsReady())
        {
            return;
        }

        TickHeroes(deltaTime);
        TickSkills(deltaTime);
        TickPets(deltaTime);
        TickBossTimer(deltaTime);
    }

    private void TickHeroes(float deltaTime)
    {
        foreach (HeroState hero in heroes)
        {
            hero.AttackCooldown -= deltaTime;
            if (hero.AttackCooldown > 0f)
            {
                continue;
            }

            hero.AttackCooldown += hero.Definition.AttackInterval;
            DealDamage(hero);

            if (TargetHp <= 0)
            {
                return;
            }
        }
    }

    private void TickBossTimer(float deltaTime)
    {
        if (!IsBossFight || TargetHp <= 0)
        {
            return;
        }

        BossTimeRemaining -= deltaTime;
        if (BossTimeRemaining <= 0f)
        {
            BossTimeRemaining = 0f;
            progressManager.HandleBossFailed();
            LastBattleLog = "보스 실패: " + GameData.BossFallbackStageId + " 반복 파밍으로 이동";
            NotifyChanged();
        }
    }

    private void DealDamage(HeroState hero)
    {
        int damage = CalculateDamage(hero, out bool isCritical);
        ApplyDamage(damage, hero.Definition.DisplayName, isCritical);
    }

    private void TickSkills(float deltaTime)
    {
        if (TargetHp <= 0)
        {
            return;
        }

        foreach (CombatSkillState skill in skills)
        {
            skill.CooldownRemaining -= deltaTime;
            if (skill.CooldownRemaining > 0f)
            {
                continue;
            }

            skill.CooldownRemaining += skill.Definition.CooldownSeconds;
            int damage = ToDamageInt(GetPartyAttackPower() * skill.Definition.PartyAttackMultiplier * abilityManager.FinalDamageMultiplier);
            ApplyDamage(damage, skill.Definition.DisplayName, false);

            if (TargetHp <= 0)
            {
                return;
            }
        }
    }

    private void TickPets(float deltaTime)
    {
        if (TargetHp <= 0)
        {
            return;
        }

        foreach (PetState pet in pets)
        {
            pet.AttackCooldown -= deltaTime;
            if (pet.AttackCooldown > 0f)
            {
                continue;
            }

            pet.AttackCooldown += pet.Definition.AttackInterval;
            ApplyDamage(ToDamageInt(pet.Definition.AttackPower * abilityManager.FinalDamageMultiplier), pet.Definition.DisplayName, false);

            if (TargetHp <= 0)
            {
                return;
            }
        }
    }

    private int CalculateDamage(HeroState hero, out bool isCritical)
    {
        double damage = hero.AttackPower + abilityManager.AttackPowerBonus;
        isCritical = random.NextDouble() < abilityManager.CriticalChance;
        if (isCritical)
        {
            damage *= abilityManager.CriticalDamageMultiplier;
            if (random.NextDouble() < abilityManager.DoubleCriticalChance)
            {
                damage *= abilityManager.DoubleCriticalBonusMultiplier;
            }
        }

        damage *= abilityManager.FinalDamageMultiplier;
        return ToDamageInt(damage);
    }

    private double GetPartyAttackPower()
    {
        if (heroes == null || abilityManager == null)
        {
            return 0d;
        }

        double total = 0d;
        foreach (HeroState hero in heroes)
        {
            total += hero.AttackPower + abilityManager.AttackPowerBonus;
        }

        return Math.Max(1d, total);
    }

    private static int ToDamageInt(double damage)
    {
        if (double.IsNaN(damage) || damage <= 1d)
        {
            return 1;
        }

        if (damage >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return Mathf.Max(1, Mathf.FloorToInt((float)damage));
    }

    private float GetPetGoldBonusMultiplier()
    {
        float bonus = 1f;
        foreach (PetState pet in pets)
        {
            bonus += pet.Definition.GoldBonusPercent;
        }

        return bonus;
    }

    private void ApplyDamage(int damage, string sourceName, bool isCritical)
    {
        int appliedDamage = Mathf.Max(1, damage);
        TargetHp = Mathf.Max(0, TargetHp - appliedDamage);
        LastHitSourceName = sourceName;
        LastHitDamage = appliedDamage;
        LastHitWasCritical = isCritical;
        HitSequence += 1;
        LastDamageLog = sourceName + " -" + appliedDamage + (isCritical ? " CRIT" : string.Empty);

        if (TargetHp <= 0)
        {
            HandleTargetDefeated();
        }
        else
        {
            NotifyChanged();
        }
    }

    private void HandleTargetDefeated()
    {
        StageDefinition stage = progressManager.CurrentStage;

        if (stage.Type == StageType.Boss)
        {
            BossDefinition boss = GameData.GetBoss(stage.TargetId);
            LastRewardLog = "+" + boss.ClearGold + " 골드";
            wallet.AddGold(boss.ClearGold);
            progressManager.HandleStageCleared();
            LastBattleLog = "보스 처치 성공: 챕터 1 클리어";
            NotifyChanged();
            return;
        }

        int gold = Mathf.FloorToInt(GameData.GetEnemyGold(stage) * GetPetGoldBonusMultiplier());
        int heroExp = GameData.GetEnemyHeroExpItem(stage);
        LastRewardLog = "+" + gold + " 골드, +" + heroExp + " EXP";
        wallet.AddGold(gold);
        wallet.AddHeroExpItem(heroExp);
        KillsThisStage += 1;

        if (KillsThisStage >= RequiredKills)
        {
            progressManager.HandleStageCleared();
            LastBattleLog = stage.Id + " 완료";
            NotifyChanged();
            return;
        }

        LastBattleLog = stage.Id + " 처치 " + KillsThisStage + "/" + RequiredKills;
        SpawnTarget();
    }

    private string BuildSupportStatusText()
    {
        return "Auto Skill/Pet active    Field " + VisibleEnemyCount;
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
        Changed?.Invoke();
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
