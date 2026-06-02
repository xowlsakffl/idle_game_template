using System;
using System.Collections.Generic;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;

namespace IdleGame.Editor
{
    public static partial class IdleGameQaRunner
    {
        private static void RunWithFreshPrefs(PlayerPrefsScope prefs, Action<RuntimeHarness> test)
        {
            prefs.ClearKnownKeys();
            using (var game = new RuntimeHarness())
            {
                test(game);
            }
        }

        private static void SimulateUntil(RuntimeHarness game, Func<bool> condition, float timeoutSeconds, string failureMessage)
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                game.Battle.DebugSimulateSeconds(0.1f);
                elapsed += 0.1f;
            }

            AssertTrue(condition(), failureMessage + " within " + timeoutSeconds + " seconds");
        }

        private static float SimulateUntilState(
            RuntimeHarness game,
            Func<bool> condition,
            float timeoutSeconds,
            bool spendExp)
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                game.Battle.DebugSimulateSeconds(0.25f);
                if (spendExp)
                {
                    SpendAffordableGrowth(game);
                }

                elapsed += 0.25f;
            }

            return elapsed;
        }

        private static int SpendCheapestHeroLevels(RuntimeHarness game)
        {
            int upgrades = 0;
            bool upgraded;
            do
            {
                upgraded = false;
                HeroState cheapest = null;
                foreach (HeroState hero in game.Battle.Heroes)
                {
                    if (hero.LevelUpCost <= game.Wallet.HeroExpItem
                        && (cheapest == null || hero.LevelUpCost < cheapest.LevelUpCost))
                    {
                        cheapest = hero;
                    }
                }

                if (cheapest != null && game.Battle.TryLevelUpHero(cheapest.Definition.Id))
                {
                    upgrades += 1;
                    upgraded = true;
                }
            }
            while (upgraded);

            return upgrades;
        }

        private static int SpendCheapestAccountAbilities(RuntimeHarness game)
        {
            int upgrades = 0;
            bool upgraded;
            do
            {
                upgraded = false;
                AbilityState cheapest = null;
                foreach (AbilityState ability in game.Abilities.States)
                {
                    if (!ability.IsMaxed
                        && ability.LevelUpCost <= game.Wallet.Gold
                        && (cheapest == null || ability.LevelUpCost < cheapest.LevelUpCost))
                    {
                        cheapest = ability;
                    }
                }

                if (cheapest != null && game.Abilities.TryLevelUp(cheapest.Definition.Kind))
                {
                    upgrades += 1;
                    upgraded = true;
                }
            }
            while (upgraded);

            return upgrades;
        }

        private static int SpendAffordableGrowth(RuntimeHarness game)
        {
            int upgrades;
            int total = 0;
            do
            {
                upgrades = SpendCheapestHeroLevels(game) + SpendCheapestAccountAbilities(game);
                total += upgrades;
            }
            while (upgrades > 0);

            return total;
        }

        private static string FormatHeroLevels(BattleManager battle)
        {
            var parts = new List<string>();
            foreach (HeroState hero in battle.Heroes)
            {
                parts.Add(hero.Definition.Id + ":Lv" + hero.Level + "/S" + hero.Stars + "/ATK" + hero.AttackPower);
            }

            return string.Join(", ", parts);
        }

        private static string FormatAbilityLevels(AbilityManager abilities)
        {
            var parts = new List<string>();
            foreach (AbilityState ability in abilities.States)
            {
                parts.Add(ability.Definition.Id + ":Lv" + ability.Level);
            }

            return string.Join(", ", parts);
        }

        private static string FormatSeconds(float seconds)
        {
            return seconds.ToString("0.0") + "s";
        }

        private static int GetTotalShards(BattleManager battle)
        {
            int total = 0;
            foreach (HeroState hero in battle.Heroes)
            {
                total += hero.Shards;
            }

            return total;
        }

        private static int GetMaxVisibleEnemySpawnSequence(BattleManager battle)
        {
            int maxSequence = -1;
            for (int i = 0; i < battle.VisibleEnemyCount; i++)
            {
                maxSequence = Math.Max(maxSequence, battle.GetVisibleEnemySpawnSequence(i));
            }

            return maxSequence;
        }

        private static HeroState FindHeroState(BattleManager battle, string heroId)
        {
            foreach (HeroState hero in battle.Heroes)
            {
                if (hero.Definition.Id == heroId)
                {
                    return hero;
                }
            }

            throw new InvalidOperationException("QA failed: missing hero " + heroId);
        }

        private static EquipmentState FindFirstOwnedEquipment(EquipmentInventory equipmentInventory)
        {
            foreach (EquipmentState state in equipmentInventory.States)
            {
                if (state.IsOwned)
                {
                    return state;
                }
            }

            throw new InvalidOperationException("QA failed: missing owned equipment");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("QA failed: " + message);
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    "QA failed: " + message + " Expected=" + expected + " Actual=" + actual);
            }
        }
    }
}
