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
    public struct BattleHudRenderArgs
    {
        public StageProgressManager ProgressManager;
        public BattleManager BattleManager;
        public BattlefieldWorldView BattlefieldWorldView;
        public BattleHudVisualState VisualState;
        public RectTransform BattlefieldRect;
        public RawImage BattlefieldWorldImage;
        public Text DamagePopupText;
        public Text CenterSpawnText;
        public IReadOnlyDictionary<string, Image> HeroBattleImages;
        public IReadOnlyDictionary<string, Text> HeroBattleTexts;
        public IReadOnlyDictionary<string, RectTransform> HeroBattleRects;
        public IReadOnlyList<Image> EnemyBattleImages;
        public IReadOnlyList<Text> EnemyBattleTexts;
        public IReadOnlyList<RectTransform> EnemyBattleRects;
        public IReadOnlyList<GameObject> EnemyHpBarObjects;
        public IReadOnlyList<Image> EnemyHpFillImages;
        public float HitFlashRemaining;
        public float HeroAttackFlashRemaining;
        public float Time;
        public float DeltaTime;
    }

    public sealed class BattleHudRenderer
    {
        private static readonly Vector2[] HeroFormation =
        {
            new Vector2(0f, -16f),
            new Vector2(-86f, -26f),
            new Vector2(86f, -26f),
            new Vector2(-48f, 56f),
            new Vector2(48f, 56f),
            new Vector2(-134f, 22f),
            new Vector2(134f, 22f),
            new Vector2(0f, 104f)
        };

        private int observedBattleKillCount = -1;
        private string observedBattleStageId = string.Empty;

        public void ResetRuntimeState()
        {
            observedBattleKillCount = -1;
            observedBattleStageId = string.Empty;
        }

        public void Refresh(BattleHudRenderArgs args)
        {
            if (args.BattleManager == null
                || args.DamagePopupText == null
                || args.BattlefieldRect == null
                || args.VisualState == null)
            {
                return;
            }

            bool flashActive = args.HitFlashRemaining > 0f;
            float flashRatio = args.HitFlashRemaining / 0.28f;
            bool heroBatchFlashActive = args.HeroAttackFlashRemaining > 0f;
            float heroBatchFlashRatio = args.HeroAttackFlashRemaining / 0.28f;
            float time = args.Time;
            float visualDeltaTime = BattleHudMotion.GetClampedDeltaTime(args.DeltaTime);
            float fieldWidth = Mathf.Max(760f, args.BattlefieldRect.rect.width);
            float fieldHeight = Mathf.Max(260f, args.BattlefieldRect.rect.height);

            if (IsWorldBattlefieldEnabled(args))
            {
                RefreshWorldBattlefieldImage(args);
                SetLegacyBattlefieldActorsVisible(args, false);
                if (args.CenterSpawnText != null)
                {
                    args.CenterSpawnText.gameObject.SetActive(false);
                }

                args.DamagePopupText.text = string.Empty;
                return;
            }

            SetLegacyBattlefieldActorsVisible(args, true);
            if (args.CenterSpawnText != null)
            {
                args.CenterSpawnText.gameObject.SetActive(true);
            }

            foreach (RectTransform heroRect in args.HeroBattleRects.Values)
            {
                if (heroRect != null)
                {
                    heroRect.localScale = Vector3.zero;
                }
            }

            args.VisualState.PrepareFrame();
            int heroIndex = 0;
            foreach (HeroState hero in args.BattleManager.DeployedHeroes)
            {
                if (!args.HeroBattleImages.TryGetValue(hero.Definition.Id, out Image image))
                {
                    continue;
                }

                bool isLastSource = args.BattleManager.LastHitSourceName == hero.Definition.DisplayName && flashActive;
                bool isBatchSource = heroBatchFlashActive && IsHeroInRecentAttackBatch(args.BattleManager, hero.Definition.Id);
                bool isAttackSource = isLastSource || isBatchSource;
                float attackFlashRatio = Mathf.Max(isLastSource ? flashRatio : 0f, isBatchSource ? heroBatchFlashRatio : 0f);
                if (args.HeroBattleRects.TryGetValue(hero.Definition.Id, out RectTransform heroRect))
                {
                    Vector2 formationPosition = HeroFormation[heroIndex % HeroFormation.Length];
                    Vector2 roamOffset = BattleHudMotion.GetHeroRoamOffset(hero, heroIndex, time, fieldWidth, fieldHeight);
                    Vector2 traitMotion = BattleHudMotion.GetHeroTraitMotionOffset(hero, heroIndex, time, false, 0f);
                    Vector2 battlePosition = BattleHudMotion.ClampBattlefieldPosition(formationPosition + roamOffset + traitMotion, fieldWidth, fieldHeight, 58f);
                    heroRect.localScale = Vector3.one * BattleHudMotion.GetHeroTraitScale(hero, isAttackSource, attackFlashRatio, time, heroIndex);
                    Vector2 currentHeroPosition = args.VisualState.GetDisplayedHeroPositionOrDefault(hero.Definition.Id, battlePosition);
                    args.VisualState.AddActiveHeroPosition(currentHeroPosition);
                    args.VisualState.SetHeroBasePosition(hero.Definition.Id, battlePosition);
                }

                Color baseColor = HeroUiText.GetRarityColor(hero.Definition.Rarity);
                image.color = isAttackSource
                    ? Color.Lerp(baseColor, new Color(1f, 0.86f, 0.22f, 1f), attackFlashRatio)
                    : baseColor;

                if (args.HeroBattleTexts.TryGetValue(hero.Definition.Id, out Text text))
                {
                    text.text = string.Empty;
                }

                heroIndex += 1;
            }

            RefreshSpawnPortal(args, time);
            RefreshEnemies(args, flashActive, flashRatio, time, visualDeltaTime, fieldWidth, fieldHeight);
            RefreshHeroesAfterEnemies(args, flashActive, flashRatio, heroBatchFlashActive, heroBatchFlashRatio, time, visualDeltaTime, fieldWidth, fieldHeight);
            RefreshDamagePopup(args, flashRatio);
        }

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

        private static void RefreshSpawnPortal(BattleHudRenderArgs args, float time)
        {
            if (args.CenterSpawnText == null)
            {
                return;
            }

            RectTransform portalRect = args.CenterSpawnText.GetComponent<RectTransform>();
            float pulse = 1f + Mathf.Sin(time * 5.6f) * 0.13f;
            portalRect.localScale = new Vector3(pulse, pulse, 1f);
            args.CenterSpawnText.text = "◎";
            args.CenterSpawnText.color = args.BattleManager.IsBossFight
                ? new Color(1f, 0.70f, 0.16f, 0.72f)
                : new Color(0.35f, 0.72f, 1f, 0.42f + Mathf.Sin(time * 3.2f) * 0.12f);
        }

        private static bool IsWorldBattlefieldEnabled(BattleHudRenderArgs args)
        {
            return args.BattlefieldWorldView != null && args.BattlefieldWorldView.OutputTexture != null && args.BattlefieldWorldImage != null;
        }

        private static void RefreshWorldBattlefieldImage(BattleHudRenderArgs args)
        {
            if (args.BattlefieldWorldImage == null || args.BattlefieldWorldView == null)
            {
                return;
            }

            args.BattlefieldWorldImage.texture = args.BattlefieldWorldView.OutputTexture;
            args.BattlefieldWorldImage.gameObject.SetActive(true);
        }

        private static void SetLegacyBattlefieldActorsVisible(BattleHudRenderArgs args, bool visible)
        {
            foreach (RectTransform heroRect in args.HeroBattleRects.Values)
            {
                if (heroRect != null && heroRect.gameObject.activeSelf != visible)
                {
                    heroRect.gameObject.SetActive(visible);
                }
            }

            for (int i = 0; i < args.EnemyBattleRects.Count; i++)
            {
                RectTransform enemyRect = args.EnemyBattleRects[i];
                if (enemyRect != null && enemyRect.gameObject.activeSelf != visible)
                {
                    enemyRect.gameObject.SetActive(visible);
                }
            }
        }

        private static void RefreshDamagePopup(BattleHudRenderArgs args, float flashRatio)
        {
            if (args.BattleManager.HitSequence <= 0)
            {
                args.DamagePopupText.text = "READY";
                args.DamagePopupText.color = new Color(0.72f, 0.78f, 0.86f, 1f);
                return;
            }

            args.DamagePopupText.text = args.BattleManager.LastHitSourceName
                + "\n-" + NumberFormatter.Format(args.BattleManager.LastHitDamage)
                + (args.BattleManager.LastHitWasCritical ? " CRIT" : string.Empty);
            args.DamagePopupText.color = args.BattleManager.LastHitWasCritical
                ? new Color(1f, 0.91f, 0.24f, 1f)
                : new Color(1f, 0.55f, 0.32f, 1f);

            RectTransform damageRect = args.DamagePopupText.GetComponent<RectTransform>();
            damageRect.anchoredPosition = new Vector2(0f, 24f + 40f * flashRatio);
            damageRect.localScale = Vector3.one * (1f + 0.25f * flashRatio);
        }

        private static bool IsHeroInRecentAttackBatch(BattleManager battleManager, string heroId)
        {
            IReadOnlyList<string> attackIds = battleManager.RecentHeroAttackIds;
            for (int i = 0; i < attackIds.Count; i++)
            {
                if (attackIds[i] == heroId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetEnemyHpBar(BattleHudRenderArgs args, int index, bool visible, float ratio, bool isBoss)
        {
            if (index < 0 || index >= args.EnemyHpBarObjects.Count || index >= args.EnemyHpFillImages.Count)
            {
                return;
            }

            GameObject hpBar = args.EnemyHpBarObjects[index];
            if (hpBar == null)
            {
                return;
            }

            hpBar.SetActive(visible);
            if (!visible)
            {
                return;
            }

            Image fill = args.EnemyHpFillImages[index];
            if (fill == null)
            {
                return;
            }

            float clampedRatio = Mathf.Clamp01(ratio);
            fill.rectTransform.anchorMax = new Vector2(clampedRatio, 1f);
            fill.color = isBoss
                ? new Color(0.95f, 0.18f, 0.15f, 1f)
                : Color.Lerp(new Color(0.95f, 0.23f, 0.16f, 1f), new Color(0.35f, 0.93f, 0.28f, 1f), clampedRatio);
        }

    }
}
