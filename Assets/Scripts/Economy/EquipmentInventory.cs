using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class EquipmentInventory : MonoBehaviour
    {
        private readonly List<EquipmentState> states = new List<EquipmentState>();
        private readonly Dictionary<string, EquipmentState> statesById = new Dictionary<string, EquipmentState>();
        private readonly Dictionary<string, Dictionary<EquipmentSlot, string>> equippedByHero = new Dictionary<string, Dictionary<EquipmentSlot, string>>();
        private SaveManager saveManager;

        public event Action Changed;

        public void Initialize(SaveManager save)
        {
            saveManager = save;
            states.Clear();
            statesById.Clear();
            equippedByHero.Clear();

            foreach (EquipmentDefinition equipment in GameData.Equipments)
            {
                int level = PlayerPrefs.GetInt(SaveKeys.EquipmentLevel(equipment.Id), 1);
                int stars = PlayerPrefs.GetInt(SaveKeys.EquipmentStars(equipment.Id), 0);
                int count = PlayerPrefs.GetInt(SaveKeys.EquipmentCount(equipment.Id), 0);
                var state = new EquipmentState(equipment, level, stars, count);
                states.Add(state);
                statesById[equipment.Id] = state;
            }

            foreach (HeroDefinition hero in GameData.Heroes)
            {
                var slots = new Dictionary<EquipmentSlot, string>();
                foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                {
                    slots[slot] = PlayerPrefs.GetString(SaveKeys.HeroEquipmentSlot(hero.Id, slot), string.Empty);
                }

                equippedByHero[hero.Id] = slots;
            }

            NotifyChanged();
        }

        public IReadOnlyList<EquipmentState> States => states;
    }
}
