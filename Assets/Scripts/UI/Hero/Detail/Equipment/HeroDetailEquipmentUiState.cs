using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public sealed partial class HeroDetailEquipmentUiState
    {
        public string SelectedEquipmentId = string.Empty;
        public string SelectedEquipmentDetailId = string.Empty;
        public EquipmentSlot SelectedSlot = EquipmentSlot.Weapon;
        public HeroRarity SelectedBulkDismantleRarity = HeroRarity.Rare;
        public bool SlotSelectionActive;
        public bool DetailPopupOpen;
        public bool DismantlePopupOpen;
        public bool BulkDismantlePromptOpen;

        public readonly HashSet<EquipmentSlot> SelectedSlots = new HashSet<EquipmentSlot>();
        public readonly HashSet<string> SelectedDismantleEquipmentIds = new HashSet<string>();

        public HeroDetailEquipmentUiState()
        {
            ResetRuntime();
        }

        public void ResetRuntime()
        {
            SelectedEquipmentId = string.Empty;
            SelectedEquipmentDetailId = string.Empty;
            SelectedSlot = EquipmentSlot.Weapon;
            SelectedBulkDismantleRarity = HeroRarity.Rare;
            SlotSelectionActive = false;
            DetailPopupOpen = false;
            DismantlePopupOpen = false;
            BulkDismantlePromptOpen = false;
            SelectedDismantleEquipmentIds.Clear();
            ResetFilters();
        }

        public void ResetForHero()
        {
            SelectedEquipmentId = string.Empty;
            SelectedEquipmentDetailId = string.Empty;
            SelectedDismantleEquipmentIds.Clear();
            SlotSelectionActive = false;
            DetailPopupOpen = false;
            DismantlePopupOpen = false;
            BulkDismantlePromptOpen = false;
            ResetFilters();
        }

        public void CloseForNonEquipmentTab()
        {
            SelectedEquipmentId = string.Empty;
            SlotSelectionActive = false;
            DetailPopupOpen = false;
            DismantlePopupOpen = false;
            BulkDismantlePromptOpen = false;
        }

        public void OpenDetail(string equipmentId)
        {
            SelectedEquipmentDetailId = equipmentId ?? string.Empty;
            DetailPopupOpen = true;
        }

        public void CloseDetail()
        {
            DetailPopupOpen = false;
            SelectedEquipmentDetailId = string.Empty;
        }

        public void OpenDismantle()
        {
            DetailPopupOpen = false;
            DismantlePopupOpen = true;
            BulkDismantlePromptOpen = false;
            SelectedDismantleEquipmentIds.Clear();
        }

        public void CloseDismantle()
        {
            DismantlePopupOpen = false;
            BulkDismantlePromptOpen = false;
            SelectedDismantleEquipmentIds.Clear();
        }

        public void OpenBulkDismantlePrompt()
        {
            BulkDismantlePromptOpen = true;
        }

        public void CloseBulkDismantlePrompt()
        {
            BulkDismantlePromptOpen = false;
        }

        public void ChangeBulkDismantleRarity(int direction)
        {
            int rarity = Mathf.Clamp(
                (int)SelectedBulkDismantleRarity + direction,
                (int)HeroRarity.Common,
                (int)HeroRarity.Mythic);
            SelectedBulkDismantleRarity = (HeroRarity)rarity;
        }

        public void ToggleFilter(EquipmentSlot slot)
        {
            if (SelectedSlots.Contains(slot))
            {
                SelectedSlots.Remove(slot);
                if (SlotSelectionActive && SelectedSlot == slot)
                {
                    SlotSelectionActive = false;
                }
            }
            else
            {
                SelectedSlots.Add(slot);
            }
        }

        public void SelectFilter(EquipmentSlot slot)
        {
            SelectedSlot = slot;
            SlotSelectionActive = true;
            SelectedSlots.Clear();
            SelectedSlots.Add(slot);
        }

        public void ResetFilters()
        {
            SelectedSlots.Clear();
            foreach (EquipmentSlot slot in EquipmentUiText.FilterSlots)
            {
                SelectedSlots.Add(slot);
            }
        }

        public void EnsureFilter(EquipmentSlot slot)
        {
            if (!SelectedSlots.Contains(slot))
            {
                SelectedSlots.Add(slot);
            }
        }

        public void ClearSelectedEquipmentIfFilteredOut(EquipmentInventory equipmentInventory)
        {
            EquipmentState selectedState = equipmentInventory != null
                ? equipmentInventory.GetState(SelectedEquipmentId)
                : null;
            if (selectedState != null && !SelectedSlots.Contains(selectedState.Definition.Slot))
            {
                SelectedEquipmentId = string.Empty;
            }
        }

        public bool TryOpenDetail(EquipmentInventory equipmentInventory, string equipmentId)
        {
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
            if (state == null || !state.IsOwned)
            {
                return false;
            }

            OpenDetail(equipmentId);
            return true;
        }

        public void ToggleFilterAndPrune(EquipmentSlot slot, EquipmentInventory equipmentInventory)
        {
            ToggleFilter(slot);
            ClearSelectedEquipmentIfFilteredOut(equipmentInventory);
        }

        public void SelectFilterAndPrune(EquipmentSlot slot, EquipmentInventory equipmentInventory)
        {
            SelectFilter(slot);
            ClearSelectedEquipmentIfFilteredOut(equipmentInventory);
        }

    }
}
