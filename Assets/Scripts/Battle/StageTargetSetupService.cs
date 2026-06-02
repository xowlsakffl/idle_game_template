using IdleGame.Data;

namespace IdleGame.Battle
{
    internal static class StageTargetSetupService
    {
        internal readonly struct StageTargetSetup
        {
            public StageTargetSetup(
                bool isBossFight,
                int requiredKills,
                string targetName,
                GameNumber targetMaxHp,
                GameNumber targetHp,
                float bossTimeRemaining,
                int visibleEnemyCount,
                string battleLog)
            {
                IsBossFight = isBossFight;
                RequiredKills = requiredKills;
                TargetName = targetName;
                TargetMaxHp = targetMaxHp;
                TargetHp = targetHp;
                BossTimeRemaining = bossTimeRemaining;
                VisibleEnemyCount = visibleEnemyCount;
                BattleLog = battleLog;
            }

            public bool IsBossFight { get; }
            public int RequiredKills { get; }
            public string TargetName { get; }
            public GameNumber TargetMaxHp { get; }
            public GameNumber TargetHp { get; }
            public float BossTimeRemaining { get; }
            public int VisibleEnemyCount { get; }
            public string BattleLog { get; }
        }

        public static StageTargetSetup Build(StageDefinition stage)
        {
            bool isBossFight = stage.Type == StageType.Boss;
            int requiredKills = stage.RequiredKills;

            if (isBossFight)
            {
                BossDefinition boss = GameData.GetBoss(stage.TargetId);
                GameNumber targetHp = GameData.GetBossHp(stage);
                return new StageTargetSetup(
                    true,
                    requiredKills,
                    boss.DisplayName,
                    targetHp,
                    targetHp,
                    boss.TimeLimitSeconds,
                    1,
                    stage.Id + " 보스전 시작");
            }

            EnemyDefinition enemy = GameData.GetEnemy(stage.TargetId);
            return new StageTargetSetup(
                false,
                requiredKills,
                enemy.DisplayName + " 무리",
                GameData.GetEnemyHp(stage),
                GameNumber.Zero,
                0f,
                0,
                stage.Id + " 전투 중");
        }
    }
}
