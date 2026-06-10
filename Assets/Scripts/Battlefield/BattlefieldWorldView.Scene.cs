using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Progression;
using IdleGame.Speed;

namespace IdleGame.Battlefield
{
    public sealed partial class BattlefieldWorldView
    {
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

            fieldMapRoot = new GameObject("FieldMap").transform;
            fieldMapRoot.SetParent(sceneRoot, false);

            dungeonMapRoot = new GameObject("DungeonMap").transform;
            dungeonMapRoot.SetParent(sceneRoot, false);
            dungeonMapRoot.gameObject.SetActive(false);

            actorRoot = new GameObject("Actors").transform;
            actorRoot.SetParent(sceneRoot, false);

            templateRoot = new GameObject("Templates").transform;
            templateRoot.SetParent(sceneRoot, false);

            CreateBackgroundVisual(fieldMapRoot);
            CreateEnvironmentDecorations(fieldMapRoot);
            CreateDungeonMapVisual(dungeonMapRoot);

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

        private void CreateBackgroundVisual(Transform parent)
        {
            CreateGrassBase(parent);
            CreateGrassPatches(parent);

            CreateGroundBand(parent, "GroundPath", new Vector2(0.05f, -0.22f), new Vector2(8.2f, 1.85f), -13f, new Color(0.30f, 0.27f, 0.22f, 0.44f), -48);
            CreateGroundBand(parent, "GroundUpperShade", new Vector2(-0.85f, 3.45f), new Vector2(9.4f, 1.50f), 8f, new Color(0.12f, 0.16f, 0.16f, 0.22f), -47);
            CreateGroundBand(parent, "GroundLowerShade", new Vector2(1.05f, -3.95f), new Vector2(9.6f, 1.72f), 6f, new Color(0.10f, 0.14f, 0.16f, 0.26f), -47);

            Sprite shadowSprite = BattlefieldSpriteCatalog.GetGroundTileSprite();
            if (shadowSprite == null)
            {
                return;
            }

            Vector2[] shadowPositions =
            {
                new Vector2(-2.75f, 2.65f),
                new Vector2(2.92f, 2.15f),
                new Vector2(-3.05f, -1.95f),
                new Vector2(3.05f, -2.60f),
                new Vector2(0.05f, 0.45f)
            };

            for (int i = 0; i < shadowPositions.Length; i++)
            {
                SpriteRenderer shadow = CreateSpriteRenderer("GroundShadow" + i, parent, shadowSprite, new Color(0f, 0f, 0f, i == 4 ? 0.20f : 0.14f), -46);
                shadow.transform.position = ToWorld(shadowPositions[i]);
                shadow.transform.localScale = i == 4 ? new Vector3(2.65f, 1.25f, 1f) : new Vector3(1.40f, 0.72f, 1f);
                shadow.transform.localRotation = Quaternion.Euler(0f, 0f, i * 17f - 22f);
            }
        }

        private void CreateGrassBase(Transform parent)
        {
            backgroundBaseRenderer = CreateSpriteRenderer(
                "GrassBase",
                parent,
                squareSprite,
                new Color(0.28f, 0.48f, 0.25f, 1f),
                -50);

            backgroundBaseRenderer.transform.localScale = new Vector3(FieldHalfWidth * 2.35f, FieldHalfHeight * 2.35f, 1f);

            Sprite grassSprite = BattlefieldSpriteCatalog.GetGrassTileSprite(0);
            if (grassSprite != null)
            {
                backgroundTextureWashRenderer = CreateSpriteRenderer(
                    "GrassTextureWash",
                    parent,
                    grassSprite,
                    new Color(0.88f, 1f, 0.76f, 0.12f),
                    -49);
                backgroundTextureWashRenderer.transform.localScale = new Vector3(9.2f, 11.2f, 1f);
                backgroundTextureWashRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -2.5f);
            }
        }

        private void CreateDungeonMapVisual(Transform parent)
        {
            dungeonBaseRenderer = CreateSpriteRenderer(
                "DungeonStoneBase",
                parent,
                squareSprite,
                new Color(0.22f, 0.20f, 0.30f, 1f),
                -55);
            dungeonBaseRenderer.transform.localScale = new Vector3(FieldHalfWidth * 2.35f, FieldHalfHeight * 2.35f, 1f);

            dungeonWashRenderer = CreateSpriteRenderer(
                "DungeonMagicWash",
                parent,
                squareSprite,
                new Color(1f, 0.52f, 1f, 0.14f),
                -54);
            dungeonWashRenderer.transform.localScale = new Vector3(8.9f, 10.8f, 1f);

            CreateDungeonFloor(parent);
            CreateDungeonGate(parent);
            CreateDungeonWall(parent);
            CreateDungeonPillars(parent);
            CreateDungeonCrystals(parent);
        }

