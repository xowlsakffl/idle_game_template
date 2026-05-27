using System.Collections.Generic;
using System.Globalization;
using System;
using IdleGame.Data;

namespace IdleGame.UI.Hero.Formation
{
    public static class HeroFormationDraftRules
    {
        public const int MinimumFilledSlots = 1;
        private static readonly StringComparer KoreanNameComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ko-KR"), false);

        public static List<string> CreateDraft(IReadOnlyList<string> savedHeroIds, int slotCount)
        {
            var draft = new List<string>(Math.Max(0, slotCount));
            for (int i = 0; i < slotCount; i++)
            {
                draft.Add(savedHeroIds != null && i < savedHeroIds.Count ? savedHeroIds[i] : string.Empty);
            }

            return draft;
        }

        public static bool HasPendingChanges(
            IReadOnlyList<string> editingHeroIds,
            IReadOnlyList<string> savedHeroIds,
            bool isDirty,
            int selectedPreset,
            int activePreset,
            int slotCount)
        {
            if (!MatchesSaved(editingHeroIds, savedHeroIds, slotCount))
            {
                return true;
            }

            if (isDirty)
            {
                return true;
            }

            return selectedPreset != activePreset && CountFilled(editingHeroIds) > 0;
        }

        public static bool MatchesSaved(IReadOnlyList<string> editingHeroIds, IReadOnlyList<string> savedHeroIds, int slotCount)
        {
            for (int i = 0; i < slotCount; i++)
            {
                string editingHeroId = editingHeroIds != null && i < editingHeroIds.Count ? editingHeroIds[i] : string.Empty;
                string savedHeroId = savedHeroIds != null && i < savedHeroIds.Count ? savedHeroIds[i] : string.Empty;
                if (editingHeroId != savedHeroId)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Contains(IReadOnlyList<string> heroIds, string heroId)
        {
            return IndexOf(heroIds, heroId) >= 0;
        }

        public static int IndexOf(IReadOnlyList<string> heroIds, string heroId)
        {
            if (heroIds == null || string.IsNullOrEmpty(heroId))
            {
                return -1;
            }

            for (int i = 0; i < heroIds.Count; i++)
            {
                if (heroIds[i] == heroId)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int CountFilled(IReadOnlyList<string> heroIds)
        {
            if (heroIds == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < heroIds.Count; i++)
            {
                if (!string.IsNullOrEmpty(heroIds[i]))
                {
                    count += 1;
                }
            }

            return count;
        }

        public static bool TryRemoveAt(IList<string> heroIds, int slotIndex)
        {
            if (heroIds == null
                || slotIndex < 0
                || slotIndex >= heroIds.Count
                || string.IsNullOrEmpty(heroIds[slotIndex])
                || CountFilledList(heroIds) <= MinimumFilledSlots)
            {
                return false;
            }

            heroIds[slotIndex] = string.Empty;
            return true;
        }

        public static bool TryPlaceHero(IList<string> heroIds, int slotIndex, string heroId)
        {
            if (heroIds == null || slotIndex < 0 || slotIndex >= heroIds.Count || string.IsNullOrEmpty(heroId))
            {
                return false;
            }

            for (int i = 0; i < heroIds.Count; i++)
            {
                if (heroIds[i] == heroId)
                {
                    heroIds[i] = string.Empty;
                }
            }

            heroIds[slotIndex] = heroId;
            return true;
        }

        public static List<string> BuildAutoDraft(IEnumerable<HeroState> heroes, Func<HeroState, double> getCombatPower, int slotCount)
        {
            var sortedHeroes = new List<HeroState>();
            if (heroes != null)
            {
                foreach (HeroState hero in heroes)
                {
                    if (hero != null && hero.IsOwned)
                    {
                        sortedHeroes.Add(hero);
                    }
                }
            }

            sortedHeroes.Sort((left, right) => CompareHeroesForAutoFormation(left, right, getCombatPower));

            var draft = new List<string>(Math.Max(0, slotCount));
            for (int i = 0; i < slotCount; i++)
            {
                draft.Add(i < sortedHeroes.Count ? sortedHeroes[i].Definition.Id : string.Empty);
            }

            return draft;
        }

        public static List<HeroDefinition> SortRosterDefinitions(IEnumerable<HeroDefinition> heroes)
        {
            var sortedHeroes = heroes != null ? new List<HeroDefinition>(heroes) : new List<HeroDefinition>();
            sortedHeroes.Sort(CompareHeroRosterDefinitions);
            return sortedHeroes;
        }

        private static int CompareHeroesForAutoFormation(HeroState left, HeroState right, Func<HeroState, double> getCombatPower)
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

            double leftPower = getCombatPower != null ? getCombatPower(left) : 0d;
            double rightPower = getCombatPower != null ? getCombatPower(right) : 0d;
            int powerCompare = rightPower.CompareTo(leftPower);
            if (powerCompare != 0)
            {
                return powerCompare;
            }

            int starCompare = right.Stars.CompareTo(left.Stars);
            if (starCompare != 0)
            {
                return starCompare;
            }

            int levelCompare = right.Level.CompareTo(left.Level);
            if (levelCompare != 0)
            {
                return levelCompare;
            }

            int rarityCompare = ((int)right.Definition.Rarity).CompareTo((int)left.Definition.Rarity);
            if (rarityCompare != 0)
            {
                return rarityCompare;
            }

            return string.CompareOrdinal(left.Definition.Id, right.Definition.Id);
        }

        private static int CountFilledList(IList<string> heroIds)
        {
            if (heroIds == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < heroIds.Count; i++)
            {
                if (!string.IsNullOrEmpty(heroIds[i]))
                {
                    count += 1;
                }
            }

            return count;
        }

        private static int CompareHeroRosterDefinitions(HeroDefinition left, HeroDefinition right)
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

            int rarityCompare = ((int)left.Rarity).CompareTo((int)right.Rarity);
            if (rarityCompare != 0)
            {
                return rarityCompare;
            }

            int nameCompare = KoreanNameComparer.Compare(left.DisplayName, right.DisplayName);
            if (nameCompare != 0)
            {
                return nameCompare;
            }

            return string.CompareOrdinal(left.Id, right.Id);
        }
    }
}
