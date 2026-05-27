using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Detail;
using IdleGame.UI.Hero.Detail.Equipment;

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

        private void OpenEquipmentDetailPopup(string equipmentId)
        {
            if (!heroDetailEquipmentState.TryOpenDetail(equipmentInventory, equipmentId))
            {
                return;
            }

            activeHeroDetailTab = HeroDetailTab.Equipment;
            UpdateView();
        }

        private void CloseEquipmentDetailPopup()
        {
            heroDetailEquipmentState.CloseDetail();
            UpdateView();
        }

        private void OpenEquipmentDismantlePopup()
        {
            heroDetailEquipmentState.OpenDismantle();
            activeHeroDetailTab = HeroDetailTab.Equipment;
            UpdateView();
        }

        private void CloseEquipmentDismantlePopup()
        {
            heroDetailEquipmentState.CloseDismantle();
            UpdateView();
        }

        private void OpenEquipmentBulkDismantlePrompt()
        {
            heroDetailEquipmentState.OpenBulkDismantlePrompt();
            UpdateView();
        }

        private void CloseEquipmentBulkDismantlePrompt()
        {
            heroDetailEquipmentState.CloseBulkDismantlePrompt();
            UpdateView();
        }

        private void SelectDismantleEquipment(string equipmentCopyKey)
        {
            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.ToggleDismantleSelection(
                equipmentCopyKey,
                heroDetailEquipmentState.SelectedDismantleEquipmentIds,
                equipmentInventory));
            UpdateView();
        }

        private void ChangeBulkDismantleRarity(int direction)
        {
            heroDetailEquipmentState.ChangeBulkDismantleRarity(direction);
            UpdateView();
        }

        private void ToggleHeroDetailEquipmentFilter(EquipmentSlot slot)
        {
            activeHeroDetailTab = HeroDetailTab.Equipment;
            heroDetailEquipmentState.ToggleFilterAndPrune(slot, equipmentInventory);
            UpdateView();
        }

        private void SelectHeroDetailEquipmentFilter(EquipmentSlot slot)
        {
            activeHeroDetailTab = HeroDetailTab.Equipment;
            heroDetailEquipmentState.SelectFilterAndPrune(slot, equipmentInventory);

            UpdateView();
        }

        private void SelectHeroDetailEquipmentForSlot(string equipmentId)
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            if (!heroDetailEquipmentState.TrySelectEquipmentForSlot(hero, equipmentInventory, equipmentId))
            {
                return;
            }

            activeHeroDetailTab = HeroDetailTab.Equipment;
            UpdateView();
        }

        private void SelectOrRemoveHeroDetailEquipment(string equipmentId)
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyEquipmentActionResult(heroDetailEquipmentState.ToggleHeroEquipment(hero, equipmentInventory, equipmentId));
            UpdateView();
        }

        private void TryPlaceSelectedHeroDetailEquipment(EquipmentSlot slot)
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyEquipmentActionResult(heroDetailEquipmentState.PlaceSelectedEquipment(hero, equipmentInventory, slot));
            UpdateView();
        }

        private void RemoveHeroDetailEquipment(EquipmentSlot slot)
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return;
            }

            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.RemoveSlotEquipment(hero, equipmentInventory, slot));
            UpdateView();
        }

        private void UnequipAllHeroDetailEquipment()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return;
            }

            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.UnequipAll(hero, equipmentInventory));
            UpdateView();
        }

        private void AutoEquipHeroDetailEquipment()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return;
            }

            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.AutoEquip(hero, equipmentInventory));
            UpdateView();
        }

        private void ToggleSelectedEquipmentDetailEquip()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyEquipmentActionResult(heroDetailEquipmentState.ToggleDetailEquipment(hero, equipmentInventory));
            UpdateView();
        }

        private void LevelUpSelectedEquipmentDetail()
        {
            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.LevelUp(
                equipmentInventory,
                wallet,
                heroDetailEquipmentState.SelectedEquipmentDetailId));
            UpdateView();
        }

        private bool CanLevelUpSelectedEquipmentDetail()
        {
            return HeroDetailEquipmentActionService.CanLevelUp(
                equipmentInventory,
                wallet,
                heroDetailEquipmentState.SelectedEquipmentDetailId);
        }

        private void StarUpSelectedEquipmentDetail()
        {
            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.StarUp(
                equipmentInventory,
                heroDetailEquipmentState.SelectedEquipmentDetailId));
            UpdateView();
        }

        private void DismantleSelectedEquipment()
        {
            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.DismantleSelected(
                equipmentInventory,
                wallet,
                heroDetailEquipmentState.SelectedDismantleEquipmentIds,
                heroDetailEquipmentState.SelectedSlots));
            UpdateView();
        }

        private void ConfirmBulkDismantleEquipment()
        {
            ApplyEquipmentActionResult(HeroDetailEquipmentActionService.BulkDismantle(
                equipmentInventory,
                wallet,
                heroDetailEquipmentState.SelectedBulkDismantleRarity,
                heroDetailEquipmentState.SelectedDismantleEquipmentIds));
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
                CreateCornerActionButton = CreateCornerActionButton,
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

        private double GetHeroDetailCombatPower(HeroState hero)
        {
            var heroes = new List<HeroState> { hero };
            return abilityManager.GetTotalCombatPower(heroes);
        }

        private void ToggleSelectedHeroDetailFormation()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            EnsureHeroFormationDraft();
            ApplyHeroActionResult(HeroActionService.TryToggleSelectedHeroDetailFormation(heroFormationState.EditingHeroIds, hero));
            UpdateView();
        }

        private void RemoveSelectedHeroDetailFromFormation()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            EnsureHeroFormationDraft();
            ApplyHeroActionResult(HeroActionService.TryRemoveSelectedHeroDetailFromFormation(heroFormationState.EditingHeroIds, hero));
            UpdateView();
        }

        private void LevelUpSelectedHeroDetail()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyHeroActionResult(HeroActionService.TryLevelUpHero(battleManager, wallet, hero));
            UpdateView();
        }

        private bool CanLevelUpSelectedHeroDetail()
        {
            return HeroActionService.CanLevelUpHero(wallet, FindHeroState(selectedHeroDetailId));
        }

        private void StarUpSelectedHeroDetail()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyHeroActionResult(HeroActionService.TryStarUpHero(battleManager, hero));
            UpdateView();
        }

        private void ApplyEquipmentActionResult(HeroDetailEquipmentActionResult result)
        {
            if (result == null)
            {
                return;
            }

            heroDetailEquipmentState.ApplyActionResult(result);

            if (!string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }
        }
    }
}
