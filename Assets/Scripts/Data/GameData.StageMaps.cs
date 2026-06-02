using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public static partial class GameData
    {
        private static Dictionary<string, EnemyDefinition> BuildEnemyMap()
        {
            var map = new Dictionary<string, EnemyDefinition>();
            foreach (EnemyDefinition enemy in enemies)
            {
                map[enemy.Id] = enemy;
            }

            return map;
        }

        private static Dictionary<string, BossDefinition> BuildBossMap()
        {
            var map = new Dictionary<string, BossDefinition>();
            foreach (BossDefinition boss in bosses)
            {
                map[boss.Id] = boss;
            }

            return map;
        }

        private static Dictionary<string, StageDefinition> BuildStageMap()
        {
            var map = new Dictionary<string, StageDefinition>();
            foreach (StageDefinition stage in stages)
            {
                map[stage.Id] = stage;
            }

            return map;
        }

        private static Dictionary<string, int> BuildStageIndexMap()
        {
            var map = new Dictionary<string, int>();
            for (int i = 0; i < stages.Length; i++)
            {
                map[stages[i].Id] = i;
            }

            return map;
        }
    }
}
