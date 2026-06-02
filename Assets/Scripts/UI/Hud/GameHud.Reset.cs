using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void ResetRuntimeUiState()
        {
            resourceText = null;
            rubyResourceText = null;
            battleHud.Reset();
            heroHud.Reset();
            accountLevelText = null;
            supportSummaryText = null;
            totalCombatPowerText = null;
            growthNoticeText = null;
            fieldStagePillText = null;
            guideQuestText = null;
            accountExpFill = null;
            contentLayoutElement = null;
            runtimeTickState.ResetRuntime();
            dirtyHudFlags = HudDirtyFlags.All;
            hudRefreshQueued = false;
            lastRenderedActiveTab = HudTab.Growth;
            lastRenderedContentPanelOpen = true;
            lastRenderedHeroDetailPanelOpen = false;
            cachedGrowthAttention = false;
            cachedHeroAttention = false;

            contentPanelOpen = true;
            contentRoot = null;
            growthPanel = null;
            fortressPanel = null;
            facilityPanel = null;
            heroFacilityContent = null;
            facilityAssignmentModal = null;
            heroFacilitySummaryText = null;
            heroFacilityNoticeText = null;
            fortressViewRefs = null;
            stagePanel = null;
            summonPanel = null;
            shopPanel = null;
            supportPanel = null;
            debugPanel = null;
            facilityRewardPopup = null;
            guideQuestDot = null;
            gachaText = null;
            debugText = null;
            facilityRewardPopupListText = null;
            selectedGrowthLevelStep = 1;
            selectedHeroDetailId = string.Empty;
            heroFormationState.ResetRuntime();
            heroFormationPromptTargetState.Reset(activeTab, contentPanelOpen, activeHeroPageTab);
            heroDetailEquipmentState.ResetRuntime();
            totemRuneState.ResetRuntime();
            facilityAssignmentModalOpen = false;
            heroTranscendState.ResetRuntime();
            selectedHeroTraitId = "ATK_CORE";
            activeHeroPageTab = HeroPageTab.Formation;
            activeHeroDetailTab = HeroDetailTab.BasicInfo;
            heroDetailPanelOpen = false;
            heroTranscendAutoRollCoroutine = null;

            abilityButtonTexts.Clear();
            abilityCostBadgeTexts.Clear();
            facilityCardTexts.Clear();
            facilityUpgradeButtons.Clear();
            facilityCollectButtons.Clear();
            facilityAssignmentRowTexts.Clear();
            facilityAssignmentSlotTexts.Clear();
            facilityCardStates.Clear();
            facilityAssignmentRowTextStates.Clear();
            facilityAssignmentSlotStates.Clear();
            heroDetailEquipmentState.ResetFilters();
            abilityNotificationDots.Clear();
            growthStepButtons.Clear();
            stageButtons.Clear();
            tabButtons.Clear();
            tabButtonLabels.Clear();
            tabNotificationDots.Clear();
        }
    }
}
