using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleManager : MonoBehaviour
{
    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private SaveManager saveManager;
    private List<HeroState> heroes;
    private bool initialized;

    public event Action Changed;

    public IReadOnlyList<HeroState> Heroes => heroes;
    public string TargetName { get; private set; } = string.Empty;
    public int TargetHp { get; private set; }
    public int TargetMaxHp { get; private set; }
    public int KillsThisStage { get; private set; }
    public int RequiredKills { get; private set; } = 10;
    public float BossTimeRemaining { get; private set; }
    public bool IsBossFight { get; private set; }
    public string LastBattleLog { get; private set; } = "전투 준비 중";

    public void Initialize(StageProgressManager progress, CurrencyWallet currency, SaveManager save)
    {
        progressManager = progress;
        wallet = currency;
        saveManager = save;
        heroes = saveManager.LoadHeroes();

        progressManager.Changed += StartStage;
        initialized = true;
        StartStage();
    }

    private void Update()
    {
        if (!initialized || TargetMaxHp <= 0)
        {
            return;
        }

        TickHeroes(Time.deltaTime);
        TickBossTimer(Time.deltaTime);
    }

    public bool TryLevelUpHero(string heroId)
    {
        HeroState hero = FindHero(heroId);
        if (hero == null)
        {
            return false;
        }

        int cost = hero.LevelUpCost;
        if (!wallet.SpendGold(cost))
        {
            LastBattleLog = hero.Definition.DisplayName + " 레벨업 실패: 골드 부족";
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

    private void StartStage()
    {
        if (!initialized)
        {
            return;
        }

        foreach (HeroState hero in heroes)
        {
            hero.AttackCooldown = 0f;
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
            LastBattleLog = stage.Id + " 보스전 시작";
        }
        else
        {
            EnemyDefinition enemy = GameData.GetEnemy(stage.TargetId);
            TargetName = enemy.DisplayName;
            TargetMaxHp = GameData.GetEnemyHp(stage);
            TargetHp = TargetMaxHp;
            BossTimeRemaining = 0f;
            LastBattleLog = stage.Id + " 전투 중";
        }

        NotifyChanged();
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
            DealDamage(hero.AttackPower);

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
            LastBattleLog = "보스 실패: " + GameData.BossFallbackStageId + " 반복 파밍으로 이동";
            progressManager.HandleBossFailed();
            NotifyChanged();
        }
    }

    private void DealDamage(int damage)
    {
        TargetHp = Mathf.Max(0, TargetHp - Mathf.Max(1, damage));
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
            wallet.AddGold(boss.ClearGold);
            LastBattleLog = "보스 처치 성공: 챕터 1 클리어";
            progressManager.HandleStageCleared();
            NotifyChanged();
            return;
        }

        int gold = GameData.GetEnemyGold(stage);
        wallet.AddGold(gold);
        KillsThisStage += 1;

        if (KillsThisStage >= RequiredKills)
        {
            LastBattleLog = stage.Id + " 완료";
            progressManager.HandleStageCleared();
            NotifyChanged();
            return;
        }

        LastBattleLog = stage.Id + " 처치 " + KillsThisStage + "/" + RequiredKills + ", +" + gold + " 골드";
        SpawnTarget();
    }

    private HeroState FindHero(string heroId)
    {
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
}
