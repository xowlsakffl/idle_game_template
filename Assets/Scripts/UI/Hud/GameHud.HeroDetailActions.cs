using System.Collections.Generic;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private double GetHeroDetailCombatPower(HeroState hero)
        {
            var heroes = new List<HeroState> { hero };
            return abilityManager.GetTotalCombatPower(heroes);
        }

        private void ToggleSelectedHeroDetailFormation()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            EnsureHeroFormationDraft();
            ApplyHeroActionResult(HeroActionService.TryToggleSelectedHeroDetailFormation(heroFormationState.EditingHeroIds, hero));
            UpdateView();
        }

        private void RemoveSelectedHeroDetailFromFormation()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            EnsureHeroFormationDraft();
            ApplyHeroActionResult(HeroActionService.TryRemoveSelectedHeroDetailFromFormation(heroFormationState.EditingHeroIds, hero));
            UpdateView();
        }

        private void LevelUpSelectedHeroDetail()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyHeroActionResult(HeroActionService.TryLevelUpHero(battleManager, wallet, hero));
            UpdateView();
        }

        private bool CanLevelUpSelectedHeroDetail()
        {
            return HeroActionService.CanLevelUpHero(wallet, FindHeroState(selectedHeroDetailId));
        }

        private void StarUpSelectedHeroDetail()
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyHeroActionResult(HeroActionService.TryStarUpHero(battleManager, hero));
            UpdateView();
        }
    }
}
