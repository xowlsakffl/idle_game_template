using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Speed;

namespace IdleGame.Battlefield
{
    public sealed partial class BattlefieldWorldView
    {
        private void PlayHitBursts()
        {
            if (battleManager.HitSequence != observedHitSequence)
            {
                observedHitSequence = battleManager.HitSequence;
                if (battleManager.HitSequence > 0)
                {
                    Vector2 hitPosition = battleManager.LastHitPosition;
                    int hitIndex = battleManager.IsBossFight ? 0 : battleManager.RecentHitEnemyIndex;
                    if (hitIndex >= 0 && hitIndex < enemyActors.Count)
                    {
                        enemyActors[hitIndex].HitPulse = ActorHitPulseDuration;
                    }

                    SpawnDamageFloater(
                        hitPosition,
                        "-" + NumberFormatter.Format(battleManager.LastHitDamage),
                        battleManager.LastHitWasCritical ? new Color(1f, 0.92f, 0.16f, 1f) : new Color(1f, 0.42f, 0.18f, 1f),
                        battleManager.LastHitWasCritical ? 0.78f : 0.62f);
                    SpawnImpactEffect(
                        hitPosition,
                        battleManager.LastHitWasCritical ? 1 : 0,
                        battleManager.LastHitWasCritical ? new Color(1f, 0.82f, 0.22f, 0.94f) : new Color(1f, 0.52f, 0.24f, 0.88f),
                        battleManager.LastHitWasCritical ? 0.58f : 0.42f);
                }
            }

            if (battleManager.MonsterHitSequence != observedMonsterHitSequence)
            {
                observedMonsterHitSequence = battleManager.MonsterHitSequence;
                int attackingEnemyIndex = battleManager.RecentAttackingEnemyIndex;
                if (attackingEnemyIndex >= 0 && attackingEnemyIndex < enemyActors.Count)
                {
                    enemyActors[attackingEnemyIndex].AttackPulse = EnemyAttackPulseDuration;
                }

                int damagedHeroIndex = battleManager.RecentDamagedHeroIndex;
                IReadOnlyList<HeroState> deployedHeroes = battleManager.DeployedHeroes;
                if (damagedHeroIndex >= 0 && damagedHeroIndex < deployedHeroes.Count
                    && heroActors.TryGetValue(deployedHeroes[damagedHeroIndex].Definition.Id, out WorldActor damagedHero))
                {
                    damagedHero.HitPulse = ActorHitPulseDuration;
                }

                SpawnImpactEffect(battleManager.LastMonsterHitPosition, 3, new Color(0.82f, 0.76f, 0.64f, 0.72f), 0.34f);
            }

            if (battleManager.EnemyDefeatSequence != observedEnemyDefeatSequence)
            {
                observedEnemyDefeatSequence = battleManager.EnemyDefeatSequence;
                SpawnDamageFloater(battleManager.LastDefeatedEnemyPosition, "KO", new Color(1f, 0.86f, 0.24f, 1f), 0.55f);
                SpawnImpactEffect(battleManager.LastDefeatedEnemyPosition, 4, new Color(1f, 0.44f, 0.20f, 0.82f), 0.48f);
            }

            if (battleManager.FortressAttackSequence != observedFortressAttackSequence)
            {
                observedFortressAttackSequence = battleManager.FortressAttackSequence;
                if (battleManager.FortressAttackSequence > 0 && battleManager.LastHitSourceName == "요새")
                {
                    SpawnFortressCannonVisual(battleManager.LastHitPosition, battleManager.FortressAttackSequence);
                }
            }

            if (battleManager.HeroAttackBatchSequence != observedHeroAttackBatchSequence)
            {
                observedHeroAttackBatchSequence = battleManager.HeroAttackBatchSequence;
                IReadOnlyList<string> attackIds = battleManager.RecentHeroAttackIds;
                for (int i = 0; i < attackIds.Count; i++)
                {
                    if (heroLocalPositions.TryGetValue(attackIds[i], out Vector2 position))
                    {
                        if (heroActors.TryGetValue(attackIds[i], out WorldActor attackingHero))
                        {
                            attackingHero.AttackPulse = HeroAttackPulseDuration;
                        }

                        HeroState hero = FindDeployedHero(attackIds[i]);
                        if (hero != null)
                        {
                            int targetIndex = battleManager.GetHeroTargetVisualIndex(attackIds[i]);
                            Vector2 targetPosition = targetIndex >= 0
                                ? battleManager.GetVisibleEnemyBattlePosition(targetIndex)
                                : battleManager.LastHitPosition;
                            SpawnHeroAttackVisual(hero.Definition, position, targetPosition);
                        }

                    }
                }
            }
        }

