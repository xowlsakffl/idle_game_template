using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.UI.Navigation;
using IdleGame.UI.Support;

namespace IdleGame.UI.Hud
{
    public sealed class HudNotificationDotPresenterArgs
    {
        public Dictionary<HudTab, List<GameObject>> TabNotificationDots;
        public GameObject GuideQuestDot;
        public CurrencyWallet Wallet;
        public StageProgressManager ProgressManager;
        public BattleManager BattleManager;
        public bool HasGrowthAttention;
        public bool HasHeroAttention;
    }

    public static class HudNotificationDotPresenter
    {
        public static void Refresh(HudNotificationDotPresenterArgs args)
        {
            if (args == null)
            {
                return;
            }

            bool hasSummonAttention = args.Wallet != null
                && (args.Wallet.HeroSummonTicket > 0
                    || args.Wallet.EquipmentSummonTicket > 0
                    || args.Wallet.Ruby >= 100);
            bool hasStageAttention = args.ProgressManager != null
                && args.ProgressManager.Mode == ProgressMode.BossBlocked;
            bool hasSupportAttention = args.BattleManager != null
                && SupportPanelStateBuilder.HasReadySkill(args.BattleManager.Skills);
            bool canLevelFortress = args.BattleManager != null && args.BattleManager.CanLevelUpFortress;

            if (args.GuideQuestDot != null)
            {
                args.GuideQuestDot.SetActive(hasStageAttention);
            }

            SetTabNotificationDots(args.TabNotificationDots, HudTab.Growth, args.HasGrowthAttention);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Hero, args.HasHeroAttention);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Fortress, canLevelFortress);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Facility, false);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Summon, hasSummonAttention);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Stage, false);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Shop, false);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Support, hasSupportAttention);
            SetTabNotificationDots(args.TabNotificationDots, HudTab.Debug, false);
        }

        private static void SetTabNotificationDots(
            Dictionary<HudTab, List<GameObject>> notificationDots,
            HudTab tab,
            bool visible)
        {
            if (notificationDots == null || !notificationDots.TryGetValue(tab, out List<GameObject> dots))
            {
                return;
            }

            foreach (GameObject dot in dots)
            {
                if (dot != null)
                {
                    dot.SetActive(visible);
                }
            }
        }
    }
}
