using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Speed;

namespace IdleGame.Battlefield
{
    public sealed class BattlefieldWorldView : MonoBehaviour
    {
        private const int RenderWidth = 720;
        private const int RenderHeight = 980;
        private const float FieldHalfWidth = 3.85f;
        private const float FieldHalfHeight = 5.15f;
        private const float OriginX = 1000f;

        private static Sprite squareSprite;

        private readonly Dictionary<string, WorldActor> heroActors = new Dictionary<string, WorldActor>();
        private readonly List<WorldActor> enemyActors = new List<WorldActor>();
        private readonly List<DamageFloater> damageFloaters = new List<DamageFloater>();
        private readonly List<ProjectileVisual> projectiles = new List<ProjectileVisual>();
        private readonly Dictionary<string, Vector2> heroLocalPositions = new Dictionary<string, Vector2>();
        private readonly List<Vector2> enemyLocalPositions = new List<Vector2>();

        private BattleManager battleManager;
        private GameSpeedManager speedManager;
        private Camera renderCamera;
        private RenderTexture renderTexture;
        private Transform sceneRoot;
        private Transform actorRoot;
        private Transform templateRoot;
        private GameObject heroTemplate;
        private GameObject enemyTemplate;
        private Transform fortressRoot;
        private SpriteRenderer fortressBaseRenderer;
        private SpriteRenderer fortressHpFillRenderer;
        private int observedHitSequence = -1;
        private int observedHeroAttackBatchSequence = -1;
        private int observedMonsterHitSequence = -1;
        private int observedEnemyDefeatSequence = -1;
        private bool sceneCreated;

        public Texture OutputTexture
        {
            get
            {
                EnsureScene();
                return renderTexture;
            }
        }

        public void Initialize(BattleManager battle, GameSpeedManager speed = null)
        {
            battleManager = battle;
            speedManager = speed;
            observedHitSequence = battleManager != null ? battleManager.HitSequence : -1;
            observedHeroAttackBatchSequence = battleManager != null ? battleManager.HeroAttackBatchSequence : -1;
            observedMonsterHitSequence = battleManager != null ? battleManager.MonsterHitSequence : -1;
            observedEnemyDefeatSequence = battleManager != null ? battleManager.EnemyDefeatSequence : -1;
            EnsureScene();
        }

        private void LateUpdate()
        {
            if (battleManager == null)
            {
                return;
            }

            EnsureScene();

            float speedMultiplier = speedManager != null ? Mathf.Max(1, speedManager.CurrentMultiplier) : 1f;
            float rawDeltaTime = Time.deltaTime > 0f ? Time.deltaTime : 1f / 60f;
            float deltaTime = Mathf.Min(rawDeltaTime * speedMultiplier, 0.08f);
            TickActorAnimationState(deltaTime);
            UpdateFortressVisual();
            UpdateHeroes(deltaTime);
            UpdateEnemies(deltaTime);
            PlayHitBursts();
            UpdateProjectiles(deltaTime);
            UpdateDamageFloaters(deltaTime);
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }
        }

