using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleManager : MonoBehaviour
{
    private const float InitialEnemySpawnGraceSeconds = 0.25f;
    private const float RespawnEnemySpawnGraceSeconds = 0.45f;
    private readonly System.Random random = new System.Random();
    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private SaveManager saveManager;
    private AbilityManager abilityManager;
    private GameSpeedManager speedManager;
    private List<HeroState> heroes;
    private static readonly IReadOnlyList<HeroState> EmptyHeroes = Array.Empty<HeroState>();
    private readonly List<HeroState> deployedHeroes = new List<HeroState>();
    private readonly List<string> activeFormationHeroIds = new List<string>();
    private readonly List<HeroState> readyHeroAttacks = new List<HeroState>();
    private readonly List<string> recentHeroAttackIds = new List<string>();
    private readonly List<VisibleEnemyState> visibleEnemies = new List<VisibleEnemyState>();
    private readonly List<CombatSkillState> skills = new List<CombatSkillState>();
    private readonly List<PetState> pets = new List<PetState>();
    private readonly Dictionary<string, double> heroDamageMeter = new Dictionary<string, double>();
    private readonly Dictionary<string, int> heroTargetSpawnSequences = new Dictionary<string, int>();
    private readonly Dictionary<string, int> skillTargetSpawnSequences = new Dictionary<string, int>();
    private readonly Dictionary<string, int> petTargetSpawnSequences = new Dictionary<string, int>();
    private int activeHeroPreset = 1;
    private int stageRunSequence;
    private int nextEnemySpawnSequence;
    private int recentHitEnemyIndex = -1;
    private bool skillAutoEnabled = true;
    private bool feverAutoEnabled = true;
    private bool initialized;

    public event Action Changed;

    public IReadOnlyList<HeroState> Heroes => heroes != null ? heroes : EmptyHeroes;
    public IReadOnlyList<HeroState> DeployedHeroes => deployedHeroes;
    public IReadOnlyList<string> ActiveFormationHeroIds => activeFormationHeroIds;
    public IReadOnlyList<string> RecentHeroAttackIds => recentHeroAttackIds;
    public int ActiveHeroPreset => activeHeroPreset;
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
    public int HeroAttackBatchSequence { get; private set; }
    public int RecentHitEnemyIndex => recentHitEnemyIndex;
    public string SupportStatusText => BuildSupportStatusText();
    public double PartyAttackPower => GetPartyAttackPower();
    public double TotalCombatPower => abilityManager != null ? abilityManager.GetTotalCombatPower(DeployedHeroes) : 1d;
    public float PetGoldBonusPercent => (GetPetGoldBonusMultiplier() - 1f) * 100f;
    public bool SkillAutoEnabled => skillAutoEnabled;
    public bool FeverAutoEnabled => feverAutoEnabled;

    public double GetHeroDamageDone(string heroId)
    {
        return !string.IsNullOrEmpty(heroId) && heroDamageMeter.TryGetValue(heroId, out double damage)
            ? damage
            : 0d;
    }

    public double GetMaxHeroDamageDone()
    {
        double maxDamage = 0d;
        foreach (HeroState hero in deployedHeroes)
        {
            maxDamage = Math.Max(maxDamage, GetHeroDamageDone(hero.Definition.Id));
        }

        return maxDamage;
    }

    public float GetVisibleEnemyHpRatio(int visualIndex)
    {
        if (IsBossFight)
        {
            return visualIndex == 0 && TargetMaxHp > 0 ? Mathf.Clamp01((float)TargetHp / TargetMaxHp) : 0f;
        }

        if (visualIndex < 0 || visualIndex >= visibleEnemies.Count)
        {
            return 0f;
        }

        VisibleEnemyState enemy = visibleEnemies[visualIndex];
        return enemy.MaxHp > 0 ? Mathf.Clamp01((float)enemy.Hp / enemy.MaxHp) : 0f;
    }

    public int GetVisibleEnemyDisplayNumber(int visualIndex)
    {
        if (IsBossFight)
        {
            return 1;
        }

        if (visualIndex < 0 || visualIndex >= visibleEnemies.Count)
        {
            return KillsThisStage + visualIndex + 1;
        }

        return visibleEnemies[visualIndex].DisplayNumber;
    }

    public int GetVisibleEnemySpawnSequence(int visualIndex)
    {
        if (IsBossFight)
        {
            return -2;
        }

        if (visualIndex < 0 || visualIndex >= visibleEnemies.Count)
        {
            return -1;
        }

        return visibleEnemies[visualIndex].SpawnSequence;
    }

    public int GetHeroTargetVisualIndex(string heroId)
    {
        if (IsBossFight)
        {
            return 0;
        }

        if (string.IsNullOrEmpty(heroId)
            || !heroTargetSpawnSequences.TryGetValue(heroId, out int spawnSequence))
        {
            return -1;
        }

        return FindVisibleEnemyIndexBySpawnSequence(spawnSequence);
    }

    public int GetHeroTargetSpawnSequence(string heroId)
    {
        return !string.IsNullOrEmpty(heroId)
            && heroTargetSpawnSequences.TryGetValue(heroId, out int spawnSequence)
            ? spawnSequence
            : -1;
    }

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
        activeHeroPreset = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.HeroFormationPreset, 1), 1, GameData.MaxHeroPresets);
        skillAutoEnabled = saveManager.LoadBool(SaveKeys.SkillAutoEnabled, true);
        feverAutoEnabled = saveManager.LoadBool(SaveKeys.FeverAutoEnabled, true);
        RefreshDeployedHeroes();
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

    public void ToggleSkillAuto()
    {
        skillAutoEnabled = !skillAutoEnabled;
        SaveAutoControlState();
        NotifyChanged();
    }

    public void ToggleFeverAuto()
    {
        feverAutoEnabled = !feverAutoEnabled;
        SaveAutoControlState();
        NotifyChanged();
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
        if (hero.Level >= hero.MaxLevel)
        {
            LastBattleLog = hero.Definition.DisplayName + " 레벨업 실패: 현재 성급 최대 레벨";
            NotifyChanged();
            return false;
        }

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

    public void SetActiveHeroPreset(int preset)
    {
        activeHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        PlayerPrefs.SetInt(SaveKeys.HeroFormationPreset, activeHeroPreset);
        saveManager.Flush();
        RefreshDeployedHeroes();
        NotifyChanged();
    }

    public IReadOnlyList<string> GetHeroFormationHeroIds(int preset)
    {
        return LoadFormationHeroIds(Mathf.Clamp(preset, 1, GameData.MaxHeroPresets));
    }

    public bool ApplyHeroFormation(int preset, IReadOnlyList<string> heroIds)
    {
        if (!IsReady())
        {
            return false;
        }

        List<string> normalizedIds = NormalizeFormationHeroIds(heroIds);
        if (GetFilledFormationCount(normalizedIds) <= 0)
        {
            LastBattleLog = "편성 실패: 최소 1명이 필요";
            NotifyChanged();
            return false;
        }

        activeHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        PlayerPrefs.SetInt(SaveKeys.HeroFormationPreset, activeHeroPreset);
        SaveFormationHeroIds(activeHeroPreset, normalizedIds);
        RefreshDeployedHeroes();
        LastBattleLog = "프리셋 " + activeHeroPreset + " 편성 저장";
        StartStage();
        return true;
    }

    public bool ToggleHeroInActiveFormation(string heroId)
    {
        HeroState hero = FindHero(heroId);
        if (hero == null)
        {
            return false;
        }

        List<string> ids = LoadFormationHeroIds(activeHeroPreset);
        int existingIndex = ids.IndexOf(heroId);
        if (existingIndex >= 0)
        {
            if (GetFilledFormationCount(ids) <= 1)
            {
                LastBattleLog = "편성 실패: 최소 1명은 필요";
                NotifyChanged();
                return false;
            }

            ids[existingIndex] = string.Empty;
        }
        else
        {
            int emptyIndex = ids.FindIndex(string.IsNullOrEmpty);
            if (emptyIndex < 0)
            {
                LastBattleLog = "편성 실패: 최대 " + GameData.MaxPartyHeroes + "명";
                NotifyChanged();
                return false;
            }

            ids[emptyIndex] = heroId;
        }

        SaveFormationHeroIds(activeHeroPreset, ids);
        RefreshDeployedHeroes();
        LastBattleLog = "프리셋 " + activeHeroPreset + " 편성 갱신";
        NotifyChanged();
        return true;
    }

    public bool SetHeroInActiveFormationSlot(string heroId, int slotIndex)
    {
        HeroState hero = FindHero(heroId);
        if (hero == null || slotIndex < 0 || slotIndex >= GameData.MaxPartyHeroes)
        {
            return false;
        }

        List<string> ids = LoadFormationHeroIds(activeHeroPreset);
        for (int i = 0; i < ids.Count; i++)
        {
            if (ids[i] == heroId)
            {
                ids[i] = string.Empty;
            }
        }

        ids[slotIndex] = heroId;
        SaveFormationHeroIds(activeHeroPreset, ids);
        RefreshDeployedHeroes();
        LastBattleLog = hero.Definition.DisplayName + " 슬롯 " + (slotIndex + 1) + " 배치";
        NotifyChanged();
        return true;
    }

    public bool RemoveHeroFromActiveFormationSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= GameData.MaxPartyHeroes)
        {
            return false;
        }

        List<string> ids = LoadFormationHeroIds(activeHeroPreset);
        if (string.IsNullOrEmpty(ids[slotIndex]))
        {
            return false;
        }

        if (GetFilledFormationCount(ids) <= 1)
        {
            LastBattleLog = "편성 실패: 최소 1명은 필요";
            NotifyChanged();
            return false;
        }

        ids[slotIndex] = string.Empty;
        SaveFormationHeroIds(activeHeroPreset, ids);
        RefreshDeployedHeroes();
        LastBattleLog = "슬롯 " + (slotIndex + 1) + " 편성 해제";
        NotifyChanged();
        return true;
    }

    public bool RemoveHeroFromActiveFormation(string heroId)
    {
        List<string> ids = LoadFormationHeroIds(activeHeroPreset);
        int index = ids.IndexOf(heroId);
        return index >= 0 && RemoveHeroFromActiveFormationSlot(index);
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

    public int BulkStarUpHeroes()
    {
        if (!IsReady())
        {
            return 0;
        }

        int totalStarUps = 0;
        int affectedHeroes = 0;
        foreach (HeroState hero in heroes)
        {
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
                saveManager.SaveHero(hero);
            }
        }

        if (totalStarUps > 0)
        {
            saveManager.Flush();
            LastBattleLog = "일괄 승급: " + affectedHeroes + "명, 총 " + totalStarUps + "성 상승";
        }
        else
        {
            LastBattleLog = "일괄 승급 실패: 승급 가능한 영웅 없음";
        }

        NotifyChanged();
        return totalStarUps;
    }

    public void DebugLevelAllHeroes(int levels)
    {
        if (!IsReady() || levels <= 0)
        {
            return;
        }

        foreach (HeroState hero in heroes)
        {
            hero.Level = Mathf.Min(hero.MaxLevel, hero.Level + levels);
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

        foreach (HeroState hero in deployedHeroes)
        {
            hero.AttackCooldown = Mathf.Min(hero.AttackInterval, InitialEnemySpawnGraceSeconds + 0.1f);
        }

        foreach (CombatSkillState skill in skills)
        {
            skill.CooldownRemaining = skill.Definition.CooldownSeconds;
        }

        foreach (PetState pet in pets)
        {
            pet.AttackCooldown = Mathf.Min(pet.Definition.AttackInterval, InitialEnemySpawnGraceSeconds + 0.2f);
        }

        stageRunSequence += 1;
        ResetHeroDamageMeter();
        KillsThisStage = 0;
        nextEnemySpawnSequence = 0;
        recentHitEnemyIndex = -1;
        visibleEnemies.Clear();
        ClearTargetLocks();
        SpawnTarget();
    }

    private void SpawnTarget()
    {
        StageDefinition stage = progressManager.CurrentStage;
        IsBossFight = stage.Type == StageType.Boss;
        RequiredKills = stage.RequiredKills;

        if (IsBossFight)
        {
            visibleEnemies.Clear();
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
            BossTimeRemaining = 0f;
            visibleEnemies.Clear();
            FillVisibleEnemies();
            SyncTargetFromVisibleEnemies();
            LastBattleLog = stage.Id + " 전투 중";
        }

        NotifyChanged();
    }

    private void FillVisibleEnemies()
    {
        if (IsBossFight)
        {
            return;
        }

        while (visibleEnemies.Count < GameData.MaxVisibleEnemies
            && nextEnemySpawnSequence < RequiredKills)
        {
            visibleEnemies.Add(CreateVisibleEnemy(InitialEnemySpawnGraceSeconds));
        }

        VisibleEnemyCount = visibleEnemies.Count;
    }

    private VisibleEnemyState CreateVisibleEnemy(float spawnGraceSeconds)
    {
        int spawnSequence = nextEnemySpawnSequence;
        nextEnemySpawnSequence += 1;
        return new VisibleEnemyState(spawnSequence, Mathf.Max(1, TargetMaxHp), spawnSequence + 1, spawnGraceSeconds);
    }

    private void SyncTargetFromVisibleEnemies()
    {
        if (IsBossFight)
        {
            return;
        }

        VisibleEnemyCount = visibleEnemies.Count;
        if (visibleEnemies.Count <= 0)
        {
            TargetHp = 0;
            return;
        }

        TargetMaxHp = visibleEnemies[0].MaxHp;
        TargetHp = visibleEnemies[0].Hp;
    }

    private bool HasAttackableTarget()
    {
        return IsBossFight ? TargetHp > 0 : FindFirstAttackableVisibleEnemyIndex() >= 0;
    }

    private void TickVisibleEnemySpawnGrace(float deltaTime)
    {
        if (IsBossFight || visibleEnemies.Count <= 0)
        {
            return;
        }

        foreach (VisibleEnemyState enemy in visibleEnemies)
        {
            if (enemy.SpawnGraceRemaining > 0f)
            {
                enemy.SpawnGraceRemaining = Mathf.Max(0f, enemy.SpawnGraceRemaining - deltaTime);
            }
        }
    }

    private void TickBattle(float deltaTime)
    {
        if (!IsReady())
        {
            return;
        }

        TickVisibleEnemySpawnGrace(deltaTime);
        if (!HasAttackableTarget())
        {
            return;
        }

        int currentRunSequence = stageRunSequence;
        TickHeroes(deltaTime);
        if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
        {
            return;
        }

        TickSkills(deltaTime);
        if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
        {
            return;
        }

        TickPets(deltaTime);
        if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
        {
            return;
        }

        TickBossTimer(deltaTime);
    }

    private void TickHeroes(float deltaTime)
    {
        readyHeroAttacks.Clear();

        foreach (HeroState hero in deployedHeroes)
        {
            hero.AttackCooldown -= deltaTime;
            if (hero.AttackCooldown > 0f)
            {
                continue;
            }

            hero.AttackCooldown += hero.AttackInterval;
            readyHeroAttacks.Add(hero);
        }

        if (readyHeroAttacks.Count <= 0)
        {
            return;
        }

        recentHeroAttackIds.Clear();
        foreach (HeroState hero in readyHeroAttacks)
        {
            recentHeroAttackIds.Add(hero.Definition.Id);
        }

        HeroAttackBatchSequence += 1;

        int currentRunSequence = stageRunSequence;
        foreach (HeroState hero in readyHeroAttacks)
        {
            if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
            {
                return;
            }

            DealDamage(hero);

            if (stageRunSequence != currentRunSequence || !HasAttackableTarget())
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
        if (IsBossFight)
        {
            ApplyDamage(damage, hero.Definition.DisplayName, isCritical, hero.Definition.Id);
            return;
        }

        ApplyDamageToVisibleEnemy(SelectVisibleEnemyIndexForHero(hero), damage, hero.Definition.DisplayName, isCritical, hero.Definition.Id);
    }

    private void TickSkills(float deltaTime)
    {
        if (!HasAttackableTarget() || !skillAutoEnabled)
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
            if (IsBossFight)
            {
                ApplyDamage(damage, skill.Definition.DisplayName, false);
            }
            else
            {
                ApplyDamageToVisibleEnemy(SelectVisibleEnemyIndexForSkill(skill), damage, skill.Definition.DisplayName, false);
            }

            if (!HasAttackableTarget())
            {
                return;
            }
        }
    }

    private void TickPets(float deltaTime)
    {
        if (!HasAttackableTarget())
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
            int damage = ToDamageInt(pet.Definition.AttackPower * abilityManager.FinalDamageMultiplier);
            if (IsBossFight)
            {
                ApplyDamage(damage, pet.Definition.DisplayName, false);
            }
            else
            {
                ApplyDamageToVisibleEnemy(SelectVisibleEnemyIndexForPet(pet), damage, pet.Definition.DisplayName, false);
            }

            if (!HasAttackableTarget())
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
        if (deployedHeroes.Count <= 0 || abilityManager == null)
        {
            return 0d;
        }

        double total = 0d;
        foreach (HeroState hero in deployedHeroes)
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

    private int SelectVisibleEnemyIndexForHero(HeroState hero)
    {
        if (visibleEnemies.Count <= 0)
        {
            return -1;
        }

        string heroId = hero.Definition.Id;
        if (heroTargetSpawnSequences.TryGetValue(heroId, out int spawnSequence))
        {
            int lockedIndex = FindVisibleEnemyIndexBySpawnSequence(spawnSequence);
            if (lockedIndex >= 0 && visibleEnemies[lockedIndex].IsAttackable)
            {
                return lockedIndex;
            }
        }

        int heroIndex = deployedHeroes.IndexOf(hero);
        if (heroIndex < 0)
        {
            heroIndex = 0;
        }

        int targetIndex = FindAttackableVisibleEnemyIndex(heroIndex);
        if (targetIndex < 0)
        {
            return -1;
        }

        heroTargetSpawnSequences[heroId] = visibleEnemies[targetIndex].SpawnSequence;
        return targetIndex;
    }

    private int SelectVisibleEnemyIndexForSkill(CombatSkillState skill)
    {
        int skillIndex = skills.IndexOf(skill);
        return SelectVisibleEnemyIndexForLockedSource(
            skill.Definition.Id,
            skillTargetSpawnSequences,
            Mathf.Max(0, skillIndex));
    }

    private int SelectVisibleEnemyIndexForPet(PetState pet)
    {
        int petIndex = pets.IndexOf(pet);
        return SelectVisibleEnemyIndexForLockedSource(
            pet.Definition.Id,
            petTargetSpawnSequences,
            Mathf.Max(0, petIndex));
    }

    private int SelectVisibleEnemyIndexForLockedSource(
        string sourceId,
        Dictionary<string, int> targetLocks,
        int preferredOffset)
    {
        if (visibleEnemies.Count <= 0)
        {
            return -1;
        }

        if (!string.IsNullOrEmpty(sourceId)
            && targetLocks.TryGetValue(sourceId, out int spawnSequence))
        {
            int lockedIndex = FindVisibleEnemyIndexBySpawnSequence(spawnSequence);
            if (lockedIndex >= 0 && visibleEnemies[lockedIndex].IsAttackable)
            {
                return lockedIndex;
            }
        }

        int targetIndex = FindAttackableVisibleEnemyIndex(preferredOffset);
        if (targetIndex < 0)
        {
            return -1;
        }

        if (!string.IsNullOrEmpty(sourceId))
        {
            targetLocks[sourceId] = visibleEnemies[targetIndex].SpawnSequence;
        }

        return targetIndex;
    }

    private int FindFirstAttackableVisibleEnemyIndex()
    {
        return FindAttackableVisibleEnemyIndex(0);
    }

    private int FindAttackableVisibleEnemyIndex(int preferredOffset)
    {
        if (visibleEnemies.Count <= 0)
        {
            return -1;
        }

        int startIndex = Mathf.Abs(preferredOffset) % visibleEnemies.Count;
        for (int offset = 0; offset < visibleEnemies.Count; offset++)
        {
            int index = (startIndex + offset) % visibleEnemies.Count;
            if (visibleEnemies[index].IsAttackable)
            {
                return index;
            }
        }

        return -1;
    }

    private int FindVisibleEnemyIndexBySpawnSequence(int spawnSequence)
    {
        for (int i = 0; i < visibleEnemies.Count; i++)
        {
            if (visibleEnemies[i].SpawnSequence == spawnSequence)
            {
                return i;
            }
        }

        return -1;
    }

    private void ApplyDamageToVisibleEnemy(int enemyIndex, int damage, string sourceName, bool isCritical, string heroId = null)
    {
        if (enemyIndex < 0 || enemyIndex >= visibleEnemies.Count)
        {
            SyncTargetFromVisibleEnemies();
            NotifyChanged();
            return;
        }

        VisibleEnemyState enemy = visibleEnemies[enemyIndex];
        int appliedDamage = Mathf.Max(1, damage);
        enemy.Hp = Mathf.Max(0, enemy.Hp - appliedDamage);
        recentHitEnemyIndex = enemyIndex;
        LastHitSourceName = sourceName;
        LastHitDamage = appliedDamage;
        LastHitWasCritical = isCritical;
        HitSequence += 1;
        LastDamageLog = sourceName + " -" + NumberFormatter.Format(appliedDamage) + (isCritical ? " CRIT" : string.Empty);
        AddHeroDamage(heroId, appliedDamage);

        if (enemy.Hp <= 0)
        {
            HandleVisibleEnemyDefeated(enemyIndex, enemy.SpawnSequence);
            return;
        }

        SyncTargetFromVisibleEnemies();
        NotifyChanged();
    }

    private void ApplyDamage(int damage, string sourceName, bool isCritical, string heroId = null)
    {
        int appliedDamage = Mathf.Max(1, damage);
        TargetHp = Mathf.Max(0, TargetHp - appliedDamage);
        LastHitSourceName = sourceName;
        LastHitDamage = appliedDamage;
        LastHitWasCritical = isCritical;
        HitSequence += 1;
        LastDamageLog = sourceName + " -" + NumberFormatter.Format(appliedDamage) + (isCritical ? " CRIT" : string.Empty);
        AddHeroDamage(heroId, appliedDamage);

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
            LastRewardLog = "+" + NumberFormatter.Format(boss.ClearGold) + " 골드";
            wallet.AddGold(boss.ClearGold);
            progressManager.HandleStageCleared();
            LastBattleLog = "보스 처치 성공: 챕터 1 클리어";
            NotifyChanged();
            return;
        }

        int gold = Mathf.FloorToInt(GameData.GetEnemyGold(stage) * GetPetGoldBonusMultiplier());
        int heroExp = GameData.GetEnemyHeroExpItem(stage);
        LastRewardLog = "+" + NumberFormatter.Format(gold) + " 골드, +" + NumberFormatter.Format(heroExp) + " EXP";
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

    private void HandleVisibleEnemyDefeated(int enemyIndex, int defeatedSpawnSequence)
    {
        StageDefinition stage = progressManager.CurrentStage;
        int gold = Mathf.FloorToInt(GameData.GetEnemyGold(stage) * GetPetGoldBonusMultiplier());
        int heroExp = GameData.GetEnemyHeroExpItem(stage);
        LastRewardLog = "+" + NumberFormatter.Format(gold) + " 골드, +" + NumberFormatter.Format(heroExp) + " EXP";
        wallet.AddGold(gold);
        wallet.AddHeroExpItem(heroExp);
        KillsThisStage += 1;
        RemoveTargetLocksForSpawn(defeatedSpawnSequence);

        if (KillsThisStage >= RequiredKills)
        {
            visibleEnemies.Clear();
            SyncTargetFromVisibleEnemies();
            progressManager.HandleStageCleared();
            LastBattleLog = stage.Id + " 완료";
            NotifyChanged();
            return;
        }

        if (enemyIndex >= 0 && enemyIndex < visibleEnemies.Count && nextEnemySpawnSequence < RequiredKills)
        {
            visibleEnemies[enemyIndex] = CreateVisibleEnemy(RespawnEnemySpawnGraceSeconds);
            recentHitEnemyIndex = enemyIndex;
        }
        else if (enemyIndex >= 0 && enemyIndex < visibleEnemies.Count)
        {
            visibleEnemies.RemoveAt(enemyIndex);
            recentHitEnemyIndex = visibleEnemies.Count > 0 ? Mathf.Clamp(enemyIndex, 0, visibleEnemies.Count - 1) : -1;
        }

        SyncTargetFromVisibleEnemies();
        LastBattleLog = stage.Id + " 처치 " + KillsThisStage + "/" + RequiredKills;
        NotifyChanged();
    }

    private string BuildSupportStatusText()
    {
        return "Skill Auto " + (skillAutoEnabled ? "ON" : "OFF")
            + "    Fever Auto " + (feverAutoEnabled ? "ON" : "OFF")
            + "    Field " + VisibleEnemyCount;
    }

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

    private void RemoveTargetLocksForSpawn(int spawnSequence)
    {
        RemoveTargetLocksForSpawn(heroTargetSpawnSequences, spawnSequence);
        RemoveTargetLocksForSpawn(skillTargetSpawnSequences, spawnSequence);
        RemoveTargetLocksForSpawn(petTargetSpawnSequences, spawnSequence);
    }

    private static void RemoveTargetLocksForSpawn(Dictionary<string, int> targetLocks, int spawnSequence)
    {
        if (targetLocks.Count <= 0)
        {
            return;
        }

        var removeKeys = new List<string>();
        foreach (KeyValuePair<string, int> entry in targetLocks)
        {
            if (entry.Value == spawnSequence)
            {
                removeKeys.Add(entry.Key);
            }
        }

        foreach (string key in removeKeys)
        {
            targetLocks.Remove(key);
        }
    }

    private void ResetHeroDamageMeter()
    {
        heroDamageMeter.Clear();
        foreach (HeroState hero in deployedHeroes)
        {
            heroDamageMeter[hero.Definition.Id] = 0d;
        }
    }

    private void AddHeroDamage(string heroId, int damage)
    {
        if (string.IsNullOrEmpty(heroId) || damage <= 0)
        {
            return;
        }

        if (!heroDamageMeter.ContainsKey(heroId))
        {
            heroDamageMeter[heroId] = 0d;
        }

        heroDamageMeter[heroId] += damage;
    }

    private void RefreshDeployedHeroes()
    {
        deployedHeroes.Clear();
        if (heroes == null)
        {
            return;
        }

        List<string> ids = LoadFormationHeroIds(activeHeroPreset);
        activeFormationHeroIds.Clear();
        activeFormationHeroIds.AddRange(ids);

        foreach (string heroId in ids)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                continue;
            }

            HeroState hero = FindHero(heroId);
            if (hero != null && !deployedHeroes.Contains(hero) && deployedHeroes.Count < GameData.MaxPartyHeroes)
            {
                deployedHeroes.Add(hero);
            }
        }

        if (deployedHeroes.Count <= 0)
        {
            foreach (HeroState hero in heroes)
            {
                if (deployedHeroes.Count >= GameData.MaxPartyHeroes)
                {
                    break;
                }

                deployedHeroes.Add(hero);
            }

            activeFormationHeroIds.Clear();
            foreach (HeroState hero in deployedHeroes)
            {
                activeFormationHeroIds.Add(hero.Definition.Id);
            }

            while (activeFormationHeroIds.Count < GameData.MaxPartyHeroes)
            {
                activeFormationHeroIds.Add(string.Empty);
            }
        }
    }

    private List<string> LoadFormationHeroIds(int preset)
    {
        var ids = new List<string>(GameData.MaxPartyHeroes);
        bool hasSavedFormation = false;
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            string key = SaveKeys.HeroFormationSlot(preset, i);
            hasSavedFormation |= PlayerPrefs.HasKey(key);
            ids.Add(PlayerPrefs.GetString(key, string.Empty));
        }

        if (!hasSavedFormation && preset == 1 && heroes != null)
        {
            ids.Clear();
            for (int i = 0; i < GameData.MaxPartyHeroes; i++)
            {
                ids.Add(i < heroes.Count ? heroes[i].Definition.Id : string.Empty);
            }
        }

        return ids;
    }

    private void SaveFormationHeroIds(int preset, List<string> ids)
    {
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            string heroId = i < ids.Count ? ids[i] : string.Empty;
            PlayerPrefs.SetString(SaveKeys.HeroFormationSlot(preset, i), heroId ?? string.Empty);
        }

        saveManager.Flush();
    }

    private List<string> NormalizeFormationHeroIds(IReadOnlyList<string> sourceIds)
    {
        var ids = new List<string>(GameData.MaxPartyHeroes);
        var usedHeroIds = new HashSet<string>();
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            string heroId = sourceIds != null && i < sourceIds.Count ? sourceIds[i] : string.Empty;
            if (string.IsNullOrEmpty(heroId) || FindHero(heroId) == null || usedHeroIds.Contains(heroId))
            {
                ids.Add(string.Empty);
                continue;
            }

            usedHeroIds.Add(heroId);
            ids.Add(heroId);
        }

        return ids;
    }

    private static int GetFilledFormationCount(List<string> ids)
    {
        int count = 0;
        foreach (string heroId in ids)
        {
            if (!string.IsNullOrEmpty(heroId))
            {
                count += 1;
            }
        }

        return count;
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

    private sealed class VisibleEnemyState
    {
        public VisibleEnemyState(int spawnSequence, int maxHp, int displayNumber, float spawnGraceSeconds)
        {
            SpawnSequence = spawnSequence;
            MaxHp = maxHp;
            Hp = maxHp;
            DisplayNumber = displayNumber;
            SpawnGraceRemaining = Mathf.Max(0f, spawnGraceSeconds);
        }

        public int SpawnSequence { get; }
        public int MaxHp { get; }
        public int DisplayNumber { get; }
        public int Hp { get; set; }
        public float SpawnGraceRemaining { get; set; }
        public bool IsAttackable => SpawnGraceRemaining <= 0f && Hp > 0;
    }
}
