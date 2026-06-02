using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class CombatTargetingService
    {
        public static void RemoveTargetLocksForSpawn(
            Dictionary<string, int> heroTargetLocks,
            Dictionary<string, int> skillTargetLocks,
            Dictionary<string, int> petTargetLocks,
            int spawnSequence)
        {
            RemoveTargetLocksForSpawn(heroTargetLocks, spawnSequence);
            RemoveTargetLocksForSpawn(skillTargetLocks, spawnSequence);
            RemoveTargetLocksForSpawn(petTargetLocks, spawnSequence);
        }

        private static bool IsFrontlineHero(HeroState hero)
        {
            return hero != null
                && (hero.Definition.Trait == HeroTrait.Melee || hero.Definition.Trait == HeroTrait.Defense);
        }

        private static void RemoveTargetLocksForSpawn(Dictionary<string, int> targetLocks, int spawnSequence)
        {
            if (targetLocks == null || targetLocks.Count <= 0)
            {
                return;
            }

            var removeKeys = new List<string>();
            foreach (KeyValuePair<string, int> entry in targetLocks)
            {
                if (entry.Value == spawnSequence)
                {
                    removeKeys.Add(entry.Key);
                }
            }

            foreach (string key in removeKeys)
            {
                targetLocks.Remove(key);
            }
        }
    }
}
