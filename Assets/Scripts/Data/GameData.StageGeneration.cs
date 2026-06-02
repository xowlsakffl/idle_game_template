using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public static partial class GameData
    {
        private static StageDefinition Normal(string id, int number, string enemyId, double hpMultiplier, double goldMultiplier)
        {
            return new StageDefinition(id, 1, number, StageType.Normal, enemyId, hpMultiplier, goldMultiplier, NormalStageRequiredKills, null);
        }

        private static bool TryParseStageId(string id, out int chapter, out int number)
        {
            chapter = 1;
            number = 1;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            string[] parts = id.Split('-');
            if (parts.Length != 2)
            {
                return false;
            }

            return int.TryParse(parts[0], out chapter)
                && int.TryParse(parts[1], out number)
                && chapter >= 1
                && number >= 1
                && number <= StagesPerChapter;
        }

        private static string BuildStageId(int chapter, int number)
        {
            return chapter + "-" + number;
        }

        private static StageDefinition GenerateStage(int chapter, int number)
        {
            int globalIndex = (chapter - 1) * StagesPerChapter + number;
            if (number == StagesPerChapter)
            {
                BossDefinition boss = bosses[0];
                GameNumber targetHp = GenerateTargetHp(globalIndex) * BossHpMultiplier;
                GameNumber targetGold = GenerateTargetGold(globalIndex) * BossGoldMultiplier;
                return new StageDefinition(
                    BuildStageId(chapter, number),
                    chapter,
                    number,
                    StageType.Boss,
                    boss.Id,
                    targetHp / Math.Max(1d, boss.BaseHp),
                    targetGold / Math.Max(1d, boss.ClearGold),
                    1,
                    BuildStageId(chapter, StagesPerChapter - 1));
            }

            EnemyDefinition enemy = GetGeneratedEnemy(globalIndex);
            GameNumber hpMultiplier = GenerateTargetHp(globalIndex) / Math.Max(1d, enemy.BaseHp);
            GameNumber goldMultiplier = GenerateTargetGold(globalIndex) / Math.Max(1d, enemy.BaseGold);
            return new StageDefinition(
                BuildStageId(chapter, number),
                chapter,
                number,
                StageType.Normal,
                enemy.Id,
                hpMultiplier,
                goldMultiplier,
                NormalStageRequiredKills,
                null);
        }

        private static EnemyDefinition GetGeneratedEnemy(int globalIndex)
        {
            int groupIndex = Math.Max(0, (globalIndex - 1) / 4);
            return enemies[groupIndex % enemies.Length];
        }

        private static GameNumber GenerateTargetHp(int globalIndex)
        {
            double growthValue = GeneratedStageHpBase
                * Math.Pow(NormalStageHpGrowth, globalIndex - 1 + GeneratedStageHpOffset)
                + globalIndex * 6d;
            return ClampNumber(growthValue);
        }

        private static GameNumber GenerateTargetGold(int globalIndex)
        {
            double growthValue = GeneratedStageGoldBase
                * Math.Pow(NormalStageGoldGrowth, globalIndex - 1 + GeneratedStageGoldOffset)
                + globalIndex * 0.45d;
            return ClampNumber(growthValue);
        }
    }
}
