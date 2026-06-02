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
