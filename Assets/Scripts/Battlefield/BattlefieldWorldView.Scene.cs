using UnityEngine;
using IdleGame.Data;

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
    }
}
