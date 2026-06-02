using System.Collections.Generic;
using System;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Hero.Formation;

namespace IdleGame.UI.Hero
{
    public sealed class HeroActionResult
    {
        public bool Success;
        public bool FormationDirty;
        public bool CloseHeroDetailPanel;
        public bool ClearSelectedHeroForPlacement;
        public bool HasSelectedHeroForPlacement;
        public string SelectedHeroForPlacement;
        public string OpenHeroDetailId;
        public string Message;
    }

    public static partial class HeroActionService
    {
        public static HeroActionResult TryLevelUpHero(BattleManager battleManager, CurrencyWallet wallet, HeroState hero)
        {
            if (hero == null || battleManager == null)
            {
                return new HeroActionResult();
            }

            if (!hero.IsOwned)
            {
                return new HeroActionResult
                {
                    Message = "아직 획득하지 않은 영웅입니다."
                };
            }

            if (hero.Level >= hero.MaxLevel)
            {
                return new HeroActionResult
                {
                    Message = "이미 최대 레벨입니다."
                };
            }

            if (wallet == null || wallet.HeroExpItem < hero.LevelUpCost)
            {
                return new HeroActionResult
                {
                    Message = "경험치책이 부족합니다."
                };
            }

            bool leveled = battleManager.TryLevelUpHero(hero.Definition.Id);
            return new HeroActionResult
            {
                Success = leveled,
                Message = leveled ? string.Empty : "레벨업에 실패했습니다."
            };
        }

        public static bool CanLevelUpHero(CurrencyWallet wallet, HeroState hero)
        {
            return hero != null
                && hero.IsOwned
                && hero.Level < hero.MaxLevel
                && wallet != null
                && wallet.HeroExpItem >= hero.LevelUpCost;
        }

        public static HeroActionResult TryStarUpHero(BattleManager battleManager, HeroState hero)
        {
            if (hero == null || battleManager == null)
            {
                return new HeroActionResult();
            }

            if (!hero.IsOwned)
            {
                return new HeroActionResult
                {
                    Message = "아직 획득하지 않은 영웅입니다."
                };
            }

            if (hero.IsMaxStars)
            {
                return new HeroActionResult
                {
                    Message = "이미 최대 성급입니다."
                };
            }

            if (!hero.CanStarUp)
            {
                return new HeroActionResult
                {
                    Message = "영웅 조각이 부족합니다."
                };
            }

            bool starred = battleManager.TryStarUpHero(hero.Definition.Id);
            return new HeroActionResult
            {
                Success = starred,
                Message = starred ? string.Empty : "승급에 실패했습니다."
            };
        }

        public static HeroActionResult TryBulkStarUpHeroes(BattleManager battleManager)
        {
            if (battleManager == null)
            {
                return new HeroActionResult();
            }

            int starUps = battleManager.BulkStarUpHeroes();
            return new HeroActionResult
            {
                Success = starUps > 0
            };
        }

    }
}
