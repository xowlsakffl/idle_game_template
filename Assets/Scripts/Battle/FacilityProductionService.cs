using System;
using System.Collections.Generic;
using IdleGame.Data;

namespace IdleGame.Battle
{
    internal static class FacilityProductionService
    {
        public static bool RefreshProduction(
            FacilityState state,
            Func<FacilityState, GameNumber> productionPerHourResolver,
            long nowTicks)
        {
            if (state == null)
            {
                return false;
            }

            if (state.LastUpdateUtcTicks <= 0L)
            {
                state.LastUpdateUtcTicks = nowTicks;
                return true;
            }

            double elapsedSeconds = TimeSpan.FromTicks(Math.Max(0L, nowTicks - state.LastUpdateUtcTicks)).TotalSeconds;
            if (elapsedSeconds <= 0d)
            {
                return false;
            }

            GameNumber productionPerHour = productionPerHourResolver != null
                ? productionPerHourResolver(state)
                : GameNumber.Zero;
            GameNumber maxStored = GetMaxStoredAmount(productionPerHour);
            GameNumber produced = productionPerHour * (elapsedSeconds / FacilityDefinition.ProductionCycleSeconds);
            state.StoredAmount = GameNumber.Min(maxStored, GameNumber.Max(GameNumber.Zero, state.StoredAmount + produced));
            state.LastUpdateUtcTicks = nowTicks;
            return true;
        }

        public static GameNumber GetProductionPerHour(
            FacilityState state,
            Func<FacilityState, double> heroBonusPercentResolver)
        {
            return state != null
                ? state.Definition.GetProductionPerHour(state.Level, heroBonusPercentResolver != null ? heroBonusPercentResolver(state) : 0d)
                : GameNumber.Zero;
        }

        public static GameNumber GetMaxStoredAmount(GameNumber productionPerHour)
        {
            return productionPerHour * (FacilityDefinition.MaxAccumulatedSeconds / FacilityDefinition.ProductionCycleSeconds);
        }

        public static double GetHeroBonusPercent(FacilityState state, Func<string, HeroState> findHero)
        {
            if (state == null)
            {
                return 0d;
            }

            double total = 0d;
            for (int i = 0; i < state.UnlockedSlotCount; i++)
            {
                HeroState hero = findHero?.Invoke(state.GetAssignedHeroId(i));
                if (hero != null && hero.IsOwned)
                {
                    total += GetHeroProductionBonusPercent(hero);
                }
            }

            return Math.Min(FacilityDefinition.MaxHeroProductionBonusPercent, total);
        }

        public static double GetHeroProductionBonusPercent(HeroState hero)
        {
            if (hero == null)
            {
                return 0d;
            }

            double rarity = Math.Max(0, (int)hero.Definition.Rarity);
            double score = hero.AttackPower * 0.018d
                + hero.MaxHp * 0.006d
                + hero.Level * 0.025d
                + hero.Stars * 0.35d
                + rarity * 0.85d;
            return Math.Max(1d, Math.Min(10d, score));
        }

        public static int FillEmptyAssignments(
            FacilityState state,
            IEnumerable<HeroState> heroes,
            HashSet<string> usedHeroIds)
        {
            if (state == null)
            {
                return 0;
            }

            if (usedHeroIds == null)
            {
                usedHeroIds = new HashSet<string>();
            }

            var localUsedHeroIds = new HashSet<string>();
            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                string heroId = state.GetAssignedHeroId(slot);
                bool unlocked = slot < state.UnlockedSlotCount;
                HeroState hero = FindHero(heroes, heroId);
                if (!unlocked
                    || string.IsNullOrEmpty(heroId)
                    || hero == null
                    || !hero.IsOwned
                    || localUsedHeroIds.Contains(heroId)
                    || usedHeroIds.Contains(heroId))
                {
                    state.SetAssignedHeroId(slot, string.Empty);
                    continue;
                }

                localUsedHeroIds.Add(heroId);
                usedHeroIds.Add(heroId);
            }

            List<HeroState> candidates = GetAssignmentCandidates(heroes, usedHeroIds);
            int candidateIndex = 0;
            int assigned = 0;
            for (int slot = 0; slot < state.UnlockedSlotCount; slot++)
            {
                if (!string.IsNullOrEmpty(state.GetAssignedHeroId(slot)))
                {
                    continue;
                }

                while (candidateIndex < candidates.Count && usedHeroIds.Contains(candidates[candidateIndex].Definition.Id))
                {
                    candidateIndex += 1;
                }

                if (candidateIndex >= candidates.Count)
                {
                    break;
                }

                string heroId = candidates[candidateIndex].Definition.Id;
                state.SetAssignedHeroId(slot, heroId);
                usedHeroIds.Add(heroId);
                assigned += 1;
                candidateIndex += 1;
            }

            return assigned;
        }

        public static HashSet<string> GetAssignedHeroIdsExcept(IEnumerable<FacilityState> facilities, string excludedFacilityId)
        {
            var used = new HashSet<string>();
            if (facilities == null)
            {
                return used;
            }

            foreach (FacilityState facility in facilities)
            {
                if (facility == null || facility.Definition.Id == excludedFacilityId)
                {
                    continue;
                }

                for (int i = 0; i < facility.UnlockedSlotCount; i++)
                {
                    string heroId = facility.GetAssignedHeroId(i);
                    if (!string.IsNullOrEmpty(heroId))
                    {
                        used.Add(heroId);
                    }
                }
            }

            return used;
        }

        private static List<HeroState> GetAssignmentCandidates(IEnumerable<HeroState> heroes, HashSet<string> usedHeroIds)
        {
            var candidates = new List<HeroState>();
            if (heroes == null)
            {
                return candidates;
            }

            foreach (HeroState hero in heroes)
            {
                if (hero != null
                    && hero.IsOwned
                    && (usedHeroIds == null || !usedHeroIds.Contains(hero.Definition.Id)))
                {
                    candidates.Add(hero);
                }
            }

            candidates.Sort((left, right) => GetHeroSortScore(right).CompareTo(GetHeroSortScore(left)));
            return candidates;
        }

        private static double GetHeroSortScore(HeroState hero)
        {
            if (hero == null)
            {
                return 0d;
            }

            return hero.AttackPower
                + hero.MaxHp * 0.25d
                + hero.Level * 6d
                + hero.Stars * 120d
                + Math.Max(0, (int)hero.Definition.Rarity) * 220d;
        }

        private static HeroState FindHero(IEnumerable<HeroState> heroes, string heroId)
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
