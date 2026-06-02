using System.Collections.Generic;
using System;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Hero.Formation;

namespace IdleGame.UI.Hero
{
    public static partial class HeroActionService
    {
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
