using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Battlefield;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Gacha;
using IdleGame.Progression;
using IdleGame.Speed;
using IdleGame.UI.Facility;
using IdleGame.UI.Fortress;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail;
using IdleGame.UI.Hero.Detail.Equipment;
using IdleGame.UI.Hero.Formation;
using IdleGame.UI.Hero.TotemRune;
using IdleGame.UI.Hero.Trait;
using IdleGame.UI.Hero.Transcend;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud : MonoBehaviour
    {
        private const float EnemyDeathVisualSeconds = 0.28f;
        private const float HeroTranscendAutoRollIntervalSeconds = 0.14f;
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
        private readonly GameHudBattleRefs battleHud = new GameHudBattleRefs(EnemyDeathVisualSeconds);
        private readonly HeroHudRefs heroHud = new HeroHudRefs();
        private readonly HudRuntimeTickState runtimeTickState = new HudRuntimeTickState();

        private HudTab activeTab = HudTab.Growth;
        private bool contentPanelOpen = true;
        private HudDirtyFlags dirtyHudFlags = HudDirtyFlags.All;
        private bool hudRefreshQueued;
        private HudTab lastRenderedActiveTab = HudTab.Growth;
        private bool lastRenderedContentPanelOpen = true;
        private bool lastRenderedHeroDetailPanelOpen;
        private bool cachedGrowthAttention;
        private bool cachedHeroAttention;
        private HudWalletSnapshot lastWalletSnapshot;
        private GameObject canvasObject;

        private Text resourceText;
        private Text rubyResourceText;
        private Text accountLevelText;
        private Text supportSummaryText;
        private Text totalCombatPowerText;
        private Text growthNoticeText;
        private Text fieldStagePillText;
        private Text guideQuestText;
        private Image accountExpFill;
        private LayoutElement contentLayoutElement;

        private GameObject contentRoot;
        private GameObject growthPanel;
        private GameObject fortressPanel;
        private GameObject facilityPanel;
        private GameObject heroFacilityContent;
        private GameObject facilityAssignmentModal;
        private Text heroFacilitySummaryText;
        private Text heroFacilityNoticeText;
        private FortressPanelViewRefs fortressViewRefs;
        private GameObject stagePanel;
        private GameObject summonPanel;
        private GameObject shopPanel;
        private GameObject supportPanel;
        private GameObject debugPanel;
        private GameObject facilityRewardPopup;
        private GameObject guideQuestDot;
        private Text gachaText;
        private Text debugText;
        private Text facilityRewardPopupListText;
        private int selectedGrowthLevelStep = 1;
        private string selectedHeroDetailId = string.Empty;
        private readonly HeroFormationUiState heroFormationState = new HeroFormationUiState();
        private readonly HeroFormationPromptTargetState heroFormationPromptTargetState = new HeroFormationPromptTargetState();
        private readonly HeroDetailEquipmentUiState heroDetailEquipmentState = new HeroDetailEquipmentUiState();
        private readonly TotemRuneUiState totemRuneState = new TotemRuneUiState();
        private readonly HeroTranscendUiState heroTranscendState = new HeroTranscendUiState();
        private bool facilityAssignmentModalOpen;
        private string selectedHeroTraitId = "ATK_CORE";
        private HeroPageTab activeHeroPageTab = HeroPageTab.Formation;
        private HeroDetailTab activeHeroDetailTab = HeroDetailTab.BasicInfo;
        private bool heroDetailPanelOpen;

        private readonly Dictionary<AbilityKind, Text> abilityButtonTexts = new Dictionary<AbilityKind, Text>();
        private readonly Dictionary<AbilityKind, Text> abilityCostBadgeTexts = new Dictionary<AbilityKind, Text>();
        private readonly Dictionary<string, Text> facilityCardTexts = new Dictionary<string, Text>();
        private readonly Dictionary<string, Button> facilityUpgradeButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Button> facilityCollectButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Text> facilityAssignmentRowTexts = new Dictionary<string, Text>();
        private readonly Dictionary<string, List<Text>> facilityAssignmentSlotTexts = new Dictionary<string, List<Text>>();
        private readonly Dictionary<string, FacilityCardViewState> facilityCardStates = new Dictionary<string, FacilityCardViewState>();
        private readonly Dictionary<string, string> facilityAssignmentRowTextStates = new Dictionary<string, string>();
        private readonly Dictionary<string, FacilityAssignmentSlotViewState> facilityAssignmentSlotStates = new Dictionary<string, FacilityAssignmentSlotViewState>();
        private readonly Dictionary<AbilityKind, GameObject> abilityNotificationDots = new Dictionary<AbilityKind, GameObject>();
        private readonly Dictionary<int, Button> growthStepButtons = new Dictionary<int, Button>();
        private readonly Dictionary<string, Button> stageButtons = new Dictionary<string, Button>();
        private readonly Dictionary<HudTab, Button> tabButtons = new Dictionary<HudTab, Button>();
        private readonly Dictionary<HudTab, string> tabButtonLabels = new Dictionary<HudTab, string>();
        private readonly Dictionary<HudTab, List<GameObject>> tabNotificationDots = new Dictionary<HudTab, List<GameObject>>();
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
            lastWalletSnapshot = HudWalletSnapshot.Capture(wallet);
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
            if (battleManager == null || battleHud.DamagePopupText == null)
            {
                return;
            }

            bool noticeExpired = runtimeTickState.Tick(battleManager, Time.deltaTime, Time.unscaledTime);
            if (noticeExpired)
            {
                ClearGrowthNoticeTexts();
            }

            if (IsBattlePanelVisible())
            {
                RefreshBattlefieldVisuals();
            }

            RefreshPendingRuneSlotGlow();
        }

        private void LateUpdate()
        {
            if (!hudRefreshQueued)
            {
                return;
            }

            hudRefreshQueued = false;
            UpdateView();
        }

        private void OnDestroy()
        {
            StopHeroTranscendAutoRoll();
            UnsubscribeEvents();
        }

        private void UpdateView()
        {
            if (resourceText == null)
            {
                return;
            }

            HudPanelRefreshState panelState = BuildPanelRefreshState();
            StageDefinition stage = progressManager.CurrentStage;
            RefreshHeaderPanel(panelState, stage);
            RefreshBattlePanel(panelState, stage);
            RefreshVisibleContentPanel(panelState);
            RefreshClosedPanelAttention(panelState);
            RefreshNotifications(panelState);
            RefreshPanelVisibility(panelState);
            RefreshFacilityPanelVisibility(panelState);
            RefreshHeroOverlayPanels(panelState);
            RefreshBottomNavigation(panelState);
            MarkHudRendered();
        }

        private void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Max(0.1f, scale);
            UpdateView();
        }
    }
}
