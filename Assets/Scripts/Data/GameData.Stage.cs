using System.Collections.Generic;
using System;
using UnityEngine;

namespace IdleGame.Data
{
    public static partial class GameData
    {
        public static EnemyDefinition GetEnemy(string id)
        {
            return enemiesById.TryGetValue(id, out EnemyDefinition enemy) ? enemy : enemies[0];
        }

        public static BossDefinition GetBoss(string id)
        {
            return bossesById.TryGetValue(id, out BossDefinition boss) ? boss : bosses[0];
        }

        public static StageDefinition GetStage(string id)
        {
            if (!string.IsNullOrEmpty(id) && stagesById.TryGetValue(id, out StageDefinition stage))
            {
                return stage;
            }

            return TryParseStageId(id, out int chapter, out int number)
                ? GenerateStage(chapter, number)
                : stages[0];
        }

        public static string GetNextStageId(string currentStageId)
        {
            if (!TryParseStageId(currentStageId, out int chapter, out int number))
            {
                int index = GetStageIndex(currentStageId);
                return index >= 0 && index < stages.Length - 1 ? stages[index + 1].Id : null;
            }

            if (number < StagesPerChapter)
            {
                return BuildStageId(chapter, number + 1);
            }

            return BuildStageId(chapter + 1, 1);
        }

        public static string GetPreviousNormalStageId(string currentStageId)
        {
            if (TryParseStageId(currentStageId, out int chapter, out int number))
            {
                if (number > 1)
                {
                    return BuildStageId(chapter, Math.Min(number - 1, StagesPerChapter - 1));
                }

                return chapter > 1 ? BuildStageId(chapter - 1, StagesPerChapter - 1) : FirstStageId;
            }

            return FirstStageId;
        }

        public static bool IsStageUnlocked(string stageId, string highestStageId)
        {
            return GetStageIndex(stageId) <= GetStageIndex(highestStageId);
        }

        public static string MaxStageId(string left, string right)
        {
            return GetStageIndex(left) >= GetStageIndex(right) ? left : right;
        }

        public static int GetStageIndex(string stageId)
        {
            if (TryParseStageId(stageId, out int chapter, out int number))
            {
                return (chapter - 1) * StagesPerChapter + number - 1;
            }

            return stageIndexesById.TryGetValue(stageId ?? string.Empty, out int index) ? index : 0;
        }

        public static GameNumber GetEnemyHp(StageDefinition stage)
        {
            EnemyDefinition enemy = GetEnemy(stage.TargetId);
            return ClampNumber(GameNumber.Floor(enemy.BaseHp * stage.HpMultiplier));
        }

        public static GameNumber GetEnemyGold(StageDefinition stage)
        {
            EnemyDefinition enemy = GetEnemy(stage.TargetId);
            return ClampNumber(GameNumber.Floor(enemy.BaseGold * stage.GoldMultiplier));
        }

        public static GameNumber GetEnemyHeroExpItem(StageDefinition stage)
        {
            return ClampNumber(GameNumber.Ceiling(GetEnemyGold(stage) * 0.20d));
        }

        public static StageClearReward GetStageFirstClearReward(StageDefinition stage)
        {
            if (stage == null)
            {
                return StageClearReward.Empty;
            }

            if (stage.Type == StageType.Boss)
            {
                int chapter = Math.Max(1, stage.Chapter);
                return new StageClearReward(
                    heroSummonTickets: 1,
                    equipmentSummonTickets: 1,
                    ruby: 100,
                    heroExpItems: 30 + chapter * 10,
                    equipmentExpItems: 25 + chapter * 8,
                    heroTranscendStones: chapter % 5 == 0 ? 5 : 1);
            }

            int stageTier = Math.Max(1, stage.Chapter);
            int heroTickets = stage.Number == 10 ? 1 : 0;
            int equipmentTickets = stage.Number == 5 || stage.Number == 15 ? 1 : 0;
            int ruby = stage.Number == 10 ? 25 : 0;
            int heroExp = stage.Number > 0 && stage.Number % 3 == 0 ? 3 + stageTier * 2 : 0;
            int equipmentExp = stage.Number > 0 && stage.Number % 6 == 0 ? 3 + stageTier * 2 : 0;

            return new StageClearReward(heroTickets, equipmentTickets, ruby, heroExp, equipmentExp, 0);
        }

        public static GameNumber GetBossHp(StageDefinition stage)
        {
            BossDefinition boss = GetBoss(stage.TargetId);
            return ClampNumber(GameNumber.Floor(boss.BaseHp * stage.HpMultiplier));
        }

        public static GameNumber GetBossClearGold(StageDefinition stage)
        {
            BossDefinition boss = GetBoss(stage.TargetId);
            return ClampNumber(GameNumber.Floor(boss.ClearGold * stage.GoldMultiplier));
        }

        public static GameNumber GetOfflineGoldPerSecond(string stageId)
        {
            StageDefinition stage = GetStage(stageId);
            if (stage.Type == StageType.Boss)
            {
                stage = GetStage(GetPreviousNormalStageId(stage.Id));
            }

            double multiplier;
            if (stage.Number <= 4)
            {
                multiplier = 0.20d;
            }
            else if (stage.Number <= 8)
            {
                multiplier = 0.25d;
            }
            else if (stage.Number <= 12)
            {
                multiplier = 0.30d;
            }
            else if (stage.Number <= 16)
            {
                multiplier = 0.35d;
            }
            else
            {
                multiplier = 0.40d;
            }

            return ClampNumber(GetEnemyGold(stage) * multiplier);
        }
    }
}
