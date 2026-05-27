using UnityEngine.UI;
using IdleGame.Battle;

namespace IdleGame.UI.Battle
{
    public sealed class BattleLogPresenterArgs
    {
        public BattleManager BattleManager;
        public Text SupportText;
        public Text LogText;
        public Text RewardText;
    }

    public static class BattleLogPresenter
    {
        public static void Refresh(BattleLogPresenterArgs args)
        {
            if (args == null || args.BattleManager == null)
            {
                return;
            }

            BattleManager battleManager = args.BattleManager;
            if (args.SupportText != null)
            {
                args.SupportText.text = battleManager.SupportStatusText;
            }

            if (args.LogText != null)
            {
                args.LogText.text = BuildBattleLog(battleManager);
            }

            if (args.RewardText != null)
            {
                args.RewardText.text = battleManager.LastRewardLog;
            }
        }

        private static string BuildBattleLog(BattleManager battleManager)
        {
            if (string.IsNullOrEmpty(battleManager.LastDamageLog))
            {
                return battleManager.LastBattleLog;
            }

            return battleManager.LastBattleLog + "\n" + battleManager.LastDamageLog;
        }
    }
}
