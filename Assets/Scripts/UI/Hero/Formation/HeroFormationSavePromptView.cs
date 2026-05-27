using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Formation
{
    public static class HeroFormationSavePromptView
    {
        public static GameObject Build(Transform parent, Action onConfirm, Action onCancel)
        {
            GameObject prompt = HudUiFactory.CreatePanel("HeroFormationSavePrompt", parent, new Color(0f, 0f, 0f, 0.62f));
            LayoutElement overlayLayout = prompt.AddComponent<LayoutElement>();
            overlayLayout.ignoreLayout = true;
            HudUiFactory.StretchToParent(prompt);

            GameObject dialog = HudUiFactory.CreatePanel("HeroFormationSaveDialog", prompt.transform, new Color(0.12f, 0.16f, 0.24f, 1f));
            RectTransform dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(660f, 320f);
            dialogRect.anchoredPosition = Vector2.zero;

            Text title = HudUiFactory.CreateText("HeroFormationSaveTitle", dialog.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 70f);
            titleRect.anchoredPosition = new Vector2(0f, -26f);
            title.text = "편성 저장";

            Text message = HudUiFactory.CreateText("HeroFormationSaveMessage", dialog.transform, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform messageRect = message.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0f, 0.5f);
            messageRect.anchorMax = new Vector2(1f, 0.5f);
            messageRect.pivot = new Vector2(0.5f, 0.5f);
            messageRect.sizeDelta = new Vector2(0f, 92f);
            messageRect.anchoredPosition = new Vector2(0f, 12f);
            message.text = "변경된 영웅 편성을 저장하시겠습니까?\n저장하면 현재 스테이지가 다시 시작됩니다.";

            GameObject buttonRow = new GameObject("HeroFormationSaveButtons", typeof(RectTransform));
            buttonRow.transform.SetParent(dialog.transform, false);
            RectTransform rowRect = buttonRow.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 0f);
            rowRect.anchorMax = new Vector2(1f, 0f);
            rowRect.pivot = new Vector2(0.5f, 0f);
            rowRect.sizeDelta = new Vector2(-72f, 76f);
            rowRect.anchoredPosition = new Vector2(0f, 34f);
            HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 18;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            Button confirmButton = HudUiFactory.CreateButton("확인", buttonRow.transform, 28, new Color(0.36f, 0.52f, 0.22f, 1f));
            Button cancelButton = HudUiFactory.CreateButton("취소", buttonRow.transform, 28, new Color(0.26f, 0.29f, 0.34f, 1f));
            confirmButton.onClick.AddListener(() => onConfirm?.Invoke());
            cancelButton.onClick.AddListener(() => onCancel?.Invoke());

            prompt.SetActive(false);
            return prompt;
        }
    }
}
