using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Battle
{
    public sealed partial class BattleManager : MonoBehaviour
    {
        private const float InitialEnemySpawnGraceSeconds = 0.75f;
        private const float RespawnEnemySpawnGraceSeconds = 1.35f;
        private const float FieldHalfWidth = 3.85f;
        private const float FieldHalfHeight = 5.15f;
        private const float EnemyAttackRange = 0.62f;
        private const float FortressEnemyAttackRange = 0.78f;
        private const float EnemyAttackIntervalSeconds = 1.15f;
        private const float HeroReviveSeconds = 3f;
        private const float HeroSeparationRadius = 0.72f;
        private const float EnemySeparationRadius = 0.42f;
        private const int FortressMaxLevelValue = 300;
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
        private readonly List<RuneState> activeUsableRunes = new List<RuneState>();
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
        private readonly BattleHitFeedbackState hitFeedback = new BattleHitFeedbackState();
        private int activeHeroPreset = 1;
        private int stageRunSequence;
        private int nextEnemySpawnSequence;
        private int fortressLevel = 1;
        private int fortressAttackSequence;
        private GameNumber fortressExperience = GameNumber.Zero;
        private GameNumber fortressHp = GameNumber.One;
        private float fortressAttackCooldown;
        private bool skillAutoEnabled = true;
        private bool feverAutoEnabled = true;
        private bool initialized;

        public event Action<BattleChangeFlags> ChangedWithFlags;

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
        public string TargetName { get; private set; } = string.Empty;
        public GameNumber TargetHp { get; private set; }
        public GameNumber TargetMaxHp { get; private set; }
        public int KillsThisStage { get; private set; }
        public int RequiredKills { get; private set; } = GameData.NormalStageRequiredKills;
        public int VisibleEnemyCount { get; private set; }
        public float BossTimeRemaining { get; private set; }
        public bool IsBossFight { get; private set; }
        public string LastBattleLog { get; private set; } = "전투 준비 중";
        public string LastDamageLog => hitFeedback.LastDamageLog;
        public string LastRewardLog { get; private set; } = string.Empty;
        public string LastHitSourceName => hitFeedback.LastHitSourceName;
        public GameNumber LastHitDamage => hitFeedback.LastHitDamage;
        public bool LastHitWasCritical => hitFeedback.LastHitWasCritical;
        public int HitSequence => hitFeedback.HitSequence;
        public int HeroAttackBatchSequence { get; private set; }
        public int MonsterHitSequence => hitFeedback.MonsterHitSequence;
        public int EnemyDefeatSequence => hitFeedback.EnemyDefeatSequence;
        public int FortressAttackSequence => fortressAttackSequence;
        public int RecentHitEnemyIndex => hitFeedback.RecentHitEnemyIndex;
        public int RecentAttackingEnemyIndex => hitFeedback.RecentAttackingEnemyIndex;
        public int RecentDamagedHeroIndex => hitFeedback.RecentDamagedHeroIndex;
        public Vector2 LastHitPosition => hitFeedback.LastHitPosition;
        public Vector2 LastMonsterHitPosition => hitFeedback.LastMonsterHitPosition;
        public Vector2 LastDefeatedEnemyPosition => hitFeedback.LastDefeatedEnemyPosition;
        public int FortressLevel => fortressLevel;
        public int FortressMaxLevel => FortressMaxLevelValue;
        public GameNumber FortressExperience => fortressExperience;
        public GameNumber FortressCurrentLevelExperience => GetFortressRequiredExperienceForLevel(fortressLevel);
        public GameNumber FortressNextLevelExperience => GetFortressRequiredExperienceForLevel(fortressLevel + 1);
        public GameNumber FortressHp => fortressHp;
        public GameNumber FortressMaxHp => CalculateFortressMaxHp(fortressLevel);
        public GameNumber FortressAttackPower => CalculateFortressAttackPower(fortressLevel);
        public float FortressAttackInterval => CalculateFortressAttackInterval(fortressLevel);
        public float FortressAttackRange => CalculateFortressAttackRange(fortressLevel);
        public float FortressHpRatio => FortressMaxHp > GameNumber.Zero ? Mathf.Clamp01((float)fortressHp.RatioTo(FortressMaxHp)) : 0f;
        public bool CanLevelUpFortress => fortressLevel < FortressMaxLevelValue && fortressExperience >= FortressNextLevelExperience;
        public double FortressCombatPower => CalculateFortressCombatPower(fortressLevel);
        public string SupportStatusText => BuildSupportStatusText();
        public double PartyAttackPower => GetPartyAttackPower();
        public double HeroOwnedAttackBonusPercent => GetHeroOwnedAttackBonusPercent();
        public double TotalCombatPower => GameData.ClampCombatPower(abilityManager != null
            ? abilityManager.GetTotalCombatPower(DeployedHeroes) * GetHeroOwnedAttackMultiplier() * GetTotemAttackMultiplier(null) * GetTotemHpMultiplier(null) * GetRuneAttackMultiplier(null) * GetRuneHpMultiplier() * GetTalentMultiplier(TalentEffectKind.AttackPercent) * GetTalentMultiplier(TalentEffectKind.HpPercent) + FortressCombatPower
            : 1d);
        public float PetGoldBonusPercent => (CombatRewardService.GetPetGoldBonusMultiplier(pets) - 1f) * 100f;
        public bool SkillAutoEnabled => skillAutoEnabled;
        public bool FeverAutoEnabled => feverAutoEnabled;

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
            LoadFortress();
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
            NotifyChanged(BattleChangeFlags.AutoControl);
        }

        public void ToggleFeverAuto()
        {
            feverAutoEnabled = !feverAutoEnabled;
            SaveAutoControlState();
            NotifyChanged(BattleChangeFlags.AutoControl);
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

        private bool ApplyLoggedChange(bool changed, string battleLog, Action onChanged)
        {
            ApplyBattleLog(battleLog);
            if (!changed)
            {
                if (!string.IsNullOrEmpty(battleLog))
                {
                    NotifyChanged(BattleChangeFlags.BattleLog);
                }

                return false;
            }

            onChanged?.Invoke();
            return true;
        }

        private void ApplyBattleLog(string battleLog)
        {
            if (!string.IsNullOrEmpty(battleLog))
            {
                LastBattleLog = battleLog;
            }
        }

    }
}
