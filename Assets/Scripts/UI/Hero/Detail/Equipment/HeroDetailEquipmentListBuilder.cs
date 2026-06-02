using System.Collections.Generic;
using System.Globalization;
using System;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public sealed class HeroDetailEquipmentInventoryListEntry
    {
        public EquipmentDefinition Equipment;
        public EquipmentState State;
        public string CardKey;
        public string EquippedHeroId;
        public int CopyNumber;
    }

    public sealed class HeroDetailEquipmentInventoryListState
    {
        public readonly List<HeroDetailEquipmentInventoryListEntry> Entries = new List<HeroDetailEquipmentInventoryListEntry>();
        public int OwnedCount;
        public int VisibleCount => Entries.Count;
    }

    public sealed class HeroDetailEquipmentDismantleListEntry
    {
        public EquipmentDefinition Equipment;
        public EquipmentState State;
        public string CardKey;
        public int CopyNumber;
        public int Reward;
        public bool Selected;
    }

    public sealed class HeroDetailEquipmentDismantleListState
    {
        public readonly List<HeroDetailEquipmentDismantleListEntry> Entries = new List<HeroDetailEquipmentDismantleListEntry>();
        public int VisibleCount => Entries.Count;
        public int SelectedCount;
        public int SelectedReward;
    }

    public sealed class HeroDetailEquipmentBulkDismantleCandidateState
    {
        public int Count;
        public int Reward;
    }

    public static partial class HeroDetailEquipmentListBuilder
    {
        private static readonly StringComparer KoreanNameComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ko-KR"), false);

        public static HeroDetailEquipmentInventoryListState BuildInventoryList(
            IEnumerable<EquipmentDefinition> equipmentDefinitions,
            EquipmentInventory equipmentInventory,
            IEnumerable<HeroState> heroes,
            ICollection<EquipmentSlot> selectedSlots)
        {
            var result = new HeroDetailEquipmentInventoryListState();
            if (equipmentDefinitions == null || equipmentInventory == null)
            {
                return result;
            }

            foreach (EquipmentDefinition equipment in equipmentDefinitions)
            {
                EquipmentState state = equipmentInventory.GetState(equipment.Id);
                bool owned = state != null && state.IsOwned;
                if (!owned)
                {
                    continue;
                }

                result.OwnedCount += state.Count;
                if (selectedSlots == null || !selectedSlots.Contains(equipment.Slot))
                {
                    continue;
                }

                foreach (HeroState equippedHero in GetEquippedHeroes(equipment.Id, equipmentInventory, heroes))
                {
                    result.Entries.Add(new HeroDetailEquipmentInventoryListEntry
                    {
                        Equipment = equipment,
                        State = state,
                        CardKey = BuildEquippedCopyKey(equipment.Id, equippedHero.Definition.Id),
                        EquippedHeroId = equippedHero.Definition.Id,
                        CopyNumber = 0
                    });
                }

                int availableCount = equipmentInventory.GetAvailableCount(equipment.Id);
                for (int i = 0; i < availableCount; i++)
                {
                    result.Entries.Add(new HeroDetailEquipmentInventoryListEntry
                    {
                        Equipment = equipment,
                        State = state,
                        CardKey = BuildCopyKey(equipment.Id, i),
                        EquippedHeroId = string.Empty,
                        CopyNumber = i + 1
                    });
                }
            }

            return result;
        }

        public static HeroDetailEquipmentDismantleListState BuildDismantleList(
            IEnumerable<EquipmentDefinition> equipmentDefinitions,
            EquipmentInventory equipmentInventory,
            ICollection<EquipmentSlot> selectedSlots,
            ISet<string> selectedCardKeys)
        {
            var result = new HeroDetailEquipmentDismantleListState();
            if (equipmentDefinitions == null || equipmentInventory == null)
            {
                return result;
            }

            var sortedEquipment = new List<EquipmentDefinition>(equipmentDefinitions);
            sortedEquipment.Sort(CompareForDismantleList);
            foreach (EquipmentDefinition equipment in sortedEquipment)
            {
                EquipmentState state = equipmentInventory.GetState(equipment.Id);
                int availableCount = equipmentInventory.GetAvailableCount(equipment.Id);
                bool visible = state != null
                    && state.IsOwned
                    && availableCount > 0
                    && selectedSlots != null
                    && selectedSlots.Contains(equipment.Slot);
                if (!visible)
                {
                    continue;
                }

                int reward = equipmentInventory.GetDismantleReward(state, 1);
                for (int i = 0; i < availableCount; i++)
                {
                    string cardKey = BuildCopyKey(equipment.Id, i);
                    bool selected = selectedCardKeys != null && selectedCardKeys.Contains(cardKey);
                    result.Entries.Add(new HeroDetailEquipmentDismantleListEntry
                    {
                        Equipment = equipment,
                        State = state,
                        CardKey = cardKey,
                        CopyNumber = i + 1,
                        Reward = reward,
                        Selected = selected
                    });

                    if (selected)
                    {
                        result.SelectedCount += 1;
                        result.SelectedReward += reward;
                    }
                }
            }

            return result;
        }

        public static void PruneInvalidDismantleSelections(
            ISet<string> selectedCardKeys,
            EquipmentInventory equipmentInventory,
            ICollection<EquipmentSlot> selectedSlots)
        {
            if (selectedCardKeys == null || selectedCardKeys.Count <= 0)
            {
                return;
            }

            var selectedKeys = new List<string>(selectedCardKeys);
            foreach (string equipmentCopyKey in selectedKeys)
            {
                string equipmentId = GetEquipmentIdFromCopyKey(equipmentCopyKey);
                int copyIndex = GetCopyIndexFromCopyKey(equipmentCopyKey);
                EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
                int availableCount = equipmentInventory != null ? equipmentInventory.GetAvailableCount(equipmentId) : 0;
                if (state == null
                    || !state.IsOwned
                    || availableCount <= 0
                    || copyIndex < 0
                    || copyIndex >= availableCount
                    || selectedSlots == null
                    || !selectedSlots.Contains(state.Definition.Slot))
                {
                    selectedCardKeys.Remove(equipmentCopyKey);
                }
            }
        }

        public static HeroDetailEquipmentBulkDismantleCandidateState CountBulkDismantleCandidates(
            EquipmentInventory equipmentInventory,
            HeroRarity maxRarity)
        {
            var result = new HeroDetailEquipmentBulkDismantleCandidateState();
            if (equipmentInventory == null)
            {
                return result;
            }

            foreach (EquipmentState state in equipmentInventory.States)
            {
                if (state == null
                    || !state.IsOwned
                    || state.Definition.Rarity > maxRarity)
                {
                    continue;
                }

                int availableCount = equipmentInventory.GetAvailableCount(state.Definition.Id);
                if (availableCount <= 0)
                {
                    continue;
                }

                result.Count += availableCount;
                result.Reward += equipmentInventory.GetDismantleReward(state, availableCount);
            }

            return result;
        }
    }
}
