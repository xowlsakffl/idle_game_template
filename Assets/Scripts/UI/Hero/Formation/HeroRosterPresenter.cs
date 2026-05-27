using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.UI.Hero.Formation
{
    public sealed class HeroRosterPresenterArgs
    {
        public BattleManager BattleManager;
        public CurrencyWallet Wallet;
        public bool RefreshPanel;
        public string SelectedHeroForPlacement;
        public Dictionary<string, Button> HeroRosterButtons;
        public Dictionary<string, Text> HeroButtonTexts;
        public Dictionary<string, Button> HeroRosterActionButtons;
        public Dictionary<string, GameObject> HeroRosterDeployedOverlays;
        public Dictionary<string, GameObject> HeroNotificationDots;
        public Func<string, bool> IsHeroInFormation;
        public Func<double, string> FormatShortNumber;
        public Func<HeroDefinition, string> GetShortHeroLabel;
    }

    public static class HeroRosterPresenter
    {
        public static bool Refresh(HeroRosterPresenterArgs args)
        {
            if (args == null || args.BattleManager == null || args.Wallet == null)
            {
                return false;
            }

            bool hasHeroAttention = false;
            foreach (HeroState hero in args.BattleManager.Heroes)
            {
                bool canLevel = hero.LevelUpCost <= args.Wallet.HeroExpItem;
                bool needsAttention = hero.IsOwned && (hero.CanStarUp || canLevel);
                hasHeroAttention |= needsAttention;

                if (!args.RefreshPanel)
                {
                    continue;
                }

                bool isDeployed = args.IsHeroInFormation != null && args.IsHeroInFormation(hero.Definition.Id);
                bool isSelectedForPlacement = args.SelectedHeroForPlacement == hero.Definition.Id;
                HeroFormationView.ApplyRosterCardState(
                    hero.Definition.Id,
                    HeroFormationStateBuilder.BuildRosterCardState(
                        hero,
                        isDeployed,
                        isSelectedForPlacement,
                        needsAttention,
                        args.FormatShortNumber,
                        args.GetShortHeroLabel),
                    args.HeroRosterButtons,
                    args.HeroButtonTexts,
                    args.HeroRosterActionButtons,
                    args.HeroRosterDeployedOverlays,
                    args.HeroNotificationDots);
            }

            return hasHeroAttention;
        }
    }
}
