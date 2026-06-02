using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;
using IdleGame.UI.Hero.Detail;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void OpenHeroDetailPanel(string heroId)
        {
            if (FindHeroState(heroId) == null)
            {
                return;
            }

            selectedHeroDetailId = heroId;
            activeHeroDetailTab = HeroDetailTab.BasicInfo;
            heroDetailEquipmentState.ResetForHero();
            heroDetailPanelOpen = true;
            UpdateView();
        }

        private void CloseHeroDetailPanel()
        {
            StopHeroTranscendAutoRoll();
            CloseHeroTranscendConfirmPrompt();
            heroDetailPanelOpen = false;
            selectedHeroDetailId = string.Empty;
            heroDetailEquipmentState.ResetForHero();
            UpdateView();
        }

        private void SelectHeroDetailTab(HeroDetailTab tab)
        {
            activeHeroDetailTab = tab;
            if (tab != HeroDetailTab.Equipment)
            {
                heroDetailEquipmentState.CloseForNonEquipmentTab();
            }

            UpdateView();
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
                heroDetailEquipmentState.ResetForHero();
                return;
            }

            string noticeText = runtimeTickState.GetActiveNotice(Time.unscaledTime);
            HeroDetailPanelPresenterResult result = HeroDetailPanelPresenter.Refresh(new HeroDetailPanelPresenterArgs
            {
                Refs = heroHud.DetailViewRefs,
                EquipmentInventory = equipmentInventory,
                Wallet = wallet,
                BattleManager = battleManager,
                Hero = hero,
                SelectedHeroId = selectedHeroDetailId,
                SelectedEquipmentId = heroDetailEquipmentState.SelectedEquipmentId,
                SelectedEquipmentDetailId = heroDetailEquipmentState.SelectedEquipmentDetailId,
                SlotSelectionActive = heroDetailEquipmentState.SlotSelectionActive,
                SelectedSlot = heroDetailEquipmentState.SelectedSlot,
                SelectedSlots = heroDetailEquipmentState.SelectedSlots,
                EquipmentDetailPopupOpen = heroDetailEquipmentState.DetailPopupOpen,
                EquipmentDismantlePopupOpen = heroDetailEquipmentState.DismantlePopupOpen,
                EquipmentBulkDismantlePromptOpen = heroDetailEquipmentState.BulkDismantlePromptOpen,
                SelectedBulkDismantleRarity = heroDetailEquipmentState.SelectedBulkDismantleRarity,
                SelectedDismantleEquipmentIds = heroDetailEquipmentState.SelectedDismantleEquipmentIds,
                ActiveTab = activeHeroDetailTab,
                InFormation = IsHeroInEditingFormation(hero.Definition.Id),
                CombatPower = GetHeroDetailCombatPower(hero),
                SelectedTranscendSlotIndex = heroTranscendState.SelectedSlotIndex,
                HeroTranscendStopOnlySs = heroTranscendState.StopOnlySs,
                HeroTranscendAutoRolling = heroTranscendState.AutoRolling,
                NoticeText = noticeText,
                TabButtons = heroHud.DetailTabButtons,
                EquipmentSlotButtons = heroHud.DetailEquipmentSlotButtons,
                EquipmentSlotTexts = heroHud.DetailEquipmentSlotTexts,
                EquipmentSlotRemoveButtons = heroHud.DetailEquipmentSlotRemoveButtons,
                EquipmentFilterButtons = heroHud.DetailEquipmentFilterButtons,
                DismantleFilterButtons = heroHud.EquipmentDismantleFilterButtons,
                EquipmentCardButtons = heroHud.DetailEquipmentCardButtons,
                EquipmentCardTexts = heroHud.DetailEquipmentCardTexts,
                EquipmentActionButtons = heroHud.DetailEquipmentActionButtons,
                DismantleCardButtons = heroHud.EquipmentDismantleCardButtons,
                DismantleCardTexts = heroHud.EquipmentDismantleCardTexts,
                CreateButton = CreateButton,
                CreateCornerActionButton = (label, parent, color) => HudUiFactory.CreateCornerActionButton(label, parent, 18, color),
                OpenEquipmentDetail = OpenEquipmentDetailPopup,
                ToggleEquipment = SelectOrRemoveHeroDetailEquipment,
                SelectDismantleEquipment = SelectDismantleEquipment,
                FindHeroState = FindHeroState,
                GetShortHeroLabel = GetShortHeroLabel
            });

            heroTranscendState.SelectSlot(result.SelectedTranscendSlotIndex);
            if (result.EquipmentDetailPopupInvalid)
            {
                heroDetailEquipmentState.CloseDetail();
            }
        }
    }
}
