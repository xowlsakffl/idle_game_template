using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class EquipmentInventory
    {
        public bool Equip(string heroId, string equipmentId)
        {
            EquipmentState state = GetState(equipmentId);
            if (string.IsNullOrEmpty(heroId) || state == null || !state.IsOwned)
            {
                return false;
            }

            EquipmentSlot slot = state.Definition.Slot;
            string currentEquipmentId = GetEquippedEquipmentId(heroId, slot);
            if (currentEquipmentId != equipmentId && GetAvailableCount(equipmentId) <= 0)
            {
                return false;
            }

            if (!equippedByHero.TryGetValue(heroId, out Dictionary<EquipmentSlot, string> slots))
            {
                slots = new Dictionary<EquipmentSlot, string>();
                equippedByHero[heroId] = slots;
            }

            slots[slot] = equipmentId;
            SaveHeroEquipmentSlot(heroId, slot, equipmentId);
            NotifyChanged();
            return true;
        }

        public bool Unequip(string heroId, EquipmentSlot slot)
        {
            if (string.IsNullOrEmpty(heroId) || !equippedByHero.TryGetValue(heroId, out Dictionary<EquipmentSlot, string> slots))
            {
                return false;
            }

            if (!slots.TryGetValue(slot, out string equipmentId) || string.IsNullOrEmpty(equipmentId))
            {
                return false;
            }

            slots[slot] = string.Empty;
            SaveHeroEquipmentSlot(heroId, slot, string.Empty);
            NotifyChanged();
            return true;
        }

        public bool UnequipEquipment(string heroId, string equipmentId)
        {
            if (string.IsNullOrEmpty(heroId) || string.IsNullOrEmpty(equipmentId) || !equippedByHero.TryGetValue(heroId, out Dictionary<EquipmentSlot, string> slots))
            {
                return false;
            }

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slots.TryGetValue(slot, out string equippedId) && equippedId == equipmentId)
                {
                    return Unequip(heroId, slot);
                }
            }

            return false;
        }

        public int UnequipAll(string heroId)
        {
            if (string.IsNullOrEmpty(heroId) || !equippedByHero.TryGetValue(heroId, out Dictionary<EquipmentSlot, string> slots))
            {
                return 0;
            }

            int unequippedCount = 0;
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (!slots.TryGetValue(slot, out string equipmentId) || string.IsNullOrEmpty(equipmentId))
                {
                    continue;
                }

                slots[slot] = string.Empty;
                SaveHeroEquipmentSlot(heroId, slot, string.Empty);
                unequippedCount += 1;
            }

            if (unequippedCount > 0)
            {
                NotifyChanged();
            }

            return unequippedCount;
        }

        public int EquipBestAvailable(string heroId)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                return 0;
            }

            if (!equippedByHero.TryGetValue(heroId, out Dictionary<EquipmentSlot, string> slots))
            {
                slots = new Dictionary<EquipmentSlot, string>();
                equippedByHero[heroId] = slots;
            }

            bool changed = false;
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slots.TryGetValue(slot, out string currentEquipmentId) && !string.IsNullOrEmpty(currentEquipmentId))
                {
                    slots[slot] = string.Empty;
                    SaveHeroEquipmentSlot(heroId, slot, string.Empty);
                    changed = true;
                }
            }

            int equippedCount = 0;
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                EquipmentState bestState = FindBestAvailableEquipment(slot);
                if (bestState == null)
                {
                    continue;
                }

                slots[slot] = bestState.Definition.Id;
                SaveHeroEquipmentSlot(heroId, slot, bestState.Definition.Id);
                equippedCount += 1;
                changed = true;
            }

            if (changed)
            {
                NotifyChanged();
            }

            return equippedCount;
        }
        private EquipmentState FindBestAvailableEquipment(EquipmentSlot slot)
        {
            EquipmentState bestState = null;
            long bestScore = long.MinValue;
            foreach (EquipmentState state in states)
            {
                if (state == null
                    || !state.IsOwned
                    || state.Definition.Slot != slot
                    || GetAvailableCount(state.Definition.Id) <= 0)
                {
                    continue;
                }

                long score = GetAutoEquipScore(state);
                if (bestState == null || score > bestScore)
                {
                    bestState = state;
                    bestScore = score;
                }
            }

            return bestState;
        }

        private static long GetAutoEquipScore(EquipmentState state)
        {
            if (state == null)
            {
                return long.MinValue;
            }

            return state.AttackBonus * 10L
                + state.HpBonus
                + (int)state.Definition.Rarity * 10000L
                + state.Stars * 1000L
                + state.Level;
        }
    }
}
