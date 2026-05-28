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
using IdleGame.UI.Battle;
using IdleGame.UI.Common;
using IdleGame.UI.Debugging;
using IdleGame.UI.Facility;
using IdleGame.UI.Fortress;
using IdleGame.UI.Growth;
using IdleGame.UI.Header;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail;
using IdleGame.UI.Hero.Detail.Equipment;
using IdleGame.UI.Hero.Formation;
using IdleGame.UI.Hero.TotemRune;
using IdleGame.UI.Hero.Trait;
using IdleGame.UI.Hero.Transcend;
using IdleGame.UI.Navigation;
using IdleGame.UI.Summon;

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

        private void SubscribeEvents()
        {
            progressManager.Changed += OnProgressChanged;
            wallet.Changed += OnWalletChanged;
            accountProgressManager.Changed += OnAccountProgressChanged;
            abilityManager.Changed += OnAbilityChanged;
            speedManager.Changed += OnSpeedChanged;
            battleManager.Changed += OnBattleChanged;
            gachaManager.Changed += OnGachaChanged;
            equipmentInventory.Changed += OnEquipmentInventoryChanged;
        }

        private void UnsubscribeEvents()
        {
            if (progressManager != null)
            {
                progressManager.Changed -= OnProgressChanged;
            }

            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }

            if (accountProgressManager != null)
            {
                accountProgressManager.Changed -= OnAccountProgressChanged;
            }

            if (abilityManager != null)
            {
                abilityManager.Changed -= OnAbilityChanged;
            }

            if (speedManager != null)
            {
                speedManager.Changed -= OnSpeedChanged;
            }

            if (battleManager != null)
            {
                battleManager.Changed -= OnBattleChanged;
            }

            if (gachaManager != null)
            {
                gachaManager.Changed -= OnGachaChanged;
            }

            if (equipmentInventory != null)
            {
                equipmentInventory.Changed -= OnEquipmentInventoryChanged;
            }
        }

        private void OnProgressChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Header | HudDirtyFlags.Battle | HudDirtyFlags.Stage | HudDirtyFlags.Navigation);
        }

        private void OnWalletChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Header
                | HudDirtyFlags.Growth
                | HudDirtyFlags.Hero
                | HudDirtyFlags.HeroDetail
                | HudDirtyFlags.Facility
                | HudDirtyFlags.Summon
                | HudDirtyFlags.Debug
                | HudDirtyFlags.Navigation);
        }

        private void OnAccountProgressChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Header
                | HudDirtyFlags.Hero
                | HudDirtyFlags.HeroDetail
                | HudDirtyFlags.Facility
                | HudDirtyFlags.Debug
                | HudDirtyFlags.Navigation);
        }

        private void OnAbilityChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Battle | HudDirtyFlags.Growth | HudDirtyFlags.Navigation);
        }

        private void OnSpeedChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Battle | HudDirtyFlags.Debug);
        }

        private void OnBattleChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Header
                | HudDirtyFlags.Battle
                | HudDirtyFlags.Fortress
                | HudDirtyFlags.Facility
                | HudDirtyFlags.Support
                | HudDirtyFlags.Debug
                | HudDirtyFlags.Navigation);
        }

        private void OnGachaChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Summon | HudDirtyFlags.Hero | HudDirtyFlags.HeroDetail | HudDirtyFlags.Navigation);
        }

        private void OnEquipmentInventoryChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Summon | HudDirtyFlags.Hero | HudDirtyFlags.HeroDetail | HudDirtyFlags.Navigation);
        }

        private void QueueHudRefresh(HudDirtyFlags flags)
        {
            dirtyHudFlags |= flags;
            hudRefreshQueued = true;
        }


        private void UpdateView()
        {
            if (resourceText == null)
            {
                return;
            }

            HudPanelRefreshState panelState = HudPanelRefreshState.Build(new HudPanelRefreshStateBuildArgs
            {
                DirtyFlags = dirtyHudFlags,
                ActiveTab = activeTab,
                ContentPanelOpen = contentPanelOpen,
                HeroDetailPanelOpen = heroDetailPanelOpen,
                LastRenderedActiveTab = lastRenderedActiveTab,
                LastRenderedContentPanelOpen = lastRenderedContentPanelOpen,
                LastRenderedHeroDetailPanelOpen = lastRenderedHeroDetailPanelOpen
            });

            StageDefinition stage = progressManager.CurrentStage;
            if (panelState.RefreshHeader)
            {
                HeaderHudPresenter.Refresh(new HeaderHudPresenterArgs
                {
                    Wallet = wallet,
                    AccountProgressManager = accountProgressManager,
                    BattleManager = battleManager,
                    Stage = stage,
                    ResourceText = resourceText,
                    RubyResourceText = rubyResourceText,
                    StageText = battleHud.StageText,
                    ModeText = battleHud.ModeText,
                    AccountLevelText = accountLevelText,
                    FieldStagePillText = fieldStagePillText,
                    AccountExpFill = accountExpFill,
                    FormatGameNumber = value => FormatShortNumber(value),
                    FormatDoubleNumber = value => FormatShortNumber(value),
                    FormatCountNumber = FormatCountNumber
                });
            }

            if (panelState.RefreshBattlePanel)
            {
                BattleProgressPresenter.Refresh(new BattleProgressPresenterArgs
                {
                    BattleManager = battleManager,
                    Stage = stage,
                    TargetText = battleHud.TargetText,
                    ProgressFill = battleHud.HpFill,
                    ProgressValueText = battleHud.HpText,
                    ProgressText = battleHud.ProgressText,
                    GuideQuestText = guideQuestText
                });

                RefreshDamageMeter();

                BattleLogPresenter.Refresh(new BattleLogPresenterArgs
                {
                    BattleManager = battleManager,
                    SupportText = battleHud.SupportText,
                    LogText = battleHud.LogText,
                    RewardText = battleHud.RewardText
                });

                RefreshBattlefieldVisuals();

                BattleControlPresenter.Refresh(new BattleControlPresenterArgs
                {
                    BattleManager = battleManager,
                    SpeedManager = speedManager,
                    SkillAutoButton = battleHud.SkillAutoButton,
                    FeverAutoButton = battleHud.FeverAutoButton,
                    SpeedCycleButton = battleHud.SpeedCycleButton
                });
            }

            if (panelState.RefreshFortressPanel)
            {
                RefreshFortressPanel();
            }

            if (panelState.RefreshSummonPanel)
            {
                SummonPanelPresenter.Refresh(new SummonPanelPresenterArgs
                {
                    GachaManager = gachaManager,
                    EquipmentInventory = equipmentInventory,
                    ResultText = gachaText,
                    RefreshPanel = true
                });
            }

            if (panelState.GrowthPanelOpen)
            {
                HudStatusPresenter.Refresh(new HudStatusPresenterArgs
                {
                    BattleManager = battleManager,
                    TotalCombatPowerText = totalCombatPowerText,
                    GrowthNoticeText = growthNoticeText,
                    GrowthNoticeMessage = runtimeTickState.NoticeMessage,
                    GrowthNoticeUntil = runtimeTickState.NoticeUntil,
                    CurrentTime = Time.unscaledTime,
                    FormatShortNumber = value => FormatShortNumber(value)
                });
            }

            if (panelState.RefreshGrowthAttention)
            {
                cachedGrowthAttention = GrowthPanelPresenter.Refresh(new GrowthPanelPresenterArgs
                {
                    AbilityManager = abilityManager,
                    Wallet = wallet,
                    SelectedGrowthLevelStep = selectedGrowthLevelStep,
                    RefreshPanel = panelState.RefreshGrowthPanel,
                    GrowthStepButtons = growthStepButtons,
                    AbilityButtonTexts = abilityButtonTexts,
                    AbilityCostBadgeTexts = abilityCostBadgeTexts,
                    AbilityNotificationDots = abilityNotificationDots,
                    FormatShortNumber = FormatShortNumber
                });
            }

            if (panelState.RefreshHeroPanel)
            {
                RefreshHeroFormationPanel();
            }

            if (panelState.RefreshHeroAttention)
            {
                cachedHeroAttention = HeroRosterPresenter.Refresh(new HeroRosterPresenterArgs
                {
                    BattleManager = battleManager,
                    Wallet = wallet,
                    RefreshPanel = panelState.RefreshHeroPanel,
                    SelectedHeroForPlacement = heroFormationState.SelectedHeroForPlacement,
                    HeroRosterButtons = heroHud.RosterButtons,
                    HeroButtonTexts = heroHud.HeroButtonTexts,
                    HeroRosterActionButtons = heroHud.RosterActionButtons,
                    HeroRosterDeployedOverlays = heroHud.RosterDeployedOverlays,
                    HeroNotificationDots = heroHud.NotificationDots,
                    IsHeroInFormation = IsHeroInEditingFormation,
                    FormatShortNumber = FormatShortNumber,
                    GetShortHeroLabel = GetShortHeroLabel
                });
            }

            if (panelState.RefreshSupportPanel)
            {
                SecondaryPanelView.ApplySupportPanelState(
                    supportSummaryText,
                    battleManager.Skills,
                    battleManager.Pets,
                    battleManager.PartyAttackPower,
                    battleManager.PetGoldBonusPercent,
                    value => FormatShortNumber(value),
                    heroHud.SkillStatusTexts,
                    heroHud.PetStatusTexts);
            }

            if (panelState.RefreshStagePanel)
            {
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
            }

            if (panelState.RefreshDebugPanel)
            {
                DebugPanelPresenter.Refresh(new DebugPanelPresenterArgs
                {
                    RefreshPanel = true,
                    StatusText = debugText,
                    SpeedManager = speedManager,
                    Wallet = wallet,
                    AccountProgressManager = accountProgressManager,
                    ProgressManager = progressManager,
                    BattleManager = battleManager,
                    FormatShortNumber = value => FormatShortNumber(value),
                    FormatCountNumber = FormatCountNumber
                });
            }

            if (panelState.RefreshNotifications)
            {
                HudNotificationDotPresenter.Refresh(new HudNotificationDotPresenterArgs
                {
                    TabNotificationDots = tabNotificationDots,
                    GuideQuestDot = guideQuestDot,
                    Wallet = wallet,
                    ProgressManager = progressManager,
                    BattleManager = battleManager,
                    HasGrowthAttention = cachedGrowthAttention,
                    HasHeroAttention = cachedHeroAttention
                });
            }

            HudPanelVisibilityPresenter.Refresh(new HudPanelVisibilityPresenterArgs
            {
                PanelState = panelState,
                BattlePanel = battleHud.Panel,
                BattleLayoutElement = battleHud.LayoutElement,
                ContentRoot = contentRoot,
                ContentLayoutElement = contentLayoutElement,
                GrowthPanel = growthPanel,
                HeroPanel = heroHud.Panel,
                FortressPanel = fortressPanel,
                FacilityPanel = facilityPanel,
                StagePanel = stagePanel,
                SummonPanel = summonPanel,
                ShopPanel = shopPanel,
                SupportPanel = supportPanel,
                DebugPanel = debugPanel,
                HeroFacilityContent = heroFacilityContent
            });

            if (panelState.FacilityPanelOpen)
            {
                if (panelState.RefreshFacilityPanel)
                {
                    RefreshHeroFacilityPanel();
                }
            }
            else
            {
                facilityAssignmentModalOpen = false;
            }

            if (heroHud.FormationSavePrompt != null)
            {
                heroHud.FormationSavePrompt.SetActive(heroFormationState.SavePromptOpen);
            }

            if (heroHud.DetailPanel != null)
            {
                heroHud.DetailPanel.SetActive(heroDetailPanelOpen);
                if (panelState.RefreshHeroDetailPanel)
                {
                    RefreshHeroDetailPanel();
                }
            }

            if (panelState.RefreshNavigation)
            {
                HudBottomNavPresenter.Refresh(new HudBottomNavPresenterArgs
                {
                    TabButtons = tabButtons,
                    TabButtonLabels = tabButtonLabels,
                    ActiveTab = activeTab,
                    ContentPanelOpen = contentPanelOpen,
                    HeroDetailPanelOpen = heroDetailPanelOpen
                });
            }

            hudRefreshQueued = false;
            dirtyHudFlags = HudDirtyFlags.None;
            lastRenderedActiveTab = activeTab;
            lastRenderedContentPanelOpen = contentPanelOpen;
            lastRenderedHeroDetailPanelOpen = heroDetailPanelOpen;
        }

        private void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Max(0.1f, scale);
            UpdateView();
        }

        private void RefreshFortressPanel()
        {
            if (battleManager == null || fortressViewRefs == null)
            {
                return;
            }

            FortressPanelView.ApplyState(
                fortressViewRefs,
                FortressPanelStateBuilder.Build(
                    battleManager,
                    value => FormatShortNumber(value),
                    value => FormatShortNumber(value)));
        }

        private void RefreshDamageMeter()
        {
            battleHud.DamageMeterRowStates = DamageMeterStateBuilder.BuildRows(
                battleManager.DeployedHeroes,
                battleManager.GetHeroDamageDone,
                battleManager.GetMaxHeroDamageDone(),
                battleHud.DamageMeterRows.Count,
                GetShortHeroLabel,
                value => FormatShortNumber(value),
                battleHud.DamageMeterHeroScratch,
                battleHud.DamageMeterRowStates);

            DamageMeterView.Apply(
                battleHud.DamageMeterText,
                battleHud.DamageMeterRowStates,
                battleHud.DamageMeterRows,
                battleHud.DamageMeterFills,
                battleHud.DamageMeterRowTexts);
        }

        private void RefreshHeroFacilityPanel()
        {
            FacilityPanelPresenter.Refresh(new FacilityPanelPresenterArgs
            {
                BattleManager = battleManager,
                Wallet = wallet,
                AssignmentModalOpen = facilityAssignmentModalOpen && activeTab == HudTab.Facility,
                SummaryText = heroFacilitySummaryText,
                AssignmentModal = facilityAssignmentModal,
                FacilityCardTexts = facilityCardTexts,
                FacilityUpgradeButtons = facilityUpgradeButtons,
                FacilityCollectButtons = facilityCollectButtons,
                AssignmentRowTexts = facilityAssignmentRowTexts,
                AssignmentSlotTexts = facilityAssignmentSlotTexts,
                FormatShortNumber = FormatShortNumber,
                FormatCountNumber = FormatCountNumber,
                FindHeroState = FindHeroState,
                GetShortHeroLabel = GetShortHeroLabel
            });
        }

        private void OpenFacilityAssignmentModal()
        {
            facilityAssignmentModalOpen = true;
            UpdateView();
        }

        private void CloseFacilityAssignmentModal()
        {
            facilityAssignmentModalOpen = false;
            UpdateView();
        }

        private void CollectFacilityFromHud(string facilityId)
        {
            ApplyFacilityActionResult(FacilityActionService.TryCollectFacility(battleManager, facilityId));
            UpdateView();
        }

        private void CollectAllFacilitiesFromHud()
        {
            ApplyFacilityActionResult(FacilityActionService.TryCollectAllFacilities(battleManager, FormatShortNumber));
            UpdateView();
        }

        private void ShowFacilityRewardPopup(List<string> rewardLines)
        {
            if (facilityRewardPopup == null || facilityRewardPopupListText == null)
            {
                return;
            }

            facilityRewardPopupListText.text = rewardLines == null || rewardLines.Count == 0
                ? "수령한 보상이 없습니다."
                : string.Join("\n", rewardLines);
            facilityRewardPopup.SetActive(true);
        }

        private void CloseFacilityRewardPopup()
        {
            if (facilityRewardPopup != null)
            {
                facilityRewardPopup.SetActive(false);
            }
        }

        private void UpgradeFacilityFromHud(string facilityId)
        {
            ApplyFacilityActionResult(FacilityActionService.TryUpgradeFacility(battleManager, facilityId));
            UpdateView();
        }

        private void AutoAssignAllFacilitiesFromHud()
        {
            ApplyFacilityActionResult(FacilityActionService.AutoAssignAllFacilities(battleManager));
            UpdateView();
        }

        private void ClearAllFacilityAssignmentsFromHud()
        {
            ApplyFacilityActionResult(FacilityActionService.ClearAllFacilityAssignments(battleManager));
            UpdateView();
        }

        private void RefreshHeroTraitPanel()
        {
            HeroTraitPanelPresenterResult result = HeroTraitPanelPresenter.Refresh(new HeroTraitPanelPresenterArgs
            {
                AccountProgressManager = accountProgressManager,
                SelectedTalentId = selectedHeroTraitId,
                SummaryText = heroHud.TraitSummaryText,
                DetailText = heroHud.TraitDetailText,
                LevelUpButton = heroHud.TraitLevelUpButton,
                ButtonTexts = heroHud.TraitButtonTexts,
                Buttons = heroHud.TraitButtons,
                FormatShortNumber = FormatShortNumber
            });
            selectedHeroTraitId = result.SelectedTalentId;
        }

        private void LevelUpSelectedHeroTrait()
        {
            GrowthActionResult result = GrowthActionService.TryLevelUpTalent(accountProgressManager, selectedHeroTraitId);
            ApplyGrowthActionResult(result);
            if (result != null && result.Success)
            {
                UpdateView();
            }
        }

        private bool CanLevelUpSelectedHeroTrait()
        {
            return GrowthActionService.CanLevelUpTalent(accountProgressManager, selectedHeroTraitId);
        }

        private void LevelUpFortressFromHud()
        {
            GrowthActionResult result = GrowthActionService.TryLevelUpFortress(battleManager);
            ApplyGrowthActionResult(result);
            if (result != null && result.Success)
            {
                UpdateView();
            }
        }

        private bool CanLevelUpFortressFromHud()
        {
            return GrowthActionService.CanLevelUpFortress(battleManager);
        }

        private void TryLevelUpAbilityFromHud(AbilityKind kind)
        {
            ApplyGrowthActionResult(GrowthActionService.TryLevelUpAbility(
                abilityManager,
                wallet,
                kind,
                selectedGrowthLevelStep));
        }

        private bool CanLevelUpAbilityFromHud(AbilityKind kind)
        {
            return GrowthActionService.CanLevelUpAbility(
                abilityManager,
                wallet,
                kind,
                selectedGrowthLevelStep);
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

        private void ApplyGrowthActionResult(GrowthActionResult result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }
        }

        private void ApplyFacilityActionResult(FacilityActionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.ShowRewardPopup)
            {
                ShowFacilityRewardPopup(result.RewardLines);
                return;
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }
        }

        private void ShowGrowthNotice(string message)
        {
            runtimeTickState.ShowNotice(message, Time.unscaledTime, 1.6f);
            SetGrowthNoticeTexts(message);
        }

        private void SetGrowthNoticeTexts(string message)
        {
            if (growthNoticeText != null)
            {
                growthNoticeText.text = message;
            }

            if (heroHud.DetailViewRefs != null && heroHud.DetailViewRefs.NoticeText != null)
            {
                heroHud.DetailViewRefs.NoticeText.text = message;
            }

            if (heroHud.DetailTranscendNoticeText != null)
            {
                heroHud.DetailTranscendNoticeText.text = message;
            }

            SetHeroDetailEquipmentNoticeText(message);

            if (heroFacilityNoticeText != null)
            {
                heroFacilityNoticeText.text = message;
            }
        }

        private void ClearGrowthNoticeTexts()
        {
            SetGrowthNoticeTexts(string.Empty);
        }

        private void SetHeroDetailEquipmentNoticeText(string message)
        {
            if (heroHud.DetailViewRefs == null)
            {
                return;
            }

            if (heroHud.DetailViewRefs.EquipmentDetailNoticeText != null)
            {
                heroHud.DetailViewRefs.EquipmentDetailNoticeText.text = message;
            }

            if (heroHud.DetailViewRefs.EquipmentDismantleNoticeText != null)
            {
                heroHud.DetailViewRefs.EquipmentDismantleNoticeText.text = message;
            }

            if (heroHud.DetailViewRefs.EquipmentBulkDismantleNoticeText != null)
            {
                heroHud.DetailViewRefs.EquipmentBulkDismantleNoticeText.text = message;
            }
        }

        private void RefreshBattlefieldVisuals()
        {
            battleHud.Renderer.Refresh(new BattleHudRenderArgs
            {
                ProgressManager = progressManager,
                BattleManager = battleManager,
                BattlefieldWorldView = battlefieldWorldView,
                VisualState = battleHud.VisualState,
                BattlefieldRect = battleHud.BattlefieldRect,
                BattlefieldWorldImage = battleHud.BattlefieldWorldImage,
                DamagePopupText = battleHud.DamagePopupText,
                CenterSpawnText = battleHud.CenterSpawnText,
                HeroBattleImages = battleHud.HeroImages,
                HeroBattleTexts = battleHud.HeroTexts,
                HeroBattleRects = battleHud.HeroRects,
                EnemyBattleImages = battleHud.EnemyImages,
                EnemyBattleTexts = battleHud.EnemyTexts,
                EnemyBattleRects = battleHud.EnemyRects,
                EnemyHpBarObjects = battleHud.EnemyHpBars,
                EnemyHpFillImages = battleHud.EnemyHpFills,
                HitFlashRemaining = runtimeTickState.HitFlashRemaining,
                HeroAttackFlashRemaining = runtimeTickState.HeroAttackFlashRemaining,
                Time = Time.time,
                DeltaTime = Time.deltaTime
            });
        }

        private bool IsBattlePanelVisible()
        {
            return !contentPanelOpen || activeTab == HudTab.Growth;
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
                    return "시설";
                default:
                    return tab.ToString();
            }
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

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            return HudUiFactory.CreatePanel(name, parent, color);
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
            return HudUiFactory.CreateButton(label, parent, fontSize, color);
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
            HudUiFactory.SetButtonColor(button, color);
        }

        private Text CreateText(string name, Transform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment)
        {
            return HudUiFactory.CreateText(name, parent, fontSize, fontStyle, alignment);
        }

        private void StretchToParent(GameObject target)
        {
            HudUiFactory.StretchToParent(target);
        }

        private LayoutElement AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
        {
            return HudUiFactory.AddLayoutElement(target, preferredWidth, preferredHeight);
        }
    }
}
