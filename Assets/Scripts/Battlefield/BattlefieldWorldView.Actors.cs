using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Speed;

namespace IdleGame.Battlefield
{
    public sealed partial class BattlefieldWorldView
    {
        private void TickActorAnimationState(float deltaTime)
        {
            foreach (WorldActor actor in heroActors.Values)
            {
                actor.AttackPulse = Mathf.Max(0f, actor.AttackPulse - deltaTime);
                actor.HitPulse = Mathf.Max(0f, actor.HitPulse - deltaTime);
                actor.SpawnPulse = Mathf.Max(0f, actor.SpawnPulse - deltaTime);
            }

            for (int i = 0; i < enemyActors.Count; i++)
            {
                WorldActor actor = enemyActors[i];
                actor.AttackPulse = Mathf.Max(0f, actor.AttackPulse - deltaTime);
                actor.HitPulse = Mathf.Max(0f, actor.HitPulse - deltaTime);
                actor.SpawnPulse = Mathf.Max(0f, actor.SpawnPulse - deltaTime);
            }
        }

        private void UpdateHeroes(float deltaTime)
        {
            IReadOnlyList<HeroState> deployedHeroes = battleManager.DeployedHeroes;
            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                HeroState hero = deployedHeroes[i];
                WorldActor actor = GetOrCreateHeroActor(hero.Definition);
                actor.Root.SetActive(true);

                Vector2 targetPosition = battleManager.GetHeroBattlePosition(hero.Definition.Id);
                bool isAttacking = actor.AttackPulse > 0f;

                if (!heroLocalPositions.TryGetValue(hero.Definition.Id, out Vector2 currentPosition))
                {
                    currentPosition = targetPosition;
                }

                Vector2 previousPosition = currentPosition;
                float moveSpeed = 4.8f + Mathf.Max(0.1f, hero.MoveSpeed) * 0.65f;
                currentPosition = Vector2.MoveTowards(currentPosition, targetPosition, moveSpeed * deltaTime);
                bool isMoving = (currentPosition - previousPosition).sqrMagnitude > 0.0001f;
                heroLocalPositions[hero.Definition.Id] = currentPosition;

                actor.LocalPosition = currentPosition;
                Vector2 renderPosition = currentPosition + GetHeroAnimationOffset(actor, currentPosition);
                actor.Root.transform.position = ToWorld(renderPosition);
                bool alive = battleManager.IsHeroBattleAlive(hero.Definition.Id);
                bool fortressProtected = CombatMovementService.IsFortressProtectedHero(hero);
                bool hit = actor.HitPulse > 0f;
                ConfigureHeroActorVisuals(actor, hero.Definition, alive, hit, isAttacking, isMoving, deltaTime);
                actor.HpRoot.SetActive(!fortressProtected);
                SetActorHp(actor, battleManager.GetHeroHpRatio(hero.Definition.Id), alive ? new Color(0.42f, 0.95f, 0.34f, 1f) : new Color(0.55f, 0.58f, 0.64f, 1f));

                float scale = fortressProtected ? 0.74f : hero.Definition.Trait == HeroTrait.Defense ? 1.16f : 1f;
                if (!alive)
                {
                    scale *= 0.76f;
                }

                if (isAttacking)
                {
                    scale += GetPulseRatio(actor.AttackPulse, HeroAttackPulseDuration) * 0.16f;
                }

                if (hit)
                {
                    scale += GetPulseRatio(actor.HitPulse, ActorHitPulseDuration) * 0.10f;
                }

                actor.Root.transform.localScale = new Vector3(scale, scale, 1f);
            }

            HideUnusedHeroActors(deployedHeroes);
        }

        private void HideUnusedHeroActors(IReadOnlyList<HeroState> deployedHeroes)
        {
            foreach (KeyValuePair<string, WorldActor> pair in heroActors)
            {
                bool deployed = false;
                for (int i = 0; i < deployedHeroes.Count; i++)
                {
                    if (deployedHeroes[i].Definition.Id == pair.Key)
                    {
                        deployed = true;
                        break;
                    }
                }

                if (!deployed)
                {
                    pair.Value.Root.SetActive(false);
                }
            }
        }

