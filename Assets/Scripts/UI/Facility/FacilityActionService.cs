using System.Collections.Generic;
using System;
using IdleGame.Battle;
using IdleGame.Data;

namespace IdleGame.UI.Facility
{
    public sealed class FacilityActionResult
    {
        public bool Success;
        public bool ShowRewardPopup;
        public List<string> RewardLines = new List<string>();
        public string Message;
    }

    public static class FacilityActionService
    {
        public static FacilityActionResult TryCollectFacility(BattleManager battleManager, string facilityId)
        {
            bool collected = battleManager != null && battleManager.CollectFacility(facilityId);
            return new FacilityActionResult
            {
                Success = collected,
                Message = collected ? "시설 보상을 수령했습니다." : "수령할 보상이 없습니다."
            };
        }

        public static FacilityActionResult TryCollectAllFacilities(
            BattleManager battleManager,
            Func<GameNumber, string> formatShortNumber)
        {
            if (battleManager == null)
            {
                return new FacilityActionResult
                {
                    Message = "수령할 보상이 없습니다."
                };
            }

            List<string> rewardLines = FacilityPanelStateBuilder.BuildRewardPopupLines(
                battleManager.Facilities,
                formatShortNumber);
            int collected = battleManager.CollectAllFacilities();

            return new FacilityActionResult
            {
                Success = collected > 0,
                ShowRewardPopup = collected > 0,
                RewardLines = rewardLines,
                Message = collected > 0 ? string.Empty : "수령할 보상이 없습니다."
            };
        }

        public static FacilityActionResult TryUpgradeFacility(BattleManager battleManager, string facilityId)
        {
            bool upgraded = battleManager != null && battleManager.TryUpgradeFacility(facilityId);
            return new FacilityActionResult
            {
                Success = upgraded,
                Message = upgraded ? "시설을 업그레이드했습니다." : "자재가 부족하거나 MAX입니다."
            };
        }

        public static FacilityActionResult AutoAssignAllFacilities(BattleManager battleManager)
        {
            if (battleManager == null)
            {
                return new FacilityActionResult();
            }

            battleManager.AutoAssignAllFacilities();
            return new FacilityActionResult
            {
                Success = true,
                Message = "빈 시설 슬롯에 추천 배치했습니다."
            };
        }

        public static FacilityActionResult ClearAllFacilityAssignments(BattleManager battleManager)
        {
            if (battleManager == null)
            {
                return new FacilityActionResult();
            }

            battleManager.ClearAllFacilityAssignments();
            return new FacilityActionResult
            {
                Success = true,
                Message = "시설 배치를 모두 해제했습니다."
            };
        }
    }
}
