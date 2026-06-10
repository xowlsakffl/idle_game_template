using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Speed;

namespace IdleGame.UI.Battle
{
    public sealed class BattleControlPresenterArgs
    {
        public BattleManager BattleManager;
        public GameSpeedManager SpeedManager;
        public Button SkillAutoButton;
        public Button FeverAutoButton;
        public Button SpeedCycleButton;
        public Button DungeonRepeatButton;
        public Button DungeonExitButton;
    }

    public static class BattleControlPresenter
    {
        public static void Refresh(BattleControlPresenterArgs args)
        {
            if (args == null)
            {
                return;
            }

            if (args.SkillAutoButton != null && args.BattleManager != null)
            {
                BattlePanelView.RefreshAutoControlButton(
                    args.SkillAutoButton,
                    "스킬",
                    args.BattleManager.SkillAutoEnabled,
                    new Color(0.88f, 0.66f, 0.16f, 1f),
                    new Color(0.28f, 0.29f, 0.32f, 1f));
            }

            if (args.FeverAutoButton != null && args.BattleManager != null)
            {
                BattlePanelView.RefreshAutoControlButton(
                    args.FeverAutoButton,
                    "피버",
                    args.BattleManager.FeverAutoEnabled,
                    new Color(0.88f, 0.62f, 0.18f, 1f),
                    new Color(0.28f, 0.29f, 0.32f, 1f));
            }

            if (args.SpeedCycleButton != null && args.SpeedManager != null)
            {
                BattlePanelView.RefreshSpeedButton(args.SpeedCycleButton, args.SpeedManager.CurrentMultiplier);
            }

            BattlePanelView.RefreshDungeonRunControls(
                args.DungeonRepeatButton,
                args.DungeonExitButton,
                args.BattleManager != null && args.BattleManager.IsDungeonRunActive,
                args.BattleManager != null && args.BattleManager.IsDungeonRepeatActive);
        }
    }
}