        private void SpawnFortressCannonVisual(Vector2 targetPosition, int attackSequence)
        {
            int attackIndex = Mathf.Max(0, attackSequence - 1);
            float side = attackIndex % 2 == 0 ? -1f : 1f;
            Vector2 startPosition = GetFortressMuzzlePosition(side, battleManager.FortressLevel);
            if ((targetPosition - startPosition).sqrMagnitude <= 0.001f)
            {
                targetPosition = startPosition + Vector2.up * 0.85f;
            }

            SpawnProjectile(
                startPosition,
                targetPosition,
                new Color(0.08f, 0.08f, 0.07f, 1f),
                0.42f,
                0.16f,
                circleSprite);
            SpawnImpactEffect(startPosition + (targetPosition - startPosition).normalized * 0.16f, 2, new Color(0.88f, 0.66f, 0.34f, 0.52f), 0.26f);
        }

        private static Vector2 GetFortressMuzzlePosition(float side, int fortressLevel)
        {
            int tier = fortressLevel >= 180 ? 3 : fortressLevel >= 90 ? 2 : fortressLevel >= 25 ? 1 : 0;
            float normalizedSide = side < 0f ? -1f : 1f;
            return new Vector2(normalizedSide * (0.68f + tier * 0.16f), 0.64f + tier * 0.06f);
        }

        private void SpawnImpactEffect(Vector2 localPosition, int variant, Color color, float scale)
        {
            SpriteEffectVisual effect = GetSpriteEffect();
            BattlefieldSpriteAnimation animation = BattlefieldSpriteCatalog.GetImpactAnimation(variant);
            effect.Root.SetActive(true);
            effect.Animation = animation;
            effect.Body.sprite = animation != null && animation.IsValid ? animation.GetFrameClamped(0f) : squareSprite;
            effect.Body.color = color;
            effect.StartPosition = localPosition + new Vector2(0f, 0.08f);
            effect.BaseColor = color;
            effect.Duration = animation != null && animation.IsValid ? Mathf.Max(0.18f, animation.Duration) : 0.28f;
            effect.Life = effect.Duration;
            effect.StartScale = Mathf.Max(0.18f, scale * 0.44f);
            effect.EndScale = Mathf.Max(0.34f, scale);
            effect.RotationSpeed = variant % 2 == 0 ? 72f : -58f;
            effect.Root.transform.position = ToWorld(effect.StartPosition);
            effect.Root.transform.localScale = Vector3.one * effect.StartScale;
            effect.Body.transform.localPosition = Vector3.zero;
            effect.Body.transform.localScale = Vector3.one;
            effect.Body.transform.localRotation = Quaternion.Euler(0f, 0f, -18f + variant * 11f);
        }

        private SpriteEffectVisual GetSpriteEffect()
        {
            for (int i = 0; i < spriteEffects.Count; i++)
            {
                if (!spriteEffects[i].Root.activeSelf)
                {
                    return spriteEffects[i];
                }
            }

            GameObject root = new GameObject("ImpactEffect");
            root.transform.SetParent(sceneRoot, false);
            SpriteRenderer body = CreateSpriteRenderer("Body", root.transform, squareSprite, Color.white, 62);
            var effect = new SpriteEffectVisual(root, body);
            spriteEffects.Add(effect);
            return effect;
        }

        private void UpdateSpriteEffects(float deltaTime)
        {
            for (int i = 0; i < spriteEffects.Count; i++)
            {
                SpriteEffectVisual effect = spriteEffects[i];
                if (!effect.Root.activeSelf)
                {
                    continue;
                }

                effect.Life = Mathf.Max(0f, effect.Life - deltaTime);
                float progress = 1f - effect.Life / Mathf.Max(0.001f, effect.Duration);
                float eased = EaseOut(progress);
                float scale = Mathf.Lerp(effect.StartScale, effect.EndScale, eased);
                float elapsed = effect.Duration - effect.Life;
                effect.Root.transform.position = ToWorld(effect.StartPosition + new Vector2(0f, 0.12f * progress));
                effect.Root.transform.localScale = Vector3.one * scale;
                effect.Body.transform.localRotation *= Quaternion.Euler(0f, 0f, effect.RotationSpeed * deltaTime);
                effect.Body.sprite = effect.Animation != null && effect.Animation.IsValid
                    ? effect.Animation.GetFrameClamped(elapsed)
                    : squareSprite;

                Color color = effect.BaseColor;
                color.a = Mathf.Clamp01(effect.BaseColor.a * (1f - progress));
                effect.Body.color = color;

                if (effect.Life <= 0f)
                {
                    effect.Root.SetActive(false);
                }
            }
        }

        private void SpawnDamageFloater(Vector2 localPosition, string text, Color color, float scale)
        {
            DamageFloater floater = GetDamageFloater();
            floater.Root.SetActive(true);
            floater.StartPosition = localPosition + new Vector2(0f, 0.22f);
            floater.Life = 0.46f;
            floater.Duration = 0.46f;
            floater.Text.text = text;
            floater.Text.characterSize = 0.058f * scale;
            floater.BaseColor = color;
            floater.Root.transform.position = ToWorld(floater.StartPosition);
            floater.Root.transform.localScale = Vector3.one;
        }

        private DamageFloater GetDamageFloater()
        {
            for (int i = 0; i < damageFloaters.Count; i++)
            {
                if (!damageFloaters[i].Root.activeSelf)
                {
                    return damageFloaters[i];
                }
            }

            TextMesh text = CreateTextMesh("DamageFloater", sceneRoot, string.Empty, 0.105f, Color.white, 70);
            var floater = new DamageFloater(text.gameObject, text);
            damageFloaters.Add(floater);
            return floater;
        }

