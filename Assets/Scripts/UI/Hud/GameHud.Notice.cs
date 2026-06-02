using UnityEngine;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
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
    }
}
