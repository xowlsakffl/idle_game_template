using UnityEngine;
using IdleGame.Data;
using IdleGame.Speed;
using IdleGame.UI.Battle;
using IdleGame.UI.Common;
using IdleGame.UI.Debugging;
using IdleGame.UI.Facility;
using IdleGame.UI.Fortress;
using IdleGame.UI.Growth;
using IdleGame.UI.Navigation;

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
