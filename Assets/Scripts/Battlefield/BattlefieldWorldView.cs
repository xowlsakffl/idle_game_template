using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Speed;

namespace IdleGame.Battlefield
{
    public sealed partial class BattlefieldWorldView : MonoBehaviour
    {
        private const int RenderWidth = 720;
        private const int RenderHeight = 980;
        private const float FieldHalfWidth = 3.85f;
        private const float FieldHalfHeight = 5.15f;
        private const float OriginX = 1000f;
        private const float HeroAttackPulseDuration = 0.52f;
        private const float EnemyAttackPulseDuration = 0.62f;
        private const float ActorHitPulseDuration = 0.20f;

        private static Sprite squareSprite;
        private static Sprite circleSprite;

        private readonly Dictionary<string, WorldActor> heroActors = new Dictionary<string, WorldActor>();
        private readonly List<WorldActor> enemyActors = new List<WorldActor>();
        private readonly List<DamageFloater> damageFloaters = new List<DamageFloater>();
        private readonly List<SpriteEffectVisual> spriteEffects = new List<SpriteEffectVisual>();
        private readonly List<ProjectileVisual> projectiles = new List<ProjectileVisual>();
        private readonly Dictionary<string, Vector2> heroLocalPositions = new Dictionary<string, Vector2>();
        private readonly List<Vector2> enemyLocalPositions = new List<Vector2>();

        private BattleManager battleManager;
        private GameSpeedManager speedManager;
        private Camera renderCamera;
        private RenderTexture renderTexture;
        private Transform sceneRoot;
        private Transform fieldMapRoot;
        private Transform dungeonMapRoot;
        private Transform actorRoot;
        private Transform templateRoot;
        private GameObject heroTemplate;
        private GameObject enemyTemplate;
        private SpriteRenderer backgroundBaseRenderer;
        private SpriteRenderer backgroundTextureWashRenderer;
        private SpriteRenderer dungeonBaseRenderer;
        private SpriteRenderer dungeonWashRenderer;
        private SpriteRenderer dungeonGateRenderer;
        private SpriteRenderer dungeonGateGemRenderer;
        private Transform fortressRoot;
        private SpriteRenderer fortressBaseRenderer;
        private SpriteRenderer fortressLeftTowerRenderer;
        private SpriteRenderer fortressRightTowerRenderer;
        private SpriteRenderer fortressLeftCannonBaseRenderer;
        private SpriteRenderer fortressRightCannonBaseRenderer;
        private SpriteRenderer fortressLeftCannonBarrelRenderer;
        private SpriteRenderer fortressRightCannonBarrelRenderer;
        private SpriteRenderer fortressHpFillRenderer;
        private int observedHitSequence = -1;
        private int observedHeroAttackBatchSequence = -1;
        private int observedMonsterHitSequence = -1;
        private int observedEnemyDefeatSequence = -1;
        private int observedFortressAttackSequence = -1;
        private bool observedDungeonSceneModeInitialized;
        private bool observedDungeonSceneMode;
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
            observedFortressAttackSequence = battleManager != null ? battleManager.FortressAttackSequence : -1;
            observedDungeonSceneMode = battleManager != null && battleManager.IsDungeonRunActive;
            observedDungeonSceneModeInitialized = true;
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
            UpdateSceneTone();
            UpdateFortressVisual();
            UpdateHeroes(deltaTime);
            UpdateEnemies(deltaTime);
            PlayHitBursts();
            UpdateProjectiles(deltaTime);
            UpdateSpriteEffects(deltaTime);
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
            public Sprite Sprite { get; set; }
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
            public string AnimationKey { get; set; }
            public float AnimationTime { get; set; }
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

        private sealed class SpriteEffectVisual
        {
            public SpriteEffectVisual(GameObject root, SpriteRenderer body)
            {
                Root = root;
                Body = body;
                Root.SetActive(false);
            }

            public GameObject Root { get; }
            public SpriteRenderer Body { get; }
            public Vector2 StartPosition { get; set; }
            public Color BaseColor { get; set; }
            public BattlefieldSpriteAnimation Animation { get; set; }
            public float Life { get; set; }
            public float Duration { get; set; }
            public float StartScale { get; set; }
            public float EndScale { get; set; }
            public float RotationSpeed { get; set; }
        }
    }
}
