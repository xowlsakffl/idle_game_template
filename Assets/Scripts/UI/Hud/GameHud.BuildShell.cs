using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using IdleGame.UI.Common;
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

            GameObject root = CreatePanel("Root", canvasObject.transform, new Color(0.09f, 0.12f, 0.09f, 1f));
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
            CreateDungeonEntryTransition(root.transform);
            CreateDungeonClearPopup(root.transform);
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

        private void CreateDungeonEntryTransition(Transform parent)
        {
            dungeonTransitionRoot = HudUiFactory.CreatePanel("DungeonEntryTransition", parent, new Color(0.02f, 0.025f, 0.04f, 0.96f));
            LayoutElement overlayLayout = dungeonTransitionRoot.AddComponent<LayoutElement>();
            overlayLayout.ignoreLayout = true;
            HudUiFactory.StretchToParent(dungeonTransitionRoot);
            dungeonTransitionRoot.SetActive(false);
            dungeonTransitionCanvasGroup = dungeonTransitionRoot.AddComponent<CanvasGroup>();
            dungeonTransitionCanvasGroup.alpha = 0f;
            dungeonTransitionCanvasGroup.blocksRaycasts = true;
            dungeonTransitionCanvasGroup.interactable = false;

            GameObject gate = HudUiFactory.CreatePanel("DungeonEntryGate", dungeonTransitionRoot.transform, new Color(0.15f, 0.17f, 0.26f, 1f));
            RectTransform gateRect = gate.GetComponent<RectTransform>();
            gateRect.anchorMin = new Vector2(0.5f, 0.5f);
            gateRect.anchorMax = new Vector2(0.5f, 0.5f);
            gateRect.pivot = new Vector2(0.5f, 0.5f);
            gateRect.sizeDelta = new Vector2(560f, 620f);
            gateRect.anchoredPosition = new Vector2(0f, 40f);
            HudUiFactory.ApplySprite(gate.GetComponent<Image>(), HudSpriteKind.BluePanel, new Color(0.24f, 0.31f, 0.48f, 1f));

            Text gateMark = HudUiFactory.CreateText("DungeonEntryGateMark", gate.transform, 120, FontStyle.Bold, TextAnchor.MiddleCenter);
            gateMark.text = "◆";
            gateMark.color = new Color(1f, 0.52f, 1f, 0.95f);
            RectTransform markRect = gateMark.GetComponent<RectTransform>();
            markRect.anchorMin = new Vector2(0.5f, 0.5f);
            markRect.anchorMax = new Vector2(0.5f, 0.5f);
            markRect.pivot = new Vector2(0.5f, 0.5f);
            markRect.sizeDelta = new Vector2(220f, 180f);
            markRect.anchoredPosition = new Vector2(0f, 88f);

            dungeonTransitionTitleText = HudUiFactory.CreateText("DungeonEntryTitle", dungeonTransitionRoot.transform, 54, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform titleRect = dungeonTransitionTitleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(840f, 96f);
            titleRect.anchoredPosition = new Vector2(0f, -210f);

            dungeonTransitionSubtitleText = HudUiFactory.CreateText("DungeonEntrySubtitle", dungeonTransitionRoot.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform subtitleRect = dungeonTransitionSubtitleText.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
            subtitleRect.pivot = new Vector2(0.5f, 0.5f);
            subtitleRect.sizeDelta = new Vector2(760f, 70f);
            subtitleRect.anchoredPosition = new Vector2(0f, -286f);
        }

        private void CreateDungeonClearPopup(Transform parent)
        {
            dungeonClearPopupRoot = HudUiFactory.CreatePanel("DungeonClearPopup", parent, new Color(0f, 0f, 0f, 0.72f));
            LayoutElement overlayLayout = dungeonClearPopupRoot.AddComponent<LayoutElement>();
            overlayLayout.ignoreLayout = true;
            HudUiFactory.StretchToParent(dungeonClearPopupRoot);
            dungeonClearPopupRoot.SetActive(false);

            GameObject dialog = HudUiFactory.CreateSpritePanel("DungeonClearDialog", dungeonClearPopupRoot.transform, HudSpriteKind.BluePanel, new Color(0.32f, 0.40f, 0.58f, 1f));
            RectTransform dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(740f, 560f);
            dialogRect.anchoredPosition = Vector2.zero;

            GameObject titleRibbon = HudUiFactory.CreateSpritePanel("DungeonClearTitleRibbon", dialog.transform, HudSpriteKind.BlueRibbon, Color.white);
            RectTransform ribbonRect = titleRibbon.GetComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 1f);
            ribbonRect.anchorMax = new Vector2(0.5f, 1f);
            ribbonRect.pivot = new Vector2(0.5f, 1f);
            ribbonRect.sizeDelta = new Vector2(500f, 74f);
            ribbonRect.anchoredPosition = new Vector2(0f, 26f);

            dungeonClearPopupTitleText = HudUiFactory.CreateText("DungeonClearTitleText", titleRibbon.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            dungeonClearPopupTitleText.text = "던전 클리어";
            HudUiFactory.StretchToParent(dungeonClearPopupTitleText.gameObject);

            GameObject rewardBox = HudUiFactory.CreateSpritePanel("DungeonClearRewardBox", dialog.transform, HudSpriteKind.CarvedPanel, new Color(0.18f, 0.22f, 0.33f, 1f));
            RectTransform rewardRect = rewardBox.GetComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
            rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
            rewardRect.pivot = new Vector2(0.5f, 0.5f);
            rewardRect.sizeDelta = new Vector2(610f, 245f);
            rewardRect.anchoredPosition = new Vector2(0f, 38f);

            dungeonClearPopupRewardText = HudUiFactory.CreateText("DungeonClearRewardText", rewardBox.transform, 38, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(dungeonClearPopupRewardText.gameObject);
            RectTransform rewardTextRect = dungeonClearPopupRewardText.GetComponent<RectTransform>();
            rewardTextRect.offsetMin = new Vector2(24f, 18f);
            rewardTextRect.offsetMax = new Vector2(-24f, -18f);
            dungeonClearPopupRewardText.lineSpacing = 0.9f;
            HudUiFactory.ConfigureBestFitText(dungeonClearPopupRewardText, 22, 38, 0.9f);

            Text tapText = HudUiFactory.CreateText("DungeonClearTapText", dialog.transform, 26, FontStyle.Bold, TextAnchor.MiddleCenter);
            tapText.text = "화면을 탭해 닫기";
            RectTransform tapRect = tapText.GetComponent<RectTransform>();
            tapRect.anchorMin = new Vector2(0.5f, 0f);
            tapRect.anchorMax = new Vector2(0.5f, 0f);
            tapRect.pivot = new Vector2(0.5f, 0f);
            tapRect.sizeDelta = new Vector2(420f, 54f);
            tapRect.anchoredPosition = new Vector2(0f, 58f);

            GameObject closeCatcher = new GameObject("DungeonClearTapCatcher", typeof(RectTransform), typeof(Image), typeof(Button));
            closeCatcher.transform.SetParent(dungeonClearPopupRoot.transform, false);
            HudUiFactory.StretchToParent(closeCatcher);
            Image closeImage = closeCatcher.GetComponent<Image>();
            closeImage.color = Color.clear;
            Button closeButton = closeCatcher.GetComponent<Button>();
            closeButton.targetGraphic = closeImage;
            closeButton.onClick.AddListener(CloseDungeonClearPopup);
        }
    }
}
