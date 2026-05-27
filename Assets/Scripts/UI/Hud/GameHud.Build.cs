using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battlefield;
using IdleGame.Data;
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
using IdleGame.UI.Hero.Formation;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
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
            CreateFacilityRewardPopup(root.transform);
        }

        private void CreateHeader(Transform parent)
        {
            HeaderHudViewRefs refs = HeaderHudView.Build(new HeaderHudViewBuildArgs
            {
                Parent = parent,
                ShowDebugGrantButton = IsDebugPanelEnabled(),
                OnDebugGrant = DebugGrantTestCurrency
            });

            battleHud.StageText = refs.StageText;
            battleHud.ModeText = refs.ModeText;
            resourceText = refs.ResourceText;
            rubyResourceText = refs.RubyResourceText;
            accountLevelText = refs.AccountLevelText;
            accountExpFill = refs.AccountExpFill;
        }

        private void CreateHeroFormationSavePrompt(Transform parent)
        {
            heroHud.FormationSavePrompt = HeroFormationSavePromptView.Build(
                parent,
                ConfirmHeroFormationSavePrompt,
                CancelHeroFormationSavePrompt);
        }

        private void CreateFacilityRewardPopup(Transform parent)
        {
            FacilityRewardPopupRefs refs = FacilityView.BuildRewardPopup(parent, CloseFacilityRewardPopup);
            facilityRewardPopup = refs.Popup;
            facilityRewardPopupListText = refs.ListText;
        }

        private void CreateHeroDetailPanel(Transform parent)
        {
            heroHud.DetailViewRefs = HeroDetailView.Build(new HeroDetailViewBuildArgs
            {
                Parent = parent,
                OnToggleFormation = ToggleSelectedHeroDetailFormation,
                OnLevelUpHero = LevelUpSelectedHeroDetail,
                CanLevelUpHero = CanLevelUpSelectedHeroDetail,
                OnStarUpHero = StarUpSelectedHeroDetail,
                OnPlaceEquipmentSlot = TryPlaceSelectedHeroDetailEquipment,
                OnRemoveEquipmentSlot = RemoveHeroDetailEquipment,
                OnToggleEquipmentFilter = ToggleHeroDetailEquipmentFilter,
                OnOpenEquipmentDismantle = OpenEquipmentDismantlePopup,
                OnUnequipAllEquipment = UnequipAllHeroDetailEquipment,
                OnAutoEquipEquipment = AutoEquipHeroDetailEquipment,
                OnSelectTranscendSlot = slot =>
                {
                    heroTranscendState.SelectSlot(slot);
                    UpdateView();
                },
                OnToggleTranscendSlotLock = ToggleHeroTranscendSlotLock,
                OnToggleTranscendStopMode = ToggleHeroTranscendStopMode,
                OnRollTranscendManual = RollSelectedHeroTranscendManual,
                OnAutoRollTranscend = AutoRollSelectedHeroTranscend,
                OnSelectTab = SelectHeroDetailTab,
                OnConfirmTranscendRollPrompt = ConfirmHeroTranscendRollPrompt,
                OnCancelTranscendRollPrompt = CancelHeroTranscendRollPrompt,
                OnToggleSelectedEquipmentDetailEquip = ToggleSelectedEquipmentDetailEquip,
                OnLevelUpSelectedEquipmentDetail = LevelUpSelectedEquipmentDetail,
                CanLevelUpSelectedEquipmentDetail = CanLevelUpSelectedEquipmentDetail,
                OnStarUpSelectedEquipmentDetail = StarUpSelectedEquipmentDetail,
                OnCloseEquipmentDetailPopup = CloseEquipmentDetailPopup,
                OnDismantleSelectedEquipment = DismantleSelectedEquipment,
                OnOpenEquipmentBulkDismantlePrompt = OpenEquipmentBulkDismantlePrompt,
                OnCloseEquipmentDismantlePopup = CloseEquipmentDismantlePopup,
                OnChangeBulkDismantleRarity = ChangeBulkDismantleRarity,
                OnConfirmBulkDismantleEquipment = ConfirmBulkDismantleEquipment,
                OnCloseEquipmentBulkDismantlePrompt = CloseEquipmentBulkDismantlePrompt,
                TabButtons = heroHud.DetailTabButtons,
                EquipmentSlotButtons = heroHud.DetailEquipmentSlotButtons,
                EquipmentSlotTexts = heroHud.DetailEquipmentSlotTexts,
                EquipmentSlotRemoveButtons = heroHud.DetailEquipmentSlotRemoveButtons,
                EquipmentFilterButtons = heroHud.DetailEquipmentFilterButtons,
                DismantleFilterButtons = heroHud.EquipmentDismantleFilterButtons,
                SelectedEquipmentSlots = heroDetailEquipmentState.SelectedSlots,
                TranscendSlotButtons = heroHud.DetailTranscendSlotButtons,
                TranscendSlotTexts = heroHud.DetailTranscendSlotTexts,
                TranscendLockButtons = heroHud.DetailTranscendLockButtons
            });

            HeroDetailViewRefs refs = heroHud.DetailViewRefs;
            heroHud.DetailPanel = refs.Panel;
            heroHud.DetailStatsPanel = refs.StatsPanel;
            heroHud.DetailActionRow = refs.ActionRow;
            heroHud.DetailEquipmentContent = refs.EquipmentContent;
            heroHud.DetailTranscendContent = refs.TranscendContent;
            heroHud.TranscendConfirmPrompt = refs.TranscendConfirmPrompt;
            heroHud.DetailTranscendText = refs.TranscendText;
            heroHud.DetailTranscendNoticeText = refs.TranscendNoticeText;
            heroHud.TranscendConfirmMessageText = refs.TranscendConfirmMessageText;
            heroHud.DetailExcludeButton = refs.ExcludeButton;
            heroHud.DetailLevelUpButton = refs.LevelUpButton;
            heroHud.DetailStarUpButton = refs.StarUpButton;
            heroHud.DetailTranscendChangeButton = refs.TranscendChangeButton;
            heroHud.DetailTranscendAutoButton = refs.TranscendAutoButton;
            heroHud.DetailTranscendStopButton = refs.TranscendStopButton;
        }

        private void CreateBattlePanel(Transform parent)
        {
            BattlePanelViewRefs refs = BattlePanelView.Build(new BattlePanelViewBuildArgs
            {
                Parent = parent,
                BattlefieldWorldView = battlefieldWorldView,
                VisualState = battleHud.VisualState,
                HeroBattleImages = battleHud.HeroImages,
                HeroBattleTexts = battleHud.HeroTexts,
                HeroBattleRects = battleHud.HeroRects,
                EnemyBattleImages = battleHud.EnemyImages,
                EnemyBattleTexts = battleHud.EnemyTexts,
                EnemyBattleRects = battleHud.EnemyRects,
                EnemyHpBarObjects = battleHud.EnemyHpBars,
                EnemyHpFillImages = battleHud.EnemyHpFills,
                DamageMeterRows = battleHud.DamageMeterRows,
                DamageMeterFills = battleHud.DamageMeterFills,
                DamageMeterRowTexts = battleHud.DamageMeterRowTexts,
                OnToggleSkillAuto = () => battleManager?.ToggleSkillAuto(),
                OnToggleFeverAuto = () => battleManager?.ToggleFeverAuto(),
                OnCycleSpeed = () => speedManager?.CycleSpeed()
            });

            battleHud.Panel = refs.Panel;
            battleHud.LayoutElement = refs.LayoutElement;
            battleHud.TargetText = refs.TargetText;
            battleHud.HpFill = refs.KillProgressFill;
            battleHud.HpText = refs.KillProgressText;
            battleHud.ProgressText = refs.ProgressText;
            battleHud.SupportText = refs.SupportText;
            battleHud.LogText = refs.LogText;
            battleHud.RewardText = refs.RewardText;
            battleHud.SkillAutoButton = refs.SkillAutoButton;
            battleHud.FeverAutoButton = refs.FeverAutoButton;
            battleHud.SpeedCycleButton = refs.SpeedCycleButton;
            battleHud.BattlefieldRect = refs.BattlefieldRect;
            battleHud.BattlefieldWorldImage = refs.BattlefieldWorldImage;
            battleHud.CenterSpawnText = refs.CenterSpawnText;
            fieldStagePillText = refs.FieldStagePillText;
            battleHud.DamagePopupText = refs.DamagePopupText;
            battleHud.DamageMeterText = refs.DamageMeterText;
            guideQuestText = refs.GuideQuestText;
            guideQuestDot = refs.GuideQuestDot;
        }

        private void CreateContentPanels(Transform parent)
        {
            ContentPanelsViewRefs refs = ContentPanelsView.Build(new ContentPanelsViewBuildArgs
            {
                Parent = parent,
                ShowDebugPanel = IsDebugPanelEnabled()
            });

            contentRoot = refs.Root;
            contentLayoutElement = refs.LayoutElement;
            growthPanel = refs.GrowthPanel;
            heroHud.Panel = refs.HeroPanel;
            fortressPanel = refs.FortressPanel;
            facilityPanel = refs.FacilityPanel;
            stagePanel = refs.StagePanel;
            summonPanel = refs.SummonPanel;
            shopPanel = refs.ShopPanel;
            supportPanel = refs.SupportPanel;
            debugPanel = refs.DebugPanel;

            CreateGrowthPanel(growthPanel.transform);
            CreateHeroPanel(heroHud.Panel.transform);
            CreateFortressPanelV2(fortressPanel.transform);
            CreateHeroFacilityContent(facilityPanel.transform);
            CreateStagePanel(stagePanel.transform);
            CreateSummonPanel(summonPanel.transform);
            CreateShopPanel(shopPanel.transform);
            CreateSupportPanel(supportPanel.transform);

            if (debugPanel != null)
            {
                CreateDebugPanel(debugPanel.transform);
            }
        }

        private void CreateGrowthPanel(Transform parent)
        {
            GrowthPanelViewRefs refs = GrowthPanelView.Build(new GrowthPanelViewBuildArgs
            {
                Parent = parent,
                Abilities = abilityManager.States,
                OnSelectLevelStep = step =>
                {
                    selectedGrowthLevelStep = step;
                    UpdateView();
                },
                OnLevelUpAbility = TryLevelUpAbilityFromHud,
                CanLevelUpAbility = CanLevelUpAbilityFromHud,
                GrowthStepButtons = growthStepButtons,
                AbilityButtonTexts = abilityButtonTexts,
                AbilityCostBadgeTexts = abilityCostBadgeTexts,
                AbilityNotificationDots = abilityNotificationDots
            });

            totalCombatPowerText = refs.TotalCombatPowerText;
            growthNoticeText = refs.GrowthNoticeText;
        }

        private void CreateFortressPanelV2(Transform parent)
        {
            fortressViewRefs = FortressPanelView.Build(new FortressPanelViewBuildArgs
            {
                Parent = parent,
                OnLevelUp = LevelUpFortressFromHud,
                CanLevelUp = CanLevelUpFortressFromHud
            });
        }

        private void CreateHeroPanel(Transform parent)
        {
            HeroPanelView.BuildHeader(parent);

            HeroFormationViewRefs formationRefs = HeroFormationView.Build(new HeroFormationViewBuildArgs
            {
                Parent = parent,
                RosterHeroes = GetSortedHeroRosterDefinitions(),
                GetRarityColor = HeroUiText.GetRarityColor,
                OnFormationSlotClick = TryPlaceSelectedHeroInSlot,
                OnFormationSlotRemove = RemoveHeroFromEditingFormationSlot,
                OnPresetClick = RequestHeroPresetChange,
                OnRuneSlotClick = HandleFormationRuneSlotClick,
                OnRuneSlotRemove = RemoveRuneFromFormationSlot,
                OnHeroCardClick = OpenHeroDetailPanel,
                OnHeroRosterActionClick = SelectOrRemoveRosterHero,
                OnAutoArrange = AutoArrangeEditingFormation,
                OnBulkStarUp = BulkStarUpHeroesFromHud,
                PresetButtons = heroHud.PresetButtons,
                FormationSlotButtons = heroHud.FormationSlotButtons,
                FormationSlotRemoveButtons = heroHud.FormationSlotRemoveButtons,
                RuneSlotButtons = heroHud.FormationRuneSlotButtons,
                RuneSlotTexts = heroHud.FormationRuneSlotTexts,
                RuneSlotRemoveButtons = heroHud.FormationRuneSlotRemoveButtons,
                HeroRosterButtons = heroHud.RosterButtons,
                HeroButtonTexts = heroHud.HeroButtonTexts,
                HeroRosterActionButtons = heroHud.RosterActionButtons,
                HeroRosterDeployedOverlays = heroHud.RosterDeployedOverlays,
                HeroNotificationDots = heroHud.NotificationDots,
                FormationSlotTexts = heroHud.FormationSlotTexts
            });

            heroHud.FormationContent = formationRefs.Content;
            heroHud.FormationSummaryText = formationRefs.SummaryText;
            heroHud.OwnedEffectText = formationRefs.OwnedEffectText;
            heroHud.RosterGridRect = formationRefs.RosterGridRect;
            CreateHeroSubContent(parent);

            HeroPanelViewRefs heroPanelRefs = HeroPanelView.BuildFooter(new HeroPanelViewBuildFooterArgs
            {
                Parent = parent,
                OnTabClick = RequestHeroPageTabChange,
                TabButtons = heroHud.PageTabButtons
            });
            heroHud.PlaceholderText = heroPanelRefs.PlaceholderText;
        }

        private void CreateHeroSubContent(Transform parent)
        {
            HeroSubContentViewRefs refs = HeroSubContentView.Build(new HeroSubContentViewBuildArgs
            {
                Parent = parent,
                Totems = GameData.Totems,
                Runes = GameData.Runes,
                OnTalentSelected = talentId =>
                {
                    selectedHeroTraitId = talentId;
                    UpdateView();
                },
                OnTraitLevelUp = LevelUpSelectedHeroTrait,
                CanTraitLevelUp = CanLevelUpSelectedHeroTrait,
                OnSelectTotem = SelectTotem,
                OnTotemAction = totemId =>
                {
                    totemRuneState.SetResolvedTotem(totemId);
                    EquipSelectedTotem();
                },
                OnEquipSelectedTotem = EquipSelectedTotem,
                OnLevelUpTotem = LevelUpSelectedTotem,
                CanLevelUpTotem = CanLevelUpSelectedTotem,
                OnSelectRune = SelectRune,
                OnRuneAction = StartPendingRuneEquip,
                OnEquipSelectedRune = EquipSelectedRune,
                OnLevelUpRune = LevelUpSelectedRune,
                CanLevelUpRune = CanLevelUpSelectedRune,
                TalentButtons = heroHud.TraitButtons,
                TalentButtonTexts = heroHud.TraitButtonTexts,
                TotemButtons = heroHud.TotemButtons,
                TotemButtonTexts = heroHud.TotemButtonTexts,
                TotemActionButtons = heroHud.TotemActionButtons,
                RuneButtons = heroHud.RuneButtons,
                RuneButtonTexts = heroHud.RuneButtonTexts,
                RuneActionButtons = heroHud.RuneActionButtons
            });

            heroHud.TraitContent = refs.TraitContent;
            heroHud.TraitSummaryText = refs.TraitSummaryText;
            heroHud.TraitDetailText = refs.TraitDetailText;
            heroHud.TraitLevelUpButton = refs.TraitLevelUpButton;
            heroHud.TotemContent = refs.TotemContent;
            heroHud.TotemSummaryText = refs.TotemSummaryText;
            heroHud.TotemDetailText = refs.TotemDetailText;
            heroHud.TotemEquipButton = refs.TotemEquipButton;
            heroHud.TotemLevelUpButton = refs.TotemLevelUpButton;
            heroHud.RuneContent = refs.RuneContent;
            heroHud.RuneSummaryText = refs.RuneSummaryText;
            heroHud.RuneDetailText = refs.RuneDetailText;
            heroHud.RuneEquipButton = refs.RuneEquipButton;
            heroHud.RuneLevelUpButton = refs.RuneLevelUpButton;
        }

        private void CreateHeroFacilityContent(Transform parent)
        {
            FacilityViewRefs refs = FacilityView.Build(new FacilityViewBuildArgs
            {
                Parent = parent,
                Facilities = GameData.Facilities,
                OnCollectFacility = CollectFacilityFromHud,
                OnUpgradeFacility = UpgradeFacilityFromHud,
                OnOpenAssignments = OpenFacilityAssignmentModal,
                OnCollectAll = CollectAllFacilitiesFromHud,
                OnCloseAssignments = CloseFacilityAssignmentModal,
                OnAutoAssignAll = AutoAssignAllFacilitiesFromHud,
                OnClearAssignments = ClearAllFacilityAssignmentsFromHud,
                FacilityCardTexts = facilityCardTexts,
                FacilityUpgradeButtons = facilityUpgradeButtons,
                FacilityCollectButtons = facilityCollectButtons,
                AssignmentRowTexts = facilityAssignmentRowTexts,
                AssignmentSlotTexts = facilityAssignmentSlotTexts
            });

            heroFacilityContent = refs.Content;
            heroFacilitySummaryText = refs.SummaryText;
            heroFacilityNoticeText = refs.NoticeText;
            facilityAssignmentModal = refs.AssignmentModal;
        }
        private List<HeroDefinition> GetSortedHeroRosterDefinitions()
        {
            return HeroFormationDraftRules.SortRosterDefinitions(GameData.Heroes);
        }

        private void CreateStagePanel(Transform parent)
        {
            SecondaryPanelView.BuildStagePanel(new StagePanelViewBuildArgs
            {
                Parent = parent,
                Stages = GameData.Stages,
                OnResumeAutoProgress = progressManager.ResumeAutoProgress,
                OnSelectStage = stageId => progressManager.SelectStage(stageId),
                StageButtons = stageButtons
            });
        }

        private void CreateSummonPanel(Transform parent)
        {
            SummonPanelViewRefs refs = SecondaryPanelView.BuildSummonPanel(new SummonPanelViewBuildArgs
            {
                Parent = parent,
                OnRollHeroes = gachaManager.RollHeroes,
                OnRollEquipment = gachaManager.RollEquipment
            });
            gachaText = refs.ResultText;
        }

        private void CreateShopPanel(Transform parent)
        {
            SecondaryPanelView.BuildShopPanel(new ShopPanelViewBuildArgs
            {
                Parent = parent,
                OnSelectPremiumSpeed = () => speedManager.TrySelectSpeed(GameSpeedManager.PremiumSpeed)
            });
        }

        private void CreateSupportPanel(Transform parent)
        {
            SupportPanelViewRefs refs = SecondaryPanelView.BuildSupportPanel(new SupportPanelViewBuildArgs
            {
                Parent = parent,
                Skills = battleManager.Skills,
                Pets = battleManager.Pets,
                SkillStatusTexts = heroHud.SkillStatusTexts,
                PetStatusTexts = heroHud.PetStatusTexts
            });
            supportSummaryText = refs.SummaryText;
        }

        private void CreateDebugPanel(Transform parent)
        {
            DebugPanelViewRefs refs = DebugPanelView.Build(new DebugPanelViewBuildArgs
            {
                Parent = parent,
                Buttons = DebugPanelStateBuilder.BuildMainButtons(
                    wallet,
                    battleManager,
                    accountProgressManager,
                    speedManager,
                    progressManager,
                    resetSaveAction),
                TimeButtons = DebugPanelStateBuilder.BuildTimeButtons(SetTimeScale),
                OnRefresh = UpdateView
            });

            debugText = refs.StatusText;
        }

        private void DebugGrantTestCurrency()
        {
            wallet.AddGold(100000);
            wallet.AddRuby(5000);
            wallet.AddHeroExpItem(20000);
            wallet.AddEquipmentExpItem(20000);
            wallet.AddTotemEssence(10000);
            wallet.AddFacilityMaterials(5000, 5000, 5000);
            wallet.AddHeroTranscendStone(300);
            wallet.AddHeroSummonTicket(50);
            wallet.AddEquipmentSummonTicket(50);
            accountProgressManager.DebugAddLevels(25);
            accountProgressManager.DebugAddTalentPoints(200);
            UpdateView();
        }

        private void CreateBottomNav(Transform parent)
        {
            BottomNavView.Build(new BottomNavViewBuildArgs<HudTab>
            {
                Parent = parent,
                Items = new[]
                {
                    new BottomNavItem<HudTab> { Tab = HudTab.Growth, Label = "성장" },
                    new BottomNavItem<HudTab> { Tab = HudTab.Hero, Label = "영웅" },
                    new BottomNavItem<HudTab> { Tab = HudTab.Fortress, Label = "요새" },
                    new BottomNavItem<HudTab> { Tab = HudTab.Facility, Label = "시설" },
                    new BottomNavItem<HudTab> { Tab = HudTab.Summon, Label = "소환" },
                    new BottomNavItem<HudTab> { Tab = HudTab.Stage, Label = "배틀" },
                    new BottomNavItem<HudTab> { Tab = HudTab.Shop, Label = "상점" }
                },
                OnTabClick = RequestTabChange,
                TabButtons = tabButtons,
                TabButtonLabels = tabButtonLabels,
                TabNotificationDots = tabNotificationDots
            });
        }

    }
}
