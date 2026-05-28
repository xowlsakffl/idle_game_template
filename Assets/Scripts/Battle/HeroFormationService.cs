using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Save;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class HeroFormationService
    {
        public static List<string> LoadFormationHeroIds(int preset, IReadOnlyList<HeroState> heroes)
        {
            var ids = new List<string>(GameData.MaxPartyHeroes);
            bool hasSavedFormation = false;
            for (int i = 0; i < GameData.MaxPartyHeroes; i++)
            {
                string key = SaveKeys.HeroFormationSlot(preset, i);
                hasSavedFormation |= PlayerPrefs.HasKey(key);
                ids.Add(PlayerPrefs.GetString(key, string.Empty));
            }

            if (!hasSavedFormation && preset == 1 && heroes != null)
            {
                ids.Clear();
                for (int i = 0; i < GameData.MaxPartyHeroes; i++)
                {
                    ids.Add(GetDefaultFormationHeroId(heroes, i));
                }
            }

            return ids;
        }

        public static void SaveFormationHeroIds(int preset, IReadOnlyList<string> ids, SaveManager saveManager)
        {
            for (int i = 0; i < GameData.MaxPartyHeroes; i++)
            {
                string heroId = ids != null && i < ids.Count ? ids[i] : string.Empty;
                PlayerPrefs.SetString(SaveKeys.HeroFormationSlot(preset, i), heroId ?? string.Empty);
            }

            saveManager?.Flush();
        }

        public static List<string> NormalizeFormationHeroIds(IReadOnlyList<string> sourceIds, IReadOnlyList<HeroState> heroes)
        {
            var ids = new List<string>(GameData.MaxPartyHeroes);
            var usedHeroIds = new HashSet<string>();
            for (int i = 0; i < GameData.MaxPartyHeroes; i++)
            {
                string heroId = sourceIds != null && i < sourceIds.Count ? sourceIds[i] : string.Empty;
                HeroState hero = FindHero(heroes, heroId);
                if (string.IsNullOrEmpty(heroId) || hero == null || !hero.IsOwned || usedHeroIds.Contains(heroId))
                {
                    ids.Add(string.Empty);
                    continue;
                }

                usedHeroIds.Add(heroId);
                ids.Add(heroId);
            }

            return ids;
        }

        public static int GetFilledFormationCount(IReadOnlyList<string> ids)
        {
            int count = 0;
            if (ids == null)
            {
                return count;
            }

            foreach (string heroId in ids)
            {
                if (!string.IsNullOrEmpty(heroId))
                {
                    count += 1;
                }
            }

            return count;
        }

        public static void RefreshDeployedHeroes(
            IReadOnlyList<HeroState> heroes,
            int activePreset,
            IList<HeroState> deployedHeroes,
            IList<string> activeFormationHeroIds)
        {
            deployedHeroes.Clear();
            if (heroes == null)
            {
                return;
            }

            List<string> ids = NormalizeFormationHeroIds(LoadFormationHeroIds(activePreset, heroes), heroes);
            activeFormationHeroIds.Clear();
            foreach (string id in ids)
            {
                activeFormationHeroIds.Add(id);
            }

            foreach (string heroId in ids)
            {
                if (string.IsNullOrEmpty(heroId))
                {
                    continue;
                }

                HeroState hero = FindHero(heroes, heroId);
                if (hero != null && hero.IsOwned && !deployedHeroes.Contains(hero) && deployedHeroes.Count < GameData.MaxPartyHeroes)
                {
                    deployedHeroes.Add(hero);
                }
            }

            if (deployedHeroes.Count > 0)
            {
                return;
            }

            foreach (HeroState hero in heroes)
            {
                if (deployedHeroes.Count >= GameData.MaxPartyHeroes)
                {
                    break;
                }

                if (hero.IsOwned)
                {
                    deployedHeroes.Add(hero);
                }
            }

            activeFormationHeroIds.Clear();
            foreach (HeroState hero in deployedHeroes)
            {
                activeFormationHeroIds.Add(hero.Definition.Id);
            }

            while (activeFormationHeroIds.Count < GameData.MaxPartyHeroes)
            {
                activeFormationHeroIds.Add(string.Empty);
            }
        }

        private static string GetDefaultFormationHeroId(IReadOnlyList<HeroState> heroes, int formationIndex)
        {
            if (heroes == null)
            {
                return string.Empty;
            }

            int ownedIndex = 0;
            foreach (HeroState hero in heroes)
            {
                if (!hero.IsOwned)
                {
                    continue;
                }

                if (ownedIndex == formationIndex)
                {
                    return hero.Definition.Id;
                }

                ownedIndex += 1;
            }

            return string.Empty;
        }

        private static HeroState FindHero(IReadOnlyList<HeroState> heroes, string heroId)
        {
            if (heroes == null || string.IsNullOrEmpty(heroId))
            {
                return null;
            }

            foreach (HeroState hero in heroes)
            {
                if (hero.Definition.Id == heroId)
                {
                    return hero;
                }
            }

            return null;
        }
    }
}
