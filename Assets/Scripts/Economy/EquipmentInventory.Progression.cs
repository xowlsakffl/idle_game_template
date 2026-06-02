using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class EquipmentInventory
    {
        public EquipmentState AddEquipment(string equipmentId, int amount)
        {
            if (string.IsNullOrEmpty(equipmentId) || amount <= 0)
            {
                return null;
            }

            EquipmentState state = GetState(equipmentId);
            if (state == null)
            {
                return null;
            }

            state.AddCopies(amount);
            SaveEquipment(state);
            NotifyChanged();
            return state;
        }

        public bool TryLevelUpEquipment(string equipmentId)
        {
            EquipmentState state = GetState(equipmentId);
            if (state == null || !state.IsOwned || state.Level >= state.MaxLevel)
            {
                return false;
            }

            state.Level += 1;
            SaveEquipment(state);
            NotifyChanged();
            return true;
        }

        public bool TryStarUpEquipment(string equipmentId)
        {
            EquipmentState state = GetState(equipmentId);
            if (state == null || !state.IsOwned || state.IsMaxStars)
            {
                return false;
            }

            int cost = state.StarUpCost;
            if (GetStarUpMaterialCount(equipmentId) < cost)
            {
                return false;
            }

            state.Count = Math.Max(0, state.Count - cost);
            state.Stars += 1;
            SaveEquipment(state);
            NotifyChanged();
            return true;
        }
    }
}
