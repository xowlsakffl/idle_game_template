using System.Collections.Generic;
using IdleGame.UI.Facility;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
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
                CachedCardStates = facilityCardStates,
                CachedAssignmentRowTexts = facilityAssignmentRowTextStates,
                CachedAssignmentSlotStates = facilityAssignmentSlotStates,
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
    }
}
