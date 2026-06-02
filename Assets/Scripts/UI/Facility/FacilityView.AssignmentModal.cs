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
    }
}
