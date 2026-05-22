using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleManager : MonoBehaviour
{
    private const string UnequippedTotemId = "__NONE__";
    private const string UnequippedRuneId = "__NONE__";
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
    private readonly List<TotemState> totems = new List<TotemState>();
    private readonly Dictionary<string, TotemState> totemsById = new Dictionary<string, TotemState>();
    private readonly List<RuneState> runes = new List<RuneState>();
    private readonly Dictionary<string, RuneState> runesById = new Dictionary<string, RuneState>();
    private readonly List<FacilityState> facilities = new List<FacilityState>();
    private readonly Dictionary<string, FacilityState> facilitiesById = new Dictionary<string, FacilityState>();
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
    public IReadOnlyList<TotemState> Totems => totems;
    public IReadOnlyList<RuneState> Runes => runes;
    public IReadOnlyList<FacilityState> Facilities => facilities;
    public TotemState ActiveTotem => GetTotemState(GetEquippedTotemId(activeHeroPreset, 1));
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
    public double HeroOwnedAttackBonusPercent => GetHeroOwnedAttackBonusPercent();
    public double TotalCombatPower => GameData.ClampCombatPower(abilityManager != null
        ? abilityManager.GetTotalCombatPower(DeployedHeroes) * GetHeroOwnedAttackMultiplier() * GetTotemAttackMultiplier(null) * GetTotemHpMultiplier(null) * GetRuneAttackMultiplier(null) * GetRuneHpMultiplier() * GetTalentMultiplier(TalentEffectKind.AttackPercent) * GetTalentMultiplier(TalentEffectKind.HpPercent)
        : 1d);
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
        LoadTotems();
        LoadRunes();
        LoadFacilities();
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

    public bool ApplyHeroFormationLoadout(
        int preset,
        IReadOnlyList<string> heroIds,
        IReadOnlyList<string> totemIds,
        IReadOnlyList<string> runeIds)
    {
        if (!IsReady())
        {
            return false;
        }

        List<string> normalizedHeroIds = NormalizeFormationHeroIds(heroIds);
        if (GetFilledFormationCount(normalizedHeroIds) <= 0)
        {
            LastBattleLog = "편성 실패: 최소 1명이 필요";
            NotifyChanged();
            return false;
        }

        int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        activeHeroPreset = normalizedPreset;
        PlayerPrefs.SetInt(SaveKeys.HeroFormationPreset, activeHeroPreset);
        SaveFormationHeroIds(activeHeroPreset, normalizedHeroIds);

        for (int slot = 1; slot <= GameData.MaxTotemSlots; slot++)
        {
            saveManager.SaveString(SaveKeys.HeroFormationTotem(activeHeroPreset, slot), UnequippedTotemId);
        }

        HashSet<string> usedRunes = new HashSet<string>();
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            string runeId = runeIds != null && slot - 1 < runeIds.Count ? runeIds[slot - 1] : string.Empty;
            RuneState state = GetRuneState(runeId);
            bool valid = IsRuneSlotUnlocked(slot) && state != null && state.Unlocked && !usedRunes.Contains(state.Definition.Id);
            string savedId = valid ? state.Definition.Id : UnequippedRuneId;
            if (valid)
            {
                usedRunes.Add(state.Definition.Id);
            }

            saveManager.SaveString(SaveKeys.HeroFormationRune(activeHeroPreset, slot), savedId);
        }

        saveManager.Flush();
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
        LastBattleLog = "QA: 모든 영웅 레벨 +" + levels;
        NotifyChanged();
    }

    public TotemState GetTotemState(string totemId)
    {
        return !string.IsNullOrEmpty(totemId) && totemsById.TryGetValue(totemId, out TotemState state)
            ? state
            : null;
    }

    public string GetEquippedTotemId(int preset)
    {
        return GetEquippedTotemId(preset, 1);
    }

    public string GetEquippedTotemId(int preset, int slot)
    {
        string fallbackId = string.Empty;
        int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxTotemSlots);
        if (saveManager == null)
        {
            string playerPrefValue = PlayerPrefs.GetString(SaveKeys.HeroFormationTotem(normalizedPreset, normalizedSlot), fallbackId);
            if (playerPrefValue == UnequippedTotemId)
            {
                return string.Empty;
            }

            return string.IsNullOrEmpty(playerPrefValue) ? fallbackId : GameData.GetTotem(playerPrefValue).Id;
        }

        string savedId = saveManager.LoadString(SaveKeys.HeroFormationTotem(normalizedPreset, normalizedSlot), fallbackId);
        if (savedId == UnequippedTotemId)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(savedId) ? fallbackId : GameData.GetTotem(savedId).Id;
    }

    public bool IsTotemSlotUnlocked(int slot)
    {
        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxTotemSlots);
        int accountLevel = accountProgressManager != null ? accountProgressManager.Level : 1;
        return accountLevel >= GameData.GetTotemSlotUnlockLevel(normalizedSlot);
    }

    public int GetTotemSlotUnlockLevel(int slot)
    {
        return GameData.GetTotemSlotUnlockLevel(slot);
    }

    public bool SetTotemForPreset(int preset, string totemId)
    {
        return SetTotemForPreset(preset, 1, totemId);
    }

    public bool SetTotemForPreset(int preset, int slot, string totemId)
    {
        if (!IsReady())
        {
            return false;
        }

        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxTotemSlots);
        if (!IsTotemSlotUnlocked(normalizedSlot))
        {
            LastBattleLog = "토템 장착 실패: " + normalizedSlot + "번 슬롯은 아직 잠겨 있음";
            NotifyChanged();
            return false;
        }

        TotemState state = GetTotemState(totemId);
        if (state == null || !state.Unlocked)
        {
            LastBattleLog = "토템 장착 실패: 보유하지 않은 토템";
            NotifyChanged();
            return false;
        }

        int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        for (int i = 1; i <= GameData.MaxTotemSlots; i++)
        {
            if (i != normalizedSlot && GetEquippedTotemId(normalizedPreset, i) == state.Definition.Id)
            {
                LastBattleLog = "토템 장착 실패: 이미 다른 슬롯에 장착됨";
                NotifyChanged();
                return false;
            }
        }

        saveManager.SaveString(SaveKeys.HeroFormationTotem(normalizedPreset, normalizedSlot), state.Definition.Id);
        saveManager.Flush();
        LastBattleLog = "프리셋 " + normalizedPreset + " " + normalizedSlot + "번 토템 장착: " + state.DisplayName;

        if (normalizedPreset == activeHeroPreset)
        {
            StartStage();
        }
        else
        {
            NotifyChanged();
        }

        return true;
    }

    public bool ClearTotemForPreset(int preset, int slot)
    {
        if (!IsReady())
        {
            return false;
        }

        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxTotemSlots);
        if (!IsTotemSlotUnlocked(normalizedSlot))
        {
            LastBattleLog = "토템 해제 실패: " + normalizedSlot + "번 슬롯은 아직 잠겨 있음";
            NotifyChanged();
            return false;
        }

        int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        saveManager.SaveString(SaveKeys.HeroFormationTotem(normalizedPreset, normalizedSlot), UnequippedTotemId);
        saveManager.Flush();
        LastBattleLog = "프리셋 " + normalizedPreset + " " + normalizedSlot + "번 토템 해제";

        if (normalizedPreset == activeHeroPreset)
        {
            StartStage();
        }
        else
        {
            NotifyChanged();
        }

        return true;
    }

    public bool TryLevelUpTotem(string totemId)
    {
        if (!IsReady())
        {
            return false;
        }

        TotemState state = GetTotemState(totemId);
        if (state == null)
        {
            return false;
        }

        if (!state.Unlocked)
        {
            LastBattleLog = state.DisplayName + " 강화 실패: 보유하지 않은 토템";
            NotifyChanged();
            return false;
        }

        if (state.IsMaxed)
        {
            LastBattleLog = state.DisplayName + " MAX";
            NotifyChanged();
            return false;
        }

        int cost = state.LevelUpCost;
        if (!wallet.SpendTotemEssence(cost))
        {
            LastBattleLog = state.DisplayName + " 강화 실패: 토템 정수 부족";
            NotifyChanged();
            return false;
        }

        state.Level += 1;
        state.Unlocked = true;
        PlayerPrefs.SetInt(SaveKeys.TotemLevel(state.Definition.Id), state.Level);
        saveManager.SaveBool(SaveKeys.TotemUnlocked(state.Definition.Id), true);
        saveManager.Flush();
        LastBattleLog = state.DisplayName + " Lv." + state.Level;
        StartStage();

        return true;
    }

    public bool CanPromoteTotemTier(string totemId)
    {
        TotemState state = GetTotemState(totemId);
        return state != null
            && state.Unlocked
            && state.CanPromote
            && AreTotemsReadyToAdvanceGrade(state.Grade);
    }

    private bool AreTotemsReadyToAdvanceGrade(TotemGrade grade)
    {
        bool hasCurrentGrade = false;
        foreach (TotemState state in totems)
        {
            if (state == null || !state.Unlocked)
            {
                return false;
            }

            if (state.Grade < grade)
            {
                return false;
            }

            if (state.Grade == grade)
            {
                hasCurrentGrade = true;
                if (!state.IsMaxed)
                {
                    return false;
                }
            }
        }

        return hasCurrentGrade;
    }

    public bool TryPromoteTotem(string totemId)
    {
        if (!IsReady())
        {
            return false;
        }

        TotemState state = GetTotemState(totemId);
        if (state == null || !state.Unlocked)
        {
            LastBattleLog = "토템 진화 실패: 보유하지 않은 토템";
            NotifyChanged();
            return false;
        }

        if (!state.CanPromote)
        {
            LastBattleLog = state.DisplayName + " 진화 불가";
            NotifyChanged();
            return false;
        }

        if (!AreTotemsReadyToAdvanceGrade(state.Grade))
        {
            LastBattleLog = "토템 진화 실패: 같은 등급 토템을 모두 Lv." + TotemDefinition.MaxLevel + "까지 강화해야 함";
            NotifyChanged();
            return false;
        }

        TotemGrade currentGrade = state.Grade;
        TotemGrade nextGrade = TotemDefinition.GetNextGrade(currentGrade);
        int cost = state.PromoteCost;
        if (!wallet.SpendTotemEssence(cost))
        {
            LastBattleLog = state.DisplayName + " 진화 실패: 토템 정수 부족";
            NotifyChanged();
            return false;
        }

        foreach (TotemState totemState in totems)
        {
            if (totemState == null || totemState.Grade != currentGrade)
            {
                continue;
            }

            totemState.Grade = nextGrade;
            totemState.Level = 1;
            totemState.Unlocked = true;
            PlayerPrefs.SetInt(SaveKeys.TotemLevel(totemState.Definition.Id), totemState.Level);
            PlayerPrefs.SetInt(SaveKeys.TotemGrade(totemState.Definition.Id), (int)totemState.Grade);
            saveManager.SaveBool(SaveKeys.TotemUnlocked(totemState.Definition.Id), true);
        }

        saveManager.Flush();
        LastBattleLog = TotemDefinition.GetGradeLabel(currentGrade) + " 토템 전체 진화 완료";
        StartStage();

        return true;
    }

    public void DebugUnlockAllTotems()
    {
        if (!IsReady())
        {
            return;
        }

        foreach (TotemState state in totems)
        {
            state.Unlocked = true;
            saveManager.SaveBool(SaveKeys.TotemUnlocked(state.Definition.Id), true);
        }

        saveManager.Flush();
        LastBattleLog = "QA: 모든 기본 토템 보유";
        NotifyChanged();
    }

    public RuneState GetRuneState(string runeId)
    {
        return !string.IsNullOrEmpty(runeId) && runesById.TryGetValue(runeId, out RuneState state)
            ? state
            : null;
    }

    public int GetRuneSlotUnlockLevel(int slot)
    {
        return GameData.GetRuneSlotUnlockLevel(slot);
    }

    public bool IsRuneSlotUnlocked(int slot)
    {
        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
        int accountLevel = accountProgressManager != null ? accountProgressManager.Level : 1;
        return accountLevel >= GameData.GetRuneSlotUnlockLevel(normalizedSlot);
    }

    public string GetEquippedRuneId(int preset, int slot)
    {
        int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
        string key = SaveKeys.HeroFormationRune(normalizedPreset, normalizedSlot);
        string savedId = saveManager != null
            ? saveManager.LoadString(key, string.Empty)
            : PlayerPrefs.GetString(key, string.Empty);

        if (savedId == UnequippedRuneId)
        {
            return string.Empty;
        }

        RuneState state = GetRuneState(savedId);
        return state != null ? state.Definition.Id : string.Empty;
    }

    public bool SetRuneForPreset(int preset, int slot, string runeId)
    {
        if (!IsReady())
        {
            return false;
        }

        RuneState state = GetRuneState(runeId);
        if (state == null || !state.Unlocked)
        {
            LastBattleLog = "룬 장착 실패: 보유하지 않은 룬";
            NotifyChanged();
            return false;
        }

        int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
        if (!IsRuneSlotUnlocked(normalizedSlot))
        {
            LastBattleLog = normalizedSlot + "번 룬 슬롯은 계정 Lv." + GetRuneSlotUnlockLevel(normalizedSlot) + "에 해금됩니다.";
            NotifyChanged();
            return false;
        }

        for (int i = 1; i <= GameData.MaxRuneSlots; i++)
        {
            if (i != normalizedSlot && GetEquippedRuneId(normalizedPreset, i) == state.Definition.Id)
            {
                LastBattleLog = "룬 장착 실패: 이미 다른 슬롯에 장착됨";
                NotifyChanged();
                return false;
            }
        }

        saveManager.SaveString(SaveKeys.HeroFormationRune(normalizedPreset, normalizedSlot), state.Definition.Id);
        saveManager.Flush();
        LastBattleLog = "프리셋 " + normalizedPreset + " " + normalizedSlot + "번 룬 장착: " + state.Definition.DisplayName;

        if (normalizedPreset == activeHeroPreset)
        {
            StartStage();
        }
        else
        {
            NotifyChanged();
        }

        return true;
    }

    public bool ClearRuneForPreset(int preset, int slot)
    {
        if (!IsReady())
        {
            return false;
        }

        int normalizedPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        int normalizedSlot = Mathf.Clamp(slot, 1, GameData.MaxRuneSlots);
        saveManager.SaveString(SaveKeys.HeroFormationRune(normalizedPreset, normalizedSlot), UnequippedRuneId);
        saveManager.Flush();
        LastBattleLog = "프리셋 " + normalizedPreset + " " + normalizedSlot + "번 룬 해제";

        if (normalizedPreset == activeHeroPreset)
        {
            StartStage();
        }
        else
        {
            NotifyChanged();
        }

        return true;
    }

    public bool TryPromoteRune(string runeId)
    {
        if (!IsReady())
        {
            return false;
        }

        RuneState state = GetRuneState(runeId);
        if (state == null || !state.Unlocked)
        {
            LastBattleLog = "룬 승급 실패: 보유하지 않은 룬";
            NotifyChanged();
            return false;
        }

        if (state.IsMaxGrade)
        {
            LastBattleLog = state.Definition.DisplayName + " MAX";
            NotifyChanged();
            return false;
        }

        int cost = state.PromoteCost;
        if (state.Copies < cost)
        {
            LastBattleLog = state.Definition.DisplayName + " 승급 실패: 같은 룬 부족";
            NotifyChanged();
            return false;
        }

        state.Copies -= cost;
        state.Grade = (RuneGrade)Mathf.Clamp((int)state.Grade + 1, 0, (int)RuneDefinition.MaxGrade);
        state.Unlocked = true;
        SaveRuneState(state);
        LastBattleLog = state.Definition.DisplayName + " " + state.GradeLabel + " 승급";

        if (IsRuneEquipped(activeHeroPreset, state.Definition.Id))
        {
            StartStage();
        }
        else
        {
            NotifyChanged();
        }

        return true;
    }

    public bool TryLevelUpRune(string runeId)
    {
        return TryPromoteRune(runeId);
    }

    public int TryPromoteAllRunes()
    {
        if (!IsReady())
        {
            return 0;
        }

        int promotedCount = 0;
        bool activePresetChanged = false;
        foreach (RuneState state in runes)
        {
            while (state.CanPromote)
            {
                state.Copies -= state.PromoteCost;
                state.Grade = (RuneGrade)Mathf.Clamp((int)state.Grade + 1, 0, (int)RuneDefinition.MaxGrade);
                state.Unlocked = true;
                SaveRuneState(state, false);
                promotedCount += 1;
                activePresetChanged |= IsRuneEquipped(activeHeroPreset, state.Definition.Id);
            }
        }

        saveManager.Flush();
        LastBattleLog = promotedCount > 0
            ? "룬 일괄 승급 " + promotedCount + "회"
            : "승급 가능한 룬이 없습니다.";

        if (activePresetChanged)
        {
            StartStage();
        }
        else
        {
            NotifyChanged();
        }

        return promotedCount;
    }

    public void DebugUnlockAllRunes()
    {
        if (!IsReady())
        {
            return;
        }

        foreach (RuneState state in runes)
        {
            state.Unlocked = true;
            SaveRuneState(state, false);
        }

        saveManager.Flush();
        LastBattleLog = "QA: 모든 기본 룬 보유";
        NotifyChanged();
    }

    public void DebugAddRuneCopies(int copiesPerRune)
    {
        if (!IsReady())
        {
            return;
        }

        int amount = Mathf.Max(0, copiesPerRune);
        if (amount <= 0)
        {
            return;
        }

        foreach (RuneState state in runes)
        {
            state.Unlocked = true;
            state.Copies = Mathf.Clamp(state.Copies + amount, 0, GameData.MaxIntBalanceValue);
            SaveRuneState(state, false);
        }

        saveManager.Flush();
        LastBattleLog = "QA: 모든 룬 중복 +" + amount;
        NotifyChanged();
    }

    public FacilityState GetFacilityState(string facilityId)
    {
        return !string.IsNullOrEmpty(facilityId) && facilitiesById.TryGetValue(facilityId, out FacilityState state)
            ? state
            : null;
    }

    public GameNumber GetFacilityProductionPerHour(string facilityId)
    {
        FacilityState state = GetFacilityState(facilityId);
        if (state == null)
        {
            return GameNumber.Zero;
        }

        RefreshFacilityProduction(state, false);
        return GetFacilityProductionPerHour(state);
    }

    public GameNumber GetFacilityMaxStoredAmount(string facilityId)
    {
        GameNumber perHour = GetFacilityProductionPerHour(facilityId);
        return perHour * (FacilityDefinition.MaxAccumulatedSeconds / FacilityDefinition.ProductionCycleSeconds);
    }

    public double GetFacilityHeroBonusPercent(string facilityId)
    {
        FacilityState state = GetFacilityState(facilityId);
        return state != null ? GetFacilityHeroBonusPercent(state) : 0d;
    }

    public bool TryUpgradeFacility(string facilityId)
    {
        if (!IsReady())
        {
            return false;
        }

        FacilityState state = GetFacilityState(facilityId);
        if (state == null)
        {
            return false;
        }

        RefreshFacilityProduction(state, false);
        if (state.IsMaxed)
        {
            LastBattleLog = state.Definition.DisplayName + " MAX";
            NotifyChanged();
            return false;
        }

        FacilityUpgradeCost cost = state.UpgradeCost;
        if (!wallet.SpendFacilityMaterials(cost))
        {
            LastBattleLog = state.Definition.DisplayName + " 업그레이드 실패: 자재 부족";
            NotifyChanged();
            return false;
        }

        state.Level += 1;
        SaveFacilityState(state, true);
        LastBattleLog = state.Definition.DisplayName + " Lv." + state.Level;
        NotifyChanged();
        return true;
    }

    public bool CollectFacility(string facilityId)
    {
        if (!IsReady())
        {
            return false;
        }

        FacilityState state = GetFacilityState(facilityId);
        if (state == null)
        {
            return false;
        }

        RefreshFacilityProduction(state, false);
        if (GameNumber.Floor(state.StoredAmount) <= GameNumber.Zero)
        {
            LastBattleLog = state.Definition.DisplayName + " 수령할 보상 없음";
            NotifyChanged();
            return false;
        }

        string rewardText = GrantFacilityReward(state, state.StoredAmount);
        state.StoredAmount = GameNumber.Zero;
        state.LastUpdateUtcTicks = DateTime.UtcNow.Ticks;
        SaveFacilityState(state, true);
        LastRewardLog = rewardText;
        LastBattleLog = state.Definition.DisplayName + " 보상 수령";
        NotifyChanged();
        return true;
    }

    public int CollectAllFacilities()
    {
        if (!IsReady())
        {
            return 0;
        }

        int collected = 0;
        var rewardParts = new List<string>();
        foreach (FacilityState state in facilities)
        {
            RefreshFacilityProduction(state, false);
            if (GameNumber.Floor(state.StoredAmount) <= GameNumber.Zero)
            {
                continue;
            }

            rewardParts.Add(GrantFacilityReward(state, state.StoredAmount));
            state.StoredAmount = GameNumber.Zero;
            state.LastUpdateUtcTicks = DateTime.UtcNow.Ticks;
            SaveFacilityState(state, false);
            collected += 1;
        }

        saveManager.Flush();
        LastRewardLog = rewardParts.Count > 0 ? string.Join(" / ", rewardParts) : "시설 보상 없음";
        LastBattleLog = collected > 0 ? "시설 보상 모두 획득" : "수령할 시설 보상 없음";
        NotifyChanged();
        return collected;
    }

    public bool AutoAssignFacility(string facilityId)
    {
        if (!IsReady())
        {
            return false;
        }

        FacilityState state = GetFacilityState(facilityId);
        if (state == null)
        {
            return false;
        }

        RefreshFacilityProduction(state, false);
        HashSet<string> usedHeroIds = GetAssignedFacilityHeroIdsExcept(state.Definition.Id);
        FillFacilityEmptyAssignments(state, usedHeroIds);

        SaveFacilityState(state, true);
        LastBattleLog = state.Definition.DisplayName + " 추천 배치";
        NotifyChanged();
        return true;
    }

    public void AutoAssignAllFacilities()
    {
        if (!IsReady())
        {
            return;
        }

        HashSet<string> usedHeroIds = new HashSet<string>();
        foreach (FacilityState state in facilities)
        {
            RefreshFacilityProduction(state, false);
            FillFacilityEmptyAssignments(state, usedHeroIds);
            SaveFacilityState(state, false);
        }

        saveManager.Flush();
        LastBattleLog = "시설 전체 추천 배치";
        NotifyChanged();
    }

    public void ClearFacilityAssignments(string facilityId)
    {
        FacilityState state = GetFacilityState(facilityId);
        if (state == null)
        {
            return;
        }

        RefreshFacilityProduction(state, false);
        state.ClearAssignments();
        SaveFacilityState(state, true);
        LastBattleLog = state.Definition.DisplayName + " 배치 해제";
        NotifyChanged();
    }

    public void ClearAllFacilityAssignments()
    {
        if (!IsReady())
        {
            return;
        }

        foreach (FacilityState state in facilities)
        {
            RefreshFacilityProduction(state, false);
            state.ClearAssignments();
            SaveFacilityState(state, false);
        }

        saveManager.Flush();
        LastBattleLog = "시설 배치 모두 해제";
        NotifyChanged();
    }

    public void DebugSimulateFacilityHours(float hours)
    {
        if (!IsReady() || hours <= 0f)
        {
            return;
        }

        long ticks = TimeSpan.FromHours(hours).Ticks;
        foreach (FacilityState state in facilities)
        {
            state.LastUpdateUtcTicks = Math.Max(0L, state.LastUpdateUtcTicks - ticks);
            RefreshFacilityProduction(state, false);
            SaveFacilityState(state, false);
        }

        saveManager.Flush();
        LastBattleLog = "QA: 시설 생산 " + hours.ToString("0.#") + "시간";
        NotifyChanged();
    }

    public void DebugLevelUpAllFacilities()
    {
        if (!IsReady())
        {
            return;
        }

        foreach (FacilityState state in facilities)
        {
            RefreshFacilityProduction(state, false);
            if (!state.IsMaxed)
            {
                state.Level += 1;
            }

            SaveFacilityState(state, false);
        }

        saveManager.Flush();
        LastBattleLog = "QA: 모든 시설 Lv.+1";
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
            skill.CooldownRemaining = skill.Definition.CooldownSeconds * (float)(GetTotemSkillCooldownMultiplier() * GetRuneSkillCooldownMultiplier());
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

            int targetIndex = -1;
            if (heroTargetSpawnSequences.TryGetValue(hero.Definition.Id, out int lockedSpawnSequence))
            {
                targetIndex = FindVisibleEnemyIndexBySpawnSequence(lockedSpawnSequence);
                if (targetIndex < 0 || visibleEnemies[targetIndex].Hp <= GameNumber.Zero)
                {
                    RemoveHeroTargetLock(hero.Definition.Id);
                    targetIndex = -1;
                }
            }

            if (targetIndex < 0)
            {
                targetIndex = FindNearestVisibleEnemyIndex(heroState.Position, false);
                if (targetIndex >= 0)
                {
                    heroTargetSpawnSequences[hero.Definition.Id] = visibleEnemies[targetIndex].SpawnSequence;
                }
            }

            if (targetIndex < 0)
            {
                heroState.Position = Vector2.MoveTowards(
                    heroState.Position,
                    GetHeroBattleSlotPosition(i),
                    GetHeroMoveSpeed(hero) * deltaTime);
                continue;
            }

            VisibleEnemyState enemy = visibleEnemies[targetIndex];
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

            float attackRangeSqr = EnemyAttackRange * EnemyAttackRange;
            float distanceSqr = (enemy.Position - targetHero.Position).sqrMagnitude;
            if (distanceSqr > attackRangeSqr)
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

            hero.AttackCooldown += hero.AttackInterval / (float)(GetTotemAttackSpeedMultiplier(hero) * GetRuneAttackSpeedMultiplier(hero));
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

            skill.CooldownRemaining += skill.Definition.CooldownSeconds * (float)(GetTotemSkillCooldownMultiplier() * GetRuneSkillCooldownMultiplier());
            GameNumber damage = NormalizeDamage(GetPartyAttackPower()
                * skill.Definition.PartyAttackMultiplier
                * abilityManager.FinalDamageMultiplier
                * GetTalentMultiplier(TalentEffectKind.FinalDamagePercent)
                * GetTalentMultiplier(TalentEffectKind.SkillDamagePercent)
                * GetTotemSkillDamageMultiplier()
                * GetRuneSkillDamageMultiplier());
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
            * GetHeroOwnedAttackMultiplier()
            * GetTalentMultiplier(TalentEffectKind.AttackPercent)
            * GetTotemAttackMultiplier(hero)
            * GetRuneAttackMultiplier(hero);
        double criticalChance = Math.Min(0.75d, abilityManager.CriticalChance + (GetTotemCriticalChanceBonus() + GetRuneCriticalChanceBonus()) / 100d);
        isCritical = random.NextDouble() < criticalChance;
        if (isCritical)
        {
            damage *= abilityManager.CriticalDamageMultiplier * GetTalentMultiplier(TalentEffectKind.CriticalDamagePercent);
            if (random.NextDouble() < abilityManager.DoubleCriticalChance)
            {
                damage *= abilityManager.DoubleCriticalBonusMultiplier;
            }
        }

        damage *= abilityManager.FinalDamageMultiplier * GetTalentMultiplier(TalentEffectKind.FinalDamagePercent) * GetRuneFinalDamageMultiplier();
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
            total += (hero.AttackPower + abilityManager.AttackPowerBonus)
                * GetHeroOwnedAttackMultiplier()
                * GetTalentMultiplier(TalentEffectKind.AttackPercent)
                * GetTotemAttackMultiplier(hero)
                * GetRuneAttackMultiplier(hero);
        }

        return Math.Max(1d, total);
    }

    public double GetHeroOwnedAttackBonusPercent(HeroState hero)
    {
        if (hero == null || !hero.IsOwned)
        {
            return 0d;
        }

        return 0.5d + Math.Max(0, hero.Stars) * 0.08d;
    }

    private double GetHeroOwnedAttackBonusPercent()
    {
        if (heroes == null)
        {
            return 0d;
        }

        double total = 0d;
        foreach (HeroState hero in heroes)
        {
            total += GetHeroOwnedAttackBonusPercent(hero);
        }

        return total;
    }

    private double GetHeroOwnedAttackMultiplier()
    {
        return 1d + GetHeroOwnedAttackBonusPercent() / 100d;
    }

    private static GameNumber NormalizeDamage(double damage)
    {
        if (double.IsNaN(damage) || damage <= 1d)
        {
            return GameNumber.One;
        }

        if (double.IsInfinity(damage))
        {
            return GameNumber.FromDouble(double.MaxValue / 1024d);
        }

        return GameData.ClampNumber(GameNumber.Floor(GameNumber.Max(GameNumber.One, damage)));
    }

    private static GameNumber NormalizeDamage(GameNumber damage)
    {
        return GameData.ClampNumber(GameNumber.Floor(GameNumber.Max(GameNumber.One, damage)));
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
            if (lockedIndex >= 0 && visibleEnemies[lockedIndex].IsAttackable)
            {
                float attackRangeSqr = attackRange * attackRange;
                return (heroState.Position - visibleEnemies[lockedIndex].Position).sqrMagnitude <= attackRangeSqr
                    ? lockedIndex
                    : -1;
            }

            RemoveHeroTargetLock(heroId);
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
    }

    private void HandleTargetDefeated()
    {
        StageDefinition stage = progressManager.CurrentStage;

        if (stage.Type == StageType.Boss)
        {
            GameNumber bossGold = GameNumber.Floor(GameData.GetBossClearGold(stage) * GetTalentMultiplier(TalentEffectKind.GoldGainPercent) * GetTotemGoldMultiplier() * GetRuneGoldMultiplier());
            GameNumber bossAccountExp = GetAccountExperienceReward(stage, true);
            StageClearReward firstClearReward = ShouldGrantFirstClearReward(stage)
                ? GameData.GetStageFirstClearReward(stage)
                : StageClearReward.Empty;
            LastRewardLog = "+" + NumberFormatter.Format(bossGold) + " 골드";
            wallet.AddGold(bossGold);
            LastRewardLog += ", +" + NumberFormatter.Format(bossAccountExp) + " Account EXP";
            accountProgressManager?.AddExperience(bossAccountExp);
            LastRewardLog += GrantHuntingFacilityMaterials(stage, true);
            ApplyStageFirstClearReward(firstClearReward);
            AppendStageFirstClearRewardLog(firstClearReward);
            progressManager.HandleStageCleared();
            LastBattleLog = "보스 처치 성공: " + stage.Id + " 클리어";
            NotifyChanged();
            return;
        }

        GameNumber gold = GameNumber.Floor(GameData.GetEnemyGold(stage) * GetPetGoldBonusMultiplier() * GetTalentMultiplier(TalentEffectKind.GoldGainPercent) * GetTotemGoldMultiplier() * GetRuneGoldMultiplier());
        GameNumber heroExp = GameNumber.Floor(GameData.GetEnemyHeroExpItem(stage) * GetTalentMultiplier(TalentEffectKind.HeroExpGainPercent) * GetTotemHeroExpMultiplier() * GetRuneHeroExpMultiplier());
        GameNumber accountExp = GetAccountExperienceReward(stage, false);
        LastRewardLog = "+" + NumberFormatter.Format(gold) + " 골드, +" + NumberFormatter.Format(heroExp) + " EXP";
        wallet.AddGold(gold);
        wallet.AddHeroExpItem(heroExp);
        LastRewardLog += ", +" + NumberFormatter.Format(accountExp) + " Account EXP";
        accountProgressManager?.AddExperience(accountExp);
        LastRewardLog += GrantHuntingFacilityMaterials(stage, false);
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
        GameNumber gold = GameNumber.Floor(GameData.GetEnemyGold(stage) * GetPetGoldBonusMultiplier() * GetTalentMultiplier(TalentEffectKind.GoldGainPercent) * GetTotemGoldMultiplier() * GetRuneGoldMultiplier());
        GameNumber heroExp = GameNumber.Floor(GameData.GetEnemyHeroExpItem(stage) * GetTalentMultiplier(TalentEffectKind.HeroExpGainPercent) * GetTotemHeroExpMultiplier() * GetRuneHeroExpMultiplier());
        GameNumber accountExp = GetAccountExperienceReward(stage, false);
        LastRewardLog = "+" + NumberFormatter.Format(gold) + " 골드, +" + NumberFormatter.Format(heroExp) + " EXP";
        wallet.AddGold(gold);
        wallet.AddHeroExpItem(heroExp);
        LastRewardLog += ", +" + NumberFormatter.Format(accountExp) + " Account EXP";
        accountProgressManager?.AddExperience(accountExp);
        LastRewardLog += GrantHuntingFacilityMaterials(stage, false);
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
            parts.Add("영웅권 +" + reward.HeroSummonTickets);
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
            * (float)GetTalentMultiplier(TalentEffectKind.MoveSpeedPercent)
            * (float)GetTotemMoveSpeedMultiplier()
            * (float)GetRuneMoveSpeedMultiplier();
    }

    private float CalculateHeroBattleMaxHp(HeroState hero)
    {
        double hp = hero != null ? hero.MaxHp : 1d;
        if (abilityManager != null)
        {
            hp += abilityManager.MaxHpBonus;
        }

        hp *= GetTalentMultiplier(TalentEffectKind.HpPercent) * GetTotemHpMultiplier(hero) * GetRuneHpMultiplier();
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

        baseReward *= GetTalentMultiplier(TalentEffectKind.AccountExpGainPercent) * GetTotemAccountExpMultiplier() * GetRuneAccountExpMultiplier();
        return GameNumber.Floor(GameNumber.FromDouble(Math.Max(1d, baseReward)));
    }

    private string GrantHuntingFacilityMaterials(StageDefinition stage, bool boss)
    {
        if (stage == null || wallet == null)
        {
            return string.Empty;
        }

        int stageIndex = GameData.GetStageIndex(stage.Id) + 1;
        long wood = 0L;
        long brick = 0L;
        long iron = 0L;

        if (boss)
        {
            wood = 8L + stageIndex;
            brick = stageIndex >= 8 ? 2L + stageIndex / 5L : 1L;
            iron = stageIndex >= 18 ? 1L + stageIndex / 12L : 0L;
        }
        else
        {
            double woodChance = Math.Min(0.18d, 0.04d + stageIndex * 0.002d);
            if (random.NextDouble() < woodChance)
            {
                wood = 1L + stageIndex / 25L;
            }
        }

        if (wood <= 0L && brick <= 0L && iron <= 0L)
        {
            return string.Empty;
        }

        wallet.AddFacilityMaterials(wood, brick, iron);
        var parts = new List<string>();
        if (wood > 0L)
        {
            parts.Add("목재 +" + wood);
        }

        if (brick > 0L)
        {
            parts.Add("벽돌 +" + brick);
        }

        if (iron > 0L)
        {
            parts.Add("철재 +" + iron);
        }

        return " / " + string.Join(", ", parts);
    }

    private double GetTalentMultiplier(TalentEffectKind kind)
    {
        return accountProgressManager != null ? accountProgressManager.GetMultiplier(kind) : 1d;
    }

    private double GetDamageTakenMultiplier()
    {
        return (accountProgressManager != null ? accountProgressManager.DamageTakenMultiplier : 1d)
            * GetTotemDamageTakenMultiplier()
            * GetRuneDamageTakenMultiplier();
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

    private void LoadTotems()
    {
        totems.Clear();
        totemsById.Clear();
        foreach (TotemDefinition definition in GameData.Totems)
        {
            int level = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.TotemLevel(definition.Id), 1), 1, TotemDefinition.MaxLevel);
            TotemGrade grade = (TotemGrade)Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.TotemGrade(definition.Id), 0), 0, (int)TotemGrade.Mythic);
            bool unlocked = saveManager.LoadBool(SaveKeys.TotemUnlocked(definition.Id), definition.StartUnlocked);
            var state = new TotemState(definition, level, grade, unlocked);
            totems.Add(state);
            totemsById[definition.Id] = state;
        }
    }

    private void LoadRunes()
    {
        runes.Clear();
        runesById.Clear();
        foreach (RuneDefinition definition in GameData.Runes)
        {
            int savedGrade = PlayerPrefs.GetInt(SaveKeys.RuneGrade(definition.Id), -1);
            if (savedGrade < 0)
            {
                int legacyLevel = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.RuneLevel(definition.Id), 1), 1, RuneDefinition.MaxLevel);
                savedGrade = Mathf.Clamp((legacyLevel - 1) / 10, 0, (int)RuneDefinition.MaxGrade);
            }

            RuneGrade grade = (RuneGrade)Mathf.Clamp(savedGrade, 0, (int)RuneDefinition.MaxGrade);
            int copies = Mathf.Max(0, PlayerPrefs.GetInt(SaveKeys.RuneCopies(definition.Id), 0));
            bool unlocked = saveManager.LoadBool(SaveKeys.RuneUnlocked(definition.Id), definition.StartUnlocked);
            var state = new RuneState(definition, grade, copies, unlocked);
            runes.Add(state);
            runesById[definition.Id] = state;
        }
    }

    private void LoadFacilities()
    {
        facilities.Clear();
        facilitiesById.Clear();
        long nowTicks = DateTime.UtcNow.Ticks;
        foreach (FacilityDefinition definition in GameData.Facilities)
        {
            int level = Mathf.Clamp(PlayerPrefs.GetInt(SaveKeys.FacilityLevel(definition.Id), 1), 1, FacilityDefinition.MaxLevel);
            GameNumber storedAmount = saveManager.LoadGameNumber(SaveKeys.FacilityStoredAmount(definition.Id), GameNumber.Zero);
            long lastUpdateTicks = saveManager.LoadLong(SaveKeys.FacilityLastUpdateUtcTicks(definition.Id), nowTicks);
            var state = new FacilityState(definition, level, storedAmount, lastUpdateTicks);
            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                state.SetAssignedHeroId(slot, saveManager.LoadString(SaveKeys.FacilityAssignedHero(definition.Id, slot), string.Empty));
            }

            facilities.Add(state);
            facilitiesById[definition.Id] = state;
            RefreshFacilityProduction(state, false);
            SaveFacilityState(state, false);
        }

        NormalizeFacilityAssignments();
        saveManager.Flush();
    }

    private void NormalizeFacilityAssignments()
    {
        var usedHeroIds = new HashSet<string>();
        foreach (FacilityState state in facilities)
        {
            if (state == null)
            {
                continue;
            }

            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                string heroId = state.GetAssignedHeroId(slot);
                bool unlocked = slot < state.UnlockedSlotCount;
                HeroState hero = FindHero(heroId);
                if (!unlocked
                    || string.IsNullOrEmpty(heroId)
                    || hero == null
                    || !hero.IsOwned
                    || usedHeroIds.Contains(heroId))
                {
                    state.SetAssignedHeroId(slot, string.Empty);
                    continue;
                }

                usedHeroIds.Add(heroId);
            }

            SaveFacilityState(state, false);
        }
    }

    private void SaveFacilityState(FacilityState state, bool flush)
    {
        if (state == null || saveManager == null)
        {
            return;
        }

        PlayerPrefs.SetInt(SaveKeys.FacilityLevel(state.Definition.Id), state.Level);
        saveManager.SaveGameNumber(SaveKeys.FacilityStoredAmount(state.Definition.Id), state.StoredAmount);
        saveManager.SaveLong(SaveKeys.FacilityLastUpdateUtcTicks(state.Definition.Id), state.LastUpdateUtcTicks);
        for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
        {
            saveManager.SaveString(SaveKeys.FacilityAssignedHero(state.Definition.Id, slot), state.GetAssignedHeroId(slot));
        }

        if (flush)
        {
            saveManager.Flush();
        }
    }

    private void SaveRuneState(RuneState state, bool flush = true)
    {
        if (state == null || saveManager == null)
        {
            return;
        }

        PlayerPrefs.SetInt(SaveKeys.RuneGrade(state.Definition.Id), (int)state.Grade);
        PlayerPrefs.SetInt(SaveKeys.RuneCopies(state.Definition.Id), Mathf.Max(0, state.Copies));
        saveManager.SaveBool(SaveKeys.RuneUnlocked(state.Definition.Id), state.Unlocked);
        if (flush)
        {
            saveManager.Flush();
        }
    }

    private bool IsTotemEquipped(int preset, string totemId)
    {
        if (string.IsNullOrEmpty(totemId))
        {
            return false;
        }

        for (int slot = 1; slot <= GameData.MaxTotemSlots; slot++)
        {
            if (IsTotemSlotUnlocked(slot) && GetEquippedTotemId(preset, slot) == totemId)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsRuneEquipped(int preset, string runeId)
    {
        if (string.IsNullOrEmpty(runeId))
        {
            return false;
        }

        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            if (GetEquippedRuneId(preset, slot) == runeId)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerable<TotemState> GetActiveUsableTotems()
    {
        foreach (TotemState state in totems)
        {
            if (state != null && state.Unlocked)
            {
                yield return state;
            }
        }
    }

    private double GetTotemAttackMultiplier(HeroState hero)
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetAttackPercent(state.Level, state.Grade, deployedHeroes, IsBossFight);
            if (hero != null)
            {
                percent += state.Definition.GetTraitAttackPercent(state.Level, state.Grade, hero.Definition.Trait, deployedHeroes);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemHpMultiplier(HeroState hero)
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetHpPercent(state.Level, state.Grade, deployedHeroes);
            if (hero != null)
            {
                percent += state.Definition.GetTraitHpPercent(state.Level, state.Grade, hero.Definition.Trait);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemGoldMultiplier()
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetGoldGainPercent(state.Level, state.Grade);
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemHeroExpMultiplier()
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetHeroExpGainPercent(state.Level, state.Grade);
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemAccountExpMultiplier()
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetAccountExpGainPercent(state.Level, state.Grade);
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemAttackSpeedMultiplier(HeroState hero)
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetAttackSpeedPercent(state.Level, state.Grade, hero);
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemMoveSpeedMultiplier()
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetMoveSpeedPercent(state.Level, state.Grade);
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemSkillDamageMultiplier()
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetSkillDamagePercent(state.Level, state.Grade);
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetTotemSkillCooldownMultiplier()
    {
        double reduction = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            reduction += state.Definition.GetSkillCooldownReductionPercent(state.Level, state.Grade);
        }

        return Math.Max(0.65d, 1d - Math.Min(35d, reduction) / 100d);
    }

    private double GetTotemCriticalChanceBonus()
    {
        double percent = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            percent += state.Definition.GetCriticalChancePercent(state.Level, state.Grade);
        }

        return Math.Max(0d, percent);
    }

    private double GetTotemDamageTakenMultiplier()
    {
        double reduction = 0d;
        foreach (TotemState state in GetActiveUsableTotems())
        {
            reduction += state.Definition.GetDamageReductionPercent(state.Level, state.Grade);
        }

        reduction = Math.Min(90d, Math.Max(0d, reduction));
        return Math.Max(0.1d, 1d - reduction / 100d);
    }

    private RuneState GetActiveUsableRune(int slot)
    {
        if (!IsRuneSlotUnlocked(slot))
        {
            return null;
        }

        RuneState state = GetRuneState(GetEquippedRuneId(activeHeroPreset, slot));
        return state != null && state.Unlocked ? state : null;
    }

    private double GetRuneAttackMultiplier(HeroState hero)
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetAttackPercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneFinalDamageMultiplier()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetFinalDamagePercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneHpMultiplier()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetHpPercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneGoldMultiplier()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetGoldGainPercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneHeroExpMultiplier()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetHeroExpGainPercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneAccountExpMultiplier()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetAccountExpGainPercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneAttackSpeedMultiplier(HeroState hero)
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetAttackSpeedPercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneMoveSpeedMultiplier()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetMoveSpeedPercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneSkillDamageMultiplier()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetSkillDamagePercent(state.Grade);
            }
        }

        return 1d + Math.Max(0d, percent) / 100d;
    }

    private double GetRuneSkillCooldownMultiplier()
    {
        double reduction = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                reduction += state.Definition.GetSkillCooldownReductionPercent(state.Grade);
            }
        }

        return Math.Max(0.75d, 1d - Math.Min(25d, reduction) / 100d);
    }

    private double GetRuneCriticalChanceBonus()
    {
        double percent = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                percent += state.Definition.GetCriticalChancePercent(state.Grade);
            }
        }

        return Math.Max(0d, percent);
    }

    private double GetRuneDamageTakenMultiplier()
    {
        double reduction = 0d;
        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            RuneState state = GetActiveUsableRune(slot);
            if (state != null)
            {
                reduction += state.Definition.GetDamageReductionPercent(state.Grade);
            }
        }

        reduction = Math.Min(80d, Math.Max(0d, reduction));
        return Math.Max(0.2d, 1d - reduction / 100d);
    }

    private void RefreshFacilityProduction(FacilityState state, bool save)
    {
        if (state == null)
        {
            return;
        }

        long nowTicks = DateTime.UtcNow.Ticks;
        if (state.LastUpdateUtcTicks <= 0L)
        {
            state.LastUpdateUtcTicks = nowTicks;
            if (save)
            {
                SaveFacilityState(state, true);
            }

            return;
        }

        double elapsedSeconds = TimeSpan.FromTicks(Math.Max(0L, nowTicks - state.LastUpdateUtcTicks)).TotalSeconds;
        if (elapsedSeconds <= 0d)
        {
            return;
        }

        GameNumber productionPerHour = GetFacilityProductionPerHour(state);
        GameNumber maxStored = productionPerHour * (FacilityDefinition.MaxAccumulatedSeconds / FacilityDefinition.ProductionCycleSeconds);
        GameNumber produced = productionPerHour * (elapsedSeconds / FacilityDefinition.ProductionCycleSeconds);
        state.StoredAmount = GameNumber.Min(maxStored, GameNumber.Max(GameNumber.Zero, state.StoredAmount + produced));
        state.LastUpdateUtcTicks = nowTicks;
        if (save)
        {
            SaveFacilityState(state, true);
        }
    }

    private GameNumber GetFacilityProductionPerHour(FacilityState state)
    {
        return state != null
            ? state.Definition.GetProductionPerHour(state.Level, GetFacilityHeroBonusPercent(state))
            : GameNumber.Zero;
    }

    private double GetFacilityHeroBonusPercent(FacilityState state)
    {
        if (state == null)
        {
            return 0d;
        }

        double total = 0d;
        for (int i = 0; i < state.UnlockedSlotCount; i++)
        {
            HeroState hero = FindHero(state.GetAssignedHeroId(i));
            if (hero != null && hero.IsOwned)
            {
                total += GetFacilityHeroProductionBonusPercent(hero);
            }
        }

        return Math.Min(FacilityDefinition.MaxHeroProductionBonusPercent, total);
    }

    private double GetFacilityHeroProductionBonusPercent(HeroState hero)
    {
        if (hero == null)
        {
            return 0d;
        }

        double rarity = Math.Max(0, (int)hero.Definition.Rarity);
        double score = hero.AttackPower * 0.018d
            + hero.MaxHp * 0.006d
            + hero.Level * 0.025d
            + hero.Stars * 0.35d
            + rarity * 0.85d;
        return Math.Max(1d, Math.Min(10d, score));
    }

    private List<HeroState> GetFacilityAssignmentCandidates(HashSet<string> usedHeroIds)
    {
        var candidates = new List<HeroState>();
        foreach (HeroState hero in Heroes)
        {
            if (hero != null
                && hero.IsOwned
                && (usedHeroIds == null || !usedHeroIds.Contains(hero.Definition.Id)))
            {
                candidates.Add(hero);
            }
        }

        candidates.Sort((left, right) => GetFacilityHeroSortScore(right).CompareTo(GetFacilityHeroSortScore(left)));
        return candidates;
    }

    private int FillFacilityEmptyAssignments(FacilityState state, HashSet<string> usedHeroIds)
    {
        if (state == null)
        {
            return 0;
        }

        if (usedHeroIds == null)
        {
            usedHeroIds = new HashSet<string>();
        }

        var localUsedHeroIds = new HashSet<string>();
        for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
        {
            string heroId = state.GetAssignedHeroId(slot);
            bool unlocked = slot < state.UnlockedSlotCount;
            HeroState hero = FindHero(heroId);
            if (!unlocked
                || string.IsNullOrEmpty(heroId)
                || hero == null
                || !hero.IsOwned
                || localUsedHeroIds.Contains(heroId)
                || usedHeroIds.Contains(heroId))
            {
                state.SetAssignedHeroId(slot, string.Empty);
                continue;
            }

            localUsedHeroIds.Add(heroId);
            usedHeroIds.Add(heroId);
        }

        List<HeroState> candidates = GetFacilityAssignmentCandidates(usedHeroIds);
        int candidateIndex = 0;
        int assigned = 0;
        for (int slot = 0; slot < state.UnlockedSlotCount; slot++)
        {
            if (!string.IsNullOrEmpty(state.GetAssignedHeroId(slot)))
            {
                continue;
            }

            while (candidateIndex < candidates.Count && usedHeroIds.Contains(candidates[candidateIndex].Definition.Id))
            {
                candidateIndex += 1;
            }

            if (candidateIndex >= candidates.Count)
            {
                break;
            }

            string heroId = candidates[candidateIndex].Definition.Id;
            state.SetAssignedHeroId(slot, heroId);
            usedHeroIds.Add(heroId);
            assigned += 1;
            candidateIndex += 1;
        }

        return assigned;
    }

    private double GetFacilityHeroSortScore(HeroState hero)
    {
        if (hero == null)
        {
            return 0d;
        }

        return hero.AttackPower
            + hero.MaxHp * 0.25d
            + hero.Level * 6d
            + hero.Stars * 120d
            + Math.Max(0, (int)hero.Definition.Rarity) * 220d;
    }

    private HashSet<string> GetAssignedFacilityHeroIdsExcept(string excludedFacilityId)
    {
        var used = new HashSet<string>();
        foreach (FacilityState facility in facilities)
        {
            if (facility == null || facility.Definition.Id == excludedFacilityId)
            {
                continue;
            }

            for (int i = 0; i < facility.UnlockedSlotCount; i++)
            {
                string heroId = facility.GetAssignedHeroId(i);
                if (!string.IsNullOrEmpty(heroId))
                {
                    used.Add(heroId);
                }
            }
        }

        return used;
    }

    private string GrantFacilityReward(FacilityState state, GameNumber amount)
    {
        if (state == null || amount <= GameNumber.Zero)
        {
            return string.Empty;
        }

        GameNumber reward = GameNumber.Floor(amount);
        switch (state.Definition.RewardKind)
        {
            case FacilityRewardKind.Gold:
                wallet.AddGold(reward);
                return state.Definition.DisplayName + " +" + NumberFormatter.Format(reward) + " 골드";
            case FacilityRewardKind.HeroExpItem:
                wallet.AddHeroExpItem(reward);
                return state.Definition.DisplayName + " +" + NumberFormatter.Format(reward) + " 영웅 경험치책";
            case FacilityRewardKind.EquipmentExpItem:
                wallet.AddEquipmentExpItem(reward);
                return state.Definition.DisplayName + " +" + NumberFormatter.Format(reward) + " 장비책";
            case FacilityRewardKind.TotemEssence:
                {
                    long count = GameNumberToLong(reward);
                    wallet.AddTotemEssence(count);
                    return state.Definition.DisplayName + " +" + count + " 토템 정수";
                }
            case FacilityRewardKind.RuneCopyBox:
                {
                    long boxes = GameNumberToLong(reward);
                    GrantRuneCopiesFromBoxes(boxes);
                    return state.Definition.DisplayName + " +" + boxes + " 룬 사본 상자";
                }
            case FacilityRewardKind.HeroTranscendStone:
                {
                    long count = GameNumberToLong(reward);
                    wallet.AddHeroTranscendStone(count);
                    return state.Definition.DisplayName + " +" + count + " 초월석";
                }
            default:
                return string.Empty;
        }
    }

    private long GameNumberToLong(GameNumber value)
    {
        double clamped = GameData.ClampVisibleNumber(value.ToDoubleClamped());
        if (clamped <= 0d)
        {
            return 0L;
        }

        if (clamped >= long.MaxValue)
        {
            return long.MaxValue;
        }

        return GameData.ClampCount((long)Math.Floor(clamped));
    }

    private void GrantRuneCopiesFromBoxes(long boxes)
    {
        if (boxes <= 0 || runes.Count <= 0)
        {
            return;
        }

        for (long i = 0; i < boxes; i++)
        {
            RuneState state = runes[random.Next(runes.Count)];
            state.Unlocked = true;
            state.Copies = Mathf.Clamp(state.Copies + 1, 0, GameData.MaxIntBalanceValue);
            SaveRuneState(state, false);
        }
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
