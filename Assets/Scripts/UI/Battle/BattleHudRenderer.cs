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

    public sealed partial class BattleHudRenderer
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



    }
}
