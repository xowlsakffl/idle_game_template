using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Battlefield;
using IdleGame.Data;
using IdleGame.Progression;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Battle
{
    public sealed partial class BattleHudRenderer
    {
        private static void RefreshHeroesAfterEnemies(
            BattleHudRenderArgs args,
            bool flashActive,
            float flashRatio,
            bool heroBatchFlashActive,
            float heroBatchFlashRatio,
            float time,
            float visualDeltaTime,
            float fieldWidth,
            float fieldHeight)
        {
            args.VisualState.ClearActiveHeroPositions();
            int heroIndex = 0;
            foreach (HeroState hero in args.BattleManager.DeployedHeroes)
            {
                if (!args.HeroBattleRects.TryGetValue(hero.Definition.Id, out RectTransform heroRect))
                {
                    continue;
                }

                Vector2 basePosition = args.VisualState.TryGetHeroBasePosition(hero.Definition.Id, out Vector2 storedPosition)
                    ? storedPosition
                    : Vector2.zero;
                bool isLastSource = args.BattleManager.LastHitSourceName == hero.Definition.DisplayName && flashActive;
                bool isBatchSource = heroBatchFlashActive && IsHeroInRecentAttackBatch(args.BattleManager, hero.Definition.Id);
                bool isAttackSource = isLastSource || isBatchSource;
                float attackFlashRatio = Mathf.Max(isLastSource ? flashRatio : 0f, isBatchSource ? heroBatchFlashRatio : 0f);
                int targetIndex = args.BattleManager.GetHeroTargetVisualIndex(hero.Definition.Id);
                Vector2 enemyPosition = args.VisualState.GetHeroLockedEnemyPosition(targetIndex, basePosition);
                Vector2 pursuitOffset = BattleHudMotion.GetHeroPursuitOffset(hero, heroIndex, basePosition, enemyPosition, time, isAttackSource, attackFlashRatio);
                Vector2 desiredHeroPosition = BattleHudMotion.ClampBattlefieldPosition(basePosition + pursuitOffset, fieldWidth, fieldHeight, 58f);
                Vector2 finalHeroPosition = args.VisualState.SmoothHeroPosition(hero, desiredHeroPosition, visualDeltaTime, isAttackSource, attackFlashRatio);
                heroRect.anchoredPosition = finalHeroPosition;
                heroRect.localScale = Vector3.one * BattleHudMotion.GetHeroTraitScale(hero, isAttackSource, attackFlashRatio, time, heroIndex);
                args.VisualState.AddActiveHeroPosition(finalHeroPosition);

                if (args.HeroBattleImages.TryGetValue(hero.Definition.Id, out Image image))
                {
                    Color baseColor = HeroUiText.GetRarityColor(hero.Definition.Rarity);
                    image.color = isAttackSource
                        ? Color.Lerp(baseColor, new Color(1f, 0.86f, 0.22f, 1f), attackFlashRatio)
                        : baseColor;
                }

                heroIndex += 1;
            }
        }
    }
}
