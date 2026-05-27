using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.Formation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void LoadHeroFormationDraftFromPreset(int preset)
        {
            heroFormationState.LoadDraft(battleManager, preset);
        }

        private void EnsureHeroFormationDraft()
        {
            heroFormationState.EnsureDraft(battleManager);
        }

        private bool HasHeroFormationPendingChanges()
        {
            return heroFormationState.HasPendingChanges(battleManager);
        }

        private void RefreshHeroFormationPanel()
        {
            EnsureHeroFormationDraft();
            HeroFormationPanelRefreshResult result = HeroFormationPanelPresenter.Refresh(new HeroFormationPanelPresenterArgs
            {
                ActivePageTab = activeHeroPageTab,
                SelectedPreset = heroFormationState.SelectedPreset,
                DeployedCount = Mathf.Min(GameData.MaxPartyHeroes, GetEditingFormationFilledCount()),
                HasPendingChanges = HasHeroFormationPendingChanges(),
                SelectedHeroForPlacement = heroFormationState.SelectedHeroForPlacement,
                PendingRuneEquipId = totemRuneState.PendingRuneEquipId,
                FormationContent = heroHud.FormationContent,
                TraitContent = heroHud.TraitContent,
                TotemContent = heroHud.TotemContent,
                RuneContent = heroHud.RuneContent,
                PlaceholderText = heroHud.PlaceholderText,
                SummaryText = heroHud.FormationSummaryText,
                OwnedEffectText = heroHud.OwnedEffectText,
                PageTabButtons = heroHud.PageTabButtons,
                PresetButtons = heroHud.PresetButtons,
                EditingFormationHeroIds = heroFormationState.EditingHeroIds,
                FormationSlotTexts = heroHud.FormationSlotTexts,
                FormationSlotButtons = heroHud.FormationSlotButtons,
                FormationSlotRemoveButtons = heroHud.FormationSlotRemoveButtons,
                RuneSlotTexts = heroHud.FormationRuneSlotTexts,
                RuneSlotButtons = heroHud.FormationRuneSlotButtons,
                RuneSlotRemoveButtons = heroHud.FormationRuneSlotRemoveButtons,
                BattleManager = battleManager,
                FindHeroState = FindHeroState,
                GetPageTabLabel = GetHeroPageTabLabel,
                FormatShortNumber = FormatShortNumber,
                GetShortHeroLabel = GetShortHeroLabel
            });

            if (result.TraitOpen)
            {
                RefreshHeroTraitPanel();
            }

            if (result.TotemOpen)
            {
                RefreshHeroTotemPanel();
            }

            if (result.RuneOpen)
            {
                RefreshHeroRunePanel();
            }
        }


        private bool IsHeroInEditingFormation(string heroId)
        {
            EnsureHeroFormationDraft();
            return heroFormationState.ContainsHero(heroId);
        }

        private int GetEditingFormationHeroIndex(string heroId)
        {
            EnsureHeroFormationDraft();
            return heroFormationState.IndexOfHero(heroId);
        }

        private int GetEditingFormationFilledCount()
        {
            EnsureHeroFormationDraft();
            return heroFormationState.CountFilled();
        }

        private void AutoArrangeEditingFormation()
        {
            heroFormationState.AutoArrange(battleManager, GetHeroDetailCombatPower);
            UpdateView();
        }

        private void BulkStarUpHeroesFromHud()
        {
            ApplyHeroActionResult(HeroActionService.TryBulkStarUpHeroes(battleManager));
            UpdateView();
        }

        private void SelectOrRemoveRosterHero(string heroId)
        {
            HeroState hero = FindHeroState(heroId);
            EnsureHeroFormationDraft();
            ApplyHeroActionResult(HeroActionService.TryToggleRosterHero(
                heroFormationState.EditingHeroIds,
                hero,
                heroFormationState.SelectedHeroForPlacement));
            UpdateView();
        }

        private void RemoveHeroFromEditingFormationSlot(int slotIndex)
        {
            EnsureHeroFormationDraft();
            ApplyHeroActionResult(HeroActionService.TryRemoveHeroFromSlot(heroFormationState.EditingHeroIds, slotIndex));
            UpdateView();
        }

        private void TryPlaceSelectedHeroInSlot(int slotIndex)
        {
            EnsureHeroFormationDraft();
            ApplyHeroActionResult(HeroActionService.TryPlaceSelectedHeroInSlot(
                heroFormationState.EditingHeroIds,
                slotIndex,
                heroFormationState.SelectedHeroForPlacement,
                FindHeroState));
            UpdateView();
        }


        private void ApplyHeroActionResult(HeroActionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.CloseHeroDetailPanel)
            {
                CloseHeroDetailPanel();
            }

            heroFormationState.ApplyActionResult(result);

            if (!string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }

            if (!string.IsNullOrEmpty(result.OpenHeroDetailId))
            {
                OpenHeroDetailPanel(result.OpenHeroDetailId);
            }
        }

    }
}
