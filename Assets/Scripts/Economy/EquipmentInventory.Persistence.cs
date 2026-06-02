using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class EquipmentInventory
    {
        private void SaveEquipment(EquipmentState state)
        {
            PlayerPrefs.SetInt(SaveKeys.EquipmentLevel(state.Definition.Id), state.Level);
            PlayerPrefs.SetInt(SaveKeys.EquipmentStars(state.Definition.Id), state.Stars);
            PlayerPrefs.SetInt(SaveKeys.EquipmentCount(state.Definition.Id), state.Count);
            saveManager.Flush();
        }
        private void SaveHeroEquipmentSlot(string heroId, EquipmentSlot slot, string equipmentId)
        {
            PlayerPrefs.SetString(SaveKeys.HeroEquipmentSlot(heroId, slot), equipmentId ?? string.Empty);
            saveManager.Flush();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
