using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.UI.Growth;
using IdleGame.UI.Hero.Trait;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void RefreshHeroTraitPanel()
        {
            HeroTraitPanelPresenterResult result = HeroTraitPanelPresenter.Refresh(new HeroTraitPanelPresenterArgs
            {
                AccountProgressManager = accountProgressManager,
                SelectedTalentId = selectedHeroTraitId,
                SummaryText = heroHud.TraitSummaryText,
                DetailText = heroHud.TraitDetailText,
                LevelUpButton = heroHud.TraitLevelUpButton,
                ButtonTexts = heroHud.TraitButtonTexts,
                Buttons = heroHud.TraitButtons,
                FormatShortNumber = FormatShortNumber
            });
            selectedHeroTraitId = result.SelectedTalentId;
        }

        private void LevelUpSelectedHeroTrait()
        {
            GrowthActionResult result = GrowthActionService.TryLevelUpTalent(accountProgressManager, selectedHeroTraitId);
            ApplyGrowthActionResult(result);
            if (result != null && result.Success)
            {
                UpdateView();
            }
        }

        private bool CanLevelUpSelectedHeroTrait()
        {
            return GrowthActionService.CanLevelUpTalent(accountProgressManager, selectedHeroTraitId);
        }

        private void LevelUpFortressFromHud()
        {
            GrowthActionResult result = GrowthActionService.TryLevelUpFortress(battleManager);
            ApplyGrowthActionResult(result);
            if (result != null && result.Success)
            {
                UpdateView();
            }
        }

        private bool CanLevelUpFortressFromHud()
        {
            return GrowthActionService.CanLevelUpFortress(battleManager);
        }

        private void TryLevelUpAbilityFromHud(AbilityKind kind)
        {
            ApplyGrowthActionResult(GrowthActionService.TryLevelUpAbility(
                abilityManager,
                wallet,
                kind,
                selectedGrowthLevelStep));
        }

        private bool CanLevelUpAbilityFromHud(AbilityKind kind)
        {
            return GrowthActionService.CanLevelUpAbility(
                abilityManager,
                wallet,
                kind,
                selectedGrowthLevelStep);
        }

        private void ApplyGrowthActionResult(GrowthActionResult result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }
        }
    }
}
