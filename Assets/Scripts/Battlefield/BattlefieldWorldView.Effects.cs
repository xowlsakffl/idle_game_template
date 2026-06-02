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
                        enemyActors[hitIndex].HitPulse = 0.20f;
                    }

                    SpawnDamageFloater(
                        hitPosition,
                        "-" + NumberFormatter.Format(battleManager.LastHitDamage),
                        battleManager.LastHitWasCritical ? new Color(1f, 0.92f, 0.16f, 1f) : new Color(1f, 0.42f, 0.18f, 1f),
                        battleManager.LastHitWasCritical ? 1.25f : 1f);
                }
            }

            if (battleManager.MonsterHitSequence != observedMonsterHitSequence)
            {
                observedMonsterHitSequence = battleManager.MonsterHitSequence;
                int attackingEnemyIndex = battleManager.RecentAttackingEnemyIndex;
                if (attackingEnemyIndex >= 0 && attackingEnemyIndex < enemyActors.Count)
                {
                    enemyActors[attackingEnemyIndex].AttackPulse = 0.18f;
                }

                int damagedHeroIndex = battleManager.RecentDamagedHeroIndex;
                IReadOnlyList<HeroState> deployedHeroes = battleManager.DeployedHeroes;
                if (damagedHeroIndex >= 0 && damagedHeroIndex < deployedHeroes.Count
                    && heroActors.TryGetValue(deployedHeroes[damagedHeroIndex].Definition.Id, out WorldActor damagedHero))
                {
                    damagedHero.HitPulse = 0.20f;
                }

                SpawnDamageFloater(battleManager.LastMonsterHitPosition, "HIT", new Color(0.95f, 0.20f, 0.18f, 1f), 0.72f);
            }

            if (battleManager.EnemyDefeatSequence != observedEnemyDefeatSequence)
            {
                observedEnemyDefeatSequence = battleManager.EnemyDefeatSequence;
                SpawnDamageFloater(battleManager.LastDefeatedEnemyPosition, "KO", new Color(1f, 0.86f, 0.24f, 1f), 1.05f);
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
                            attackingHero.AttackPulse = 0.22f;
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

        private void SpawnDamageFloater(Vector2 localPosition, string text, Color color, float scale)
        {
            DamageFloater floater = GetDamageFloater();
            floater.Root.SetActive(true);
            floater.StartPosition = localPosition + new Vector2(0f, 0.34f);
            floater.Life = 0.72f;
            floater.Duration = 0.72f;
            floater.Text.text = text;
            floater.Text.characterSize = 0.105f * scale;
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
                Vector2 position = floater.StartPosition + new Vector2(0f, 0.62f * progress);
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
                    SpawnProjectile(startPosition + new Vector2(0.18f, 0.12f), targetPosition, new Color(1f, 0.86f, 0.34f, 1f), 0.24f, 0.20f);
                    break;
                case HeroTrait.Support:
                    SpawnProjectile(startPosition + new Vector2(0.10f, 0.22f), targetPosition, Color.Lerp(GetRarityColor(hero.Rarity), Color.white, 0.18f), 0.30f, 0.24f);
                    break;
                case HeroTrait.Defense:
                    SpawnProjectile(startPosition, targetPosition, new Color(0.66f, 0.86f, 1f, 1f), 0.20f, 0.18f);
                    break;
                default:
                    SpawnProjectile(startPosition, targetPosition, new Color(1f, 0.95f, 0.74f, 1f), 0.16f, 0.18f);
                    break;
            }
        }

        private void SpawnProjectile(Vector2 startPosition, Vector2 targetPosition, Color color, float duration, float size)
        {
            ProjectileVisual projectile = GetProjectile();
            projectile.Root.SetActive(true);
            projectile.StartPosition = startPosition;
            projectile.TargetPosition = targetPosition;
            projectile.Duration = Mathf.Max(0.05f, duration);
            projectile.Life = projectile.Duration;
            projectile.BaseColor = color;
            projectile.Size = size;
            projectile.Body.color = color;
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
            SpriteRenderer body = CreateSpriteRenderer("Body", root.transform, squareSprite, Color.white, 55);
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
                color.a = Mathf.Clamp01(1f - progress * 0.78f);
                projectile.Body.color = color;
                projectile.Body.sprite = squareSprite;
                projectile.Body.transform.localPosition = Vector3.zero;
                projectile.Body.transform.localScale = new Vector3(Mathf.Max(0.12f, projectile.Size * 1.20f), Mathf.Max(0.035f, projectile.Size * 0.18f), 1f);
                projectile.Body.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                if (projectile.Life <= 0f)
                {
                    projectile.Root.SetActive(false);
                }
            }
        }
    }
}
