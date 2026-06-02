using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public static partial class HeroDetailEquipmentActionService
    {
        public static HeroDetailEquipmentActionResult ToggleHeroEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            string equipmentId,
            bool slotSelectionActive,
            EquipmentSlot selectedSlot)
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

            if (equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, equipmentId))
            {
                bool unequipped = equipmentInventory.UnequipEquipment(hero.Definition.Id, equipmentId);
                return new HeroDetailEquipmentActionResult
                {
                    Success = unequipped,
                    ClearSelectedEquipment = true,
                    ClearSlotSelection = true
                };
            }

            if (!slotSelectionActive)
            {
                return new HeroDetailEquipmentActionResult
                {
                    NeedsSelection = true
                };
            }

            if (state.Definition.Slot != selectedSlot)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = EquipmentUiText.GetSlotLabel(selectedSlot) + " 칸에는 "
                        + EquipmentUiText.GetSlotLabel(state.Definition.Slot) + " 장비를 장착할 수 없습니다."
                };
            }

            return EquipToHero(hero, equipmentInventory, state);
        }

        public static HeroDetailEquipmentActionResult PlaceSelectedEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            string equipmentId,
            EquipmentSlot slot)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            EquipmentState state = equipmentInventory.GetState(equipmentId);
            if (state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult
                {
                    ClearSelectedEquipment = true
                };
            }

            if (state.Definition.Slot != slot)
            {
                return new HeroDetailEquipmentActionResult
                {
                    SelectSlotFilter = true
                };
            }

            return EquipToHero(hero, equipmentInventory, state);
        }

        public static HeroDetailEquipmentActionResult RemoveSlotEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            EquipmentSlot slot)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            bool removed = equipmentInventory.Unequip(hero.Definition.Id, slot);
            return new HeroDetailEquipmentActionResult
            {
                Success = removed,
                ClearSelectedEquipment = true,
                ClearSlotSelection = true
            };
        }

        public static HeroDetailEquipmentActionResult UnequipAll(HeroState hero, EquipmentInventory equipmentInventory)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            int unequippedCount = equipmentInventory.UnequipAll(hero.Definition.Id);
            return new HeroDetailEquipmentActionResult
            {
                Success = unequippedCount > 0,
                ClearSelectedEquipment = true,
                ClearSelectedEquipmentDetail = true,
                ClearSlotSelection = true,
                CloseEquipmentDetailPopup = true,
                Message = unequippedCount > 0
                    ? "장비 " + unequippedCount + "개를 해제했습니다."
                    : "해제할 장비가 없습니다."
            };
        }

        public static HeroDetailEquipmentActionResult AutoEquip(HeroState hero, EquipmentInventory equipmentInventory)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            int equippedCount = equipmentInventory.EquipBestAvailable(hero.Definition.Id);
            return new HeroDetailEquipmentActionResult
            {
                Success = equippedCount > 0,
                ClearSelectedEquipment = true,
                ClearSelectedEquipmentDetail = true,
                ClearSlotSelection = true,
                CloseEquipmentDetailPopup = true,
                Message = equippedCount > 0
                    ? "강한 장비 " + equippedCount + "개를 자동 장착했습니다."
                    : "장착할 장비가 없습니다."
            };
        }

        public static HeroDetailEquipmentActionResult ToggleDetailEquipment(
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

            if (equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, state.Definition.Id))
            {
                bool unequipped = equipmentInventory.UnequipEquipment(hero.Definition.Id, state.Definition.Id);
                return new HeroDetailEquipmentActionResult
                {
                    Success = unequipped,
                    ClearSelectedEquipment = true,
                    ClearSlotSelection = true
                };
            }

            HeroDetailEquipmentActionResult result = EquipToHero(hero, equipmentInventory, state);
            if (result.Success)
            {
                result.ClearSelectedEquipmentDetail = true;
                result.CloseEquipmentDetailPopup = true;
            }

            return result;
        }

        private static HeroDetailEquipmentActionResult EquipToHero(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            EquipmentState state)
        {
            if (equipmentInventory.GetAvailableCount(state.Definition.Id) <= 0)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "장착 가능한 장비 수량이 부족합니다."
                };
            }

            bool equipped = equipmentInventory.Equip(hero.Definition.Id, state.Definition.Id);
            return new HeroDetailEquipmentActionResult
            {
                Success = equipped,
                ClearSelectedEquipment = equipped,
                ClearSlotSelection = equipped,
                Message = equipped ? string.Empty : "장착 가능한 장비 수량이 부족합니다."
            };
        }
    }
}
