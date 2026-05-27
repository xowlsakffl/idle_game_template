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

    public static class HeroDetailEquipmentListBuilder
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

        public static string BuildEquippedOwnerText(
            string equipmentId,
            EquipmentInventory equipmentInventory,
            IEnumerable<HeroState> heroes,
            Func<HeroDefinition, string> shortHeroLabelFactory)
        {
            List<HeroState> equippedHeroes = GetEquippedHeroes(equipmentId, equipmentInventory, heroes);
            if (equippedHeroes.Count <= 0)
            {
                return string.Empty;
            }

            var labels = new List<string>();
            foreach (HeroState hero in equippedHeroes)
            {
                labels.Add(shortHeroLabelFactory != null ? shortHeroLabelFactory(hero.Definition) : hero.Definition.DisplayName);
            }

            return string.Join(", ", labels);
        }

        public static string BuildCopyKey(string equipmentId, int copyIndex)
        {
            return equipmentId + "#" + copyIndex;
        }

        public static string BuildEquippedCopyKey(string equipmentId, string heroId)
        {
            return equipmentId + "@equipped:" + heroId;
        }

        public static string GetEquipmentIdFromCopyKey(string equipmentCopyKey)
        {
            if (string.IsNullOrEmpty(equipmentCopyKey))
            {
                return string.Empty;
            }

            int separatorIndex = equipmentCopyKey.LastIndexOf('#');
            return separatorIndex >= 0 ? equipmentCopyKey.Substring(0, separatorIndex) : equipmentCopyKey;
        }

        public static int GetCopyIndexFromCopyKey(string equipmentCopyKey)
        {
            if (string.IsNullOrEmpty(equipmentCopyKey))
            {
                return -1;
            }

            int separatorIndex = equipmentCopyKey.LastIndexOf('#');
            if (separatorIndex < 0 || separatorIndex >= equipmentCopyKey.Length - 1)
            {
                return -1;
            }

            return int.TryParse(equipmentCopyKey.Substring(separatorIndex + 1), out int copyIndex) ? copyIndex : -1;
        }

        private static List<HeroState> GetEquippedHeroes(
            string equipmentId,
            EquipmentInventory equipmentInventory,
            IEnumerable<HeroState> heroes)
        {
            var equippedHeroes = new List<HeroState>();
            if (string.IsNullOrEmpty(equipmentId) || equipmentInventory == null || heroes == null)
            {
                return equippedHeroes;
            }

            foreach (HeroState hero in heroes)
            {
                if (hero != null
                    && hero.IsOwned
                    && equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, equipmentId))
                {
                    equippedHeroes.Add(hero);
                }
            }

            equippedHeroes.Sort((left, right) => KoreanNameComparer.Compare(left.Definition.DisplayName, right.Definition.DisplayName));
            return equippedHeroes;
        }

        private static int CompareForDismantleList(EquipmentDefinition left, EquipmentDefinition right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            int rarityCompare = ((int)right.Rarity).CompareTo((int)left.Rarity);
            if (rarityCompare != 0)
            {
                return rarityCompare;
            }

            int slotCompare = ((int)left.Slot).CompareTo((int)right.Slot);
            if (slotCompare != 0)
            {
                return slotCompare;
            }

            return KoreanNameComparer.Compare(left.DisplayName, right.DisplayName);
        }
    }
}
