using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class EquipmentInventory
    {
        public int GetOwnedCount(string equipmentId)
        {
            EquipmentState state = GetState(equipmentId);
            return state != null ? state.Count : 0;
        }

        public EquipmentState GetState(string equipmentId)
        {
            return !string.IsNullOrEmpty(equipmentId) && statesById.TryGetValue(equipmentId, out EquipmentState state) ? state : null;
        }

        public int GetTotalOwnedCount()
        {
            int total = 0;
            foreach (EquipmentState state in states)
            {
                total += state.Count;
            }

            return total;
        }

        public string GetEquippedEquipmentId(string heroId, EquipmentSlot slot)
        {
            if (string.IsNullOrEmpty(heroId) || !equippedByHero.TryGetValue(heroId, out Dictionary<EquipmentSlot, string> slots))
            {
                return string.Empty;
            }

            return slots.TryGetValue(slot, out string equipmentId) ? equipmentId : string.Empty;
        }

        public EquipmentState GetEquippedState(string heroId, EquipmentSlot slot)
        {
            string equipmentId = GetEquippedEquipmentId(heroId, slot);
            return GetState(equipmentId);
        }

        public bool IsEquipmentEquippedToHero(string heroId, string equipmentId)
        {
            if (string.IsNullOrEmpty(heroId) || string.IsNullOrEmpty(equipmentId) || !equippedByHero.TryGetValue(heroId, out Dictionary<EquipmentSlot, string> slots))
            {
                return false;
            }

            foreach (string equippedId in slots.Values)
            {
                if (equippedId == equipmentId)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetEquippedCount(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId))
            {
                return 0;
            }

            int count = 0;
            foreach (Dictionary<EquipmentSlot, string> slots in equippedByHero.Values)
            {
                foreach (string equippedId in slots.Values)
                {
                    if (equippedId == equipmentId)
                    {
                        count += 1;
                    }
                }
            }

            return count;
        }

        public int GetAvailableCount(string equipmentId)
        {
            return Math.Max(0, GetOwnedCount(equipmentId) - GetEquippedCount(equipmentId));
        }

        public int GetStarUpMaterialCount(string equipmentId)
        {
            EquipmentState state = GetState(equipmentId);
            if (state == null || !state.IsOwned)
            {
                return 0;
            }

            return Math.Max(0, state.Count - Math.Max(1, GetEquippedCount(equipmentId)));
        }
    }
}
