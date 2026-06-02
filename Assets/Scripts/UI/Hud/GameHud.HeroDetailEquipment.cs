using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.UI.Hero.Detail;
using IdleGame.UI.Hero.Detail.Equipment;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
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
