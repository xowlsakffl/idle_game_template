using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class EquipmentInventory
    {
        public bool TryDismantleEquipment(string equipmentId, out int reward)
        {
            reward = 0;
            EquipmentState state = GetState(equipmentId);
            if (state == null || !state.IsOwned || GetAvailableCount(equipmentId) <= 0)
            {
                return false;
            }

            reward = GetDismantleReward(state, 1);
            state.Count = Math.Max(0, state.Count - 1);
            SaveEquipment(state);
            NotifyChanged();
            return true;
        }

        public int DismantleByRarity(
            HeroRarity maxRarity,
            IReadOnlyCollection<EquipmentSlot> selectedSlots,
            out int reward)
        {
            reward = 0;
            int dismantledCount = 0;
            foreach (EquipmentState state in states)
            {
                if (state == null
                    || !state.IsOwned
                    || state.Definition.Rarity > maxRarity
                    || (selectedSlots != null && !ContainsSlot(selectedSlots, state.Definition.Slot)))
                {
                    continue;
                }

                int dismantleCount = GetAvailableCount(state.Definition.Id);
                if (dismantleCount <= 0)
                {
                    continue;
                }

                reward += GetDismantleReward(state, dismantleCount);
                dismantledCount += dismantleCount;
                state.Count = Math.Max(0, state.Count - dismantleCount);
                SaveEquipment(state);
            }

            if (dismantledCount > 0)
            {
                NotifyChanged();
            }

            return dismantledCount;
        }

        public int GetDismantleReward(EquipmentState state, int count)
        {
            if (state == null || count <= 0)
            {
                return 0;
            }

            int rarityValue = GetRarityDismantleValue(state.Definition.Rarity);
            int levelValue = Math.Max(0, state.Level - 1) / 5;
            int starValue = state.Stars * 3;
            return Math.Max(1, (rarityValue + levelValue + starValue) * count);
        }
        private static int GetRarityDismantleValue(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return 5;
                case HeroRarity.Uncommon:
                    return 12;
                case HeroRarity.Rare:
                    return 30;
                case HeroRarity.Epic:
                    return 80;
                case HeroRarity.Legendary:
                    return 180;
                case HeroRarity.Mythic:
                    return 420;
                default:
                    return 5;
            }
        }

        private static bool ContainsSlot(IReadOnlyCollection<EquipmentSlot> slots, EquipmentSlot slot)
        {
            foreach (EquipmentSlot candidate in slots)
            {
                if (candidate == slot)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
