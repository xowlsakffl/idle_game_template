using System.Collections.Generic;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
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

            actorRoot = new GameObject("Actors").transform;
            actorRoot.SetParent(sceneRoot, false);

            templateRoot = new GameObject("Templates").transform;
            templateRoot.SetParent(sceneRoot, false);

            CreateBackgroundVisual(sceneRoot);
            CreateEnvironmentDecorations(sceneRoot);

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
            SpriteRenderer baseRenderer = CreateSpriteRenderer(
                "GrassBase",
                parent,
                squareSprite,
                new Color(0.28f, 0.48f, 0.25f, 1f),
                -50);

            baseRenderer.transform.localScale = new Vector3(FieldHalfWidth * 2.35f, FieldHalfHeight * 2.35f, 1f);

            Sprite grassSprite = BattlefieldSpriteCatalog.GetGrassTileSprite(0);
            if (grassSprite != null)
            {
                SpriteRenderer textureWash = CreateSpriteRenderer(
                    "GrassTextureWash",
                    parent,
                    grassSprite,
                    new Color(0.88f, 1f, 0.76f, 0.12f),
                    -49);
                textureWash.transform.localScale = new Vector3(9.2f, 11.2f, 1f);
                textureWash.transform.localRotation = Quaternion.Euler(0f, 0f, -2.5f);
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

            Sprite fortressSprite = BattlefieldSpriteCatalog.GetFortressSprite();
            bool usesAssetSprite = fortressSprite != null;
            fortressBaseRenderer = CreateSpriteRenderer(
                "FortressBody",
                fortressRoot,
                usesAssetSprite ? fortressSprite : squareSprite,
                usesAssetSprite ? Color.white : new Color(0.25f, 0.30f, 0.38f, 1f),
                1);
            fortressBaseRenderer.transform.localPosition = Vector3.zero;
            fortressBaseRenderer.transform.localScale = usesAssetSprite ? new Vector3(1.50f, 1.50f, 1f) : new Vector3(1.62f, 1.10f, 1f);

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

            float hpRatio = battleManager.FortressHpRatio;
            bool alive = hpRatio > 0f;
            fortressRoot.localScale = Vector3.one;

            Color healthyBase = new Color(0.25f, 0.30f, 0.38f, 1f);
            Color damagedBase = new Color(0.22f, 0.13f, 0.12f, 1f);
            if (fortressBaseRenderer != null)
            {
                fortressBaseRenderer.color = fortressBaseRenderer.sprite == squareSprite
                    ? Color.Lerp(damagedBase, healthyBase, hpRatio)
                    : Color.Lerp(new Color(0.72f, 0.64f, 0.60f, 1f), Color.white, hpRatio);
            }

            if (fortressHpFillRenderer != null)
            {
                fortressHpFillRenderer.color = alive ? new Color(0.40f, 0.95f, 0.72f, 1f) : new Color(0.60f, 0.12f, 0.10f, 1f);
                fortressHpFillRenderer.transform.localScale = new Vector3(1.28f * Mathf.Clamp01(hpRatio), 0.056f, 1f);
                fortressHpFillRenderer.transform.localPosition = new Vector3(-0.64f * (1f - Mathf.Clamp01(hpRatio)), 1.35f, 0f);
            }
        }
    }
}
