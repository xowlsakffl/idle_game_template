using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleManager : MonoBehaviour
{
    private const float InitialEnemySpawnGraceSeconds = 0.75f;
    private const float RespawnEnemySpawnGraceSeconds = 1.35f;
    private const float FieldHalfWidth = 3.85f;
    private const float FieldHalfHeight = 5.15f;
    private const float EnemyAttackRange = 0.62f;
    private const float EnemyAttackIntervalSeconds = 1.15f;
    private const float HeroReviveSeconds = 3f;
    private const float HeroSeparationRadius = 0.72f;
    private const float EnemySeparationRadius = 0.42f;
    private readonly System.Random random = new System.Random();
    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private SaveManager saveManager;
    private AbilityManager abilityManager;
    private AccountProgressManager accountProgressManager;
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
    private readonly Dictionary<string, GameNumber> heroDamageMeter = new Dictionary<string, GameNumber>();
    private readonly Dictionary<string, int> heroTargetSpawnSequences = new Dictionary<string, int>();
    private readonly Dictionary<string, int> skillTargetSpawnSequences = new Dictionary<string, int>();
    private readonly Dictionary<string, int> petTargetSpawnSequences = new Dictionary<string, int>();
    private readonly Dictionary<string, BattleHeroRuntimeState> heroRuntimeStates = new Dictionary<string, BattleHeroRuntimeState>();
    private int activeHeroPreset = 1;
    private int stageRunSequence;
    private int nextEnemySpawnSequence;
    private int recentHitEnemyIndex = -1;
    private int recentAttackingEnemyIndex = -1;
    private int recentDamagedHeroIndex = -1;
    private int monsterHitSequence;
    private int enemyDefeatSequence;
    private Vector2 lastHitPosition;
    private Vector2 lastMonsterHitPosition;
    private Vector2 lastDefeatedEnemyPosition;
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
    public GameNumber TargetHp { get; private set; }
    public GameNumber TargetMaxHp { get; private set; }
    public int KillsThisStage { get; private set; }
    public int RequiredKills { get; private set; } = GameData.NormalStageRequiredKills;
    public int VisibleEnemyCount { get; private set; }
    public float BossTimeRemaining { get; private set; }
    public bool IsBossFight { get; private set; }
    public string LastBattleLog { get; private set; } = "전투 준비 중";
    public string LastDamageLog { get; private set; } = string.Empty;
    public string LastRewardLog { get; private set; } = string.Empty;
    public string LastHitSourceName { get; private set; } = string.Empty;
    public GameNumber LastHitDamage { get; private set; }
    public bool LastHitWasCritical { get; private set; }
    public int HitSequence { get; private set; }
    public int HeroAttackBatchSequence { get; private set; }
    public int MonsterHitSequence => monsterHitSequence;
    public int EnemyDefeatSequence => enemyDefeatSequence;
    public int RecentHitEnemyIndex => recentHitEnemyIndex;
    public int RecentAttackingEnemyIndex => recentAttackingEnemyIndex;
    public int RecentDamagedHeroIndex => recentDamagedHeroIndex;
    public Vector2 LastHitPosition => lastHitPosition;
    public Vector2 LastMonsterHitPosition => lastMonsterHitPosition;
    public Vector2 LastDefeatedEnemyPosition => lastDefeatedEnemyPosition;
    public string SupportStatusText => BuildSupportStatusText();
    public double PartyAttackPower => GetPartyAttackPower();
    public double TotalCombatPower => abilityManager != null
        ? abilityManager.GetTotalCombatPower(DeployedHeroes) * GetTalentMultiplier(TalentEffectKind.AttackPercent) * GetTalentMultiplier(TalentEffectKind.HpPercent)
        : 1d;
    public float PetGoldBonusPercent => (GetPetGoldBonusMultiplier() - 1f) * 100f;
    public bool SkillAutoEnabled => skillAutoEnabled;
    public bool FeverAutoEnabled => feverAutoEnabled;

    public GameNumber GetHeroDamageDone(string heroId)
    {
        return !string.IsNullOrEmpty(heroId) && heroDamageMeter.TryGetValue(heroId, out GameNumber damage)
            ? damage
            : GameNumber.Zero;
    }

    public GameNumber GetMaxHeroDamageDone()
    {
        GameNumber maxDamage = GameNumber.Zero;
        foreach (HeroState hero in deployedHeroes)
        {
            maxDamage = GameNumber.Max(maxDamage, GetHeroDamageDone(hero.Definition.Id));
        }

        return maxDamage;
    }

    public float GetVisibleEnemyHpRatio(int visualIndex)
    {
        if (IsBossFight)
        {
            return visualIndex == 0 && TargetMaxHp > GameNumber.Zero ? Mathf.Clamp01((float)TargetHp.RatioTo(TargetMaxHp)) : 0f;
        }

        if (visualIndex < 0 || visualIndex >= visibleEnemies.Count)
        {
            return 0f;
        }

        VisibleEnemyState enemy = visibleEnemies[visualIndex];
        return enemy.MaxHp > GameNumber.Zero ? Mathf.Clamp01((float)enemy.Hp.RatioTo(enemy.MaxHp)) : 0f;
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

    public Vector2 GetHeroBattlePosition(string heroId)
    {
        return !string.IsNullOrEmpty(heroId)
            && heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
            ? state.Position
            : Vector2.zero;
    }

    public float GetHeroHpRatio(string heroId)
    {
        if (string.IsNullOrEmpty(heroId)
            || !heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
            || state.MaxHp <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(state.Hp / state.MaxHp);
    }

    public bool IsHeroBattleAlive(string heroId)
    {
        return !string.IsNullOrEmpty(heroId)
            && heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
            && state.IsAlive;
    }

    public Vector2 GetVisibleEnemyBattlePosition(int visualIndex)
    {
        if (IsBossFight)
        {
            return new Vector2(0f, 2.05f);
        }

        if (visualIndex < 0 || visualIndex >= visibleEnemies.Count)
        {
            return Vector2.zero;
        }

        return visibleEnemies[visualIndex].Position;
    }

    public void Initialize(
        StageProgressManager progress,
        CurrencyWallet currency,
        SaveManager save,
        AbilityManager abilities,
        GameSpeedManager speed,
        AccountProgressManager accountProgress = null)
    {
        if (progressManager != null)
        {
            progressManager.Changed -= StartStage;
        }

        progressManager = progress;
        wallet = currency;
        saveManager = save;
        abilityManager = abilities;
        accountProgressManager = accountProgress;
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
        if (!IsReady() || TargetMaxHp <= GameNumber.Zero)
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

        if (!hero.IsOwned)
        {
            LastBattleLog = hero.Definition.DisplayName + " 레벨업 실패: 미보유 영웅";
            NotifyChanged();
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
        return NormalizeFormationHeroIds(LoadFormationHeroIds(Mathf.Clamp(preset, 1, GameData.MaxHeroPresets)));
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
        if (hero == null || !hero.IsOwned)
        {
            return false;
        }

        List<string> ids = NormalizeFormationHeroIds(LoadFormationHeroIds(activeHeroPreset));
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
        if (hero == null || !hero.IsOwned || slotIndex < 0 || slotIndex >= GameData.MaxPartyHeroes)
        {
            return false;
        }

        List<string> ids = NormalizeFormationHeroIds(LoadFormationHeroIds(activeHeroPreset));
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

        List<string> ids = NormalizeFormationHeroIds(LoadFormationHeroIds(activeHeroPreset));
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
        List<string> ids = NormalizeFormationHeroIds(LoadFormationHeroIds(activeHeroPreset));
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

    public bool TryRollHeroTranscendOption(string heroId, int slotIndex, bool advanced, out HeroTranscendOptionDefinition option)
    {
        option = null;
        HeroState hero = FindHero(heroId);
        if (hero == null || slotIndex < 0 || slotIndex >= HeroDefinition.MaxTranscendSlots)
        {
            return false;
        }

        if (!hero.IsTranscendSlotUnlocked(slotIndex))
        {
            LastBattleLog = hero.Definition.DisplayName + " 초월 실패: " + HeroDefinition.GetTranscendRequiredStars(slotIndex) + "성 필요";
            NotifyChanged();
            return false;
        }

        option = GameData.RollHeroTranscendOption(hero.Definition, advanced);
        if (option == null)
        {
            LastBattleLog = hero.Definition.DisplayName + " 초월 실패: 옵션 없음";
            NotifyChanged();
            return false;
        }

        hero.SetTranscendOptionId(slotIndex, option.Id);
        saveManager.SaveHeroTranscendOption(hero, slotIndex);
        saveManager.Flush();

        LastBattleLog = hero.Definition.DisplayName + " 초월 " + (slotIndex + 1) + "번: " + option.Grade + " " + option.Description;
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
        ResetBattleHeroRuntimeStates();
        KillsThisStage = 0;
        nextEnemySpawnSequence = 0;
        recentHitEnemyIndex = -1;
        recentAttackingEnemyIndex = -1;
        recentDamagedHeroIndex = -1;
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
        return new VisibleEnemyState(
            spawnSequence,
            GameNumber.Max(GameNumber.One, TargetMaxHp),
            spawnSequence + 1,
            spawnGraceSeconds,
            GetEnemySpawnPosition(spawnSequence));
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
            TargetHp = GameNumber.Zero;
            return;
        }

        TargetMaxHp = visibleEnemies[0].MaxHp;
        TargetHp = visibleEnemies[0].Hp;
    }

    private bool HasAttackableTarget()
    {
        return IsBossFight ? TargetHp > GameNumber.Zero : FindFirstAttackableVisibleEnemyIndex() >= 0;
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
        TickBattleActorMovement(deltaTime);
        TickEnemyAttacks(deltaTime);
        if (!HasAttackableTarget())
        {
            TickBossTimer(deltaTime);
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

    private void TickBattleActorMovement(float deltaTime)
    {
        EnsureBattleHeroRuntimeStates();

        for (int i = 0; i < deployedHeroes.Count; i++)
        {
            HeroState hero = deployedHeroes[i];
            if (!heroRuntimeStates.TryGetValue(hero.Definition.Id, out BattleHeroRuntimeState heroState))
            {
                continue;
            }

            heroState.SlotIndex = i;
            if (!heroState.IsAlive)
            {
                heroState.ReviveRemaining = Mathf.Max(0f, heroState.ReviveRemaining - deltaTime);
                if (heroState.ReviveRemaining <= 0f)
                {
                    heroState.Hp = heroState.MaxHp;
                    heroState.Position = GetHeroBattleSlotPosition(i);
                    hero.AttackCooldown = Mathf.Min(hero.AttackInterval, 0.15f);
                }

                continue;
            }

            int targetIndex = FindNearestVisibleEnemyIndex(heroState.Position, false);
            if (targetIndex < 0)
            {
                heroState.Position = Vector2.MoveTowards(
                    heroState.Position,
                    GetHeroBattleSlotPosition(i),
                    GetHeroMoveSpeed(hero) * deltaTime);
                continue;
            }

            VisibleEnemyState enemy = visibleEnemies[targetIndex];
            heroTargetSpawnSequences[hero.Definition.Id] = enemy.SpawnSequence;
            float attackRange = GetHeroAttackRange(hero);
            heroState.Position = MoveTowardCombatRange(
                heroState.Position,
                enemy.Position,
                attackRange * 0.74f,
                GetHeroMoveSpeed(hero),
                deltaTime);
        }

        ApplyHeroSeparation();

        if (IsBossFight)
        {
            return;
        }

        foreach (VisibleEnemyState enemy in visibleEnemies)
        {
            if (enemy.Hp <= GameNumber.Zero)
            {
                continue;
            }

            BattleHeroRuntimeState targetHero = FindNearestLivingHero(enemy.Position);
            if (targetHero == null)
            {
                enemy.Position = Vector2.MoveTowards(enemy.Position, Vector2.zero, GetEnemyMoveSpeed(enemy) * deltaTime);
                continue;
            }

            enemy.TargetHeroId = targetHero.Hero.Definition.Id;
            enemy.Position = MoveTowardCombatRange(
                enemy.Position,
                targetHero.Position,
                EnemyAttackRange * 0.82f,
                GetEnemyMoveSpeed(enemy),
                deltaTime);
        }

        ApplyEnemySeparation();
    }

    private void TickEnemyAttacks(float deltaTime)
    {
        if (IsBossFight || visibleEnemies.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < visibleEnemies.Count; i++)
        {
            VisibleEnemyState enemy = visibleEnemies[i];
            if (!enemy.IsAttackable)
            {
                continue;
            }

            BattleHeroRuntimeState targetHero = FindNearestLivingHero(enemy.Position);
            if (targetHero == null)
            {
                continue;
            }

            float distance = Vector2.Distance(enemy.Position, targetHero.Position);
            if (distance > EnemyAttackRange)
            {
                enemy.AttackCooldown = Mathf.Min(enemy.AttackCooldown, 0.18f);
                continue;
            }

            enemy.AttackCooldown -= deltaTime;
            if (enemy.AttackCooldown > 0f)
            {
                continue;
            }

            enemy.AttackCooldown += EnemyAttackIntervalSeconds;
            ApplyMonsterDamageToHero(i, enemy, targetHero);
        }
    }

    private void ApplyMonsterDamageToHero(int enemyIndex, VisibleEnemyState enemy, BattleHeroRuntimeState heroState)
    {
        if (heroState == null || !heroState.IsAlive)
        {
            return;
        }

        float damage = Mathf.Max(1f, heroState.MaxHp * 0.035f * (float)GetDamageTakenMultiplier());
        heroState.Hp = Mathf.Max(0f, heroState.Hp - damage);
        recentAttackingEnemyIndex = enemyIndex;
        recentDamagedHeroIndex = heroState.SlotIndex;
        lastMonsterHitPosition = heroState.Position;
        monsterHitSequence += 1;

        if (heroState.Hp <= 0f)
        {
            heroState.ReviveRemaining = HeroReviveSeconds;
            RemoveHeroTargetLock(heroState.Hero.Definition.Id);
            LastBattleLog = heroState.Hero.Definition.DisplayName + " 전투불능";
        }
    }

    private void TickHeroes(float deltaTime)
    {
        readyHeroAttacks.Clear();
        recentHeroAttackIds.Clear();

        foreach (HeroState hero in deployedHeroes)
        {
            if (!IsHeroAlive(hero.Definition.Id))
            {
                continue;
            }

            hero.AttackCooldown -= deltaTime;
            if (hero.AttackCooldown > 0f)
            {
                continue;
            }

            if (SelectVisibleEnemyIndexForHero(hero) < 0)
            {
                hero.AttackCooldown = Mathf.Min(hero.AttackCooldown, 0.08f);
                continue;
            }

            hero.AttackCooldown += hero.AttackInterval;
            readyHeroAttacks.Add(hero);
        }

        if (readyHeroAttacks.Count <= 0)
        {
            return;
        }

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
        if (!IsBossFight || TargetHp <= GameNumber.Zero)
        {
            return;
        }

        BossTimeRemaining -= deltaTime;
        if (BossTimeRemaining <= 0f)
        {
            BossTimeRemaining = 0f;
            string fallbackStageId = GameData.GetPreviousNormalStageId(progressManager.CurrentStageId);
            progressManager.HandleBossFailed();
            LastBattleLog = "보스 실패: " + fallbackStageId + " 반복 파밍으로 이동";
            NotifyChanged();
        }
    }

    private void DealDamage(HeroState hero)
    {
        GameNumber damage = CalculateDamage(hero, out bool isCritical);
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
            GameNumber damage = NormalizeDamage(GetPartyAttackPower()
                * skill.Definition.PartyAttackMultiplier
                * abilityManager.FinalDamageMultiplier
                * GetTalentMultiplier(TalentEffectKind.FinalDamagePercent)
                * GetTalentMultiplier(TalentEffectKind.SkillDamagePercent));
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
            GameNumber damage = NormalizeDamage(pet.Definition.AttackPower
                * abilityManager.FinalDamageMultiplier
                * GetTalentMultiplier(TalentEffectKind.FinalDamagePercent));
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

    private GameNumber CalculateDamage(HeroState hero, out bool isCritical)
    {
        double damage = (hero.AttackPower + abilityManager.AttackPowerBonus)
            * GetTalentMultiplier(TalentEffectKind.AttackPercent);
        isCritical = random.NextDouble() < abilityManager.CriticalChance;
        if (isCritical)
        {
            damage *= abilityManager.CriticalDamageMultiplier * GetTalentMultiplier(TalentEffectKind.CriticalDamagePercent);
            if (random.NextDouble() < abilityManager.DoubleCriticalChance)
            {
                damage *= abilityManager.DoubleCriticalBonusMultiplier;
            }
        }

        damage *= abilityManager.FinalDamageMultiplier * GetTalentMultiplier(TalentEffectKind.FinalDamagePercent);
        return NormalizeDamage(damage);
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
            total += (hero.AttackPower + abilityManager.AttackPowerBonus) * GetTalentMultiplier(TalentEffectKind.AttackPercent);
        }

        return Math.Max(1d, total);
    }

    private static GameNumber NormalizeDamage(double damage)
    {
        if (double.IsNaN(damage) || damage <= 1d)
        {
            return GameNumber.One;
        }

        if (double.IsInfinity(damage))
        {
            return new GameNumber(999.999d, 1000000000);
        }

        return GameNumber.Floor(GameNumber.Max(GameNumber.One, damage));
    }

    private static GameNumber NormalizeDamage(GameNumber damage)
    {
        return GameNumber.Floor(GameNumber.Max(GameNumber.One, damage));
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
        if (!heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState heroState) || !heroState.IsAlive)
        {
            return -1;
        }

        float attackRange = GetHeroAttackRange(hero);
        if (heroTargetSpawnSequences.TryGetValue(heroId, out int spawnSequence))
        {
            int lockedIndex = FindVisibleEnemyIndexBySpawnSequence(spawnSequence);
            if (lockedIndex >= 0
                && visibleEnemies[lockedIndex].IsAttackable
                && Vector2.Distance(heroState.Position, visibleEnemies[lockedIndex].Position) <= attackRange)
            {
                return lockedIndex;
            }
        }

        int targetIndex = FindNearestAttackableEnemyInRange(heroState.Position, attackRange);
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

    private void ApplyDamageToVisibleEnemy(int enemyIndex, GameNumber damage, string sourceName, bool isCritical, string heroId = null)
    {
        if (enemyIndex < 0 || enemyIndex >= visibleEnemies.Count)
        {
            SyncTargetFromVisibleEnemies();
            NotifyChanged();
            return;
        }

        VisibleEnemyState enemy = visibleEnemies[enemyIndex];
        GameNumber appliedDamage = NormalizeDamage(damage);
        enemy.Hp = GameNumber.Max(GameNumber.Zero, enemy.Hp - appliedDamage);
        recentHitEnemyIndex = enemyIndex;
        lastHitPosition = enemy.Position;
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

    private void ApplyDamage(GameNumber damage, string sourceName, bool isCritical, string heroId = null)
    {
        GameNumber appliedDamage = NormalizeDamage(damage);
        TargetHp = GameNumber.Max(GameNumber.Zero, TargetHp - appliedDamage);
        lastHitPosition = new Vector2(0f, 2.05f);
        LastHitSourceName = sourceName;
        LastHitDamage = appliedDamage;
        LastHitWasCritical = isCritical;
        HitSequence += 1;
        LastDamageLog = sourceName + " -" + NumberFormatter.Format(appliedDamage) + (isCritical ? " CRIT" : string.Empty);
        AddHeroDamage(heroId, appliedDamage);

        if (TargetHp <= GameNumber.Zero)
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
            GameNumber bossGold = GameNumber.Floor(GameData.GetBossClearGold(stage) * GetTalentMultiplier(TalentEffectKind.GoldGainPercent));
            GameNumber bossAccountExp = GetAccountExperienceReward(stage, true);
            StageClearReward firstClearReward = ShouldGrantFirstClearReward(stage)
                ? GameData.GetStageFirstClearReward(stage)
                : StageClearReward.Empty;
            LastRewardLog = "+" + NumberFormatter.Format(bossGold) + " 골드";
            wallet.AddGold(bossGold);
            LastRewardLog += ", +" + NumberFormatter.Format(bossAccountExp) + " Account EXP";
            accountProgressManager?.AddExperience(bossAccountExp);
            ApplyStageFirstClearReward(firstClearReward);
            AppendStageFirstClearRewardLog(firstClearReward);
            progressManager.HandleStageCleared();
            LastBattleLog = "보스 처치 성공: " + stage.Id + " 클리어";
            NotifyChanged();
            return;
        }

        GameNumber gold = GameNumber.Floor(GameData.GetEnemyGold(stage) * GetPetGoldBonusMultiplier() * GetTalentMultiplier(TalentEffectKind.GoldGainPercent));
        GameNumber heroExp = GameNumber.Floor(GameData.GetEnemyHeroExpItem(stage) * GetTalentMultiplier(TalentEffectKind.HeroExpGainPercent));
        GameNumber accountExp = GetAccountExperienceReward(stage, false);
        LastRewardLog = "+" + NumberFormatter.Format(gold) + " 골드, +" + NumberFormatter.Format(heroExp) + " EXP";
        wallet.AddGold(gold);
        wallet.AddHeroExpItem(heroExp);
        LastRewardLog += ", +" + NumberFormatter.Format(accountExp) + " Account EXP";
        accountProgressManager?.AddExperience(accountExp);
        KillsThisStage += 1;

        if (KillsThisStage >= RequiredKills)
        {
            StageClearReward firstClearReward = ShouldGrantFirstClearReward(stage)
                ? GameData.GetStageFirstClearReward(stage)
                : StageClearReward.Empty;
            ApplyStageFirstClearReward(firstClearReward);
            AppendStageFirstClearRewardLog(firstClearReward);
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
        GameNumber gold = GameNumber.Floor(GameData.GetEnemyGold(stage) * GetPetGoldBonusMultiplier() * GetTalentMultiplier(TalentEffectKind.GoldGainPercent));
        GameNumber heroExp = GameNumber.Floor(GameData.GetEnemyHeroExpItem(stage) * GetTalentMultiplier(TalentEffectKind.HeroExpGainPercent));
        GameNumber accountExp = GetAccountExperienceReward(stage, false);
        LastRewardLog = "+" + NumberFormatter.Format(gold) + " 골드, +" + NumberFormatter.Format(heroExp) + " EXP";
        wallet.AddGold(gold);
        wallet.AddHeroExpItem(heroExp);
        LastRewardLog += ", +" + NumberFormatter.Format(accountExp) + " Account EXP";
        accountProgressManager?.AddExperience(accountExp);
        KillsThisStage += 1;
        RemoveTargetLocksForSpawn(defeatedSpawnSequence);
        if (enemyIndex >= 0 && enemyIndex < visibleEnemies.Count)
        {
            lastDefeatedEnemyPosition = visibleEnemies[enemyIndex].Position;
            enemyDefeatSequence += 1;
        }

        if (KillsThisStage >= RequiredKills)
        {
            StageClearReward firstClearReward = ShouldGrantFirstClearReward(stage)
                ? GameData.GetStageFirstClearReward(stage)
                : StageClearReward.Empty;
            ApplyStageFirstClearReward(firstClearReward);
            AppendStageFirstClearRewardLog(firstClearReward);
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
            recentHitEnemyIndex = -1;
        }
        else if (enemyIndex >= 0 && enemyIndex < visibleEnemies.Count)
        {
            visibleEnemies.RemoveAt(enemyIndex);
            recentHitEnemyIndex = -1;
        }

        SyncTargetFromVisibleEnemies();
        LastBattleLog = stage.Id + " 처치 " + KillsThisStage + "/" + RequiredKills;
        NotifyChanged();
    }

    private bool ShouldGrantFirstClearReward(StageDefinition stage)
    {
        if (stage == null || progressManager == null)
        {
            return false;
        }

        if (stage.Type == StageType.Boss)
        {
            return stage.Id == progressManager.HighestStageId
                && (stage.Id != GameData.ChapterOneBossStageId || !progressManager.ChapterOneBossCleared);
        }

        return stage.Id == progressManager.HighestStageId;
    }

    private void ApplyStageFirstClearReward(StageClearReward reward)
    {
        if (reward.IsEmpty)
        {
            return;
        }

        wallet.AddHeroSummonTicket(reward.HeroSummonTickets);
        wallet.AddEquipmentSummonTicket(reward.EquipmentSummonTickets);
        wallet.AddRuby(reward.Ruby);
        wallet.AddHeroExpItem(reward.HeroExpItems);
        wallet.AddEquipmentExpItem(reward.EquipmentExpItems);
        wallet.AddHeroTranscendStone(reward.HeroTranscendStones);
    }

    private void AppendStageFirstClearRewardLog(StageClearReward reward)
    {
        if (reward.IsEmpty)
        {
            return;
        }

        string rewardText = BuildStageFirstClearRewardText(reward);
        if (string.IsNullOrEmpty(rewardText))
        {
            return;
        }

        LastRewardLog += " / 최초 클리어 " + rewardText;
    }

    private static string BuildStageFirstClearRewardText(StageClearReward reward)
    {
        var parts = new List<string>();
        if (reward.HeroSummonTickets > 0)
        {
            parts.Add("히어로권 +" + reward.HeroSummonTickets);
        }

        if (reward.EquipmentSummonTickets > 0)
        {
            parts.Add("장비권 +" + reward.EquipmentSummonTickets);
        }

        if (reward.Ruby > 0)
        {
            parts.Add("루비 +" + reward.Ruby);
        }

        if (reward.HeroExpItems > 0)
        {
            parts.Add("경험치책 +" + reward.HeroExpItems);
        }

        if (reward.EquipmentExpItems > 0)
        {
            parts.Add("장비책 +" + reward.EquipmentExpItems);
        }

        if (reward.HeroTranscendStones > 0)
        {
            parts.Add("초월석 +" + reward.HeroTranscendStones);
        }

        return string.Join(", ", parts);
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

    private void RemoveHeroTargetLock(string heroId)
    {
        if (!string.IsNullOrEmpty(heroId))
        {
            heroTargetSpawnSequences.Remove(heroId);
        }
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
            heroDamageMeter[hero.Definition.Id] = GameNumber.Zero;
        }
    }

    private void ResetBattleHeroRuntimeStates()
    {
        heroRuntimeStates.Clear();
        for (int i = 0; i < deployedHeroes.Count; i++)
        {
            HeroState hero = deployedHeroes[i];
            heroRuntimeStates[hero.Definition.Id] = new BattleHeroRuntimeState(hero, GetHeroBattleSlotPosition(i), i, CalculateHeroBattleMaxHp(hero));
        }
    }

    private void EnsureBattleHeroRuntimeStates()
    {
        for (int i = 0; i < deployedHeroes.Count; i++)
        {
            HeroState hero = deployedHeroes[i];
            if (!heroRuntimeStates.TryGetValue(hero.Definition.Id, out BattleHeroRuntimeState state))
            {
                heroRuntimeStates[hero.Definition.Id] = new BattleHeroRuntimeState(hero, GetHeroBattleSlotPosition(i), i, CalculateHeroBattleMaxHp(hero));
                continue;
            }

            state.SlotIndex = i;
            state.MaxHp = CalculateHeroBattleMaxHp(hero);
            state.Hp = Mathf.Min(state.Hp, state.MaxHp);
        }

        var removeKeys = new List<string>();
        foreach (string heroId in heroRuntimeStates.Keys)
        {
            bool deployed = false;
            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                if (deployedHeroes[i].Definition.Id == heroId)
                {
                    deployed = true;
                    break;
                }
            }

            if (!deployed)
            {
                removeKeys.Add(heroId);
            }
        }

        foreach (string heroId in removeKeys)
        {
            heroRuntimeStates.Remove(heroId);
        }
    }

    private bool IsHeroAlive(string heroId)
    {
        return !string.IsNullOrEmpty(heroId)
            && heroRuntimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
            && state.IsAlive;
    }

    private BattleHeroRuntimeState FindNearestLivingHero(Vector2 fromPosition)
    {
        BattleHeroRuntimeState nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (BattleHeroRuntimeState heroState in heroRuntimeStates.Values)
        {
            if (!heroState.IsAlive)
            {
                continue;
            }

            float distance = (heroState.Position - fromPosition).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = heroState;
            }
        }

        return nearest;
    }

    private int FindNearestVisibleEnemyIndex(Vector2 fromPosition, bool attackableOnly)
    {
        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < visibleEnemies.Count; i++)
        {
            VisibleEnemyState enemy = visibleEnemies[i];
            if (enemy.Hp <= GameNumber.Zero || (attackableOnly && !enemy.IsAttackable))
            {
                continue;
            }

            float distance = (enemy.Position - fromPosition).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private int FindNearestAttackableEnemyInRange(Vector2 fromPosition, float range)
    {
        int nearestIndex = -1;
        float nearestDistance = range * range;
        for (int i = 0; i < visibleEnemies.Count; i++)
        {
            VisibleEnemyState enemy = visibleEnemies[i];
            if (!enemy.IsAttackable)
            {
                continue;
            }

            float distance = (enemy.Position - fromPosition).sqrMagnitude;
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private static Vector2 MoveTowardCombatRange(Vector2 current, Vector2 target, float preferredDistance, float speed, float deltaTime)
    {
        Vector2 toTarget = target - current;
        float distance = toTarget.magnitude;
        if (distance <= 0.001f)
        {
            return current;
        }

        Vector2 desired = distance > preferredDistance
            ? target - toTarget.normalized * preferredDistance
            : current;
        return ClampBattlePosition(Vector2.MoveTowards(current, desired, speed * deltaTime));
    }

    private void ApplyHeroSeparation()
    {
        var states = new List<BattleHeroRuntimeState>();
        for (int i = 0; i < deployedHeroes.Count; i++)
        {
            HeroState hero = deployedHeroes[i];
            if (heroRuntimeStates.TryGetValue(hero.Definition.Id, out BattleHeroRuntimeState state) && state.IsAlive)
            {
                states.Add(state);
            }
        }

        for (int i = 0; i < states.Count; i++)
        {
            for (int j = i + 1; j < states.Count; j++)
            {
                PushActorsApart(states[i], states[j], HeroSeparationRadius, i, j);
            }
        }
    }

    private void ApplyEnemySeparation()
    {
        for (int i = 0; i < visibleEnemies.Count; i++)
        {
            VisibleEnemyState left = visibleEnemies[i];
            if (left.Hp <= GameNumber.Zero)
            {
                continue;
            }

            for (int j = i + 1; j < visibleEnemies.Count; j++)
            {
                VisibleEnemyState right = visibleEnemies[j];
                if (right.Hp <= GameNumber.Zero)
                {
                    continue;
                }

                PushEnemiesApart(left, right, EnemySeparationRadius, i, j);
            }
        }
    }

    private static void PushActorsApart(BattleHeroRuntimeState left, BattleHeroRuntimeState right, float minDistance, int leftIndex, int rightIndex)
    {
        Vector2 delta = right.Position - left.Position;
        float distance = delta.magnitude;
        if (distance >= minDistance)
        {
            return;
        }

        Vector2 direction = distance > 0.001f ? delta / distance : GetFallbackSeparationDirection(leftIndex, rightIndex);
        float push = (minDistance - distance) * 0.5f;
        left.Position = ClampBattlePosition(left.Position - direction * push);
        right.Position = ClampBattlePosition(right.Position + direction * push);
    }

    private static void PushEnemiesApart(VisibleEnemyState left, VisibleEnemyState right, float minDistance, int leftIndex, int rightIndex)
    {
        Vector2 delta = right.Position - left.Position;
        float distance = delta.magnitude;
        if (distance >= minDistance)
        {
            return;
        }

        Vector2 direction = distance > 0.001f ? delta / distance : GetFallbackSeparationDirection(leftIndex, rightIndex);
        float push = (minDistance - distance) * 0.5f;
        left.Position = ClampBattlePosition(left.Position - direction * push);
        right.Position = ClampBattlePosition(right.Position + direction * push);
    }

    private static Vector2 GetFallbackSeparationDirection(int leftIndex, int rightIndex)
    {
        float angle = (leftIndex * 37f + rightIndex * 53f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    private static Vector2 GetHeroBattleSlotPosition(int heroIndex)
    {
        switch (heroIndex % GameData.MaxPartyHeroes)
        {
            case 0:
                return new Vector2(-0.78f, -0.28f);
            case 1:
                return new Vector2(0.78f, -0.28f);
            case 2:
                return new Vector2(-1.28f, 0.50f);
            case 3:
                return new Vector2(1.28f, 0.50f);
            case 4:
                return new Vector2(0f, -1.05f);
            case 5:
                return new Vector2(-1.72f, -0.25f);
            case 6:
                return new Vector2(1.72f, -0.25f);
            default:
                return new Vector2(0f, 0.98f);
        }
    }

    private static Vector2 GetEnemySpawnPosition(int spawnSequence)
    {
        int side = Mathf.Abs(spawnSequence) % 4;
        float offset = Mathf.Lerp(-2.6f, 2.6f, PseudoRandom01(spawnSequence * 19 + 5));
        switch (side)
        {
            case 0:
                return new Vector2(-FieldHalfWidth - 0.55f, offset);
            case 1:
                return new Vector2(FieldHalfWidth + 0.55f, offset);
            case 2:
                return new Vector2(offset, FieldHalfHeight + 0.55f);
            default:
                return new Vector2(offset, -FieldHalfHeight - 0.55f);
        }
    }

    private static float GetHeroAttackRange(HeroState hero)
    {
        switch (hero.Definition.Trait)
        {
            case HeroTrait.Melee:
                return 0.82f;
            case HeroTrait.Ranged:
                return 2.45f;
            case HeroTrait.Support:
                return 1.95f;
            case HeroTrait.Defense:
                return 0.92f;
            default:
                return 1.25f;
        }
    }

    private float GetHeroMoveSpeed(HeroState hero)
    {
        return (0.95f + Mathf.Max(0.1f, hero.MoveSpeed) * 0.34f)
            * (float)GetTalentMultiplier(TalentEffectKind.MoveSpeedPercent);
    }

    private float CalculateHeroBattleMaxHp(HeroState hero)
    {
        double hp = hero != null ? hero.MaxHp : 1d;
        if (abilityManager != null)
        {
            hp += abilityManager.MaxHpBonus;
        }

        hp *= GetTalentMultiplier(TalentEffectKind.HpPercent);
        return Mathf.Max(1f, (float)Math.Min(float.MaxValue, hp));
    }

    private GameNumber GetAccountExperienceReward(StageDefinition stage, bool boss)
    {
        if (stage == null)
        {
            return GameNumber.Zero;
        }

        double baseReward = 2d + stage.Chapter * 0.5d + stage.Number * 0.08d;
        if (boss)
        {
            baseReward *= 30d;
        }

        baseReward *= GetTalentMultiplier(TalentEffectKind.AccountExpGainPercent);
        return GameNumber.Floor(GameNumber.FromDouble(Math.Max(1d, baseReward)));
    }

    private double GetTalentMultiplier(TalentEffectKind kind)
    {
        return accountProgressManager != null ? accountProgressManager.GetMultiplier(kind) : 1d;
    }

    private double GetDamageTakenMultiplier()
    {
        return accountProgressManager != null ? accountProgressManager.DamageTakenMultiplier : 1d;
    }

    private static float GetEnemyMoveSpeed(VisibleEnemyState enemy)
    {
        return 1.15f + (Mathf.Abs(enemy.SpawnSequence) % 4) * 0.09f;
    }

    private static Vector2 ClampBattlePosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, -FieldHalfWidth + 0.12f, FieldHalfWidth - 0.12f),
            Mathf.Clamp(position.y, -FieldHalfHeight + 0.12f, FieldHalfHeight - 0.12f));
    }

    private static float PseudoRandom01(int seed)
    {
        unchecked
        {
            uint value = (uint)(seed * 747796405 + 2891336453);
            value = ((value >> ((int)(value >> 28) + 4)) ^ value) * 277803737;
            value = (value >> 22) ^ value;
            return (value & 0xFFFFFF) / 16777215f;
        }
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

    private void RefreshDeployedHeroes()
    {
        deployedHeroes.Clear();
        if (heroes == null)
        {
            return;
        }

        List<string> ids = NormalizeFormationHeroIds(LoadFormationHeroIds(activeHeroPreset));
        activeFormationHeroIds.Clear();
        activeFormationHeroIds.AddRange(ids);

        foreach (string heroId in ids)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                continue;
            }

            HeroState hero = FindHero(heroId);
            if (hero != null && hero.IsOwned && !deployedHeroes.Contains(hero) && deployedHeroes.Count < GameData.MaxPartyHeroes)
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

                if (hero.IsOwned)
                {
                    deployedHeroes.Add(hero);
                }
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
                ids.Add(GetDefaultFormationHeroId(i));
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
            HeroState hero = FindHero(heroId);
            if (string.IsNullOrEmpty(heroId) || hero == null || !hero.IsOwned || usedHeroIds.Contains(heroId))
            {
                ids.Add(string.Empty);
                continue;
            }

            usedHeroIds.Add(heroId);
            ids.Add(heroId);
        }

        return ids;
    }

    private string GetDefaultFormationHeroId(int formationIndex)
    {
        if (heroes == null)
        {
            return string.Empty;
        }

        int ownedIndex = 0;
        foreach (HeroState hero in heroes)
        {
            if (!hero.IsOwned)
            {
                continue;
            }

            if (ownedIndex == formationIndex)
            {
                return hero.Definition.Id;
            }

            ownedIndex += 1;
        }

        return string.Empty;
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

    private sealed class BattleHeroRuntimeState
    {
        public BattleHeroRuntimeState(HeroState hero, Vector2 position, int slotIndex, float maxHp)
        {
            Hero = hero;
            Position = position;
            SlotIndex = slotIndex;
            MaxHp = Mathf.Max(1f, maxHp);
            Hp = MaxHp;
        }

        public HeroState Hero { get; }
        public Vector2 Position { get; set; }
        public int SlotIndex { get; set; }
        public float MaxHp { get; set; }
        public float Hp { get; set; }
        public float ReviveRemaining { get; set; }
        public bool IsAlive => Hp > 0f && ReviveRemaining <= 0f;
    }

    private sealed class VisibleEnemyState
    {
        public VisibleEnemyState(int spawnSequence, GameNumber maxHp, int displayNumber, float spawnGraceSeconds, Vector2 spawnPosition)
        {
            SpawnSequence = spawnSequence;
            MaxHp = maxHp;
            Hp = maxHp;
            DisplayNumber = displayNumber;
            SpawnGraceRemaining = Mathf.Max(0f, spawnGraceSeconds);
            Position = spawnPosition;
            SpawnPosition = spawnPosition;
            AttackCooldown = EnemyAttackIntervalSeconds * (0.35f + 0.05f * (Mathf.Abs(spawnSequence) % 5));
        }

        public int SpawnSequence { get; }
        public GameNumber MaxHp { get; }
        public int DisplayNumber { get; }
        public GameNumber Hp { get; set; }
        public float SpawnGraceRemaining { get; set; }
        public float AttackCooldown { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 SpawnPosition { get; }
        public string TargetHeroId { get; set; } = string.Empty;
        public bool IsAttackable => SpawnGraceRemaining <= 0f && Hp > GameNumber.Zero;
    }
}