        private void UpdateDamageFloaters(float deltaTime)
        {
            for (int i = 0; i < damageFloaters.Count; i++)
            {
                DamageFloater floater = damageFloaters[i];
                if (!floater.Root.activeSelf)
                {
                    continue;
                }

                floater.Life = Mathf.Max(0f, floater.Life - deltaTime);
                float progress = 1f - floater.Life / Mathf.Max(0.001f, floater.Duration);
                Vector2 position = floater.StartPosition + new Vector2(0f, 0.34f * progress);
                floater.Root.transform.position = ToWorld(position);
                Color color = floater.BaseColor;
                color.a = Mathf.Clamp01(1f - progress);
                floater.Text.color = color;

                if (floater.Life <= 0f)
                {
                    floater.Root.SetActive(false);
                }
            }
        }

        private void SpawnHeroAttackVisual(HeroDefinition hero, Vector2 startPosition, Vector2 targetPosition)
        {
            if ((targetPosition - startPosition).sqrMagnitude <= 0.001f)
            {
                targetPosition = startPosition + Vector2.up * 0.7f;
            }

            switch (hero.Trait)
            {
                case HeroTrait.Ranged:
                    SpawnProjectile(startPosition + new Vector2(0.18f, 0.12f), targetPosition, new Color(1f, 0.90f, 0.40f, 1f), 0.34f, 0.34f, BattlefieldSpriteCatalog.GetArrowSprite());
                    break;
                case HeroTrait.Support:
                    SpawnProjectile(startPosition + new Vector2(0.10f, 0.22f), targetPosition, Color.Lerp(GetRarityColor(hero.Rarity), Color.white, 0.18f), 0.34f, 0.30f, null);
                    break;
                case HeroTrait.Defense:
                    SpawnProjectile(startPosition, targetPosition, new Color(0.66f, 0.86f, 1f, 1f), 0.26f, 0.26f, null);
                    break;
                default:
                    SpawnProjectile(startPosition, targetPosition, new Color(1f, 0.95f, 0.74f, 1f), 0.24f, 0.24f, null);
                    break;
            }
        }

        private void SpawnProjectile(Vector2 startPosition, Vector2 targetPosition, Color color, float duration, float size, Sprite sprite)
        {
            ProjectileVisual projectile = GetProjectile();
            projectile.Root.SetActive(true);
            projectile.StartPosition = startPosition;
            projectile.TargetPosition = targetPosition;
            projectile.Duration = Mathf.Max(0.05f, duration);
            projectile.Life = projectile.Duration;
            projectile.BaseColor = color;
            projectile.Sprite = sprite;
            projectile.Size = size;
            projectile.Body.color = color;
            projectile.Body.sprite = sprite != null ? sprite : squareSprite;
            projectile.Root.transform.position = ToWorld(startPosition);
            projectile.Root.transform.localScale = Vector3.one;
        }

        private ProjectileVisual GetProjectile()
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (!projectiles[i].Root.activeSelf)
                {
                    return projectiles[i];
                }
            }

            GameObject root = new GameObject("Projectile");
            root.transform.SetParent(sceneRoot, false);
            SpriteRenderer body = CreateSpriteRenderer("Body", root.transform, squareSprite, Color.white, 66);
            var projectile = new ProjectileVisual(root, body);
            projectiles.Add(projectile);
            return projectile;
        }

        private void UpdateProjectiles(float deltaTime)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                ProjectileVisual projectile = projectiles[i];
                if (!projectile.Root.activeSelf)
                {
                    continue;
                }

                projectile.Life = Mathf.Max(0f, projectile.Life - deltaTime);
                float progress = 1f - projectile.Life / Mathf.Max(0.001f, projectile.Duration);
                Vector2 position = Vector2.Lerp(projectile.StartPosition, projectile.TargetPosition, EaseOut(progress));
                projectile.Root.transform.position = ToWorld(position);

                Vector2 direction = projectile.TargetPosition - projectile.StartPosition;
                float angle = direction.sqrMagnitude > 0.001f ? Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg : 0f;
                Color color = projectile.BaseColor;
                color.a = Mathf.Clamp01(1f - progress * 0.45f);
                projectile.Body.color = color;
                bool usesAssetSprite = projectile.Sprite != null;
                projectile.Body.sprite = usesAssetSprite ? projectile.Sprite : squareSprite;
                projectile.Body.transform.localPosition = Vector3.zero;
                projectile.Body.transform.localScale = usesAssetSprite
                    ? Vector3.one * Mathf.Max(0.18f, projectile.Size * 2.8f)
                    : new Vector3(Mathf.Max(0.12f, projectile.Size * 1.20f), Mathf.Max(0.035f, projectile.Size * 0.18f), 1f);
                projectile.Body.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                if (projectile.Life <= 0f)
                {
                    projectile.Root.SetActive(false);
                }
            }
        }
    }
}
