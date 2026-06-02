using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static class StageClearRewardService
    {
        internal readonly struct StageKillResult
        {
            public StageKillResult(int kills, bool isComplete, string battleLog)
            {
                Kills = kills;
                IsComplete = isComplete;
                BattleLog = battleLog;
            }

            public int Kills { get; }
            public bool IsComplete { get; }
            public string BattleLog { get; }
        }

        public static StageClearReward GetFirstClearReward(
            StageDefinition stage,
            string highestStageId,
            bool chapterOneBossCleared)
        {
            return ShouldGrantFirstClearReward(stage, highestStageId, chapterOneBossCleared)
                ? GameData.GetStageFirstClearReward(stage)
                : StageClearReward.Empty;
        }

        public static void ApplyFirstClearReward(CurrencyWallet wallet, StageClearReward reward)
        {
            if (wallet == null || reward.IsEmpty)
            {
                return;
            }

            wallet.AddHeroSummonTicket(reward.HeroSummonTickets);
            wallet.AddEquipmentSummonTicket(reward.EquipmentSummonTickets);
            wallet.AddRuby(reward.Ruby);
            wallet.AddHeroExpItem(reward.HeroExpItems);
            wallet.AddEquipmentExpItem(reward.EquipmentExpItems);
            wallet.AddHeroTranscendStone(reward.HeroTranscendStones);
        }

        public static string BuildFirstClearRewardLogSuffix(StageClearReward reward)
        {
            if (reward.IsEmpty)
            {
                return string.Empty;
            }

            string rewardText = CombatRewardService.BuildStageFirstClearRewardText(reward);
            return string.IsNullOrEmpty(rewardText)
                ? string.Empty
                : " / 최초 클리어 " + rewardText;
        }

        public static string GrantFirstClearReward(
            StageDefinition stage,
            CurrencyWallet wallet,
            string highestStageId,
            bool chapterOneBossCleared)
        {
            StageClearReward reward = GetFirstClearReward(stage, highestStageId, chapterOneBossCleared);
            ApplyFirstClearReward(wallet, reward);
            return BuildFirstClearRewardLogSuffix(reward);
        }

        public static StageKillResult RegisterKill(StageDefinition stage, int currentKills, int requiredKills)
        {
            int nextKills = currentKills + 1;
            bool isComplete = nextKills >= requiredKills;
            string battleLog = isComplete
                ? BuildStageCompleteLog(stage)
                : BuildKillProgressLog(stage, nextKills, requiredKills);
            return new StageKillResult(nextKills, isComplete, battleLog);
        }

        public static string BuildBossClearLog(StageDefinition stage)
        {
            return stage != null ? "보스 처치 성공: " + stage.Id + " 클리어" : "보스 처치 성공";
        }

        public static string BuildStageCompleteLog(StageDefinition stage)
        {
            return stage != null ? stage.Id + " 완료" : "스테이지 완료";
        }

        public static string BuildKillProgressLog(StageDefinition stage, int kills, int requiredKills)
        {
            return stage != null
                ? stage.Id + " 처치 " + kills + "/" + requiredKills
                : "처치 " + kills + "/" + requiredKills;
        }

        private static bool ShouldGrantFirstClearReward(
            StageDefinition stage,
            string highestStageId,
            bool chapterOneBossCleared)
        {
            if (stage == null)
            {
                return false;
            }

            if (stage.Type == StageType.Boss)
            {
                return stage.Id == highestStageId
                    && (stage.Id != GameData.ChapterOneBossStageId || !chapterOneBossCleared);
            }

            return stage.Id == highestStageId;
        }
    }
}
