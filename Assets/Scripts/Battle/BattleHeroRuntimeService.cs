using System;
using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class BattleHeroRuntimeService
    {
        public static void ResetDamageMeter(
            IDictionary<string, GameNumber> damageMeter,
            IReadOnlyList<HeroState> deployedHeroes)
        {
            if (damageMeter == null)
            {
                return;
            }

            damageMeter.Clear();
            if (deployedHeroes == null)
            {
                return;
            }

            foreach (HeroState hero in deployedHeroes)
            {
                if (hero != null)
                {
                    damageMeter[hero.Definition.Id] = GameNumber.Zero;
                }
            }
        }

        public static void ResetRuntimeStates(
            IDictionary<string, BattleHeroRuntimeState> runtimeStates,
            IReadOnlyList<HeroState> deployedHeroes,
            Func<HeroState, int, Vector2> getSlotPosition,
            Func<HeroState, float> getMaxHp)
        {
            if (runtimeStates == null)
            {
                return;
            }

            runtimeStates.Clear();
            if (deployedHeroes == null)
            {
                return;
            }

            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                HeroState hero = deployedHeroes[i];
                if (hero != null)
                {
                    runtimeStates[hero.Definition.Id] = CreateRuntimeState(hero, i, getSlotPosition, getMaxHp);
                }
            }
        }

        public static void EnsureRuntimeStates(
            IDictionary<string, BattleHeroRuntimeState> runtimeStates,
            IReadOnlyList<HeroState> deployedHeroes,
            Func<HeroState, int, Vector2> getSlotPosition,
            Func<HeroState, float> getMaxHp)
        {
            if (runtimeStates == null || deployedHeroes == null)
            {
                return;
            }

            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                HeroState hero = deployedHeroes[i];
                if (hero == null)
                {
                    continue;
                }

                if (!runtimeStates.TryGetValue(hero.Definition.Id, out BattleHeroRuntimeState state))
                {
                    runtimeStates[hero.Definition.Id] = CreateRuntimeState(hero, i, getSlotPosition, getMaxHp);
                    continue;
                }

                state.SlotIndex = i;
                state.MaxHp = getMaxHp?.Invoke(hero) ?? 1f;
                state.Hp = Mathf.Min(state.Hp, state.MaxHp);
            }

            RemoveUndeployedRuntimeStates(runtimeStates, deployedHeroes);
        }

        public static bool IsHeroAlive(
            IReadOnlyDictionary<string, BattleHeroRuntimeState> runtimeStates,
            string heroId)
        {
            return !string.IsNullOrEmpty(heroId)
                && runtimeStates != null
                && runtimeStates.TryGetValue(heroId, out BattleHeroRuntimeState state)
                && state.IsAlive;
        }

        private static BattleHeroRuntimeState CreateRuntimeState(
            HeroState hero,
            int slotIndex,
            Func<HeroState, int, Vector2> getSlotPosition,
            Func<HeroState, float> getMaxHp)
        {
            Vector2 position = getSlotPosition?.Invoke(hero, slotIndex) ?? Vector2.zero;
            float maxHp = getMaxHp?.Invoke(hero) ?? 1f;
            return new BattleHeroRuntimeState(hero, position, slotIndex, maxHp);
        }

        private static void RemoveUndeployedRuntimeStates(
            IDictionary<string, BattleHeroRuntimeState> runtimeStates,
            IReadOnlyList<HeroState> deployedHeroes)
        {
            var deployedHeroIds = new HashSet<string>();
            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                HeroState hero = deployedHeroes[i];
                if (hero != null)
                {
                    deployedHeroIds.Add(hero.Definition.Id);
                }
            }

            var removeKeys = new List<string>();
            foreach (string heroId in runtimeStates.Keys)
            {
                if (!deployedHeroIds.Contains(heroId))
                {
                    removeKeys.Add(heroId);
                }
            }

            foreach (string heroId in removeKeys)
            {
                runtimeStates.Remove(heroId);
            }
        }
    }
}
