using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using IdleGame.UI.Header;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void DestroyExistingHudCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.gameObject.name == "IdleGameCanvas")
                {
                    Destroy(canvas.gameObject);
                }
            }
        }

        private void CreateEventSystemIfNeeded()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private void CreateHud()
        {
            canvasObject = new GameObject("IdleGameCanvas", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject root = CreatePanel("Root", canvasObject.transform, new Color(0.07f, 0.08f, 0.10f, 1f));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(0, 0, 0, 0);
            rootLayout.spacing = 0;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            CreateHeader(root.transform);
            CreateBattlePanel(root.transform);
            CreateContentPanels(root.transform);
            CreateBottomNav(root.transform);
            CreateHeroDetailPanel(root.transform);
            CreateHeroFormationSavePrompt(root.transform);
            CreateFacilityRewardPopup(root.transform);
        }

        private void CreateHeader(Transform parent)
        {
            HeaderHudViewRefs refs = HeaderHudView.Build(new HeaderHudViewBuildArgs
            {
                Parent = parent,
                ShowDebugGrantButton = IsDebugPanelEnabled(),
                OnDebugGrant = DebugGrantTestCurrency
            });

            battleHud.StageText = refs.StageText;
            battleHud.ModeText = refs.ModeText;
            resourceText = refs.ResourceText;
            rubyResourceText = refs.RubyResourceText;
            accountLevelText = refs.AccountLevelText;
            accountExpFill = refs.AccountExpFill;
        }
    }
}