        private void UpdateEnemies(float deltaTime)
        {
            int visible = Mathf.Clamp(battleManager.VisibleEnemyCount, 0, enemyActors.Count);
            for (int i = 0; i < enemyActors.Count; i++)
            {
                WorldActor actor = enemyActors[i];
                if (i >= visible)
                {
                    actor.Root.SetActive(false);
                    actor.LastSpawnSequence = -1;
                    continue;
                }

                int spawnSequence = battleManager.GetVisibleEnemySpawnSequence(i);
                Vector2 desiredPosition = battleManager.GetVisibleEnemyBattlePosition(i);
                bool spawnChanged = actor.LastSpawnSequence != spawnSequence;
                if (spawnChanged)
                {
                    if (actor.LastSpawnSequence >= 0 && actor.Root.activeSelf)
                    {
                        SpawnDamageFloater(actor.LocalPosition, "KO", new Color(1f, 0.86f, 0.24f, 1f), 0.95f);
                    }

                    actor.LocalPosition = desiredPosition;
                    enemyLocalPositions[i] = actor.LocalPosition;
                    actor.LastSpawnSequence = spawnSequence;
                    actor.SpawnPulse = 0.30f;
                }

                actor.Root.SetActive(true);

                Vector2 previousPosition = actor.LocalPosition;
                float speed = battleManager.IsBossFight ? 5.0f : 6.4f + (spawnSequence % 4) * 0.24f;
                actor.LocalPosition = Vector2.MoveTowards(actor.LocalPosition, desiredPosition, speed * deltaTime);
                bool isMoving = (actor.LocalPosition - previousPosition).sqrMagnitude > 0.0001f;
                enemyLocalPositions[i] = actor.LocalPosition;
                Vector2 renderPosition = actor.LocalPosition + GetEnemyAnimationOffset(actor, actor.LocalPosition);
                actor.Root.transform.position = ToWorld(renderPosition);

                float hitPulse = actor.HitPulse > 0f ? GetPulseRatio(actor.HitPulse, ActorHitPulseDuration) * 0.06f : 0f;
                float spawnScale = actor.SpawnPulse > 0f ? Mathf.Lerp(0.35f, 1f, 1f - actor.SpawnPulse / 0.30f) : 1f;
                float scale = battleManager.IsBossFight ? 2.18f : 1.08f + (i % 3) * 0.06f;
                if (actor.AttackPulse > 0f)
                {
                    scale += GetPulseRatio(actor.AttackPulse, EnemyAttackPulseDuration) * 0.11f;
                }

                actor.Root.transform.localScale = new Vector3((scale + hitPulse) * spawnScale, (scale + hitPulse) * spawnScale, 1f);
                ConfigureEnemyActorVisuals(actor, i, battleManager.IsBossFight, actor.HitPulse > 0f, actor.AttackPulse > 0f, isMoving, deltaTime);
                actor.HpRoot.SetActive(true);
                SetActorHp(actor, battleManager.GetVisibleEnemyHpRatio(i), battleManager.IsBossFight ? new Color(1f, 0.20f, 0.16f, 1f) : new Color(0.40f, 0.95f, 0.24f, 1f));
            }
        }

        private void ResetEnemyVisualContinuity()
        {
            for (int i = 0; i < enemyActors.Count; i++)
            {
                WorldActor actor = enemyActors[i];
                actor.Root.SetActive(false);
                actor.LocalPosition = Vector2.zero;
                actor.LastSpawnSequence = -1;
                actor.AttackPulse = 0f;
                actor.HitPulse = 0f;
                actor.SpawnPulse = 0f;
                actor.AnimationKey = null;
                actor.AnimationTime = 0f;

                if (i < enemyLocalPositions.Count)
                {
                    enemyLocalPositions[i] = Vector2.zero;
                }
            }

            HideTransientCombatVisuals();
        }

