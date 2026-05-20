using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class GameHud : MonoBehaviour
{
    private const float EnemyDeathVisualSeconds = 0.28f;
    private const float HeroTranscendAutoRollIntervalSeconds = 0.14f;
    private static readonly StringComparer KoreanNameComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ko-KR"), false);
    private static Sprite roundedPanelSprite;
    private static Sprite roundedButtonSprite;
    private static Sprite roundedPillSprite;
    private static Sprite circleSprite;
    private static Sprite ringSprite;
    private static Sprite coinIconSprite;
    private static Sprite gemIconSprite;
    private static Sprite powerIconSprite;
    private static readonly EquipmentSlot[] HeroDetailEquipmentFilterSlots =
    {
        EquipmentSlot.Weapon,
        EquipmentSlot.Hat,
        EquipmentSlot.Armor,
        EquipmentSlot.Accessory,
        EquipmentSlot.Potion
    };

    private enum HudTab
    {
        Growth,
        Hero,
        Stage,
        Summon,
        Shop,
        Support,
        Debug
    }

    private enum HeroPageTab
    {
        Formation,
        Trait,
        Statue,
        Seal,
        Relic
    }

    private enum HeroDetailTab
    {
        BasicInfo,
        Equipment,
        Transcend
    }

    private StageProgressManager progressManager;
    private CurrencyWallet wallet;
    private AccountProgressManager accountProgressManager;
    private AbilityManager abilityManager;
    private GameSpeedManager speedManager;
    private BattleManager battleManager;
    private GachaManager gachaManager;
    private EquipmentInventory equipmentInventory;
    private BattlefieldWorldView battlefieldWorldView;
    private Action resetSaveAction;

    private HudTab activeTab = HudTab.Growth;
    private bool contentPanelOpen = true;
    private GameObject canvasObject;

    private Text resourceText;
    private Text rubyResourceText;
    private Text stageText;
    private Text modeText;
    private Text accountLevelText;
    private Text targetText;
    private Text hpText;
    private Text progressText;
    private Text supportText;
    private Text logText;
    private Text rewardText;
    private Text damagePopupText;
    private Text supportSummaryText;
    private Text totalCombatPowerText;
    private Text growthNoticeText;
    private Text fieldStagePillText;
    private Text guideQuestText;
    private Text damageMeterText;
    private Text centerSpawnText;
    private Image hpFill;
    private Image accountExpFill;
    private RawImage battlefieldWorldImage;
    private RectTransform battlefieldRect;
    private LayoutElement battleLayoutElement;
    private LayoutElement contentLayoutElement;
    private int observedHitSequence = -1;
    private int observedHeroAttackBatchSequence = -1;
    private int observedBattleKillCount = -1;
    private string observedBattleStageId = string.Empty;
    private float hitFlashRemaining;
    private float heroAttackFlashRemaining;

    private GameObject contentRoot;
    private GameObject growthPanel;
    private GameObject heroPanel;
    private GameObject heroFormationContent;
    private GameObject heroTraitContent;
    private GameObject heroTotemContent;
    private GameObject heroRuneContent;
    private Text heroFormationSummaryText;
    private Text heroTraitSummaryText;
    private Text heroTraitDetailText;
    private Text heroTotemSummaryText;
    private Text heroTotemDetailText;
    private Text heroRuneSummaryText;
    private Text heroRuneDetailText;
    private Text heroFormationTotemText;
    private Text heroFormationTotemSecondText;
    private Text heroOwnedEffectText;
    private Text heroPlaceholderText;
    private GameObject stagePanel;
    private GameObject summonPanel;
    private GameObject shopPanel;
    private GameObject supportPanel;
    private GameObject debugPanel;
    private GameObject heroDetailPanel;
    private GameObject heroDetailStatsPanel;
    private GameObject heroDetailActionRow;
    private GameObject heroDetailEquipmentContent;
    private GameObject heroDetailTranscendContent;
    private GameObject equipmentDetailPopup;
    private GameObject equipmentDismantlePopup;
    private GameObject equipmentBulkDismantlePrompt;
    private GameObject heroTranscendConfirmPrompt;
    private Transform heroDetailEquipmentGridTransform;
    private Transform equipmentDismantleGridTransform;
    private RectTransform heroRosterGridRect;
    private GameObject heroFormationSavePrompt;
    private GameObject guideQuestDot;
    private Text gachaText;
    private Text debugText;
    private Text heroDetailTitleText;
    private Text heroDetailTraitText;
    private Text heroDetailStarsText;
    private Text heroDetailCharacterText;
    private Text heroDetailLevelText;
    private Text heroDetailPowerText;
    private Text heroDetailExpBookText;
    private Text heroDetailSkillText;
    private Text heroDetailStatsText;
    private Text heroDetailStarEffectsText;
    private Text heroDetailOwnedEffectText;
    private Text heroDetailNoticeText;
    private Text heroDetailEquipmentSummaryText;
    private Text heroDetailEquipmentEmptyText;
    private Text heroDetailTranscendText;
    private Text heroDetailTranscendNoticeText;
    private Text heroTranscendConfirmMessageText;
    private Text equipmentDetailIconText;
    private Text equipmentDetailTitleText;
    private Text equipmentDetailMetaText;
    private Text equipmentDetailStatsText;
    private Text equipmentDetailSetText;
    private Text equipmentDetailBookText;
    private Text equipmentDetailNoticeText;
    private Text equipmentDismantleSummaryText;
    private Text equipmentDismantleEmptyText;
    private Text equipmentDismantleNoticeText;
    private Text equipmentBulkDismantleRarityText;
    private Text equipmentBulkDismantleInfoText;
    private Text equipmentBulkDismantleNoticeText;
    private int selectedGrowthLevelStep = 1;
    private int selectedHeroPreset = 1;
    private int pendingHeroPreset = 0;
    private HudTab pendingTabAfterHeroFormationPrompt = HudTab.Growth;
    private string selectedHeroForPlacement = string.Empty;
    private string selectedHeroDetailId = string.Empty;
    private string selectedHeroDetailEquipmentId = string.Empty;
    private string selectedEquipmentDetailId = string.Empty;
    private string selectedTotemId = "TOTEM_COMBAT";
    private string pendingTotemEquipId = string.Empty;
    private int selectedTotemSlot = 1;
    private string selectedRuneId = "RUNE_STRIKE";
    private int selectedRuneSlot = 1;
    private EquipmentSlot selectedHeroDetailEquipmentSlot = EquipmentSlot.Weapon;
    private HeroRarity selectedBulkDismantleRarity = HeroRarity.Rare;
    private int selectedHeroTranscendSlotIndex = 0;
    private string selectedHeroTraitId = "ATK_CORE";
    private HeroPageTab activeHeroPageTab = HeroPageTab.Formation;
    private HeroDetailTab activeHeroDetailTab = HeroDetailTab.BasicInfo;
    private string growthNoticeMessage = string.Empty;
    private float growthNoticeUntil;
    private bool heroFormationDirty;
    private bool heroFormationSavePromptOpen;
    private bool heroDetailPanelOpen;
    private bool heroDetailEquipmentSlotSelectionActive;
    private bool equipmentDetailPopupOpen;
    private bool equipmentDismantlePopupOpen;
    private bool equipmentBulkDismantlePromptOpen;
    private bool heroTranscendStopOnlySs = true;
    private bool pendingHeroTranscendAutoRoll;
    private bool heroTranscendAutoRolling;
    private bool pendingContentOpenAfterHeroFormationPrompt = true;
    private bool pendingHeroPresetSwitch;

    private readonly Dictionary<AbilityKind, Text> abilityButtonTexts = new Dictionary<AbilityKind, Text>();
    private readonly Dictionary<AbilityKind, Text> abilityCostBadgeTexts = new Dictionary<AbilityKind, Text>();
    private readonly Dictionary<string, Text> heroButtonTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Button> heroTraitButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Text> heroTraitButtonTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Button> heroTotemButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Text> heroTotemButtonTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Button> heroRuneButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Text> heroRuneButtonTexts = new Dictionary<string, Text>();
    private readonly Dictionary<int, Button> heroRuneSlotButtons = new Dictionary<int, Button>();
    private readonly Dictionary<int, Text> heroRuneSlotTexts = new Dictionary<int, Text>();
    private readonly Dictionary<HeroPageTab, Button> heroPageTabButtons = new Dictionary<HeroPageTab, Button>();
    private readonly Dictionary<HeroDetailTab, Button> heroDetailTabButtons = new Dictionary<HeroDetailTab, Button>();
    private readonly Dictionary<EquipmentSlot, Button> heroDetailEquipmentSlotButtons = new Dictionary<EquipmentSlot, Button>();
    private readonly Dictionary<EquipmentSlot, Text> heroDetailEquipmentSlotTexts = new Dictionary<EquipmentSlot, Text>();
    private readonly Dictionary<EquipmentSlot, Button> heroDetailEquipmentSlotRemoveButtons = new Dictionary<EquipmentSlot, Button>();
    private readonly Dictionary<EquipmentSlot, Button> heroDetailEquipmentFilterButtons = new Dictionary<EquipmentSlot, Button>();
    private readonly Dictionary<EquipmentSlot, Button> equipmentDismantleFilterButtons = new Dictionary<EquipmentSlot, Button>();
    private readonly HashSet<EquipmentSlot> heroDetailEquipmentSelectedSlots = new HashSet<EquipmentSlot>();
    private readonly Dictionary<string, Button> heroDetailEquipmentCardButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Text> heroDetailEquipmentCardTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Button> heroDetailEquipmentActionButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Button> equipmentDismantleCardButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Text> equipmentDismantleCardTexts = new Dictionary<string, Text>();
    private readonly HashSet<string> selectedDismantleEquipmentIds = new HashSet<string>();
    private readonly List<Button> heroDetailTranscendSlotButtons = new List<Button>();
    private readonly List<Text> heroDetailTranscendSlotTexts = new List<Text>();
    private readonly List<Button> heroDetailTranscendLockButtons = new List<Button>();
    private readonly Dictionary<int, Button> heroPresetButtons = new Dictionary<int, Button>();
    private readonly Dictionary<int, Button> heroFormationSlotButtons = new Dictionary<int, Button>();
    private readonly Dictionary<int, Button> heroFormationSlotRemoveButtons = new Dictionary<int, Button>();
    private readonly Dictionary<string, Button> heroRosterButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Button> heroRosterActionButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, GameObject> heroRosterDeployedOverlays = new Dictionary<string, GameObject>();
    private readonly List<Text> heroFormationSlotTexts = new List<Text>();
    private readonly Dictionary<AbilityKind, GameObject> abilityNotificationDots = new Dictionary<AbilityKind, GameObject>();
    private readonly Dictionary<string, GameObject> heroNotificationDots = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, Text> skillStatusTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Text> petStatusTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, Image> heroBattleImages = new Dictionary<string, Image>();
    private readonly Dictionary<string, Text> heroBattleTexts = new Dictionary<string, Text>();
    private readonly Dictionary<string, RectTransform> heroBattleRects = new Dictionary<string, RectTransform>();
    private readonly Dictionary<int, Button> growthStepButtons = new Dictionary<int, Button>();
    private readonly Dictionary<string, Button> stageButtons = new Dictionary<string, Button>();
    private readonly Dictionary<HudTab, Button> tabButtons = new Dictionary<HudTab, Button>();
    private readonly Dictionary<HudTab, string> tabButtonLabels = new Dictionary<HudTab, string>();
    private readonly Dictionary<HudTab, List<GameObject>> tabNotificationDots = new Dictionary<HudTab, List<GameObject>>();
    private readonly List<string> editingFormationHeroIds = new List<string>();
    private readonly List<Image> enemyBattleImages = new List<Image>();
    private readonly List<Text> enemyBattleTexts = new List<Text>();
    private readonly List<RectTransform> enemyBattleRects = new List<RectTransform>();
    private readonly List<GameObject> enemyHpBarObjects = new List<GameObject>();
    private readonly List<Image> enemyHpFillImages = new List<Image>();
    private readonly List<Vector2> activeHeroBattlePositions = new List<Vector2>();
    private readonly List<Vector2> activeEnemyBattlePositions = new List<Vector2>();
    private readonly List<Vector2> activeEnemyBattlePositionsByIndex = new List<Vector2>();
    private readonly List<bool> activeEnemyBattlePositionStates = new List<bool>();
    private readonly Dictionary<string, Vector2> heroBaseBattlePositions = new Dictionary<string, Vector2>();
    private readonly Dictionary<string, Vector2> displayedHeroBattlePositions = new Dictionary<string, Vector2>();
    private readonly List<Vector2> displayedEnemyBattlePositions = new List<Vector2>();
    private readonly List<bool> displayedEnemyActiveStates = new List<bool>();
    private readonly List<int> displayedEnemySpawnSequences = new List<int>();
    private readonly List<float> displayedEnemyDeathDelays = new List<float>();
    private readonly List<Vector2> displayedEnemyDeathPositions = new List<Vector2>();
    private readonly List<GameObject> damageMeterRows = new List<GameObject>();
    private readonly List<Image> damageMeterFills = new List<Image>();
    private readonly List<Text> damageMeterRowTexts = new List<Text>();
    private Button skillAutoButton;
    private Button feverAutoButton;
    private Button speedCycleButton;
    private Button heroTraitLevelUpButton;
    private Button heroDetailExcludeButton;
    private Button heroDetailLevelUpButton;
    private Button heroDetailStarUpButton;
    private Button heroFormationTotemButton;
    private Button heroFormationTotemSecondButton;
    private Button heroTotemEquipButton;
    private Button heroTotemLevelUpButton;
    private Button heroRuneEquipButton;
    private Button heroRuneLevelUpButton;
    private Button heroDetailTranscendChangeButton;
    private Button heroDetailTranscendAutoButton;
    private Button heroDetailTranscendStopButton;
    private Button equipmentDetailEquipButton;
    private Button equipmentDetailLevelUpButton;
    private Button equipmentDetailStarUpButton;
    private Button equipmentDismantleButton;
    private Button equipmentBulkDismantleButton;
    private Coroutine heroTranscendAutoRollCoroutine;

    public void Initialize(
        StageProgressManager progress,
        CurrencyWallet currency,
        AccountProgressManager accountProgress,
        AbilityManager abilities,
        GameSpeedManager speed,
        BattleManager battle,
        GachaManager gacha,
        EquipmentInventory equipment,
        Action resetSave,
        BattlefieldWorldView worldView = null)
    {
        UnsubscribeEvents();

        progressManager = progress;
        wallet = currency;
        accountProgressManager = accountProgress;
        abilityManager = abilities;
        speedManager = speed;
        battleManager = battle;
        gachaManager = gacha;
        equipmentInventory = equipment;
        battlefieldWorldView = worldView;
        resetSaveAction = resetSave;

        ResetRuntimeUiState();
        CreateEventSystemIfNeeded();
        DestroyExistingHudCanvas();
        CreateHud();

        SubscribeEvents();

        UpdateView();
    }

    private void Update()
    {
        if (battleManager == null || damagePopupText == null)
        {
            return;
        }

        if (observedHitSequence != battleManager.HitSequence)
        {
            observedHitSequence = battleManager.HitSequence;
            hitFlashRemaining = 0.28f;
        }

        if (observedHeroAttackBatchSequence != battleManager.HeroAttackBatchSequence)
        {
            observedHeroAttackBatchSequence = battleManager.HeroAttackBatchSequence;
            heroAttackFlashRemaining = battleManager.HeroAttackBatchSequence > 0 ? 0.28f : 0f;
        }

        if (hitFlashRemaining > 0f)
        {
            hitFlashRemaining = Mathf.Max(0f, hitFlashRemaining - Time.deltaTime);
        }

        if (heroAttackFlashRemaining > 0f)
        {
            heroAttackFlashRemaining = Mathf.Max(0f, heroAttackFlashRemaining - Time.deltaTime);
        }

        if (growthNoticeText != null
            && !string.IsNullOrEmpty(growthNoticeMessage)
            && Time.unscaledTime >= growthNoticeUntil)
        {
            growthNoticeMessage = string.Empty;
            growthNoticeText.text = string.Empty;
            if (heroDetailNoticeText != null)
            {
                heroDetailNoticeText.text = string.Empty;
            }

            if (heroDetailTranscendNoticeText != null)
            {
                heroDetailTranscendNoticeText.text = string.Empty;
            }

            if (equipmentDetailNoticeText != null)
            {
                equipmentDetailNoticeText.text = string.Empty;
            }

            if (equipmentDismantleNoticeText != null)
            {
                equipmentDismantleNoticeText.text = string.Empty;
            }

            if (equipmentBulkDismantleNoticeText != null)
            {
                equipmentBulkDismantleNoticeText.text = string.Empty;
            }
        }

        RefreshBattlefieldVisuals();
        RefreshPendingTotemSlotGlow();
    }

    private void OnDestroy()
    {
        StopHeroTranscendAutoRoll();
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        progressManager.Changed += UpdateView;
        wallet.Changed += UpdateView;
        accountProgressManager.Changed += UpdateView;
        abilityManager.Changed += UpdateView;
        speedManager.Changed += UpdateView;
        battleManager.Changed += UpdateView;
        gachaManager.Changed += UpdateView;
        equipmentInventory.Changed += UpdateView;
    }

    private void UnsubscribeEvents()
    {
        if (progressManager != null)
        {
            progressManager.Changed -= UpdateView;
        }

        if (wallet != null)
        {
            wallet.Changed -= UpdateView;
        }

        if (accountProgressManager != null)
        {
            accountProgressManager.Changed -= UpdateView;
        }

        if (abilityManager != null)
        {
            abilityManager.Changed -= UpdateView;
        }

        if (speedManager != null)
        {
            speedManager.Changed -= UpdateView;
        }

        if (battleManager != null)
        {
            battleManager.Changed -= UpdateView;
        }

        if (gachaManager != null)
        {
            gachaManager.Changed -= UpdateView;
        }

        if (equipmentInventory != null)
        {
            equipmentInventory.Changed -= UpdateView;
        }
    }

    private void ResetRuntimeUiState()
    {
        resourceText = null;
        rubyResourceText = null;
        stageText = null;
        modeText = null;
        accountLevelText = null;
        targetText = null;
        hpText = null;
        progressText = null;
        supportText = null;
        logText = null;
        rewardText = null;
        damagePopupText = null;
        supportSummaryText = null;
        totalCombatPowerText = null;
        growthNoticeText = null;
        fieldStagePillText = null;
        guideQuestText = null;
        damageMeterText = null;
        centerSpawnText = null;
        hpFill = null;
        accountExpFill = null;
        battlefieldWorldImage = null;
        battlefieldRect = null;
        battleLayoutElement = null;
        contentLayoutElement = null;
        observedHitSequence = -1;
        observedHeroAttackBatchSequence = -1;
        observedBattleKillCount = -1;
        observedBattleStageId = string.Empty;
        hitFlashRemaining = 0f;
        heroAttackFlashRemaining = 0f;

        contentPanelOpen = true;
        contentRoot = null;
        growthPanel = null;
        heroPanel = null;
        heroFormationContent = null;
        heroTraitContent = null;
        heroTotemContent = null;
        heroRuneContent = null;
        heroFormationSummaryText = null;
        heroTraitSummaryText = null;
        heroTraitDetailText = null;
        heroTotemSummaryText = null;
        heroTotemDetailText = null;
        heroRuneSummaryText = null;
        heroRuneDetailText = null;
        heroFormationTotemText = null;
        heroFormationTotemSecondText = null;
        heroOwnedEffectText = null;
        heroPlaceholderText = null;
        stagePanel = null;
        summonPanel = null;
        shopPanel = null;
        supportPanel = null;
        debugPanel = null;
        heroDetailPanel = null;
        heroDetailStatsPanel = null;
        heroDetailActionRow = null;
        heroDetailEquipmentContent = null;
        heroDetailTranscendContent = null;
        equipmentDetailPopup = null;
        equipmentDismantlePopup = null;
        equipmentBulkDismantlePrompt = null;
        heroTranscendConfirmPrompt = null;
        heroDetailEquipmentGridTransform = null;
        equipmentDismantleGridTransform = null;
        heroRosterGridRect = null;
        heroFormationSavePrompt = null;
        guideQuestDot = null;
        gachaText = null;
        debugText = null;
        heroDetailTitleText = null;
        heroDetailTraitText = null;
        heroDetailStarsText = null;
        heroDetailCharacterText = null;
        heroDetailLevelText = null;
        heroDetailPowerText = null;
        heroDetailExpBookText = null;
        heroDetailSkillText = null;
        heroDetailStatsText = null;
        heroDetailStarEffectsText = null;
        heroDetailOwnedEffectText = null;
        heroDetailNoticeText = null;
        heroDetailEquipmentSummaryText = null;
        heroDetailEquipmentEmptyText = null;
        heroDetailTranscendText = null;
        heroDetailTranscendNoticeText = null;
        heroTranscendConfirmMessageText = null;
        equipmentDetailIconText = null;
        equipmentDetailTitleText = null;
        equipmentDetailMetaText = null;
        equipmentDetailStatsText = null;
        equipmentDetailSetText = null;
        equipmentDetailBookText = null;
        equipmentDetailNoticeText = null;
        equipmentDismantleSummaryText = null;
        equipmentDismantleEmptyText = null;
        equipmentDismantleNoticeText = null;
        equipmentBulkDismantleRarityText = null;
        equipmentBulkDismantleInfoText = null;
        equipmentBulkDismantleNoticeText = null;
        selectedGrowthLevelStep = 1;
        selectedHeroPreset = 1;
        pendingHeroPreset = 0;
        pendingTabAfterHeroFormationPrompt = HudTab.Growth;
        selectedHeroForPlacement = string.Empty;
        selectedHeroDetailId = string.Empty;
        selectedHeroDetailEquipmentId = string.Empty;
        selectedEquipmentDetailId = string.Empty;
        selectedTotemId = "TOTEM_COMBAT";
        pendingTotemEquipId = string.Empty;
        selectedTotemSlot = 1;
        selectedRuneId = "RUNE_STRIKE";
        selectedRuneSlot = 1;
        selectedDismantleEquipmentIds.Clear();
        selectedHeroDetailEquipmentSlot = EquipmentSlot.Weapon;
        selectedBulkDismantleRarity = HeroRarity.Rare;
        selectedHeroTranscendSlotIndex = 0;
        selectedHeroTraitId = "ATK_CORE";
        activeHeroPageTab = HeroPageTab.Formation;
        activeHeroDetailTab = HeroDetailTab.BasicInfo;
        ResetHeroDetailEquipmentFilters();
        growthNoticeMessage = string.Empty;
        growthNoticeUntil = 0f;
        heroFormationDirty = false;
        heroFormationSavePromptOpen = false;
        heroDetailPanelOpen = false;
        heroDetailEquipmentSlotSelectionActive = false;
        equipmentDetailPopupOpen = false;
        equipmentDismantlePopupOpen = false;
        equipmentBulkDismantlePromptOpen = false;
        heroTranscendStopOnlySs = PlayerPrefs.GetInt(SaveKeys.HeroTranscendStopOnlySs, 1) == 1;
        pendingHeroTranscendAutoRoll = false;
        heroTranscendAutoRolling = false;
        heroTranscendAutoRollCoroutine = null;
        pendingContentOpenAfterHeroFormationPrompt = true;
        pendingHeroPresetSwitch = false;

        abilityButtonTexts.Clear();
        abilityCostBadgeTexts.Clear();
        heroButtonTexts.Clear();
        heroTraitButtons.Clear();
        heroTraitButtonTexts.Clear();
        heroTotemButtons.Clear();
        heroTotemButtonTexts.Clear();
        heroRuneButtons.Clear();
        heroRuneButtonTexts.Clear();
        heroRuneSlotButtons.Clear();
        heroRuneSlotTexts.Clear();
        heroPageTabButtons.Clear();
        heroDetailTabButtons.Clear();
        heroDetailEquipmentSlotButtons.Clear();
        heroDetailEquipmentSlotTexts.Clear();
        heroDetailEquipmentSlotRemoveButtons.Clear();
        heroDetailEquipmentFilterButtons.Clear();
        equipmentDismantleFilterButtons.Clear();
        ResetHeroDetailEquipmentFilters();
        heroDetailEquipmentCardButtons.Clear();
        heroDetailEquipmentCardTexts.Clear();
        heroDetailEquipmentActionButtons.Clear();
        equipmentDismantleCardButtons.Clear();
        equipmentDismantleCardTexts.Clear();
        heroDetailTranscendSlotButtons.Clear();
        heroDetailTranscendSlotTexts.Clear();
        heroDetailTranscendLockButtons.Clear();
        heroPresetButtons.Clear();
        heroFormationSlotButtons.Clear();
        heroFormationSlotRemoveButtons.Clear();
        heroRosterButtons.Clear();
        heroRosterActionButtons.Clear();
        heroRosterDeployedOverlays.Clear();
        heroFormationSlotTexts.Clear();
        abilityNotificationDots.Clear();
        heroNotificationDots.Clear();
        skillStatusTexts.Clear();
        petStatusTexts.Clear();
        heroBattleImages.Clear();
        heroBattleTexts.Clear();
        heroBattleRects.Clear();
        growthStepButtons.Clear();
        stageButtons.Clear();
        tabButtons.Clear();
        tabButtonLabels.Clear();
        tabNotificationDots.Clear();
        editingFormationHeroIds.Clear();
        enemyBattleImages.Clear();
        enemyBattleTexts.Clear();
        enemyBattleRects.Clear();
        enemyHpBarObjects.Clear();
        enemyHpFillImages.Clear();
        activeHeroBattlePositions.Clear();
        activeEnemyBattlePositions.Clear();
        activeEnemyBattlePositionsByIndex.Clear();
        activeEnemyBattlePositionStates.Clear();
        heroBaseBattlePositions.Clear();
        displayedHeroBattlePositions.Clear();
        displayedEnemyBattlePositions.Clear();
        displayedEnemyActiveStates.Clear();
        displayedEnemySpawnSequences.Clear();
        displayedEnemyDeathDelays.Clear();
        displayedEnemyDeathPositions.Clear();
        damageMeterRows.Clear();
        damageMeterFills.Clear();
        damageMeterRowTexts.Clear();
        skillAutoButton = null;
        feverAutoButton = null;
        speedCycleButton = null;
        heroTraitLevelUpButton = null;
        heroDetailExcludeButton = null;
        heroDetailLevelUpButton = null;
        heroDetailStarUpButton = null;
        heroFormationTotemButton = null;
        heroFormationTotemSecondButton = null;
        heroTotemEquipButton = null;
        heroTotemLevelUpButton = null;
        heroRuneEquipButton = null;
        heroRuneLevelUpButton = null;
        heroDetailTranscendChangeButton = null;
        heroDetailTranscendAutoButton = null;
        heroDetailTranscendStopButton = null;
        equipmentDetailEquipButton = null;
        equipmentDetailLevelUpButton = null;
        equipmentDetailStarUpButton = null;
        equipmentDismantleButton = null;
        equipmentBulkDismantleButton = null;
    }

    private void DestroyExistingHudCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == "IdleGameCanvas")
            {
                Destroy(canvas.gameObject);
            }
        }
    }

    private void CreateEventSystemIfNeeded()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
    }

    private void CreateHud()
    {
        canvasObject = new GameObject("IdleGameCanvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject root = CreatePanel("Root", canvasObject.transform, new Color(0.07f, 0.08f, 0.10f, 1f));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(0, 0, 0, 0);
        rootLayout.spacing = 0;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        CreateHeader(root.transform);
        CreateBattlePanel(root.transform);
        CreateContentPanels(root.transform);
        CreateBottomNav(root.transform);
        CreateHeroDetailPanel(root.transform);
        CreateHeroFormationSavePrompt(root.transform);
    }

    private void CreateHeader(Transform parent)
    {
        GameObject panel = CreatePanel("Header", parent, new Color(0.06f, 0.19f, 0.33f, 0.98f));
        AddLayoutElement(panel, -1, 160);

        GameObject avatar = CreatePanel("PlayerAvatar", panel.transform, new Color(0.10f, 0.48f, 0.72f, 1f));
        RectTransform avatarRect = avatar.GetComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0f, 0.5f);
        avatarRect.anchorMax = new Vector2(0f, 0.5f);
        avatarRect.pivot = new Vector2(0f, 0.5f);
        avatarRect.sizeDelta = new Vector2(104f, 104f);
        avatarRect.anchoredPosition = new Vector2(22f, 2f);

        Text avatarText = CreateText("AvatarText", avatar.transform, 38, FontStyle.Bold, TextAnchor.MiddleCenter);
        avatarText.text = "G";
        StretchToParent(avatarText.gameObject);

        stageText = CreateText("Stage", panel.transform, 25, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform stageRect = stageText.GetComponent<RectTransform>();
        stageRect.anchorMin = new Vector2(0f, 0.5f);
        stageRect.anchorMax = new Vector2(0f, 0.5f);
        stageRect.pivot = new Vector2(0f, 0.5f);
        stageRect.sizeDelta = new Vector2(410f, 34f);
        stageRect.anchoredPosition = new Vector2(146f, 42f);

        CreateAnchoredIcon("CombatPowerIcon", panel.transform, GetPowerIconSprite(), new Vector2(146f, 8f), new Vector2(40f, 40f));

        modeText = CreateText("Mode", panel.transform, 31, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform modeRect = modeText.GetComponent<RectTransform>();
        modeRect.anchorMin = new Vector2(0f, 0.5f);
        modeRect.anchorMax = new Vector2(0f, 0.5f);
        modeRect.pivot = new Vector2(0f, 0.5f);
        modeRect.sizeDelta = new Vector2(382f, 42f);
        modeRect.anchoredPosition = new Vector2(192f, 8f);

        GameObject accountExpBar = CreatePanel("AccountExpBar", panel.transform, new Color(0.03f, 0.08f, 0.15f, 1f));
        RectTransform accountBarRect = accountExpBar.GetComponent<RectTransform>();
        accountBarRect.anchorMin = new Vector2(0f, 0.5f);
        accountBarRect.anchorMax = new Vector2(0f, 0.5f);
        accountBarRect.pivot = new Vector2(0f, 0.5f);
        accountBarRect.sizeDelta = new Vector2(430f, 30f);
        accountBarRect.anchoredPosition = new Vector2(146f, -38f);

        accountExpFill = CreatePanel("AccountExpFill", accountExpBar.transform, new Color(0.10f, 0.79f, 0.96f, 1f)).GetComponent<Image>();
        RectTransform accountFillRect = accountExpFill.GetComponent<RectTransform>();
        accountFillRect.anchorMin = Vector2.zero;
        accountFillRect.anchorMax = new Vector2(0f, 1f);
        accountFillRect.offsetMin = Vector2.zero;
        accountFillRect.offsetMax = Vector2.zero;

        accountLevelText = CreateText("AccountLevelText", accountExpBar.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
        StretchToParent(accountLevelText.gameObject);

        GameObject resourcePill = CreatePanel("ResourcePill", panel.transform, new Color(0.03f, 0.09f, 0.16f, 0.96f));
        RectTransform resourceRect = resourcePill.GetComponent<RectTransform>();
        resourceRect.anchorMin = new Vector2(1f, 0.5f);
        resourceRect.anchorMax = new Vector2(1f, 0.5f);
        resourceRect.pivot = new Vector2(1f, 0.5f);
        bool showDebugGrantButton = IsDebugPanelEnabled();
        resourceRect.sizeDelta = new Vector2(showDebugGrantButton ? 330f : 430f, 58f);
        resourceRect.anchoredPosition = new Vector2(showDebugGrantButton ? -194f : -112f, 34f);
        CreateHeaderResourceDisplay(
            resourcePill.transform,
            "GoldResource",
            GetCoinIconSprite(),
            new Vector2(18f, 0f),
            out resourceText);
        CreateHeaderResourceDisplay(
            resourcePill.transform,
            "RubyResource",
            GetGemIconSprite(),
            new Vector2(showDebugGrantButton ? 178f : 226f, 0f),
            out rubyResourceText);

        if (showDebugGrantButton)
        {
            Button debugGrantButton = CreateButton("DBG", panel.transform, 24, new Color(0.26f, 0.18f, 0.12f, 1f));
            RectTransform debugRect = debugGrantButton.GetComponent<RectTransform>();
            debugRect.anchorMin = new Vector2(1f, 0.5f);
            debugRect.anchorMax = new Vector2(1f, 0.5f);
            debugRect.pivot = new Vector2(1f, 0.5f);
            debugRect.sizeDelta = new Vector2(76f, 58f);
            debugRect.anchoredPosition = new Vector2(-108f, 34f);
            debugGrantButton.onClick.AddListener(DebugGrantTestCurrency);
        }

        Button menuButton = CreateButton("≡", panel.transform, 36, new Color(0.12f, 0.16f, 0.22f, 1f));
        RectTransform menuRect = menuButton.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(1f, 0.5f);
        menuRect.anchorMax = new Vector2(1f, 0.5f);
        menuRect.pivot = new Vector2(1f, 0.5f);
        menuRect.sizeDelta = new Vector2(76f, 58f);
        menuRect.anchoredPosition = new Vector2(-22f, 34f);
    }

    private void CreateHeroFormationSavePrompt(Transform parent)
    {
        heroFormationSavePrompt = CreatePanel("HeroFormationSavePrompt", parent, new Color(0f, 0f, 0f, 0.62f));
        LayoutElement overlayLayout = heroFormationSavePrompt.AddComponent<LayoutElement>();
        overlayLayout.ignoreLayout = true;
        StretchToParent(heroFormationSavePrompt);

        GameObject dialog = CreatePanel("HeroFormationSaveDialog", heroFormationSavePrompt.transform, new Color(0.12f, 0.16f, 0.24f, 1f));
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.sizeDelta = new Vector2(660f, 320f);
        dialogRect.anchoredPosition = Vector2.zero;

        Text title = CreateText("HeroFormationSaveTitle", dialog.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 70f);
        titleRect.anchoredPosition = new Vector2(0f, -26f);
        title.text = "편성 저장";

        Text message = CreateText("HeroFormationSaveMessage", dialog.transform, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0f, 0.5f);
        messageRect.anchorMax = new Vector2(1f, 0.5f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.sizeDelta = new Vector2(0f, 92f);
        messageRect.anchoredPosition = new Vector2(0f, 12f);
        message.text = "변경된 영웅 편성을 저장하시겠습니까?\n저장하면 현재 스테이지가 다시 시작됩니다.";

        GameObject buttonRow = new GameObject("HeroFormationSaveButtons", typeof(RectTransform));
        buttonRow.transform.SetParent(dialog.transform, false);
        RectTransform rowRect = buttonRow.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 0f);
        rowRect.anchorMax = new Vector2(1f, 0f);
        rowRect.pivot = new Vector2(0.5f, 0f);
        rowRect.sizeDelta = new Vector2(-72f, 76f);
        rowRect.anchoredPosition = new Vector2(0f, 34f);
        HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 18;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        Button confirmButton = CreateButton("확인", buttonRow.transform, 28, new Color(0.36f, 0.52f, 0.22f, 1f));
        Button cancelButton = CreateButton("취소", buttonRow.transform, 28, new Color(0.26f, 0.29f, 0.34f, 1f));
        confirmButton.onClick.AddListener(ConfirmHeroFormationSavePrompt);
        cancelButton.onClick.AddListener(CancelHeroFormationSavePrompt);

        heroFormationSavePrompt.SetActive(false);
    }

    private void CreateHeroDetailPanel(Transform parent)
    {
        heroDetailPanel = CreatePanel("HeroDetailPanel", parent, new Color(0.04f, 0.06f, 0.12f, 0.96f));
        LayoutElement overlayLayout = heroDetailPanel.AddComponent<LayoutElement>();
        overlayLayout.ignoreLayout = true;
        StretchToParent(heroDetailPanel);
        RectTransform detailPanelRect = heroDetailPanel.GetComponent<RectTransform>();
        detailPanelRect.offsetMin = new Vector2(0f, 130f);

        GameObject header = CreatePanel("HeroDetailHeader", heroDetailPanel.transform, new Color(0.24f, 0.36f, 0.62f, 1f));
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.03f, 1f);
        headerRect.anchorMax = new Vector2(0.97f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.sizeDelta = new Vector2(0f, 74f);
        headerRect.anchoredPosition = new Vector2(0f, -28f);

        heroDetailTitleText = CreateText("HeroDetailTitle", header.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
        StretchToParent(heroDetailTitleText.gameObject);

        heroDetailTraitText = CreateText("HeroDetailTrait", heroDetailPanel.transform, 25, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform traitRect = heroDetailTraitText.GetComponent<RectTransform>();
        traitRect.anchorMin = new Vector2(0f, 1f);
        traitRect.anchorMax = new Vector2(0f, 1f);
        traitRect.pivot = new Vector2(0f, 1f);
        traitRect.sizeDelta = new Vector2(260f, 96f);
        traitRect.anchoredPosition = new Vector2(30f, -132f);

        heroDetailStarsText = CreateText("HeroDetailStars", heroDetailPanel.transform, 30, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform starsRect = heroDetailStarsText.GetComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(0.5f, 1f);
        starsRect.anchorMax = new Vector2(0.5f, 1f);
        starsRect.pivot = new Vector2(0.5f, 1f);
        starsRect.sizeDelta = new Vector2(420f, 52f);
        starsRect.anchoredPosition = new Vector2(0f, -150f);

        heroDetailCharacterText = CreateText("HeroDetailCharacter", heroDetailPanel.transform, 52, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform characterRect = heroDetailCharacterText.GetComponent<RectTransform>();
        characterRect.anchorMin = new Vector2(0.5f, 1f);
        characterRect.anchorMax = new Vector2(0.5f, 1f);
        characterRect.pivot = new Vector2(0.5f, 1f);
        characterRect.sizeDelta = new Vector2(420f, 360f);
        characterRect.anchoredPosition = new Vector2(0f, -222f);

        CreateHeroDetailEquipmentSlot(EquipmentSlot.Weapon, new Vector2(170f, -300f));
        CreateHeroDetailEquipmentSlot(EquipmentSlot.Armor, new Vector2(170f, -424f));
        CreateHeroDetailEquipmentSlot(EquipmentSlot.Potion, new Vector2(170f, -548f));
        CreateHeroDetailEquipmentSlot(EquipmentSlot.Hat, new Vector2(910f, -300f));
        CreateHeroDetailEquipmentSlot(EquipmentSlot.Accessory, new Vector2(910f, -424f));

        heroDetailLevelText = CreateText("HeroDetailLevel", heroDetailPanel.transform, 27, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform levelRect = heroDetailLevelText.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.5f, 1f);
        levelRect.anchorMax = new Vector2(0.5f, 1f);
        levelRect.pivot = new Vector2(0.5f, 1f);
        levelRect.sizeDelta = new Vector2(520f, 52f);
        levelRect.anchoredPosition = new Vector2(0f, -650f);

        GameObject summaryBar = CreatePanel("HeroDetailSummaryBar", heroDetailPanel.transform, new Color(0.39f, 0.50f, 0.67f, 1f));
        RectTransform summaryRect = summaryBar.GetComponent<RectTransform>();
        summaryRect.anchorMin = new Vector2(0f, 1f);
        summaryRect.anchorMax = new Vector2(1f, 1f);
        summaryRect.pivot = new Vector2(0.5f, 1f);
        summaryRect.sizeDelta = new Vector2(0f, 82f);
        summaryRect.anchoredPosition = new Vector2(0f, -712f);

        heroDetailPowerText = CreateText("HeroDetailPower", summaryBar.transform, 32, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform powerRect = heroDetailPowerText.GetComponent<RectTransform>();
        powerRect.anchorMin = new Vector2(0f, 0f);
        powerRect.anchorMax = new Vector2(0.55f, 1f);
        powerRect.offsetMin = new Vector2(28f, 0f);
        powerRect.offsetMax = Vector2.zero;

        heroDetailExpBookText = CreateText("HeroDetailExpBook", summaryBar.transform, 28, FontStyle.Bold, TextAnchor.MiddleRight);
        RectTransform expRect = heroDetailExpBookText.GetComponent<RectTransform>();
        expRect.anchorMin = new Vector2(0.55f, 0f);
        expRect.anchorMax = new Vector2(1f, 1f);
        expRect.offsetMin = Vector2.zero;
        expRect.offsetMax = new Vector2(-28f, 0f);

        heroDetailSkillText = CreateText("HeroDetailSkill", heroDetailPanel.transform, 25, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform skillRect = heroDetailSkillText.GetComponent<RectTransform>();
        skillRect.anchorMin = new Vector2(0.04f, 1f);
        skillRect.anchorMax = new Vector2(0.96f, 1f);
        skillRect.pivot = new Vector2(0.5f, 1f);
        skillRect.sizeDelta = new Vector2(0f, 136f);
        skillRect.anchoredPosition = new Vector2(0f, -820f);

        heroDetailStatsPanel = CreatePanel("HeroDetailStatsPanel", heroDetailPanel.transform, new Color(0.22f, 0.31f, 0.48f, 1f));
        RectTransform statsRect = heroDetailStatsPanel.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.04f, 1f);
        statsRect.anchorMax = new Vector2(0.96f, 1f);
        statsRect.pivot = new Vector2(0.5f, 1f);
        statsRect.sizeDelta = new Vector2(0f, 230f);
        statsRect.anchoredPosition = new Vector2(0f, -972f);

        heroDetailStatsText = CreateText("HeroDetailStats", heroDetailStatsPanel.transform, 26, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform statTextRect = heroDetailStatsText.GetComponent<RectTransform>();
        statTextRect.anchorMin = Vector2.zero;
        statTextRect.anchorMax = Vector2.one;
        statTextRect.offsetMin = new Vector2(24f, 22f);
        statTextRect.offsetMax = new Vector2(-24f, -20f);

        heroDetailStarEffectsText = CreateText("HeroDetailStarEffects", heroDetailStatsPanel.transform, 23, FontStyle.Bold, TextAnchor.LowerLeft);
        RectTransform effectRect = heroDetailStarEffectsText.GetComponent<RectTransform>();
        effectRect.anchorMin = Vector2.zero;
        effectRect.anchorMax = new Vector2(1f, 0.48f);
        effectRect.offsetMin = new Vector2(24f, 14f);
        effectRect.offsetMax = new Vector2(-24f, -6f);

        heroDetailOwnedEffectText = CreateText("HeroDetailOwnedEffect", heroDetailPanel.transform, 26, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform ownedRect = heroDetailOwnedEffectText.GetComponent<RectTransform>();
        ownedRect.anchorMin = new Vector2(0.04f, 1f);
        ownedRect.anchorMax = new Vector2(0.96f, 1f);
        ownedRect.pivot = new Vector2(0.5f, 1f);
        ownedRect.sizeDelta = new Vector2(0f, 54f);
        ownedRect.anchoredPosition = new Vector2(0f, -1222f);

        heroDetailNoticeText = CreateText("HeroDetailNotice", heroDetailPanel.transform, 23, FontStyle.Bold, TextAnchor.MiddleCenter);
        heroDetailNoticeText.color = new Color(1f, 0.58f, 0.34f, 1f);
        RectTransform noticeRect = heroDetailNoticeText.GetComponent<RectTransform>();
        noticeRect.anchorMin = new Vector2(0.05f, 0f);
        noticeRect.anchorMax = new Vector2(0.95f, 0f);
        noticeRect.pivot = new Vector2(0.5f, 0f);
        noticeRect.sizeDelta = new Vector2(0f, 36f);
        noticeRect.anchoredPosition = new Vector2(0f, 194f);

        CreateHeroDetailActionButtons();
        CreateHeroDetailEquipmentContent();
        CreateHeroDetailTranscendContent();
        CreateHeroDetailBottomTabs();
        CreateHeroTranscendConfirmPrompt();
        CreateEquipmentDetailPopup();
        CreateEquipmentDismantlePopup();

        heroDetailPanel.SetActive(false);
    }

    private void CreateHeroTranscendConfirmPrompt()
    {
        heroTranscendConfirmPrompt = CreatePanel("HeroTranscendConfirmPrompt", heroDetailPanel.transform, new Color(0.01f, 0.015f, 0.025f, 0.66f));
        StretchToParent(heroTranscendConfirmPrompt);

        GameObject dialog = CreatePanel("HeroTranscendConfirmDialog", heroTranscendConfirmPrompt.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
        RectTransform dialogRect = dialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.08f, 0.36f);
        dialogRect.anchorMax = new Vector2(0.92f, 0.64f);
        dialogRect.offsetMin = Vector2.zero;
        dialogRect.offsetMax = Vector2.zero;

        Text title = CreateText("HeroTranscendConfirmTitle", dialog.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 64f);
        titleRect.anchoredPosition = new Vector2(0f, -22f);
        title.text = "SS 옵션 변경 확인";

        heroTranscendConfirmMessageText = CreateText("HeroTranscendConfirmMessage", dialog.transform, 26, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform messageRect = heroTranscendConfirmMessageText.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.06f, 0.38f);
        messageRect.anchorMax = new Vector2(0.94f, 0.76f);
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;

        GameObject buttonRow = new GameObject("HeroTranscendConfirmActions", typeof(RectTransform));
        buttonRow.transform.SetParent(dialog.transform, false);
        RectTransform rowRect = buttonRow.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.12f, 0f);
        rowRect.anchorMax = new Vector2(0.88f, 0f);
        rowRect.pivot = new Vector2(0.5f, 0f);
        rowRect.sizeDelta = new Vector2(0f, 74f);
        rowRect.anchoredPosition = new Vector2(0f, 24f);
        HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 18;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        Button confirmButton = CreateButton("확인", buttonRow.transform, 28, new Color(0.54f, 0.76f, 0.96f, 1f));
        Button cancelButton = CreateButton("취소", buttonRow.transform, 28, new Color(0.26f, 0.29f, 0.34f, 1f));
        confirmButton.onClick.AddListener(ConfirmHeroTranscendRollPrompt);
        cancelButton.onClick.AddListener(CancelHeroTranscendRollPrompt);

        heroTranscendConfirmPrompt.SetActive(false);
    }

    private void CreateEquipmentDetailPopup()
    {
        equipmentDetailPopup = CreatePanel("EquipmentDetailPopup", heroDetailPanel.transform, new Color(0.01f, 0.015f, 0.025f, 0.72f));
        StretchToParent(equipmentDetailPopup);

        GameObject modal = CreatePanel("EquipmentDetailModal", equipmentDetailPopup.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
        RectTransform modalRect = modal.GetComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.05f, 0.08f);
        modalRect.anchorMax = new Vector2(0.95f, 0.86f);
        modalRect.offsetMin = Vector2.zero;
        modalRect.offsetMax = Vector2.zero;

        equipmentDetailIconText = CreateText("EquipmentDetailIcon", modal.transform, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform iconRect = equipmentDetailIconText.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 1f);
        iconRect.anchorMax = new Vector2(0f, 1f);
        iconRect.pivot = new Vector2(0f, 1f);
        iconRect.sizeDelta = new Vector2(168f, 150f);
        iconRect.anchoredPosition = new Vector2(34f, -34f);

        equipmentDetailMetaText = CreateText("EquipmentDetailMeta", modal.transform, 25, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform metaRect = equipmentDetailMetaText.GetComponent<RectTransform>();
        metaRect.anchorMin = new Vector2(0f, 1f);
        metaRect.anchorMax = new Vector2(1f, 1f);
        metaRect.pivot = new Vector2(0f, 1f);
        metaRect.sizeDelta = new Vector2(-260f, 62f);
        metaRect.anchoredPosition = new Vector2(220f, -42f);

        equipmentDetailTitleText = CreateText("EquipmentDetailTitle", modal.transform, 36, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform titleRect = equipmentDetailTitleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.sizeDelta = new Vector2(-260f, 92f);
        titleRect.anchoredPosition = new Vector2(220f, -112f);

        GameObject statsPanel = CreatePanel("EquipmentDetailStats", modal.transform, new Color(0.30f, 0.38f, 0.54f, 1f));
        RectTransform statsRect = statsPanel.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.04f, 1f);
        statsRect.anchorMax = new Vector2(0.96f, 1f);
        statsRect.pivot = new Vector2(0.5f, 1f);
        statsRect.sizeDelta = new Vector2(0f, 420f);
        statsRect.anchoredPosition = new Vector2(0f, -215f);

        equipmentDetailStatsText = CreateText("EquipmentDetailStatsText", statsPanel.transform, 30, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform statTextRect = equipmentDetailStatsText.GetComponent<RectTransform>();
        statTextRect.anchorMin = Vector2.zero;
        statTextRect.anchorMax = Vector2.one;
        statTextRect.offsetMin = new Vector2(28f, 240f);
        statTextRect.offsetMax = new Vector2(-28f, -28f);

        equipmentDetailSetText = CreateText("EquipmentDetailSetText", statsPanel.transform, 26, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform setTextRect = equipmentDetailSetText.GetComponent<RectTransform>();
        setTextRect.anchorMin = Vector2.zero;
        setTextRect.anchorMax = Vector2.one;
        setTextRect.offsetMin = new Vector2(28f, 28f);
        setTextRect.offsetMax = new Vector2(-28f, -170f);

        equipmentDetailBookText = CreateText("EquipmentDetailBook", modal.transform, 30, FontStyle.Bold, TextAnchor.MiddleRight);
        RectTransform bookRect = equipmentDetailBookText.GetComponent<RectTransform>();
        bookRect.anchorMin = new Vector2(0.45f, 0f);
        bookRect.anchorMax = new Vector2(0.95f, 0f);
        bookRect.pivot = new Vector2(1f, 0f);
        bookRect.sizeDelta = new Vector2(0f, 64f);
        bookRect.anchoredPosition = new Vector2(0f, 136f);

        equipmentDetailNoticeText = CreateText("EquipmentDetailNotice", modal.transform, 23, FontStyle.Bold, TextAnchor.MiddleCenter);
        equipmentDetailNoticeText.color = new Color(1f, 0.62f, 0.34f, 1f);
        RectTransform noticeRect = equipmentDetailNoticeText.GetComponent<RectTransform>();
        noticeRect.anchorMin = new Vector2(0.06f, 0f);
        noticeRect.anchorMax = new Vector2(0.94f, 0f);
        noticeRect.pivot = new Vector2(0.5f, 0f);
        noticeRect.sizeDelta = new Vector2(0f, 36f);
        noticeRect.anchoredPosition = new Vector2(0f, 103f);

        GameObject actionRow = new GameObject("EquipmentDetailActions", typeof(RectTransform));
        actionRow.transform.SetParent(modal.transform, false);
        RectTransform actionRect = actionRow.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0.04f, 0f);
        actionRect.anchorMax = new Vector2(0.96f, 0f);
        actionRect.pivot = new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(0f, 82f);
        actionRect.anchoredPosition = new Vector2(0f, 26f);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 18;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;

        equipmentDetailEquipButton = CreateButton("장착", actionRow.transform, 27, new Color(0.54f, 0.76f, 0.96f, 1f));
        equipmentDetailLevelUpButton = CreateButton("레벨업", actionRow.transform, 24, new Color(0.54f, 0.78f, 0.22f, 1f));
        equipmentDetailStarUpButton = CreateButton("승급", actionRow.transform, 27, new Color(0.88f, 0.62f, 0.16f, 1f));
        equipmentDetailEquipButton.onClick.AddListener(ToggleSelectedEquipmentDetailEquip);
        ConfigureHoldRepeat(equipmentDetailLevelUpButton, LevelUpSelectedEquipmentDetail, CanLevelUpSelectedEquipmentDetail);
        equipmentDetailStarUpButton.onClick.AddListener(StarUpSelectedEquipmentDetail);

        Button closeButton = CreateButton("X", equipmentDetailPopup.transform, 40, new Color(0.20f, 0.28f, 0.43f, 1f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(112f, 86f);
        closeRect.anchoredPosition = new Vector2(0f, 14f);
        closeButton.onClick.AddListener(CloseEquipmentDetailPopup);

        equipmentDetailPopup.SetActive(false);
    }

    private void CreateEquipmentDismantlePopup()
    {
        equipmentDismantlePopup = CreatePanel("EquipmentDismantlePopup", heroDetailPanel.transform, new Color(0.01f, 0.015f, 0.025f, 0.72f));
        StretchToParent(equipmentDismantlePopup);

        GameObject modal = CreatePanel("EquipmentDismantleModal", equipmentDismantlePopup.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
        RectTransform modalRect = modal.GetComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.05f, 0.06f);
        modalRect.anchorMax = new Vector2(0.95f, 0.86f);
        modalRect.offsetMin = Vector2.zero;
        modalRect.offsetMax = Vector2.zero;

        GameObject titleBar = CreatePanel("EquipmentDismantleTitleBar", modal.transform, new Color(0.25f, 0.37f, 0.63f, 1f));
        RectTransform titleBarRect = titleBar.GetComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0.08f, 1f);
        titleBarRect.anchorMax = new Vector2(0.92f, 1f);
        titleBarRect.pivot = new Vector2(0.5f, 1f);
        titleBarRect.sizeDelta = new Vector2(0f, 72f);
        titleBarRect.anchoredPosition = new Vector2(0f, 34f);
        Text titleText = CreateText("EquipmentDismantleTitle", titleBar.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
        StretchToParent(titleText.gameObject);
        titleText.text = "장비 분해";

        GameObject filterRow = CreatePanel("EquipmentDismantleFilters", modal.transform, new Color(0.30f, 0.38f, 0.54f, 1f));
        RectTransform filterRect = filterRow.GetComponent<RectTransform>();
        filterRect.anchorMin = new Vector2(0.04f, 1f);
        filterRect.anchorMax = new Vector2(0.96f, 1f);
        filterRect.pivot = new Vector2(0.5f, 1f);
        filterRect.sizeDelta = new Vector2(0f, 66f);
        filterRect.anchoredPosition = new Vector2(0f, -72f);
        HorizontalLayoutGroup filterLayout = filterRow.AddComponent<HorizontalLayoutGroup>();
        filterLayout.padding = new RectOffset(12, 12, 6, 6);
        filterLayout.spacing = 8;
        filterLayout.childControlWidth = true;
        filterLayout.childControlHeight = true;
        filterLayout.childForceExpandWidth = true;
        filterLayout.childForceExpandHeight = true;
        foreach (EquipmentSlot slot in HeroDetailEquipmentFilterSlots)
        {
            CreateEquipmentDismantleFilterButton(filterRow.transform, slot);
        }

        equipmentDismantleSummaryText = CreateText("EquipmentDismantleSummary", modal.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform summaryRect = equipmentDismantleSummaryText.GetComponent<RectTransform>();
        summaryRect.anchorMin = new Vector2(0.04f, 1f);
        summaryRect.anchorMax = new Vector2(0.96f, 1f);
        summaryRect.pivot = new Vector2(0.5f, 1f);
        summaryRect.sizeDelta = new Vector2(0f, 42f);
        summaryRect.anchoredPosition = new Vector2(0f, -146f);

        GameObject scrollObject = CreatePanel("EquipmentDismantleScroll", modal.transform, new Color(0.12f, 0.16f, 0.24f, 1f));
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.04f, 0f);
        scrollRectTransform.anchorMax = new Vector2(0.96f, 1f);
        scrollRectTransform.offsetMin = new Vector2(0f, 166f);
        scrollRectTransform.offsetMax = new Vector2(0f, -194f);
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 34f;

        GameObject viewport = CreatePanel("EquipmentDismantleViewport", scrollObject.transform, new Color(0f, 0f, 0f, 0f));
        StretchToParent(viewport);
        viewport.AddComponent<RectMask2D>();
        Image viewportImage = viewport.GetComponent<Image>();
        if (viewportImage != null)
        {
            viewportImage.raycastTarget = true;
        }

        GameObject gridObject = new GameObject("EquipmentDismantleGrid", typeof(RectTransform));
        gridObject.transform.SetParent(viewport.transform, false);
        RectTransform gridRect = gridObject.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(0f, 1f);
        gridRect.pivot = new Vector2(0f, 1f);
        gridRect.sizeDelta = new Vector2(810f, 0f);
        gridRect.anchoredPosition = new Vector2(12f, -12f);
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = gridRect;
        equipmentDismantleGridTransform = gridObject.transform;

        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(146f, 126f);
        grid.spacing = new Vector2(16f, 16f);
        grid.padding = new RectOffset(0, 0, 0, 16);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        ContentSizeFitter gridFitter = gridObject.AddComponent<ContentSizeFitter>();
        gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        equipmentDismantleEmptyText = CreateText("EquipmentDismantleEmpty", modal.transform, 27, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform emptyRect = equipmentDismantleEmptyText.GetComponent<RectTransform>();
        emptyRect.anchorMin = new Vector2(0.04f, 0.36f);
        emptyRect.anchorMax = new Vector2(0.96f, 0.50f);
        emptyRect.offsetMin = Vector2.zero;
        emptyRect.offsetMax = Vector2.zero;
        equipmentDismantleEmptyText.text = "분해할 장비가 없습니다.";

        GameObject actionRow = new GameObject("EquipmentDismantleActions", typeof(RectTransform));
        actionRow.transform.SetParent(modal.transform, false);
        RectTransform actionRect = actionRow.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0.15f, 0f);
        actionRect.anchorMax = new Vector2(0.85f, 0f);
        actionRect.pivot = new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(0f, 82f);
        actionRect.anchoredPosition = new Vector2(0f, 42f);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 24;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;

        equipmentDismantleButton = CreateButton("선택 분해", actionRow.transform, 28, new Color(0.54f, 0.76f, 0.96f, 1f));
        equipmentBulkDismantleButton = CreateButton("일괄 분해", actionRow.transform, 28, new Color(0.54f, 0.76f, 0.96f, 1f));
        equipmentDismantleButton.onClick.AddListener(DismantleSelectedEquipment);
        equipmentBulkDismantleButton.onClick.AddListener(OpenEquipmentBulkDismantlePrompt);

        equipmentDismantleNoticeText = CreateText("EquipmentDismantleNotice", modal.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        equipmentDismantleNoticeText.color = new Color(1f, 0.62f, 0.34f, 1f);
        RectTransform noticeRect = equipmentDismantleNoticeText.GetComponent<RectTransform>();
        noticeRect.anchorMin = new Vector2(0.05f, 0f);
        noticeRect.anchorMax = new Vector2(0.95f, 0f);
        noticeRect.pivot = new Vector2(0.5f, 0f);
        noticeRect.sizeDelta = new Vector2(0f, 34f);
        noticeRect.anchoredPosition = new Vector2(0f, 126f);

        Button closeButton = CreateButton("X", equipmentDismantlePopup.transform, 40, new Color(0.20f, 0.28f, 0.43f, 1f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.sizeDelta = new Vector2(112f, 86f);
        closeRect.anchoredPosition = new Vector2(0f, 14f);
        closeButton.onClick.AddListener(CloseEquipmentDismantlePopup);

        CreateEquipmentBulkDismantlePrompt();
        equipmentDismantlePopup.SetActive(false);
    }

    private void CreateEquipmentDismantleFilterButton(Transform parent, EquipmentSlot slot)
    {
        Button button = CreateButton(BuildEquipmentFilterButtonLabel(slot), parent, 21, new Color(0.24f, 0.30f, 0.42f, 1f));
        button.onClick.AddListener(() => ToggleHeroDetailEquipmentFilter(slot));
        equipmentDismantleFilterButtons[slot] = button;
    }

    private void CreateEquipmentBulkDismantlePrompt()
    {
        equipmentBulkDismantlePrompt = CreatePanel("EquipmentBulkDismantlePrompt", equipmentDismantlePopup.transform, new Color(0.01f, 0.015f, 0.025f, 0.62f));
        StretchToParent(equipmentBulkDismantlePrompt);

        GameObject modal = CreatePanel("EquipmentBulkDismantleModal", equipmentBulkDismantlePrompt.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
        RectTransform modalRect = modal.GetComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.05f, 0.28f);
        modalRect.anchorMax = new Vector2(0.95f, 0.72f);
        modalRect.offsetMin = Vector2.zero;
        modalRect.offsetMax = Vector2.zero;

        GameObject titleBar = CreatePanel("EquipmentBulkDismantleTitleBar", modal.transform, new Color(0.25f, 0.37f, 0.63f, 1f));
        RectTransform titleBarRect = titleBar.GetComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0.08f, 1f);
        titleBarRect.anchorMax = new Vector2(0.92f, 1f);
        titleBarRect.pivot = new Vector2(0.5f, 1f);
        titleBarRect.sizeDelta = new Vector2(0f, 68f);
        titleBarRect.anchoredPosition = new Vector2(0f, 30f);
        Text title = CreateText("EquipmentBulkDismantleTitle", titleBar.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
        StretchToParent(title.gameObject);
        title.text = "장비 일괄 분해";

        Text category = CreateText("EquipmentBulkDismantleCategory", modal.transform, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform categoryRect = category.GetComponent<RectTransform>();
        categoryRect.anchorMin = new Vector2(0.30f, 1f);
        categoryRect.anchorMax = new Vector2(0.70f, 1f);
        categoryRect.pivot = new Vector2(0.5f, 1f);
        categoryRect.sizeDelta = new Vector2(0f, 48f);
        categoryRect.anchoredPosition = new Vector2(0f, -52f);
        category.text = "품질";

        equipmentBulkDismantleInfoText = CreateText("EquipmentBulkDismantleInfo", modal.transform, 30, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform infoRect = equipmentBulkDismantleInfoText.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.08f, 1f);
        infoRect.anchorMax = new Vector2(0.92f, 1f);
        infoRect.pivot = new Vector2(0.5f, 1f);
        infoRect.sizeDelta = new Vector2(0f, 150f);
        infoRect.anchoredPosition = new Vector2(0f, -112f);

        Button leftButton = CreateButton("<", modal.transform, 42, new Color(0.20f, 0.28f, 0.43f, 1f));
        Button rightButton = CreateButton(">", modal.transform, 42, new Color(0.20f, 0.28f, 0.43f, 1f));
        RectTransform leftRect = leftButton.GetComponent<RectTransform>();
        RectTransform rightRect = rightButton.GetComponent<RectTransform>();
        leftRect.anchorMin = leftRect.anchorMax = new Vector2(0.30f, 0.42f);
        rightRect.anchorMin = rightRect.anchorMax = new Vector2(0.70f, 0.42f);
        leftRect.sizeDelta = rightRect.sizeDelta = new Vector2(82f, 82f);
        leftRect.anchoredPosition = Vector2.zero;
        rightRect.anchoredPosition = Vector2.zero;
        leftButton.onClick.AddListener(() => ChangeBulkDismantleRarity(-1));
        rightButton.onClick.AddListener(() => ChangeBulkDismantleRarity(1));

        equipmentBulkDismantleRarityText = CreateText("EquipmentBulkDismantleRarity", modal.transform, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform rarityRect = equipmentBulkDismantleRarityText.GetComponent<RectTransform>();
        rarityRect.anchorMin = rarityRect.anchorMax = new Vector2(0.5f, 0.42f);
        rarityRect.pivot = new Vector2(0.5f, 0.5f);
        rarityRect.sizeDelta = new Vector2(260f, 70f);
        rarityRect.anchoredPosition = Vector2.zero;

        Text protectText = CreateText("EquipmentBulkDismantleProtect", modal.transform, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform protectRect = protectText.GetComponent<RectTransform>();
        protectRect.anchorMin = new Vector2(0.18f, 0f);
        protectRect.anchorMax = new Vector2(0.82f, 0f);
        protectRect.pivot = new Vector2(0.5f, 0f);
        protectRect.sizeDelta = new Vector2(0f, 52f);
        protectRect.anchoredPosition = new Vector2(0f, 140f);
        protectText.text = "[x] 장착 장비 제외";

        Button confirmButton = CreateButton("일괄 분해", modal.transform, 30, new Color(0.54f, 0.76f, 0.96f, 1f));
        RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = confirmRect.anchorMax = new Vector2(0.5f, 0f);
        confirmRect.pivot = new Vector2(0.5f, 0f);
        confirmRect.sizeDelta = new Vector2(300f, 82f);
        confirmRect.anchoredPosition = new Vector2(0f, 56f);
        confirmButton.onClick.AddListener(ConfirmBulkDismantleEquipment);

        equipmentBulkDismantleNoticeText = CreateText("EquipmentBulkDismantleNotice", modal.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        equipmentBulkDismantleNoticeText.color = new Color(1f, 0.62f, 0.34f, 1f);
        RectTransform noticeRect = equipmentBulkDismantleNoticeText.GetComponent<RectTransform>();
        noticeRect.anchorMin = new Vector2(0.05f, 0f);
        noticeRect.anchorMax = new Vector2(0.95f, 0f);
        noticeRect.pivot = new Vector2(0.5f, 0f);
        noticeRect.sizeDelta = new Vector2(0f, 34f);
        noticeRect.anchoredPosition = new Vector2(0f, 18f);

        Button closeButton = CreateButton("X", equipmentBulkDismantlePrompt.transform, 40, new Color(0.20f, 0.28f, 0.43f, 1f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.5f, 0.22f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.sizeDelta = new Vector2(112f, 86f);
        closeRect.anchoredPosition = Vector2.zero;
        closeButton.onClick.AddListener(CloseEquipmentBulkDismantlePrompt);

        equipmentBulkDismantlePrompt.SetActive(false);
    }

    private void CreateHeroDetailEquipmentSlot(EquipmentSlot equipmentSlot, Vector2 anchoredPosition)
    {
        string label = GetEquipmentSlotLabel(equipmentSlot);
        Button slot = CreateButton("+\n" + label, heroDetailPanel.transform, 22, new Color(0.28f, 0.18f, 0.29f, 0.88f));
        RectTransform rect = slot.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(126f, 96f);
        rect.anchoredPosition = anchoredPosition;
        slot.onClick.AddListener(() => TryPlaceSelectedHeroDetailEquipment(equipmentSlot));
        heroDetailEquipmentSlotButtons[equipmentSlot] = slot;

        Text text = slot.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            heroDetailEquipmentSlotTexts[equipmentSlot] = text;
        }

        Button removeButton = CreateCornerActionButton("-", slot.transform, new Color(0.58f, 0.12f, 0.12f, 1f));
        removeButton.onClick.AddListener(() => RemoveHeroDetailEquipment(equipmentSlot));
        heroDetailEquipmentSlotRemoveButtons[equipmentSlot] = removeButton;
    }

    private void CreateHeroDetailEquipmentContent()
    {
        heroDetailEquipmentContent = CreatePanel("HeroDetailEquipmentContent", heroDetailPanel.transform, new Color(0.18f, 0.24f, 0.34f, 0.96f));
        RectTransform contentRect = heroDetailEquipmentContent.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.03f, 1f);
        contentRect.anchorMax = new Vector2(0.97f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 720f);
        contentRect.anchoredPosition = new Vector2(0f, -805f);

        GameObject filterRow = new GameObject("HeroDetailEquipmentFilters", typeof(RectTransform));
        filterRow.transform.SetParent(heroDetailEquipmentContent.transform, false);
        RectTransform filterRect = filterRow.GetComponent<RectTransform>();
        filterRect.anchorMin = new Vector2(0f, 1f);
        filterRect.anchorMax = new Vector2(1f, 1f);
        filterRect.pivot = new Vector2(0.5f, 1f);
        filterRect.sizeDelta = new Vector2(0f, 58f);
        filterRect.anchoredPosition = new Vector2(0f, -12f);
        HorizontalLayoutGroup filterLayout = filterRow.AddComponent<HorizontalLayoutGroup>();
        filterLayout.padding = new RectOffset(10, 10, 0, 0);
        filterLayout.spacing = 8;
        filterLayout.childControlWidth = true;
        filterLayout.childControlHeight = true;
        filterLayout.childForceExpandWidth = true;
        filterLayout.childForceExpandHeight = true;

        foreach (EquipmentSlot slot in HeroDetailEquipmentFilterSlots)
        {
            CreateHeroDetailEquipmentFilterButton(filterRow.transform, slot);
        }

        heroDetailEquipmentSummaryText = CreateText("HeroDetailEquipmentSummary", heroDetailEquipmentContent.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform summaryRect = heroDetailEquipmentSummaryText.GetComponent<RectTransform>();
        summaryRect.anchorMin = new Vector2(0f, 1f);
        summaryRect.anchorMax = new Vector2(1f, 1f);
        summaryRect.pivot = new Vector2(0.5f, 1f);
        summaryRect.sizeDelta = new Vector2(-28f, 42f);
        summaryRect.anchoredPosition = new Vector2(0f, -82f);

        GameObject scrollObject = CreatePanel("HeroDetailEquipmentScroll", heroDetailEquipmentContent.transform, new Color(0.13f, 0.17f, 0.25f, 1f));
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 1f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.pivot = new Vector2(0.5f, 1f);
        scrollRectTransform.sizeDelta = new Vector2(-18f, 438f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, -130f);
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 34f;

        GameObject viewport = CreatePanel("HeroDetailEquipmentViewport", scrollObject.transform, new Color(0f, 0f, 0f, 0f));
        StretchToParent(viewport);
        viewport.AddComponent<RectMask2D>();
        Image viewportImage = viewport.GetComponent<Image>();
        if (viewportImage != null)
        {
            viewportImage.raycastTarget = true;
        }

        GameObject gridObject = new GameObject("HeroDetailEquipmentGrid", typeof(RectTransform));
        gridObject.transform.SetParent(viewport.transform, false);
        RectTransform gridRect = gridObject.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(0f, 1f);
        gridRect.pivot = new Vector2(0f, 1f);
        gridRect.sizeDelta = new Vector2(950f, 0f);
        gridRect.anchoredPosition = new Vector2(10f, -10f);
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = gridRect;
        heroDetailEquipmentGridTransform = gridObject.transform;
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(176f, 128f);
        grid.spacing = new Vector2(10f, 10f);
        grid.padding = new RectOffset(0, 0, 0, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        ContentSizeFitter gridFitter = gridObject.AddComponent<ContentSizeFitter>();
        gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        heroDetailEquipmentEmptyText = CreateText("HeroDetailEquipmentEmpty", heroDetailEquipmentContent.transform, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform emptyRect = heroDetailEquipmentEmptyText.GetComponent<RectTransform>();
        emptyRect.anchorMin = new Vector2(0f, 1f);
        emptyRect.anchorMax = new Vector2(1f, 1f);
        emptyRect.pivot = new Vector2(0.5f, 1f);
        emptyRect.sizeDelta = new Vector2(0f, 180f);
        emptyRect.anchoredPosition = new Vector2(0f, -260f);

        GameObject actionRow = new GameObject("HeroDetailEquipmentActions", typeof(RectTransform));
        actionRow.transform.SetParent(heroDetailEquipmentContent.transform, false);
        RectTransform actionRect = actionRow.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0f, 0f);
        actionRect.anchorMax = new Vector2(1f, 0f);
        actionRect.pivot = new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(-34f, 72f);
        actionRect.anchoredPosition = new Vector2(0f, 24f);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 14;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;
        Button dismantleButton = CreateButton("장비 분해", actionRow.transform, 24, new Color(0.20f, 0.26f, 0.38f, 1f));
        Button bulkUnequipButton = CreateButton("일괄 해제", actionRow.transform, 24, new Color(0.44f, 0.58f, 0.76f, 1f));
        Button bulkEquipButton = CreateButton("일괄 장착", actionRow.transform, 24, new Color(0.54f, 0.78f, 0.22f, 1f));
        dismantleButton.onClick.AddListener(OpenEquipmentDismantlePopup);
        bulkUnequipButton.onClick.AddListener(UnequipAllHeroDetailEquipment);
        bulkEquipButton.onClick.AddListener(AutoEquipHeroDetailEquipment);

        heroDetailEquipmentContent.SetActive(false);
    }

    private void CreateHeroDetailActionButtons()
    {
        heroDetailActionRow = CreatePanel("HeroDetailActionButtons", heroDetailPanel.transform, new Color(0.15f, 0.20f, 0.31f, 1f));
        RectTransform actionRect = heroDetailActionRow.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0.06f, 0f);
        actionRect.anchorMax = new Vector2(0.94f, 0f);
        actionRect.pivot = new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(0f, 78f);
        actionRect.anchoredPosition = new Vector2(0f, 112f);

        HorizontalLayoutGroup actionLayout = heroDetailActionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.padding = new RectOffset(8, 8, 8, 8);
        actionLayout.spacing = 16;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;

        heroDetailExcludeButton = CreateButton("제외", heroDetailActionRow.transform, 27, new Color(0.54f, 0.76f, 0.96f, 1f));
        heroDetailLevelUpButton = CreateButton("레벨업", heroDetailActionRow.transform, 23, new Color(0.34f, 0.36f, 0.34f, 1f));
        heroDetailStarUpButton = CreateButton("승급", heroDetailActionRow.transform, 23, new Color(0.34f, 0.36f, 0.34f, 1f));

        heroDetailExcludeButton.onClick.AddListener(ToggleSelectedHeroDetailFormation);
        ConfigureHoldRepeat(heroDetailLevelUpButton, LevelUpSelectedHeroDetail, CanLevelUpSelectedHeroDetail);
        heroDetailStarUpButton.onClick.AddListener(StarUpSelectedHeroDetail);
    }

    private void CreateHeroDetailEquipmentFilterButton(Transform parent, EquipmentSlot slot)
    {
        Button button = CreateButton(BuildEquipmentFilterButtonLabel(slot), parent, 22, new Color(0.24f, 0.30f, 0.42f, 1f));
        button.onClick.AddListener(() => ToggleHeroDetailEquipmentFilter(slot));
        heroDetailEquipmentFilterButtons[slot] = button;
    }

    private void CreateHeroDetailTranscendContent()
    {
        heroDetailTranscendContent = CreatePanel("HeroDetailTranscendContent", heroDetailPanel.transform, new Color(0.18f, 0.23f, 0.34f, 0.96f));
        RectTransform contentRect = heroDetailTranscendContent.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.03f, 1f);
        contentRect.anchorMax = new Vector2(0.97f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 720f);
        contentRect.anchoredPosition = new Vector2(0f, -805f);

        heroDetailTranscendText = CreateText("HeroDetailTranscendText", heroDetailTranscendContent.transform, 25, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform summaryRect = heroDetailTranscendText.GetComponent<RectTransform>();
        summaryRect.anchorMin = new Vector2(0.04f, 1f);
        summaryRect.anchorMax = new Vector2(0.96f, 1f);
        summaryRect.pivot = new Vector2(0.5f, 1f);
        summaryRect.sizeDelta = new Vector2(0f, 58f);
        summaryRect.anchoredPosition = new Vector2(0f, -18f);
        heroDetailTranscendText.text = "초월은 다음 단계에서 구현";
        for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
        {
            int slotIndex = i;
            Button slotButton = CreateButton(string.Empty, heroDetailTranscendContent.transform, 24, new Color(0.22f, 0.29f, 0.43f, 1f));
            RectTransform slotRect = slotButton.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.04f, 1f);
            slotRect.anchorMax = new Vector2(0.96f, 1f);
            slotRect.pivot = new Vector2(0.5f, 1f);
            slotRect.sizeDelta = new Vector2(0f, 78f);
            slotRect.anchoredPosition = new Vector2(0f, -82f - i * 84f);
            slotButton.onClick.AddListener(() =>
            {
                selectedHeroTranscendSlotIndex = slotIndex;
                UpdateView();
            });

            Text slotText = slotButton.GetComponentInChildren<Text>(true);
            if (slotText != null)
            {
                slotText.alignment = TextAnchor.MiddleLeft;
                RectTransform slotTextRect = slotText.GetComponent<RectTransform>();
                slotTextRect.offsetMin = new Vector2(22f, 6f);
                slotTextRect.offsetMax = new Vector2(-22f, -6f);
                heroDetailTranscendSlotTexts.Add(slotText);
            }

            heroDetailTranscendSlotButtons.Add(slotButton);

            Button lockButton = CreateCornerActionButton("잠", slotButton.transform, new Color(0.20f, 0.25f, 0.36f, 1f));
            lockButton.onClick.AddListener(() => ToggleHeroTranscendSlotLock(slotIndex));
            heroDetailTranscendLockButtons.Add(lockButton);
        }

        heroDetailTranscendStopButton = CreateButton("[x] 자동 변경시 SS만 정지", heroDetailTranscendContent.transform, 25, new Color(0.26f, 0.32f, 0.43f, 1f));
        RectTransform stopRect = heroDetailTranscendStopButton.GetComponent<RectTransform>();
        stopRect.anchorMin = new Vector2(0.18f, 0f);
        stopRect.anchorMax = new Vector2(0.82f, 0f);
        stopRect.pivot = new Vector2(0.5f, 0f);
        stopRect.sizeDelta = new Vector2(0f, 56f);
        stopRect.anchoredPosition = new Vector2(0f, 122f);
        heroDetailTranscendStopButton.onClick.AddListener(ToggleHeroTranscendStopMode);

        GameObject actionRow = new GameObject("HeroDetailTranscendActions", typeof(RectTransform));
        actionRow.transform.SetParent(heroDetailTranscendContent.transform, false);
        RectTransform actionRect = actionRow.GetComponent<RectTransform>();
        actionRect.anchorMin = new Vector2(0.04f, 0f);
        actionRect.anchorMax = new Vector2(0.96f, 0f);
        actionRect.pivot = new Vector2(0.5f, 0f);
        actionRect.sizeDelta = new Vector2(0f, 82f);
        actionRect.anchoredPosition = new Vector2(0f, 28f);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 18;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;

        heroDetailTranscendChangeButton = CreateButton("변경\n10", actionRow.transform, 25, new Color(0.28f, 0.72f, 0.92f, 1f));
        heroDetailTranscendAutoButton = CreateButton("자동 변경", actionRow.transform, 25, new Color(0.70f, 0.24f, 0.82f, 1f));
        heroDetailTranscendChangeButton.onClick.AddListener(RollSelectedHeroTranscendManual);
        heroDetailTranscendAutoButton.onClick.AddListener(AutoRollSelectedHeroTranscend);

        heroDetailTranscendNoticeText = CreateText("HeroDetailTranscendNotice", heroDetailTranscendContent.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        heroDetailTranscendNoticeText.color = new Color(1f, 0.64f, 0.34f, 1f);
        RectTransform noticeRect = heroDetailTranscendNoticeText.GetComponent<RectTransform>();
        noticeRect.anchorMin = new Vector2(0.04f, 0f);
        noticeRect.anchorMax = new Vector2(0.96f, 0f);
        noticeRect.pivot = new Vector2(0.5f, 0f);
        noticeRect.sizeDelta = new Vector2(0f, 34f);
        noticeRect.anchoredPosition = new Vector2(0f, 92f);

        heroDetailTranscendContent.SetActive(false);
    }

    private void CreateHeroDetailBottomTabs()
    {
        GameObject tabRow = CreatePanel("HeroDetailBottomTabs", heroDetailPanel.transform, new Color(0.12f, 0.18f, 0.30f, 1f));
        RectTransform tabRect = tabRow.GetComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0.03f, 0f);
        tabRect.anchorMax = new Vector2(0.97f, 0f);
        tabRect.pivot = new Vector2(0.5f, 0f);
        tabRect.sizeDelta = new Vector2(0f, 82f);
        tabRect.anchoredPosition = new Vector2(0f, 20f);
        HorizontalLayoutGroup tabLayout = tabRow.AddComponent<HorizontalLayoutGroup>();
        tabLayout.padding = new RectOffset(8, 8, 8, 8);
        tabLayout.spacing = 10;
        tabLayout.childControlWidth = true;
        tabLayout.childControlHeight = true;
        tabLayout.childForceExpandWidth = true;
        tabLayout.childForceExpandHeight = true;

        CreateHeroDetailTabButton(tabRow.transform, HeroDetailTab.BasicInfo, "기본 정보");
        CreateHeroDetailTabButton(tabRow.transform, HeroDetailTab.Equipment, "장비");
        CreateHeroDetailTabButton(tabRow.transform, HeroDetailTab.Transcend, "초월");
    }

    private void CreateHeroDetailTabButton(Transform parent, HeroDetailTab tab, string label)
    {
        Button button = CreateButton(label, parent, 25, new Color(0.20f, 0.27f, 0.42f, 1f));
        button.onClick.AddListener(() => SelectHeroDetailTab(tab));
        heroDetailTabButtons[tab] = button;
    }

    private void CreateBattlePanel(Transform parent)
    {
        GameObject panel = CreatePanel("Battle", parent, new Color(0.14f, 0.16f, 0.20f, 1f));
        battleLayoutElement = AddLayoutElement(panel, -1, 870);

        CreateBattlefieldPanel(panel.transform);

        targetText = CreateText("Target", panel.transform, 36, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform targetRect = targetText.GetComponent<RectTransform>();
        targetRect.anchorMin = new Vector2(0.5f, 1f);
        targetRect.anchorMax = new Vector2(0.5f, 1f);
        targetRect.pivot = new Vector2(0.5f, 1f);
        targetRect.sizeDelta = new Vector2(420f, 48f);
        targetRect.anchoredPosition = new Vector2(0f, -88f);
        targetText.gameObject.SetActive(false);

        GameObject hpBar = CreatePanel("KillProgressBar", panel.transform, new Color(0.03f, 0.04f, 0.05f, 1f));
        RectTransform hpRect = hpBar.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0.5f, 1f);
        hpRect.anchorMax = new Vector2(0.5f, 1f);
        hpRect.pivot = new Vector2(0.5f, 1f);
        hpRect.sizeDelta = new Vector2(430f, 34f);
        hpRect.anchoredPosition = new Vector2(0f, -108f);
        hpFill = CreatePanel("KillProgressFill", hpBar.transform, new Color(0.95f, 0.63f, 0.17f, 1f)).GetComponent<Image>();
        RectTransform fillRect = hpFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        hpText = CreateText("KillProgressText", hpBar.transform, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
        hpTextRect.anchorMin = Vector2.zero;
        hpTextRect.anchorMax = Vector2.one;
        hpTextRect.offsetMin = Vector2.zero;
        hpTextRect.offsetMax = Vector2.zero;

        progressText = CreateText("Progress", panel.transform, 30, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform progressRect = progressText.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.5f, 1f);
        progressRect.anchorMax = new Vector2(0.5f, 1f);
        progressRect.pivot = new Vector2(0.5f, 1f);
        progressRect.sizeDelta = new Vector2(450f, 42f);
        progressRect.anchoredPosition = new Vector2(0f, -148f);

        supportText = CreateText("Support", panel.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform supportRect = supportText.GetComponent<RectTransform>();
        supportRect.anchorMin = new Vector2(0f, 1f);
        supportRect.anchorMax = new Vector2(0f, 1f);
        supportRect.pivot = new Vector2(0f, 1f);
        supportRect.sizeDelta = new Vector2(330f, 86f);
        supportRect.anchoredPosition = new Vector2(26f, -214f);

        CreateCombatSpeedControls(panel.transform);

        logText = CreateText("Log", panel.transform, 22, FontStyle.Bold, TextAnchor.LowerLeft);
        RectTransform logRect = logText.GetComponent<RectTransform>();
        logRect.anchorMin = new Vector2(0f, 0f);
        logRect.anchorMax = new Vector2(0f, 0f);
        logRect.pivot = new Vector2(0f, 0f);
        logRect.sizeDelta = new Vector2(460f, 80f);
        logRect.anchoredPosition = new Vector2(26f, 26f);

        rewardText = CreateText("Reward", panel.transform, 22, FontStyle.Bold, TextAnchor.LowerLeft);
        rewardText.color = new Color(1f, 0.86f, 0.36f, 1f);
        RectTransform rewardRect = rewardText.GetComponent<RectTransform>();
        rewardRect.anchorMin = new Vector2(0f, 0f);
        rewardRect.anchorMax = new Vector2(0f, 0f);
        rewardRect.pivot = new Vector2(0f, 0f);
        rewardRect.sizeDelta = new Vector2(460f, 38f);
        rewardRect.anchoredPosition = new Vector2(26f, 106f);

        hpBar.transform.SetAsLastSibling();
        progressText.transform.SetAsLastSibling();
        supportText.transform.SetAsLastSibling();
        logText.transform.SetAsLastSibling();
        rewardText.transform.SetAsLastSibling();
    }

    private void CreateBattlefieldPanel(Transform parent)
    {
        GameObject field = CreatePanel("Battlefield", parent, new Color(0.18f, 0.20f, 0.24f, 1f));
        StretchToParent(field);
        battlefieldRect = field.GetComponent<RectTransform>();

        if (battlefieldWorldView != null && battlefieldWorldView.OutputTexture != null)
        {
            GameObject worldRender = new GameObject("BattlefieldWorldRender", typeof(RectTransform), typeof(RawImage));
            worldRender.transform.SetParent(field.transform, false);
            StretchToParent(worldRender);
            battlefieldWorldImage = worldRender.GetComponent<RawImage>();
            battlefieldWorldImage.texture = battlefieldWorldView.OutputTexture;
            battlefieldWorldImage.color = Color.white;
            battlefieldWorldImage.raycastTarget = false;
        }

        centerSpawnText = CreateText("SpawnPortal", field.transform, 82, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform portalRect = centerSpawnText.GetComponent<RectTransform>();
        portalRect.anchorMin = new Vector2(0.5f, 0.5f);
        portalRect.anchorMax = new Vector2(0.5f, 0.5f);
        portalRect.pivot = new Vector2(0.5f, 0.5f);
        portalRect.sizeDelta = new Vector2(120f, 120f);
        portalRect.anchoredPosition = Vector2.zero;
        centerSpawnText.text = "●";
        centerSpawnText.color = new Color(0.95f, 0.12f, 0.10f, 0.85f);

        GameObject stagePill = CreatePanel("FieldStagePill", field.transform, new Color(0.88f, 0.90f, 0.92f, 0.94f));
        RectTransform pillRect = stagePill.GetComponent<RectTransform>();
        pillRect.anchorMin = new Vector2(0.5f, 1f);
        pillRect.anchorMax = new Vector2(0.5f, 1f);
        pillRect.pivot = new Vector2(0.5f, 1f);
        pillRect.sizeDelta = new Vector2(210f, 58f);
        pillRect.anchoredPosition = new Vector2(0f, -10f);
        fieldStagePillText = CreateText("FieldStagePillText", stagePill.transform, 27, FontStyle.Bold, TextAnchor.MiddleCenter);
        fieldStagePillText.color = new Color(0.04f, 0.05f, 0.07f, 1f);
        StretchToParent(fieldStagePillText.gameObject);

        damagePopupText = CreateText("DamagePopup", field.transform, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform damageRect = damagePopupText.GetComponent<RectTransform>();
        damageRect.anchorMin = new Vector2(0.5f, 0.5f);
        damageRect.anchorMax = new Vector2(0.5f, 0.5f);
        damageRect.pivot = new Vector2(0.5f, 0.5f);
        damageRect.sizeDelta = new Vector2(260f, 92f);
        damageRect.anchoredPosition = new Vector2(0f, 18f);

        GameObject damageMeter = CreatePanel("DamageMeter", field.transform, new Color(0.02f, 0.025f, 0.035f, 0.76f));
        RectTransform damageMeterRect = damageMeter.GetComponent<RectTransform>();
        damageMeterRect.anchorMin = new Vector2(1f, 0f);
        damageMeterRect.anchorMax = new Vector2(1f, 0f);
        damageMeterRect.pivot = new Vector2(1f, 0f);
        damageMeterRect.sizeDelta = new Vector2(270f, 248f);
        damageMeterRect.anchoredPosition = new Vector2(-12f, 244f);
        damageMeterText = CreateText("DamageMeterTitle", damageMeter.transform, 20, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform damageMeterTextRect = damageMeterText.GetComponent<RectTransform>();
        damageMeterTextRect.anchorMin = new Vector2(0f, 1f);
        damageMeterTextRect.anchorMax = new Vector2(1f, 1f);
        damageMeterTextRect.pivot = new Vector2(0.5f, 1f);
        damageMeterTextRect.sizeDelta = new Vector2(-20f, 28f);
        damageMeterTextRect.anchoredPosition = new Vector2(0f, -8f);
        damageMeterText.text = "데미지 미터기";

        GameObject meterRows = new GameObject("DamageMeterRows", typeof(RectTransform));
        meterRows.transform.SetParent(damageMeter.transform, false);
        RectTransform rowsRect = meterRows.GetComponent<RectTransform>();
        rowsRect.anchorMin = new Vector2(0f, 0f);
        rowsRect.anchorMax = new Vector2(1f, 1f);
        rowsRect.offsetMin = new Vector2(10f, 8f);
        rowsRect.offsetMax = new Vector2(-10f, -40f);
        VerticalLayoutGroup meterLayout = meterRows.AddComponent<VerticalLayoutGroup>();
        meterLayout.spacing = 4;
        meterLayout.childControlWidth = true;
        meterLayout.childControlHeight = true;
        meterLayout.childForceExpandWidth = true;
        meterLayout.childForceExpandHeight = true;

        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            CreateDamageMeterRow(meterRows.transform, i);
        }

        GameObject guideQuest = CreatePanel("GuideQuestCard", field.transform, new Color(0.03f, 0.04f, 0.06f, 0.78f));
        RectTransform guideRect = guideQuest.GetComponent<RectTransform>();
        guideRect.anchorMin = new Vector2(1f, 1f);
        guideRect.anchorMax = new Vector2(1f, 1f);
        guideRect.pivot = new Vector2(1f, 1f);
        guideRect.sizeDelta = new Vector2(310f, 86f);
        guideRect.anchoredPosition = new Vector2(-12f, -80f);
        guideQuestText = CreateText("GuideQuestText", guideQuest.transform, 21, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform guideTextRect = guideQuestText.GetComponent<RectTransform>();
        guideTextRect.anchorMin = Vector2.zero;
        guideTextRect.anchorMax = Vector2.one;
        guideTextRect.offsetMin = new Vector2(18f, 8f);
        guideTextRect.offsetMax = new Vector2(-18f, -8f);
        guideQuestDot = CreateNotificationDot(guideQuest.transform, 34f, new Vector2(-12f, -12f));

        foreach (HeroDefinition hero in GameData.Heroes)
        {
            GameObject actor = CreateBattleActor(hero.Id + "HeroActor", field.transform, new Vector2(74f, 74f), new Color(0.16f, 0.24f, 0.34f, 1f));
            Image image = actor.GetComponent<Image>();
            Text label = CreateText(hero.Id + "BattleLabel", actor.transform, 19, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchToParent(label.gameObject);
            label.text = GetRarityBadge(hero.Rarity) + "\n" + GetShortHeroLabel(hero);
            heroBattleRects[hero.Id] = actor.GetComponent<RectTransform>();
            heroBattleImages[hero.Id] = image;
            heroBattleTexts[hero.Id] = label;
        }

        for (int i = 0; i < GameData.MaxVisibleEnemies; i++)
        {
            GameObject enemy = CreateBattleActor("EnemyActor" + i, field.transform, new Vector2(58f, 58f), new Color(0.56f, 0.13f, 0.11f, 1f));
            Image image = enemy.GetComponent<Image>();
            Text label = CreateText("Enemy" + i + "Text", enemy.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            StretchToParent(label.gameObject);
            label.text = "M";

            GameObject enemyHpBar = CreatePanel("EnemyHpBar" + i, enemy.transform, new Color(0.02f, 0.025f, 0.03f, 0.92f));
            RectTransform enemyHpRect = enemyHpBar.GetComponent<RectTransform>();
            enemyHpRect.anchorMin = new Vector2(0.5f, 1f);
            enemyHpRect.anchorMax = new Vector2(0.5f, 1f);
            enemyHpRect.pivot = new Vector2(0.5f, 0f);
            enemyHpRect.sizeDelta = new Vector2(54f, 8f);
            enemyHpRect.anchoredPosition = new Vector2(0f, 4f);

            Image enemyHpFill = CreatePanel("EnemyHpFill" + i, enemyHpBar.transform, new Color(0.35f, 0.93f, 0.28f, 1f)).GetComponent<Image>();
            RectTransform enemyHpFillRect = enemyHpFill.GetComponent<RectTransform>();
            enemyHpFillRect.anchorMin = Vector2.zero;
            enemyHpFillRect.anchorMax = Vector2.one;
            enemyHpFillRect.offsetMin = Vector2.zero;
            enemyHpFillRect.offsetMax = Vector2.zero;

            enemyBattleRects.Add(enemy.GetComponent<RectTransform>());
            enemyBattleImages.Add(image);
            enemyBattleTexts.Add(label);
            enemyHpBarObjects.Add(enemyHpBar);
            enemyHpFillImages.Add(enemyHpFill);
            displayedEnemyBattlePositions.Add(Vector2.zero);
            displayedEnemyActiveStates.Add(false);
            displayedEnemySpawnSequences.Add(-1);
            displayedEnemyDeathDelays.Add(0f);
            displayedEnemyDeathPositions.Add(Vector2.zero);
            activeEnemyBattlePositionsByIndex.Add(Vector2.zero);
            activeEnemyBattlePositionStates.Add(false);
        }

        stagePill.transform.SetAsLastSibling();
        guideQuest.transform.SetAsLastSibling();
        damageMeter.transform.SetAsLastSibling();
        damagePopupText.transform.SetAsLastSibling();
    }

    private void CreateDamageMeterRow(Transform parent, int index)
    {
        GameObject row = CreatePanel("DamageMeterRow" + index, parent, new Color(0.08f, 0.10f, 0.15f, 0.92f));
        Image fill = CreatePanel("DamageMeterFill" + index, row.transform, new Color(0.42f, 0.62f, 0.32f, 0.78f)).GetComponent<Image>();
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Text text = CreateText("DamageMeterRowText" + index, row.transform, 16, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 0f);
        textRect.offsetMax = new Vector2(-8f, 0f);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 11;
        text.resizeTextMaxSize = 16;

        damageMeterRows.Add(row);
        damageMeterFills.Add(fill);
        damageMeterRowTexts.Add(text);
    }

    private void CreateCombatSpeedControls(Transform parent)
    {
        GameObject controlRow = new GameObject("BattleAutoControls", typeof(RectTransform));
        controlRow.transform.SetParent(parent, false);
        RectTransform controlRect = controlRow.GetComponent<RectTransform>();
        controlRect.anchorMin = new Vector2(1f, 0f);
        controlRect.anchorMax = new Vector2(1f, 0f);
        controlRect.pivot = new Vector2(1f, 0f);
        controlRect.sizeDelta = new Vector2(390f, 78f);
        controlRect.anchoredPosition = new Vector2(-26f, 154f);

        HorizontalLayoutGroup row = controlRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 10;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = true;

        skillAutoButton = CreateAutoControlButton("스킬\nAUTO", controlRow.transform);
        skillAutoButton.onClick.AddListener(() => battleManager.ToggleSkillAuto());

        feverAutoButton = CreateAutoControlButton("피버\nAUTO", controlRow.transform);
        feverAutoButton.onClick.AddListener(() => battleManager.ToggleFeverAuto());

        speedCycleButton = CreateAutoControlButton("가속\n1x", controlRow.transform);
        speedCycleButton.onClick.AddListener(() => speedManager.CycleSpeed());
    }

    private Button CreateAutoControlButton(string label, Transform parent)
    {
        Button button = CreateButton(label, parent, 21, new Color(0.25f, 0.25f, 0.20f, 1f));
        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = 22;
        }

        return button;
    }

    private void RefreshAutoControlButton(Button button, string label, bool enabled, Color enabledColor, Color disabledColor)
    {
        button.interactable = true;
        SetButtonColor(button, enabled ? enabledColor : disabledColor);

        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label + "\nAUTO " + (enabled ? "켜짐" : "꺼짐");
        }
    }

    private void CreateContentPanels(Transform parent)
    {
        contentRoot = CreatePanel("Content", parent, new Color(0.09f, 0.10f, 0.13f, 1f));
        contentLayoutElement = AddLayoutElement(contentRoot, -1, 760);

        RectTransform contentRect = contentRoot.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);

        growthPanel = CreatePanel("GrowthPanel", contentRoot.transform, new Color(0.13f, 0.16f, 0.22f, 1f));
        heroPanel = CreatePanel("HeroPanel", contentRoot.transform, new Color(0.12f, 0.16f, 0.23f, 1f));
        stagePanel = CreatePanel("StagePanel", contentRoot.transform, new Color(0.12f, 0.15f, 0.19f, 1f));
        summonPanel = CreatePanel("SummonPanel", contentRoot.transform, new Color(0.15f, 0.13f, 0.19f, 1f));
        shopPanel = CreatePanel("ShopPanel", contentRoot.transform, new Color(0.16f, 0.13f, 0.10f, 1f));
        supportPanel = CreatePanel("SupportPanel", contentRoot.transform, new Color(0.11f, 0.15f, 0.16f, 1f));

        StretchToParent(growthPanel);
        StretchToParent(heroPanel);
        StretchToParent(stagePanel);
        StretchToParent(summonPanel);
        StretchToParent(shopPanel);
        StretchToParent(supportPanel);

        CreateGrowthPanel(growthPanel.transform);
        CreateHeroPanel(heroPanel.transform);
        CreateStagePanel(stagePanel.transform);
        CreateSummonPanel(summonPanel.transform);
        CreateShopPanel(shopPanel.transform);
        CreateSupportPanel(supportPanel.transform);

        if (IsDebugPanelEnabled())
        {
            debugPanel = CreatePanel("DebugPanel", contentRoot.transform, new Color(0.13f, 0.13f, 0.13f, 1f));
            StretchToParent(debugPanel);
            CreateDebugPanel(debugPanel.transform);
        }
    }

    private void CreateGrowthPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 6;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("GrowthTitle", parent, 32, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "성장";
        AddLayoutElement(title.gameObject, -1, 38);

        totalCombatPowerText = CreateText("TotalCombatPower", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        totalCombatPowerText.text = "종합 전투력 0";
        AddLayoutElement(totalCombatPowerText.gameObject, -1, 42);

        growthNoticeText = CreateText("GrowthNotice", parent, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        growthNoticeText.color = new Color(1f, 0.55f, 0.34f, 1f);
        AddLayoutElement(growthNoticeText.gameObject, -1, 30);

        GameObject stepRow = new GameObject("GrowthStepRow", typeof(RectTransform));
        stepRow.transform.SetParent(parent, false);
        HorizontalLayoutGroup stepLayout = stepRow.AddComponent<HorizontalLayoutGroup>();
        stepLayout.spacing = 10;
        stepLayout.childControlWidth = true;
        stepLayout.childControlHeight = true;
        stepLayout.childForceExpandWidth = true;
        stepLayout.childForceExpandHeight = true;
        AddLayoutElement(stepRow, -1, 50);

        int[] steps = { 1, 10, 100, 1000 };
        foreach (int step in steps)
        {
            Button stepButton = CreateButton(step + "x", stepRow.transform, 24, new Color(0.18f, 0.24f, 0.38f, 1f));
            int capturedStep = step;
            stepButton.onClick.AddListener(() =>
            {
                selectedGrowthLevelStep = capturedStep;
                UpdateView();
            });
            growthStepButtons[step] = stepButton;
        }

        foreach (AbilityState ability in abilityManager.States)
        {
            Button button = CreateButton(ability.Definition.DisplayName, parent, 22, new Color(0.48f, 0.54f, 0.66f, 1f));
            AddLayoutElement(button.gameObject, -1, 64);

            AbilityKind kind = ability.Definition.Kind;
            ConfigureHoldRepeat(button, () => TryLevelUpAbilityFromHud(kind), () => CanLevelUpAbilityFromHud(kind));
            Text rowText = button.GetComponentInChildren<Text>();
            rowText.alignment = TextAnchor.MiddleLeft;
            rowText.color = Color.white;
            RectTransform rowTextRect = rowText.GetComponent<RectTransform>();
            rowTextRect.anchorMin = Vector2.zero;
            rowTextRect.anchorMax = new Vector2(0.70f, 1f);
            rowTextRect.offsetMin = new Vector2(24f, 4f);
            rowTextRect.offsetMax = new Vector2(-8f, -4f);
            abilityButtonTexts[kind] = rowText;

            GameObject costBadge = CreatePanel(ability.Definition.Id + "CostBadge", button.transform, new Color(0.56f, 0.88f, 0.24f, 1f));
            RectTransform costBadgeRect = costBadge.GetComponent<RectTransform>();
            costBadgeRect.anchorMin = new Vector2(0.73f, 0.14f);
            costBadgeRect.anchorMax = new Vector2(0.98f, 0.86f);
            costBadgeRect.offsetMin = Vector2.zero;
            costBadgeRect.offsetMax = Vector2.zero;
            Text costText = CreateText(ability.Definition.Id + "CostText", costBadge.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            costText.color = new Color(0.04f, 0.06f, 0.05f, 1f);
            StretchToParent(costText.gameObject);
            abilityCostBadgeTexts[kind] = costText;

            abilityNotificationDots[kind] = CreateNotificationDot(button.transform, 40f, new Vector2(-16f, -16f));
        }
    }

    private void CreateHeroPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text heroTitle = CreateText("HeroGrowthTitle", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        heroTitle.text = "편성";
        AddLayoutElement(heroTitle.gameObject, -1, 38);

        heroFormationContent = new GameObject("HeroFormationContent", typeof(RectTransform));
        heroFormationContent.transform.SetParent(parent, false);
        VerticalLayoutGroup formationLayout = heroFormationContent.AddComponent<VerticalLayoutGroup>();
        formationLayout.spacing = 8;
        formationLayout.childControlWidth = true;
        formationLayout.childControlHeight = true;
        formationLayout.childForceExpandWidth = true;
        formationLayout.childForceExpandHeight = false;
        AddLayoutElement(heroFormationContent, -1, 594);

        heroFormationSummaryText = CreateText("HeroFormationSummary", heroFormationContent.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(heroFormationSummaryText.gameObject, -1, 30);

        GameObject formationArea = CreatePanel("FormationArea", heroFormationContent.transform, new Color(0.33f, 0.42f, 0.58f, 1f));
        AddLayoutElement(formationArea, -1, 188);
        HorizontalLayoutGroup formationAreaLayout = formationArea.AddComponent<HorizontalLayoutGroup>();
        formationAreaLayout.padding = new RectOffset(14, 14, 14, 14);
        formationAreaLayout.spacing = 14;
        formationAreaLayout.childControlWidth = true;
        formationAreaLayout.childControlHeight = true;
        formationAreaLayout.childForceExpandWidth = true;
        formationAreaLayout.childForceExpandHeight = true;

        GameObject slotGrid = new GameObject("FormationSlots", typeof(RectTransform));
        slotGrid.transform.SetParent(formationArea.transform, false);
        GridLayoutGroup slotGridLayout = slotGrid.AddComponent<GridLayoutGroup>();
        slotGridLayout.cellSize = new Vector2(148f, 72f);
        slotGridLayout.spacing = new Vector2(10f, 12f);
        slotGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        slotGridLayout.constraintCount = 4;
        AddLayoutElement(slotGrid, -1, -1);

        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            int slotIndex = i;
            Button slot = CreateButton(string.Empty, slotGrid.transform, 18, new Color(0.18f, 0.22f, 0.31f, 1f));
            slot.onClick.AddListener(() => TryPlaceSelectedHeroInSlot(slotIndex));
            Text slotText = slot.GetComponentInChildren<Text>();
            slotText.name = "FormationSlotText" + i;
            slotText.fontSize = 18;
            slotText.alignment = TextAnchor.MiddleCenter;
            StretchToParent(slotText.gameObject);
            heroFormationSlotTexts.Add(slotText);
            heroFormationSlotButtons[i] = slot;

            Button removeButton = CreateCornerActionButton("-", slot.transform, new Color(0.58f, 0.12f, 0.12f, 1f));
            removeButton.onClick.AddListener(() =>
            {
                RemoveHeroFromEditingFormationSlot(slotIndex);
            });
            heroFormationSlotRemoveButtons[i] = removeButton;
        }

        GameObject totemColumn = new GameObject("FormationTotemColumn", typeof(RectTransform));
        totemColumn.transform.SetParent(formationArea.transform, false);
        VerticalLayoutGroup totemColumnLayout = totemColumn.AddComponent<VerticalLayoutGroup>();
        totemColumnLayout.spacing = 10;
        totemColumnLayout.childControlWidth = true;
        totemColumnLayout.childControlHeight = true;
        totemColumnLayout.childForceExpandWidth = true;
        totemColumnLayout.childForceExpandHeight = true;
        AddLayoutElement(totemColumn, 150, -1);

        Text totemTitle = CreateText("FormationTotemTitle", totemColumn.transform, 21, FontStyle.Bold, TextAnchor.MiddleCenter);
        totemTitle.text = "토템";
        AddLayoutElement(totemTitle.gameObject, -1, 32);

        heroFormationTotemButton = CreateButton(string.Empty, totemColumn.transform, 18, new Color(0.20f, 0.28f, 0.42f, 1f));
        heroFormationTotemButton.onClick.AddListener(() =>
        {
            HandleFormationTotemSlotClick(1);
        });
        heroFormationTotemText = heroFormationTotemButton.GetComponentInChildren<Text>();

        heroFormationTotemSecondButton = CreateButton(string.Empty, totemColumn.transform, 18, new Color(0.18f, 0.20f, 0.26f, 1f));
        heroFormationTotemSecondButton.onClick.AddListener(() =>
        {
            HandleFormationTotemSlotClick(2);
        });
        heroFormationTotemSecondText = heroFormationTotemSecondButton.GetComponentInChildren<Text>();

        GameObject presetColumn = new GameObject("PresetColumn", typeof(RectTransform));
        presetColumn.transform.SetParent(formationArea.transform, false);
        VerticalLayoutGroup presetLayout = presetColumn.AddComponent<VerticalLayoutGroup>();
        presetLayout.spacing = 10;
        presetLayout.childControlWidth = true;
        presetLayout.childControlHeight = true;
        presetLayout.childForceExpandWidth = true;
        presetLayout.childForceExpandHeight = true;
        AddLayoutElement(presetColumn, 132, -1);

        Text presetTitle = CreateText("PresetTitle", presetColumn.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
        presetTitle.text = "프리셋";
        AddLayoutElement(presetTitle.gameObject, -1, 34);
        for (int preset = 1; preset <= GameData.MaxHeroPresets; preset++)
        {
            Button presetButton = CreateButton(preset.ToString(), presetColumn.transform, 24, new Color(0.21f, 0.29f, 0.45f, 1f));
            int capturedPreset = preset;
            presetButton.onClick.AddListener(() =>
            {
                RequestHeroPresetChange(capturedPreset);
            });
            heroPresetButtons[preset] = presetButton;
        }

        GameObject rosterScroll = CreatePanel("HeroRosterScroll", heroFormationContent.transform, new Color(0.15f, 0.19f, 0.28f, 1f));
        AddLayoutElement(rosterScroll, -1, 258);
        ScrollRect rosterScrollRect = rosterScroll.AddComponent<ScrollRect>();
        rosterScrollRect.horizontal = false;
        rosterScrollRect.vertical = true;
        rosterScrollRect.movementType = ScrollRect.MovementType.Elastic;
        rosterScrollRect.inertia = true;
        rosterScrollRect.scrollSensitivity = 36f;

        GameObject rosterViewport = CreatePanel("HeroRosterViewport", rosterScroll.transform, new Color(0f, 0f, 0f, 0f));
        StretchToParent(rosterViewport);
        Image viewportImage = rosterViewport.GetComponent<Image>();
        if (viewportImage != null)
        {
            viewportImage.raycastTarget = true;
        }

        rosterViewport.AddComponent<RectMask2D>();
        rosterScrollRect.viewport = rosterViewport.GetComponent<RectTransform>();

        GameObject rosterGrid = new GameObject("HeroRosterGrid", typeof(RectTransform));
        rosterGrid.transform.SetParent(rosterViewport.transform, false);
        RectTransform rosterGridRect = rosterGrid.GetComponent<RectTransform>();
        heroRosterGridRect = rosterGridRect;
        int rosterColumns = 6;
        float rosterCellWidth = 154f;
        float rosterCellHeight = 124f;
        float rosterSpacingX = 10f;
        float rosterSpacingY = 10f;
        int rosterRows = Mathf.CeilToInt(GameData.Heroes.Count / (float)rosterColumns);
        float rosterWidth = rosterColumns * rosterCellWidth + Mathf.Max(0, rosterColumns - 1) * rosterSpacingX;
        float rosterHeight = Mathf.Max(rosterCellHeight, rosterRows * rosterCellHeight + Mathf.Max(0, rosterRows - 1) * rosterSpacingY);
        rosterGridRect.anchorMin = new Vector2(0f, 1f);
        rosterGridRect.anchorMax = new Vector2(0f, 1f);
        rosterGridRect.pivot = new Vector2(0f, 1f);
        rosterGridRect.sizeDelta = new Vector2(rosterWidth, rosterHeight);
        rosterGridRect.anchoredPosition = new Vector2(8f, -4f);
        rosterScrollRect.content = rosterGridRect;
        rosterScrollRect.verticalNormalizedPosition = 1f;

        GridLayoutGroup rosterLayout = rosterGrid.AddComponent<GridLayoutGroup>();
        rosterLayout.cellSize = new Vector2(rosterCellWidth, rosterCellHeight);
        rosterLayout.spacing = new Vector2(rosterSpacingX, rosterSpacingY);
        rosterLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        rosterLayout.constraintCount = rosterColumns;

        foreach (HeroDefinition hero in GetSortedHeroRosterDefinitions())
        {
            Button button = CreateButton(hero.DisplayName, rosterGrid.transform, 15, GetRarityColor(hero.Rarity));
            string heroId = hero.Id;
            button.onClick.AddListener(() => OpenHeroDetailPanel(heroId));
            heroRosterButtons[hero.Id] = button;
            heroButtonTexts[hero.Id] = button.GetComponentInChildren<Text>();

            GameObject deployedOverlay = CreatePanel(hero.Id + "DeployedOverlay", button.transform, new Color(0f, 0f, 0f, 0.62f));
            StretchToParent(deployedOverlay);
            Image overlayImage = deployedOverlay.GetComponent<Image>();
            if (overlayImage != null)
            {
                overlayImage.raycastTarget = false;
            }

            Text deployedText = CreateText(hero.Id + "DeployedOverlayText", deployedOverlay.transform, 29, FontStyle.Bold, TextAnchor.MiddleCenter);
            deployedText.text = "배치됨";
            deployedText.color = new Color(1f, 0.92f, 0.42f, 1f);
            deployedText.raycastTarget = false;
            StretchToParent(deployedText.gameObject);
            deployedOverlay.SetActive(false);
            heroRosterDeployedOverlays[hero.Id] = deployedOverlay;

            Button actionButton = CreateCornerActionButton("+", button.transform, new Color(0.88f, 0.72f, 0.20f, 1f));
            actionButton.onClick.AddListener(() => SelectOrRemoveRosterHero(heroId));
            heroRosterActionButtons[hero.Id] = actionButton;
            heroNotificationDots[hero.Id] = CreateNotificationDot(button.transform, 40f, new Vector2(-16f, -16f));
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(rosterGridRect);

        heroOwnedEffectText = CreateText("HeroOwnedEffect", heroFormationContent.transform, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
        AddLayoutElement(heroOwnedEffectText.gameObject, -1, 34);

        GameObject actionRow = new GameObject("HeroFormationActions", typeof(RectTransform));
        actionRow.transform.SetParent(heroFormationContent.transform, false);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 14;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;
        AddLayoutElement(actionRow, -1, 58);
        Button autoArrangeButton = CreateButton("자동 배치", actionRow.transform, 24, new Color(0.72f, 0.56f, 0.15f, 1f));
        autoArrangeButton.onClick.AddListener(AutoArrangeEditingFormation);
        Button bulkStarUpButton = CreateButton("일괄 승급", actionRow.transform, 24, new Color(0.34f, 0.35f, 0.37f, 1f));
        bulkStarUpButton.onClick.AddListener(BulkStarUpHeroesFromHud);

        CreateHeroTraitContent(parent);
        CreateHeroTotemContent(parent);
        CreateHeroRuneContent(parent);

        heroPlaceholderText = CreateText("HeroPagePlaceholder", parent, 28, FontStyle.Bold, TextAnchor.MiddleCenter);
        heroPlaceholderText.text = "준비 중";
        AddLayoutElement(heroPlaceholderText.gameObject, -1, 594);
        heroPlaceholderText.gameObject.SetActive(false);

        GameObject heroPageTabSpacer = new GameObject("HeroPageTabSpacer", typeof(RectTransform));
        heroPageTabSpacer.transform.SetParent(parent, false);
        LayoutElement heroPageTabSpacerLayout = AddLayoutElement(heroPageTabSpacer, -1, 1);
        heroPageTabSpacerLayout.flexibleHeight = 1f;

        GameObject heroPageTabs = new GameObject("HeroPageTabs", typeof(RectTransform));
        heroPageTabs.transform.SetParent(parent, false);
        HorizontalLayoutGroup tabLayout = heroPageTabs.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 8;
        tabLayout.childControlWidth = true;
        tabLayout.childControlHeight = true;
        tabLayout.childForceExpandWidth = true;
        tabLayout.childForceExpandHeight = true;
        AddLayoutElement(heroPageTabs, -1, 54);

        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Formation, "편성");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Trait, "특성");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Statue, "토템");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Seal, "룬");
        CreateHeroPageTabButton(heroPageTabs.transform, HeroPageTab.Relic, "성물");
    }

    private void CreateHeroTraitContent(Transform parent)
    {
        heroTraitContent = CreatePanel("HeroTraitContent", parent, new Color(0.25f, 0.33f, 0.48f, 1f));
        VerticalLayoutGroup layout = heroTraitContent.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        AddLayoutElement(heroTraitContent, -1, 594);

        heroTraitSummaryText = CreateText("HeroTraitSummary", heroTraitContent.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(heroTraitSummaryText.gameObject, -1, 42);

        GameObject treePanel = CreatePanel("HeroTraitTree", heroTraitContent.transform, new Color(0.30f, 0.39f, 0.56f, 1f));
        AddLayoutElement(treePanel, -1, 338);
        ScrollRect treeScroll = treePanel.AddComponent<ScrollRect>();
        treeScroll.horizontal = true;
        treeScroll.vertical = false;
        treeScroll.inertia = true;
        treeScroll.scrollSensitivity = 34f;

        GameObject viewport = new GameObject("HeroTraitTreeViewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(treePanel.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(10f, 10f);
        viewportRect.offsetMax = new Vector2(-10f, -10f);

        GameObject content = new GameObject("HeroTraitTreeContent", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(0f, 1f);
        contentRect.pivot = new Vector2(0f, 0.5f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup treeLayout = content.AddComponent<HorizontalLayoutGroup>();
        treeLayout.padding = new RectOffset(4, 4, 0, 0);
        treeLayout.spacing = 0;
        treeLayout.childControlWidth = true;
        treeLayout.childControlHeight = true;
        treeLayout.childForceExpandWidth = false;
        treeLayout.childForceExpandHeight = true;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        treeScroll.viewport = viewportRect;
        treeScroll.content = contentRect;

        for (int depth = 0; depth < TalentData.DepthCount; depth++)
        {
            IReadOnlyList<TalentDefinition> depthTalents = TalentData.GetTalentsInDepth(depth);
            if (depth > 0)
            {
                CreateHeroTraitConnector(content.transform, TalentData.GetTalentsInDepth(depth - 1), depthTalents);
            }

            CreateHeroTraitDepthColumn(content.transform, depth, depthTalents);
        }

        GameObject detailPanel = CreatePanel("HeroTraitDetail", heroTraitContent.transform, new Color(0.20f, 0.27f, 0.40f, 1f));
        AddLayoutElement(detailPanel, -1, 132);
        HorizontalLayoutGroup detailLayout = detailPanel.AddComponent<HorizontalLayoutGroup>();
        detailLayout.padding = new RectOffset(14, 14, 12, 12);
        detailLayout.spacing = 12;
        detailLayout.childControlWidth = true;
        detailLayout.childControlHeight = true;
        detailLayout.childForceExpandWidth = true;
        detailLayout.childForceExpandHeight = true;

        heroTraitDetailText = CreateText("HeroTraitDetailText", detailPanel.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
        heroTraitLevelUpButton = CreateButton("레벨업", detailPanel.transform, 24, new Color(0.54f, 0.78f, 0.22f, 1f));
        AddLayoutElement(heroTraitLevelUpButton.gameObject, 220, -1);
        ConfigureHoldRepeat(heroTraitLevelUpButton, LevelUpSelectedHeroTrait, CanLevelUpSelectedHeroTrait);

        heroTraitContent.SetActive(false);
    }

    private void CreateHeroTotemContent(Transform parent)
    {
        heroTotemContent = CreatePanel("HeroTotemContent", parent, new Color(0.24f, 0.31f, 0.45f, 1f));
        VerticalLayoutGroup layout = heroTotemContent.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        AddLayoutElement(heroTotemContent, -1, 594);

        heroTotemSummaryText = CreateText("HeroTotemSummary", heroTotemContent.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(heroTotemSummaryText.gameObject, -1, 42);

        GameObject circlePanel = CreatePanel("HeroTotemCirclePanel", heroTotemContent.transform, new Color(0.30f, 0.39f, 0.56f, 1f));
        AddLayoutElement(circlePanel, -1, 420);

        GameObject ring = new GameObject("HeroTotemCircleRing", typeof(RectTransform), typeof(Image));
        ring.transform.SetParent(circlePanel.transform, false);
        Image ringImage = ring.GetComponent<Image>();
        ringImage.sprite = GetRingSprite();
        ringImage.color = new Color(0.70f, 0.84f, 1f, 0.28f);
        ringImage.raycastTarget = false;
        RectTransform ringRect = ring.GetComponent<RectTransform>();
        ringRect.anchorMin = new Vector2(0.5f, 0.5f);
        ringRect.anchorMax = new Vector2(0.5f, 0.5f);
        ringRect.pivot = new Vector2(0.5f, 0.5f);
        ringRect.sizeDelta = new Vector2(390f, 390f);
        ringRect.anchoredPosition = Vector2.zero;

        GameObject centerPanel = CreatePanel("HeroTotemCenterEffect", circlePanel.transform, new Color(0.18f, 0.24f, 0.36f, 0.94f));
        RectTransform centerRect = centerPanel.GetComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.5f, 0.5f);
        centerRect.anchorMax = new Vector2(0.5f, 0.5f);
        centerRect.pivot = new Vector2(0.5f, 0.5f);
        centerRect.sizeDelta = new Vector2(360f, 176f);
        centerRect.anchoredPosition = Vector2.zero;

        GameObject centerGlow = new GameObject("HeroTotemCenterGlow", typeof(RectTransform), typeof(Image));
        centerGlow.transform.SetParent(centerPanel.transform, false);
        Image centerGlowImage = centerGlow.GetComponent<Image>();
        centerGlowImage.sprite = GetCircleSprite();
        centerGlowImage.color = new Color(0.55f, 0.78f, 1f, 0.08f);
        centerGlowImage.raycastTarget = false;
        RectTransform centerGlowRect = centerGlow.GetComponent<RectTransform>();
        centerGlowRect.anchorMin = new Vector2(0.5f, 0.5f);
        centerGlowRect.anchorMax = new Vector2(0.5f, 0.5f);
        centerGlowRect.pivot = new Vector2(0.5f, 0.5f);
        centerGlowRect.sizeDelta = new Vector2(330f, 330f);
        centerGlowRect.anchoredPosition = Vector2.zero;

        heroTotemDetailText = CreateText("HeroTotemDetailText", centerPanel.transform, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform detailTextRect = heroTotemDetailText.GetComponent<RectTransform>();
        detailTextRect.anchorMin = Vector2.zero;
        detailTextRect.anchorMax = Vector2.one;
        detailTextRect.offsetMin = new Vector2(18f, 12f);
        detailTextRect.offsetMax = new Vector2(-18f, -12f);
        heroTotemDetailText.resizeTextForBestFit = true;
        heroTotemDetailText.resizeTextMinSize = 15;
        heroTotemDetailText.resizeTextMaxSize = 20;
        heroTotemDetailText.lineSpacing = 0.92f;

        foreach (TotemDefinition totem in GameData.Totems)
        {
            Button button = CreateButton(string.Empty, circlePanel.transform, 18, GetTotemColor(totem));
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(132f, 132f);
            buttonRect.anchoredPosition = GetTotemCirclePosition(totem.Archetype);
            ConfigureTotemNodeButton(button);

            string capturedId = totem.Id;
            button.onClick.AddListener(() => SelectTotem(capturedId));
            heroTotemButtons[totem.Id] = button;
            heroTotemButtonTexts[totem.Id] = button.GetComponentInChildren<Text>();
            Text buttonText = heroTotemButtonTexts[totem.Id];
            if (buttonText != null)
            {
                buttonText.resizeTextForBestFit = true;
                buttonText.resizeTextMinSize = 11;
                buttonText.resizeTextMaxSize = 18;
                buttonText.lineSpacing = 0.86f;
            }
        }

        GameObject actionRow = new GameObject("HeroTotemActions", typeof(RectTransform));
        actionRow.transform.SetParent(heroTotemContent.transform, false);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 14;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;
        AddLayoutElement(actionRow, -1, 68);

        heroTotemEquipButton = CreateButton("장착", actionRow.transform, 26, new Color(0.54f, 0.76f, 0.96f, 1f));
        heroTotemEquipButton.onClick.AddListener(EquipSelectedTotem);

        heroTotemLevelUpButton = CreateButton("강화", actionRow.transform, 26, new Color(0.54f, 0.78f, 0.22f, 1f));
        ConfigureHoldRepeat(heroTotemLevelUpButton, LevelUpSelectedTotem, CanLevelUpSelectedTotem);

        heroTotemContent.SetActive(false);
    }

    private void CreateHeroRuneContent(Transform parent)
    {
        heroRuneContent = CreatePanel("HeroRuneContent", parent, new Color(0.23f, 0.30f, 0.44f, 1f));
        VerticalLayoutGroup layout = heroRuneContent.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        AddLayoutElement(heroRuneContent, -1, 594);

        heroRuneSummaryText = CreateText("HeroRuneSummary", heroRuneContent.transform, 23, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(heroRuneSummaryText.gameObject, -1, 42);

        GameObject slotRow = CreatePanel("HeroRuneSlotRow", heroRuneContent.transform, new Color(0.18f, 0.24f, 0.36f, 1f));
        HorizontalLayoutGroup slotLayout = slotRow.AddComponent<HorizontalLayoutGroup>();
        slotLayout.padding = new RectOffset(10, 10, 10, 10);
        slotLayout.spacing = 10;
        slotLayout.childControlWidth = true;
        slotLayout.childControlHeight = true;
        slotLayout.childForceExpandWidth = true;
        slotLayout.childForceExpandHeight = true;
        AddLayoutElement(slotRow, -1, 102);

        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            int capturedSlot = slot;
            Button slotButton = CreateButton(string.Empty, slotRow.transform, 18, new Color(0.17f, 0.21f, 0.31f, 1f));
            slotButton.onClick.AddListener(() =>
            {
                selectedRuneSlot = capturedSlot;
                string equippedRuneId = battleManager != null ? battleManager.GetEquippedRuneId(selectedHeroPreset, capturedSlot) : string.Empty;
                if (!string.IsNullOrEmpty(equippedRuneId))
                {
                    selectedRuneId = equippedRuneId;
                }

                RefreshHeroRunePanel();
            });
            heroRuneSlotButtons[capturedSlot] = slotButton;
            heroRuneSlotTexts[capturedSlot] = slotButton.GetComponentInChildren<Text>();
        }

        GameObject runeGridPanel = CreatePanel("HeroRuneGridPanel", heroRuneContent.transform, new Color(0.28f, 0.36f, 0.52f, 1f));
        GridLayoutGroup runeGrid = runeGridPanel.AddComponent<GridLayoutGroup>();
        runeGrid.padding = new RectOffset(10, 10, 10, 10);
        runeGrid.spacing = new Vector2(8f, 8f);
        runeGrid.cellSize = new Vector2(128f, 90f);
        runeGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        runeGrid.constraintCount = 5;
        AddLayoutElement(runeGridPanel, -1, 208);

        foreach (RuneDefinition rune in GameData.Runes)
        {
            Button button = CreateButton(string.Empty, runeGridPanel.transform, 15, GetRuneColor(rune));
            string capturedId = rune.Id;
            button.onClick.AddListener(() => SelectRune(capturedId));
            heroRuneButtons[rune.Id] = button;
            heroRuneButtonTexts[rune.Id] = button.GetComponentInChildren<Text>();

            Text buttonText = heroRuneButtonTexts[rune.Id];
            if (buttonText != null)
            {
                buttonText.resizeTextForBestFit = true;
                buttonText.resizeTextMinSize = 10;
                buttonText.resizeTextMaxSize = 15;
                buttonText.lineSpacing = 0.86f;
            }
        }

        GameObject detailPanel = CreatePanel("HeroRuneDetailPanel", heroRuneContent.transform, new Color(0.20f, 0.26f, 0.39f, 1f));
        AddLayoutElement(detailPanel, -1, 116);
        heroRuneDetailText = CreateText("HeroRuneDetailText", detailPanel.transform, 21, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform detailRect = heroRuneDetailText.GetComponent<RectTransform>();
        detailRect.anchorMin = Vector2.zero;
        detailRect.anchorMax = Vector2.one;
        detailRect.offsetMin = new Vector2(18f, 10f);
        detailRect.offsetMax = new Vector2(-18f, -10f);
        heroRuneDetailText.resizeTextForBestFit = true;
        heroRuneDetailText.resizeTextMinSize = 14;
        heroRuneDetailText.resizeTextMaxSize = 21;
        heroRuneDetailText.lineSpacing = 0.92f;

        GameObject actionRow = new GameObject("HeroRuneActions", typeof(RectTransform));
        actionRow.transform.SetParent(heroRuneContent.transform, false);
        HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 14;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;
        AddLayoutElement(actionRow, -1, 68);

        heroRuneEquipButton = CreateButton("장착", actionRow.transform, 25, new Color(0.54f, 0.76f, 0.96f, 1f));
        heroRuneEquipButton.onClick.AddListener(EquipSelectedRune);

        heroRuneLevelUpButton = CreateButton("강화", actionRow.transform, 25, new Color(0.54f, 0.78f, 0.22f, 1f));
        ConfigureHoldRepeat(heroRuneLevelUpButton, LevelUpSelectedRune, CanLevelUpSelectedRune);

        heroRuneContent.SetActive(false);
    }

    private static Vector2 GetTotemCirclePosition(TotemArchetype archetype)
    {
        switch (archetype)
        {
            case TotemArchetype.Combat:
                return new Vector2(-250f, 126f);
            case TotemArchetype.Guardian:
                return new Vector2(250f, 126f);
            case TotemArchetype.Storm:
                return new Vector2(250f, -126f);
            case TotemArchetype.Support:
                return new Vector2(0f, -190f);
            case TotemArchetype.Arcane:
                return new Vector2(-250f, -126f);
            default:
                return Vector2.zero;
        }
    }

    private void ConfigureTotemNodeButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = GetCircleSprite();
            buttonImage.type = Image.Type.Simple;
        }

        GameObject rim = new GameObject("TotemNodeRim", typeof(RectTransform), typeof(Image));
        rim.transform.SetParent(button.transform, false);
        rim.transform.SetAsFirstSibling();
        Image rimImage = rim.GetComponent<Image>();
        rimImage.sprite = GetRingSprite();
        rimImage.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
        rimImage.raycastTarget = false;
        StretchToParent(rim);

        GameObject inner = new GameObject("TotemNodeInnerGlow", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(button.transform, false);
        inner.transform.SetAsFirstSibling();
        Image innerImage = inner.GetComponent<Image>();
        innerImage.sprite = GetCircleSprite();
        innerImage.color = new Color(1f, 1f, 1f, 0.10f);
        innerImage.raycastTarget = false;
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0.14f, 0.14f);
        innerRect.anchorMax = new Vector2(0.86f, 0.86f);
        innerRect.offsetMin = Vector2.zero;
        innerRect.offsetMax = Vector2.zero;
    }

    private void CreateHeroTraitDepthColumn(Transform parent, int depth, IReadOnlyList<TalentDefinition> depthTalents)
    {
        GameObject column = new GameObject("HeroTraitDepth" + depth, typeof(RectTransform));
        column.transform.SetParent(parent, false);
        AddLayoutElement(column, 132, -1);

        Text depthLabel = CreateText("HeroTraitDepthLabel" + depth, column.transform, 14, FontStyle.Bold, TextAnchor.MiddleCenter);
        depthLabel.text = "D" + (depth + 1);
        depthLabel.color = new Color(0.78f, 0.86f, 1f, 1f);
        RectTransform depthLabelRect = depthLabel.GetComponent<RectTransform>();
        depthLabelRect.anchorMin = new Vector2(0f, 0.92f);
        depthLabelRect.anchorMax = new Vector2(1f, 1f);
        depthLabelRect.offsetMin = Vector2.zero;
        depthLabelRect.offsetMax = Vector2.zero;

        for (int i = 0; i < depthTalents.Count; i++)
        {
            TalentDefinition talent = depthTalents[i];
            Button node = CreateButton(string.Empty, column.transform, 15, new Color(0.22f, 0.28f, 0.38f, 1f));
            RectTransform nodeRect = node.GetComponent<RectTransform>();
            float laneY = GetHeroTraitLaneY(depthTalents.Count, i);
            nodeRect.anchorMin = new Vector2(0.5f, laneY);
            nodeRect.anchorMax = new Vector2(0.5f, laneY);
            nodeRect.pivot = new Vector2(0.5f, 0.5f);
            nodeRect.sizeDelta = new Vector2(112f, 78f);
            nodeRect.anchoredPosition = Vector2.zero;

            Text nodeText = node.GetComponentInChildren<Text>();
            if (nodeText != null)
            {
                nodeText.resizeTextForBestFit = true;
                nodeText.resizeTextMinSize = 10;
                nodeText.resizeTextMaxSize = 15;
                nodeText.lineSpacing = 0.88f;
            }

            string talentId = talent.Id;
            node.onClick.AddListener(() =>
            {
                selectedHeroTraitId = talentId;
                UpdateView();
            });
            heroTraitButtons[talent.Id] = node;
            heroTraitButtonTexts[talent.Id] = nodeText;
        }
    }

    private void CreateHeroTraitConnector(
        Transform parent,
        IReadOnlyList<TalentDefinition> previousDepth,
        IReadOnlyList<TalentDefinition> currentDepth)
    {
        GameObject connector = new GameObject("HeroTraitConnector", typeof(RectTransform));
        connector.transform.SetParent(parent, false);
        AddLayoutElement(connector, 44, -1);

        for (int currentIndex = 0; currentIndex < currentDepth.Count; currentIndex++)
        {
            TalentDefinition current = currentDepth[currentIndex];
            for (int prerequisiteIndex = 0; prerequisiteIndex < current.PrerequisiteIds.Count; prerequisiteIndex++)
            {
                int previousIndex = FindHeroTraitDepthIndex(previousDepth, current.PrerequisiteIds[prerequisiteIndex]);
                if (previousIndex < 0)
                {
                    continue;
                }

                CreateHeroTraitConnectorLine(
                    connector.transform,
                    GetHeroTraitLaneY(previousDepth.Count, previousIndex),
                    GetHeroTraitLaneY(currentDepth.Count, currentIndex));
            }
        }
    }

    private void CreateHeroTraitConnectorLine(Transform parent, float fromLaneY, float toLaneY)
    {
        const float width = 44f;
        const float height = 252f;
        Vector2 start = new Vector2(width * -0.5f, (fromLaneY - 0.5f) * height);
        Vector2 end = new Vector2(width * 0.5f, (toLaneY - 0.5f) * height);
        Vector2 delta = end - start;

        GameObject line = new GameObject("HeroTraitConnectorLine", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(parent, false);
        Image image = line.GetComponent<Image>();
        image.color = new Color(0.16f, 0.22f, 0.35f, 0.95f);
        image.raycastTarget = false;

        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(delta.magnitude, 5f);
        rect.anchoredPosition = (start + end) * 0.5f;
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private static int FindHeroTraitDepthIndex(IReadOnlyList<TalentDefinition> depthTalents, string talentId)
    {
        for (int i = 0; i < depthTalents.Count; i++)
        {
            if (depthTalents[i].Id == talentId)
            {
                return i;
            }
        }

        return -1;
    }

    private static float GetHeroTraitLaneY(int nodeCount, int nodeIndex)
    {
        if (nodeCount <= 1)
        {
            return 0.50f;
        }

        if (nodeCount == 2)
        {
            return nodeIndex == 0 ? 0.64f : 0.36f;
        }

        return 0.78f - Mathf.Clamp(nodeIndex, 0, 2) * 0.28f;
    }

    private List<HeroDefinition> GetSortedHeroRosterDefinitions()
    {
        var heroes = new List<HeroDefinition>(GameData.Heroes);
        heroes.Sort(CompareHeroRosterDefinitions);
        return heroes;
    }

    private static int CompareHeroRosterDefinitions(HeroDefinition left, HeroDefinition right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int rarityCompare = ((int)left.Rarity).CompareTo((int)right.Rarity);
        if (rarityCompare != 0)
        {
            return rarityCompare;
        }

        int nameCompare = KoreanNameComparer.Compare(left.DisplayName, right.DisplayName);
        if (nameCompare != 0)
        {
            return nameCompare;
        }

        return string.CompareOrdinal(left.Id, right.Id);
    }

    private void CreateStagePanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button resumeButton = CreateButton("자동 진행 재개", parent, 30, new Color(0.12f, 0.34f, 0.30f, 1f));
        AddLayoutElement(resumeButton.gameObject, -1, 86);
        resumeButton.onClick.AddListener(() => progressManager.ResumeAutoProgress());

        GameObject gridObject = new GameObject("StageGrid", typeof(RectTransform));
        gridObject.transform.SetParent(parent, false);
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(228, 72);
        grid.spacing = new Vector2(14, 14);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        AddLayoutElement(gridObject, -1, 430);

        foreach (StageDefinition stage in GameData.Stages)
        {
            Color buttonColor = stage.Type == StageType.Boss
                ? new Color(0.42f, 0.18f, 0.16f, 1f)
                : new Color(0.20f, 0.24f, 0.31f, 1f);
            Button button = CreateButton(stage.Id, gridObject.transform, 26, buttonColor);
            string stageId = stage.Id;
            button.onClick.AddListener(() => progressManager.SelectStage(stageId));
            stageButtons[stage.Id] = button;
        }
    }

    private void CreateSummonPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("SummonTitle", parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "소환";
        AddLayoutElement(title.gameObject, -1, 58);

        Text heroTitle = CreateText("HeroSummonTitle", parent, 27, FontStyle.Bold, TextAnchor.MiddleLeft);
        heroTitle.text = "영웅 뽑기";
        AddLayoutElement(heroTitle.gameObject, -1, 36);

        GameObject heroButtonRow = CreateSummonButtonRow(parent, "HeroSummonButtons");
        Button heroRollOne = CreateButton("영웅 1회", heroButtonRow.transform, 28, new Color(0.36f, 0.24f, 0.45f, 1f));
        Button heroRollTen = CreateButton("영웅 10회", heroButtonRow.transform, 28, new Color(0.36f, 0.24f, 0.45f, 1f));
        heroRollOne.onClick.AddListener(() => gachaManager.RollHeroes(1));
        heroRollTen.onClick.AddListener(() => gachaManager.RollHeroes(10));

        Text equipmentTitle = CreateText("EquipmentSummonTitle", parent, 27, FontStyle.Bold, TextAnchor.MiddleLeft);
        equipmentTitle.text = "장비 뽑기";
        AddLayoutElement(equipmentTitle.gameObject, -1, 36);

        GameObject equipmentButtonRow = CreateSummonButtonRow(parent, "EquipmentSummonButtons");
        Button equipmentRollOne = CreateButton("장비 1회", equipmentButtonRow.transform, 28, new Color(0.24f, 0.32f, 0.44f, 1f));
        Button equipmentRollTen = CreateButton("장비 10회", equipmentButtonRow.transform, 28, new Color(0.24f, 0.32f, 0.44f, 1f));
        equipmentRollOne.onClick.AddListener(() => gachaManager.RollEquipment(1));
        equipmentRollTen.onClick.AddListener(() => gachaManager.RollEquipment(10));

        Text rule = CreateText("SummonRule", parent, 25, FontStyle.Normal, TextAnchor.UpperLeft);
        rule.text = "영웅: 뽑기권 우선, 부족분 루비 150개"
            + "\n장비: 장비 뽑기권 우선, 부족분 루비 100개"
            + "\n확률: " + GachaManager.GetRateSummaryText()
            + "\n영웅 조각: 1회당 선택 영웅 조각 1개";
        AddLayoutElement(rule.gameObject, -1, 120);

        gachaText = CreateText("GachaResult", parent, 26, FontStyle.Normal, TextAnchor.UpperLeft);
        AddLayoutElement(gachaText.gameObject, -1, 256);
    }

    private GameObject CreateSummonButtonRow(Transform parent, string name)
    {
        GameObject buttonRow = new GameObject(name, typeof(RectTransform));
        buttonRow.transform.SetParent(parent, false);
        HorizontalLayoutGroup row = buttonRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 16;
        row.childControlWidth = true;
        row.childForceExpandWidth = true;
        AddLayoutElement(buttonRow, -1, 78);
        return buttonRow;
    }

    private void CreateShopPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("ShopTitle", parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "상점";
        AddLayoutElement(title.gameObject, -1, 58);

        Button speedProduct = CreateButton("4x", parent, 30, new Color(0.42f, 0.28f, 0.14f, 1f));
        AddLayoutElement(speedProduct.gameObject, -1, 96);
        speedProduct.onClick.AddListener(() => speedManager.TrySelectSpeed(GameSpeedManager.PremiumSpeed));

        Button rubyProduct = CreateButton("Ruby", parent, 30, new Color(0.36f, 0.18f, 0.42f, 1f));
        AddLayoutElement(rubyProduct.gameObject, -1, 96);

        Button ticketProduct = CreateButton("Ticket", parent, 30, new Color(0.24f, 0.30f, 0.42f, 1f));
        AddLayoutElement(ticketProduct.gameObject, -1, 96);
    }

    private void CreateSupportPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 14;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("SupportTitle", parent, 34, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "지원 - 자동 스킬과 펫";
        AddLayoutElement(title.gameObject, -1, 54);

        supportSummaryText = CreateText("PartySupportInfo", parent, 26, FontStyle.Bold, TextAnchor.MiddleLeft);
        AddLayoutElement(supportSummaryText.gameObject, -1, 46);

        Text skillTitle = CreateText("SkillSupportTitle", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        skillTitle.text = "자동 스킬";
        AddLayoutElement(skillTitle.gameObject, -1, 42);

        foreach (CombatSkillState skill in battleManager.Skills)
        {
            GameObject row = CreatePanel(skill.Definition.Id + "SupportRow", parent, new Color(0.19f, 0.24f, 0.28f, 1f));
            AddLayoutElement(row, -1, 96);
            Text text = CreateText(skill.Definition.Id + "SupportText", row.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18, 8);
            textRect.offsetMax = new Vector2(-18, -8);
            skillStatusTexts[skill.Definition.Id] = text;
        }

        Text petTitle = CreateText("PetSupportTitle", parent, 28, FontStyle.Bold, TextAnchor.MiddleLeft);
        petTitle.text = "펫";
        AddLayoutElement(petTitle.gameObject, -1, 42);

        foreach (PetState pet in battleManager.Pets)
        {
            GameObject row = CreatePanel(pet.Definition.Id + "SupportRow", parent, new Color(0.18f, 0.26f, 0.22f, 1f));
            AddLayoutElement(row, -1, 106);
            Text text = CreateText(pet.Definition.Id + "SupportText", row.transform, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18, 8);
            textRect.offsetMax = new Vector2(-18, -8);
            petStatusTexts[pet.Definition.Id] = text;
        }
    }

    private void CreateDebugPanel(Transform parent)
    {
        VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Text title = CreateText("DebugTitle", parent, 36, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = "QA 디버그";
        AddLayoutElement(title.gameObject, -1, 58);

        GameObject gridObject = new GameObject("DebugGrid", typeof(RectTransform));
        gridObject.transform.SetParent(parent, false);
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 82);
        grid.spacing = new Vector2(16, 16);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        AddLayoutElement(gridObject, -1, 620);

        CreateDebugButton("Gold +5000", gridObject.transform, () => wallet.AddGold(5000));
        CreateDebugButton("EXP +5000", gridObject.transform, () => wallet.AddHeroExpItem(5000));
        CreateDebugButton("Equip EXP +5000", gridObject.transform, () => wallet.AddEquipmentExpItem(5000));
        CreateDebugButton("Totem Essence +5000", gridObject.transform, () => wallet.AddTotemEssence(5000));
        CreateDebugButton("Rune Dust +5000", gridObject.transform, () => wallet.AddRuneDust(5000));
        CreateDebugButton("Ruby +1500", gridObject.transform, () => wallet.AddRuby(1500));
        CreateDebugButton("Transcend +100", gridObject.transform, () => wallet.AddHeroTranscendStone(100));
        CreateDebugButton("Hero Ticket +10", gridObject.transform, () => wallet.AddHeroSummonTicket(10));
        CreateDebugButton("Equip Ticket +10", gridObject.transform, () => wallet.AddEquipmentSummonTicket(10));
        CreateDebugButton("Hero Lv +5", gridObject.transform, () => battleManager.DebugLevelAllHeroes(5));
        CreateDebugButton("계정 EXP +50K", gridObject.transform, () => accountProgressManager.AddExperience(GameNumber.FromDouble(50000)));
        CreateDebugButton("계정 Lv +100", gridObject.transform, () => accountProgressManager.DebugAddLevels(100));
        CreateDebugButton("특성P +1000", gridObject.transform, () => accountProgressManager.DebugAddTalentPoints(1000));
        CreateDebugButton("Unlock Totems", gridObject.transform, () => battleManager.DebugUnlockAllTotems());
        CreateDebugButton("Unlock Runes", gridObject.transform, () => battleManager.DebugUnlockAllRunes());
        CreateDebugButton("Unlock 4x", gridObject.transform, () => speedManager.DebugSetFourTimesEntitlement(true));
        CreateDebugButton("Unlock All", gridObject.transform, () => progressManager.DebugUnlockThrough(GameData.ChapterOneBossStageId));
        CreateDebugButton("1-19 Repeat", gridObject.transform, () => progressManager.DebugJumpToStage(GameData.BossFallbackStageId, ProgressMode.RepeatSelected));
        CreateDebugButton("1-20 Boss", gridObject.transform, () => progressManager.DebugJumpToStage(GameData.ChapterOneBossStageId, ProgressMode.AutoProgress));
        CreateDebugButton("Reset Save", gridObject.transform, () => resetSaveAction?.Invoke(), new Color(0.45f, 0.16f, 0.14f, 1f), false);

        GameObject speedRow = new GameObject("SpeedButtons", typeof(RectTransform));
        speedRow.transform.SetParent(parent, false);
        HorizontalLayoutGroup row = speedRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 16;
        row.childControlWidth = true;
        row.childForceExpandWidth = true;
        AddLayoutElement(speedRow, -1, 90);

        CreateDebugButton("Time x1", speedRow.transform, () => SetTimeScale(1f));
        CreateDebugButton("Time x5", speedRow.transform, () => SetTimeScale(5f));
        CreateDebugButton("Time x20", speedRow.transform, () => SetTimeScale(20f));

        debugText = CreateText("DebugStatus", parent, 26, FontStyle.Normal, TextAnchor.UpperLeft);
        AddLayoutElement(debugText.gameObject, -1, 260);
    }

    private void CreateDebugButton(string label, Transform parent, Action action)
    {
        CreateDebugButton(label, parent, action, new Color(0.23f, 0.25f, 0.28f, 1f));
    }

    private void CreateDebugButton(string label, Transform parent, Action action, Color color, bool refreshAfter = true)
    {
        Button button = CreateButton(label, parent, 25, color);
        button.onClick.AddListener(() =>
        {
            action?.Invoke();
            if (refreshAfter)
            {
                UpdateView();
            }
        });
    }

    private void DebugGrantTestCurrency()
    {
        wallet.AddGold(100000);
        wallet.AddRuby(5000);
        wallet.AddHeroExpItem(20000);
        wallet.AddEquipmentExpItem(20000);
        wallet.AddTotemEssence(10000);
        wallet.AddRuneDust(10000);
        wallet.AddHeroTranscendStone(300);
        wallet.AddHeroSummonTicket(50);
        wallet.AddEquipmentSummonTicket(50);
        accountProgressManager.DebugAddLevels(25);
        accountProgressManager.DebugAddTalentPoints(200);
        UpdateView();
    }

    private void CreateBottomNav(Transform parent)
    {
        GameObject panel = CreatePanel("BottomNav", parent, new Color(0.08f, 0.11f, 0.17f, 1f));
        AddLayoutElement(panel, -1, 130);

        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateTabButton(panel.transform, HudTab.Growth, "⚔\n성장");
        CreateTabButton(panel.transform, HudTab.Hero, "★\n영웅");
        CreateTabButton(panel.transform, HudTab.Summon, "◇\n소환");
        CreateTabButton(panel.transform, HudTab.Stage, "▣\n던전 배틀");
        CreateTabButton(panel.transform, HudTab.Shop, "▤\n상점");
    }

    private void CreateHeroPageTabButton(Transform parent, HeroPageTab tab, string label)
    {
        Button button = CreateButton(label, parent, 22, new Color(0.18f, 0.24f, 0.38f, 1f));
        button.onClick.AddListener(() =>
        {
            activeHeroPageTab = tab;
            UpdateView();
        });
        heroPageTabButtons[tab] = button;
    }

    private Button CreateCornerActionButton(string label, Transform parent, Color color)
    {
        Button button = CreateButton(label, parent, 20, color);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(34f, 34f);
        rect.anchoredPosition = new Vector2(-4f, -4f);
        return button;
    }

    private void CreateTabButton(Transform parent, HudTab tab, string label)
    {
        Button button = CreateButton(label, parent, 24, new Color(0.13f, 0.17f, 0.25f, 1f));
        RegisterTabNotificationDot(tab, CreateNotificationDot(button.transform, 38f, new Vector2(-14f, -14f)));
        button.onClick.AddListener(() =>
        {
            RequestTabChange(tab);
        });
        tabButtons[tab] = button;
        tabButtonLabels[tab] = label;
    }

    private void RequestTabChange(HudTab tab)
    {
        if (heroDetailPanelOpen)
        {
            heroDetailPanelOpen = false;
            selectedHeroDetailId = string.Empty;
            if (tab == HudTab.Hero)
            {
                UpdateView();
                return;
            }
        }

        HudTab targetTab;
        bool targetContentOpen;
        if ((tab == HudTab.Growth || tab == HudTab.Hero) && activeTab == tab && contentPanelOpen)
        {
            targetTab = activeTab;
            targetContentOpen = false;
        }
        else
        {
            targetTab = tab;
            targetContentOpen = true;
        }

        if (ShouldPromptHeroFormationSave(targetTab, targetContentOpen))
        {
            ShowHeroFormationSavePromptForTab(targetTab, targetContentOpen);
            return;
        }

        ApplyTabState(targetTab, targetContentOpen);
    }

    private bool ShouldPromptHeroFormationSave(HudTab targetTab, bool targetContentOpen)
    {
        return !heroFormationSavePromptOpen
            && contentPanelOpen
            && activeTab == HudTab.Hero
            && HasHeroFormationPendingChanges()
            && (targetTab != HudTab.Hero || !targetContentOpen);
    }

    private void ShowHeroFormationSavePromptForTab(HudTab targetTab, bool targetContentOpen)
    {
        pendingHeroPresetSwitch = false;
        pendingHeroPreset = 0;
        pendingTabAfterHeroFormationPrompt = targetTab;
        pendingContentOpenAfterHeroFormationPrompt = targetContentOpen;
        heroFormationSavePromptOpen = true;
        UpdateView();
    }

    private void ShowHeroFormationSavePromptForPreset(int preset)
    {
        pendingHeroPresetSwitch = true;
        pendingHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        heroFormationSavePromptOpen = true;
        UpdateView();
    }

    private void ApplyTabState(HudTab targetTab, bool targetContentOpen)
    {
        bool openingHeroPanel = targetTab == HudTab.Hero && (!contentPanelOpen || activeTab != HudTab.Hero);
        activeTab = targetTab;
        contentPanelOpen = targetContentOpen;
        if (openingHeroPanel)
        {
            LoadHeroFormationDraftFromPreset(battleManager.ActiveHeroPreset);
        }

        UpdateView();
    }

    private void ConfirmHeroFormationSavePrompt()
    {
        if (!battleManager.ApplyHeroFormation(selectedHeroPreset, editingFormationHeroIds))
        {
            heroFormationSavePromptOpen = false;
            UpdateView();
            return;
        }

        heroFormationDirty = false;
        selectedHeroForPlacement = string.Empty;
        heroFormationSavePromptOpen = false;

        if (pendingHeroPresetSwitch)
        {
            int preset = pendingHeroPreset;
            ClearHeroFormationPromptTarget();
            LoadHeroFormationDraftFromPreset(preset);
            UpdateView();
            return;
        }

        HudTab targetTab = pendingTabAfterHeroFormationPrompt;
        bool targetContentOpen = pendingContentOpenAfterHeroFormationPrompt;
        ClearHeroFormationPromptTarget();
        ApplyTabState(targetTab, targetContentOpen);
    }

    private void CancelHeroFormationSavePrompt()
    {
        heroFormationSavePromptOpen = false;
        ClearHeroFormationPromptTarget();
        UpdateView();
    }

    private void ClearHeroFormationPromptTarget()
    {
        pendingHeroPresetSwitch = false;
        pendingHeroPreset = 0;
        pendingTabAfterHeroFormationPrompt = activeTab;
        pendingContentOpenAfterHeroFormationPrompt = contentPanelOpen;
    }

    private void UpdateView()
    {
        if (resourceText == null)
        {
            return;
        }

        StageDefinition stage = progressManager.CurrentStage;
        resourceText.text = FormatShortNumber(wallet.Gold);
        if (rubyResourceText != null)
        {
            rubyResourceText.text = FormatCountNumber(wallet.Ruby);
        }

        stageText.text = "프로필 캐릭터";
        modeText.text = FormatShortNumber(battleManager.TotalCombatPower);

        if (accountProgressManager != null)
        {
            float accountExpRatio = Mathf.Clamp01((float)accountProgressManager.Experience.RatioTo(accountProgressManager.NextLevelExperience));
            if (accountExpFill != null)
            {
                accountExpFill.rectTransform.anchorMax = new Vector2(accountExpRatio, 1f);
            }

            if (accountLevelText != null)
            {
                accountLevelText.text = "계정 Lv." + accountProgressManager.Level
                    + "  " + FormatShortNumber(accountProgressManager.Experience)
                    + "/" + FormatShortNumber(accountProgressManager.NextLevelExperience);
            }
        }
        if (fieldStagePillText != null)
        {
            fieldStagePillText.text = battleManager.IsBossFight ? stage.Id + " BOSS" : stage.Id;
        }

        if (targetText != null)
        {
            targetText.gameObject.SetActive(false);
        }

        float progressRatio = battleManager.IsBossFight
            ? (battleManager.TargetMaxHp <= GameNumber.Zero ? 0f : Mathf.Clamp01(1f - (float)battleManager.TargetHp.RatioTo(battleManager.TargetMaxHp)))
            : (battleManager.RequiredKills <= 0 ? 0f : Mathf.Clamp01((float)battleManager.KillsThisStage / battleManager.RequiredKills));
        hpFill.rectTransform.anchorMax = new Vector2(progressRatio, 1f);
        hpFill.color = battleManager.IsBossFight
            ? new Color(0.90f, 0.18f, 0.16f, 1f)
            : new Color(0.95f, 0.63f, 0.17f, 1f);

        if (battleManager.IsBossFight)
        {
            hpText.text = "BOSS " + Mathf.RoundToInt(progressRatio * 100f) + "%";
            progressText.text = "Boss Timer: " + Mathf.CeilToInt(battleManager.BossTimeRemaining) + "s";
        }
        else
        {
            hpText.text = battleManager.KillsThisStage + " / " + battleManager.RequiredKills;
            progressText.text = "100마리 처치";
        }

        if (guideQuestText != null)
        {
            string questGoal = battleManager.IsBossFight
                ? "보스 처치"
                : "스테이지 " + stage.Id + " 클리어";
            string questProgress = battleManager.IsBossFight
                ? Mathf.CeilToInt(battleManager.BossTimeRemaining) + "초 남음"
                : battleManager.KillsThisStage + "/" + battleManager.RequiredKills;
            guideQuestText.text = "가이드 퀘스트\n" + questGoal + "  " + questProgress;
        }

        RefreshDamageMeter();

        supportText.text = battleManager.SupportStatusText;

        logText.text = battleManager.LastBattleLog;
        if (!string.IsNullOrEmpty(battleManager.LastDamageLog))
        {
            logText.text += "\n" + battleManager.LastDamageLog;
        }

        rewardText.text = battleManager.LastRewardLog;
        RefreshBattlefieldVisuals();

        gachaText.text = gachaManager.LastResult
            + "\n\n보유 장비"
            + "\n" + equipmentInventory.GetOwnedSummary(6);

        if (totalCombatPowerText != null)
        {
            totalCombatPowerText.text = "종합 전투력  " + FormatShortNumber(battleManager.TotalCombatPower);
        }

        if (growthNoticeText != null)
        {
            growthNoticeText.text = Time.unscaledTime < growthNoticeUntil ? growthNoticeMessage : string.Empty;
        }

        foreach (KeyValuePair<int, Button> pair in growthStepButtons)
        {
            bool selected = pair.Key == selectedGrowthLevelStep;
            SetButtonColor(pair.Value, selected ? new Color(0.32f, 0.29f, 0.18f, 1f) : new Color(0.18f, 0.24f, 0.38f, 1f));

            Text text = pair.Value.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = selected ? "[" + pair.Key + "x]" : pair.Key + "x";
                text.color = selected ? new Color(1f, 0.91f, 0.40f, 1f) : Color.white;
            }
        }

        bool hasGrowthAttention = false;
        foreach (AbilityState ability in abilityManager.States)
        {
            if (abilityButtonTexts.TryGetValue(ability.Definition.Kind, out Text text))
            {
                int cappedLevels = abilityManager.GetCappedLevelCount(ability, selectedGrowthLevelStep);
                long selectedCost = abilityManager.GetLevelUpCost(ability, cappedLevels);
                bool canBuySelected = !ability.IsMaxed && cappedLevels > 0 && selectedCost > 0 && selectedCost <= wallet.Gold;
                bool canBuyOne = !ability.IsMaxed && ability.LevelUpCost > 0 && ability.LevelUpCost <= wallet.Gold;
                hasGrowthAttention |= canBuyOne;
                SetNotificationDot(abilityNotificationDots, ability.Definition.Kind, canBuySelected);

                string costText = ability.IsMaxed ? "MAX" : "G " + FormatShortNumber(selectedCost);
                string levelText = ability.IsMaxed ? "MAX" : "Lv." + ability.Level + "/" + ability.Definition.MaxLevel;
                text.text = ability.Definition.DisplayName
                    + "  " + levelText
                    + "\n" + abilityManager.GetDisplayValue(ability);

                Button rowButton = text.GetComponentInParent<Button>();
                if (rowButton != null)
                {
                    SetButtonColor(rowButton, ability.IsMaxed
                        ? new Color(0.26f, 0.30f, 0.39f, 1f)
                        : canBuySelected ? new Color(0.48f, 0.54f, 0.66f, 1f) : new Color(0.24f, 0.26f, 0.30f, 1f));
                }

                if (abilityCostBadgeTexts.TryGetValue(ability.Definition.Kind, out Text costBadgeText))
                {
                    costBadgeText.text = costText;
                    costBadgeText.color = ability.IsMaxed ? new Color(1f, 0.88f, 0.24f, 1f) : new Color(0.04f, 0.06f, 0.05f, 1f);
                    Image badgeImage = costBadgeText.GetComponentInParent<Image>();
                    if (badgeImage != null)
                    {
                        badgeImage.color = ability.IsMaxed
                            ? new Color(0.14f, 0.19f, 0.31f, 1f)
                            : canBuySelected ? new Color(0.56f, 0.88f, 0.24f, 1f) : new Color(0.20f, 0.24f, 0.33f, 1f);
                    }
                }
            }
        }

        RefreshHeroFormationPanel();

        bool hasHeroAttention = false;
        foreach (HeroState hero in battleManager.Heroes)
        {
            if (heroButtonTexts.TryGetValue(hero.Definition.Id, out Text text))
            {
                bool canLevel = hero.LevelUpCost <= wallet.HeroExpItem;
                bool isOwned = hero.IsOwned;
                if (heroRosterButtons.TryGetValue(hero.Definition.Id, out Button rosterButton) && rosterButton != null)
                {
                    rosterButton.gameObject.SetActive(isOwned);
                }

                bool needsAttention = isOwned && (hero.CanStarUp || canLevel);
                hasHeroAttention |= needsAttention;
                SetNotificationDot(heroNotificationDots, hero.Definition.Id, needsAttention);

                string starCostText = hero.IsMaxStars
                    ? "S MAX"
                    : "S " + hero.Shards + "/" + hero.StarUpCost;
                bool isDeployed = IsHeroInEditingFormation(hero.Definition.Id);
                bool isSelectedForPlacement = selectedHeroForPlacement == hero.Definition.Id;
                string actionText = isSelectedForPlacement ? "선택됨" : "대기";

                text.text = GetTraitBadge(hero.Definition.Trait)
                    + " " + GetRarityBadge(hero.Definition.Rarity)
                    + (isOwned ? " Lv." + hero.Level : " 미보유")
                    + "\n" + GetShortHeroLabel(hero.Definition)
                    + "  " + hero.Definition.RarityLabel + "  " + FormatStars(hero.Stars)
                    + "\n공 " + FormatShortNumber(hero.AttackPower) + "  체 " + FormatShortNumber(hero.MaxHp)
                    + "\n속 " + hero.AttackSpeed.ToString("0.##") + "  이 " + hero.MoveSpeed.ToString("0.#")
                    + "\n" + hero.Definition.PassiveLabel + "  " + starCostText + (isDeployed || !isOwned ? string.Empty : "  " + actionText);

                Button heroButton = text.GetComponentInParent<Button>();
                if (heroButton != null)
                {
                    SetButtonColor(heroButton, isDeployed
                        ? new Color(0.13f, 0.15f, 0.18f, 1f)
                        : isSelectedForPlacement ? new Color(0.55f, 0.49f, 0.20f, 1f) : GetRarityColor(hero.Definition.Rarity));
                }

                if (heroRosterDeployedOverlays.TryGetValue(hero.Definition.Id, out GameObject deployedOverlay))
                {
                    deployedOverlay.SetActive(isDeployed);
                }

                if (heroRosterActionButtons.TryGetValue(hero.Definition.Id, out Button actionButton))
                {
                    actionButton.interactable = isOwned;
                    Text actionTextComponent = actionButton.GetComponentInChildren<Text>(true);
                    if (actionTextComponent != null)
                    {
                        actionTextComponent.text = !isOwned ? "잠" : isDeployed ? "-" : "+";
                    }

                    SetButtonColor(actionButton, !isOwned
                        ? new Color(0.24f, 0.25f, 0.28f, 1f)
                        : isDeployed
                        ? new Color(0.58f, 0.12f, 0.12f, 1f)
                        : new Color(0.88f, 0.72f, 0.20f, 1f));
                }
            }
        }

        if (heroRosterGridRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(heroRosterGridRect);
        }

        if (supportSummaryText != null)
        {
            supportSummaryText.text = "Party ATK " + FormatShortNumber(battleManager.PartyAttackPower)
                + "    Pet Gold +" + battleManager.PetGoldBonusPercent.ToString("0.#") + "%";
        }

        foreach (CombatSkillState skill in battleManager.Skills)
        {
            if (skillStatusTexts.TryGetValue(skill.Definition.Id, out Text text))
            {
                double projectedDamage = battleManager.PartyAttackPower * skill.Definition.PartyAttackMultiplier;
                text.text = skill.Definition.DisplayName
                    + "    Cooldown " + Mathf.CeilToInt(skill.CooldownRemaining) + "s"
                    + "\nDamage " + FormatShortNumber(projectedDamage)
                    + "    Party ATK x" + skill.Definition.PartyAttackMultiplier.ToString("0.0");
            }
        }

        foreach (PetState pet in battleManager.Pets)
        {
            if (petStatusTexts.TryGetValue(pet.Definition.Id, out Text text))
            {
                text.text = pet.Definition.DisplayName
                    + "    Next " + Mathf.CeilToInt(pet.AttackCooldown) + "s"
                    + "\nATK " + FormatShortNumber(pet.Definition.AttackPower)
                    + "    Interval " + pet.Definition.AttackInterval.ToString("0.0") + "s"
                    + "    Gold +" + battleManager.PetGoldBonusPercent.ToString("0.#") + "%";
            }
        }

        foreach (KeyValuePair<string, Button> pair in stageButtons)
        {
            bool unlocked = GameData.IsStageUnlocked(pair.Key, progressManager.HighestStageId);
            pair.Value.interactable = unlocked;
            Text text = pair.Value.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = pair.Key == progressManager.CurrentStageId ? "[" + pair.Key + "]" : pair.Key;
            }
        }

        if (skillAutoButton != null)
        {
            RefreshAutoControlButton(
                skillAutoButton,
                "스킬",
                battleManager.SkillAutoEnabled,
                new Color(0.88f, 0.66f, 0.16f, 1f),
                new Color(0.28f, 0.29f, 0.32f, 1f));
        }

        if (feverAutoButton != null)
        {
            RefreshAutoControlButton(
                feverAutoButton,
                "피버",
                battleManager.FeverAutoEnabled,
                new Color(0.88f, 0.62f, 0.18f, 1f),
                new Color(0.28f, 0.29f, 0.32f, 1f));
        }

        if (speedCycleButton != null)
        {
            int currentSpeed = speedManager.CurrentMultiplier;
            speedCycleButton.interactable = true;
            SetButtonColor(speedCycleButton, currentSpeed == GameSpeedManager.PremiumSpeed
                ? new Color(0.60f, 0.40f, 0.16f, 1f)
                : currentSpeed == GameSpeedManager.FreeSpeed
                    ? new Color(0.34f, 0.44f, 0.20f, 1f)
                    : new Color(0.18f, 0.24f, 0.32f, 1f));

            Text text = speedCycleButton.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = "가속\n" + currentSpeed + "x";
            }
        }

        if (debugText != null)
        {
            string accountDebugText = accountProgressManager != null
                ? "\nAccount Lv: " + accountProgressManager.Level
                    + " EXP " + FormatShortNumber(accountProgressManager.Experience)
                    + "/" + FormatShortNumber(accountProgressManager.NextLevelExperience)
                    + " TP " + accountProgressManager.AvailableTalentPoints
                    + "/" + accountProgressManager.TotalTalentPointsEarned
                    + " Bonus " + accountProgressManager.DebugTalentPointBonus
                : string.Empty;
            debugText.text = "Time Scale x" + Time.timeScale.ToString("0.##")
                + "\nCombat Speed x" + speedManager.CurrentMultiplier
                + "\n4x Entitlement: " + speedManager.HasFourTimesSpeedEntitlement
                + "\nTotem Essence: " + FormatCountNumber(wallet.TotemEssence)
                + "\nRune Dust: " + FormatCountNumber(wallet.RuneDust)
                + accountDebugText
                + "\nOffline Reward Stage: " + progressManager.GetOfflineRewardStageId()
                + "\nBoss Cleared: " + progressManager.ChapterOneBossCleared
                + "\nLast Battle: " + battleManager.LastBattleLog;
        }

        bool hasSummonAttention = wallet.HeroSummonTicket > 0
            || wallet.EquipmentSummonTicket > 0
            || wallet.Ruby >= 100;
        bool hasStageAttention = progressManager.Mode == ProgressMode.BossBlocked;
        if (guideQuestDot != null)
        {
            guideQuestDot.SetActive(hasStageAttention);
        }

        bool hasSupportAttention = false;
        foreach (CombatSkillState skill in battleManager.Skills)
        {
            if (skill.CooldownRemaining <= 0.5f)
            {
                hasSupportAttention = true;
                break;
            }
        }

        SetTabNotificationDots(HudTab.Growth, hasGrowthAttention);
        SetTabNotificationDots(HudTab.Hero, hasHeroAttention);
        SetTabNotificationDots(HudTab.Summon, hasSummonAttention);
        SetTabNotificationDots(HudTab.Stage, hasStageAttention);
        SetTabNotificationDots(HudTab.Shop, false);
        SetTabNotificationDots(HudTab.Support, hasSupportAttention);
        SetTabNotificationDots(HudTab.Debug, false);

        if (battleLayoutElement != null)
        {
            battleLayoutElement.preferredHeight = contentPanelOpen ? 870f : 1630f;
        }

        if (contentLayoutElement != null)
        {
            contentLayoutElement.preferredHeight = contentPanelOpen ? 760f : 0f;
        }

        if (contentRoot != null)
        {
            contentRoot.SetActive(contentPanelOpen);
        }

        growthPanel.SetActive(contentPanelOpen && activeTab == HudTab.Growth);
        heroPanel.SetActive(contentPanelOpen && activeTab == HudTab.Hero);
        stagePanel.SetActive(contentPanelOpen && activeTab == HudTab.Stage);
        summonPanel.SetActive(contentPanelOpen && activeTab == HudTab.Summon);
        shopPanel.SetActive(contentPanelOpen && activeTab == HudTab.Shop);
        supportPanel.SetActive(contentPanelOpen && activeTab == HudTab.Support);
        if (debugPanel != null)
        {
            debugPanel.SetActive(contentPanelOpen && activeTab == HudTab.Debug);
        }

        if (heroFormationSavePrompt != null)
        {
            heroFormationSavePrompt.SetActive(heroFormationSavePromptOpen);
        }

        if (heroDetailPanel != null)
        {
            RefreshHeroDetailPanel();
            heroDetailPanel.SetActive(heroDetailPanelOpen);
        }

        foreach (KeyValuePair<HudTab, Button> pair in tabButtons)
        {
            Text text = pair.Value.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                bool activeAndOpen = contentPanelOpen && pair.Key == activeTab;
                bool heroDetailCloseTab = heroDetailPanelOpen && pair.Key == HudTab.Hero;
                string label = tabButtonLabels.TryGetValue(pair.Key, out string savedLabel) ? savedLabel : text.text;
                if (heroDetailCloseTab)
                {
                    text.text = "X\n영웅";
                }
                else if ((pair.Key == HudTab.Growth || pair.Key == HudTab.Hero) && activeAndOpen)
                {
                    text.text = GetTabCloseLabel(pair.Key);
                }
                else
                {
                    text.text = label;
                }

                text.color = activeAndOpen || heroDetailCloseTab ? new Color(1f, 0.91f, 0.40f, 1f) : Color.white;
            }
        }
    }

    private string GetTabCloseLabel(HudTab tab)
    {
        switch (tab)
        {
            case HudTab.Growth:
                return "X\n성장";
            case HudTab.Hero:
                return "X\n영웅";
            default:
                return "X";
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = Mathf.Max(0.1f, scale);
        UpdateView();
    }

    private void OpenHeroDetailPanel(string heroId)
    {
        if (FindHeroState(heroId) == null)
        {
            return;
        }

        selectedHeroDetailId = heroId;
        activeHeroDetailTab = HeroDetailTab.BasicInfo;
        selectedHeroDetailEquipmentId = string.Empty;
        selectedEquipmentDetailId = string.Empty;
        selectedDismantleEquipmentIds.Clear();
        heroDetailEquipmentSlotSelectionActive = false;
        equipmentDetailPopupOpen = false;
        equipmentDismantlePopupOpen = false;
        equipmentBulkDismantlePromptOpen = false;
        ResetHeroDetailEquipmentFilters();
        heroDetailPanelOpen = true;
        UpdateView();
    }

    private void CloseHeroDetailPanel()
    {
        StopHeroTranscendAutoRoll();
        CloseHeroTranscendConfirmPrompt();
        heroDetailPanelOpen = false;
        selectedHeroDetailId = string.Empty;
        selectedHeroDetailEquipmentId = string.Empty;
        selectedEquipmentDetailId = string.Empty;
        selectedDismantleEquipmentIds.Clear();
        heroDetailEquipmentSlotSelectionActive = false;
        equipmentDetailPopupOpen = false;
        equipmentDismantlePopupOpen = false;
        equipmentBulkDismantlePromptOpen = false;
        UpdateView();
    }

    private void SelectHeroDetailTab(HeroDetailTab tab)
    {
        activeHeroDetailTab = tab;
        if (tab != HeroDetailTab.Equipment)
        {
            selectedHeroDetailEquipmentId = string.Empty;
            heroDetailEquipmentSlotSelectionActive = false;
            equipmentDetailPopupOpen = false;
            equipmentDismantlePopupOpen = false;
            equipmentBulkDismantlePromptOpen = false;
        }

        UpdateView();
    }

    private void OpenEquipmentDetailPopup(string equipmentId)
    {
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
        if (state == null || !state.IsOwned)
        {
            return;
        }

        selectedEquipmentDetailId = equipmentId;
        equipmentDetailPopupOpen = true;
        activeHeroDetailTab = HeroDetailTab.Equipment;
        UpdateView();
    }

    private void CloseEquipmentDetailPopup()
    {
        equipmentDetailPopupOpen = false;
        selectedEquipmentDetailId = string.Empty;
        UpdateView();
    }

    private void OpenEquipmentDismantlePopup()
    {
        equipmentDetailPopupOpen = false;
        equipmentDismantlePopupOpen = true;
        equipmentBulkDismantlePromptOpen = false;
        selectedDismantleEquipmentIds.Clear();
        activeHeroDetailTab = HeroDetailTab.Equipment;
        UpdateView();
    }

    private void CloseEquipmentDismantlePopup()
    {
        equipmentDismantlePopupOpen = false;
        equipmentBulkDismantlePromptOpen = false;
        selectedDismantleEquipmentIds.Clear();
        UpdateView();
    }

    private void OpenEquipmentBulkDismantlePrompt()
    {
        equipmentBulkDismantlePromptOpen = true;
        UpdateView();
    }

    private void CloseEquipmentBulkDismantlePrompt()
    {
        equipmentBulkDismantlePromptOpen = false;
        UpdateView();
    }

    private static string BuildEquipmentCopyKey(string equipmentId, int copyIndex)
    {
        return equipmentId + "#" + copyIndex;
    }

    private static string GetEquipmentIdFromCopyKey(string equipmentCopyKey)
    {
        if (string.IsNullOrEmpty(equipmentCopyKey))
        {
            return string.Empty;
        }

        int separatorIndex = equipmentCopyKey.LastIndexOf('#');
        return separatorIndex >= 0 ? equipmentCopyKey.Substring(0, separatorIndex) : equipmentCopyKey;
    }

    private static int GetCopyIndexFromCopyKey(string equipmentCopyKey)
    {
        if (string.IsNullOrEmpty(equipmentCopyKey))
        {
            return -1;
        }

        int separatorIndex = equipmentCopyKey.LastIndexOf('#');
        if (separatorIndex < 0 || separatorIndex >= equipmentCopyKey.Length - 1)
        {
            return -1;
        }

        return int.TryParse(equipmentCopyKey.Substring(separatorIndex + 1), out int copyIndex) ? copyIndex : -1;
    }

    private void SelectDismantleEquipment(string equipmentCopyKey)
    {
        string equipmentId = GetEquipmentIdFromCopyKey(equipmentCopyKey);
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
        if (state == null || !state.IsOwned)
        {
            return;
        }

        if (equipmentInventory.GetAvailableCount(equipmentId) <= 0)
        {
            selectedDismantleEquipmentIds.Remove(equipmentCopyKey);
            ShowGrowthNotice("장착 중인 장비는 분해할 수 없습니다.");
            UpdateView();
            return;
        }

        if (selectedDismantleEquipmentIds.Contains(equipmentCopyKey))
        {
            selectedDismantleEquipmentIds.Remove(equipmentCopyKey);
        }
        else
        {
            selectedDismantleEquipmentIds.Add(equipmentCopyKey);
        }
        UpdateView();
    }

    private void ChangeBulkDismantleRarity(int direction)
    {
        int rarity = Mathf.Clamp((int)selectedBulkDismantleRarity + direction, (int)HeroRarity.Common, (int)HeroRarity.Mythic);
        selectedBulkDismantleRarity = (HeroRarity)rarity;
        UpdateView();
    }

    private void ToggleHeroDetailEquipmentFilter(EquipmentSlot slot)
    {
        activeHeroDetailTab = HeroDetailTab.Equipment;
        if (heroDetailEquipmentSelectedSlots.Contains(slot))
        {
            heroDetailEquipmentSelectedSlots.Remove(slot);
            if (heroDetailEquipmentSlotSelectionActive && selectedHeroDetailEquipmentSlot == slot)
            {
                heroDetailEquipmentSlotSelectionActive = false;
            }
        }
        else
        {
            heroDetailEquipmentSelectedSlots.Add(slot);
        }

        ClearSelectedHeroDetailEquipmentIfFilteredOut();
        UpdateView();
    }

    private void SelectHeroDetailEquipmentFilter(EquipmentSlot slot)
    {
        activeHeroDetailTab = HeroDetailTab.Equipment;
        selectedHeroDetailEquipmentSlot = slot;
        heroDetailEquipmentSlotSelectionActive = true;
        heroDetailEquipmentSelectedSlots.Clear();
        heroDetailEquipmentSelectedSlots.Add(slot);
        ClearSelectedHeroDetailEquipmentIfFilteredOut();

        UpdateView();
    }

    private void ResetHeroDetailEquipmentFilters()
    {
        heroDetailEquipmentSelectedSlots.Clear();
        foreach (EquipmentSlot slot in HeroDetailEquipmentFilterSlots)
        {
            heroDetailEquipmentSelectedSlots.Add(slot);
        }
    }

    private void ClearSelectedHeroDetailEquipmentIfFilteredOut()
    {
        EquipmentState selectedState = equipmentInventory != null
            ? equipmentInventory.GetState(selectedHeroDetailEquipmentId)
            : null;
        if (selectedState != null && !heroDetailEquipmentSelectedSlots.Contains(selectedState.Definition.Slot))
        {
            selectedHeroDetailEquipmentId = string.Empty;
        }
    }

    private void SelectHeroDetailEquipmentForSlot(string equipmentId)
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
        if (hero == null || !hero.IsOwned || state == null || !state.IsOwned)
        {
            return;
        }

        activeHeroDetailTab = HeroDetailTab.Equipment;
        if (!heroDetailEquipmentSelectedSlots.Contains(state.Definition.Slot))
        {
            heroDetailEquipmentSelectedSlots.Add(state.Definition.Slot);
        }

        bool equippedToHero = equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, equipmentId);
        if (equippedToHero)
        {
            selectedHeroDetailEquipmentId = string.Empty;
            UpdateView();
            return;
        }

        if (equipmentInventory.GetAvailableCount(equipmentId) <= 0)
        {
            selectedHeroDetailEquipmentId = string.Empty;
            UpdateView();
            return;
        }

        selectedHeroDetailEquipmentId = selectedHeroDetailEquipmentId == equipmentId ? string.Empty : equipmentId;
        UpdateView();
    }

    private void SelectOrRemoveHeroDetailEquipment(string equipmentId)
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || !hero.IsOwned || equipmentInventory == null)
        {
            return;
        }

        EquipmentState state = equipmentInventory.GetState(equipmentId);
        if (state == null || !state.IsOwned)
        {
            return;
        }

        if (equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, equipmentId))
        {
            equipmentInventory.UnequipEquipment(hero.Definition.Id, equipmentId);
            if (selectedHeroDetailEquipmentId == equipmentId)
            {
                selectedHeroDetailEquipmentId = string.Empty;
            }

            heroDetailEquipmentSlotSelectionActive = false;
            UpdateView();
            return;
        }

        if (heroDetailEquipmentSlotSelectionActive)
        {
            if (state.Definition.Slot != selectedHeroDetailEquipmentSlot)
            {
                ShowGrowthNotice(GetEquipmentSlotLabel(selectedHeroDetailEquipmentSlot) + " 칸에는 "
                    + GetEquipmentSlotLabel(state.Definition.Slot) + " 장비를 장착할 수 없습니다.");
                return;
            }

            if (equipmentInventory.Equip(hero.Definition.Id, equipmentId))
            {
                selectedHeroDetailEquipmentId = string.Empty;
                heroDetailEquipmentSlotSelectionActive = false;
            }
            else
            {
                ShowGrowthNotice("장착 가능한 장비 수량이 부족합니다.");
            }

            UpdateView();
            return;
        }

        SelectHeroDetailEquipmentForSlot(equipmentId);
    }

    private void TryPlaceSelectedHeroDetailEquipment(EquipmentSlot slot)
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || !hero.IsOwned || equipmentInventory == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(selectedHeroDetailEquipmentId))
        {
            SelectHeroDetailEquipmentFilter(slot);
            return;
        }

        EquipmentState selectedState = equipmentInventory.GetState(selectedHeroDetailEquipmentId);
        if (selectedState == null || !selectedState.IsOwned)
        {
            selectedHeroDetailEquipmentId = string.Empty;
            UpdateView();
            return;
        }

        if (selectedState.Definition.Slot != slot)
        {
            SelectHeroDetailEquipmentFilter(slot);
            return;
        }

        if (equipmentInventory.Equip(hero.Definition.Id, selectedHeroDetailEquipmentId))
        {
            selectedHeroDetailEquipmentId = string.Empty;
            heroDetailEquipmentSlotSelectionActive = false;
        }
        else
        {
            ShowGrowthNotice("장착 가능한 장비 수량이 부족합니다.");
        }

        UpdateView();
    }

    private void RemoveHeroDetailEquipment(EquipmentSlot slot)
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || !hero.IsOwned || equipmentInventory == null)
        {
            return;
        }

        equipmentInventory.Unequip(hero.Definition.Id, slot);
        selectedHeroDetailEquipmentId = string.Empty;
        heroDetailEquipmentSlotSelectionActive = false;
        UpdateView();
    }

    private void UnequipAllHeroDetailEquipment()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || !hero.IsOwned || equipmentInventory == null)
        {
            return;
        }

        int unequippedCount = equipmentInventory.UnequipAll(hero.Definition.Id);
        selectedHeroDetailEquipmentId = string.Empty;
        selectedEquipmentDetailId = string.Empty;
        equipmentDetailPopupOpen = false;
        heroDetailEquipmentSlotSelectionActive = false;
        ShowGrowthNotice(unequippedCount > 0
            ? "장비 " + unequippedCount + "개를 해제했습니다."
            : "해제할 장비가 없습니다.");
        UpdateView();
    }

    private void AutoEquipHeroDetailEquipment()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || !hero.IsOwned || equipmentInventory == null)
        {
            return;
        }

        int equippedCount = equipmentInventory.EquipBestAvailable(hero.Definition.Id);
        selectedHeroDetailEquipmentId = string.Empty;
        selectedEquipmentDetailId = string.Empty;
        equipmentDetailPopupOpen = false;
        heroDetailEquipmentSlotSelectionActive = false;
        ShowGrowthNotice(equippedCount > 0
            ? "강한 장비 " + equippedCount + "개를 자동 장착했습니다."
            : "장착할 장비가 없습니다.");
        UpdateView();
    }

    private void ToggleSelectedEquipmentDetailEquip()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(selectedEquipmentDetailId) : null;
        if (hero == null || !hero.IsOwned || state == null || !state.IsOwned)
        {
            return;
        }

        if (equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, state.Definition.Id))
        {
            equipmentInventory.UnequipEquipment(hero.Definition.Id, state.Definition.Id);
            selectedHeroDetailEquipmentId = string.Empty;
            heroDetailEquipmentSlotSelectionActive = false;
            UpdateView();
            return;
        }

        if (equipmentInventory.GetAvailableCount(state.Definition.Id) <= 0)
        {
            ShowGrowthNotice("장착 가능한 장비 수량이 부족합니다.");
            return;
        }

        if (equipmentInventory.Equip(hero.Definition.Id, state.Definition.Id))
        {
            selectedHeroDetailEquipmentId = string.Empty;
            selectedHeroDetailEquipmentSlot = state.Definition.Slot;
            heroDetailEquipmentSlotSelectionActive = false;
            equipmentDetailPopupOpen = false;
            selectedEquipmentDetailId = string.Empty;
        }

        UpdateView();
    }

    private void LevelUpSelectedEquipmentDetail()
    {
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(selectedEquipmentDetailId) : null;
        if (state == null || !state.IsOwned || wallet == null)
        {
            return;
        }

        if (state.Level >= state.MaxLevel)
        {
            ShowGrowthNotice(state.IsMaxStars ? "이미 최대 레벨입니다." : "승급 후 레벨업할 수 있습니다.");
            return;
        }

        int cost = state.LevelUpCost;
        if (wallet.EquipmentExpItem < cost)
        {
            ShowGrowthNotice("장비 레벨업 책이 부족합니다.");
            return;
        }

        if (!wallet.SpendEquipmentExpItem(cost))
        {
            ShowGrowthNotice("장비 레벨업 책이 부족합니다.");
            return;
        }

        if (!equipmentInventory.TryLevelUpEquipment(state.Definition.Id))
        {
            wallet.AddEquipmentExpItem(cost);
            ShowGrowthNotice("장비 레벨업에 실패했습니다.");
            return;
        }

        UpdateView();
    }

    private bool CanLevelUpSelectedEquipmentDetail()
    {
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(selectedEquipmentDetailId) : null;
        return state != null
            && state.IsOwned
            && wallet != null
            && state.Level < state.MaxLevel
            && wallet.EquipmentExpItem >= state.LevelUpCost;
    }

    private void StarUpSelectedEquipmentDetail()
    {
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(selectedEquipmentDetailId) : null;
        if (state == null || !state.IsOwned)
        {
            return;
        }

        if (state.IsMaxStars)
        {
            ShowGrowthNotice("이미 최대 성급입니다.");
            return;
        }

        if (equipmentInventory.GetStarUpMaterialCount(state.Definition.Id) < state.StarUpCost)
        {
            ShowGrowthNotice("승급 재료 장비가 부족합니다.");
            return;
        }

        if (!equipmentInventory.TryStarUpEquipment(state.Definition.Id))
        {
            ShowGrowthNotice("장비 승급에 실패했습니다.");
            return;
        }

        UpdateView();
    }

    private void DismantleSelectedEquipment()
    {
        if (selectedDismantleEquipmentIds.Count <= 0 || equipmentInventory == null || wallet == null)
        {
            ShowGrowthNotice("분해할 장비를 선택하세요.");
            return;
        }

        PruneInvalidDismantleSelections();
        if (selectedDismantleEquipmentIds.Count <= 0)
        {
            ShowGrowthNotice("분해할 수 있는 선택 장비가 없습니다.");
            UpdateView();
            return;
        }

        int dismantledCount = 0;
        int totalReward = 0;
        var selectedKeys = new List<string>(selectedDismantleEquipmentIds);
        foreach (string equipmentCopyKey in selectedKeys)
        {
            string equipmentId = GetEquipmentIdFromCopyKey(equipmentCopyKey);
            if (!equipmentInventory.TryDismantleEquipment(equipmentId, out int reward))
            {
                selectedDismantleEquipmentIds.Remove(equipmentCopyKey);
                continue;
            }

            dismantledCount += 1;
            totalReward += reward;
            selectedDismantleEquipmentIds.Remove(equipmentCopyKey);
        }

        if (dismantledCount <= 0)
        {
            ShowGrowthNotice("장비 분해에 실패했습니다.");
            UpdateView();
            return;
        }

        wallet.AddEquipmentExpItem(totalReward);
        ShowGrowthNotice("장비 " + dismantledCount + "개 분해: 장비책 +" + FormatShortNumber(totalReward));
        UpdateView();
    }

    private void ConfirmBulkDismantleEquipment()
    {
        if (equipmentInventory == null || wallet == null)
        {
            return;
        }

        int dismantledCount = equipmentInventory.DismantleByRarity(selectedBulkDismantleRarity, null, out int reward);
        if (dismantledCount <= 0)
        {
            ShowGrowthNotice("분해할 장비가 없습니다.");
            return;
        }

        wallet.AddEquipmentExpItem(reward);
        selectedDismantleEquipmentIds.Clear();
        equipmentBulkDismantlePromptOpen = false;
        ShowGrowthNotice("장비 " + dismantledCount + "개 분해: 장비책 +" + FormatShortNumber(reward));
        UpdateView();
    }

    private void RefreshDamageMeter()
    {
        if (damageMeterText != null)
        {
            damageMeterText.text = "데미지 미터기";
        }

        GameNumber maxDamage = GameNumber.Max(GameNumber.One, battleManager.GetMaxHeroDamageDone());
        var meterHeroes = new List<HeroState>(battleManager.DeployedHeroes);
        meterHeroes.Sort(CompareHeroesForDamageMeter);
        int deployedCount = meterHeroes.Count;
        for (int i = 0; i < damageMeterRows.Count; i++)
        {
            bool active = i < deployedCount;
            damageMeterRows[i].SetActive(active);
            if (!active)
            {
                continue;
            }

            HeroState hero = meterHeroes[i];
            GameNumber damage = battleManager.GetHeroDamageDone(hero.Definition.Id);
            float ratio = Mathf.Clamp01((float)damage.RatioTo(maxDamage));

            if (i < damageMeterFills.Count)
            {
                Image fill = damageMeterFills[i];
                fill.rectTransform.anchorMax = new Vector2(ratio, 1f);
                fill.color = Color.Lerp(GetRarityColor(hero.Definition.Rarity), new Color(1f, 0.78f, 0.18f, 1f), 0.28f);
            }

            if (i < damageMeterRowTexts.Count)
            {
                damageMeterRowTexts[i].text = GetShortHeroLabel(hero.Definition)
                    + "  " + FormatShortNumber(damage);
            }
        }
    }

    private int CompareHeroesForDamageMeter(HeroState left, HeroState right)
    {
        GameNumber rightDamage = battleManager.GetHeroDamageDone(right.Definition.Id);
        GameNumber leftDamage = battleManager.GetHeroDamageDone(left.Definition.Id);
        int damageCompare = rightDamage.CompareTo(leftDamage);
        if (damageCompare != 0)
        {
            return damageCompare;
        }

        return string.CompareOrdinal(left.Definition.Id, right.Definition.Id);
    }

    private void RefreshHeroDetailPanel()
    {
        if (!heroDetailPanelOpen)
        {
            return;
        }

        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null)
        {
            heroDetailPanelOpen = false;
            selectedHeroDetailId = string.Empty;
            selectedEquipmentDetailId = string.Empty;
            selectedDismantleEquipmentIds.Clear();
            equipmentDetailPopupOpen = false;
            equipmentDismantlePopupOpen = false;
            equipmentBulkDismantlePromptOpen = false;
            return;
        }

        if (heroDetailTitleText != null)
        {
            heroDetailTitleText.text = "i  상세 정보";
        }

        if (heroDetailTraitText != null)
        {
            heroDetailTraitText.text = GetTraitLabel(hero.Definition.Trait)
                + "\n" + hero.Definition.RarityLabel;
        }

        if (heroDetailStarsText != null)
        {
            heroDetailStarsText.text = FormatStars(hero.Stars) + "  " + hero.Stars + "/" + HeroDefinition.MaxStars + "성";
        }

        if (heroDetailCharacterText != null)
        {
            heroDetailCharacterText.text = hero.Definition.DisplayName
                + "\n<size=30>" + hero.Definition.Role + "</size>"
                + "\n<size=28>" + hero.Definition.PassiveLabel + "</size>";
            heroDetailCharacterText.color = Color.Lerp(GetRarityColor(hero.Definition.Rarity), Color.white, 0.18f);
        }

        if (heroDetailLevelText != null)
        {
            heroDetailLevelText.text = "Lv. " + hero.Level + "/" + hero.MaxLevel
                + "    15성 최대 " + HeroDefinition.MaxLevelAtMaxStars;
        }

        if (heroDetailPowerText != null)
        {
            heroDetailPowerText.text = "전투력  " + FormatShortNumber(GetHeroDetailCombatPower(hero));
        }

        if (heroDetailExpBookText != null)
        {
            heroDetailExpBookText.text = activeHeroDetailTab == HeroDetailTab.Equipment
                ? "장비책  " + FormatShortNumber(wallet.EquipmentExpItem)
                : "경험치책  " + FormatShortNumber(wallet.HeroExpItem);
        }

        if (activeHeroDetailTab == HeroDetailTab.Transcend && heroDetailExpBookText != null)
        {
            heroDetailExpBookText.text = "초월석  " + FormatCountNumber(wallet.HeroTranscendStone);
        }

        if (heroDetailSkillText != null)
        {
            heroDetailSkillText.text = GetHeroSkillName(hero)
                + "\n" + GetHeroSkillDescription(hero);
        }

        if (heroDetailStatsText != null)
        {
            heroDetailStatsText.text = "공격력  " + FormatShortNumber(hero.AttackPower)
                + "        체력  " + FormatShortNumber(hero.MaxHp)
                + "\n공속  " + hero.AttackSpeed.ToString("0.##")
                + "        이속  " + hero.MoveSpeed.ToString("0.#");
        }

        if (heroDetailStarEffectsText != null)
        {
            heroDetailStarEffectsText.text = GetStarEffectLine(hero, 5, "패시브 효과 50% 강화")
                + "\n" + GetStarEffectLine(hero, 10, "공격력/체력/공속/이속 +10%");
        }

        if (heroDetailOwnedEffectText != null)
        {
            heroDetailOwnedEffectText.text = hero.IsOwned
                ? "[보유 효과]  공격력 +" + battleManager.GetHeroOwnedAttackBonusPercent(hero).ToString("0.##") + "%"
                : "[미보유]  뽑기로 조각을 획득하면 배치 가능";
        }

        if (heroDetailNoticeText != null)
        {
            heroDetailNoticeText.text = Time.unscaledTime < growthNoticeUntil ? growthNoticeMessage : string.Empty;
        }

        RefreshHeroDetailActionButtons(hero);
        RefreshHeroDetailEquipmentSlots(hero);
        RefreshHeroDetailTabState();
        RefreshHeroDetailEquipmentContent();
        RefreshHeroDetailTranscendContent(hero);
        RefreshEquipmentDetailPopup();
        RefreshEquipmentDismantlePopup();
        RefreshEquipmentBulkDismantlePrompt();
    }

    private void RefreshHeroDetailActionButtons(HeroState hero)
    {
        bool inFormation = IsHeroInEditingFormation(hero.Definition.Id);
        bool isOwned = hero.IsOwned;
        bool maxLevel = hero.Level >= hero.MaxLevel;
        bool canPayLevelUp = isOwned && wallet != null && wallet.HeroExpItem >= hero.LevelUpCost;
        bool maxStars = hero.IsMaxStars;
        bool canStarUp = isOwned && hero.CanStarUp;

        if (heroDetailExcludeButton != null)
        {
            heroDetailExcludeButton.interactable = isOwned;
            SetButtonText(heroDetailExcludeButton, !isOwned ? "미보유" : inFormation ? "제외" : "배치");
            SetButtonColor(heroDetailExcludeButton, !isOwned
                ? new Color(0.35f, 0.36f, 0.38f, 1f)
                : inFormation
                ? new Color(0.54f, 0.76f, 0.96f, 1f)
                : new Color(0.54f, 0.78f, 0.22f, 1f));
        }

        if (heroDetailLevelUpButton != null)
        {
            heroDetailLevelUpButton.interactable = isOwned;
            SetButtonText(heroDetailLevelUpButton, !isOwned
                ? "레벨업\n미보유"
                : maxLevel
                ? "레벨업\nMAX"
                : "레벨업\n" + FormatShortNumber(hero.LevelUpCost));
            SetButtonColor(heroDetailLevelUpButton, !isOwned || maxLevel
                ? new Color(0.26f, 0.27f, 0.29f, 1f)
                : canPayLevelUp ? new Color(0.54f, 0.78f, 0.22f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f));
        }

        if (heroDetailStarUpButton != null)
        {
            heroDetailStarUpButton.interactable = isOwned;
            SetButtonText(heroDetailStarUpButton, !isOwned
                ? "승급\n미보유"
                : maxStars
                ? "승급\nMAX"
                : "승급\n" + FormatCountNumber(hero.Shards) + "/" + FormatCountNumber(hero.StarUpCost));
            SetButtonColor(heroDetailStarUpButton, !isOwned || maxStars
                ? new Color(0.26f, 0.27f, 0.29f, 1f)
                : canStarUp ? new Color(0.54f, 0.72f, 0.96f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f));
        }
    }

    private void RefreshHeroDetailEquipmentSlots(HeroState hero)
    {
        EquipmentState selectedState = equipmentInventory != null ? equipmentInventory.GetState(selectedHeroDetailEquipmentId) : null;
        foreach (KeyValuePair<EquipmentSlot, Text> pair in heroDetailEquipmentSlotTexts)
        {
            EquipmentSlot slot = pair.Key;
            Text slotText = pair.Value;
            EquipmentState equippedState = equipmentInventory != null ? equipmentInventory.GetEquippedState(hero.Definition.Id, slot) : null;
            bool occupied = equippedState != null && equippedState.IsOwned;
            bool selectedEquipmentSlot = selectedState != null && selectedState.Definition.Slot == slot;
            bool selectedTargetSlot = heroDetailEquipmentSlotSelectionActive && selectedHeroDetailEquipmentSlot == slot;
            bool selectedSlot = selectedEquipmentSlot || selectedTargetSlot;
            string label = GetEquipmentSlotLabel(slot);

            if (occupied)
            {
                slotText.text = "Lv." + equippedState.Level
                    + "\n" + label
                    + "\n" + equippedState.Definition.DisplayName;
                slotText.color = Color.white;
            }
            else if (selectedSlot)
            {
                slotText.text = "+\n" + label + "\n선택중";
                slotText.color = new Color(1f, 0.91f, 0.40f, 1f);
            }
            else
            {
                slotText.text = "+\n" + label;
                slotText.color = new Color(0.72f, 0.76f, 0.88f, 1f);
            }

            if (heroDetailEquipmentSlotButtons.TryGetValue(slot, out Button slotButton))
            {
                Color color = occupied
                    ? selectedTargetSlot
                        ? Color.Lerp(GetRarityColor(equippedState.Definition.Rarity), new Color(1f, 0.91f, 0.40f, 1f), 0.42f)
                        : GetRarityColor(equippedState.Definition.Rarity)
                    : selectedSlot ? new Color(0.54f, 0.45f, 0.16f, 1f) : new Color(0.28f, 0.18f, 0.29f, 0.88f);
                SetButtonColor(slotButton, color);
            }

            if (heroDetailEquipmentSlotRemoveButtons.TryGetValue(slot, out Button removeButton))
            {
                removeButton.gameObject.SetActive(occupied);
            }
        }
    }

    private void RefreshHeroDetailTabState()
    {
        bool basicInfoActive = activeHeroDetailTab == HeroDetailTab.BasicInfo;
        bool equipmentActive = activeHeroDetailTab == HeroDetailTab.Equipment;
        bool transcendActive = activeHeroDetailTab == HeroDetailTab.Transcend;

        if (heroDetailSkillText != null)
        {
            heroDetailSkillText.gameObject.SetActive(basicInfoActive);
        }

        if (heroDetailStatsPanel != null)
        {
            heroDetailStatsPanel.SetActive(basicInfoActive);
        }

        if (heroDetailOwnedEffectText != null)
        {
            heroDetailOwnedEffectText.gameObject.SetActive(basicInfoActive);
        }

        if (heroDetailNoticeText != null)
        {
            heroDetailNoticeText.gameObject.SetActive(basicInfoActive || transcendActive);
        }

        if (heroDetailActionRow != null)
        {
            heroDetailActionRow.SetActive(basicInfoActive);
        }

        if (heroDetailEquipmentContent != null)
        {
            heroDetailEquipmentContent.SetActive(equipmentActive);
        }

        if (heroDetailTranscendContent != null)
        {
            heroDetailTranscendContent.SetActive(transcendActive);
        }

        foreach (KeyValuePair<HeroDetailTab, Button> pair in heroDetailTabButtons)
        {
            bool selected = pair.Key == activeHeroDetailTab;
            SetButtonColor(pair.Value, selected ? new Color(0.56f, 0.68f, 0.94f, 1f) : new Color(0.20f, 0.27f, 0.42f, 1f));
        }
    }

    private void RefreshHeroDetailTranscendContent(HeroState hero)
    {
        if (heroDetailTranscendContent == null || hero == null)
        {
            return;
        }

        selectedHeroTranscendSlotIndex = Mathf.Clamp(selectedHeroTranscendSlotIndex, 0, HeroDefinition.MaxTranscendSlots - 1);
        int selectedRequiredStars = HeroDefinition.GetTranscendRequiredStars(selectedHeroTranscendSlotIndex);
        bool selectedUnlocked = hero.IsTranscendSlotUnlocked(selectedHeroTranscendSlotIndex);
        int unlockedSlotCount = CountUnlockedHeroTranscendSlots(hero);
        int lockedSlotCount = CountLockedHeroTranscendSlots(hero);
        int changeableSlotCount = CountChangeableHeroTranscendSlots(hero);

        if (heroDetailTranscendText != null)
        {
            heroDetailTranscendText.text = "초월 슬롯 " + (selectedHeroTranscendSlotIndex + 1)
                + " / " + HeroDefinition.MaxTranscendSlots
                + "    " + (selectedUnlocked ? "변경 가능" : selectedRequiredStars + "성 필요")
                + "\n전용 옵션과 공용 옵션 중 하나가 등장합니다.";
        }

        if (heroDetailTranscendText != null)
        {
            heroDetailTranscendText.text = "초월 슬롯 " + (selectedHeroTranscendSlotIndex + 1)
                + " / " + HeroDefinition.MaxTranscendSlots
                + "    " + (selectedUnlocked ? "선택됨" : selectedRequiredStars + "성 필요")
                + "\n열림 " + unlockedSlotCount + "칸 / 잠금 " + lockedSlotCount + "칸 / 변경 " + changeableSlotCount + "칸";
        }

        for (int i = 0; i < heroDetailTranscendSlotTexts.Count && i < HeroDefinition.MaxTranscendSlots; i++)
        {
            bool unlocked = hero.IsTranscendSlotUnlocked(i);
            bool selected = selectedHeroTranscendSlotIndex == i;
            bool locked = unlocked && IsHeroTranscendSlotLocked(hero.Definition.Id, i);
            int requiredStars = HeroDefinition.GetTranscendRequiredStars(i);
            string optionId = hero.GetTranscendOptionId(i);
            HeroTranscendOptionDefinition option = string.IsNullOrEmpty(optionId)
                ? null
                : GameData.GetHeroTranscendOption(optionId);
            Text slotText = heroDetailTranscendSlotTexts[i];

            if (unlocked && option != null)
            {
                slotText.text = "<size=34><color=" + GetTranscendGradeHex(option.Grade) + ">" + option.Grade + "</color></size>"
                    + "  [" + option.ScopeLabel + "] " + option.Description
                    + "\n<size=22>슬롯 " + (i + 1) + "  해금 " + requiredStars + "성  가중치 " + option.ProbabilityWeight.ToString("0.####") + "</size>";
            }
            else if (unlocked)
            {
                slotText.text = "<size=34>옵션 없음</size>\n<size=22>변경을 눌러 옵션을 부여하세요.</size>";
            }
            else
            {
                slotText.text = "<size=30>잠김</size>  " + FormatStars(requiredStars)
                    + "\n<size=22>" + requiredStars + "성부터 추가 초월 가능</size>";
            }

            if (unlocked && locked)
            {
                slotText.text = "[잠금] " + slotText.text;
            }

            if (heroDetailTranscendSlotButtons.Count > i)
            {
                Color color = unlocked
                    ? selected
                        ? Color.Lerp(GetTranscendGradeColor(option != null ? option.Grade : HeroTranscendGrade.F), new Color(1f, 0.92f, 0.42f, 1f), 0.34f)
                        : GetTranscendGradeColor(option != null ? option.Grade : HeroTranscendGrade.F)
                    : new Color(0.19f, 0.22f, 0.30f, 1f);
                SetButtonColor(heroDetailTranscendSlotButtons[i], color);
                if (locked)
                {
                    SetButtonColor(heroDetailTranscendSlotButtons[i], new Color(0.18f, 0.20f, 0.24f, 1f));
                }
            }

            if (heroDetailTranscendLockButtons.Count > i)
            {
                Button lockButton = heroDetailTranscendLockButtons[i];
                lockButton.gameObject.SetActive(unlocked);
                SetButtonText(lockButton, locked ? "해" : "잠");
                SetButtonColor(lockButton, locked
                    ? new Color(0.72f, 0.46f, 0.16f, 1f)
                    : new Color(0.20f, 0.25f, 0.36f, 1f));
            }
        }

        if (heroDetailTranscendStopButton != null)
        {
            SetButtonText(heroDetailTranscendStopButton, (heroTranscendStopOnlySs ? "[x] " : "[ ] ") + "자동 변경시 SS만 정지");
            SetButtonColor(heroDetailTranscendStopButton, heroTranscendStopOnlySs
                ? new Color(0.46f, 0.62f, 0.30f, 1f)
                : new Color(0.26f, 0.32f, 0.43f, 1f));
        }

        int rollCost = GetHeroTranscendRollCost(hero);
        bool canRoll = changeableSlotCount > 0 && wallet != null && wallet.HeroTranscendStone >= rollCost;
        if (heroDetailTranscendChangeButton != null)
        {
            SetButtonText(heroDetailTranscendChangeButton, "변경\n" + rollCost);
            SetButtonColor(heroDetailTranscendChangeButton, canRoll ? new Color(0.28f, 0.72f, 0.92f, 1f) : new Color(0.35f, 0.36f, 0.38f, 1f));
        }

        if (heroDetailTranscendAutoButton != null)
        {
            SetButtonText(heroDetailTranscendAutoButton, heroTranscendAutoRolling
                ? "자동 변경\n중지"
                : heroTranscendStopOnlySs ? "자동 변경\nSS 정지" : "자동 변경\nS 이상 정지");
            SetButtonColor(heroDetailTranscendAutoButton, heroTranscendAutoRolling
                ? new Color(0.86f, 0.52f, 0.16f, 1f)
                : canRoll ? new Color(0.70f, 0.24f, 0.82f, 1f) : new Color(0.35f, 0.36f, 0.38f, 1f));
        }
    }

    private void ToggleHeroTranscendStopMode()
    {
        heroTranscendStopOnlySs = !heroTranscendStopOnlySs;
        PlayerPrefs.SetInt(SaveKeys.HeroTranscendStopOnlySs, heroTranscendStopOnlySs ? 1 : 0);
        PlayerPrefs.Save();
        UpdateView();
    }

    private void RollSelectedHeroTranscendManual()
    {
        RequestHeroTranscendRoll(false);
    }

    private void AutoRollSelectedHeroTranscend()
    {
        RequestHeroTranscendRoll(true);
    }

    private void RequestHeroTranscendRoll(bool autoRoll)
    {
        if (autoRoll && heroTranscendAutoRolling)
        {
            StopHeroTranscendAutoRoll();
            ShowGrowthNotice("자동 변경을 중지했습니다.");
            UpdateView();
            return;
        }

        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null)
        {
            return;
        }

        if (!hero.IsOwned)
        {
            ShowGrowthNotice("아직 획득하지 않은 영웅입니다.");
            return;
        }

        if (HasSsInChangeableHeroTranscendSlots(hero))
        {
            OpenHeroTranscendConfirmPrompt(autoRoll);
            return;
        }

        ExecuteHeroTranscendRoll(autoRoll);
    }

    private void ExecuteHeroTranscendRoll(bool autoRoll)
    {
        if (autoRoll)
        {
            StartHeroTranscendAutoRoll();
            return;
        }

        TryRollHeroTranscendBatch(true, out _, out _);
        UpdateView();
    }

    private void OpenHeroTranscendConfirmPrompt(bool autoRoll)
    {
        pendingHeroTranscendAutoRoll = autoRoll;
        if (heroTranscendConfirmMessageText != null)
        {
            heroTranscendConfirmMessageText.text = "변경 대상에 SS 옵션이 있습니다.\n계속 변경하시겠습니까?";
        }

        if (heroTranscendConfirmPrompt != null)
        {
            heroTranscendConfirmPrompt.SetActive(true);
        }
    }

    private void ConfirmHeroTranscendRollPrompt()
    {
        bool autoRoll = pendingHeroTranscendAutoRoll;
        CloseHeroTranscendConfirmPrompt();
        ExecuteHeroTranscendRoll(autoRoll);
    }

    private void CancelHeroTranscendRollPrompt()
    {
        CloseHeroTranscendConfirmPrompt();
    }

    private void CloseHeroTranscendConfirmPrompt()
    {
        pendingHeroTranscendAutoRoll = false;
        if (heroTranscendConfirmPrompt != null)
        {
            heroTranscendConfirmPrompt.SetActive(false);
        }
    }

    private bool HasSsInChangeableHeroTranscendSlots(HeroState hero)
    {
        if (hero == null)
        {
            return false;
        }

        List<int> targetSlots = GetChangeableHeroTranscendSlots(hero);
        foreach (int slotIndex in targetSlots)
        {
            HeroTranscendOptionDefinition option = GameData.GetHeroTranscendOption(hero.GetTranscendOptionId(slotIndex));
            if (option != null && option.Grade >= HeroTranscendGrade.SS)
            {
                return true;
            }
        }

        return false;
    }

    private void StartHeroTranscendAutoRoll()
    {
        if (heroTranscendAutoRollCoroutine != null)
        {
            return;
        }

        heroTranscendAutoRollCoroutine = StartCoroutine(RunHeroTranscendAutoRoll());
        UpdateView();
    }

    private void StopHeroTranscendAutoRoll()
    {
        if (heroTranscendAutoRollCoroutine != null)
        {
            StopCoroutine(heroTranscendAutoRollCoroutine);
            heroTranscendAutoRollCoroutine = null;
        }

        heroTranscendAutoRolling = false;
    }

    private IEnumerator RunHeroTranscendAutoRoll()
    {
        heroTranscendAutoRolling = true;
        int rolls = 0;
        HeroTranscendOptionDefinition lastOption = null;
        while (true)
        {
            if (!TryRollHeroTranscendBatch(false, out lastOption, out _))
            {
                break;
            }

            rolls += 1;
            if (ShouldStopHeroTranscendAuto(lastOption))
            {
                break;
            }

            UpdateView();
            yield return new WaitForSecondsRealtime(HeroTranscendAutoRollIntervalSeconds);
        }

        heroTranscendAutoRolling = false;
        heroTranscendAutoRollCoroutine = null;

        if (rolls > 0 && lastOption != null)
        {
            ShowGrowthNotice("자동 변경 " + rolls + "회: " + lastOption.Grade + " " + lastOption.Description);
        }
        else
        {
            ShowGrowthNotice("자동 변경할 수 없습니다.");
        }

        UpdateView();
    }

    private bool TryRollHeroTranscendBatch(bool showNotice, out HeroTranscendOptionDefinition bestOption, out int changedSlots)
    {
        bestOption = null;
        changedSlots = 0;
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || !hero.IsOwned)
        {
            return false;
        }

        List<int> targetSlots = GetChangeableHeroTranscendSlots(hero);
        if (targetSlots.Count <= 0)
        {
            if (showNotice)
            {
                ShowGrowthNotice("변경할 초월칸이 없습니다.");
            }

            return false;
        }

        int cost = GetHeroTranscendRollCost(hero);
        if (wallet == null || !wallet.SpendHeroTranscendStone(cost))
        {
            if (showNotice)
            {
                ShowGrowthNotice("초월석이 부족합니다.");
            }

            return false;
        }

        foreach (int slotIndex in targetSlots)
        {
            if (!battleManager.TryRollHeroTranscendOption(hero.Definition.Id, slotIndex, false, out HeroTranscendOptionDefinition option))
            {
                continue;
            }

            changedSlots += 1;
            if (IsBetterTranscendOption(option, bestOption))
            {
                bestOption = option;
            }
        }

        if (changedSlots <= 0)
        {
            if (wallet != null)
            {
                wallet.AddHeroTranscendStone(cost);
            }

            if (showNotice)
            {
                ShowGrowthNotice("초월 변경에 실패했습니다.");
            }

            return false;
        }

        if (showNotice)
        {
            ShowGrowthNotice("초월 변경 " + changedSlots + "칸: "
                + (bestOption != null ? bestOption.Grade + " " + bestOption.Description : "완료"));
        }

        return true;
    }

    private int GetHeroTranscendRollCost(HeroState hero)
    {
        return 10 + CountLockedHeroTranscendSlots(hero) * 10;
    }

    private List<int> GetChangeableHeroTranscendSlots(HeroState hero)
    {
        var slots = new List<int>();
        if (hero == null)
        {
            return slots;
        }

        for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
        {
            if (hero.IsTranscendSlotUnlocked(i) && !IsHeroTranscendSlotLocked(hero.Definition.Id, i))
            {
                slots.Add(i);
            }
        }

        return slots;
    }

    private int CountUnlockedHeroTranscendSlots(HeroState hero)
    {
        if (hero == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
        {
            if (hero.IsTranscendSlotUnlocked(i))
            {
                count += 1;
            }
        }

        return count;
    }

    private int CountLockedHeroTranscendSlots(HeroState hero)
    {
        if (hero == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
        {
            if (hero.IsTranscendSlotUnlocked(i) && IsHeroTranscendSlotLocked(hero.Definition.Id, i))
            {
                count += 1;
            }
        }

        return count;
    }

    private int CountChangeableHeroTranscendSlots(HeroState hero)
    {
        if (hero == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
        {
            if (hero.IsTranscendSlotUnlocked(i) && !IsHeroTranscendSlotLocked(hero.Definition.Id, i))
            {
                count += 1;
            }
        }

        return count;
    }

    private bool IsHeroTranscendSlotLocked(string heroId, int slotIndex)
    {
        return !string.IsNullOrEmpty(heroId)
            && slotIndex >= 0
            && slotIndex < HeroDefinition.MaxTranscendSlots
            && PlayerPrefs.GetInt(SaveKeys.HeroTranscendLocked(heroId, slotIndex), 0) == 1;
    }

    private void ToggleHeroTranscendSlotLock(int slotIndex)
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null)
        {
            return;
        }

        if (!hero.IsTranscendSlotUnlocked(slotIndex))
        {
            ShowGrowthNotice(HeroDefinition.GetTranscendRequiredStars(slotIndex) + "성부터 잠금할 수 있습니다.");
            return;
        }

        bool locked = IsHeroTranscendSlotLocked(hero.Definition.Id, slotIndex);
        PlayerPrefs.SetInt(SaveKeys.HeroTranscendLocked(hero.Definition.Id, slotIndex), locked ? 0 : 1);
        PlayerPrefs.Save();
        selectedHeroTranscendSlotIndex = slotIndex;
        UpdateView();
    }

    private bool IsBetterTranscendOption(HeroTranscendOptionDefinition candidate, HeroTranscendOptionDefinition current)
    {
        if (candidate == null)
        {
            return false;
        }

        return current == null || candidate.Grade > current.Grade;
    }

    private bool ShouldStopHeroTranscendAuto(HeroTranscendOptionDefinition option)
    {
        if (option == null)
        {
            return false;
        }

        return heroTranscendStopOnlySs
            ? option.Grade >= HeroTranscendGrade.SS
            : option.Grade >= HeroTranscendGrade.S;
    }

    private void HideEquipmentCards(Dictionary<string, Button> cardButtons)
    {
        foreach (KeyValuePair<string, Button> pair in cardButtons)
        {
            if (pair.Value != null)
            {
                pair.Value.gameObject.SetActive(false);
            }
        }
    }

    private void RefreshHeroDetailEquipmentCard(
        EquipmentDefinition equipment,
        EquipmentState state,
        string cardKey,
        bool equippedToHero,
        int copyNumber)
    {
        Button cardButton = GetOrCreateHeroDetailEquipmentCard(equipment, cardKey);
        if (cardButton == null)
        {
            return;
        }

        cardButton.gameObject.SetActive(true);
        cardButton.transform.SetAsLastSibling();
        bool selected = selectedHeroDetailEquipmentId == equipment.Id && !equippedToHero;
        Color color = equippedToHero
            ? new Color(0.13f, 0.15f, 0.18f, 1f)
            : selected ? new Color(0.54f, 0.45f, 0.16f, 1f) : GetRarityColor(equipment.Rarity);
        SetButtonColor(cardButton, color);

        if (heroDetailEquipmentCardTexts.TryGetValue(cardKey, out Text cardText))
        {
            cardText.text = BuildEquipmentCardText(equipment, state, equippedToHero, copyNumber);
        }

        if (heroDetailEquipmentActionButtons.TryGetValue(cardKey, out Button actionButton))
        {
            actionButton.gameObject.SetActive(true);
            actionButton.interactable = true;
            Text actionText = actionButton.GetComponentInChildren<Text>(true);
            if (actionText != null)
            {
                actionText.text = equippedToHero ? "-" : "+";
            }

            SetButtonColor(actionButton, equippedToHero
                ? new Color(0.58f, 0.12f, 0.12f, 1f)
                : new Color(0.88f, 0.72f, 0.20f, 1f));
        }
    }

    private Button GetOrCreateHeroDetailEquipmentCard(EquipmentDefinition equipment, string cardKey)
    {
        if (heroDetailEquipmentCardButtons.TryGetValue(cardKey, out Button existingButton))
        {
            return existingButton;
        }

        if (heroDetailEquipmentGridTransform == null)
        {
            return null;
        }

        Button card = CreateButton(equipment.DisplayName, heroDetailEquipmentGridTransform, 18, GetRarityColor(equipment.Rarity));
        string equipmentId = equipment.Id;
        card.onClick.AddListener(() => OpenEquipmentDetailPopup(equipmentId));
        Text text = card.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 18;
            heroDetailEquipmentCardTexts[cardKey] = text;
        }

        heroDetailEquipmentCardButtons[cardKey] = card;
        Button actionButton = CreateCornerActionButton("+", card.transform, new Color(0.88f, 0.72f, 0.20f, 1f));
        actionButton.onClick.AddListener(() => SelectOrRemoveHeroDetailEquipment(equipmentId));
        heroDetailEquipmentActionButtons[cardKey] = actionButton;
        return card;
    }

    private void RefreshHeroDetailEquipmentContent()
    {
        foreach (KeyValuePair<EquipmentSlot, Button> pair in heroDetailEquipmentFilterButtons)
        {
            bool selected = heroDetailEquipmentSelectedSlots.Contains(pair.Key);
            SetButtonText(pair.Value, BuildEquipmentFilterButtonLabel(pair.Key));
            SetButtonColor(pair.Value, selected ? new Color(0.46f, 0.62f, 0.30f, 1f) : new Color(0.24f, 0.30f, 0.42f, 1f));
        }

        foreach (KeyValuePair<EquipmentSlot, Button> pair in equipmentDismantleFilterButtons)
        {
            bool selected = heroDetailEquipmentSelectedSlots.Contains(pair.Key);
            SetButtonText(pair.Value, BuildEquipmentFilterButtonLabel(pair.Key));
            SetButtonColor(pair.Value, selected ? new Color(0.46f, 0.62f, 0.30f, 1f) : new Color(0.24f, 0.30f, 0.42f, 1f));
        }

        if (heroDetailEquipmentContent == null || equipmentInventory == null)
        {
            return;
        }

        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || !hero.IsOwned)
        {
            HideEquipmentCards(heroDetailEquipmentCardButtons);
            if (heroDetailEquipmentSummaryText != null)
            {
                heroDetailEquipmentSummaryText.text = "미보유 영웅은 장비를 장착할 수 없습니다.";
            }

            if (heroDetailEquipmentEmptyText != null)
            {
                heroDetailEquipmentEmptyText.text = "뽑기로 조각을 획득하면 장비 장착이 열립니다.";
                heroDetailEquipmentEmptyText.gameObject.SetActive(true);
            }

            return;
        }

        EquipmentState selectedState = equipmentInventory.GetState(selectedHeroDetailEquipmentId);
        HideEquipmentCards(heroDetailEquipmentCardButtons);
        int visibleCount = 0;
        int ownedCount = 0;
        foreach (EquipmentDefinition equipment in GameData.Equipments)
        {
            EquipmentState state = equipmentInventory.GetState(equipment.Id);
            bool owned = state != null && state.IsOwned;
            bool visible = owned && heroDetailEquipmentSelectedSlots.Contains(equipment.Slot);
            if (!owned)
            {
                continue;
            }

            ownedCount += state.Count;
            bool equippedToHero = hero != null && equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, equipment.Id);
            int availableCount = equipmentInventory.GetAvailableCount(equipment.Id);

            if (visible)
            {
                if (equippedToHero)
                {
                    RefreshHeroDetailEquipmentCard(equipment, state, BuildEquipmentCopyKey(equipment.Id, -1), true, 0);
                    visibleCount += 1;
                }

                for (int i = 0; i < availableCount; i++)
                {
                    RefreshHeroDetailEquipmentCard(equipment, state, BuildEquipmentCopyKey(equipment.Id, i), false, i + 1);
                    visibleCount += 1;
                }
            }
        }

        if (heroDetailEquipmentSummaryText != null)
        {
            string filterLabel = BuildEquipmentFilterSummaryLabel();
            string slotLabel = heroDetailEquipmentSlotSelectionActive
                ? "    장착 칸 " + GetEquipmentSlotLabel(selectedHeroDetailEquipmentSlot)
                : string.Empty;
            string selectedLabel = selectedState != null
                ? "    선택 " + selectedState.Definition.DisplayName + " → 슬롯 클릭"
                : string.Empty;
            heroDetailEquipmentSummaryText.text = "필터 " + filterLabel
                + "    보유 " + ownedCount
                + "    표시 " + visibleCount
                + slotLabel
                + selectedLabel;
        }

        if (heroDetailEquipmentEmptyText != null)
        {
            heroDetailEquipmentEmptyText.text = "표시할 장비가 없습니다.";
            heroDetailEquipmentEmptyText.gameObject.SetActive(visibleCount <= 0);
        }
    }

    private void RefreshEquipmentDetailPopup()
    {
        if (equipmentDetailPopup == null)
        {
            return;
        }

        if (!equipmentDetailPopupOpen)
        {
            equipmentDetailPopup.SetActive(false);
            return;
        }

        HeroState hero = FindHeroState(selectedHeroDetailId);
        EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(selectedEquipmentDetailId) : null;
        if (hero == null || state == null || !state.IsOwned)
        {
            equipmentDetailPopupOpen = false;
            selectedEquipmentDetailId = string.Empty;
            equipmentDetailPopup.SetActive(false);
            return;
        }

        bool equippedToHero = equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, state.Definition.Id);
        int availableCount = equipmentInventory.GetAvailableCount(state.Definition.Id);
        int starMaterialCount = equipmentInventory.GetStarUpMaterialCount(state.Definition.Id);
        bool levelCap = state.Level >= state.MaxLevel;
        bool absoluteMaxLevel = state.IsMaxStars && levelCap;
        bool canPayLevelUp = wallet != null && wallet.EquipmentExpItem >= state.LevelUpCost;
        bool canStarUp = !state.IsMaxStars && starMaterialCount >= state.StarUpCost;

        if (equipmentDetailIconText != null)
        {
            equipmentDetailIconText.text = "Lv." + state.Level
                + "\n" + state.Definition.SlotLabel
                + "\n" + FormatStars(state.Stars);
            equipmentDetailIconText.color = Color.Lerp(GetRarityColor(state.Definition.Rarity), Color.white, 0.15f);
        }

        if (equipmentDetailMetaText != null)
        {
            equipmentDetailMetaText.text = state.Definition.SlotLabel
                + "    " + state.Definition.RarityLabel
                + (equippedToHero ? "\n장착중" : "\n보유 x" + state.Count + " / 남음 " + availableCount);
        }

        if (equipmentDetailTitleText != null)
        {
            equipmentDetailTitleText.text = state.Definition.DisplayName
                + "\n<size=28>Lv. " + state.Level + "/" + state.MaxLevel
                + "    " + state.Stars + "/" + EquipmentDefinition.MaxStars + "성</size>";
        }

        if (equipmentDetailStatsText != null)
        {
            equipmentDetailStatsText.text = "공격력"
                + "\n+" + FormatShortNumber(state.AttackBonus)
                + "\n체력"
                + "\n+" + FormatShortNumber(state.HpBonus);
        }

        if (equipmentDetailSetText != null)
        {
            equipmentDetailSetText.text = BuildEquipmentDetailEffectText(state);
        }

        if (equipmentDetailBookText != null)
        {
            equipmentDetailBookText.text = "장비책  " + FormatShortNumber(wallet.EquipmentExpItem);
        }

        if (equipmentDetailNoticeText != null)
        {
            equipmentDetailNoticeText.text = Time.unscaledTime < growthNoticeUntil ? growthNoticeMessage : string.Empty;
        }

        if (equipmentDetailEquipButton != null)
        {
            equipmentDetailEquipButton.interactable = true;
            SetButtonText(equipmentDetailEquipButton, equippedToHero ? "해제" : "장착");
            SetButtonColor(equipmentDetailEquipButton, equippedToHero
                ? new Color(0.54f, 0.76f, 0.96f, 1f)
                : availableCount > 0 ? new Color(0.54f, 0.78f, 0.22f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f));
        }

        if (equipmentDetailLevelUpButton != null)
        {
            equipmentDetailLevelUpButton.interactable = true;
            SetButtonText(equipmentDetailLevelUpButton, absoluteMaxLevel
                ? "레벨업\nMAX"
                : levelCap
                    ? "레벨업\n승급 필요"
                    : "레벨업\n" + FormatShortNumber(state.LevelUpCost));
            SetButtonColor(equipmentDetailLevelUpButton, absoluteMaxLevel || levelCap
                ? new Color(0.34f, 0.35f, 0.36f, 1f)
                : canPayLevelUp ? new Color(0.54f, 0.78f, 0.22f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f));
        }

        if (equipmentDetailStarUpButton != null)
        {
            equipmentDetailStarUpButton.interactable = true;
            SetButtonText(equipmentDetailStarUpButton, state.IsMaxStars
                ? "승급\nMAX"
                : "승급\n" + FormatCountNumber(starMaterialCount) + "/" + FormatCountNumber(state.StarUpCost));
            SetButtonColor(equipmentDetailStarUpButton, state.IsMaxStars
                ? new Color(0.34f, 0.35f, 0.36f, 1f)
                : canStarUp ? new Color(0.88f, 0.62f, 0.16f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f));
        }

        equipmentDetailPopup.SetActive(true);
    }

    private void PruneInvalidDismantleSelections()
    {
        if (selectedDismantleEquipmentIds.Count <= 0)
        {
            return;
        }

        var selectedKeys = new List<string>(selectedDismantleEquipmentIds);
        foreach (string equipmentCopyKey in selectedKeys)
        {
            string equipmentId = GetEquipmentIdFromCopyKey(equipmentCopyKey);
            int copyIndex = GetCopyIndexFromCopyKey(equipmentCopyKey);
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
            int availableCount = equipmentInventory != null ? equipmentInventory.GetAvailableCount(equipmentId) : 0;
            if (state == null
                || !state.IsOwned
                || availableCount <= 0
                || copyIndex < 0
                || copyIndex >= availableCount
                || !heroDetailEquipmentSelectedSlots.Contains(state.Definition.Slot))
            {
                selectedDismantleEquipmentIds.Remove(equipmentCopyKey);
            }
        }
    }

    private void RefreshEquipmentDismantleCard(EquipmentDefinition equipment, EquipmentState state, string cardKey, int copyNumber)
    {
        Button cardButton = GetOrCreateEquipmentDismantleCard(equipment, cardKey);
        if (cardButton == null)
        {
            return;
        }

        bool selected = selectedDismantleEquipmentIds.Contains(cardKey);
        cardButton.gameObject.SetActive(true);
        cardButton.transform.SetAsLastSibling();
        SetButtonColor(cardButton, selected ? new Color(0.54f, 0.45f, 0.16f, 1f) : GetRarityColor(equipment.Rarity));

        if (equipmentDismantleCardTexts.TryGetValue(cardKey, out Text cardText))
        {
            cardText.text = BuildEquipmentDismantleCardText(state, copyNumber);
        }
    }

    private Button GetOrCreateEquipmentDismantleCard(EquipmentDefinition equipment, string cardKey)
    {
        if (equipmentDismantleCardButtons.TryGetValue(cardKey, out Button existingButton))
        {
            return existingButton;
        }

        if (equipmentDismantleGridTransform == null)
        {
            return null;
        }

        Button card = CreateButton(equipment.DisplayName, equipmentDismantleGridTransform, 16, GetRarityColor(equipment.Rarity));
        string equipmentCopyKey = cardKey;
        card.onClick.AddListener(() => SelectDismantleEquipment(equipmentCopyKey));
        Text cardText = card.GetComponentInChildren<Text>(true);
        if (cardText != null)
        {
            cardText.alignment = TextAnchor.MiddleCenter;
            cardText.fontSize = 16;
            equipmentDismantleCardTexts[cardKey] = cardText;
        }

        equipmentDismantleCardButtons[cardKey] = card;
        return card;
    }

    private void RefreshEquipmentDismantlePopup()
    {
        if (equipmentDismantlePopup == null)
        {
            return;
        }

        if (!equipmentDismantlePopupOpen)
        {
            equipmentDismantlePopup.SetActive(false);
            return;
        }

        PruneInvalidDismantleSelections();

        HideEquipmentCards(equipmentDismantleCardButtons);
        int visibleCount = 0;
        int selectedReward = 0;
        int selectedCount = 0;
        var sortedEquipment = new List<EquipmentDefinition>(GameData.Equipments);
        sortedEquipment.Sort(CompareEquipmentForDismantleList);
        foreach (EquipmentDefinition equipment in sortedEquipment)
        {
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipment.Id) : null;
            bool owned = state != null && state.IsOwned;
            int availableCount = equipmentInventory != null ? equipmentInventory.GetAvailableCount(equipment.Id) : 0;
            bool visible = owned && availableCount > 0 && heroDetailEquipmentSelectedSlots.Contains(equipment.Slot);
            if (!visible)
            {
                continue;
            }

            for (int i = 0; i < availableCount; i++)
            {
                string cardKey = BuildEquipmentCopyKey(equipment.Id, i);
                RefreshEquipmentDismantleCard(equipment, state, cardKey, i + 1);
                visibleCount += 1;

                if (selectedDismantleEquipmentIds.Contains(cardKey))
                {
                    selectedCount += 1;
                    selectedReward += equipmentInventory.GetDismantleReward(state, 1);
                }
            }
        }

        if (equipmentDismantleSummaryText != null)
        {
            string selectedLabel = selectedCount <= 0
                ? string.Empty
                : "    선택 " + selectedCount + "개 / 보상 +" + FormatShortNumber(selectedReward);
            equipmentDismantleSummaryText.text = "필터 " + BuildEquipmentFilterSummaryLabel()
                + "    표시 " + visibleCount
                + selectedLabel;
        }

        if (equipmentDismantleButton != null)
        {
            bool hasSelection = selectedCount > 0;
            SetButtonText(equipmentDismantleButton, hasSelection
                ? "선택 분해\n" + selectedCount + "개 +" + FormatShortNumber(selectedReward)
                : "선택 분해");
            SetButtonColor(equipmentDismantleButton, hasSelection ? new Color(0.54f, 0.76f, 0.96f, 1f) : new Color(0.35f, 0.38f, 0.44f, 1f));
        }

        if (equipmentDismantleEmptyText != null)
        {
            equipmentDismantleEmptyText.gameObject.SetActive(visibleCount <= 0);
        }

        if (equipmentDismantleNoticeText != null)
        {
            equipmentDismantleNoticeText.text = Time.unscaledTime < growthNoticeUntil ? growthNoticeMessage : string.Empty;
        }

        equipmentDismantlePopup.SetActive(true);
    }

    private void RefreshEquipmentBulkDismantlePrompt()
    {
        if (equipmentBulkDismantlePrompt == null)
        {
            return;
        }

        if (!equipmentBulkDismantlePromptOpen || !equipmentDismantlePopupOpen)
        {
            equipmentBulkDismantlePrompt.SetActive(false);
            return;
        }

        int count = CountBulkDismantleCandidates(selectedBulkDismantleRarity, out int reward);
        if (equipmentBulkDismantleInfoText != null)
        {
            equipmentBulkDismantleInfoText.text = "해당 등급 이하의 전체 장비를 일괄 분해합니다."
                + "\n장착 중인 장비는 분해되지 않습니다."
                + "\n대상 " + count + "개 / 장비책 +" + FormatShortNumber(reward);
        }

        if (equipmentBulkDismantleRarityText != null)
        {
            equipmentBulkDismantleRarityText.text = GetRarityLabel(selectedBulkDismantleRarity);
            equipmentBulkDismantleRarityText.color = Color.Lerp(GetRarityColor(selectedBulkDismantleRarity), Color.white, 0.16f);
        }

        if (equipmentBulkDismantleNoticeText != null)
        {
            equipmentBulkDismantleNoticeText.text = Time.unscaledTime < growthNoticeUntil ? growthNoticeMessage : string.Empty;
        }

        equipmentBulkDismantlePrompt.SetActive(true);
    }

    private string BuildEquipmentCardText(EquipmentDefinition equipment, EquipmentState state, bool equippedToHero, int copyNumber)
    {
        int level = state != null ? state.Level : 1;
        int maxLevel = state != null ? state.MaxLevel : equipment.GetMaxLevel(0);
        int stars = state != null ? state.Stars : 0;
        int attack = state != null ? state.AttackBonus : equipment.GetAttackBonus(1, 0);
        int hp = state != null ? state.HpBonus : equipment.GetHpBonus(1, 0);
        string status = equippedToHero ? "<color=#FFD84D>장착중</color>" : equipment.SlotLabel;
        string copyLabel = equippedToHero ? "장착본" : "개별 #" + copyNumber;

        return status
            + "\n" + equipment.RarityLabel + " " + equipment.DisplayName
            + "\nLv." + level + "/" + maxLevel + "  " + stars + "성"
            + "\nATK+" + attack + " HP+" + hp
            + "\n" + copyLabel;
    }

    private string BuildEquipmentDismantleCardText(EquipmentState state, int copyNumber)
    {
        int reward = equipmentInventory != null ? equipmentInventory.GetDismantleReward(state, 1) : 0;
        return "Lv." + state.Level
            + "\n" + state.Definition.RarityLabel + " " + state.Definition.SlotLabel
            + "\n" + state.Definition.DisplayName
            + "\n분해 #" + copyNumber
            + "\n+" + FormatShortNumber(reward);
    }

    private int CountBulkDismantleCandidates(HeroRarity maxRarity, out int reward)
    {
        reward = 0;
        if (equipmentInventory == null)
        {
            return 0;
        }

        int count = 0;
        foreach (EquipmentState state in equipmentInventory.States)
        {
            if (state == null
                || !state.IsOwned
                || state.Definition.Rarity > maxRarity)
            {
                continue;
            }

            int availableCount = equipmentInventory.GetAvailableCount(state.Definition.Id);
            if (availableCount <= 0)
            {
                continue;
            }

            count += availableCount;
            reward += equipmentInventory.GetDismantleReward(state, availableCount);
        }

        return count;
    }

    private int CompareEquipmentForDismantleList(EquipmentDefinition left, EquipmentDefinition right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int rarityCompare = ((int)right.Rarity).CompareTo((int)left.Rarity);
        if (rarityCompare != 0)
        {
            return rarityCompare;
        }

        int slotCompare = ((int)left.Slot).CompareTo((int)right.Slot);
        if (slotCompare != 0)
        {
            return slotCompare;
        }

        return KoreanNameComparer.Compare(left.DisplayName, right.DisplayName);
    }

    private string BuildEquipmentDetailEffectText(EquipmentState state)
    {
        string slotEffect;
        switch (state.Definition.Slot)
        {
            case EquipmentSlot.Weapon:
                slotEffect = "공격력 +" + FormatShortNumber(state.AttackBonus);
                break;
            case EquipmentSlot.Hat:
                slotEffect = "체력 +" + FormatShortNumber(state.HpBonus);
                break;
            case EquipmentSlot.Armor:
                slotEffect = "받는 피해 감소 +" + (1 + state.Stars) + "%";
                break;
            case EquipmentSlot.Accessory:
                slotEffect = "치명타 데미지 +" + (5 + state.Stars * 2) + "%";
                break;
            case EquipmentSlot.Potion:
                slotEffect = "전투 회복력 +" + (3 + state.Stars) + "%";
                break;
            default:
                slotEffect = "기본 능력 강화";
                break;
        }

        return "<color=#80FF5C>" + state.Definition.RarityLabel + " 세트</color>"
            + "\n" + state.Definition.DisplayName
            + "\n" + state.Definition.SlotLabel + " 숙련"
            + "\n\n<color=#80FF5C>3세트 효과</color>"
            + "\n최종 데미지 증가 +" + (10 + state.Stars * 2) + "%"
            + "\n\n<color=#80FF5C>5세트 효과</color>"
            + "\n" + slotEffect
            + "\n레벨 " + state.Level + " 기준 능력치 적용";
    }

    private double GetHeroDetailCombatPower(HeroState hero)
    {
        var heroes = new List<HeroState> { hero };
        return abilityManager.GetTotalCombatPower(heroes);
    }

    private string GetHeroSkillName(HeroState hero)
    {
        switch (hero.Definition.Trait)
        {
            case HeroTrait.Ranged:
                return "화살비";
            case HeroTrait.Melee:
                return "연속 베기";
            case HeroTrait.Support:
                return "축복의 빛";
            case HeroTrait.Defense:
                return "수호 강타";
            default:
                return "기본 공격";
        }
    }

    private string GetHeroSkillDescription(HeroState hero)
    {
        switch (hero.Definition.Trait)
        {
            case HeroTrait.Ranged:
                return "하늘을 향해 투사체를 발사하여 공격력의 42% 피해를 4회 입힌다.";
            case HeroTrait.Melee:
                return "가까운 적에게 파고들어 공격력의 180% 피해를 입힌다.";
            case HeroTrait.Support:
                return "전장의 아군을 지원해 5초간 파티 공격력을 12% 높인다.";
            case HeroTrait.Defense:
                return "방패로 적을 밀어내 공격력의 90%와 체력의 8%만큼 피해를 입힌다.";
            default:
                return "현재 타깃에게 피해를 입힌다.";
        }
    }

    private string GetStarEffectLine(HeroState hero, int requiredStars, string effectText)
    {
        bool unlocked = hero.Stars >= requiredStars;
        string state = unlocked ? "<color=#90FF58>해금</color>" : "<color=#7C8495>잠김</color>";
        return requiredStars + "성 " + state + "  " + effectText;
    }

    private void LoadHeroFormationDraftFromPreset(int preset)
    {
        selectedHeroPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        selectedHeroForPlacement = string.Empty;
        editingFormationHeroIds.Clear();

        IReadOnlyList<string> savedIds = battleManager.GetHeroFormationHeroIds(selectedHeroPreset);
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            editingFormationHeroIds.Add(i < savedIds.Count ? savedIds[i] : string.Empty);
        }

        heroFormationDirty = false;
    }

    private void EnsureHeroFormationDraft()
    {
        if (editingFormationHeroIds.Count == GameData.MaxPartyHeroes)
        {
            return;
        }

        LoadHeroFormationDraftFromPreset(battleManager.ActiveHeroPreset);
    }

    private bool HasHeroFormationPendingChanges()
    {
        EnsureHeroFormationDraft();

        IReadOnlyList<string> savedIds = battleManager.GetHeroFormationHeroIds(selectedHeroPreset);
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            string editingHeroId = i < editingFormationHeroIds.Count ? editingFormationHeroIds[i] : string.Empty;
            string savedHeroId = i < savedIds.Count ? savedIds[i] : string.Empty;
            if (editingHeroId != savedHeroId)
            {
                return true;
            }
        }

        if (heroFormationDirty)
        {
            return true;
        }

        return selectedHeroPreset != battleManager.ActiveHeroPreset && GetEditingFormationFilledCount() > 0;
    }

    private void RequestHeroPresetChange(int preset)
    {
        int nextPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
        if (nextPreset == selectedHeroPreset)
        {
            return;
        }

        if (heroFormationDirty)
        {
            ShowHeroFormationSavePromptForPreset(nextPreset);
            return;
        }

        LoadHeroFormationDraftFromPreset(nextPreset);
        UpdateView();
    }

    private void RefreshHeroFormationPanel()
    {
        EnsureHeroFormationDraft();
        bool formationOpen = activeHeroPageTab == HeroPageTab.Formation;
        bool traitOpen = activeHeroPageTab == HeroPageTab.Trait;
        bool totemOpen = activeHeroPageTab == HeroPageTab.Statue;
        bool runeOpen = activeHeroPageTab == HeroPageTab.Seal;
        if (heroFormationContent != null)
        {
            heroFormationContent.SetActive(formationOpen);
        }

        if (heroTraitContent != null)
        {
            heroTraitContent.SetActive(traitOpen);
        }

        if (heroTotemContent != null)
        {
            heroTotemContent.SetActive(totemOpen);
        }

        if (heroRuneContent != null)
        {
            heroRuneContent.SetActive(runeOpen);
        }

        if (heroPlaceholderText != null)
        {
            bool placeholderOpen = !formationOpen && !traitOpen && !totemOpen && !runeOpen;
            heroPlaceholderText.gameObject.SetActive(placeholderOpen);
            if (placeholderOpen)
            {
                heroPlaceholderText.text = GetHeroPageTabLabel(activeHeroPageTab) + " 준비 중";
            }
        }

        foreach (KeyValuePair<HeroPageTab, Button> pair in heroPageTabButtons)
        {
            SetButtonColor(pair.Value, pair.Key == activeHeroPageTab
                ? new Color(0.42f, 0.54f, 0.82f, 1f)
                : new Color(0.18f, 0.24f, 0.38f, 1f));
        }

        foreach (KeyValuePair<int, Button> pair in heroPresetButtons)
        {
            SetButtonColor(pair.Value, pair.Key == selectedHeroPreset
                ? new Color(0.50f, 0.64f, 0.96f, 1f)
                : new Color(0.21f, 0.29f, 0.45f, 1f));
        }

        int deployedCount = Mathf.Min(GameData.MaxPartyHeroes, GetEditingFormationFilledCount());
        if (heroFormationSummaryText != null)
        {
            heroFormationSummaryText.text = "출전 " + deployedCount + "/" + GameData.MaxPartyHeroes
                + "    프리셋 " + selectedHeroPreset
                + (HasHeroFormationPendingChanges() ? "    저장 필요" : string.Empty)
                + (string.IsNullOrEmpty(selectedHeroForPlacement)
                    ? string.Empty
                    : "    배치 선택: " + GameData.GetHero(selectedHeroForPlacement).DisplayName);
        }

        for (int i = 0; i < heroFormationSlotTexts.Count; i++)
        {
            Text slotText = heroFormationSlotTexts[i];
            string heroId = i < editingFormationHeroIds.Count ? editingFormationHeroIds[i] : string.Empty;
            HeroState hero = FindHeroState(heroId);
            bool occupied = hero != null;
            if (heroFormationSlotButtons.TryGetValue(i, out Button slotButton))
            {
                SetButtonColor(slotButton, occupied ? GetRarityColor(hero.Definition.Rarity) : new Color(0.18f, 0.22f, 0.31f, 1f));
            }

            if (heroFormationSlotRemoveButtons.TryGetValue(i, out Button removeButton))
            {
                removeButton.gameObject.SetActive(occupied);
            }

            if (occupied)
            {
                slotText.text = GetTraitBadge(hero.Definition.Trait)
                    + " " + GetRarityBadge(hero.Definition.Rarity)
                    + " Lv." + hero.Level
                    + "\n" + GetShortHeroLabel(hero.Definition) + "  " + FormatStars(hero.Stars)
                    + "\n공 " + FormatShortNumber(hero.AttackPower) + " 체 " + FormatShortNumber(hero.MaxHp);
                slotText.color = Color.white;
            }
            else
            {
                slotText.text = "빈 슬롯";
                slotText.color = new Color(0.64f, 0.70f, 0.78f, 1f);
            }
        }

        if (heroOwnedEffectText != null)
        {
            heroOwnedEffectText.text = "보유 효과 : 공격력+" + battleManager.HeroOwnedAttackBonusPercent.ToString("0.##") + "%";
        }

        RefreshFormationTotemSlot();

        if (traitOpen)
        {
            RefreshHeroTraitPanel();
        }

        if (totemOpen)
        {
            RefreshHeroTotemPanel();
        }

        if (runeOpen)
        {
            RefreshHeroRunePanel();
        }
    }

    private void RefreshFormationTotemSlot()
    {
        if (battleManager == null)
        {
            return;
        }

        RefreshFormationTotemSlot(1, heroFormationTotemButton, heroFormationTotemText);
        RefreshFormationTotemSlot(2, heroFormationTotemSecondButton, heroFormationTotemSecondText);
    }

    private void RefreshFormationTotemSlot(int slot, Button button, Text text)
    {
        if (text == null || battleManager == null)
        {
            return;
        }

        if (!battleManager.IsTotemSlotUnlocked(slot))
        {
            text.text = "잠김\n토템 " + slot;
            if (button != null)
            {
                SetButtonColor(button, new Color(0.18f, 0.20f, 0.26f, 1f));
            }

            return;
        }

        TotemState pendingState = battleManager.GetTotemState(pendingTotemEquipId);
        string equippedTotemId = battleManager.GetEquippedTotemId(selectedHeroPreset, slot);
        TotemState state = battleManager.GetTotemState(equippedTotemId);
        if (state == null)
        {
            text.text = pendingState != null
                ? "+\n" + pendingState.DisplayName + "\n장착"
                : "+\n토템 " + slot;
            if (button != null)
            {
                SetButtonColor(button, pendingState != null ? Color.Lerp(GetTotemColor(pendingState.Definition), Color.white, 0.22f) : new Color(0.20f, 0.28f, 0.42f, 1f));
            }

            return;
        }

        text.text = state.Definition.Icon
            + "\n" + state.DisplayName
            + "\n" + state.GradeLabel + " Lv." + state.Level + "/" + TotemDefinition.MaxLevel
            + (pendingState != null && state.Definition.Id != pendingState.Definition.Id ? "\n교체 가능" : string.Empty);

        if (button != null)
        {
            SetButtonColor(button, GetTotemColor(state.Definition));
        }
    }

    private void RefreshHeroTotemPanel()
    {
        if (battleManager == null || wallet == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(selectedTotemId))
        {
            selectedTotemId = battleManager.GetEquippedTotemId(selectedHeroPreset, selectedTotemSlot);
        }

        string equippedTotemId = battleManager.GetEquippedTotemId(selectedHeroPreset, selectedTotemSlot);
        TotemState selectedState = battleManager.GetTotemState(selectedTotemId) ?? battleManager.GetTotemState(equippedTotemId);
        if (selectedState == null && GameData.Totems.Count > 0)
        {
            selectedState = battleManager.GetTotemState(GameData.Totems[0].Id);
        }

        if (selectedState != null)
        {
            selectedTotemId = selectedState.Definition.Id;
        }

        if (heroTotemSummaryText != null)
        {
            string slotOneName = GetEquippedTotemLabel(1);
            string slotTwoName = battleManager.IsTotemSlotUnlocked(2) ? GetEquippedTotemLabel(2) : "2번 슬롯 잠김";
            heroTotemSummaryText.text = "프리셋 " + selectedHeroPreset
                + "  선택 슬롯 " + selectedTotemSlot
                + "  정수 " + FormatCountNumber(wallet.TotemEssence)
                + "\n1번 " + slotOneName + " / 2번 " + slotTwoName;
        }

        foreach (TotemDefinition totem in GameData.Totems)
        {
            TotemState state = battleManager.GetTotemState(totem.Id);
            bool selected = selectedState != null && selectedState.Definition.Id == totem.Id;
            bool equippedInSelectedSlot = equippedTotemId == totem.Id;
            bool equippedInAnySlot = IsTotemEquippedInAnySlot(totem.Id);
            bool unlocked = state != null && state.Unlocked;

            if (heroTotemButtonTexts.TryGetValue(totem.Id, out Text text) && text != null)
            {
                text.text = totem.Icon
                    + "\n" + GetTotemCategoryLabel(totem.Archetype)
                    + "\n" + (state != null ? state.GradeLabel : TotemDefinition.GetGradeLabel(TotemGrade.Common))
                    + " Lv." + (state != null ? state.Level : 1)
                    + (equippedInSelectedSlot ? "\n선택 슬롯" : equippedInAnySlot ? "\n장착중" : string.Empty)
                    + (unlocked ? string.Empty : "\n미보유");
            }

            if (heroTotemButtons.TryGetValue(totem.Id, out Button button) && button != null)
            {
                Color baseColor = unlocked ? GetTotemColor(totem) : new Color(0.20f, 0.22f, 0.26f, 1f);
                SetButtonColor(button, selected ? Color.Lerp(baseColor, Color.white, 0.35f) : baseColor);
            }
        }

        if (heroTotemDetailText != null && selectedState != null)
        {
            bool isBoss = progressManager != null && progressManager.CurrentStage.Type == StageType.Boss;
            heroTotemDetailText.text = selectedState.Definition.Icon + " " + selectedState.DisplayName
                + "  " + selectedState.GradeLabel + " Lv." + selectedState.Level + "/" + TotemDefinition.MaxLevel
                + "\n효과: " + selectedState.Definition.Role
                + "\n" + selectedState.Definition.GetEffectSummary(selectedState.Level, selectedState.Grade, battleManager.DeployedHeroes, isBoss);
        }

        if (heroTotemEquipButton != null && selectedState != null)
        {
            int equippedSlot = GetEquippedTotemSlot(selectedState.Definition.Id);
            bool equipped = equippedSlot > 0;
            heroTotemEquipButton.interactable = selectedState.Unlocked;
            SetButtonText(heroTotemEquipButton, equipped ? "장착 해제" : "장착");
            SetButtonColor(heroTotemEquipButton, equipped
                ? new Color(0.42f, 0.54f, 0.82f, 1f)
                : selectedState.Unlocked ? new Color(0.54f, 0.76f, 0.96f, 1f) : new Color(0.35f, 0.36f, 0.38f, 1f));
        }

        if (heroTotemLevelUpButton != null && selectedState != null)
        {
            bool canLevel = selectedState.Unlocked && !selectedState.IsMaxed && wallet.TotemEssence >= selectedState.LevelUpCost;
            bool canPromote = selectedState.Unlocked && selectedState.CanPromote && wallet.TotemEssence >= selectedState.PromoteCost;
            heroTotemLevelUpButton.interactable = selectedState.Unlocked;
            SetButtonText(heroTotemLevelUpButton, selectedState.CanPromote
                ? "진화\n" + FormatCountNumber(selectedState.PromoteCost)
                : selectedState.IsMaxed ? "MAX" : "강화\n" + FormatCountNumber(selectedState.LevelUpCost));
            SetButtonColor(heroTotemLevelUpButton, selectedState.CanPromote
                ? canPromote ? new Color(0.92f, 0.58f, 0.18f, 1f) : new Color(0.36f, 0.30f, 0.22f, 1f)
                : selectedState.IsMaxed ? new Color(0.34f, 0.36f, 0.40f, 1f)
                : canLevel ? new Color(0.54f, 0.78f, 0.22f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f));
        }
    }

    private string GetEquippedTotemLabel(int slot)
    {
        if (battleManager == null || !battleManager.IsTotemSlotUnlocked(slot))
        {
            return "잠김";
        }

        TotemState state = battleManager.GetTotemState(battleManager.GetEquippedTotemId(selectedHeroPreset, slot));
        return state != null ? state.DisplayName : "없음";
    }

    private void RefreshHeroRunePanel()
    {
        if (battleManager == null || wallet == null)
        {
            return;
        }

        selectedRuneSlot = Mathf.Clamp(selectedRuneSlot, 1, GameData.MaxRuneSlots);
        if (string.IsNullOrEmpty(selectedRuneId) && GameData.Runes.Count > 0)
        {
            selectedRuneId = GameData.Runes[0].Id;
        }

        RuneState selectedState = battleManager.GetRuneState(selectedRuneId);
        if (selectedState == null && GameData.Runes.Count > 0)
        {
            selectedState = battleManager.GetRuneState(GameData.Runes[0].Id);
        }

        if (selectedState != null)
        {
            selectedRuneId = selectedState.Definition.Id;
        }

        if (heroRuneSummaryText != null)
        {
            heroRuneSummaryText.text = "프리셋 " + selectedHeroPreset
                + "  룬 슬롯 " + selectedRuneSlot + "/" + GameData.MaxRuneSlots
                + "  룬 가루 " + FormatCountNumber(wallet.RuneDust);
        }

        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            string equippedRuneId = battleManager.GetEquippedRuneId(selectedHeroPreset, slot);
            RuneState state = battleManager.GetRuneState(equippedRuneId);
            bool selectedSlot = slot == selectedRuneSlot;
            if (heroRuneSlotTexts.TryGetValue(slot, out Text slotText) && slotText != null)
            {
                slotText.text = state != null
                    ? slot + "\n" + state.Definition.Icon + " " + state.Definition.DisplayName + "\nLv." + state.Level + "/" + RuneDefinition.MaxLevel
                    : slot + "\n+\n빈 룬 슬롯";
                slotText.resizeTextForBestFit = true;
                slotText.resizeTextMinSize = 10;
                slotText.resizeTextMaxSize = 18;
            }

            if (heroRuneSlotButtons.TryGetValue(slot, out Button slotButton) && slotButton != null)
            {
                Color baseColor = state != null ? GetRuneColor(state.Definition) : new Color(0.17f, 0.21f, 0.31f, 1f);
                SetButtonColor(slotButton, selectedSlot ? Color.Lerp(baseColor, Color.white, 0.28f) : baseColor);
            }
        }

        foreach (RuneDefinition rune in GameData.Runes)
        {
            RuneState state = battleManager.GetRuneState(rune.Id);
            bool selected = selectedState != null && selectedState.Definition.Id == rune.Id;
            bool equipped = IsRuneEquippedInAnySlot(rune.Id);
            bool unlocked = state != null && state.Unlocked;

            if (heroRuneButtonTexts.TryGetValue(rune.Id, out Text text) && text != null)
            {
                text.text = rune.Icon
                    + "\n" + rune.DisplayName
                    + "\nLv." + (state != null ? state.Level : 1)
                    + (equipped ? "\n장착중" : string.Empty)
                    + (unlocked ? string.Empty : "\n미보유");
            }

            if (heroRuneButtons.TryGetValue(rune.Id, out Button button) && button != null)
            {
                Color baseColor = unlocked ? GetRuneColor(rune) : new Color(0.20f, 0.22f, 0.26f, 1f);
                SetButtonColor(button, selected ? Color.Lerp(baseColor, Color.white, 0.35f) : baseColor);
            }
        }

        if (heroRuneDetailText != null && selectedState != null)
        {
            heroRuneDetailText.text = selectedState.Definition.Icon + " " + selectedState.Definition.DisplayName
                + "  Lv." + selectedState.Level + "/" + RuneDefinition.MaxLevel
                + "\n" + selectedState.Definition.Role
                + "\n" + selectedState.Definition.GetEffectSummary(selectedState.Level);
        }

        if (heroRuneEquipButton != null && selectedState != null)
        {
            int equippedSlot = GetEquippedRuneSlot(selectedState.Definition.Id);
            bool equipped = equippedSlot > 0;
            SetButtonText(heroRuneEquipButton, equipped ? "장착 해제" : selectedRuneSlot + "번 슬롯 장착");
            SetButtonColor(heroRuneEquipButton, equipped
                ? new Color(0.42f, 0.54f, 0.82f, 1f)
                : new Color(0.54f, 0.76f, 0.96f, 1f));
        }

        if (heroRuneLevelUpButton != null && selectedState != null)
        {
            bool canLevel = selectedState.Unlocked && !selectedState.IsMaxed && wallet.RuneDust >= selectedState.LevelUpCost;
            SetButtonText(heroRuneLevelUpButton, selectedState.IsMaxed ? "MAX" : "강화\n" + FormatCountNumber(selectedState.LevelUpCost));
            SetButtonColor(heroRuneLevelUpButton, selectedState.IsMaxed
                ? new Color(0.34f, 0.36f, 0.40f, 1f)
                : canLevel ? new Color(0.54f, 0.78f, 0.22f, 1f) : new Color(0.35f, 0.36f, 0.34f, 1f));
        }
    }

    private void SelectRune(string runeId)
    {
        selectedRuneId = runeId;
        RefreshHeroRunePanel();
    }

    private void EquipSelectedRune()
    {
        if (battleManager == null || string.IsNullOrEmpty(selectedRuneId))
        {
            return;
        }

        int equippedSlot = GetEquippedRuneSlot(selectedRuneId);
        if (equippedSlot > 0)
        {
            if (battleManager.ClearRuneForPreset(selectedHeroPreset, equippedSlot))
            {
                ShowGrowthNotice(equippedSlot + "번 룬을 해제했습니다.");
            }

            UpdateView();
            return;
        }

        if (battleManager.SetRuneForPreset(selectedHeroPreset, selectedRuneSlot, selectedRuneId))
        {
            RuneState state = battleManager.GetRuneState(selectedRuneId);
            ShowGrowthNotice((state != null ? state.Definition.DisplayName : "룬") + "을 " + selectedRuneSlot + "번 슬롯에 장착했습니다.");
        }
        else
        {
            ShowGrowthNotice("룬을 장착할 수 없습니다.");
        }

        UpdateView();
    }

    private void LevelUpSelectedRune()
    {
        if (battleManager == null || string.IsNullOrEmpty(selectedRuneId))
        {
            return;
        }

        RuneState state = battleManager.GetRuneState(selectedRuneId);
        if (state == null)
        {
            return;
        }

        if (state.IsMaxed)
        {
            ShowGrowthNotice("이미 최대 레벨입니다.");
            return;
        }

        if (wallet == null || wallet.RuneDust < state.LevelUpCost)
        {
            ShowGrowthNotice("룬 가루가 부족합니다.");
            return;
        }

        battleManager.TryLevelUpRune(selectedRuneId);
        UpdateView();
    }

    private bool CanLevelUpSelectedRune()
    {
        if (battleManager == null || wallet == null || string.IsNullOrEmpty(selectedRuneId))
        {
            return false;
        }

        RuneState state = battleManager.GetRuneState(selectedRuneId);
        return state != null && state.Unlocked && !state.IsMaxed && wallet.RuneDust >= state.LevelUpCost;
    }

    private bool IsRuneEquippedInAnySlot(string runeId)
    {
        return GetEquippedRuneSlot(runeId) > 0;
    }

    private int GetEquippedRuneSlot(string runeId)
    {
        if (battleManager == null || string.IsNullOrEmpty(runeId))
        {
            return 0;
        }

        for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
        {
            if (battleManager.GetEquippedRuneId(selectedHeroPreset, slot) == runeId)
            {
                return slot;
            }
        }

        return 0;
    }

    private bool IsTotemEquippedInAnySlot(string totemId)
    {
        return GetEquippedTotemSlot(totemId) > 0;
    }

    private int GetEquippedTotemSlot(string totemId)
    {
        if (battleManager == null || string.IsNullOrEmpty(totemId))
        {
            return 0;
        }

        for (int slot = 1; slot <= GameData.MaxTotemSlots; slot++)
        {
            if (battleManager.IsTotemSlotUnlocked(slot) && battleManager.GetEquippedTotemId(selectedHeroPreset, slot) == totemId)
            {
                return slot;
            }
        }

        return 0;
    }

    private void RefreshHeroTraitPanel()
    {
        if (accountProgressManager == null)
        {
            if (heroTraitSummaryText != null)
            {
                heroTraitSummaryText.text = "계정 성장 데이터를 불러오는 중";
            }

            return;
        }

        if (heroTraitSummaryText != null)
        {
            heroTraitSummaryText.text = "계정 Lv." + accountProgressManager.Level
                + "  EXP " + FormatShortNumber(accountProgressManager.Experience)
                + "/" + FormatShortNumber(accountProgressManager.NextLevelExperience)
                + "  특성 포인트 " + accountProgressManager.AvailableTalentPoints
                + "/" + accountProgressManager.TotalTalentPointsEarned;
        }

        foreach (TalentDefinition talent in TalentData.Talents)
        {
            int level = accountProgressManager.GetTalentLevel(talent.Id);
            bool unlocked = accountProgressManager.IsTalentUnlocked(talent);
            bool maxed = level >= talent.MaxLevel;
            bool selected = talent.Id == selectedHeroTraitId;

            if (heroTraitButtonTexts.TryGetValue(talent.Id, out Text text) && text != null)
            {
                text.text = talent.Icon
                    + "\n" + talent.DisplayName
                    + "\n" + (maxed ? "MAX" : level + "/" + talent.MaxLevel)
                    + (unlocked ? string.Empty : "\n잠김");
            }

            if (heroTraitButtons.TryGetValue(talent.Id, out Button button) && button != null)
            {
                SetButtonColor(button, GetHeroTraitNodeColor(unlocked, maxed, selected));
            }
        }

        TalentDefinition selectedTalent = TalentData.GetTalent(selectedHeroTraitId);
        int selectedLevel = accountProgressManager.GetTalentLevel(selectedTalent.Id);
        bool selectedUnlocked = accountProgressManager.IsTalentUnlocked(selectedTalent);
        bool selectedMaxed = selectedLevel >= selectedTalent.MaxLevel;

        if (heroTraitDetailText != null)
        {
            heroTraitDetailText.text = selectedTalent.Icon + " " + selectedTalent.DisplayName
                + " [" + selectedTalent.BranchName + "]"
                + "\n현재: " + selectedTalent.FormatValue(selectedLevel)
                + (selectedMaxed ? "\n다음: MAX" : "\n다음: " + selectedTalent.FormatValue(selectedLevel + 1))
                + "\nLv." + selectedLevel + "/" + selectedTalent.MaxLevel
                + (!selectedUnlocked ? BuildHeroTraitUnlockConditionText(selectedTalent) : string.Empty);
        }

        if (heroTraitLevelUpButton != null)
        {
            bool canLevel = selectedUnlocked
                && !selectedMaxed
                && accountProgressManager.AvailableTalentPoints >= selectedTalent.CostPerLevel;
            SetButtonText(heroTraitLevelUpButton, selectedMaxed ? "MAX" : "레벨업\n" + selectedTalent.CostPerLevel + "P");
            SetButtonColor(heroTraitLevelUpButton, canLevel
                ? new Color(0.54f, 0.78f, 0.22f, 1f)
                : new Color(0.34f, 0.36f, 0.40f, 1f));
        }
    }

    private Color GetHeroTraitNodeColor(bool unlocked, bool maxed, bool selected)
    {
        if (selected)
        {
            return new Color(0.34f, 0.85f, 0.86f, 1f);
        }

        if (!unlocked)
        {
            return new Color(0.24f, 0.27f, 0.32f, 1f);
        }

        return maxed
            ? new Color(0.88f, 0.63f, 0.16f, 1f)
            : new Color(0.22f, 0.48f, 0.58f, 1f);
    }

    private string BuildHeroTraitUnlockConditionText(TalentDefinition talent)
    {
        IReadOnlyList<TalentDefinition> prerequisites = TalentData.GetPrerequisiteTalents(talent);
        if (prerequisites.Count == 0)
        {
            return string.Empty;
        }

        string label = prerequisites[0].DisplayName;
        for (int i = 1; i < prerequisites.Count; i++)
        {
            label += " / " + prerequisites[i].DisplayName;
        }

        return "\n해금 조건: 연결된 이전 특성 MAX (" + label + ")";
    }

    private void SelectTotem(string totemId)
    {
        selectedTotemId = totemId;
        pendingTotemEquipId = string.Empty;
        RefreshHeroTotemPanel();
    }

    private void EquipSelectedTotem()
    {
        if (battleManager == null || string.IsNullOrEmpty(selectedTotemId))
        {
            return;
        }

        int equippedSlot = GetEquippedTotemSlot(selectedTotemId);
        if (equippedSlot > 0)
        {
            if (battleManager.ClearTotemForPreset(selectedHeroPreset, equippedSlot))
            {
                pendingTotemEquipId = string.Empty;
                ShowGrowthNotice(equippedSlot + "번 토템을 해제했습니다.");
            }
            else
            {
                ShowGrowthNotice("토템을 해제할 수 없습니다.");
            }

            UpdateView();
            return;
        }

        pendingTotemEquipId = selectedTotemId;
        activeTab = HudTab.Hero;
        contentPanelOpen = true;
        activeHeroPageTab = HeroPageTab.Formation;
        TotemState pendingState = battleManager.GetTotemState(pendingTotemEquipId);
        ShowGrowthNotice((pendingState != null ? pendingState.DisplayName : "토템") + "을 장착할 슬롯을 선택하세요.");
        UpdateView();
    }

    private void HandleFormationTotemSlotClick(int slot)
    {
        selectedTotemSlot = slot;
        if (!string.IsNullOrEmpty(pendingTotemEquipId))
        {
            TryEquipPendingTotemInSlot(slot);
            return;
        }

        if (battleManager != null && !battleManager.IsTotemSlotUnlocked(selectedTotemSlot))
        {
            ShowGrowthNotice(selectedTotemSlot + "번 토템 슬롯은 추후 해금됩니다.");
            UpdateView();
            return;
        }

        selectedTotemId = battleManager != null ? battleManager.GetEquippedTotemId(selectedHeroPreset, selectedTotemSlot) : selectedTotemId;
        activeHeroPageTab = HeroPageTab.Statue;
        UpdateView();
    }

    private void TryEquipPendingTotemInSlot(int slot)
    {
        if (battleManager == null || string.IsNullOrEmpty(pendingTotemEquipId))
        {
            return;
        }

        if (!battleManager.IsTotemSlotUnlocked(slot))
        {
            ShowGrowthNotice(slot + "번 토템 슬롯은 아직 잠겨 있습니다.");
            UpdateView();
            return;
        }

        selectedTotemSlot = slot;
        selectedTotemId = pendingTotemEquipId;
        if (battleManager.SetTotemForPreset(selectedHeroPreset, slot, pendingTotemEquipId))
        {
            TotemState state = battleManager.GetTotemState(pendingTotemEquipId);
            pendingTotemEquipId = string.Empty;
            ShowGrowthNotice((state != null ? state.DisplayName : "토템") + "을 " + slot + "번 슬롯에 장착했습니다.");
        }
        else
        {
            ShowGrowthNotice("장착할 수 없는 토템입니다.");
        }

        UpdateView();
    }

    private void RefreshPendingTotemSlotGlow()
    {
        if (battleManager == null
            || string.IsNullOrEmpty(pendingTotemEquipId)
            || activeTab != HudTab.Hero
            || activeHeroPageTab != HeroPageTab.Formation)
        {
            return;
        }

        TotemState state = battleManager.GetTotemState(pendingTotemEquipId);
        if (state == null)
        {
            return;
        }

        float glow = 0.35f + Mathf.PingPong(Time.unscaledTime * 2.4f, 1f) * 0.45f;
        Color glowColor = Color.Lerp(GetTotemColor(state.Definition), Color.white, glow);
        if (heroFormationTotemButton != null && battleManager.IsTotemSlotUnlocked(1))
        {
            SetButtonColor(heroFormationTotemButton, glowColor);
        }

        if (heroFormationTotemSecondButton != null && battleManager.IsTotemSlotUnlocked(2))
        {
            SetButtonColor(heroFormationTotemSecondButton, glowColor);
        }
    }

    private void LevelUpSelectedTotem()
    {
        if (battleManager == null || string.IsNullOrEmpty(selectedTotemId))
        {
            return;
        }

        TotemState state = battleManager.GetTotemState(selectedTotemId);
        if (state == null)
        {
            return;
        }

        if (!state.Unlocked)
        {
            ShowGrowthNotice("보유하지 않은 토템입니다.");
            return;
        }

        if (state.IsMaxed)
        {
            if (!state.CanPromote)
            {
                ShowGrowthNotice("이미 최대 등급입니다.");
                return;
            }

            if (wallet == null || wallet.TotemEssence < state.PromoteCost)
            {
                ShowGrowthNotice("토템 정수가 부족합니다.");
                return;
            }

            if (!battleManager.TryPromoteTotem(selectedTotemId))
            {
                ShowGrowthNotice("토템 진화에 실패했습니다.");
                return;
            }

            UpdateView();
            return;
        }

        if (wallet == null || wallet.TotemEssence < state.LevelUpCost)
        {
            ShowGrowthNotice("토템 정수가 부족합니다.");
            return;
        }

        if (!battleManager.TryLevelUpTotem(selectedTotemId))
        {
            ShowGrowthNotice("토템 강화에 실패했습니다.");
            return;
        }

        UpdateView();
    }

    private bool CanLevelUpSelectedTotem()
    {
        if (battleManager == null || wallet == null || string.IsNullOrEmpty(selectedTotemId))
        {
            return false;
        }

        TotemState state = battleManager.GetTotemState(selectedTotemId);
        return state != null
            && state.Unlocked
            && ((!state.IsMaxed && wallet.TotemEssence >= state.LevelUpCost)
                || (state.CanPromote && wallet.TotemEssence >= state.PromoteCost));
    }

    private void LevelUpSelectedHeroTrait()
    {
        if (accountProgressManager == null)
        {
            return;
        }

        TalentDefinition talent = TalentData.GetTalent(selectedHeroTraitId);
        if (!accountProgressManager.IsTalentUnlocked(talent))
        {
            ShowGrowthNotice("선으로 연결된 이전 특성을 MAX 찍어야 합니다.");
            return;
        }

        if (accountProgressManager.GetTalentLevel(talent.Id) >= talent.MaxLevel)
        {
            ShowGrowthNotice("이미 최대 레벨입니다.");
            return;
        }

        if (accountProgressManager.AvailableTalentPoints < talent.CostPerLevel)
        {
            ShowGrowthNotice("특성 포인트가 부족합니다.");
            return;
        }

        if (!accountProgressManager.TryLevelUpTalent(talent.Id))
        {
            ShowGrowthNotice("특성 레벨업에 실패했습니다.");
            return;
        }

        ShowGrowthNotice(talent.DisplayName + " Lv." + accountProgressManager.GetTalentLevel(talent.Id));
        UpdateView();
    }

    private bool CanLevelUpSelectedHeroTrait()
    {
        if (accountProgressManager == null)
        {
            return false;
        }

        TalentDefinition talent = TalentData.GetTalent(selectedHeroTraitId);
        int level = accountProgressManager.GetTalentLevel(talent.Id);
        return accountProgressManager.IsTalentUnlocked(talent)
            && level < talent.MaxLevel
            && accountProgressManager.AvailableTalentPoints >= talent.CostPerLevel;
    }

    private void TryLevelUpAbilityFromHud(AbilityKind kind)
    {
        AbilityState ability = FindAbilityState(kind);
        if (ability == null)
        {
            return;
        }

        int cappedLevels = abilityManager.GetCappedLevelCount(ability, selectedGrowthLevelStep);
        long cost = abilityManager.GetLevelUpCost(ability, cappedLevels);
        if (ability.IsMaxed)
        {
            ShowGrowthNotice("이미 최대 레벨입니다.");
            return;
        }

        if (cappedLevels <= 0 || cost <= 0 || wallet.Gold < cost)
        {
            ShowGrowthNotice("골드가 부족합니다.");
            return;
        }

        if (!abilityManager.TryLevelUp(kind, selectedGrowthLevelStep))
        {
            ShowGrowthNotice("골드가 부족합니다.");
        }
    }

    private bool CanLevelUpAbilityFromHud(AbilityKind kind)
    {
        AbilityState ability = FindAbilityState(kind);
        if (ability == null || ability.IsMaxed || abilityManager == null || wallet == null)
        {
            return false;
        }

        int cappedLevels = abilityManager.GetCappedLevelCount(ability, selectedGrowthLevelStep);
        long cost = abilityManager.GetLevelUpCost(ability, cappedLevels);
        return cappedLevels > 0 && cost > 0 && wallet.Gold >= cost;
    }

    private bool IsHeroInEditingFormation(string heroId)
    {
        EnsureHeroFormationDraft();
        foreach (string editingHeroId in editingFormationHeroIds)
        {
            if (editingHeroId == heroId)
            {
                return true;
            }
        }

        return false;
    }

    private int GetEditingFormationHeroIndex(string heroId)
    {
        EnsureHeroFormationDraft();
        for (int i = 0; i < editingFormationHeroIds.Count; i++)
        {
            if (editingFormationHeroIds[i] == heroId)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetEditingFormationFilledCount()
    {
        EnsureHeroFormationDraft();
        int count = 0;
        foreach (string heroId in editingFormationHeroIds)
        {
            if (!string.IsNullOrEmpty(heroId))
            {
                count += 1;
            }
        }

        return count;
    }

    private void AutoArrangeEditingFormation()
    {
        EnsureHeroFormationDraft();
        if (battleManager == null || battleManager.Heroes.Count <= 0)
        {
            return;
        }

        var sortedHeroes = new List<HeroState>();
        foreach (HeroState hero in battleManager.Heroes)
        {
            if (hero != null && hero.IsOwned)
            {
                sortedHeroes.Add(hero);
            }
        }

        sortedHeroes.Sort(CompareHeroesForAutoFormation);

        editingFormationHeroIds.Clear();
        for (int i = 0; i < GameData.MaxPartyHeroes; i++)
        {
            editingFormationHeroIds.Add(i < sortedHeroes.Count ? sortedHeroes[i].Definition.Id : string.Empty);
        }

        selectedHeroForPlacement = string.Empty;
        heroFormationDirty = HasHeroFormationPendingChanges();
        UpdateView();
    }

    private int CompareHeroesForAutoFormation(HeroState left, HeroState right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int powerCompare = GetHeroDetailCombatPower(right).CompareTo(GetHeroDetailCombatPower(left));
        if (powerCompare != 0)
        {
            return powerCompare;
        }

        int starCompare = right.Stars.CompareTo(left.Stars);
        if (starCompare != 0)
        {
            return starCompare;
        }

        int levelCompare = right.Level.CompareTo(left.Level);
        if (levelCompare != 0)
        {
            return levelCompare;
        }

        int rarityCompare = ((int)right.Definition.Rarity).CompareTo((int)left.Definition.Rarity);
        if (rarityCompare != 0)
        {
            return rarityCompare;
        }

        return string.CompareOrdinal(left.Definition.Id, right.Definition.Id);
    }

    private void BulkStarUpHeroesFromHud()
    {
        if (battleManager == null)
        {
            return;
        }

        battleManager.BulkStarUpHeroes();
        UpdateView();
    }

    private void ToggleSelectedHeroDetailFormation()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null)
        {
            return;
        }

        if (!hero.IsOwned)
        {
            ShowGrowthNotice("아직 획득하지 않은 영웅입니다.");
            return;
        }

        EnsureHeroFormationDraft();
        if (IsHeroInEditingFormation(hero.Definition.Id))
        {
            RemoveSelectedHeroDetailFromFormation();
            return;
        }

        selectedHeroForPlacement = hero.Definition.Id;
        heroDetailPanelOpen = false;
        selectedHeroDetailId = string.Empty;
        selectedHeroDetailEquipmentId = string.Empty;
        heroDetailEquipmentSlotSelectionActive = false;
        ShowGrowthNotice("배치할 칸을 선택하세요.");
        UpdateView();
    }

    private void RemoveSelectedHeroDetailFromFormation()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null)
        {
            return;
        }

        EnsureHeroFormationDraft();
        int slotIndex = GetEditingFormationHeroIndex(hero.Definition.Id);
        if (slotIndex < 0)
        {
            ShowGrowthNotice("출전 중인 영웅이 아닙니다.");
            return;
        }

        if (GetEditingFormationFilledCount() <= 1)
        {
            ShowGrowthNotice("최소 1명은 편성해야 합니다.");
            return;
        }

        editingFormationHeroIds[slotIndex] = string.Empty;
        selectedHeroForPlacement = string.Empty;
        heroFormationDirty = true;
        CloseHeroDetailPanel();
    }

    private void LevelUpSelectedHeroDetail()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || battleManager == null)
        {
            return;
        }

        if (!hero.IsOwned)
        {
            ShowGrowthNotice("아직 획득하지 않은 영웅입니다.");
            return;
        }

        if (hero.Level >= hero.MaxLevel)
        {
            ShowGrowthNotice("이미 최대 레벨입니다.");
            return;
        }

        if (wallet == null || wallet.HeroExpItem < hero.LevelUpCost)
        {
            ShowGrowthNotice("경험치책이 부족합니다.");
            return;
        }

        if (!battleManager.TryLevelUpHero(hero.Definition.Id))
        {
            ShowGrowthNotice("레벨업에 실패했습니다.");
            return;
        }

        UpdateView();
    }

    private bool CanLevelUpSelectedHeroDetail()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        return hero != null
            && hero.IsOwned
            && hero.Level < hero.MaxLevel
            && wallet != null
            && wallet.HeroExpItem >= hero.LevelUpCost;
    }

    private void StarUpSelectedHeroDetail()
    {
        HeroState hero = FindHeroState(selectedHeroDetailId);
        if (hero == null || battleManager == null)
        {
            return;
        }

        if (!hero.IsOwned)
        {
            ShowGrowthNotice("아직 획득하지 않은 영웅입니다.");
            return;
        }

        if (hero.IsMaxStars)
        {
            ShowGrowthNotice("이미 최대 성급입니다.");
            return;
        }

        if (!hero.CanStarUp)
        {
            ShowGrowthNotice("영웅 조각이 부족합니다.");
            return;
        }

        if (!battleManager.TryStarUpHero(hero.Definition.Id))
        {
            ShowGrowthNotice("승급에 실패했습니다.");
            return;
        }

        UpdateView();
    }

    private void SelectOrRemoveRosterHero(string heroId)
    {
        HeroState hero = FindHeroState(heroId);
        if (hero == null)
        {
            return;
        }

        if (!hero.IsOwned)
        {
            ShowGrowthNotice("뽑기로 조각을 획득해야 배치할 수 있습니다.");
            return;
        }

        int existingIndex = GetEditingFormationHeroIndex(heroId);
        if (existingIndex >= 0)
        {
            if (selectedHeroForPlacement == heroId)
            {
                selectedHeroForPlacement = string.Empty;
            }

            RemoveHeroFromEditingFormationSlot(existingIndex);
            return;
        }

        selectedHeroForPlacement = selectedHeroForPlacement == heroId ? string.Empty : heroId;
        UpdateView();
    }

    private void RemoveHeroFromEditingFormationSlot(int slotIndex)
    {
        EnsureHeroFormationDraft();
        if (slotIndex < 0 || slotIndex >= editingFormationHeroIds.Count || string.IsNullOrEmpty(editingFormationHeroIds[slotIndex]))
        {
            return;
        }

        if (GetEditingFormationFilledCount() <= 1)
        {
            return;
        }

        editingFormationHeroIds[slotIndex] = string.Empty;
        selectedHeroForPlacement = string.Empty;
        heroFormationDirty = true;
        UpdateView();
    }

    private void TryPlaceSelectedHeroInSlot(int slotIndex)
    {
        EnsureHeroFormationDraft();
        if (slotIndex < 0 || slotIndex >= editingFormationHeroIds.Count)
        {
            return;
        }

        if (string.IsNullOrEmpty(selectedHeroForPlacement))
        {
            OpenHeroDetailPanel(editingFormationHeroIds[slotIndex]);
            return;
        }

        string heroId = selectedHeroForPlacement;
        HeroState hero = FindHeroState(heroId);
        if (hero == null || !hero.IsOwned)
        {
            selectedHeroForPlacement = string.Empty;
            ShowGrowthNotice("배치할 수 없는 영웅입니다.");
            UpdateView();
            return;
        }

        for (int i = 0; i < editingFormationHeroIds.Count; i++)
        {
            if (editingFormationHeroIds[i] == heroId)
            {
                editingFormationHeroIds[i] = string.Empty;
            }
        }

        editingFormationHeroIds[slotIndex] = heroId;
        selectedHeroForPlacement = string.Empty;
        heroFormationDirty = true;
        UpdateView();
    }

    private HeroState FindHeroState(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            return null;
        }

        foreach (HeroState hero in battleManager.Heroes)
        {
            if (hero.Definition.Id == heroId)
            {
                return hero;
            }
        }

        return null;
    }

    private AbilityState FindAbilityState(AbilityKind kind)
    {
        foreach (AbilityState ability in abilityManager.States)
        {
            if (ability.Definition.Kind == kind)
            {
                return ability;
            }
        }

        return null;
    }

    private void ShowGrowthNotice(string message)
    {
        growthNoticeMessage = message;
        growthNoticeUntil = Time.unscaledTime + 1.6f;
        if (growthNoticeText != null)
        {
            growthNoticeText.text = message;
        }

        if (heroDetailNoticeText != null)
        {
            heroDetailNoticeText.text = message;
        }

        if (heroDetailTranscendNoticeText != null)
        {
            heroDetailTranscendNoticeText.text = message;
        }

        if (equipmentDetailNoticeText != null)
        {
            equipmentDetailNoticeText.text = message;
        }

        if (equipmentDismantleNoticeText != null)
        {
            equipmentDismantleNoticeText.text = message;
        }

        if (equipmentBulkDismantleNoticeText != null)
        {
            equipmentBulkDismantleNoticeText.text = message;
        }
    }

    private void RegisterTabNotificationDot(HudTab tab, GameObject dot)
    {
        if (!tabNotificationDots.TryGetValue(tab, out List<GameObject> dots))
        {
            dots = new List<GameObject>();
            tabNotificationDots[tab] = dots;
        }

        dots.Add(dot);
        dot.SetActive(false);
    }

    private void SetTabNotificationDots(HudTab tab, bool visible)
    {
        if (!tabNotificationDots.TryGetValue(tab, out List<GameObject> dots))
        {
            return;
        }

        foreach (GameObject dot in dots)
        {
            if (dot != null)
            {
                dot.SetActive(visible);
            }
        }
    }

    private void SetNotificationDot<TKey>(Dictionary<TKey, GameObject> dots, TKey key, bool visible)
    {
        if (dots.TryGetValue(key, out GameObject dot) && dot != null)
        {
            dot.SetActive(visible);
        }
    }

    private void RefreshBattlefieldVisuals()
    {
        if (battleManager == null || damagePopupText == null || battlefieldRect == null)
        {
            return;
        }

        bool flashActive = hitFlashRemaining > 0f;
        float flashRatio = hitFlashRemaining / 0.28f;
        bool heroBatchFlashActive = heroAttackFlashRemaining > 0f;
        float heroBatchFlashRatio = heroAttackFlashRemaining / 0.28f;
        float time = Time.time;
        float visualDeltaTime = GetBattleVisualDeltaTime();
        float fieldWidth = Mathf.Max(760f, battlefieldRect.rect.width);
        float fieldHeight = Mathf.Max(260f, battlefieldRect.rect.height);

        if (IsWorldBattlefieldEnabled())
        {
            RefreshWorldBattlefieldImage();
            SetLegacyBattlefieldActorsVisible(false);
            if (centerSpawnText != null)
            {
                centerSpawnText.gameObject.SetActive(false);
            }

            damagePopupText.text = string.Empty;
            return;
        }

        SetLegacyBattlefieldActorsVisible(true);
        if (centerSpawnText != null)
        {
            centerSpawnText.gameObject.SetActive(true);
        }

        Vector2[] heroFormation =
        {
            new Vector2(0f, -16f),
            new Vector2(-86f, -26f),
            new Vector2(86f, -26f),
            new Vector2(-48f, 56f),
            new Vector2(48f, 56f),
            new Vector2(-134f, 22f),
            new Vector2(134f, 22f),
            new Vector2(0f, 104f)
        };

        foreach (RectTransform heroRect in heroBattleRects.Values)
        {
            heroRect.localScale = Vector3.zero;
        }

        activeHeroBattlePositions.Clear();
        activeEnemyBattlePositions.Clear();
        ResetActiveEnemyBattlePositions();
        heroBaseBattlePositions.Clear();
        int heroIndex = 0;
        foreach (HeroState hero in battleManager.DeployedHeroes)
        {
            if (!heroBattleImages.TryGetValue(hero.Definition.Id, out Image image))
            {
                continue;
            }

            bool isLastSource = battleManager.LastHitSourceName == hero.Definition.DisplayName && flashActive;
            bool isBatchSource = heroBatchFlashActive && IsHeroInRecentAttackBatch(hero.Definition.Id);
            bool isAttackSource = isLastSource || isBatchSource;
            float attackFlashRatio = Mathf.Max(isLastSource ? flashRatio : 0f, isBatchSource ? heroBatchFlashRatio : 0f);
            if (heroBattleRects.TryGetValue(hero.Definition.Id, out RectTransform heroRect))
            {
                Vector2 formationPosition = heroFormation[heroIndex % heroFormation.Length];
                Vector2 roamOffset = GetHeroRoamOffset(hero, heroIndex, time, fieldWidth, fieldHeight);
                Vector2 traitMotion = GetHeroTraitMotionOffset(hero, heroIndex, time, false, 0f);
                Vector2 battlePosition = ClampBattlefieldPosition(formationPosition + roamOffset + traitMotion, fieldWidth, fieldHeight, 58f);
                heroRect.localScale = Vector3.one * GetHeroTraitScale(hero, isAttackSource, attackFlashRatio, time, heroIndex);
                Vector2 currentHeroPosition = displayedHeroBattlePositions.TryGetValue(hero.Definition.Id, out Vector2 displayedHeroPosition)
                    ? displayedHeroPosition
                    : battlePosition;
                activeHeroBattlePositions.Add(currentHeroPosition);
                heroBaseBattlePositions[hero.Definition.Id] = battlePosition;
            }

            Color baseColor = GetRarityColor(hero.Definition.Rarity);
            image.color = isAttackSource
                ? Color.Lerp(baseColor, new Color(1f, 0.86f, 0.22f, 1f), attackFlashRatio)
                : baseColor;

            if (heroBattleTexts.TryGetValue(hero.Definition.Id, out Text text))
            {
                text.text = GetRarityBadge(hero.Definition.Rarity) + "\n" + GetShortHeroLabel(hero.Definition);
            }

            heroIndex += 1;
        }

        if (centerSpawnText != null)
        {
            RectTransform portalRect = centerSpawnText.GetComponent<RectTransform>();
            float pulse = 1f + Mathf.Sin(time * 5.6f) * 0.13f;
            portalRect.localScale = new Vector3(pulse, pulse, 1f);
            centerSpawnText.text = "◎";
            centerSpawnText.color = battleManager.IsBossFight
                ? new Color(1f, 0.70f, 0.16f, 0.72f)
                : new Color(0.35f, 0.72f, 1f, 0.42f + Mathf.Sin(time * 3.2f) * 0.12f);
        }

        int visible = Mathf.Clamp(battleManager.VisibleEnemyCount, 0, enemyBattleImages.Count);
        string currentBattleStageId = progressManager != null ? progressManager.CurrentStageId : string.Empty;
        bool stageChanged = observedBattleStageId != currentBattleStageId;
        if (stageChanged || observedBattleKillCount > battleManager.KillsThisStage)
        {
            ResetDisplayedEnemyVisuals();
        }

        observedBattleStageId = currentBattleStageId;
        observedBattleKillCount = battleManager.KillsThisStage;
        for (int i = 0; i < enemyBattleImages.Count; i++)
        {
            bool active = i < visible;
            int shiftedIndex = i;
            Image image = enemyBattleImages[shiftedIndex];
            Text text = enemyBattleTexts[shiftedIndex];
            RectTransform enemyRect = enemyBattleRects[shiftedIndex];

            if (!active)
            {
                enemyRect.anchoredPosition = Vector2.zero;
                enemyRect.localScale = Vector3.zero;
                image.color = new Color(0.13f, 0.10f, 0.10f, 0f);
                text.color = Color.white;
                text.text = string.Empty;
                SetEnemyHpBar(shiftedIndex, false, 0f, false);
                ClearActiveEnemyBattlePosition(shiftedIndex);
                SetDisplayedEnemyInactive(shiftedIndex);
                continue;
            }

            int enemySpawnSequence = battleManager.GetVisibleEnemySpawnSequence(i);
            ResetEnemyVisualIfSpawnChanged(shiftedIndex, enemySpawnSequence);
            if (TryRenderEnemyDeathVisual(shiftedIndex, enemyRect, image, text, visualDeltaTime, out Vector2 deathPosition))
            {
                enemyRect.anchoredPosition = deathPosition;
                SetEnemyHpBar(shiftedIndex, false, 0f, false);
                ClearActiveEnemyBattlePosition(shiftedIndex);
                continue;
            }

            bool frontTarget = battleManager.IsBossFight
                ? i == 0 && flashActive
                : i == battleManager.RecentHitEnemyIndex && flashActive;
            if (battleManager.IsBossFight)
            {
                Vector2 bossAnchor = GetNearestHeroAggroPosition(Vector2.zero) + new Vector2(0f, 112f);
                Vector2 bossPosition = new Vector2(
                    bossAnchor.x * 0.38f + Mathf.Sin(time * 1.4f) * 18f,
                    bossAnchor.y * 0.38f + Mathf.Cos(time * 1.1f) * 10f + fieldHeight * 0.12f);
                Vector2 desiredEnemyPosition = ClampBattlefieldPosition(bossPosition, fieldWidth, fieldHeight, 86f);
                Vector2 finalEnemyPosition = SmoothEnemyBattlePosition(shiftedIndex, desiredEnemyPosition, desiredEnemyPosition, visualDeltaTime, true);
                enemyRect.anchoredPosition = finalEnemyPosition;
                activeEnemyBattlePositions.Add(finalEnemyPosition);
                SetActiveEnemyBattlePosition(shiftedIndex, finalEnemyPosition);
                enemyRect.localScale = Vector3.one * (frontTarget ? 1.65f + 0.18f * flashRatio : 1.52f);
            }
            else
            {
                int movementSeed = enemySpawnSequence >= 0 ? enemySpawnSequence : i;
                Vector2 direction = GetEnemySpreadDirection(movementSeed);
                float spawnDistance = Mathf.Max(fieldWidth * 0.58f, fieldHeight * 0.58f);
                Vector2 spawnPosition = direction * spawnDistance;
                Vector2 drift = new Vector2(-direction.y, direction.x) * Mathf.Sin(time * 2.3f + movementSeed) * 11f;
                Vector2 provisionalPosition = GetDisplayedEnemyPositionOrSpawn(shiftedIndex, spawnPosition);
                Vector2 aggroPosition = GetNearestHeroAggroPosition(provisionalPosition);
                Vector2 targetPosition = aggroPosition + GetEnemyAggroOffset(movementSeed, direction, time);
                Vector2 desiredEnemyPosition = ClampBattlefieldPosition(targetPosition + drift, fieldWidth, fieldHeight, 42f);
                Vector2 finalEnemyPosition = SmoothEnemyBattlePosition(shiftedIndex, desiredEnemyPosition, spawnPosition, visualDeltaTime, false);
                float approach = GetEnemyApproachRatio(spawnPosition, targetPosition, finalEnemyPosition);
                enemyRect.anchoredPosition = finalEnemyPosition;
                activeEnemyBattlePositions.Add(finalEnemyPosition);
                SetActiveEnemyBattlePosition(shiftedIndex, finalEnemyPosition);
                enemyRect.localScale = Vector3.one * (0.68f + 0.30f * approach + (frontTarget ? 0.18f * flashRatio : 0f));
            }

            if (battleManager.IsBossFight)
            {
                image.color = frontTarget
                    ? new Color(1f, 0.34f, 0.22f, 1f)
                    : new Color(0.62f, 0.12f, 0.10f, 1f);
                text.color = Color.white;
                text.text = "BOSS";
                SetEnemyHpBar(shiftedIndex, true, GetEnemyHpRatio(i), true);
            }
            else
            {
                image.color = frontTarget
                    ? new Color(1f, 0.48f, 0.24f, 1f)
                    : new Color(0.52f, 0.16f + 0.03f * (i % 3), 0.12f, 1f);
                text.color = Color.white;
                text.text = "M" + battleManager.GetVisibleEnemyDisplayNumber(i);
                SetEnemyHpBar(shiftedIndex, true, GetEnemyHpRatio(i), false);
            }
        }

        activeHeroBattlePositions.Clear();
        heroIndex = 0;
        foreach (HeroState hero in battleManager.DeployedHeroes)
        {
            if (!heroBattleRects.TryGetValue(hero.Definition.Id, out RectTransform heroRect))
            {
                continue;
            }

            Vector2 basePosition = heroBaseBattlePositions.TryGetValue(hero.Definition.Id, out Vector2 storedPosition)
                ? storedPosition
                : Vector2.zero;
            bool isLastSource = battleManager.LastHitSourceName == hero.Definition.DisplayName && flashActive;
            bool isBatchSource = heroBatchFlashActive && IsHeroInRecentAttackBatch(hero.Definition.Id);
            bool isAttackSource = isLastSource || isBatchSource;
            float attackFlashRatio = Mathf.Max(isLastSource ? flashRatio : 0f, isBatchSource ? heroBatchFlashRatio : 0f);
            Vector2 enemyPosition = GetHeroLockedEnemyPosition(hero.Definition.Id, basePosition);
            Vector2 pursuitOffset = GetHeroPursuitOffset(hero, heroIndex, basePosition, enemyPosition, time, isAttackSource, attackFlashRatio);
            Vector2 desiredHeroPosition = ClampBattlefieldPosition(basePosition + pursuitOffset, fieldWidth, fieldHeight, 58f);
            Vector2 finalHeroPosition = SmoothHeroBattlePosition(hero, desiredHeroPosition, visualDeltaTime, isAttackSource, attackFlashRatio);
            heroRect.anchoredPosition = finalHeroPosition;
            heroRect.localScale = Vector3.one * GetHeroTraitScale(hero, isAttackSource, attackFlashRatio, time, heroIndex);
            activeHeroBattlePositions.Add(finalHeroPosition);

            if (heroBattleImages.TryGetValue(hero.Definition.Id, out Image image))
            {
                Color baseColor = GetRarityColor(hero.Definition.Rarity);
                image.color = isAttackSource
                    ? Color.Lerp(baseColor, new Color(1f, 0.86f, 0.22f, 1f), attackFlashRatio)
                    : baseColor;
            }

            heroIndex += 1;
        }

        RefreshDamagePopup(flashRatio);
    }

    private bool IsWorldBattlefieldEnabled()
    {
        return battlefieldWorldView != null && battlefieldWorldView.OutputTexture != null && battlefieldWorldImage != null;
    }

    private void RefreshWorldBattlefieldImage()
    {
        if (battlefieldWorldImage == null || battlefieldWorldView == null)
        {
            return;
        }

        battlefieldWorldImage.texture = battlefieldWorldView.OutputTexture;
        battlefieldWorldImage.gameObject.SetActive(true);
    }

    private void SetLegacyBattlefieldActorsVisible(bool visible)
    {
        foreach (RectTransform heroRect in heroBattleRects.Values)
        {
            if (heroRect != null && heroRect.gameObject.activeSelf != visible)
            {
                heroRect.gameObject.SetActive(visible);
            }
        }

        for (int i = 0; i < enemyBattleRects.Count; i++)
        {
            RectTransform enemyRect = enemyBattleRects[i];
            if (enemyRect != null && enemyRect.gameObject.activeSelf != visible)
            {
                enemyRect.gameObject.SetActive(visible);
            }
        }
    }

    private void RefreshDamagePopup(float flashRatio)
    {
        if (battleManager.HitSequence <= 0)
        {
            damagePopupText.text = "READY";
            damagePopupText.color = new Color(0.72f, 0.78f, 0.86f, 1f);
            return;
        }

        damagePopupText.text = battleManager.LastHitSourceName
            + "\n-" + FormatShortNumber(battleManager.LastHitDamage)
            + (battleManager.LastHitWasCritical ? " CRIT" : string.Empty);
        damagePopupText.color = battleManager.LastHitWasCritical
            ? new Color(1f, 0.91f, 0.24f, 1f)
            : new Color(1f, 0.55f, 0.32f, 1f);

        RectTransform damageRect = damagePopupText.GetComponent<RectTransform>();
        damageRect.anchoredPosition = new Vector2(0f, 24f + 40f * flashRatio);
        damageRect.localScale = Vector3.one * (1f + 0.25f * flashRatio);
    }

    private bool IsHeroInRecentAttackBatch(string heroId)
    {
        IReadOnlyList<string> attackIds = battleManager.RecentHeroAttackIds;
        for (int i = 0; i < attackIds.Count; i++)
        {
            if (attackIds[i] == heroId)
            {
                return true;
            }
        }

        return false;
    }

    private void SetEnemyHpBar(int index, bool visible, float ratio, bool isBoss)
    {
        if (index < 0 || index >= enemyHpBarObjects.Count || index >= enemyHpFillImages.Count)
        {
            return;
        }

        GameObject hpBar = enemyHpBarObjects[index];
        if (hpBar == null)
        {
            return;
        }

        hpBar.SetActive(visible);
        if (!visible)
        {
            return;
        }

        Image fill = enemyHpFillImages[index];
        if (fill == null)
        {
            return;
        }

        float clampedRatio = Mathf.Clamp01(ratio);
        fill.rectTransform.anchorMax = new Vector2(clampedRatio, 1f);
        fill.color = isBoss
            ? new Color(0.95f, 0.18f, 0.15f, 1f)
            : Color.Lerp(new Color(0.95f, 0.23f, 0.16f, 1f), new Color(0.35f, 0.93f, 0.28f, 1f), clampedRatio);
    }

    private float GetEnemyHpRatio(int visualOrderIndex)
    {
        return battleManager.GetVisibleEnemyHpRatio(visualOrderIndex);
    }

    private float GetBattleVisualDeltaTime()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f || float.IsNaN(deltaTime))
        {
            deltaTime = 1f / 60f;
        }

        return Mathf.Min(deltaTime, 0.033f);
    }

    private Vector2 SmoothHeroBattlePosition(HeroState hero, Vector2 targetPosition, float visualDeltaTime, bool isAttackSource, float flashRatio)
    {
        string heroId = hero.Definition.Id;
        if (!displayedHeroBattlePositions.TryGetValue(heroId, out Vector2 currentPosition))
        {
            displayedHeroBattlePositions[heroId] = targetPosition;
            return targetPosition;
        }

        float move = Mathf.Max(0.1f, hero.MoveSpeed);
        float attackDash = isAttackSource ? (90f + hero.AttackSpeed * 45f) * Mathf.Clamp01(flashRatio) : 0f;
        float pixelsPerSecond = 82f + move * 54f + attackDash;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, pixelsPerSecond * visualDeltaTime);
        displayedHeroBattlePositions[heroId] = nextPosition;
        return nextPosition;
    }

    private Vector2 SmoothEnemyBattlePosition(int index, Vector2 targetPosition, Vector2 spawnPosition, float visualDeltaTime, bool isBoss)
    {
        EnsureDisplayedEnemyState(index);

        Vector2 currentPosition = displayedEnemyActiveStates[index]
            ? displayedEnemyBattlePositions[index]
            : spawnPosition;

        displayedEnemyActiveStates[index] = true;
        float pixelsPerSecond = isBoss ? 135f : 150f + index % 4 * 22f;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, pixelsPerSecond * visualDeltaTime);
        displayedEnemyBattlePositions[index] = nextPosition;
        return nextPosition;
    }

    private Vector2 GetDisplayedEnemyPositionOrSpawn(int index, Vector2 spawnPosition)
    {
        EnsureDisplayedEnemyState(index);
        return displayedEnemyActiveStates[index] ? displayedEnemyBattlePositions[index] : spawnPosition;
    }

    private void ResetEnemyVisualIfSpawnChanged(int index, int spawnSequence)
    {
        EnsureDisplayedEnemyState(index);
        if (displayedEnemySpawnSequences[index] == spawnSequence)
        {
            return;
        }

        if (displayedEnemySpawnSequences[index] >= 0 && displayedEnemyActiveStates[index])
        {
            displayedEnemyDeathDelays[index] = EnemyDeathVisualSeconds;
            displayedEnemyDeathPositions[index] = displayedEnemyBattlePositions[index];
        }

        displayedEnemySpawnSequences[index] = spawnSequence;
        displayedEnemyActiveStates[index] = false;
    }

    private bool TryRenderEnemyDeathVisual(
        int index,
        RectTransform enemyRect,
        Image image,
        Text text,
        float visualDeltaTime,
        out Vector2 deathPosition)
    {
        EnsureDisplayedEnemyState(index);
        deathPosition = displayedEnemyDeathPositions[index];
        if (displayedEnemyDeathDelays[index] <= 0f)
        {
            return false;
        }

        displayedEnemyDeathDelays[index] = Mathf.Max(0f, displayedEnemyDeathDelays[index] - visualDeltaTime);
        float progress = 1f - displayedEnemyDeathDelays[index] / EnemyDeathVisualSeconds;
        float alpha = 1f - progress;
        deathPosition = displayedEnemyDeathPositions[index] + Vector2.up * (24f * progress);
        enemyRect.localScale = Vector3.one * Mathf.Lerp(0.9f, 0.18f, progress);
        image.color = new Color(1f, 0.18f, 0.12f, Mathf.Clamp01(alpha));
        text.text = "KO";
        text.color = new Color(1f, 0.95f, 0.7f, Mathf.Clamp01(alpha));
        return true;
    }

    private void SetDisplayedEnemyInactive(int index)
    {
        EnsureDisplayedEnemyState(index);
        displayedEnemyActiveStates[index] = false;
        displayedEnemySpawnSequences[index] = -1;
        displayedEnemyDeathDelays[index] = 0f;
    }

    private void EnsureDisplayedEnemyState(int index)
    {
        while (displayedEnemyBattlePositions.Count <= index)
        {
            displayedEnemyBattlePositions.Add(Vector2.zero);
        }

        while (displayedEnemyActiveStates.Count <= index)
        {
            displayedEnemyActiveStates.Add(false);
        }

        while (displayedEnemySpawnSequences.Count <= index)
        {
            displayedEnemySpawnSequences.Add(-1);
        }

        while (displayedEnemyDeathDelays.Count <= index)
        {
            displayedEnemyDeathDelays.Add(0f);
        }

        while (displayedEnemyDeathPositions.Count <= index)
        {
            displayedEnemyDeathPositions.Add(Vector2.zero);
        }

        while (activeEnemyBattlePositionsByIndex.Count <= index)
        {
            activeEnemyBattlePositionsByIndex.Add(Vector2.zero);
        }

        while (activeEnemyBattlePositionStates.Count <= index)
        {
            activeEnemyBattlePositionStates.Add(false);
        }
    }

    private void ResetActiveEnemyBattlePositions()
    {
        for (int i = 0; i < activeEnemyBattlePositionStates.Count; i++)
        {
            activeEnemyBattlePositionStates[i] = false;
        }
    }

    private void ResetDisplayedEnemyVisuals()
    {
        for (int i = 0; i < displayedEnemyActiveStates.Count; i++)
        {
            displayedEnemyActiveStates[i] = false;
        }

        for (int i = 0; i < displayedEnemySpawnSequences.Count; i++)
        {
            displayedEnemySpawnSequences[i] = -1;
        }

        for (int i = 0; i < displayedEnemyDeathDelays.Count; i++)
        {
            displayedEnemyDeathDelays[i] = 0f;
        }

        ResetActiveEnemyBattlePositions();
    }

    private void SetActiveEnemyBattlePosition(int index, Vector2 position)
    {
        EnsureDisplayedEnemyState(index);
        activeEnemyBattlePositionsByIndex[index] = position;
        activeEnemyBattlePositionStates[index] = true;
    }

    private void ClearActiveEnemyBattlePosition(int index)
    {
        EnsureDisplayedEnemyState(index);
        activeEnemyBattlePositionStates[index] = false;
    }

    private static float GetEnemyApproachRatio(Vector2 spawnPosition, Vector2 targetPosition, Vector2 currentPosition)
    {
        float totalDistance = Vector2.Distance(spawnPosition, targetPosition);
        if (totalDistance <= 0.001f)
        {
            return 1f;
        }

        float remainingDistance = Vector2.Distance(currentPosition, targetPosition);
        return Mathf.Clamp01(1f - remainingDistance / totalDistance);
    }

    private Vector2 GetHeroRoamOffset(HeroState hero, int heroIndex, float time, float fieldWidth, float fieldHeight)
    {
        float phase = heroIndex * 1.37f;
        float move = Mathf.Max(0.1f, hero.MoveSpeed);
        float xRadius = Mathf.Min(116f + move * 8f, fieldWidth * 0.20f);
        float yRadius = Mathf.Min(58f + move * 6f, fieldHeight * 0.18f);

        switch (hero.Definition.Trait)
        {
            case HeroTrait.Melee:
            {
                float patrol = Mathf.Sin(time * (0.64f + move * 0.045f) + phase);
                float weave = Mathf.Sin(time * (0.92f + move * 0.05f) + phase * 0.7f);
                return new Vector2(weave * xRadius * 0.48f, 18f + patrol * yRadius * 0.78f);
            }
            case HeroTrait.Ranged:
            {
                float strafe = Mathf.Sin(time * (0.52f + move * 0.04f) + phase);
                float backStep = Mathf.Cos(time * (0.36f + move * 0.03f) + phase);
                return new Vector2(strafe * xRadius * 0.88f, -32f + backStep * yRadius * 0.42f);
            }
            case HeroTrait.Support:
            {
                float orbitSpeed = 0.42f + move * 0.035f;
                return new Vector2(
                    Mathf.Cos(time * orbitSpeed + phase) * xRadius * 0.52f,
                    Mathf.Sin(time * (orbitSpeed + 0.18f) + phase) * yRadius * 0.64f);
            }
            case HeroTrait.Defense:
            {
                float guardPatrol = Mathf.Sin(time * (0.38f + move * 0.025f) + phase);
                float braceShift = Mathf.Sin(time * (0.78f + move * 0.04f) + phase * 0.5f);
                return new Vector2(guardPatrol * xRadius * 0.35f, -2f + braceShift * yRadius * 0.36f);
            }
            default:
            {
                return new Vector2(
                    Mathf.Sin(time * 0.6f + phase) * xRadius * 0.45f,
                    Mathf.Cos(time * 0.5f + phase) * yRadius * 0.45f);
            }
        }
    }

    private Vector2 GetNearestHeroAggroPosition(Vector2 fromPosition)
    {
        if (activeHeroBattlePositions.Count <= 0)
        {
            return Vector2.zero;
        }

        Vector2 nearest = activeHeroBattlePositions[0];
        float nearestDistance = (fromPosition - nearest).sqrMagnitude;
        for (int i = 1; i < activeHeroBattlePositions.Count; i++)
        {
            float distance = (fromPosition - activeHeroBattlePositions[i]).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = activeHeroBattlePositions[i];
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private Vector2 GetNearestEnemyPosition(Vector2 fromPosition)
    {
        if (activeEnemyBattlePositions.Count <= 0)
        {
            return fromPosition;
        }

        Vector2 nearest = activeEnemyBattlePositions[0];
        float nearestDistance = (fromPosition - nearest).sqrMagnitude;
        for (int i = 1; i < activeEnemyBattlePositions.Count; i++)
        {
            float distance = (fromPosition - activeEnemyBattlePositions[i]).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = activeEnemyBattlePositions[i];
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private Vector2 GetHeroLockedEnemyPosition(string heroId, Vector2 fallbackFromPosition)
    {
        if (battleManager != null)
        {
            int targetIndex = battleManager.GetHeroTargetVisualIndex(heroId);
            if (targetIndex >= 0
                && targetIndex < activeEnemyBattlePositionsByIndex.Count
                && targetIndex < activeEnemyBattlePositionStates.Count
                && activeEnemyBattlePositionStates[targetIndex])
            {
                return activeEnemyBattlePositionsByIndex[targetIndex];
            }
        }

        return GetNearestEnemyPosition(fallbackFromPosition);
    }

    private Vector2 GetHeroPursuitOffset(HeroState hero, int heroIndex, Vector2 fromPosition, Vector2 enemyPosition, float time, bool isAttackSource, float flashRatio)
    {
        Vector2 toEnemy = enemyPosition - fromPosition;
        if (toEnemy.sqrMagnitude <= 0.001f)
        {
            return Vector2.zero;
        }

        Vector2 direction = toEnemy.normalized;
        Vector2 tangent = new Vector2(-direction.y, direction.x);
        float phase = heroIndex * 0.91f;
        float move = Mathf.Max(0.1f, hero.MoveSpeed);
        float attack = Mathf.Max(0.1f, hero.AttackSpeed);
        float hit = isAttackSource ? Mathf.Clamp01(flashRatio) : 0f;
        float distance = toEnemy.magnitude;

        switch (hero.Definition.Trait)
        {
            case HeroTrait.Melee:
            {
                float chase = Mathf.Min(distance - 34f, 74f + move * 7f);
                float lunge = hit * (38f + attack * 12f);
                float weave = Mathf.Sin(time * (2.4f + move * 0.18f) + phase) * 10f;
                return direction * Mathf.Max(0f, chase + lunge) + tangent * weave;
            }
            case HeroTrait.Ranged:
            {
                float preferredDistance = 158f;
                float adjust = Mathf.Clamp(distance - preferredDistance, -36f, 46f);
                float strafe = Mathf.Sin(time * (1.5f + move * 0.11f) + phase) * (24f + move * 2f);
                return direction * (adjust + hit * 16f) + tangent * strafe;
            }
            case HeroTrait.Support:
            {
                float preferredDistance = 118f;
                float adjust = Mathf.Clamp(distance - preferredDistance, -28f, 36f);
                float orbit = Mathf.Sin(time * (1.15f + move * 0.08f) + phase) * 22f;
                return direction * (adjust + hit * 18f) + tangent * orbit;
            }
            case HeroTrait.Defense:
            {
                float chase = Mathf.Min(distance - 54f, 42f + move * 4f);
                float guard = Mathf.Sin(time * (0.95f + move * 0.06f) + phase) * 8f;
                return direction * Mathf.Max(0f, chase + hit * 18f) + tangent * guard;
            }
            default:
            {
                float chase = Mathf.Clamp(distance - 92f, 0f, 52f + move * 4f);
                return direction * (chase + hit * 24f);
            }
        }
    }

    private Vector2 GetEnemyAggroOffset(int index, Vector2 spawnDirection, float time)
    {
        Vector2 tangent = new Vector2(-spawnDirection.y, spawnDirection.x);
        float side = ((index % 5) - 2) * 18f;
        float ring = 54f + (index % 3) * 12f + Mathf.Sin(time * 2.1f + index) * 7f;
        return spawnDirection * ring + tangent * side;
    }

    private static Vector2 ClampBattlefieldPosition(Vector2 position, float fieldWidth, float fieldHeight, float margin)
    {
        float halfWidth = Mathf.Max(0f, fieldWidth * 0.5f - margin);
        float halfHeight = Mathf.Max(0f, fieldHeight * 0.5f - margin);
        return new Vector2(
            Mathf.Clamp(position.x, -halfWidth, halfWidth),
            Mathf.Clamp(position.y, -halfHeight, halfHeight));
    }

    private string GetShortHeroLabel(HeroDefinition hero)
    {
        if (hero.DisplayName.Length <= 2)
        {
            return hero.DisplayName;
        }

        return hero.DisplayName.Substring(hero.DisplayName.Length - 2);
    }

    private bool IsDebugPanelEnabled()
    {
        return Application.isEditor || Debug.isDebugBuild;
    }

    private string GetModeLabel(ProgressMode mode)
    {
        switch (mode)
        {
            case ProgressMode.AutoProgress:
                return "자동 진행";
            case ProgressMode.RepeatSelected:
                return "선택 반복";
            case ProgressMode.BossBlocked:
                return "보스 막힘";
            default:
                return mode.ToString();
        }
    }

    private string GetHeroPageTabLabel(HeroPageTab tab)
    {
        switch (tab)
        {
            case HeroPageTab.Formation:
                return "편성";
            case HeroPageTab.Trait:
                return "특성";
            case HeroPageTab.Statue:
                return "토템";
            case HeroPageTab.Seal:
                return "룬";
            case HeroPageTab.Relic:
                return "성물";
            default:
                return tab.ToString();
        }
    }

    private GameObject CreateNotificationDot(Transform parent, float size, Vector2 anchoredPosition)
    {
        Text dot = CreateText("RedDot", parent, Mathf.RoundToInt(size), FontStyle.Bold, TextAnchor.MiddleCenter);
        dot.text = "●";
        dot.color = new Color(1f, 0.04f, 0.04f, 1f);
        dot.raycastTarget = false;

        RectTransform rect = dot.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = anchoredPosition;

        dot.gameObject.SetActive(false);
        return dot.gameObject;
    }

    private string GetRarityBadge(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return "C";
            case HeroRarity.Uncommon:
                return "UC";
            case HeroRarity.Rare:
                return "R";
            case HeroRarity.Epic:
                return "E";
            case HeroRarity.Legendary:
                return "L";
            case HeroRarity.Mythic:
                return "M";
            default:
                return "?";
        }
    }

    private string GetRarityLabel(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return "커먼";
            case HeroRarity.Uncommon:
                return "언커먼";
            case HeroRarity.Rare:
                return "레어";
            case HeroRarity.Epic:
                return "에픽";
            case HeroRarity.Legendary:
                return "레전더리";
            case HeroRarity.Mythic:
                return "신화";
            default:
                return "미정";
        }
    }

    private string GetTraitBadge(HeroTrait trait)
    {
        switch (trait)
        {
            case HeroTrait.Melee:
                return "근";
            case HeroTrait.Ranged:
                return "원";
            case HeroTrait.Support:
                return "지";
            case HeroTrait.Defense:
                return "방";
            default:
                return "?";
        }
    }

    private string GetTraitLabel(HeroTrait trait)
    {
        switch (trait)
        {
            case HeroTrait.Melee:
                return "근접형";
            case HeroTrait.Ranged:
                return "원거리형";
            case HeroTrait.Support:
                return "지원형";
            case HeroTrait.Defense:
                return "방어형";
            default:
                return "미정";
        }
    }

    private string GetEquipmentSlotLabel(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                return "무기";
            case EquipmentSlot.Hat:
                return "모자";
            case EquipmentSlot.Armor:
                return "갑옷";
            case EquipmentSlot.Accessory:
                return "장신구";
            case EquipmentSlot.Potion:
                return "포션";
            default:
                return "미정";
        }
    }

    private string BuildEquipmentFilterButtonLabel(EquipmentSlot slot)
    {
        return (heroDetailEquipmentSelectedSlots.Contains(slot) ? "[x] " : "[ ] ") + GetEquipmentSlotLabel(slot);
    }

    private string BuildEquipmentFilterSummaryLabel()
    {
        if (heroDetailEquipmentSelectedSlots.Count >= HeroDetailEquipmentFilterSlots.Length)
        {
            return "전체";
        }

        if (heroDetailEquipmentSelectedSlots.Count <= 0)
        {
            return "없음";
        }

        string label = string.Empty;
        foreach (EquipmentSlot slot in HeroDetailEquipmentFilterSlots)
        {
            if (!heroDetailEquipmentSelectedSlots.Contains(slot))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(label))
            {
                label += ", ";
            }

            label += GetEquipmentSlotLabel(slot);
        }

        return label;
    }

    private string FormatStars(int stars)
    {
        int clampedStars = Mathf.Clamp(stars, 0, HeroDefinition.MaxStars);
        if (clampedStars <= 0)
        {
            return "<color=#6F778A>☆☆☆☆☆</color>";
        }

        int completedLayers = (clampedStars - 1) / 5;
        int starsInCurrentLayer = ((clampedStars - 1) % 5) + 1;
        string baseColor = completedLayers == 0 ? "#6F778A" : GetStarLayerColor(completedLayers - 1);
        string currentColor = GetStarLayerColor(completedLayers);
        string result = string.Empty;
        for (int i = 1; i <= 5; i++)
        {
            string color = i <= starsInCurrentLayer ? currentColor : baseColor;
            result += "<color=" + color + ">★</color>";
        }

        return result;
    }

    private string GetStarLayerColor(int layer)
    {
        switch (layer)
        {
            case 0:
                return "#FFD84D";
            case 1:
                return "#51A7FF";
            default:
                return "#C15CFF";
        }
    }

    private Color GetTranscendGradeColor(HeroTranscendGrade grade)
    {
        switch (grade)
        {
            case HeroTranscendGrade.F:
                return new Color(0.43f, 0.49f, 0.58f, 1f);
            case HeroTranscendGrade.E:
                return new Color(0.33f, 0.55f, 0.70f, 1f);
            case HeroTranscendGrade.D:
                return new Color(0.26f, 0.60f, 0.46f, 1f);
            case HeroTranscendGrade.C:
                return new Color(0.42f, 0.64f, 0.24f, 1f);
            case HeroTranscendGrade.B:
                return new Color(0.66f, 0.58f, 0.22f, 1f);
            case HeroTranscendGrade.A:
                return new Color(0.84f, 0.45f, 0.18f, 1f);
            case HeroTranscendGrade.S:
                return new Color(0.78f, 0.28f, 0.86f, 1f);
            case HeroTranscendGrade.SS:
                return new Color(0.33f, 0.72f, 1f, 1f);
            default:
                return Color.white;
        }
    }

    private string GetTranscendGradeHex(HeroTranscendGrade grade)
    {
        switch (grade)
        {
            case HeroTranscendGrade.F:
                return "#A9B2C4";
            case HeroTranscendGrade.E:
                return "#86C8FF";
            case HeroTranscendGrade.D:
                return "#65D29B";
            case HeroTranscendGrade.C:
                return "#9BDA5A";
            case HeroTranscendGrade.B:
                return "#FFD65A";
            case HeroTranscendGrade.A:
                return "#FF9C4A";
            case HeroTranscendGrade.S:
                return "#E66BFF";
            case HeroTranscendGrade.SS:
                return "#6DD7FF";
            default:
                return "#FFFFFF";
        }
    }

    private Color GetRarityColor(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Common:
                return new Color(0.36f, 0.38f, 0.40f, 1f);
            case HeroRarity.Uncommon:
                return new Color(0.18f, 0.36f, 0.25f, 1f);
            case HeroRarity.Rare:
                return new Color(0.16f, 0.32f, 0.58f, 1f);
            case HeroRarity.Epic:
                return new Color(0.44f, 0.20f, 0.62f, 1f);
            case HeroRarity.Legendary:
                return new Color(0.76f, 0.47f, 0.12f, 1f);
            case HeroRarity.Mythic:
                return new Color(0.64f, 0.16f, 0.18f, 1f);
            default:
                return new Color(0.16f, 0.24f, 0.34f, 1f);
        }
    }

    private Color GetTotemColor(TotemDefinition totem)
    {
        if (totem == null)
        {
            return new Color(0.20f, 0.27f, 0.38f, 1f);
        }

        switch (totem.Archetype)
        {
            case TotemArchetype.Combat:
                return new Color(0.55f, 0.20f, 0.20f, 1f);
            case TotemArchetype.Support:
                return new Color(0.42f, 0.36f, 0.14f, 1f);
            case TotemArchetype.Guardian:
                return new Color(0.22f, 0.42f, 0.54f, 1f);
            case TotemArchetype.Storm:
                return new Color(0.18f, 0.48f, 0.44f, 1f);
            case TotemArchetype.Arcane:
                return new Color(0.42f, 0.22f, 0.58f, 1f);
            default:
                return new Color(0.20f, 0.27f, 0.38f, 1f);
        }
    }

    private Color GetRuneColor(RuneDefinition rune)
    {
        if (rune == null)
        {
            return new Color(0.20f, 0.27f, 0.38f, 1f);
        }

        switch (rune.EffectKind)
        {
            case RuneEffectKind.Strike:
                return new Color(0.46f, 0.24f, 0.22f, 1f);
            case RuneEffectKind.Execute:
                return new Color(0.42f, 0.18f, 0.30f, 1f);
            case RuneEffectKind.Barrier:
                return new Color(0.20f, 0.34f, 0.48f, 1f);
            case RuneEffectKind.Harvest:
                return new Color(0.42f, 0.36f, 0.16f, 1f);
            case RuneEffectKind.Arcane:
                return new Color(0.36f, 0.22f, 0.56f, 1f);
            case RuneEffectKind.Storm:
                return new Color(0.18f, 0.45f, 0.43f, 1f);
            case RuneEffectKind.Focus:
                return new Color(0.30f, 0.40f, 0.60f, 1f);
            case RuneEffectKind.Vitality:
                return new Color(0.24f, 0.42f, 0.30f, 1f);
            case RuneEffectKind.Command:
                return new Color(0.38f, 0.32f, 0.52f, 1f);
            case RuneEffectKind.Regeneration:
                return new Color(0.25f, 0.38f, 0.34f, 1f);
            default:
                return new Color(0.20f, 0.27f, 0.38f, 1f);
        }
    }

    private static string GetTotemCategoryLabel(TotemArchetype archetype)
    {
        switch (archetype)
        {
            case TotemArchetype.Combat:
                return "전투\n토템";
            case TotemArchetype.Guardian:
                return "수호\n토템";
            case TotemArchetype.Support:
                return "지원\n토템";
            case TotemArchetype.Arcane:
                return "비전\n토템";
            case TotemArchetype.Storm:
                return "폭풍\n토템";
            default:
                return "토템";
        }
    }

    private Vector2 GetHeroTraitMotionOffset(HeroState hero, int heroIndex, float time, bool isLastSource, float flashRatio)
    {
        float phase = heroIndex * 0.73f;
        float move = Mathf.Max(0.1f, hero.MoveSpeed);
        float attack = Mathf.Max(0.1f, hero.AttackSpeed);
        float hit = isLastSource ? Mathf.Clamp01(flashRatio) : 0f;

        switch (hero.Definition.Trait)
        {
            case HeroTrait.Melee:
            {
                float tempo = 3.4f + move * 0.55f + attack * 0.35f;
                float lunge = Mathf.Max(0f, Mathf.Sin(time * tempo + phase)) * (14f + move * 3.4f);
                float sideStep = Mathf.Sin(time * (1.7f + move * 0.12f) + phase) * (8f + move);
                return new Vector2(sideStep, 8f + lunge + 34f * hit);
            }
            case HeroTrait.Ranged:
            {
                float strafe = Mathf.Sin(time * (1.45f + move * 0.16f) + phase) * (20f + move * 1.5f);
                float aimBob = Mathf.Sin(time * (2.3f + attack * 0.3f) + phase) * 4f;
                return new Vector2(strafe, -22f + aimBob - 22f * hit);
            }
            case HeroTrait.Support:
            {
                float orbitSpeed = 1.25f + move * 0.12f;
                float orbitX = Mathf.Cos(time * orbitSpeed + phase) * (15f + move * 1.1f);
                float orbitY = Mathf.Sin(time * (orbitSpeed + 0.32f) + phase) * (12f + move);
                return new Vector2(orbitX, orbitY + 26f * hit);
            }
            case HeroTrait.Defense:
            {
                float brace = Mathf.Sin(time * (1.05f + move * 0.08f) + phase) * 3.5f;
                float guardStep = Mathf.Max(0f, Mathf.Sin(time * (2f + attack * 0.2f) + phase)) * 7f;
                return new Vector2(Mathf.Sin(time * 0.8f + phase) * 4f, -8f + brace + guardStep + 16f * hit);
            }
            default:
            {
                float bob = Mathf.Sin(time * (3.2f + move * 0.45f) + phase) * (4f + move);
                return new Vector2(0f, bob + 28f * hit);
            }
        }
    }

    private float GetHeroTraitScale(HeroState hero, bool isLastSource, float flashRatio, float time, int heroIndex)
    {
        float phase = heroIndex * 0.73f;
        float hit = isLastSource ? Mathf.Clamp01(flashRatio) : 0f;

        switch (hero.Definition.Trait)
        {
            case HeroTrait.Melee:
                return 1f + Mathf.Max(0f, Mathf.Sin(time * (4f + hero.AttackSpeed) + phase)) * 0.035f + 0.2f * hit;
            case HeroTrait.Ranged:
                return 0.96f + Mathf.Sin(time * 1.7f + phase) * 0.015f + 0.12f * hit;
            case HeroTrait.Support:
                return 0.98f + Mathf.Sin(time * 2.2f + phase) * 0.035f + 0.14f * hit;
            case HeroTrait.Defense:
                return 1.07f + Mathf.Sin(time * 1.1f + phase) * 0.012f + 0.1f * hit;
            default:
                return 1f + 0.18f * hit;
        }
    }

    private Vector2 GetEnemySpreadDirection(int index)
    {
        float angle = index * 137.5f * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return direction.sqrMagnitude <= 0.001f ? Vector2.right : direction.normalized;
    }

    private string FormatShortNumber(double value)
    {
        return NumberFormatter.Format(value);
    }

    private string FormatShortNumber(GameNumber value)
    {
        return NumberFormatter.Format(value);
    }

    private string FormatCountNumber(long value)
    {
        return GameData.ClampCount(value).ToString("#,0");
    }

    private static Sprite GetRoundedPanelSprite()
    {
        if (roundedPanelSprite == null)
        {
            roundedPanelSprite = CreateRoundedRectSprite(64, 14, 16);
        }

        return roundedPanelSprite;
    }

    private static Sprite GetRoundedButtonSprite()
    {
        if (roundedButtonSprite == null)
        {
            roundedButtonSprite = CreateRoundedRectSprite(64, 20, 22);
        }

        return roundedButtonSprite;
    }

    private static Sprite GetRoundedPillSprite()
    {
        if (roundedPillSprite == null)
        {
            roundedPillSprite = CreateRoundedRectSprite(64, 28, 28);
        }

        return roundedPillSprite;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite == null)
        {
            circleSprite = CreateCircleSprite(96, 0f);
        }

        return circleSprite;
    }

    private static Sprite GetRingSprite()
    {
        if (ringSprite == null)
        {
            ringSprite = CreateCircleSprite(128, 0.72f);
        }

        return ringSprite;
    }

    private static Sprite GetCoinIconSprite()
    {
        if (coinIconSprite == null)
        {
            coinIconSprite = CreateCoinIconSprite(64);
        }

        return coinIconSprite;
    }

    private static Sprite GetGemIconSprite()
    {
        if (gemIconSprite == null)
        {
            gemIconSprite = CreateGemIconSprite(64);
        }

        return gemIconSprite;
    }

    private static Sprite GetPowerIconSprite()
    {
        if (powerIconSprite == null)
        {
            powerIconSprite = CreatePowerIconSprite(64);
        }

        return powerIconSprite;
    }

    private static Sprite CreateRoundedRectSprite(int size, int radius, int border)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                float cx = Mathf.Clamp(px, radius, size - radius);
                float cy = Mathf.Clamp(py, radius, size - radius);
                float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01(radius + 0.5f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply(false, true);
        texture.hideFlags = HideFlags.HideAndDontSave;
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static Sprite CreateCircleSprite(int size, float innerCutoutRatio)
    {
        Texture2D texture = CreateTransparentTexture(size);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float outerRadius = size * 0.47f;
        float innerRadius = outerRadius * Mathf.Clamp01(innerCutoutRatio);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float outerAlpha = Mathf.Clamp01(outerRadius + 0.8f - distance);
                float innerAlpha = innerRadius > 0.01f ? Mathf.Clamp01(distance - innerRadius + 0.8f) : 1f;
                float alpha = Mathf.Clamp01(outerAlpha * innerAlpha);
                if (alpha > 0f)
                {
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
        }

        return CreateSpriteFromTexture(texture);
    }

    private static Sprite CreateCoinIconSprite(int size)
    {
        Texture2D texture = CreateTransparentTexture(size);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.44f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);
                if (distance > radius)
                {
                    continue;
                }

                float vertical = Mathf.InverseLerp(0f, size, y);
                Color color = Color.Lerp(new Color(0.88f, 0.42f, 0.04f, 1f), new Color(1f, 0.92f, 0.20f, 1f), vertical);
                if (distance > radius - 5f)
                {
                    color = new Color(0.54f, 0.25f, 0.02f, 1f);
                }
                else if (Mathf.Abs(distance - radius * 0.62f) < 2.1f)
                {
                    color = new Color(1f, 0.98f, 0.48f, 1f);
                }

                Vector2 highlightCenter = center + new Vector2(-radius * 0.25f, radius * 0.25f);
                if (Vector2.Distance(point, highlightCenter) < radius * 0.24f)
                {
                    color = Color.Lerp(color, Color.white, 0.45f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        return CreateSpriteFromTexture(texture);
    }

    private static Sprite CreateGemIconSprite(int size)
    {
        Texture2D texture = CreateTransparentTexture(size);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.43f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - center.x) / radius;
                float ny = (y - center.y) / radius;
                float diamond = Mathf.Abs(nx) + Mathf.Abs(ny);
                if (diamond > 1f)
                {
                    continue;
                }

                Color color = ny > 0.10f
                    ? new Color(0.46f, 1f, 0.76f, 1f)
                    : new Color(0.10f, 0.72f, 0.96f, 1f);
                if (nx > 0.18f)
                {
                    color = Color.Lerp(color, new Color(0.03f, 0.36f, 0.86f, 1f), 0.45f);
                }
                else if (nx < -0.18f)
                {
                    color = Color.Lerp(color, Color.white, 0.18f);
                }

                if (diamond > 0.88f || Mathf.Abs(nx) < 0.025f || Mathf.Abs(nx + ny) < 0.025f || Mathf.Abs(nx - ny) < 0.025f)
                {
                    color = Color.Lerp(color, new Color(0.02f, 0.20f, 0.50f, 1f), diamond > 0.88f ? 0.75f : 0.30f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        return CreateSpriteFromTexture(texture);
    }

    private static Sprite CreatePowerIconSprite(int size)
    {
        Texture2D texture = CreateTransparentTexture(size);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.44f;
        Vector2 bladeStart = new Vector2(size * 0.30f, size * 0.25f);
        Vector2 bladeEnd = new Vector2(size * 0.70f, size * 0.74f);
        Vector2 guardStart = new Vector2(size * 0.26f, size * 0.34f);
        Vector2 guardEnd = new Vector2(size * 0.42f, size * 0.20f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);
                if (distance <= radius)
                {
                    Color color = distance > radius - 4f
                        ? new Color(0.02f, 0.08f, 0.18f, 1f)
                        : Color.Lerp(new Color(0.10f, 0.32f, 0.54f, 1f), new Color(0.20f, 0.68f, 0.95f, 1f), Mathf.InverseLerp(0f, size, y));
                    texture.SetPixel(x, y, color);
                }

                float bladeDistance = DistanceToSegment(point, bladeStart, bladeEnd);
                float guardDistance = DistanceToSegment(point, guardStart, guardEnd);
                if (bladeDistance < 3.2f || guardDistance < 3.0f)
                {
                    texture.SetPixel(x, y, bladeDistance < 1.5f ? Color.white : new Color(0.82f, 0.90f, 0.98f, 1f));
                }

                if (Vector2.Distance(point, bladeEnd) < 5f)
                {
                    texture.SetPixel(x, y, new Color(1f, 0.92f, 0.42f, 1f));
                }
            }
        }

        return CreateSpriteFromTexture(texture);
    }

    private static Texture2D CreateTransparentTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }

        return texture;
    }

    private static Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        texture.Apply(false, true);
        texture.hideFlags = HideFlags.HideAndDontSave;
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float segmentLengthSquared = segment.sqrMagnitude;
        if (segmentLengthSquared <= 0.001f)
        {
            return Vector2.Distance(point, start);
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segmentLengthSquared);
        return Vector2.Distance(point, start + segment * t);
    }

    private void ApplyPanelVisualStyle(GameObject panel, Image image, string name, Color color)
    {
        if (image == null || color.a <= 0.01f)
        {
            return;
        }

        if (!IsFlatUiPanel(name))
        {
            image.sprite = IsPillUiPanel(name) ? GetRoundedPillSprite() : GetRoundedPanelSprite();
            image.type = Image.Type.Sliced;
        }

        if (ShouldDecorateUiPanel(name))
        {
            Shadow shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.38f);
            shadow.effectDistance = new Vector2(0f, -4f);
            shadow.useGraphicAlpha = true;

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.58f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
        }
    }

    private static bool IsFlatUiPanel(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name == "Root"
            || name.Contains("Viewport")
            || name.Contains("World")
            || name.Contains("ConnectorLine")
            || name.Contains("Projectile")
            || name.Contains("DamagePopup");
    }

    private static bool IsPillUiPanel(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.Contains("Pill")
            || name.Contains("Badge")
            || name.Contains("Bar")
            || name.Contains("Fill")
            || name.Contains("Dot");
    }

    private static bool ShouldDecorateUiPanel(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return true;
        }

        return !name.Contains("Fill")
            && !name.Contains("Viewport")
            && !name.Contains("Overlay")
            && !name.Contains("Prompt")
            && !name.Contains("Popup")
            && !name.Contains("World")
            && !name.Contains("Root")
            && !name.Contains("ConnectorLine")
            && !name.Contains("DamagePopup");
    }

    private void AddInsetHighlight(Transform parent, float alpha)
    {
        GameObject highlight = new GameObject("InsetHighlight", typeof(RectTransform), typeof(Image));
        highlight.transform.SetParent(parent, false);
        Image image = highlight.GetComponent<Image>();
        image.sprite = GetRoundedButtonSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, alpha);
        image.raycastTarget = false;

        RectTransform rect = highlight.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.52f);
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(5f, 4f);
        rect.offsetMax = new Vector2(-5f, -5f);
    }

    private void ApplyTextVisualStyle(GameObject textObject, int fontSize)
    {
        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, fontSize >= 24 ? 0.86f : 0.70f);
        outline.effectDistance = fontSize >= 28 ? new Vector2(2.6f, -2.6f) : new Vector2(1.7f, -1.7f);
        outline.useGraphicAlpha = false;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
        shadow.effectDistance = new Vector2(0f, -2f);
        shadow.useGraphicAlpha = false;
    }

    private Image CreateAnchoredIcon(string name, Transform parent, Sprite sprite, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject iconObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(parent, false);
        Image image = iconObject.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        return image;
    }

    private void CreateHeaderResourceDisplay(
        Transform parent,
        string name,
        Sprite iconSprite,
        Vector2 iconPosition,
        out Text valueText)
    {
        CreateAnchoredIcon(name + "Icon", parent, iconSprite, iconPosition, new Vector2(38f, 38f));

        valueText = CreateText(name + "Text", parent, 25, FontStyle.Bold, TextAnchor.MiddleLeft);
        RectTransform textRect = valueText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.5f);
        textRect.anchorMax = new Vector2(0f, 0.5f);
        textRect.pivot = new Vector2(0f, 0.5f);
        textRect.sizeDelta = new Vector2(132f, 46f);
        textRect.anchoredPosition = new Vector2(iconPosition.x + 46f, iconPosition.y);
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        ApplyPanelVisualStyle(panel, image, name, color);
        return panel;
    }

    private GameObject CreateBattleActor(string name, Transform parent, Vector2 size, Color color)
    {
        GameObject actor = CreatePanel(name, parent, color);
        Image image = actor.GetComponent<Image>();
        image.raycastTarget = false;

        RectTransform rect = actor.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return actor;
    }

    private void ConfigureHoldRepeat(Button button, Action action, Func<bool> canRepeat = null)
    {
        if (button == null)
        {
            return;
        }

        HoldRepeatButton repeatButton = button.GetComponent<HoldRepeatButton>();
        if (repeatButton == null)
        {
            repeatButton = button.gameObject.AddComponent<HoldRepeatButton>();
        }

        repeatButton.Configure(action, canRepeat);
    }

    private Button CreateButton(string label, Transform parent, int fontSize, Color color)
    {
        GameObject buttonObject = CreatePanel(label + "Button", parent, color);
        Button button = buttonObject.AddComponent<Button>();
        Image buttonImage = buttonObject.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = GetRoundedButtonSprite();
            buttonImage.type = Image.Type.Sliced;
        }

        AddInsetHighlight(buttonObject.transform, 0.18f);
        SetButtonColor(button, color);

        Text text = CreateText(label + "Text", buttonObject.transform, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 6);
        textRect.offsetMax = new Vector2(-10, -6);
        text.text = label;

        return button;
    }

    private void SetButtonText(Button button, string text)
    {
        if (button == null)
        {
            return;
        }

        Text buttonText = button.GetComponentInChildren<Text>(true);
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    private void SetButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.12f;
        colors.pressedColor = color * 0.84f;
        colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
        button.colors = colors;

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
    }

    private Text CreateText(string name, Transform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        ApplyTextVisualStyle(textObject, fontSize);

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 22);
        return text;
    }

    private void StretchToParent(GameObject target)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private LayoutElement AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
    {
        LayoutElement element = target.AddComponent<LayoutElement>();
        if (preferredWidth > 0)
        {
            element.preferredWidth = preferredWidth;
        }

        if (preferredHeight > 0)
        {
            element.preferredHeight = preferredHeight;
        }

        return element;
    }
}
