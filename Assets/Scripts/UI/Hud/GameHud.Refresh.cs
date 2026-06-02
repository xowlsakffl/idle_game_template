using IdleGame.Data;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private HudPanelRefreshState BuildPanelRefreshState()
        {
            return HudPanelRefreshState.Build(new HudPanelRefreshStateBuildArgs
            {
                DirtyFlags = dirtyHudFlags,
                ActiveTab = activeTab,
                ContentPanelOpen = contentPanelOpen,
                HeroDetailPanelOpen = heroDetailPanelOpen,
                LastRenderedActiveTab = lastRenderedActiveTab,
                LastRenderedContentPanelOpen = lastRenderedContentPanelOpen,
                LastRenderedHeroDetailPanelOpen = lastRenderedHeroDetailPanelOpen
            });
        }

        private void RefreshFortressPanelIfNeeded(HudPanelRefreshState panelState)
        {
            if (panelState.RefreshFortressPanel)
            {
                RefreshFortressPanel();
            }
        }

        private void RefreshHeroOverlayPanels(HudPanelRefreshState panelState)
        {
            if (heroHud.FormationSavePrompt != null)
            {
                heroHud.FormationSavePrompt.SetActive(heroFormationState.SavePromptOpen);
            }

            if (heroHud.DetailPanel == null)
            {
                return;
            }

            heroHud.DetailPanel.SetActive(heroDetailPanelOpen);
            if (panelState.RefreshHeroDetailPanel)
            {
                RefreshHeroDetailPanel();
            }
        }

        private void MarkHudRendered()
        {
            hudRefreshQueued = false;
            dirtyHudFlags = HudDirtyFlags.None;
            lastRenderedActiveTab = activeTab;
            lastRenderedContentPanelOpen = contentPanelOpen;
            lastRenderedHeroDetailPanelOpen = heroDetailPanelOpen;
        }
    }
}
