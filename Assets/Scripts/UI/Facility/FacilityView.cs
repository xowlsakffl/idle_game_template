using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Facility
{
    public static partial class FacilityView
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

    }
}
