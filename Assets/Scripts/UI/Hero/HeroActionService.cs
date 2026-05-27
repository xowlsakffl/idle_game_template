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

    public static class HeroActionService
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

        public static HeroActionResult TryToggleRosterHero(
            IList<string> editingHeroIds,
            HeroState hero,
            string selectedHeroForPlacement)
        {
            if (hero == null)
            {
                return new HeroActionResult();
            }

            if (!hero.IsOwned)
            {
                return new HeroActionResult
                {
                    Message = "뽑기로 조각을 획득해야 배치할 수 있습니다."
                };
            }

            int existingIndex = IndexOf(editingHeroIds, hero.Definition.Id);
            if (existingIndex >= 0)
            {
                HeroActionResult removeResult = TryRemoveHeroFromSlot(editingHeroIds, existingIndex);
                removeResult.ClearSelectedHeroForPlacement = true;
                return removeResult;
            }

            string nextHeroId = selectedHeroForPlacement == hero.Definition.Id ? string.Empty : hero.Definition.Id;
            return new HeroActionResult
            {
                Success = true,
                HasSelectedHeroForPlacement = true,
                SelectedHeroForPlacement = nextHeroId
            };
        }

        public static HeroActionResult TryRemoveHeroFromSlot(IList<string> editingHeroIds, int slotIndex)
        {
            if (!HeroFormationDraftRules.TryRemoveAt(editingHeroIds, slotIndex))
            {
                return new HeroActionResult();
            }

            return new HeroActionResult
            {
                Success = true,
                FormationDirty = true,
                ClearSelectedHeroForPlacement = true
            };
        }

        public static HeroActionResult TryToggleSelectedHeroDetailFormation(IList<string> editingHeroIds, HeroState hero)
        {
            if (hero == null)
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

            if (IndexOf(editingHeroIds, hero.Definition.Id) >= 0)
            {
                return TryRemoveSelectedHeroDetailFromFormation(editingHeroIds, hero);
            }

            return new HeroActionResult
            {
                Success = true,
                CloseHeroDetailPanel = true,
                HasSelectedHeroForPlacement = true,
                SelectedHeroForPlacement = hero.Definition.Id,
                Message = "배치할 칸을 선택하세요."
            };
        }

        public static HeroActionResult TryRemoveSelectedHeroDetailFromFormation(IList<string> editingHeroIds, HeroState hero)
        {
            if (hero == null)
            {
                return new HeroActionResult();
            }

            int slotIndex = IndexOf(editingHeroIds, hero.Definition.Id);
            if (slotIndex < 0)
            {
                return new HeroActionResult
                {
                    Message = "출전 중인 영웅이 아닙니다."
                };
            }

            if (!HeroFormationDraftRules.TryRemoveAt(editingHeroIds, slotIndex))
            {
                return new HeroActionResult
                {
                    Message = "최소 1명은 편성해야 합니다."
                };
            }

            return new HeroActionResult
            {
                Success = true,
                FormationDirty = true,
                CloseHeroDetailPanel = true,
                ClearSelectedHeroForPlacement = true
            };
        }

        public static HeroActionResult TryPlaceSelectedHeroInSlot(
            IList<string> editingHeroIds,
            int slotIndex,
            string selectedHeroForPlacement,
            Func<string, HeroState> findHero)
        {
            if (editingHeroIds == null || slotIndex < 0 || slotIndex >= editingHeroIds.Count)
            {
                return new HeroActionResult();
            }

            if (string.IsNullOrEmpty(selectedHeroForPlacement))
            {
                string currentHeroId = editingHeroIds[slotIndex];
                return string.IsNullOrEmpty(currentHeroId)
                    ? new HeroActionResult()
                    : new HeroActionResult
                    {
                        OpenHeroDetailId = currentHeroId
                    };
            }

            HeroState hero = findHero != null ? findHero(selectedHeroForPlacement) : null;
            if (hero == null || !hero.IsOwned)
            {
                return new HeroActionResult
                {
                    ClearSelectedHeroForPlacement = true,
                    Message = "배치할 수 없는 영웅입니다."
                };
            }

            if (!HeroFormationDraftRules.TryPlaceHero(editingHeroIds, slotIndex, selectedHeroForPlacement))
            {
                return new HeroActionResult();
            }

            return new HeroActionResult
            {
                Success = true,
                FormationDirty = true,
                ClearSelectedHeroForPlacement = true
            };
        }

        private static int IndexOf(IList<string> heroIds, string heroId)
        {
            if (heroIds == null || string.IsNullOrEmpty(heroId))
            {
                return -1;
            }

            for (int i = 0; i < heroIds.Count; i++)
            {
                if (heroIds[i] == heroId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
