using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public sealed partial class HeroDetailEquipmentUiState
    {
        public bool TrySelectEquipmentForSlot(HeroState hero, EquipmentInventory equipmentInventory, string equipmentId)
        {
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
            if (hero == null || !hero.IsOwned || state == null || !state.IsOwned)
            {
                return false;
            }

            EnsureFilter(state.Definition.Slot);

            if (equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, equipmentId)
                || equipmentInventory.GetAvailableCount(equipmentId) <= 0)
            {
                SelectedEquipmentId = string.Empty;
                return true;
            }

            ToggleSelectedEquipment(equipmentId);
            return true;
        }

        public HeroDetailEquipmentActionResult ToggleHeroEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            string equipmentId)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            EquipmentState state = equipmentInventory.GetState(equipmentId);
            if (state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult();
            }

            HeroDetailEquipmentActionResult result = HeroDetailEquipmentActionService.ToggleHeroEquipment(
                hero,
                equipmentInventory,
                equipmentId,
                SlotSelectionActive,
                SelectedSlot);
            if (result.NeedsSelection)
            {
                TrySelectEquipmentForSlot(hero, equipmentInventory, equipmentId);
                return new HeroDetailEquipmentActionResult
                {
                    Success = true
                };
            }

            return result;
        }

        public HeroDetailEquipmentActionResult PlaceSelectedEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            EquipmentSlot slot)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            if (string.IsNullOrEmpty(SelectedEquipmentId))
            {
                SelectFilter(slot);
                return new HeroDetailEquipmentActionResult
                {
                    Success = true
                };
            }

            HeroDetailEquipmentActionResult result = HeroDetailEquipmentActionService.PlaceSelectedEquipment(
                hero,
                equipmentInventory,
                SelectedEquipmentId,
                slot);
            if (result.SelectSlotFilter)
            {
                SelectFilter(slot);
            }

            return result;
        }

        public HeroDetailEquipmentActionResult ToggleDetailEquipment(HeroState hero, EquipmentInventory equipmentInventory)
        {
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(SelectedEquipmentDetailId) : null;
            if (hero == null || !hero.IsOwned || state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult();
            }

            HeroDetailEquipmentActionResult result = HeroDetailEquipmentActionService.ToggleDetailEquipment(
                hero,
                equipmentInventory,
                state.Definition.Id);
            if (result.Success && result.CloseEquipmentDetailPopup)
            {
                SelectedSlot = state.Definition.Slot;
            }

            return result;
        }

        public void ToggleSelectedEquipment(string equipmentId)
        {
            SelectedEquipmentId = SelectedEquipmentId == equipmentId ? string.Empty : equipmentId;
        }

        public void ApplyActionResult(HeroDetailEquipmentActionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.ClearSelectedEquipment)
            {
                SelectedEquipmentId = string.Empty;
            }

            if (result.ClearSelectedEquipmentDetail)
            {
                SelectedEquipmentDetailId = string.Empty;
            }

            if (result.ClearSlotSelection)
            {
                SlotSelectionActive = false;
            }

            if (result.CloseEquipmentDetailPopup)
            {
                DetailPopupOpen = false;
            }

            if (result.CloseBulkDismantlePrompt)
            {
                BulkDismantlePromptOpen = false;
            }
        }
    }
}
