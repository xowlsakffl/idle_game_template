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
        private void RefreshEnemies(
            BattleHudRenderArgs args,
            bool flashActive,
            float flashRatio,
            float time,
            float visualDeltaTime,
            float fieldWidth,
            float fieldHeight)
        {
            int visible = Mathf.Clamp(args.BattleManager.VisibleEnemyCount, 0, args.EnemyBattleImages.Count);
            string currentBattleStageId = args.ProgressManager != null ? args.ProgressManager.CurrentStageId : string.Empty;
            bool stageChanged = observedBattleStageId != currentBattleStageId;
            if (stageChanged || observedBattleKillCount > args.BattleManager.KillsThisStage)
            {
                args.VisualState.ResetDisplayedEnemies();
            }

            observedBattleStageId = currentBattleStageId;
            observedBattleKillCount = args.BattleManager.KillsThisStage;
            for (int i = 0; i < args.EnemyBattleImages.Count; i++)
            {
                bool active = i < visible;
                int shiftedIndex = i;
                Image image = args.EnemyBattleImages[shiftedIndex];
                Text text = args.EnemyBattleTexts[shiftedIndex];
                RectTransform enemyRect = args.EnemyBattleRects[shiftedIndex];

                if (!active)
                {
                    enemyRect.anchoredPosition = Vector2.zero;
                    enemyRect.localScale = Vector3.zero;
                    image.color = new Color(0.13f, 0.10f, 0.10f, 0f);
                    text.color = Color.white;
                    text.text = string.Empty;
                    SetEnemyHpBar(args, shiftedIndex, false, 0f, false);
                    args.VisualState.ClearActiveEnemyPosition(shiftedIndex);
                    args.VisualState.SetEnemyInactive(shiftedIndex);
                    continue;
                }

                int enemySpawnSequence = args.BattleManager.GetVisibleEnemySpawnSequence(i);
                args.VisualState.ResetEnemyIfSpawnChanged(shiftedIndex, enemySpawnSequence);
                if (args.VisualState.TryRenderEnemyDeath(shiftedIndex, enemyRect, image, text, visualDeltaTime, out Vector2 deathPosition))
                {
                    enemyRect.anchoredPosition = deathPosition;
                    SetEnemyHpBar(args, shiftedIndex, false, 0f, false);
                    args.VisualState.ClearActiveEnemyPosition(shiftedIndex);
                    continue;
                }

                bool frontTarget = args.BattleManager.IsBossFight
                    ? i == 0 && flashActive
                    : i == args.BattleManager.RecentHitEnemyIndex && flashActive;
                if (args.BattleManager.IsBossFight)
                {
                    Vector2 bossAnchor = args.VisualState.GetNearestHeroPosition(Vector2.zero) + new Vector2(0f, 112f);
                    Vector2 bossPosition = new Vector2(
                        bossAnchor.x * 0.38f + Mathf.Sin(time * 1.4f) * 18f,
                        bossAnchor.y * 0.38f + Mathf.Cos(time * 1.1f) * 10f + fieldHeight * 0.12f);
                    Vector2 desiredEnemyPosition = BattleHudMotion.ClampBattlefieldPosition(bossPosition, fieldWidth, fieldHeight, 86f);
                    Vector2 finalEnemyPosition = args.VisualState.SmoothEnemyPosition(shiftedIndex, desiredEnemyPosition, desiredEnemyPosition, visualDeltaTime, true);
                    enemyRect.anchoredPosition = finalEnemyPosition;
                    args.VisualState.AddActiveEnemyPosition(finalEnemyPosition);
                    args.VisualState.SetActiveEnemyPosition(shiftedIndex, finalEnemyPosition);
                    enemyRect.localScale = Vector3.one * (frontTarget ? 1.65f + 0.18f * flashRatio : 1.52f);
                }
                else
                {
                    int movementSeed = enemySpawnSequence >= 0 ? enemySpawnSequence : i;
                    Vector2 direction = BattleHudMotion.GetEnemySpreadDirection(movementSeed);
                    float spawnDistance = Mathf.Max(fieldWidth * 0.58f, fieldHeight * 0.58f);
                    Vector2 spawnPosition = direction * spawnDistance;
                    Vector2 drift = new Vector2(-direction.y, direction.x) * Mathf.Sin(time * 2.3f + movementSeed) * 11f;
                    Vector2 provisionalPosition = args.VisualState.GetDisplayedEnemyPositionOrSpawn(shiftedIndex, spawnPosition);
                    Vector2 aggroPosition = args.VisualState.GetNearestHeroPosition(provisionalPosition);
                    Vector2 targetPosition = aggroPosition + BattleHudMotion.GetEnemyAggroOffset(movementSeed, direction, time);
                    Vector2 desiredEnemyPosition = BattleHudMotion.ClampBattlefieldPosition(targetPosition + drift, fieldWidth, fieldHeight, 42f);
                    Vector2 finalEnemyPosition = args.VisualState.SmoothEnemyPosition(shiftedIndex, desiredEnemyPosition, spawnPosition, visualDeltaTime, false);
                    float approach = BattleHudMotion.GetEnemyApproachRatio(spawnPosition, targetPosition, finalEnemyPosition);
                    enemyRect.anchoredPosition = finalEnemyPosition;
                    args.VisualState.AddActiveEnemyPosition(finalEnemyPosition);
                    args.VisualState.SetActiveEnemyPosition(shiftedIndex, finalEnemyPosition);
                    enemyRect.localScale = Vector3.one * (0.68f + 0.30f * approach + (frontTarget ? 0.18f * flashRatio : 0f));
                }

                if (args.BattleManager.IsBossFight)
                {
                    image.color = frontTarget
                        ? new Color(1f, 0.34f, 0.22f, 1f)
                        : new Color(0.62f, 0.12f, 0.10f, 1f);
                    text.color = Color.white;
                    text.text = string.Empty;
                    SetEnemyHpBar(args, shiftedIndex, true, args.BattleManager.GetVisibleEnemyHpRatio(i), true);
                }
                else
                {
                    image.color = frontTarget
                        ? new Color(1f, 0.48f, 0.24f, 1f)
                        : new Color(0.52f, 0.16f + 0.03f * (i % 3), 0.12f, 1f);
                    text.color = Color.white;
                    text.text = string.Empty;
                    SetEnemyHpBar(args, shiftedIndex, true, args.BattleManager.GetVisibleEnemyHpRatio(i), false);
                }
            }
        }
    }
}
