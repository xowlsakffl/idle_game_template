using System.Collections.Generic;
using System.Globalization;
using System;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public static partial class HeroDetailEquipmentListBuilder
    {
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
