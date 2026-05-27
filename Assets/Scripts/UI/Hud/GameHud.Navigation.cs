using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Formation;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void RequestHeroPageTabChange(HeroPageTab tab)
        {
            if (activeHeroPageTab == tab)
            {
                return;
            }

            if (ShouldPromptHeroFormationSaveForHeroPage(tab))
            {
                ShowHeroFormationSavePromptForHeroPage(tab);
                return;
            }

            activeHeroPageTab = tab;
            UpdateView();
        }


        private void RequestTabChange(HudTab tab)
        {
            if (heroDetailPanelOpen)
            {
                heroDetailPanelOpen = false;
                selectedHeroDetailId = string.Empty;
                if (tab == HudTab.Hero)
                {
                    UpdateView();
                    return;
                }
            }

            HudTab targetTab;
            bool targetContentOpen;
            if (activeTab == tab && contentPanelOpen)
            {
                targetTab = activeTab;
                targetContentOpen = false;
            }
            else
            {
                targetTab = tab;
                targetContentOpen = true;
            }

            if (ShouldPromptHeroFormationSave(targetTab, targetContentOpen))
            {
                ShowHeroFormationSavePromptForTab(targetTab, targetContentOpen);
                return;
            }

            ApplyTabState(targetTab, targetContentOpen);
        }

        private bool ShouldPromptHeroFormationSave(HudTab targetTab, bool targetContentOpen)
        {
            return !heroFormationState.SavePromptOpen
                && contentPanelOpen
                && activeTab == HudTab.Hero
                && HasHeroFormationPendingChanges()
                && (targetTab != HudTab.Hero || !targetContentOpen);
        }

        private bool ShouldPromptHeroFormationSaveForHeroPage(HeroPageTab targetHeroPageTab)
        {
            return !heroFormationState.SavePromptOpen
                && contentPanelOpen
                && activeTab == HudTab.Hero
                && activeHeroPageTab == HeroPageTab.Formation
                && targetHeroPageTab != HeroPageTab.Formation
                && HasHeroFormationPendingChanges();
        }

        private void ShowHeroFormationSavePromptForTab(HudTab targetTab, bool targetContentOpen)
        {
            heroFormationPromptTargetState.SetTab(targetTab, targetContentOpen);
            heroFormationState.OpenSavePrompt();
            UpdateView();
        }

        private void ShowHeroFormationSavePromptForPreset(int preset)
        {
            heroFormationPromptTargetState.SetPreset(preset);
            heroFormationState.OpenSavePrompt();
            UpdateView();
        }

        private void ShowHeroFormationSavePromptForHeroPage(HeroPageTab targetHeroPageTab)
        {
            heroFormationPromptTargetState.SetHeroPage(targetHeroPageTab);
            heroFormationState.OpenSavePrompt();
            UpdateView();
        }

        private void ApplyTabState(HudTab targetTab, bool targetContentOpen)
        {
            bool openingHeroPanel = targetTab == HudTab.Hero && (!contentPanelOpen || activeTab != HudTab.Hero);
            activeTab = targetTab;
            contentPanelOpen = targetContentOpen;
            if (openingHeroPanel)
            {
                LoadHeroFormationDraftFromPreset(battleManager.ActiveHeroPreset);
            }

            UpdateView();
        }

        private void ConfirmHeroFormationSavePrompt()
        {
            if (!heroFormationState.TryApply(battleManager))
            {
                heroFormationState.CloseSavePrompt();
                UpdateView();
                return;
            }

            heroFormationState.MarkApplied();

            if (heroFormationPromptTargetState.Kind == HeroFormationPromptTargetKind.Preset)
            {
                int preset = heroFormationPromptTargetState.Preset;
                ClearHeroFormationPromptTarget();
                LoadHeroFormationDraftFromPreset(preset);
                UpdateView();
                return;
            }

            if (heroFormationPromptTargetState.Kind == HeroFormationPromptTargetKind.HeroPage)
            {
                HeroPageTab targetHeroPageTab = heroFormationPromptTargetState.HeroPageTab;
                ClearHeroFormationPromptTarget();
                activeHeroPageTab = targetHeroPageTab;
                UpdateView();
                return;
            }

            HudTab targetTab = heroFormationPromptTargetState.Tab;
            bool targetContentOpen = heroFormationPromptTargetState.ContentOpen;
            ClearHeroFormationPromptTarget();
            ApplyTabState(targetTab, targetContentOpen);
        }

        private void CancelHeroFormationSavePrompt()
        {
            heroFormationState.CloseSavePrompt();
            ClearHeroFormationPromptTarget();
            UpdateView();
        }

        private void ClearHeroFormationPromptTarget()
        {
            heroFormationPromptTargetState.Reset(activeTab, contentPanelOpen, activeHeroPageTab);
        }


        private void RequestHeroPresetChange(int preset)
        {
            int nextPreset = Mathf.Clamp(preset, 1, GameData.MaxHeroPresets);
            if (heroFormationState.IsSelectedPreset(nextPreset))
            {
                return;
            }

            if (heroFormationState.IsDirty)
            {
                ShowHeroFormationSavePromptForPreset(nextPreset);
                return;
            }

            LoadHeroFormationDraftFromPreset(nextPreset);
            UpdateView();
        }

    }
}
