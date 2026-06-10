using UnityEngine;
using IdleGame.Gacha;
using IdleGame.Progression;
using IdleGame.Data;
using IdleGame.Speed;
using IdleGame.UI.Battle;
using IdleGame.UI.Common;
using IdleGame.UI.Debugging;
using IdleGame.UI.Facility;
using IdleGame.UI.Fortress;
using IdleGame.UI.Growth;
using IdleGame.UI.Navigation;
using IdleGame.UI.Summon;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
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
                OnCycleSpeed = () => speedManager?.CycleSpeed(),
                OnToggleDungeonRepeat = () => battleManager?.ToggleDungeonRepeatDuringRun(),
                OnExitDungeon = ExitDungeonFromHud
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
            battleHud.DungeonRepeatButton = refs.DungeonRepeatButton;
            battleHud.DungeonExitButton = refs.DungeonExitButton;
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
            CreateDungeonPanel(stagePanel.transform);
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

        private void CreateFacilityRewardPopup(Transform parent)
        {
            FacilityRewardPopupRefs refs = FacilityView.BuildRewardPopup(parent, CloseFacilityRewardPopup);
            facilityRewardPopup = refs.Popup;
            facilityRewardPopupListText = refs.ListText;
        }

        private void CreateDungeonPanel(Transform parent)
        {
            dungeonViewRefs = SecondaryPanelView.BuildDungeonPanel(new DungeonPanelViewBuildArgs
            {
                Parent = parent,
                Wallet = wallet,
                DungeonManager = dungeonProgressManager,
                GetSelectedDungeon = () => selectedDungeonKind,
                GetSelectedDungeonLevel = () => selectedDungeonLevel,
                GetRepeatDungeon = () => dungeonRepeatChallenge,
                OnOpenDungeon = OpenDungeonDetail,
                OnChangeDungeonLevel = ChangeSelectedDungeonLevel,
                OnToggleRepeatDungeon = ToggleDungeonRepeatChallenge,
                OnEnterDungeon = EnterSelectedDungeon,
                OnSweepDungeon = SweepSelectedDungeon,
                OnCloseDungeon = CloseDungeonDetail,
                FormatGameNumber = value => FormatShortNumber(value),
                FormatCountNumber = FormatCountNumber
            });
        }

        private void OpenDungeonDetail(DungeonKind kind)
        {
            selectedDungeonKind = kind;
            selectedDungeonLevel = dungeonProgressManager != null
                ? dungeonProgressManager.GetMaxSelectableLevel(kind)
                : 1;
            dungeonDetailOpen = true;
            QueueHudRefresh(HudDirtyFlags.Stage);
        }

        private void ChangeSelectedDungeonLevel(int delta)
        {
            if (!dungeonDetailOpen)
            {
                return;
            }

            int nextLevel = selectedDungeonLevel + delta;
            selectedDungeonLevel = dungeonProgressManager != null
                ? dungeonProgressManager.ClampSelectableLevel(selectedDungeonKind, nextLevel)
                : Mathf.Max(1, nextLevel);
            QueueHudRefresh(HudDirtyFlags.Stage);
        }

        private void ToggleDungeonRepeatChallenge()
        {
            dungeonRepeatChallenge = !dungeonRepeatChallenge;
            QueueHudRefresh(HudDirtyFlags.Stage);
        }

        private void EnterSelectedDungeon()
        {
            if (battleManager == null || dungeonProgressManager == null)
            {
                return;
            }

            selectedDungeonLevel = dungeonProgressManager.ClampSelectableLevel(selectedDungeonKind, selectedDungeonLevel);
            if (!battleManager.TryEnterDungeon(selectedDungeonKind, selectedDungeonLevel, dungeonRepeatChallenge))
            {
                QueueHudRefresh(HudDirtyFlags.Stage);
                return;
            }

            dungeonDetailOpen = false;
            contentPanelOpen = false;
            QueueHudRefresh(HudDirtyFlags.Header | HudDirtyFlags.Battle | HudDirtyFlags.Stage | HudDirtyFlags.Navigation);
        }

        private void ExitDungeonFromHud()
        {
            if (battleManager == null || !battleManager.IsDungeonRunActive)
            {
                return;
            }

            DungeonKind exitingKind = battleManager.ActiveDungeonKind;
            battleManager.ExitDungeonWithRefund();
            selectedDungeonKind = exitingKind;
            selectedDungeonLevel = dungeonProgressManager != null
                ? dungeonProgressManager.GetMaxSelectableLevel(selectedDungeonKind)
                : 1;
            dungeonDetailOpen = true;
            activeTab = HudTab.Stage;
            contentPanelOpen = true;
            QueueHudRefresh(HudDirtyFlags.Header | HudDirtyFlags.Battle | HudDirtyFlags.Stage | HudDirtyFlags.Navigation);
        }

        private void SweepSelectedDungeon()
        {
            if (dungeonProgressManager == null)
            {
                return;
            }

            selectedDungeonLevel = dungeonProgressManager.ClampSelectableLevel(selectedDungeonKind, selectedDungeonLevel);
            if (dungeonProgressManager.TrySweepDungeon(selectedDungeonKind, selectedDungeonLevel, out string rewardText))
            {
                OpenDungeonClearPopup(selectedDungeonKind, selectedDungeonLevel, rewardText, true, false, false);
            }

            QueueHudRefresh(HudDirtyFlags.Header | HudDirtyFlags.Stage | HudDirtyFlags.Navigation);
        }

        private void CloseDungeonDetail()
        {
            dungeonDetailOpen = false;
            QueueHudRefresh(HudDirtyFlags.Stage);
        }

        private void CreateSummonPanel(Transform parent)
        {
            summonViewRefs = SummonPanelView.Build(new SummonScreenBuildArgs
            {
                Parent = parent,
                GetSelectedPool = () => selectedSummonPool,
                GetSelectedEventTargetId = GetSelectedEventSummonTargetId,
                OnSelectPool = SelectSummonPool,
                OnSelectEventTarget = SelectEventSummonTarget,
                OnRoll = (pool, count, eventTargetId) => gachaManager.Roll(pool, count, eventTargetId),
                OnCloseResultPopup = CloseSummonResultPopup
            });
            gachaText = summonViewRefs.ResultText;
        }

        private string GetSelectedEventSummonTargetId()
        {
            if (GachaEventTargetDefinitions.Get(selectedEventSummonTargetId) != null)
            {
                return selectedEventSummonTargetId;
            }

            GachaEventTargetDefinition target = GachaEventTargetDefinitions.Default;
            selectedEventSummonTargetId = target != null ? target.Id : string.Empty;
            return selectedEventSummonTargetId;
        }

        private void SelectEventSummonTarget(string targetId)
        {
            GachaEventTargetDefinition target = GachaEventTargetDefinitions.Get(targetId);
            string normalizedId = target != null ? target.Id : string.Empty;
            if (selectedEventSummonTargetId == normalizedId)
            {
                return;
            }

            selectedEventSummonTargetId = normalizedId;
            QueueHudRefresh(HudDirtyFlags.Summon);
        }

        private void SelectSummonPool(GachaPoolKind pool)
        {
            if (selectedSummonPool == pool)
            {
                return;
            }

            selectedSummonPool = pool;
            QueueHudRefresh(HudDirtyFlags.Summon);
        }

        private void CloseSummonResultPopup()
        {
            if (!summonResultPopupOpen)
            {
                return;
            }

            summonResultPopupOpen = false;
            QueueHudRefresh(HudDirtyFlags.Summon);
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
            wallet.AddDungeonTicket(20);
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
                    new BottomNavItem<HudTab> { Tab = HudTab.Stage, Label = "던전" },
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
