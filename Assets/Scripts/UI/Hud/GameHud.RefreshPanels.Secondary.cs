using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Data;
using IdleGame.UI.Battle;
using IdleGame.UI.Common;
using IdleGame.UI.Debugging;
using IdleGame.UI.Growth;
using IdleGame.UI.Header;
using IdleGame.UI.Hero.Formation;
using IdleGame.UI.Navigation;
using IdleGame.UI.Summon;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void RefreshDebugPanel(HudPanelRefreshState panelState)
        {
            if (!panelState.RefreshDebugPanel)
            {
                return;
            }

            DebugPanelPresenter.Refresh(new DebugPanelPresenterArgs
            {
                RefreshPanel = true,
                StatusText = debugText,
                SpeedManager = speedManager,
                Wallet = wallet,
                AccountProgressManager = accountProgressManager,
                ProgressManager = progressManager,
                BattleManager = battleManager,
                FormatShortNumber = value => FormatShortNumber(value),
                FormatCountNumber = FormatCountNumber
            });
        }

        private void RefreshNotifications(HudPanelRefreshState panelState)
        {
            if (!panelState.RefreshNotifications)
            {
                return;
            }

            HudNotificationDotPresenter.Refresh(new HudNotificationDotPresenterArgs
            {
                TabNotificationDots = tabNotificationDots,
                GuideQuestDot = guideQuestDot,
                Wallet = wallet,
                ProgressManager = progressManager,
                BattleManager = battleManager,
                HasGrowthAttention = cachedGrowthAttention,
                HasHeroAttention = cachedHeroAttention
            });
        }

        private void RefreshPanelVisibility(HudPanelRefreshState panelState)
        {
            HudPanelVisibilityPresenter.Refresh(new HudPanelVisibilityPresenterArgs
            {
                PanelState = panelState,
                BattlePanel = battleHud.Panel,
                BattleLayoutElement = battleHud.LayoutElement,
                ContentRoot = contentRoot,
                ContentLayoutElement = contentLayoutElement,
                GrowthPanel = growthPanel,
                HeroPanel = heroHud.Panel,
                FortressPanel = fortressPanel,
                FacilityPanel = facilityPanel,
                StagePanel = stagePanel,
                SummonPanel = summonPanel,
                ShopPanel = shopPanel,
                SupportPanel = supportPanel,
                DebugPanel = debugPanel,
                HeroFacilityContent = heroFacilityContent
            });
        }

        private void RefreshFacilityPanelVisibility(HudPanelRefreshState panelState)
        {
            if (panelState.FacilityPanelOpen)
            {
                if (panelState.RefreshFacilityPanel)
                {
                    RefreshHeroFacilityPanel();
                }

                return;
            }

            facilityAssignmentModalOpen = false;
        }

        private void RefreshBottomNavigation(HudPanelRefreshState panelState)
        {
            if (!panelState.RefreshNavigation)
            {
                return;
            }

            HudBottomNavPresenter.Refresh(new HudBottomNavPresenterArgs
            {
                TabButtons = tabButtons,
                TabButtonLabels = tabButtonLabels,
                ActiveTab = activeTab,
                ContentPanelOpen = contentPanelOpen,
                HeroDetailPanelOpen = heroDetailPanelOpen
            });
        }
    }
}