        private void HideTransientCombatVisuals()
        {
            for (int i = 0; i < damageFloaters.Count; i++)
            {
                damageFloaters[i].Life = 0f;
                damageFloaters[i].Root.SetActive(false);
            }

            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Life = 0f;
                projectiles[i].Root.SetActive(false);
            }

            for (int i = 0; i < spriteEffects.Count; i++)
            {
                spriteEffects[i].Life = 0f;
                spriteEffects[i].Root.SetActive(false);
            }
        }

        private HeroState FindDeployedHero(string heroId)
        {
            IReadOnlyList<HeroState> deployedHeroes = battleManager.DeployedHeroes;
            for (int i = 0; i < deployedHeroes.Count; i++)
            {
                if (deployedHeroes[i].Definition.Id == heroId)
                {
                    return deployedHeroes[i];
                }
            }

            return null;
        }

        private WorldActor GetOrCreateHeroActor(HeroDefinition definition)
        {
            if (heroActors.TryGetValue(definition.Id, out WorldActor actor))
            {
                return actor;
            }

            actor = InstantiateActor(heroTemplate, "HeroActor_" + definition.Id);
            heroActors[definition.Id] = actor;
            return actor;
        }

        private WorldActor InstantiateActor(GameObject template, string actorName)
        {
            GameObject instance = Instantiate(template, actorRoot);
            instance.name = actorName;
            instance.SetActive(false);

            Transform body = instance.transform.Find("Body");
            Transform hpRoot = instance.transform.Find("HpRoot");
            Transform hpFill = hpRoot != null ? hpRoot.Find("HpFill") : null;

            return new WorldActor(
                instance,
                body.GetComponent<SpriteRenderer>(),
                hpRoot.gameObject,
                hpFill.GetComponent<SpriteRenderer>());
        }

        private GameObject CreateActorTemplate(string name, bool hero)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(templateRoot, false);
            root.SetActive(false);

            SpriteRenderer body = CreateSpriteRenderer("Body", root.transform, squareSprite, hero ? new Color(0.36f, 0.56f, 0.96f, 1f) : new Color(0.70f, 0.13f, 0.10f, 1f), hero ? 4 : 14);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = hero ? Vector3.one * 0.58f : Vector3.one * 0.54f;

            GameObject hpRoot = new GameObject("HpRoot");
            hpRoot.transform.SetParent(root.transform, false);
            hpRoot.transform.localPosition = hero ? new Vector3(0f, 0.78f, 0f) : new Vector3(0f, 1.02f, 0f);

            SpriteRenderer hpBack = CreateSpriteRenderer("HpBack", hpRoot.transform, squareSprite, new Color(0.02f, 0.025f, 0.03f, 0.95f), hero ? 7 : 17);
            hpBack.transform.localScale = new Vector3(0.86f, 0.07f, 1f);

            SpriteRenderer hpFill = CreateSpriteRenderer("HpFill", hpRoot.transform, squareSprite, new Color(0.40f, 0.95f, 0.24f, 1f), hero ? 8 : 18);
            hpFill.transform.localScale = new Vector3(0.80f, 0.045f, 1f);

            return root;
        }

        private void ConfigureHeroActorVisuals(WorldActor actor, HeroDefinition hero, bool alive, bool hit, bool attacking, bool moving, float deltaTime)
        {
            Color rarityColor = GetRarityColor(hero.Rarity);
            float alpha = alive ? 1f : 0.42f;
            Color bodyColor = WithAlpha(rarityColor, alpha);
            BattlefieldAnimationState state = attacking ? BattlefieldAnimationState.Attack : moving ? BattlefieldAnimationState.Run : BattlefieldAnimationState.Idle;
            BattlefieldSpriteAnimation animation = BattlefieldSpriteCatalog.GetHeroAnimation(hero, state);
            bool usesAssetSprite = animation != null && animation.IsValid;

            actor.Body.gameObject.SetActive(true);
            actor.Body.sprite = usesAssetSprite ? GetActorAnimationFrame(actor, animation, deltaTime) : squareSprite;
            actor.Body.color = usesAssetSprite
                ? new Color(1f, 1f, 1f, alpha)
                : bodyColor;

            float bodyScale = usesAssetSprite
                ? (hero.Trait == HeroTrait.Defense ? 1.04f : 0.96f)
                : 0.58f;
            float hitRatio = hit ? GetPulseRatio(actor.HitPulse, ActorHitPulseDuration) : 0f;
            Vector3 bodyPosition = usesAssetSprite ? new Vector3(0f, -0.10f - hitRatio * 0.025f, 0f) : Vector3.zero;
            Vector3 bodyScaleVector = new Vector3(bodyScale * (1f + hitRatio * 0.035f), bodyScale * (1f - hitRatio * 0.025f), 1f);
            SetRendererPart(actor.Body, bodyPosition, bodyScaleVector, 0f);
        }

        private void ConfigureEnemyActorVisuals(WorldActor actor, int index, bool boss, bool hit, bool attacking, bool moving, float deltaTime)
        {
            Color baseColor = boss
                ? new Color(0.86f, 0.18f, 0.14f, 1f)
                : Color.Lerp(new Color(0.58f, 0.12f, 0.10f, 1f), new Color(0.95f, 0.38f, 0.12f, 1f), index / (float)Mathf.Max(1, GameData.MaxVisibleEnemies - 1));
            BattlefieldAnimationState state = attacking ? BattlefieldAnimationState.Attack : moving ? BattlefieldAnimationState.Run : BattlefieldAnimationState.Idle;
            BattlefieldSpriteAnimation animation = BattlefieldSpriteCatalog.GetEnemyAnimation(index, boss, state);
            bool usesAssetSprite = animation != null && animation.IsValid;

            actor.Body.gameObject.SetActive(true);
            actor.Body.sprite = usesAssetSprite ? GetActorAnimationFrame(actor, animation, deltaTime) : squareSprite;
            actor.Body.color = usesAssetSprite
                ? Color.white
                : baseColor;

            float bodyScale = usesAssetSprite
                ? (boss ? 1.62f : 1.42f)
                : (boss ? 0.72f : 0.54f);
            float hitRatio = hit ? GetPulseRatio(actor.HitPulse, ActorHitPulseDuration) : 0f;
            Vector3 bodyPosition = usesAssetSprite ? new Vector3(0f, -0.08f, 0f) : Vector3.zero;
            Vector3 bodyScaleVector = new Vector3(bodyScale * (1f + hitRatio * 0.018f), bodyScale * (1f - hitRatio * 0.010f), 1f);
            SetRendererPart(actor.Body, bodyPosition, bodyScaleVector, 0f);
        }

        private static Sprite GetActorAnimationFrame(WorldActor actor, BattlefieldSpriteAnimation animation, float deltaTime)
        {
            if (actor.AnimationKey != animation.Key)
            {
                actor.AnimationKey = animation.Key;
                actor.AnimationTime = 0f;
            }
            else
            {
                actor.AnimationTime += Mathf.Max(0f, deltaTime);
            }

            return animation.GetFrame(actor.AnimationTime);
        }

        private static void SetRendererPart(SpriteRenderer renderer, Vector3 localPosition, Vector3 localScale, float zRotation)
        {
            renderer.transform.localPosition = localPosition;
            renderer.transform.localScale = localScale;
            renderer.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        }

        private static SpriteRenderer CreateSpriteRenderer(string name, Transform parent, Sprite sprite, Color color, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static TextMesh CreateTextMesh(string name, Transform parent, string text, float characterSize, Color color, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            TextMesh mesh = obj.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = characterSize;
            mesh.fontSize = 64;
            mesh.color = color;

            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.sortingOrder = sortingOrder;
            return mesh;
        }

        private void SetActorHp(WorldActor actor, float ratio, Color fillColor)
        {
            float clamped = Mathf.Clamp01(ratio);
            actor.HpFill.color = fillColor;
            actor.HpFill.transform.localScale = new Vector3(0.80f * clamped, 0.045f, 1f);
            actor.HpFill.transform.localPosition = new Vector3(-0.40f * (1f - clamped), 0f, 0f);
        }
    }
}
