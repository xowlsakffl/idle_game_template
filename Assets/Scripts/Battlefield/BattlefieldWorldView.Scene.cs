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
    }
}