        private void EnsureScene()
        {
            if (sceneCreated)
            {
                return;
            }

            EnsureSprites();
            sceneCreated = true;

            renderTexture = new RenderTexture(RenderWidth, RenderHeight, 16, RenderTextureFormat.ARGB32)
            {
                name = "IdleGameBattlefieldWorld",
                antiAliasing = 2,
                useMipMap = false
            };
            renderTexture.Create();

            sceneRoot = new GameObject("BattlefieldWorldScene").transform;
            sceneRoot.SetParent(transform, false);
            sceneRoot.position = new Vector3(OriginX, 0f, 0f);

            actorRoot = new GameObject("Actors").transform;
            actorRoot.SetParent(sceneRoot, false);

            templateRoot = new GameObject("Templates").transform;
            templateRoot.SetParent(sceneRoot, false);

            SpriteRenderer backgroundRenderer = CreateSpriteRenderer("Background", sceneRoot, squareSprite, new Color(0.13f, 0.16f, 0.20f, 1f), -50);
            backgroundRenderer.transform.localScale = new Vector3(FieldHalfWidth * 2.2f, FieldHalfHeight * 2.2f, 1f);

            CreateFortressVisual(sceneRoot);

            heroTemplate = CreateActorTemplate("HeroActorTemplate", true);
            enemyTemplate = CreateActorTemplate("EnemyActorTemplate", false);

            for (int i = 0; i < GameData.MaxVisibleEnemies; i++)
            {
                enemyActors.Add(InstantiateActor(enemyTemplate, "EnemyActor" + i));
                enemyLocalPositions.Add(Vector2.zero);
            }

            GameObject cameraObject = new GameObject("BattlefieldWorldCamera");
            cameraObject.transform.SetParent(transform, false);
            renderCamera = cameraObject.AddComponent<Camera>();
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = FieldHalfHeight;
            renderCamera.aspect = RenderWidth / (float)RenderHeight;
            renderCamera.transform.position = new Vector3(OriginX, 0f, -10f);
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
            renderCamera.targetTexture = renderTexture;
        }

        private static void EnsureSprites()
        {
            if (squareSprite != null)
            {
                return;
            }

            squareSprite = CreateSquareSprite();
        }