        private void CreateDungeonFloor(Transform parent)
        {
            CreateGroundBand(parent, "DungeonCenterAisle", new Vector2(0f, -0.58f), new Vector2(5.35f, 8.80f), 0f, new Color(0.10f, 0.12f, 0.18f, 0.34f), -53);
            CreateGroundBand(parent, "DungeonCrossAisle", new Vector2(0f, 0.62f), new Vector2(8.65f, 1.10f), 0f, new Color(0.08f, 0.09f, 0.14f, 0.28f), -52);

            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    SpriteRenderer tile = CreateSpriteRenderer("DungeonFloorTile" + y + "_" + x, parent, squareSprite, new Color(0.42f, 0.43f, 0.50f, 0.12f), -51);
                    tile.transform.position = ToWorld(new Vector2(-2.2f + x * 1.1f, -3.55f + y * 1.05f));
                    tile.transform.localScale = new Vector3(0.92f, 0.08f, 1f);
                }
            }
        }

        private void CreateDungeonGate(Transform parent)
        {
            SpriteRenderer gateBack = CreateSpriteRenderer("DungeonGateBack", parent, squareSprite, new Color(0.06f, 0.07f, 0.10f, 1f), -44);
            gateBack.transform.position = ToWorld(new Vector2(0f, 4.14f));
            gateBack.transform.localScale = new Vector3(2.16f, 1.12f, 1f);

            dungeonGateRenderer = CreateSpriteRenderer("DungeonGateArch", parent, squareSprite, new Color(0.35f, 0.36f, 0.44f, 1f), -43);
            dungeonGateRenderer.transform.position = ToWorld(new Vector2(0f, 4.28f));
            dungeonGateRenderer.transform.localScale = new Vector3(2.52f, 0.42f, 1f);

            SpriteRenderer leftPost = CreateSpriteRenderer("DungeonGateLeftPost", parent, squareSprite, new Color(0.27f, 0.28f, 0.34f, 1f), -42);
            leftPost.transform.position = ToWorld(new Vector2(-1.28f, 3.72f));
            leftPost.transform.localScale = new Vector3(0.34f, 1.24f, 1f);

            SpriteRenderer rightPost = CreateSpriteRenderer("DungeonGateRightPost", parent, squareSprite, new Color(0.27f, 0.28f, 0.34f, 1f), -42);
            rightPost.transform.position = ToWorld(new Vector2(1.28f, 3.72f));
            rightPost.transform.localScale = new Vector3(0.34f, 1.24f, 1f);

            dungeonGateGemRenderer = CreateSpriteRenderer("DungeonGateGem", parent, circleSprite, new Color(1f, 0.52f, 1f, 0.95f), -41);
            dungeonGateGemRenderer.transform.position = ToWorld(new Vector2(0f, 4.34f));
            dungeonGateGemRenderer.transform.localScale = new Vector3(0.34f, 0.34f, 1f);
        }

        private void CreateDungeonWall(Transform parent)
        {
            for (int i = 0; i < 10; i++)
            {
                float x = -4.05f + i * 0.90f;
                SpriteRenderer block = CreateSpriteRenderer("DungeonTopBlock" + i, parent, squareSprite, new Color(0.31f, 0.32f, 0.39f, 1f), -45);
                block.transform.position = ToWorld(new Vector2(x, 4.86f + (i % 2) * 0.05f));
                block.transform.localScale = new Vector3(0.76f, 0.42f, 1f);
            }

            for (int i = 0; i < 8; i++)
            {
                float y = -2.88f + i * 0.86f;
                SpriteRenderer left = CreateSpriteRenderer("DungeonLeftWall" + i, parent, squareSprite, new Color(0.24f, 0.25f, 0.31f, 0.92f), -45);
                left.transform.position = ToWorld(new Vector2(-4.12f, y));
                left.transform.localScale = new Vector3(0.34f, 0.70f, 1f);

                SpriteRenderer right = CreateSpriteRenderer("DungeonRightWall" + i, parent, squareSprite, new Color(0.24f, 0.25f, 0.31f, 0.92f), -45);
                right.transform.position = ToWorld(new Vector2(4.12f, y + 0.10f));
                right.transform.localScale = new Vector3(0.34f, 0.70f, 1f);
            }
        }

        private void CreateDungeonPillars(Transform parent)
        {
            Vector2[] positions =
            {
                new Vector2(-3.18f, 2.82f),
                new Vector2(3.18f, 2.82f),
                new Vector2(-3.10f, -1.28f),
                new Vector2(3.10f, -1.28f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                SpriteRenderer pillar = CreateSpriteRenderer("DungeonPillar" + i, parent, squareSprite, new Color(0.28f, 0.28f, 0.35f, 1f), -43);
                pillar.transform.position = ToWorld(positions[i]);
                pillar.transform.localScale = new Vector3(0.42f, 1.06f, 1f);

                SpriteRenderer cap = CreateSpriteRenderer("DungeonPillarCap" + i, parent, squareSprite, new Color(0.39f, 0.40f, 0.48f, 1f), -42);
                cap.transform.position = ToWorld(positions[i] + new Vector2(0f, 0.56f));
                cap.transform.localScale = new Vector3(0.70f, 0.22f, 1f);
            }
        }

        private void CreateDungeonCrystals(Transform parent)
        {
            Vector2[] positions =
            {
                new Vector2(-3.38f, 3.62f),
                new Vector2(3.42f, 3.52f),
                new Vector2(-3.56f, 0.30f),
                new Vector2(3.48f, 0.10f),
                new Vector2(-2.92f, -3.20f),
                new Vector2(2.82f, -3.28f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                SpriteRenderer glow = CreateSpriteRenderer("DungeonCrystalGlow" + i, parent, circleSprite, new Color(0.94f, 0.40f, 1f, 0.18f), -40);
                glow.transform.position = ToWorld(positions[i]);
                glow.transform.localScale = Vector3.one * (0.72f + (i % 2) * 0.18f);

                SpriteRenderer crystal = CreateSpriteRenderer("DungeonCrystal" + i, parent, squareSprite, new Color(0.82f, 0.38f, 1f, 0.90f), -39);
                crystal.transform.position = ToWorld(positions[i]);
                crystal.transform.localScale = new Vector3(0.22f, 0.58f + (i % 3) * 0.12f, 1f);
                crystal.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
        }

        private void UpdateSceneTone()
        {
            bool dungeon = battleManager != null && battleManager.IsDungeonRunActive;
            DungeonKind kind = dungeon ? battleManager.ActiveDungeonKind : DungeonKind.Ruby;
            if (!observedDungeonSceneModeInitialized)
            {
                observedDungeonSceneModeInitialized = true;
                observedDungeonSceneMode = dungeon;
            }
            else if (observedDungeonSceneMode != dungeon)
            {
                observedDungeonSceneMode = dungeon;
                ResetEnemyVisualContinuity();
            }

            if (fieldMapRoot != null && fieldMapRoot.gameObject.activeSelf == dungeon)
            {
                fieldMapRoot.gameObject.SetActive(!dungeon);
            }

            if (dungeonMapRoot != null && dungeonMapRoot.gameObject.activeSelf != dungeon)
            {
                dungeonMapRoot.gameObject.SetActive(dungeon);
            }

            if (!dungeon)
            {
                if (renderCamera != null)
                {
                    renderCamera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
                }

                return;
            }

            if (dungeonBaseRenderer != null)
            {
                dungeonBaseRenderer.color = GetDungeonBaseColor(kind);
            }

            if (dungeonWashRenderer != null)
            {
                dungeonWashRenderer.color = GetDungeonWashColor(kind);
            }

            if (dungeonGateRenderer != null)
            {
                dungeonGateRenderer.color = Color.Lerp(new Color(0.26f, 0.27f, 0.34f, 1f), GetDungeonBaseColor(kind), 0.42f);
            }

            if (dungeonGateGemRenderer != null)
            {
                Color gemColor = GetDungeonWashColor(kind);
                dungeonGateGemRenderer.color = new Color(gemColor.r, gemColor.g, gemColor.b, 0.96f);
            }

            if (renderCamera != null)
            {
                renderCamera.backgroundColor = GetDungeonCameraColor(kind);
            }
        }

        private static Color GetDungeonBaseColor(DungeonKind kind)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return new Color(0.45f, 0.36f, 0.18f, 1f);
                case DungeonKind.TotemEssence:
                    return new Color(0.17f, 0.39f, 0.36f, 1f);
                case DungeonKind.HeroTranscendStone:
                    return new Color(0.23f, 0.26f, 0.48f, 1f);
                default:
                    return new Color(0.37f, 0.22f, 0.47f, 1f);
            }
        }

        private static Color GetDungeonWashColor(DungeonKind kind)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return new Color(1f, 0.83f, 0.30f, 0.18f);
                case DungeonKind.TotemEssence:
                    return new Color(0.34f, 1f, 0.76f, 0.16f);
                case DungeonKind.HeroTranscendStone:
                    return new Color(0.70f, 0.78f, 1f, 0.17f);
                default:
                    return new Color(1f, 0.52f, 1f, 0.18f);
            }
        }

        private static Color GetDungeonCameraColor(DungeonKind kind)
        {
            switch (kind)
            {
                case DungeonKind.Gold:
                    return new Color(0.11f, 0.09f, 0.05f, 1f);
                case DungeonKind.TotemEssence:
                    return new Color(0.04f, 0.11f, 0.10f, 1f);
                case DungeonKind.HeroTranscendStone:
                    return new Color(0.06f, 0.07f, 0.15f, 1f);
                default:
                    return new Color(0.10f, 0.05f, 0.13f, 1f);
            }
        }

        private void CreateGrassPatches(Transform parent)
        {
            Vector2[] positions =
            {
                new Vector2(-2.95f, 3.05f),
                new Vector2(2.30f, 3.55f),
                new Vector2(-3.18f, 0.62f),
                new Vector2(3.10f, -0.05f),
                new Vector2(-2.48f, -2.80f),
                new Vector2(2.58f, -3.18f)
            };

            Vector2[] sizes =
            {
                new Vector2(2.35f, 1.52f),
                new Vector2(2.70f, 1.60f),
                new Vector2(2.10f, 1.82f),
                new Vector2(2.35f, 1.55f),
                new Vector2(2.62f, 1.48f),
                new Vector2(2.40f, 1.66f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Sprite patchSprite = BattlefieldSpriteCatalog.GetGrassTileSprite(i + 1);
                if (patchSprite == null)
                {
                    continue;
                }

                SpriteRenderer patch = CreateSpriteRenderer("GrassPatch" + i, parent, patchSprite, new Color(1f, 1f, 1f, 0.11f), -48);
                patch.transform.position = ToWorld(positions[i]);
                patch.transform.localScale = new Vector3(sizes[i].x * 0.78f, sizes[i].y * 0.78f, 1f);
                patch.transform.localRotation = Quaternion.Euler(0f, 0f, -18f + i * 11f);
            }
        }

        private void CreateGroundBand(Transform parent, string name, Vector2 localPosition, Vector2 scale, float rotation, Color color, int sortingOrder)
        {
            SpriteRenderer band = CreateSpriteRenderer(name, parent, squareSprite, color, sortingOrder);
            band.transform.position = ToWorld(localPosition);
            band.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            band.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private void CreateEnvironmentDecorations(Transform parent)
        {
            Vector2[] positions =
            {
                new Vector2(-3.48f, 4.22f),
                new Vector2(-2.32f, 4.58f),
                new Vector2(2.20f, 4.42f),
                new Vector2(3.38f, 3.78f),
                new Vector2(-3.38f, 2.18f),
                new Vector2(-2.58f, 1.22f),
                new Vector2(2.92f, 2.08f),
                new Vector2(3.36f, 0.78f),
                new Vector2(-3.50f, -0.82f),
                new Vector2(-2.74f, -1.92f),
                new Vector2(2.86f, -1.42f),
                new Vector2(3.46f, -2.64f),
                new Vector2(-3.10f, -4.00f),
                new Vector2(-1.72f, -4.48f),
                new Vector2(1.62f, -4.46f),
                new Vector2(3.22f, -3.86f),
                new Vector2(-1.24f, 3.56f),
                new Vector2(1.34f, 3.22f),
                new Vector2(-1.94f, -3.18f),
                new Vector2(1.98f, -3.04f)
            };

            float[] scales =
            {
                0.42f,
                0.76f,
                0.78f,
                0.46f,
                0.58f,
                0.40f,
                0.74f,
                0.40f,
                0.46f,
                0.62f,
                0.40f,
                0.48f,
                0.46f,
                0.76f,
                0.42f,
                0.54f,
                0.58f,
                0.54f,
                0.50f,
                0.52f
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Sprite sprite = BattlefieldSpriteCatalog.GetDecorationSprite(i);
                if (sprite == null)
                {
                    continue;
                }

                SpriteRenderer decoration = CreateSpriteRenderer("Decoration" + i, parent, sprite, Color.white, -12);
                decoration.transform.position = ToWorld(positions[i]);
                float scale = scales[i];
                decoration.transform.localScale = Vector3.one * scale;
                decoration.transform.localRotation = Quaternion.Euler(0f, 0f, -10f + i * 5f);
            }
        }

        private static void EnsureSprites()
        {
            if (squareSprite != null && circleSprite != null)
            {
                return;
            }

            if (squareSprite == null)
            {
                squareSprite = CreateSquareSprite();
            }

            if (circleSprite == null)
            {
                circleSprite = CreateCircleSprite();
            }
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

        private static Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                name = "RuntimeCircleSprite",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Vector2 center = new Vector2(7.5f, 7.5f);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= 6.8f)
                    {
                        texture.SetPixel(x, y, distance < 4.8f ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
        }

        private void CreateFortressVisual(Transform parent)
        {
            fortressRoot = new GameObject("Fortress").transform;
            fortressRoot.SetParent(parent, false);
            fortressRoot.localPosition = Vector3.zero;

            Sprite fortressSprite = BattlefieldSpriteCatalog.GetFortressSprite(1, false);
            bool usesAssetSprite = fortressSprite != null;
            fortressBaseRenderer = CreateSpriteRenderer(
                "FortressBody",
                fortressRoot,
                usesAssetSprite ? fortressSprite : squareSprite,
                usesAssetSprite ? Color.white : new Color(0.25f, 0.30f, 0.38f, 1f),
                1);
            fortressBaseRenderer.transform.localPosition = Vector3.zero;
            fortressBaseRenderer.transform.localScale = usesAssetSprite ? new Vector3(1.34f, 1.34f, 1f) : new Vector3(1.62f, 1.10f, 1f);

            fortressLeftTowerRenderer = CreateSpriteRenderer("FortressLeftTower", fortressRoot, squareSprite, Color.white, 0);
            fortressRightTowerRenderer = CreateSpriteRenderer("FortressRightTower", fortressRoot, squareSprite, Color.white, 0);

            fortressLeftCannonBaseRenderer = CreateSpriteRenderer("FortressLeftCannonBase", fortressRoot, squareSprite, new Color(0.16f, 0.14f, 0.13f, 1f), 3);
            fortressRightCannonBaseRenderer = CreateSpriteRenderer("FortressRightCannonBase", fortressRoot, squareSprite, new Color(0.16f, 0.14f, 0.13f, 1f), 3);
            fortressLeftCannonBarrelRenderer = CreateSpriteRenderer("FortressLeftCannonBarrel", fortressRoot, squareSprite, new Color(0.06f, 0.07f, 0.08f, 1f), 4);
            fortressRightCannonBarrelRenderer = CreateSpriteRenderer("FortressRightCannonBarrel", fortressRoot, squareSprite, new Color(0.06f, 0.07f, 0.08f, 1f), 4);

            SpriteRenderer hpBack = CreateSpriteRenderer("FortressHpBack", fortressRoot, squareSprite, new Color(0.02f, 0.025f, 0.03f, 0.95f), 5);
            hpBack.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            hpBack.transform.localScale = new Vector3(1.34f, 0.09f, 1f);

            fortressHpFillRenderer = CreateSpriteRenderer("FortressHpFill", fortressRoot, squareSprite, new Color(0.40f, 0.95f, 0.72f, 1f), 6);
            fortressHpFillRenderer.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            fortressHpFillRenderer.transform.localScale = new Vector3(1.28f, 0.056f, 1f);
        }

        private void UpdateFortressVisual()
        {
            if (fortressRoot == null || battleManager == null)
            {
                return;
            }

            int fortressLevel = battleManager.FortressLevel;
            float hpRatio = battleManager.FortressHpRatio;
            bool alive = hpRatio > 0f;
            bool destroyed = !alive;
            float levelRatio = Mathf.Clamp01((fortressLevel - 1f) / Mathf.Max(1f, battleManager.FortressMaxLevel - 1f));
            float visualScale = 1f + levelRatio * 0.18f + GetFortressVisualTier(fortressLevel) * 0.045f;
            fortressRoot.localScale = new Vector3(visualScale, visualScale, 1f);

            Color healthyBase = new Color(0.25f, 0.30f, 0.38f, 1f);
            Color damagedBase = new Color(0.22f, 0.13f, 0.12f, 1f);
            if (fortressBaseRenderer != null)
            {
                Sprite fortressSprite = BattlefieldSpriteCatalog.GetFortressSprite(fortressLevel, destroyed);
                if (fortressSprite != null)
                {
                    fortressBaseRenderer.sprite = fortressSprite;
                }

                fortressBaseRenderer.color = fortressBaseRenderer.sprite == squareSprite
                    ? Color.Lerp(damagedBase, healthyBase, hpRatio)
                    : Color.Lerp(new Color(0.72f, 0.64f, 0.60f, 1f), Color.white, hpRatio);
                fortressBaseRenderer.transform.localScale = fortressBaseRenderer.sprite == squareSprite
                    ? new Vector3(1.62f, 1.10f, 1f)
                    : Vector3.one * GetFortressBodyScale(fortressLevel, destroyed);
            }

            UpdateFortressTower(fortressLeftTowerRenderer, fortressLevel, destroyed, -1f);
            UpdateFortressTower(fortressRightTowerRenderer, fortressLevel, destroyed, 1f);
            UpdateFortressCannon(fortressLeftCannonBaseRenderer, fortressLeftCannonBarrelRenderer, fortressLevel, destroyed, -1f);
            UpdateFortressCannon(fortressRightCannonBaseRenderer, fortressRightCannonBarrelRenderer, fortressLevel, destroyed, 1f);

            if (fortressHpFillRenderer != null)
            {
                fortressHpFillRenderer.color = alive ? new Color(0.40f, 0.95f, 0.72f, 1f) : new Color(0.60f, 0.12f, 0.10f, 1f);
                fortressHpFillRenderer.transform.localScale = new Vector3(1.28f * Mathf.Clamp01(hpRatio), 0.056f, 1f);
                fortressHpFillRenderer.transform.localPosition = new Vector3(-0.64f * (1f - Mathf.Clamp01(hpRatio)), 1.35f, 0f);
            }
        }

        private static int GetFortressVisualTier(int level)
        {
            if (level >= 180)
            {
                return 3;
            }

            if (level >= 90)
            {
                return 2;
            }

            if (level >= 25)
            {
                return 1;
            }

            return 0;
        }

        private static float GetFortressBodyScale(int level, bool destroyed)
        {
            if (destroyed)
            {
                return 1.42f;
            }

            return level < 25 ? 1.28f : 1.44f + GetFortressVisualTier(level) * 0.05f;
        }

        private void UpdateFortressTower(SpriteRenderer renderer, int level, bool destroyed, float side)
        {
            if (renderer == null)
            {
                return;
            }

            Sprite towerSprite = BattlefieldSpriteCatalog.GetFortressTowerSprite(level, destroyed);
            bool visible = towerSprite != null;
            renderer.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            renderer.sprite = towerSprite;
            renderer.color = Color.white;
            renderer.transform.localPosition = new Vector3(side * 1.10f, 0.18f, 0f);
            renderer.transform.localScale = Vector3.one * (0.88f + GetFortressVisualTier(level) * 0.05f);
        }

        private void UpdateFortressCannon(SpriteRenderer baseRenderer, SpriteRenderer barrelRenderer, int level, bool destroyed, float side)
        {
            if (baseRenderer == null || barrelRenderer == null)
            {
                return;
            }

            bool visible = !destroyed;
            baseRenderer.gameObject.SetActive(visible);
            barrelRenderer.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            Vector2 muzzle = GetFortressMuzzlePosition(side, level);
            baseRenderer.transform.localPosition = new Vector3(muzzle.x - side * 0.05f, muzzle.y - 0.05f, 0f);
            baseRenderer.transform.localScale = new Vector3(0.24f, 0.17f, 1f);

            barrelRenderer.transform.localPosition = new Vector3(muzzle.x + side * 0.06f, muzzle.y, 0f);
            barrelRenderer.transform.localScale = new Vector3(0.30f, 0.08f, 1f);
            barrelRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, side < 0f ? 164f : 16f);
        }
    }
}
