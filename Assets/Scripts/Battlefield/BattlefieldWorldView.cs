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
        private const float EnemyDeathBurstSeconds = 0.18f;

        private static Sprite circleSprite;
        private static Sprite squareSprite;
        private static int nextActorPhaseSeed;

        private readonly Dictionary<string, WorldActor> heroActors = new Dictionary<string, WorldActor>();
        private readonly List<WorldActor> enemyActors = new List<WorldActor>();
        private readonly List<ParticleSystem> burstPool = new List<ParticleSystem>();
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
        private SpriteRenderer fortressKeepRenderer;
        private SpriteRenderer fortressHpFillRenderer;
        private TextMesh fortressLabel;
        private SpriteRenderer portalRenderer;
        private SpriteRenderer backgroundRenderer;
        private TextMesh portalText;
        private int observedHitSequence = -1;
        private int observedHeroAttackBatchSequence = -1;
        private int observedMonsterHitSequence = -1;
        private int observedEnemyDefeatSequence = -1;
        private float deathBurstRemaining;
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
            deathBurstRemaining = Mathf.Max(0f, deathBurstRemaining - deltaTime);

            TickActorAnimationState(deltaTime);
            UpdatePortal();
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

            backgroundRenderer = CreateSpriteRenderer("Background", sceneRoot, squareSprite, new Color(0.13f, 0.16f, 0.20f, 1f), -50);
            backgroundRenderer.transform.localScale = new Vector3(FieldHalfWidth * 2.2f, FieldHalfHeight * 2.2f, 1f);

            CreateBackdropDetails(sceneRoot);
            CreateFortressVisual(sceneRoot);

            portalRenderer = CreateSpriteRenderer("SpawnPortal", sceneRoot, circleSprite, new Color(0.18f, 0.62f, 1f, 0.28f), -5);
            portalRenderer.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

            portalText = CreateTextMesh("PortalLabel", sceneRoot, "●", 0.18f, new Color(0.74f, 0.92f, 1f, 0.52f), 15);
            portalText.transform.localPosition = new Vector3(0f, -0.08f, 0f);

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
            if (circleSprite != null && squareSprite != null)
            {
                return;
            }

            circleSprite = CreateCircleSprite();
            squareSprite = CreateSquareSprite();
        }

        private static Sprite CreateCircleSprite()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "RuntimeCircleSprite",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.46f;
            float borderRadius = size * 0.50f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(borderRadius - distance);
                    float inner = Mathf.Clamp01(radius - distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(Mathf.Max(alpha * 0.75f, inner))));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
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

        private void CreateBackdropDetails(Transform parent)
        {
            for (int i = 0; i < 28; i++)
            {
                float x = Mathf.Lerp(-FieldHalfWidth, FieldHalfWidth, PseudoRandom01(i * 17 + 3));
                float y = Mathf.Lerp(-FieldHalfHeight, FieldHalfHeight, PseudoRandom01(i * 23 + 11));
                float scale = Mathf.Lerp(0.04f, 0.12f, PseudoRandom01(i * 31 + 7));
                Color color = i % 3 == 0
                    ? new Color(0.30f, 0.45f, 0.62f, 0.22f)
                    : new Color(0.06f, 0.07f, 0.10f, 0.24f);
                SpriteRenderer detail = CreateSpriteRenderer("BackdropDetail" + i, parent, circleSprite, color, -45);
                detail.transform.localPosition = new Vector3(x, y, 0f);
                detail.transform.localScale = new Vector3(scale, scale * 0.55f, 1f);
            }
        }

        private void CreateFortressVisual(Transform parent)
        {
            fortressRoot = new GameObject("Fortress").transform;
            fortressRoot.SetParent(parent, false);
            fortressRoot.localPosition = Vector3.zero;

            SpriteRenderer shadow = CreateSpriteRenderer("FortressShadow", fortressRoot, circleSprite, new Color(0f, 0f, 0f, 0.26f), 0);
            shadow.transform.localPosition = new Vector3(0f, -0.62f, 0f);
            shadow.transform.localScale = new Vector3(1.92f, 0.46f, 1f);

            fortressBaseRenderer = CreateSpriteRenderer("FortressBase", fortressRoot, squareSprite, new Color(0.25f, 0.30f, 0.38f, 1f), 1);
            fortressBaseRenderer.transform.localPosition = new Vector3(0f, -0.20f, 0f);
            fortressBaseRenderer.transform.localScale = new Vector3(1.42f, 0.82f, 1f);

            SpriteRenderer leftTower = CreateSpriteRenderer("FortressLeftTower", fortressRoot, squareSprite, new Color(0.30f, 0.36f, 0.45f, 1f), 2);
            leftTower.transform.localPosition = new Vector3(-0.72f, 0.06f, 0f);
            leftTower.transform.localScale = new Vector3(0.34f, 1.18f, 1f);

            SpriteRenderer rightTower = CreateSpriteRenderer("FortressRightTower", fortressRoot, squareSprite, new Color(0.30f, 0.36f, 0.45f, 1f), 2);
            rightTower.transform.localPosition = new Vector3(0.72f, 0.06f, 0f);
            rightTower.transform.localScale = new Vector3(0.34f, 1.18f, 1f);

            fortressKeepRenderer = CreateSpriteRenderer("FortressKeep", fortressRoot, squareSprite, new Color(0.38f, 0.48f, 0.62f, 1f), 3);
            fortressKeepRenderer.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            fortressKeepRenderer.transform.localScale = new Vector3(0.68f, 1.22f, 1f);

            SpriteRenderer core = CreateSpriteRenderer("FortressCore", fortressRoot, circleSprite, new Color(0.36f, 0.76f, 1f, 0.82f), 4);
            core.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            core.transform.localScale = new Vector3(0.32f, 0.32f, 1f);

            SpriteRenderer hpBack = CreateSpriteRenderer("FortressHpBack", fortressRoot, squareSprite, new Color(0.02f, 0.025f, 0.03f, 0.95f), 5);
            hpBack.transform.localPosition = new Vector3(0f, 0.98f, 0f);
            hpBack.transform.localScale = new Vector3(1.10f, 0.08f, 1f);

            fortressHpFillRenderer = CreateSpriteRenderer("FortressHpFill", fortressRoot, squareSprite, new Color(0.40f, 0.95f, 0.72f, 1f), 6);
            fortressHpFillRenderer.transform.localPosition = new Vector3(0f, 0.98f, 0f);
            fortressHpFillRenderer.transform.localScale = new Vector3(1.04f, 0.052f, 1f);

            fortressLabel = CreateTextMesh("FortressLabel", fortressRoot, string.Empty, 0.09f, Color.white, 7);
            fortressLabel.transform.localPosition = new Vector3(0f, 1.18f, 0f);
        }

        private void UpdatePortal()
        {
            UpdateFortressVisual();

            float pulse = 1f + Mathf.Sin(Time.time * 3.8f) * 0.08f;
            if (portalRenderer != null)
            {
                Color color = battleManager.IsBossFight
                    ? new Color(1f, 0.48f, 0.16f, 0.34f)
                    : new Color(0.18f, 0.62f, 1f, 0.24f + Mathf.Sin(Time.time * 2.4f) * 0.08f);
                portalRenderer.color = color;
                portalRenderer.transform.localScale = new Vector3(1.18f * pulse, 1.18f * pulse, 1f);
            }

            if (portalText != null)
            {
                portalText.text = string.Empty;
            }
        }

        private void UpdateFortressVisual()
        {
            if (fortressRoot == null || battleManager == null)
            {
                return;
            }

            float hpRatio = battleManager.FortressHpRatio;
            bool alive = hpRatio > 0f;
            float pulse = 1f + Mathf.Sin(Time.time * 1.8f) * (alive ? 0.025f : 0.008f);
            fortressRoot.localScale = new Vector3(pulse, pulse, 1f);

            Color healthyBase = new Color(0.25f, 0.30f, 0.38f, 1f);
            Color damagedBase = new Color(0.22f, 0.13f, 0.12f, 1f);
            if (fortressBaseRenderer != null)
            {
                fortressBaseRenderer.color = Color.Lerp(damagedBase, healthyBase, hpRatio);
            }

            if (fortressKeepRenderer != null)
            {
                fortressKeepRenderer.color = Color.Lerp(new Color(0.28f, 0.17f, 0.16f, 1f), new Color(0.38f, 0.48f, 0.62f, 1f), hpRatio);
            }

            if (fortressHpFillRenderer != null)
            {
                fortressHpFillRenderer.color = alive ? new Color(0.40f, 0.95f, 0.72f, 1f) : new Color(0.60f, 0.12f, 0.10f, 1f);
                fortressHpFillRenderer.transform.localScale = new Vector3(1.04f * Mathf.Clamp01(hpRatio), 0.052f, 1f);
                fortressHpFillRenderer.transform.localPosition = new Vector3(-0.52f * (1f - Mathf.Clamp01(hpRatio)), 0.98f, 0f);
            }

            if (fortressLabel != null)
            {
                fortressLabel.text = "요새 Lv." + battleManager.FortressLevel;
                fortressLabel.color = alive ? Color.white : new Color(1f, 0.42f, 0.32f, 1f);
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
            actor.Body.color = hit
                ? Color.Lerp(WithAlpha(GetRarityColor(hero.Definition.Rarity), alive ? 1f : 0.42f), Color.white, 0.72f)
                : isAttacking
                ? Color.Lerp(GetRarityColor(hero.Definition.Rarity), new Color(1f, 0.92f, 0.25f, alive ? 1f : 0.45f), 0.62f)
                : WithAlpha(GetRarityColor(hero.Definition.Rarity), alive ? 1f : 0.42f);
                ConfigureHeroActorVisuals(actor, hero.Definition, alive, hit);
                AnimateHeroActorParts(actor, hero.Definition, Vector2.Distance(currentPosition, targetPosition) > 0.035f, isAttacking, alive);
                actor.Label.text = string.Empty;
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
                        SpawnBurst(actor.LocalPosition, new Color(1f, 0.38f, 0.18f, 1f), 1.25f);
                        deathBurstRemaining = EnemyDeathBurstSeconds;
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
                Color enemyColor = battleManager.IsBossFight
                    ? new Color(0.86f, 0.18f, 0.14f, 1f)
                    : Color.Lerp(new Color(0.58f, 0.12f, 0.10f, 1f), new Color(0.95f, 0.38f, 0.12f, 1f), i / (float)Mathf.Max(1, GameData.MaxVisibleEnemies - 1));
                actor.Body.color = actor.HitPulse > 0f ? Color.Lerp(enemyColor, Color.white, 0.82f) : enemyColor;
                ConfigureEnemyActorVisuals(actor, i, battleManager.IsBossFight, actor.HitPulse > 0f);
                AnimateEnemyActorParts(actor, Vector2.Distance(actor.LocalPosition, desiredPosition) > 0.035f, actor.AttackPulse > 0f, battleManager.IsBossFight);
                actor.Label.text = string.Empty;
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

                    SpawnBurst(hitPosition, battleManager.LastHitWasCritical ? new Color(1f, 0.92f, 0.18f, 1f) : new Color(1f, 0.42f, 0.20f, 1f), battleManager.LastHitWasCritical ? 1.65f : 1.15f);
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

                SpawnBurst(battleManager.LastMonsterHitPosition, new Color(0.92f, 0.18f, 0.18f, 1f), 0.7f);
                SpawnDamageFloater(battleManager.LastMonsterHitPosition, "HIT", new Color(0.95f, 0.20f, 0.18f, 1f), 0.72f);
            }

            if (battleManager.EnemyDefeatSequence != observedEnemyDefeatSequence)
            {
                observedEnemyDefeatSequence = battleManager.EnemyDefeatSequence;
                SpawnBurst(battleManager.LastDefeatedEnemyPosition, new Color(1f, 0.65f, 0.18f, 1f), 1.55f);
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

                        SpawnBurst(position, new Color(0.72f, 0.90f, 1f, 1f), 0.68f);
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

            Transform back = instance.transform.Find("Back");
            Transform body = instance.transform.Find("Body");
            Transform head = instance.transform.Find("Head");
            Transform headwear = instance.transform.Find("Headwear");
            Transform accent = instance.transform.Find("Accent");
            Transform armLeft = instance.transform.Find("ArmLeft");
            Transform armRight = instance.transform.Find("ArmRight");
            Transform footLeft = instance.transform.Find("FootLeft");
            Transform footRight = instance.transform.Find("FootRight");
            Transform weapon = instance.transform.Find("Weapon");
            Transform offhand = instance.transform.Find("Offhand");
            Transform eyeLeft = instance.transform.Find("EyeLeft");
            Transform eyeRight = instance.transform.Find("EyeRight");
            Transform label = instance.transform.Find("Label");
            Transform hpRoot = instance.transform.Find("HpRoot");
            Transform hpFill = hpRoot != null ? hpRoot.Find("HpFill") : null;

            return new WorldActor(
                instance,
                back.GetComponent<SpriteRenderer>(),
                body.GetComponent<SpriteRenderer>(),
                head.GetComponent<SpriteRenderer>(),
                headwear.GetComponent<SpriteRenderer>(),
                accent.GetComponent<SpriteRenderer>(),
                armLeft.GetComponent<SpriteRenderer>(),
                armRight.GetComponent<SpriteRenderer>(),
                footLeft.GetComponent<SpriteRenderer>(),
                footRight.GetComponent<SpriteRenderer>(),
                weapon.GetComponent<SpriteRenderer>(),
                offhand.GetComponent<SpriteRenderer>(),
                eyeLeft.GetComponent<SpriteRenderer>(),
                eyeRight.GetComponent<SpriteRenderer>(),
                label.GetComponent<TextMesh>(),
                hpRoot.gameObject,
                hpFill.GetComponent<SpriteRenderer>());
        }

        private GameObject CreateActorTemplate(string name, bool hero)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(templateRoot, false);
            root.SetActive(false);

            SpriteRenderer shadow = CreateSpriteRenderer("Shadow", root.transform, circleSprite, new Color(0f, 0f, 0f, 0.28f), hero ? 0 : 10);
            shadow.transform.localPosition = new Vector3(0f, -0.38f, 0f);
            shadow.transform.localScale = new Vector3(0.78f, 0.25f, 1f);

            SpriteRenderer back = CreateSpriteRenderer("Back", root.transform, hero ? circleSprite : squareSprite, hero ? new Color(0.08f, 0.10f, 0.16f, 0.92f) : new Color(0.20f, 0.06f, 0.04f, 1f), hero ? 2 : 12);
            back.transform.localPosition = hero ? new Vector3(0f, -0.06f, 0f) : new Vector3(0f, 0.34f, 0f);
            back.transform.localScale = hero ? new Vector3(0.82f, 0.70f, 1f) : new Vector3(0.62f, 0.18f, 1f);
            back.transform.localRotation = Quaternion.Euler(0f, 0f, hero ? 0f : 0f);

            SpriteRenderer body = CreateSpriteRenderer("Body", root.transform, circleSprite, hero ? new Color(0.36f, 0.56f, 0.96f, 1f) : new Color(0.70f, 0.13f, 0.10f, 1f), hero ? 4 : 14);
            body.transform.localPosition = hero ? new Vector3(0f, -0.16f, 0f) : new Vector3(0f, -0.04f, 0f);
            body.transform.localScale = hero ? new Vector3(0.66f, 0.72f, 1f) : new Vector3(0.74f, 0.66f, 1f);

            SpriteRenderer head = CreateSpriteRenderer("Head", root.transform, circleSprite, hero ? new Color(0.96f, 0.82f, 0.62f, 1f) : new Color(0.28f, 0.05f, 0.05f, 1f), hero ? 6 : 16);
            head.transform.localPosition = hero ? new Vector3(0f, 0.28f, 0f) : new Vector3(0f, 0.16f, 0f);
            head.transform.localScale = hero ? new Vector3(0.54f, 0.48f, 1f) : new Vector3(0.52f, 0.42f, 1f);

            SpriteRenderer headwear = CreateSpriteRenderer("Headwear", root.transform, hero ? squareSprite : circleSprite, hero ? new Color(0.12f, 0.16f, 0.22f, 1f) : new Color(0.12f, 0.02f, 0.02f, 1f), hero ? 9 : 19);
            headwear.transform.localPosition = hero ? new Vector3(0f, 0.52f, 0f) : new Vector3(0f, 0.37f, 0f);
            headwear.transform.localScale = hero ? new Vector3(0.46f, 0.12f, 1f) : new Vector3(0.26f, 0.12f, 1f);

            SpriteRenderer accent = CreateSpriteRenderer("Accent", root.transform, squareSprite, Color.white, hero ? 5 : 15);
            accent.transform.localPosition = hero ? new Vector3(0f, -0.18f, 0f) : new Vector3(0f, -0.12f, 0f);
            accent.transform.localScale = hero ? new Vector3(0.48f, 0.14f, 1f) : new Vector3(0.36f, 0.12f, 1f);
            accent.transform.localRotation = Quaternion.identity;

            SpriteRenderer armLeft = CreateSpriteRenderer("ArmLeft", root.transform, circleSprite, hero ? new Color(0.96f, 0.82f, 0.62f, 1f) : new Color(0.45f, 0.08f, 0.06f, 1f), hero ? 5 : 15);
            armLeft.transform.localPosition = hero ? new Vector3(-0.35f, -0.11f, 0f) : new Vector3(-0.34f, -0.02f, 0f);
            armLeft.transform.localScale = hero ? new Vector3(0.18f, 0.30f, 1f) : new Vector3(0.20f, 0.28f, 1f);

            SpriteRenderer armRight = CreateSpriteRenderer("ArmRight", root.transform, circleSprite, hero ? new Color(0.96f, 0.82f, 0.62f, 1f) : new Color(0.45f, 0.08f, 0.06f, 1f), hero ? 5 : 15);
            armRight.transform.localPosition = hero ? new Vector3(0.35f, -0.11f, 0f) : new Vector3(0.34f, -0.02f, 0f);
            armRight.transform.localScale = hero ? new Vector3(0.18f, 0.30f, 1f) : new Vector3(0.20f, 0.28f, 1f);

            SpriteRenderer footLeft = CreateSpriteRenderer("FootLeft", root.transform, circleSprite, hero ? new Color(0.08f, 0.10f, 0.14f, 1f) : new Color(0.18f, 0.03f, 0.025f, 1f), hero ? 3 : 13);
            footLeft.transform.localPosition = hero ? new Vector3(-0.18f, -0.52f, 0f) : new Vector3(-0.18f, -0.38f, 0f);
            footLeft.transform.localScale = hero ? new Vector3(0.18f, 0.12f, 1f) : new Vector3(0.20f, 0.12f, 1f);

            SpriteRenderer footRight = CreateSpriteRenderer("FootRight", root.transform, circleSprite, hero ? new Color(0.08f, 0.10f, 0.14f, 1f) : new Color(0.18f, 0.03f, 0.025f, 1f), hero ? 3 : 13);
            footRight.transform.localPosition = hero ? new Vector3(0.18f, -0.52f, 0f) : new Vector3(0.18f, -0.38f, 0f);
            footRight.transform.localScale = hero ? new Vector3(0.18f, 0.12f, 1f) : new Vector3(0.20f, 0.12f, 1f);

            SpriteRenderer weapon = CreateSpriteRenderer("Weapon", root.transform, squareSprite, hero ? new Color(0.96f, 0.98f, 1f, 1f) : new Color(0.12f, 0.03f, 0.03f, 1f), hero ? 7 : 17);
            weapon.transform.localPosition = hero ? new Vector3(0.43f, 0f, 0f) : new Vector3(-0.30f, 0.18f, 0f);
            weapon.transform.localScale = hero ? new Vector3(0.13f, 0.74f, 1f) : new Vector3(0.15f, 0.34f, 1f);
            weapon.transform.localRotation = Quaternion.Euler(0f, 0f, hero ? -34f : 42f);

            SpriteRenderer offhand = CreateSpriteRenderer("Offhand", root.transform, hero ? circleSprite : squareSprite, hero ? new Color(0.22f, 0.28f, 0.38f, 1f) : new Color(0.12f, 0.03f, 0.03f, 1f), hero ? 6 : 17);
            offhand.transform.localPosition = hero ? new Vector3(-0.35f, -0.04f, 0f) : new Vector3(0.30f, 0.18f, 0f);
            offhand.transform.localScale = hero ? new Vector3(0.30f, 0.36f, 1f) : new Vector3(0.15f, 0.34f, 1f);
            offhand.transform.localRotation = Quaternion.Euler(0f, 0f, hero ? 0f : -42f);

            SpriteRenderer eyeLeft = CreateSpriteRenderer("EyeLeft", root.transform, squareSprite, hero ? new Color(0.05f, 0.06f, 0.08f, 1f) : new Color(1f, 0.95f, 0.50f, 1f), hero ? 8 : 18);
            eyeLeft.transform.localPosition = hero ? new Vector3(-0.085f, 0.31f, 0f) : new Vector3(-0.085f, 0.19f, 0f);
            eyeLeft.transform.localScale = hero ? new Vector3(0.040f, 0.060f, 1f) : new Vector3(0.052f, 0.045f, 1f);

            SpriteRenderer eyeRight = CreateSpriteRenderer("EyeRight", root.transform, squareSprite, hero ? new Color(0.05f, 0.06f, 0.08f, 1f) : new Color(1f, 0.95f, 0.50f, 1f), hero ? 8 : 18);
            eyeRight.transform.localPosition = hero ? new Vector3(0.085f, 0.31f, 0f) : new Vector3(0.085f, 0.19f, 0f);
            eyeRight.transform.localScale = hero ? new Vector3(0.040f, 0.060f, 1f) : new Vector3(0.052f, 0.045f, 1f);

            TextMesh label = CreateTextMesh("Label", root.transform, string.Empty, 0.08f, Color.white, hero ? 8 : 18);
            label.transform.localPosition = hero ? new Vector3(0f, -0.06f, 0f) : new Vector3(0f, -0.02f, 0f);

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
            Color traitColor = GetTraitColor(hero.Trait);
            float alpha = alive ? 1f : 0.42f;

            actor.Back.color = WithAlpha(Color.Lerp(rarityColor, new Color(0.04f, 0.06f, 0.10f, 1f), 0.45f), alpha * 0.94f);
            actor.Head.color = hit
                ? Color.white
                : WithAlpha(new Color(0.96f, 0.82f, 0.62f, 1f), alpha);
            actor.Headwear.color = WithAlpha(Color.Lerp(rarityColor, new Color(0.08f, 0.10f, 0.14f, 1f), 0.38f), alpha);
            actor.Accent.color = WithAlpha(traitColor, alpha);
            actor.ArmLeft.color = WithAlpha(new Color(0.96f, 0.82f, 0.62f, 1f), alpha);
            actor.ArmRight.color = actor.ArmLeft.color;
            actor.FootLeft.color = WithAlpha(new Color(0.08f, 0.10f, 0.14f, 1f), alpha);
            actor.FootRight.color = actor.FootLeft.color;
            actor.Weapon.color = WithAlpha(GetHeroWeaponColor(hero.Trait, hero.Rarity), alpha);
            actor.Offhand.color = WithAlpha(GetHeroOffhandColor(hero.Trait), alpha);
            actor.EyeLeft.color = WithAlpha(new Color(0.05f, 0.055f, 0.07f, 1f), alpha);
            actor.EyeRight.color = actor.EyeLeft.color;

            SetRendererPart(actor.Back, new Vector3(0f, -0.06f, 0f), new Vector3(0.82f, 0.70f, 1f), 0f);
            SetRendererPart(actor.Body, new Vector3(0f, -0.16f, 0f), new Vector3(0.66f, 0.72f, 1f), 0f);
            SetRendererPart(actor.Head, new Vector3(0f, 0.28f, 0f), new Vector3(0.54f, 0.48f, 1f), 0f);
            SetRendererPart(actor.Headwear, new Vector3(0f, 0.52f, 0f), new Vector3(0.46f, 0.12f, 1f), 0f);
            SetRendererPart(actor.EyeLeft, new Vector3(-0.085f, 0.31f, 0f), new Vector3(0.040f, 0.060f, 1f), 0f);
            SetRendererPart(actor.EyeRight, new Vector3(0.085f, 0.31f, 0f), new Vector3(0.040f, 0.060f, 1f), 0f);
            SetRendererPart(actor.ArmLeft, new Vector3(-0.35f, -0.11f, 0f), new Vector3(0.18f, 0.30f, 1f), -8f);
            SetRendererPart(actor.ArmRight, new Vector3(0.35f, -0.11f, 0f), new Vector3(0.18f, 0.30f, 1f), 8f);
            SetRendererPart(actor.FootLeft, new Vector3(-0.18f, -0.52f, 0f), new Vector3(0.18f, 0.12f, 1f), 0f);
            SetRendererPart(actor.FootRight, new Vector3(0.18f, -0.52f, 0f), new Vector3(0.18f, 0.12f, 1f), 0f);

            switch (hero.Trait)
            {
                case HeroTrait.Melee:
                    SetRendererPart(actor.Weapon, new Vector3(0.43f, 0.00f, 0f), new Vector3(0.12f, 0.78f, 1f), -35f);
                    SetRendererPart(actor.Offhand, new Vector3(-0.34f, -0.05f, 0f), new Vector3(0.24f, 0.30f, 1f), 0f);
                    SetRendererPart(actor.Accent, new Vector3(0.00f, -0.18f, 0f), new Vector3(0.48f, 0.13f, 1f), 0f);
                    break;
                case HeroTrait.Ranged:
                    SetRendererPart(actor.Headwear, new Vector3(0.02f, 0.50f, 0f), new Vector3(0.56f, 0.10f, 1f), -8f);
                    SetRendererPart(actor.Weapon, new Vector3(0.43f, 0.03f, 0f), new Vector3(0.10f, 0.84f, 1f), 18f);
                    SetRendererPart(actor.Offhand, new Vector3(0.33f, 0.03f, 0f), new Vector3(0.055f, 0.80f, 1f), 18f);
                    SetRendererPart(actor.Accent, new Vector3(-0.12f, -0.19f, 0f), new Vector3(0.56f, 0.11f, 1f), 0f);
                    break;
                case HeroTrait.Support:
                    SetRendererPart(actor.Headwear, new Vector3(0f, 0.53f, 0f), new Vector3(0.28f, 0.20f, 1f), 0f);
                    SetRendererPart(actor.Weapon, new Vector3(0.42f, -0.02f, 0f), new Vector3(0.09f, 0.84f, 1f), -12f);
                    SetRendererPart(actor.Offhand, new Vector3(0.35f, 0.40f, 0f), new Vector3(0.22f, 0.22f, 1f), 0f);
                    SetRendererPart(actor.Accent, new Vector3(0.00f, -0.18f, 0f), new Vector3(0.40f, 0.12f, 1f), 0f);
                    break;
                case HeroTrait.Defense:
                    SetRendererPart(actor.Headwear, new Vector3(0f, 0.50f, 0f), new Vector3(0.50f, 0.15f, 1f), 0f);
                    SetRendererPart(actor.Weapon, new Vector3(0.42f, 0.00f, 0f), new Vector3(0.11f, 0.58f, 1f), -25f);
                    SetRendererPart(actor.Offhand, new Vector3(-0.36f, -0.06f, 0f), new Vector3(0.40f, 0.48f, 1f), 0f);
                    SetRendererPart(actor.Accent, new Vector3(0.00f, -0.16f, 0f), new Vector3(0.54f, 0.16f, 1f), 0f);
                    break;
            }
        }

        private void ConfigureEnemyActorVisuals(WorldActor actor, int index, bool boss, bool hit)
        {
            Color baseColor = boss
                ? new Color(0.86f, 0.18f, 0.14f, 1f)
                : Color.Lerp(new Color(0.58f, 0.12f, 0.10f, 1f), new Color(0.95f, 0.38f, 0.12f, 1f), index / (float)Mathf.Max(1, GameData.MaxVisibleEnemies - 1));
            Color dark = Color.Lerp(baseColor, Color.black, 0.55f);

            actor.Back.color = boss ? new Color(0.30f, 0.02f, 0.02f, 1f) : dark;
            actor.Head.color = hit ? Color.white : Color.Lerp(baseColor, Color.black, 0.18f);
            actor.Headwear.color = boss ? new Color(0.12f, 0.01f, 0.01f, 1f) : dark;
            actor.Accent.color = boss ? new Color(1f, 0.72f, 0.16f, 1f) : new Color(0.12f, 0.03f, 0.025f, 1f);
            actor.ArmLeft.color = baseColor;
            actor.ArmRight.color = baseColor;
            actor.FootLeft.color = dark;
            actor.FootRight.color = dark;
            actor.Weapon.color = dark;
            actor.Offhand.color = dark;
            actor.EyeLeft.color = boss ? new Color(1f, 0.95f, 0.35f, 1f) : new Color(1f, 0.82f, 0.28f, 1f);
            actor.EyeRight.color = actor.EyeLeft.color;

            float hornTilt = boss ? 52f : 42f;
            SetRendererPart(actor.Body, new Vector3(0f, -0.04f, 0f), boss ? new Vector3(0.82f, 0.74f, 1f) : new Vector3(0.74f, 0.66f, 1f), 0f);
            SetRendererPart(actor.Head, new Vector3(0f, 0.16f, 0f), boss ? new Vector3(0.60f, 0.48f, 1f) : new Vector3(0.52f, 0.42f, 1f), 0f);
            SetRendererPart(actor.Headwear, new Vector3(0f, 0.36f, 0f), boss ? new Vector3(0.32f, 0.14f, 1f) : new Vector3(0.24f, 0.10f, 1f), 0f);
            SetRendererPart(actor.EyeLeft, boss ? new Vector3(-0.10f, 0.20f, 0f) : new Vector3(-0.085f, 0.19f, 0f), boss ? new Vector3(0.065f, 0.052f, 1f) : new Vector3(0.052f, 0.045f, 1f), 0f);
            SetRendererPart(actor.EyeRight, boss ? new Vector3(0.10f, 0.20f, 0f) : new Vector3(0.085f, 0.19f, 0f), boss ? new Vector3(0.065f, 0.052f, 1f) : new Vector3(0.052f, 0.045f, 1f), 0f);
            SetRendererPart(actor.ArmLeft, new Vector3(-0.38f, -0.04f, 0f), boss ? new Vector3(0.24f, 0.36f, 1f) : new Vector3(0.20f, 0.28f, 1f), 18f);
            SetRendererPart(actor.ArmRight, new Vector3(0.38f, -0.04f, 0f), boss ? new Vector3(0.24f, 0.36f, 1f) : new Vector3(0.20f, 0.28f, 1f), -18f);
            SetRendererPart(actor.FootLeft, new Vector3(-0.18f, -0.38f, 0f), boss ? new Vector3(0.23f, 0.14f, 1f) : new Vector3(0.20f, 0.12f, 1f), 0f);
            SetRendererPart(actor.FootRight, new Vector3(0.18f, -0.38f, 0f), boss ? new Vector3(0.23f, 0.14f, 1f) : new Vector3(0.20f, 0.12f, 1f), 0f);
            SetRendererPart(actor.Back, new Vector3(0f, 0.36f, 0f), boss ? new Vector3(0.78f, 0.20f, 1f) : new Vector3(0.62f, 0.16f, 1f), 0f);
            SetRendererPart(actor.Weapon, new Vector3(-0.33f, 0.25f, 0f), boss ? new Vector3(0.18f, 0.52f, 1f) : new Vector3(0.13f, 0.35f, 1f), hornTilt);
            SetRendererPart(actor.Offhand, new Vector3(0.33f, 0.25f, 0f), boss ? new Vector3(0.18f, 0.52f, 1f) : new Vector3(0.13f, 0.35f, 1f), -hornTilt);
            SetRendererPart(actor.Accent, new Vector3(0f, -0.15f, 0f), boss ? new Vector3(0.44f, 0.15f, 1f) : new Vector3(0.36f, 0.10f, 1f), 0f);
        }

        private static void SetRendererPart(SpriteRenderer renderer, Vector3 localPosition, Vector3 localScale, float zRotation)
        {
            renderer.transform.localPosition = localPosition;
            renderer.transform.localScale = localScale;
            renderer.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        }

        private void AnimateHeroActorParts(WorldActor actor, HeroDefinition hero, bool moving, bool attacking, bool alive)
        {
            if (!alive)
            {
                actor.Body.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
                actor.Head.transform.localPosition += new Vector3(0.05f, -0.08f, 0f);
                actor.Headwear.transform.localPosition += new Vector3(0.05f, -0.08f, 0f);
                actor.EyeLeft.transform.localPosition += new Vector3(0.05f, -0.08f, 0f);
                actor.EyeRight.transform.localPosition += new Vector3(0.05f, -0.08f, 0f);
                return;
            }

            float phase = GetActorPhase(actor);
            float walk = moving ? Mathf.Sin(Time.time * 12.5f + phase) : Mathf.Sin(Time.time * 3.2f + phase) * 0.18f;
            float bob = moving ? Mathf.Abs(walk) * 0.045f : Mathf.Sin(Time.time * 2.0f + phase) * 0.018f;
            float attack = attacking ? Mathf.Sin(GetPulseRatio(actor.AttackPulse, 0.22f) * Mathf.PI) : 0f;

            actor.Body.transform.localPosition += new Vector3(0f, bob, 0f);
            actor.Head.transform.localPosition += new Vector3(0f, bob * 0.70f, 0f);
            actor.Headwear.transform.localPosition += new Vector3(0f, bob * 0.70f, 0f);
            actor.EyeLeft.transform.localPosition += new Vector3(0f, bob * 0.70f, 0f);
            actor.EyeRight.transform.localPosition += new Vector3(0f, bob * 0.70f, 0f);

            actor.FootLeft.transform.localPosition += new Vector3(walk * 0.08f, -Mathf.Abs(walk) * 0.035f, 0f);
            actor.FootRight.transform.localPosition += new Vector3(-walk * 0.08f, -Mathf.Abs(walk) * 0.035f, 0f);
            actor.ArmLeft.transform.localRotation = Quaternion.Euler(0f, 0f, -12f - walk * 18f);
            actor.ArmRight.transform.localRotation = Quaternion.Euler(0f, 0f, 12f + walk * 18f);

            switch (hero.Trait)
            {
                case HeroTrait.Melee:
                    actor.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, -35f - attack * 58f);
                    actor.Weapon.transform.localPosition += new Vector3(attack * 0.14f, attack * 0.06f, 0f);
                    actor.ArmRight.transform.localRotation = Quaternion.Euler(0f, 0f, 20f - attack * 42f);
                    break;
                case HeroTrait.Ranged:
                    actor.Weapon.transform.localScale += new Vector3(attack * 0.04f, attack * 0.07f, 0f);
                    actor.Offhand.transform.localPosition += new Vector3(-attack * 0.10f, 0f, 0f);
                    actor.ArmRight.transform.localRotation = Quaternion.Euler(0f, 0f, 22f + attack * 22f);
                    break;
                case HeroTrait.Support:
                    actor.Offhand.transform.localScale += new Vector3(attack * 0.10f, attack * 0.10f, 0f);
                    actor.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, -12f + attack * 18f);
                    break;
                case HeroTrait.Defense:
                    actor.Offhand.transform.localPosition += new Vector3(-attack * 0.10f, attack * 0.03f, 0f);
                    actor.Weapon.transform.localRotation = Quaternion.Euler(0f, 0f, -25f - attack * 26f);
                    break;
            }
        }

        private void AnimateEnemyActorParts(WorldActor actor, bool moving, bool attacking, bool boss)
        {
            float phase = GetActorPhase(actor);
            float walk = moving ? Mathf.Sin(Time.time * (boss ? 8.0f : 10.5f) + phase) : Mathf.Sin(Time.time * 2.8f + phase) * 0.18f;
            float bob = moving ? Mathf.Abs(walk) * 0.035f : Mathf.Sin(Time.time * 2.2f + phase) * 0.018f;
            float attack = attacking ? Mathf.Sin(GetPulseRatio(actor.AttackPulse, 0.18f) * Mathf.PI) : 0f;

            actor.Body.transform.localPosition += new Vector3(0f, bob, 0f);
            actor.Head.transform.localPosition += new Vector3(0f, bob * 0.60f, 0f);
            actor.Headwear.transform.localPosition += new Vector3(0f, bob * 0.60f, 0f);
            actor.EyeLeft.transform.localPosition += new Vector3(0f, bob * 0.60f, 0f);
            actor.EyeRight.transform.localPosition += new Vector3(0f, bob * 0.60f, 0f);
            actor.FootLeft.transform.localPosition += new Vector3(walk * 0.07f, -Mathf.Abs(walk) * 0.025f, 0f);
            actor.FootRight.transform.localPosition += new Vector3(-walk * 0.07f, -Mathf.Abs(walk) * 0.025f, 0f);
            actor.ArmLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 18f + walk * 16f - attack * 24f);
            actor.ArmRight.transform.localRotation = Quaternion.Euler(0f, 0f, -18f - walk * 16f + attack * 24f);
            actor.Head.transform.localScale += new Vector3(attack * 0.05f, attack * 0.03f, 0f);
        }

        private static float GetActorPhase(WorldActor actor)
        {
            return actor.AnimationPhase;
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

        private void SpawnBurst(Vector2 localPosition, Color color, float scale)
        {
            ParticleSystem burst = GetBurstParticle();
            burst.transform.position = ToWorld(localPosition);

            ParticleSystem.MainModule main = burst.main;
            main.startColor = color;
            main.startSize = 0.09f * scale;
            main.startLifetime = 0.32f;
            main.startSpeed = 1.8f * scale;

            ParticleSystem.EmissionModule emission = burst.emission;
            emission.enabled = false;

            burst.Emit(Mathf.RoundToInt(11f * scale));
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
                    SpawnProjectile(startPosition + new Vector2(0.18f, 0.12f), targetPosition, new Color(1f, 0.86f, 0.34f, 1f), 0.24f, 0.16f, ProjectileKind.Arrow);
                    break;
                case HeroTrait.Support:
                    SpawnProjectile(startPosition + new Vector2(0.10f, 0.22f), targetPosition, Color.Lerp(GetRarityColor(hero.Rarity), Color.white, 0.18f), 0.30f, 0.24f, ProjectileKind.Orb);
                    break;
                case HeroTrait.Defense:
                    SpawnProjectile(targetPosition, targetPosition + Vector2.up * 0.01f, new Color(0.66f, 0.86f, 1f, 1f), 0.20f, 0.55f, ProjectileKind.Shock);
                    break;
                default:
                    SpawnProjectile(targetPosition, targetPosition + Vector2.right * 0.01f, new Color(1f, 0.95f, 0.74f, 1f), 0.16f, 0.48f, ProjectileKind.Slash);
                    break;
            }
        }

        private void SpawnProjectile(Vector2 startPosition, Vector2 targetPosition, Color color, float duration, float size, ProjectileKind kind)
        {
            ProjectileVisual projectile = GetProjectile();
            projectile.Root.SetActive(true);
            projectile.StartPosition = startPosition;
            projectile.TargetPosition = targetPosition;
            projectile.Duration = Mathf.Max(0.05f, duration);
            projectile.Life = projectile.Duration;
            projectile.BaseColor = color;
            projectile.Size = size;
            projectile.Kind = kind;
            projectile.Body.color = color;
            projectile.Trail.color = WithAlpha(color, 0.36f);
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
            SpriteRenderer trail = CreateSpriteRenderer("Trail", root.transform, squareSprite, new Color(1f, 1f, 1f, 0.3f), 54);
            SpriteRenderer body = CreateSpriteRenderer("Body", root.transform, squareSprite, Color.white, 55);
            SpriteRenderer head = CreateSpriteRenderer("Head", root.transform, squareSprite, Color.white, 56);
            var projectile = new ProjectileVisual(root, body, trail, head);
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
                projectile.Trail.color = WithAlpha(projectile.BaseColor, 0.36f * (1f - progress));
                projectile.Head.color = WithAlpha(Color.white, color.a);

                switch (projectile.Kind)
                {
                    case ProjectileKind.Arrow:
                        projectile.Head.gameObject.SetActive(true);
                        projectile.Body.sprite = squareSprite;
                        projectile.Trail.sprite = squareSprite;
                        projectile.Head.sprite = squareSprite;
                        projectile.Body.transform.localPosition = Vector3.zero;
                        projectile.Body.transform.localScale = new Vector3(projectile.Size * 1.35f, projectile.Size * 0.18f, 1f);
                        projectile.Body.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                        projectile.Head.transform.localPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * projectile.Size * 0.76f, Mathf.Sin(angle * Mathf.Deg2Rad) * projectile.Size * 0.76f, 0f);
                        projectile.Head.transform.localScale = new Vector3(projectile.Size * 0.34f, projectile.Size * 0.34f, 1f);
                        projectile.Head.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 45f);
                        projectile.Trail.transform.localPosition = new Vector3(-Mathf.Cos(angle * Mathf.Deg2Rad) * projectile.Size * 0.82f, -Mathf.Sin(angle * Mathf.Deg2Rad) * projectile.Size * 0.82f, 0f);
                        projectile.Trail.transform.localScale = new Vector3(projectile.Size * 1.05f, projectile.Size * 0.08f, 1f);
                        projectile.Trail.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                        break;
                    case ProjectileKind.Orb:
                        projectile.Head.gameObject.SetActive(true);
                        projectile.Body.sprite = circleSprite;
                        projectile.Trail.sprite = circleSprite;
                        projectile.Head.sprite = circleSprite;
                        float orbPulse = 1f + Mathf.Sin(Time.time * 22f) * 0.10f;
                        projectile.Body.transform.localPosition = Vector3.zero;
                        projectile.Body.transform.localScale = new Vector3(projectile.Size * orbPulse, projectile.Size * orbPulse, 1f);
                        projectile.Body.transform.localRotation = Quaternion.identity;
                        projectile.Head.transform.localPosition = Vector3.zero;
                        projectile.Head.transform.localScale = new Vector3(projectile.Size * 0.34f, projectile.Size * 0.34f, 1f);
                        projectile.Head.transform.localRotation = Quaternion.identity;
                        projectile.Trail.transform.localPosition = Vector3.zero;
                        projectile.Trail.transform.localScale = new Vector3(projectile.Size * 1.85f * (1f - progress), projectile.Size * 1.85f * (1f - progress), 1f);
                        projectile.Trail.transform.localRotation = Quaternion.identity;
                        break;
                    case ProjectileKind.Shock:
                        projectile.Head.gameObject.SetActive(false);
                        projectile.Body.sprite = circleSprite;
                        projectile.Trail.sprite = circleSprite;
                        float shockScale = projectile.Size * Mathf.Lerp(0.55f, 1.95f, progress);
                        projectile.Body.transform.localPosition = Vector3.zero;
                        projectile.Body.transform.localScale = new Vector3(shockScale, shockScale * 0.55f, 1f);
                        projectile.Body.transform.localRotation = Quaternion.identity;
                        projectile.Trail.transform.localPosition = Vector3.zero;
                        projectile.Trail.transform.localScale = new Vector3(shockScale * 1.25f, shockScale * 0.66f, 1f);
                        projectile.Trail.transform.localRotation = Quaternion.identity;
                        break;
                    case ProjectileKind.Slash:
                        projectile.Head.gameObject.SetActive(false);
                        projectile.Body.sprite = squareSprite;
                        projectile.Trail.sprite = squareSprite;
                        float slashAngle = -35f + progress * 95f;
                        projectile.Body.transform.localPosition = Vector3.zero;
                        projectile.Body.transform.localScale = new Vector3(projectile.Size * 1.10f, projectile.Size * 0.16f, 1f);
                        projectile.Body.transform.localRotation = Quaternion.Euler(0f, 0f, slashAngle);
                        projectile.Trail.transform.localPosition = Vector3.zero;
                        projectile.Trail.transform.localScale = new Vector3(projectile.Size * 0.72f, projectile.Size * 0.08f, 1f);
                        projectile.Trail.transform.localRotation = Quaternion.Euler(0f, 0f, slashAngle - 22f);
                        break;
                }

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

        private ParticleSystem GetBurstParticle()
        {
            for (int i = 0; i < burstPool.Count; i++)
            {
                if (!burstPool[i].IsAlive(true))
                {
                    return burstPool[i];
                }
            }

            GameObject obj = new GameObject("HitBurst");
            obj.transform.SetParent(sceneRoot, false);
            ParticleSystem particle = obj.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particle.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ShapeModule shape = particle.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.04f;

            ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 40;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            burstPool.Add(particle);
            return particle;
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

        private static Color GetTraitColor(HeroTrait trait)
        {
            switch (trait)
            {
                case HeroTrait.Melee:
                    return new Color(1f, 0.86f, 0.34f, 1f);
                case HeroTrait.Ranged:
                    return new Color(0.54f, 0.86f, 1f, 1f);
                case HeroTrait.Support:
                    return new Color(0.42f, 1f, 0.56f, 1f);
                case HeroTrait.Defense:
                    return new Color(0.72f, 0.78f, 0.86f, 1f);
                default:
                    return Color.white;
            }
        }

        private static Color GetHeroWeaponColor(HeroTrait trait, HeroRarity rarity)
        {
            switch (trait)
            {
                case HeroTrait.Melee:
                    return new Color(0.92f, 0.95f, 1f, 1f);
                case HeroTrait.Ranged:
                    return new Color(0.86f, 0.58f, 0.26f, 1f);
                case HeroTrait.Support:
                    return Color.Lerp(GetRarityColor(rarity), Color.white, 0.20f);
                case HeroTrait.Defense:
                    return new Color(0.78f, 0.82f, 0.88f, 1f);
                default:
                    return Color.white;
            }
        }

        private static Color GetHeroOffhandColor(HeroTrait trait)
        {
            switch (trait)
            {
                case HeroTrait.Melee:
                    return new Color(0.26f, 0.30f, 0.36f, 1f);
                case HeroTrait.Ranged:
                    return new Color(0.96f, 0.96f, 0.82f, 1f);
                case HeroTrait.Support:
                    return new Color(0.66f, 0.96f, 1f, 1f);
                case HeroTrait.Defense:
                    return new Color(0.34f, 0.42f, 0.54f, 1f);
                default:
                    return new Color(0.26f, 0.30f, 0.36f, 1f);
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static string GetShortName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return "?";
            }

            return displayName.Length <= 2 ? displayName : displayName.Substring(displayName.Length - 2);
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

        private enum ProjectileKind
        {
            Arrow,
            Orb,
            Shock,
            Slash
        }

        private sealed class ProjectileVisual
        {
            public ProjectileVisual(GameObject root, SpriteRenderer body, SpriteRenderer trail, SpriteRenderer head)
            {
                Root = root;
                Body = body;
                Trail = trail;
                Head = head;
                Root.SetActive(false);
            }

            public GameObject Root { get; }
            public SpriteRenderer Body { get; }
            public SpriteRenderer Trail { get; }
            public SpriteRenderer Head { get; }
            public Vector2 StartPosition { get; set; }
            public Vector2 TargetPosition { get; set; }
            public Color BaseColor { get; set; }
            public ProjectileKind Kind { get; set; }
            public float Life { get; set; }
            public float Duration { get; set; }
            public float Size { get; set; }
        }

        private sealed class WorldActor
        {
            public WorldActor(
                GameObject root,
                SpriteRenderer back,
                SpriteRenderer body,
                SpriteRenderer head,
                SpriteRenderer headwear,
                SpriteRenderer accent,
                SpriteRenderer armLeft,
                SpriteRenderer armRight,
                SpriteRenderer footLeft,
                SpriteRenderer footRight,
                SpriteRenderer weapon,
                SpriteRenderer offhand,
                SpriteRenderer eyeLeft,
                SpriteRenderer eyeRight,
                TextMesh label,
                GameObject hpRoot,
                SpriteRenderer hpFill)
            {
                Root = root;
                AnimationPhase = (nextActorPhaseSeed++ % 64) * 0.73f;
                Back = back;
                Body = body;
                Head = head;
                Headwear = headwear;
                Accent = accent;
                ArmLeft = armLeft;
                ArmRight = armRight;
                FootLeft = footLeft;
                FootRight = footRight;
                Weapon = weapon;
                Offhand = offhand;
                EyeLeft = eyeLeft;
                EyeRight = eyeRight;
                Label = label;
                HpRoot = hpRoot;
                HpFill = hpFill;
            }

            public GameObject Root { get; }
            public float AnimationPhase { get; }
            public SpriteRenderer Back { get; }
            public SpriteRenderer Body { get; }
            public SpriteRenderer Head { get; }
            public SpriteRenderer Headwear { get; }
            public SpriteRenderer Accent { get; }
            public SpriteRenderer ArmLeft { get; }
            public SpriteRenderer ArmRight { get; }
            public SpriteRenderer FootLeft { get; }
            public SpriteRenderer FootRight { get; }
            public SpriteRenderer Weapon { get; }
            public SpriteRenderer Offhand { get; }
            public SpriteRenderer EyeLeft { get; }
            public SpriteRenderer EyeRight { get; }
            public TextMesh Label { get; }
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