        private static Sprite CreateSquareSprite()
        {
            Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
            {
                name = "RuntimeSquareSprite",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
        }

        private void CreateFortressVisual(Transform parent)
        {
            fortressRoot = new GameObject("Fortress").transform;
            fortressRoot.SetParent(parent, false);
            fortressRoot.localPosition = Vector3.zero;

            fortressBaseRenderer = CreateSpriteRenderer("FortressBody", fortressRoot, squareSprite, new Color(0.25f, 0.30f, 0.38f, 1f), 1);
            fortressBaseRenderer.transform.localPosition = Vector3.zero;
            fortressBaseRenderer.transform.localScale = new Vector3(1.20f, 0.82f, 1f);

            SpriteRenderer hpBack = CreateSpriteRenderer("FortressHpBack", fortressRoot, squareSprite, new Color(0.02f, 0.025f, 0.03f, 0.95f), 5);
            hpBack.transform.localPosition = new Vector3(0f, 0.98f, 0f);
            hpBack.transform.localScale = new Vector3(1.10f, 0.08f, 1f);

            fortressHpFillRenderer = CreateSpriteRenderer("FortressHpFill", fortressRoot, squareSprite, new Color(0.40f, 0.95f, 0.72f, 1f), 6);
            fortressHpFillRenderer.transform.localPosition = new Vector3(0f, 0.98f, 0f);
            fortressHpFillRenderer.transform.localScale = new Vector3(1.04f, 0.052f, 1f);
        }

        private void UpdateFortressVisual()
        {
            if (fortressRoot == null || battleManager == null)
            {
                return;
            }

            float hpRatio = battleManager.FortressHpRatio;
            bool alive = hpRatio > 0f;
            fortressRoot.localScale = Vector3.one;

            Color healthyBase = new Color(0.25f, 0.30f, 0.38f, 1f);
            Color damagedBase = new Color(0.22f, 0.13f, 0.12f, 1f);
            if (fortressBaseRenderer != null)
            {
                fortressBaseRenderer.color = Color.Lerp(damagedBase, healthyBase, hpRatio);
            }

            if (fortressHpFillRenderer != null)
            {
                fortressHpFillRenderer.color = alive ? new Color(0.40f, 0.95f, 0.72f, 1f) : new Color(0.60f, 0.12f, 0.10f, 1f);
                fortressHpFillRenderer.transform.localScale = new Vector3(1.04f * Mathf.Clamp01(hpRatio), 0.052f, 1f);
                fortressHpFillRenderer.transform.localPosition = new Vector3(-0.52f * (1f - Mathf.Clamp01(hpRatio)), 0.98f, 0f);
            }
        }

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

                float moveSpeed = 4.8f + Mathf.Max(0.1f, hero.MoveSpeed) * 0.65f;
                currentPosition = Vector2.MoveTowards(currentPosition, targetPosition, moveSpeed * deltaTime);
                heroLocalPositions[hero.Definition.Id] = currentPosition;

                actor.LocalPosition = currentPosition;
                Vector2 renderPosition = currentPosition + GetHeroAnimationOffset(actor, currentPosition);
                actor.Root.transform.position = ToWorld(renderPosition);
                bool alive = battleManager.IsHeroBattleAlive(hero.Definition.Id);
                bool hit = actor.HitPulse > 0f;
                ConfigureHeroActorVisuals(actor, hero.Definition, alive, hit);
                actor.HpRoot.SetActive(true);
                SetActorHp(actor, battleManager.GetHeroHpRatio(hero.Definition.Id), alive ? new Color(0.42f, 0.95f, 0.34f, 1f) : new Color(0.55f, 0.58f, 0.64f, 1f));

                float scale = hero.Definition.Trait == HeroTrait.Defense ? 1.16f : 1f;
                if (!alive)
                {
                    scale *= 0.76f;
                }

                if (isAttacking)
                {
                    scale += GetPulseRatio(actor.AttackPulse, 0.22f) * 0.16f;
                }

                if (hit)
                {
                    scale += GetPulseRatio(actor.HitPulse, 0.20f) * 0.10f;
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

                float speed = battleManager.IsBossFight ? 5.0f : 6.4f + (spawnSequence % 4) * 0.24f;
                actor.LocalPosition = Vector2.MoveTowards(actor.LocalPosition, desiredPosition, speed * deltaTime);
                enemyLocalPositions[i] = actor.LocalPosition;
                Vector2 renderPosition = actor.LocalPosition + GetEnemyAnimationOffset(actor, actor.LocalPosition);
                actor.Root.transform.position = ToWorld(renderPosition);

                bool isRecentHit = battleManager.IsBossFight ? i == 0 : i == battleManager.RecentHitEnemyIndex;
                float hitPulse = actor.HitPulse > 0f ? GetPulseRatio(actor.HitPulse, 0.20f) * 0.18f : 0f;
                float spawnScale = actor.SpawnPulse > 0f ? Mathf.Lerp(0.35f, 1f, 1f - actor.SpawnPulse / 0.30f) : 1f;
                float scale = battleManager.IsBossFight ? 1.72f : 0.82f + (i % 3) * 0.04f;
                if (actor.AttackPulse > 0f)
                {
                    scale += GetPulseRatio(actor.AttackPulse, 0.18f) * 0.11f;
                }

                actor.Root.transform.localScale = new Vector3((scale + hitPulse) * spawnScale, (scale + hitPulse) * spawnScale, 1f);
                ConfigureEnemyActorVisuals(actor, i, battleManager.IsBossFight, actor.HitPulse > 0f);
                actor.HpRoot.SetActive(true);
                SetActorHp(actor, battleManager.GetVisibleEnemyHpRatio(i), battleManager.IsBossFight ? new Color(1f, 0.20f, 0.16f, 1f) : new Color(0.40f, 0.95f, 0.24f, 1f));
            }
        }

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
            hpRoot.transform.localPosition = new Vector3(0f, 0.63f, 0f);

            SpriteRenderer hpBack = CreateSpriteRenderer("HpBack", hpRoot.transform, squareSprite, new Color(0.02f, 0.025f, 0.03f, 0.95f), hero ? 7 : 17);
            hpBack.transform.localScale = new Vector3(0.86f, 0.07f, 1f);

            SpriteRenderer hpFill = CreateSpriteRenderer("HpFill", hpRoot.transform, squareSprite, new Color(0.40f, 0.95f, 0.24f, 1f), hero ? 8 : 18);
            hpFill.transform.localScale = new Vector3(0.80f, 0.045f, 1f);

