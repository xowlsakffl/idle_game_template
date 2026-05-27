using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Facility
{
    public sealed class FacilityViewRefs
    {
        public GameObject Content;
        public GameObject AssignmentModal;
        public Text SummaryText;
        public Text NoticeText;
    }

    public sealed class FacilityRewardPopupRefs
    {
        public GameObject Popup;
        public Text ListText;
    }

    public sealed class FacilityViewBuildArgs
    {
        public Transform Parent;
        public IReadOnlyList<FacilityDefinition> Facilities;
        public Action<string> OnCollectFacility;
        public Action<string> OnUpgradeFacility;
        public Action OnOpenAssignments;
        public Action OnCollectAll;
        public Action OnCloseAssignments;
        public Action OnAutoAssignAll;
        public Action OnClearAssignments;
        public Dictionary<string, Text> FacilityCardTexts;
        public Dictionary<string, Button> FacilityUpgradeButtons;
        public Dictionary<string, Button> FacilityCollectButtons;
        public Dictionary<string, Text> AssignmentRowTexts;
        public Dictionary<string, List<Text>> AssignmentSlotTexts;
    }

    public sealed class FacilityCardViewState
    {
        public string Text;
        public bool CollectInteractable;
        public string CollectText;
        public Color CollectColor;
        public bool UpgradeInteractable;
        public string UpgradeText;
        public Color UpgradeColor;
    }

    public sealed class FacilityAssignmentSlotViewState
    {
        public string Text;
        public Color TextColor;
        public Color CardColor;
    }

    public static class FacilityView
    {
        public static FacilityRewardPopupRefs BuildRewardPopup(Transform parent, Action onClose)
        {
            if (parent == null)
            {
                throw new ArgumentException("Facility reward popup parent is required.", nameof(parent));
            }

            var refs = new FacilityRewardPopupRefs();
            refs.Popup = HudUiFactory.CreatePanel("FacilityRewardPopup", parent, new Color(0f, 0f, 0f, 0.64f));
            LayoutElement overlayLayout = refs.Popup.AddComponent<LayoutElement>();
            overlayLayout.ignoreLayout = true;
            HudUiFactory.StretchToParent(refs.Popup);

            GameObject dialog = HudUiFactory.CreatePanel("FacilityRewardDialog", refs.Popup.transform, new Color(0.22f, 0.29f, 0.42f, 1f));
            RectTransform dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(700f, 520f);
            dialogRect.anchoredPosition = Vector2.zero;

            Text title = HudUiFactory.CreateText("FacilityRewardTitle", dialog.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 68f);
            titleRect.anchoredPosition = new Vector2(0f, -22f);
            title.text = "시설 보상 획득";

            GameObject listPanel = HudUiFactory.CreatePanel("FacilityRewardListPanel", dialog.transform, new Color(0.14f, 0.18f, 0.27f, 1f));
            RectTransform listRect = listPanel.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0f);
            listRect.anchorMax = new Vector2(1f, 1f);
            listRect.offsetMin = new Vector2(44f, 118f);
            listRect.offsetMax = new Vector2(-44f, -104f);

            refs.ListText = HudUiFactory.CreateText("FacilityRewardListText", listPanel.transform, 29, FontStyle.Bold, TextAnchor.MiddleLeft);
            RectTransform listTextRect = refs.ListText.GetComponent<RectTransform>();
            listTextRect.anchorMin = Vector2.zero;
            listTextRect.anchorMax = Vector2.one;
            listTextRect.offsetMin = new Vector2(28f, 18f);
            listTextRect.offsetMax = new Vector2(-28f, -18f);
            refs.ListText.resizeTextForBestFit = true;
            refs.ListText.resizeTextMinSize = 20;
            refs.ListText.resizeTextMaxSize = 29;

            Button confirmButton = HudUiFactory.CreateButton("확인", dialog.transform, 30, new Color(0.46f, 0.68f, 0.24f, 1f));
            RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.5f, 0f);
            confirmRect.anchorMax = new Vector2(0.5f, 0f);
            confirmRect.pivot = new Vector2(0.5f, 0f);
            confirmRect.sizeDelta = new Vector2(320f, 72f);
            confirmRect.anchoredPosition = new Vector2(0f, 30f);
            confirmButton.onClick.AddListener(() => onClose?.Invoke());

            refs.Popup.SetActive(false);
            return refs;
        }

        public static FacilityViewRefs Build(FacilityViewBuildArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Parent == null)
            {
                throw new ArgumentException("Facility view parent is required.", nameof(args));
            }

            var refs = new FacilityViewRefs
            {
                Content = args.Parent.gameObject
            };

            Image facilityImage = refs.Content.GetComponent<Image>();
            if (facilityImage != null)
            {
                facilityImage.color = new Color(0.22f, 0.29f, 0.42f, 1f);
            }

            VerticalLayoutGroup layout = refs.Content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            refs.SummaryText = HudUiFactory.CreateText("HeroFacilitySummary", refs.Content.transform, 25, FontStyle.Bold, TextAnchor.MiddleLeft);
            HudUiFactory.AddLayoutElement(refs.SummaryText.gameObject, -1, 46);

            CreateFacilityList(args, refs.Content.transform);
            CreateActionRow(args, refs.Content.transform);

            refs.NoticeText = HudUiFactory.CreateText("HeroFacilityNotice", refs.Content.transform, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            refs.NoticeText.color = new Color(1f, 0.82f, 0.40f, 1f);
            HudUiFactory.AddLayoutElement(refs.NoticeText.gameObject, -1, 36);

            refs.AssignmentModal = CreateAssignmentModal(args, refs.Content.transform);
            refs.Content.SetActive(false);
            return refs;
        }

        public static void ApplyFacilityCardState(
            string facilityId,
            FacilityCardViewState state,
            Dictionary<string, Text> cardTexts,
            Dictionary<string, Button> collectButtons,
            Dictionary<string, Button> upgradeButtons)
        {
            if (string.IsNullOrEmpty(facilityId) || state == null)
            {
                return;
            }

            if (cardTexts != null && cardTexts.TryGetValue(facilityId, out Text text) && text != null)
            {
                text.text = state.Text ?? string.Empty;
            }

            if (collectButtons != null && collectButtons.TryGetValue(facilityId, out Button collectButton) && collectButton != null)
            {
                collectButton.interactable = state.CollectInteractable;
                SetButtonText(collectButton, state.CollectText);
                HudUiFactory.SetButtonColor(collectButton, state.CollectColor);
            }

            if (upgradeButtons != null && upgradeButtons.TryGetValue(facilityId, out Button upgradeButton) && upgradeButton != null)
            {
                upgradeButton.interactable = state.UpgradeInteractable;
                SetButtonText(upgradeButton, state.UpgradeText);
                HudUiFactory.SetButtonColor(upgradeButton, state.UpgradeColor);
            }
        }

        public static void SetAssignmentModalOpen(GameObject modal, bool open)
        {
            if (modal != null)
            {
                modal.SetActive(open);
            }
        }

        public static void ApplyAssignmentRowState(string facilityId, string text, Dictionary<string, Text> rowTexts)
        {
            if (rowTexts != null && rowTexts.TryGetValue(facilityId, out Text rowText) && rowText != null)
            {
                rowText.text = text ?? string.Empty;
            }
        }

        public static void ApplyAssignmentSlotState(string facilityId, int slot, FacilityAssignmentSlotViewState state, Dictionary<string, List<Text>> slotTexts)
        {
            if (state == null
                || slotTexts == null
                || !slotTexts.TryGetValue(facilityId, out List<Text> texts)
                || texts == null
                || slot < 0
                || slot >= texts.Count)
            {
                return;
            }

            Text slotText = texts[slot];
            if (slotText == null)
            {
                return;
            }

            slotText.text = state.Text ?? string.Empty;
            slotText.color = state.TextColor;

            Image slotImage = slotText.transform.parent != null ? slotText.transform.parent.GetComponent<Image>() : null;
            if (slotImage != null)
            {
                slotImage.color = state.CardColor;
            }
        }

        private static void CreateFacilityList(FacilityViewBuildArgs args, Transform parent)
        {
            GameObject scrollPanel = HudUiFactory.CreatePanel("HeroFacilityScroll", parent, new Color(0.15f, 0.20f, 0.30f, 1f));
            HudUiFactory.AddLayoutElement(scrollPanel, -1, 552);
            ScrollRect scrollRect = scrollPanel.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 34f;

            GameObject viewport = HudUiFactory.CreatePanel("HeroFacilityViewport", scrollPanel.transform, new Color(0f, 0f, 0f, 0f));
            HudUiFactory.StretchToParent(viewport);
            Image viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.raycastTarget = true;
            }

            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            GameObject content = new GameObject("HeroFacilityList", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = new Vector2(8f, 0f);
            contentRect.offsetMax = new Vector2(-8f, 0f);
            scrollRect.content = contentRect;

            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 8, 8);
            contentLayout.spacing = 8;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            IReadOnlyList<FacilityDefinition> facilities = args.Facilities ?? Array.Empty<FacilityDefinition>();
            foreach (FacilityDefinition facility in facilities)
            {
                CreateFacilityCard(args, content.transform, facility);
            }
        }

        private static void CreateFacilityCard(FacilityViewBuildArgs args, Transform parent, FacilityDefinition facility)
        {
            GameObject card = HudUiFactory.CreatePanel(facility.Id + "FacilityCard", parent, new Color(0.27f, 0.35f, 0.50f, 1f));
            HudUiFactory.AddLayoutElement(card, -1, 132);
            HorizontalLayoutGroup cardLayout = card.AddComponent<HorizontalLayoutGroup>();
            cardLayout.padding = new RectOffset(14, 12, 10, 10);
            cardLayout.spacing = 10;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = true;

            Text text = HudUiFactory.CreateText(facility.Id + "FacilityText", card.transform, 21, FontStyle.Bold, TextAnchor.MiddleLeft);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 13;
            text.resizeTextMaxSize = 21;
            HudUiFactory.AddLayoutElement(text.gameObject, 520, -1);
            args.FacilityCardTexts[facility.Id] = text;

            GameObject buttonColumn = new GameObject(facility.Id + "FacilityActions", typeof(RectTransform));
            buttonColumn.transform.SetParent(card.transform, false);
            VerticalLayoutGroup buttonLayout = buttonColumn.AddComponent<VerticalLayoutGroup>();
            buttonLayout.spacing = 8;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(buttonColumn, 230, -1);

            string facilityId = facility.Id;
            Button collectButton = HudUiFactory.CreateButton("수령", buttonColumn.transform, 21, new Color(0.45f, 0.62f, 0.22f, 1f));
            collectButton.onClick.AddListener(() => args.OnCollectFacility?.Invoke(facilityId));
            args.FacilityCollectButtons[facility.Id] = collectButton;

            Button upgradeButton = HudUiFactory.CreateButton("업그레이드", buttonColumn.transform, 19, new Color(0.76f, 0.48f, 0.16f, 1f));
            upgradeButton.onClick.AddListener(() => args.OnUpgradeFacility?.Invoke(facilityId));
            args.FacilityUpgradeButtons[facility.Id] = upgradeButton;
        }

        private static void CreateActionRow(FacilityViewBuildArgs args, Transform parent)
        {
            GameObject actionRow = new GameObject("HeroFacilityActions", typeof(RectTransform));
            actionRow.transform.SetParent(parent, false);
            HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 10;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = true;
            HudUiFactory.AddLayoutElement(actionRow, -1, 68);

            Button assignmentButton = HudUiFactory.CreateButton("배치 인력", actionRow.transform, 26, new Color(0.74f, 0.56f, 0.18f, 1f));
            assignmentButton.onClick.AddListener(() => args.OnOpenAssignments?.Invoke());

            Button collectAllButton = HudUiFactory.CreateButton("모두 획득", actionRow.transform, 26, new Color(0.42f, 0.68f, 0.22f, 1f));
            collectAllButton.onClick.AddListener(() => args.OnCollectAll?.Invoke());
        }

        private static GameObject CreateAssignmentModal(FacilityViewBuildArgs args, Transform parent)
        {
            GameObject modal = HudUiFactory.CreatePanel("FacilityAssignmentModal", parent, new Color(0f, 0f, 0f, 0.62f));
            LayoutElement overlayLayout = modal.AddComponent<LayoutElement>();
            overlayLayout.ignoreLayout = true;
            HudUiFactory.StretchToParent(modal);

            GameObject dialog = HudUiFactory.CreatePanel("FacilityAssignmentDialog", modal.transform, new Color(0.22f, 0.28f, 0.38f, 1f));
            RectTransform dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(920f, 660f);
            dialogRect.anchoredPosition = Vector2.zero;

            Text title = HudUiFactory.CreateText("FacilityAssignmentTitle", dialog.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 66f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            title.text = "배치 인력";

            Button closeButton = HudUiFactory.CreateButton("X", dialog.transform, 24, new Color(0.36f, 0.14f, 0.14f, 1f));
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(54f, 48f);
            closeRect.anchoredPosition = new Vector2(-16f, -16f);
            closeButton.onClick.AddListener(() => args.OnCloseAssignments?.Invoke());

            GameObject rowPanel = HudUiFactory.CreatePanel("FacilityAssignmentRows", dialog.transform, new Color(0.15f, 0.18f, 0.24f, 1f));
            RectTransform rowPanelRect = rowPanel.GetComponent<RectTransform>();
            rowPanelRect.anchorMin = new Vector2(0f, 0f);
            rowPanelRect.anchorMax = new Vector2(1f, 1f);
            rowPanelRect.offsetMin = new Vector2(34f, 108f);
            rowPanelRect.offsetMax = new Vector2(-34f, -92f);
            VerticalLayoutGroup rowLayout = rowPanel.AddComponent<VerticalLayoutGroup>();
            rowLayout.padding = new RectOffset(10, 10, 10, 10);
            rowLayout.spacing = 8;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            IReadOnlyList<FacilityDefinition> facilities = args.Facilities ?? Array.Empty<FacilityDefinition>();
            foreach (FacilityDefinition facility in facilities)
            {
                CreateAssignmentRow(args, rowPanel.transform, facility);
            }

            GameObject actionRow = new GameObject("FacilityAssignmentActions", typeof(RectTransform));
            actionRow.transform.SetParent(dialog.transform, false);
            RectTransform actionRect = actionRow.GetComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0f, 0f);
            actionRect.anchorMax = new Vector2(1f, 0f);
            actionRect.pivot = new Vector2(0.5f, 0f);
            actionRect.sizeDelta = new Vector2(-80f, 68f);
            actionRect.anchoredPosition = new Vector2(0f, 26f);
            HorizontalLayoutGroup actions = actionRow.AddComponent<HorizontalLayoutGroup>();
            actions.spacing = 18;
            actions.childControlWidth = true;
            actions.childControlHeight = true;
            actions.childForceExpandWidth = true;
            actions.childForceExpandHeight = true;

            Button autoButton = HudUiFactory.CreateButton("추천 배치", actionRow.transform, 26, new Color(0.78f, 0.58f, 0.18f, 1f));
            autoButton.onClick.AddListener(() => args.OnAutoAssignAll?.Invoke());
            Button clearButton = HudUiFactory.CreateButton("모두 해제", actionRow.transform, 26, new Color(0.72f, 0.24f, 0.14f, 1f));
            clearButton.onClick.AddListener(() => args.OnClearAssignments?.Invoke());

            modal.SetActive(false);
            return modal;
        }

        private static void CreateAssignmentRow(FacilityViewBuildArgs args, Transform parent, FacilityDefinition facility)
        {
            GameObject row = HudUiFactory.CreatePanel(facility.Id + "AssignmentRow", parent, new Color(0.28f, 0.22f, 0.14f, 1f));
            HorizontalLayoutGroup rowCardLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowCardLayout.padding = new RectOffset(12, 12, 8, 8);
            rowCardLayout.spacing = 8;
            rowCardLayout.childControlWidth = true;
            rowCardLayout.childControlHeight = true;
            rowCardLayout.childForceExpandWidth = false;
            rowCardLayout.childForceExpandHeight = true;

            Text rowText = HudUiFactory.CreateText(facility.Id + "AssignmentText", row.transform, 20, FontStyle.Bold, TextAnchor.MiddleLeft);
            rowText.resizeTextForBestFit = true;
            rowText.resizeTextMinSize = 12;
            rowText.resizeTextMaxSize = 20;
            HudUiFactory.AddLayoutElement(rowText.gameObject, 166, -1);
            args.AssignmentRowTexts[facility.Id] = rowText;

            var slotTexts = new List<Text>();
            for (int slot = 0; slot < FacilityDefinition.MaxAssignedHeroSlots; slot++)
            {
                GameObject slotCard = HudUiFactory.CreatePanel(facility.Id + "AssignmentSlotCard" + slot, row.transform, new Color(0.16f, 0.20f, 0.30f, 1f));
                HudUiFactory.AddLayoutElement(slotCard, 118, -1);

                Text slotText = HudUiFactory.CreateText(facility.Id + "AssignmentSlot" + slot, slotCard.transform, 17, FontStyle.Bold, TextAnchor.MiddleCenter);
                HudUiFactory.StretchToParent(slotText.gameObject);
                slotText.resizeTextForBestFit = true;
                slotText.resizeTextMinSize = 10;
                slotText.resizeTextMaxSize = 17;
                slotText.color = Color.white;
                slotTexts.Add(slotText);
            }

            args.AssignmentSlotTexts[facility.Id] = slotTexts;
        }

        private static void SetButtonText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }
    }
}
