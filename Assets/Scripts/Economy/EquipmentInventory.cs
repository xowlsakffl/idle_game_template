using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class EquipmentInventory : MonoBehaviour
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

    public string GetOwnedSummary(int maxLines)
    {
        int lines = 0;
        var builder = new StringBuilder();
        foreach (EquipmentState state in states)
        {
            if (!state.IsOwned)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(state.Definition.SlotLabel)
                .Append(" ")
                .Append(state.Definition.RarityLabel)
                .Append(" ")
                .Append(state.Definition.DisplayName)
                .Append(" Lv.")
                .Append(state.Level)
                .Append("/")
                .Append(state.MaxLevel)
                .Append(" ")
                .Append(state.Stars)
                .Append("성")
                .Append(" ATK+")
                .Append(state.AttackBonus)
                .Append(" HP+")
                .Append(state.HpBonus)
                .Append(" x")
                .Append(state.Count);

            lines += 1;
            if (lines >= maxLines)
            {
                break;
            }
        }

        return builder.Length > 0 ? builder.ToString() : "보유 장비 없음";
    }

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