            return root;
        }

        private void ConfigureHeroActorVisuals(WorldActor actor, HeroDefinition hero, bool alive, bool hit)
        {
            Color rarityColor = GetRarityColor(hero.Rarity);
            float alpha = alive ? 1f : 0.42f;
            Color bodyColor = hit ? Color.white : WithAlpha(rarityColor, alpha);

            actor.Body.gameObject.SetActive(true);
            actor.Body.sprite = squareSprite;
            actor.Body.color = bodyColor;
            SetRendererPart(actor.Body, Vector3.zero, Vector3.one * 0.58f, 0f);
        }

        private void ConfigureEnemyActorVisuals(WorldActor actor, int index, bool boss, bool hit)
        {
            Color baseColor = boss
                ? new Color(0.86f, 0.18f, 0.14f, 1f)
                : Color.Lerp(new Color(0.58f, 0.12f, 0.10f, 1f), new Color(0.95f, 0.38f, 0.12f, 1f), index / (float)Mathf.Max(1, GameData.MaxVisibleEnemies - 1));

            actor.Body.gameObject.SetActive(true);
            actor.Body.sprite = squareSprite;
            actor.Body.color = hit ? Color.white : baseColor;
            SetRendererPart(actor.Body, Vector3.zero, boss ? Vector3.one * 0.72f : Vector3.one * 0.54f, 0f);
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

        private Vector2 GetHeroFormationPosition(int heroIndex)
        {
            switch (heroIndex % GameData.MaxPartyHeroes)
            {
                case 0:
                    return new Vector2(-0.60f, -0.75f);
                case 1:
                    return new Vector2(0.58f, -0.78f);
                case 2:
                    return new Vector2(-1.30f, -1.34f);
                case 3:
                    return new Vector2(1.28f, -1.36f);
                case 4:
                    return new Vector2(-1.76f, -0.28f);
                case 5:
                    return new Vector2(1.76f, -0.30f);
                case 6:
                    return new Vector2(-0.22f, -1.72f);
                default:
                    return new Vector2(0.86f, -1.70f);
            }
        }

        private Vector2 GetHeroTargetPosition(string heroId, Vector2 fallback)
        {
            int targetIndex = battleManager.GetHeroTargetVisualIndex(heroId);
            if (targetIndex >= 0 && targetIndex < enemyLocalPositions.Count)
            {
                return enemyLocalPositions[targetIndex];
            }

            return FindNearestEnemy(fallback);
        }

        private Vector2 GetHeroDesiredPosition(HeroState hero, int index, Vector2 basePosition, Vector2 targetPosition, bool isAttacking)
        {
            Vector2 toTarget = targetPosition - basePosition;
            Vector2 direction = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector2.up;
            Vector2 tangent = new Vector2(-direction.y, direction.x);
            float time = Time.time;
            float phase = index * 1.17f;
            float attackPush = isAttacking ? 0.42f : 0f;
            float move = Mathf.Max(0.1f, hero.MoveSpeed);

            switch (hero.Definition.Trait)
            {
                case HeroTrait.Melee:
                    return ClampField(basePosition + direction * (0.76f + attackPush) + tangent * Mathf.Sin(time * (1.8f + move * 0.08f) + phase) * 0.18f);
                case HeroTrait.Ranged:
                    return ClampField(basePosition + direction * 0.18f + tangent * Mathf.Sin(time * (1.2f + move * 0.05f) + phase) * 0.48f);
                case HeroTrait.Support:
                    return ClampField(basePosition + new Vector2(Mathf.Cos(time * 0.9f + phase) * 0.36f, Mathf.Sin(time * 1.1f + phase) * 0.24f) + direction * attackPush * 0.45f);
                case HeroTrait.Defense:
                    return ClampField(basePosition + direction * (0.48f + attackPush * 0.55f) + tangent * Mathf.Sin(time * 0.9f + phase) * 0.14f);
                default:
                    return ClampField(basePosition + direction * (0.42f + attackPush));
            }
        }

        private Vector2 GetEnemySpawnPosition(int spawnSequence)
        {
            int side = Mathf.Abs(spawnSequence) % 4;
            float offset = Mathf.Lerp(-2.6f, 2.6f, PseudoRandom01(spawnSequence * 19 + 5));
            switch (side)
            {
                case 0:
                    return new Vector2(-FieldHalfWidth - 0.55f, offset);
                case 1:
                    return new Vector2(FieldHalfWidth + 0.55f, offset);
                case 2:
                    return new Vector2(offset, FieldHalfHeight + 0.55f);
                default:
                    return new Vector2(offset, -FieldHalfHeight - 0.55f);
            }
        }

        private Vector2 GetEnemyDesiredPosition(int index, int spawnSequence, Vector2 currentPosition)
        {
            Vector2 nearestHero = FindNearestHero(currentPosition);
            Vector2 fromCenter = currentPosition.sqrMagnitude > 0.001f ? currentPosition.normalized : GetEnemySpawnPosition(spawnSequence).normalized;
            Vector2 tangent = new Vector2(-fromCenter.y, fromCenter.x);
            float ring = 0.48f + (index % 3) * 0.16f;
            float sideOffset = ((index % 5) - 2) * 0.18f;
            Vector2 swarmOffset = fromCenter * ring + tangent * sideOffset;
            float phase = spawnSequence * 0.73f + index * 0.41f;
            Vector2 idleMotion = fromCenter * Mathf.Sin(Time.time * 1.8f + phase) * 0.12f
                + tangent * Mathf.Cos(Time.time * 2.1f + phase) * 0.16f;
            return ClampField(nearestHero + swarmOffset + idleMotion, 0.35f);
        }

        private Vector2 FindNearestHero(Vector2 fromPosition)
        {
            Vector2 nearest = Vector2.zero;
            float bestDistance = float.MaxValue;
            foreach (Vector2 heroPosition in heroLocalPositions.Values)
            {
                float distance = (fromPosition - heroPosition).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = heroPosition;
                }
            }

            return bestDistance < float.MaxValue ? nearest : Vector2.zero;
        }

        private Vector2 FindNearestEnemy(Vector2 fromPosition)
        {
            Vector2 nearest = Vector2.zero;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < enemyActors.Count; i++)
            {
                if (!enemyActors[i].Root.activeSelf)
                {
                    continue;
                }

                float distance = (fromPosition - enemyActors[i].LocalPosition).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = enemyActors[i].LocalPosition;
                }
            }

            return bestDistance < float.MaxValue ? nearest : Vector2.zero;
        }

        private bool IsHeroInAttackBatch(string heroId)
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

        private Vector2 GetHeroAnimationOffset(WorldActor actor, Vector2 currentPosition)
        {
            Vector2 offset = Vector2.zero;
            if (actor.AttackPulse > 0f)
            {
                Vector2 toHit = battleManager.LastHitPosition - currentPosition;
                if (toHit.sqrMagnitude > 0.001f)
                {
                    float pulse = Mathf.Sin(GetPulseRatio(actor.AttackPulse, 0.22f) * Mathf.PI);
                    offset += toHit.normalized * (0.22f * pulse);
                }
            }

            if (actor.HitPulse > 0f)
            {
                float pulse = GetPulseRatio(actor.HitPulse, 0.20f);
                offset += new Vector2(Mathf.Sin(Time.time * 80f), Mathf.Cos(Time.time * 65f)) * (0.05f * pulse);
            }

            return offset;
        }

        private Vector2 GetEnemyAnimationOffset(WorldActor actor, Vector2 currentPosition)
        {
            Vector2 offset = Vector2.zero;
            if (actor.AttackPulse > 0f)
            {
                Vector2 toHero = battleManager.LastMonsterHitPosition - currentPosition;
                if (toHero.sqrMagnitude > 0.001f)
                {
                    float pulse = Mathf.Sin(GetPulseRatio(actor.AttackPulse, 0.18f) * Mathf.PI);
                    offset += toHero.normalized * (0.18f * pulse);
                }
            }

            if (actor.HitPulse > 0f)
            {
                float pulse = GetPulseRatio(actor.HitPulse, 0.20f);
                offset += new Vector2(Mathf.Sin(Time.time * 90f), Mathf.Cos(Time.time * 74f)) * (0.06f * pulse);
            }

            return offset;
        }

        private static float GetPulseRatio(float remaining, float duration)
        {
            return Mathf.Clamp01(remaining / Mathf.Max(0.001f, duration));
        }

        private static float EaseOut(float value)
        {
            float t = Mathf.Clamp01(value);
            return 1f - (1f - t) * (1f - t);
        }

        private static Vector3 ToWorld(Vector2 localPosition)
        {
            return new Vector3(OriginX + localPosition.x, localPosition.y, 0f);
        }

        private static Vector2 ClampField(Vector2 position, float margin = 0.15f)
        {
            return new Vector2(
                Mathf.Clamp(position.x, -FieldHalfWidth + margin, FieldHalfWidth - margin),
                Mathf.Clamp(position.y, -FieldHalfHeight + margin, FieldHalfHeight - margin));
        }

        private static Color GetRarityColor(HeroRarity rarity)
        {
            switch (rarity)
            {
                case HeroRarity.Common:
                    return new Color(0.58f, 0.66f, 0.72f, 1f);
                case HeroRarity.Uncommon:
                    return new Color(0.34f, 0.84f, 0.30f, 1f);
                case HeroRarity.Rare:
                    return new Color(0.30f, 0.58f, 1f, 1f);
                case HeroRarity.Epic:
                    return new Color(0.72f, 0.28f, 0.96f, 1f);
                case HeroRarity.Legendary:
                    return new Color(1f, 0.62f, 0.16f, 1f);
                case HeroRarity.Mythic:
                    return new Color(1f, 0.18f, 0.18f, 1f);
                default:
                    return Color.white;
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static float PseudoRandom01(int seed)
        {
            unchecked
            {
                uint value = (uint)(seed * 747796405 + 2891336453);
                value = ((value >> ((int)(value >> 28) + 4)) ^ value) * 277803737;
                value = (value >> 22) ^ value;
                return (value & 0xFFFFFF) / 16777215f;
            }
        }

        private sealed class ProjectileVisual
        {
            public ProjectileVisual(GameObject root, SpriteRenderer body)
            {
                Root = root;
                Body = body;
                Root.SetActive(false);
            }

            public GameObject Root { get; }
            public SpriteRenderer Body { get; }
            public Vector2 StartPosition { get; set; }
            public Vector2 TargetPosition { get; set; }
            public Color BaseColor { get; set; }
            public float Life { get; set; }
            public float Duration { get; set; }
            public float Size { get; set; }
        }

        private sealed class WorldActor
        {
            public WorldActor(
                GameObject root,
                SpriteRenderer body,
                GameObject hpRoot,
                SpriteRenderer hpFill)
            {
                Root = root;
                Body = body;
                HpRoot = hpRoot;
                HpFill = hpFill;
            }

            public GameObject Root { get; }
            public SpriteRenderer Body { get; }
            public GameObject HpRoot { get; }
            public SpriteRenderer HpFill { get; }
            public Vector2 LocalPosition { get; set; }
            public int LastSpawnSequence { get; set; } = -1;
            public float AttackPulse { get; set; }
            public float HitPulse { get; set; }
            public float SpawnPulse { get; set; }
        }

        private sealed class DamageFloater
        {
            public DamageFloater(GameObject root, TextMesh text)
            {
                Root = root;
                Text = text;
                Root.SetActive(false);
            }

            public GameObject Root { get; }
            public TextMesh Text { get; }
            public Vector2 StartPosition { get; set; }
            public Color BaseColor { get; set; }
            public float Life { get; set; }
            public float Duration { get; set; }
        }
    }
}
