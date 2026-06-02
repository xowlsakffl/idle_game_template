using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail
{
    public static partial class HeroDetailView
    {
        private static void BuildTranscendConfirmPrompt(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.TranscendConfirmPrompt = HudUiFactory.CreatePanel("HeroTranscendConfirmPrompt", refs.Panel.transform, new Color(0.01f, 0.015f, 0.025f, 0.66f));
            HudUiFactory.StretchToParent(refs.TranscendConfirmPrompt);
            GameObject dialog = HudUiFactory.CreatePanel("HeroTranscendConfirmDialog", refs.TranscendConfirmPrompt.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
            SetAnchored(dialog, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.64f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Text title = CreateTextAnchored("HeroTranscendConfirmTitle", dialog.transform, 32, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 64f), new Vector2(0f, -22f));
            title.text = "SS 옵션 변경 확인";
            refs.TranscendConfirmMessageText = CreateTextAnchored("HeroTranscendConfirmMessage", dialog.transform, 26, TextAnchor.MiddleCenter, new Vector2(0.06f, 0.38f), new Vector2(0.94f, 0.76f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            GameObject row = CreateActionRow("HeroTranscendConfirmActions", dialog.transform, new Vector2(0.12f, 0f), new Vector2(0.88f, 0f), new Vector2(0f, 74f), new Vector2(0f, 24f), 18);
            Button confirmButton = HudUiFactory.CreateButton("확인", row.transform, 28, new Color(0.54f, 0.76f, 0.96f, 1f));
            Button cancelButton = HudUiFactory.CreateButton("취소", row.transform, 28, new Color(0.26f, 0.29f, 0.34f, 1f));
            confirmButton.onClick.AddListener(() => args.OnConfirmTranscendRollPrompt?.Invoke());
            cancelButton.onClick.AddListener(() => args.OnCancelTranscendRollPrompt?.Invoke());
            refs.TranscendConfirmPrompt.SetActive(false);
        }

        private static void BuildEquipmentDetailPopup(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.EquipmentDetailPopup = HudUiFactory.CreatePanel("EquipmentDetailPopup", refs.Panel.transform, new Color(0.01f, 0.015f, 0.025f, 0.72f));
            HudUiFactory.StretchToParent(refs.EquipmentDetailPopup);
            GameObject modal = HudUiFactory.CreatePanel("EquipmentDetailModal", refs.EquipmentDetailPopup.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
            SetAnchored(modal, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            refs.EquipmentDetailIconText = CreateTextAnchored("EquipmentDetailIcon", modal.transform, 25, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(168f, 150f), new Vector2(34f, -34f));
            refs.EquipmentDetailMetaText = CreateTextAnchored("EquipmentDetailMeta", modal.transform, 25, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(-260f, 62f), new Vector2(220f, -42f));
            refs.EquipmentDetailTitleText = CreateTextAnchored("EquipmentDetailTitle", modal.transform, 36, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(-260f, 92f), new Vector2(220f, -112f));

            GameObject statsPanel = HudUiFactory.CreatePanel("EquipmentDetailStats", modal.transform, new Color(0.30f, 0.38f, 0.54f, 1f));
            SetAnchored(statsPanel, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 420f), new Vector2(0f, -215f));
            refs.EquipmentDetailStatsText = HudUiFactory.CreateText("EquipmentDetailStatsText", statsPanel.transform, 30, FontStyle.Bold, TextAnchor.UpperLeft);
            SetStretchOffsets(refs.EquipmentDetailStatsText.gameObject, new Vector2(28f, 240f), new Vector2(-28f, -28f));
            refs.EquipmentDetailSetText = HudUiFactory.CreateText("EquipmentDetailSetText", statsPanel.transform, 26, FontStyle.Bold, TextAnchor.UpperLeft);
            SetStretchOffsets(refs.EquipmentDetailSetText.gameObject, new Vector2(28f, 28f), new Vector2(-28f, -170f));
            refs.EquipmentDetailBookText = CreateTextAnchored("EquipmentDetailBook", modal.transform, 30, TextAnchor.MiddleRight, new Vector2(0.45f, 0f), new Vector2(0.95f, 0f), new Vector2(1f, 0f), new Vector2(0f, 64f), new Vector2(0f, 136f));
            refs.EquipmentDetailNoticeText = CreateTextAnchored("EquipmentDetailNotice", modal.transform, 23, TextAnchor.MiddleCenter, new Vector2(0.06f, 0f), new Vector2(0.94f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(0f, 103f));
            refs.EquipmentDetailNoticeText.color = new Color(1f, 0.62f, 0.34f, 1f);

            GameObject row = CreateActionRow("EquipmentDetailActions", modal.transform, new Vector2(0.04f, 0f), new Vector2(0.96f, 0f), new Vector2(0f, 82f), new Vector2(0f, 26f), 18);
            refs.EquipmentDetailEquipButton = HudUiFactory.CreateButton("장착", row.transform, 27, new Color(0.54f, 0.76f, 0.96f, 1f));
            refs.EquipmentDetailLevelUpButton = HudUiFactory.CreateButton("레벨업", row.transform, 24, new Color(0.54f, 0.78f, 0.22f, 1f));
            refs.EquipmentDetailStarUpButton = HudUiFactory.CreateButton("승급", row.transform, 27, new Color(0.88f, 0.62f, 0.16f, 1f));
            refs.EquipmentDetailEquipButton.onClick.AddListener(() => args.OnToggleSelectedEquipmentDetailEquip?.Invoke());
            HudUiFactory.ConfigureHoldRepeat(refs.EquipmentDetailLevelUpButton, args.OnLevelUpSelectedEquipmentDetail, args.CanLevelUpSelectedEquipmentDetail);
            refs.EquipmentDetailStarUpButton.onClick.AddListener(() => args.OnStarUpSelectedEquipmentDetail?.Invoke());

            Button closeButton = HudUiFactory.CreateButton("X", refs.EquipmentDetailPopup.transform, 40, new Color(0.20f, 0.28f, 0.43f, 1f));
            SetAnchored(closeButton.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(112f, 86f), new Vector2(0f, 14f));
            closeButton.onClick.AddListener(() => args.OnCloseEquipmentDetailPopup?.Invoke());
            refs.EquipmentDetailPopup.SetActive(false);
        }

        private static void BuildEquipmentDismantlePopup(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.EquipmentDismantlePopup = HudUiFactory.CreatePanel("EquipmentDismantlePopup", refs.Panel.transform, new Color(0.01f, 0.015f, 0.025f, 0.72f));
            HudUiFactory.StretchToParent(refs.EquipmentDismantlePopup);
            GameObject modal = HudUiFactory.CreatePanel("EquipmentDismantleModal", refs.EquipmentDismantlePopup.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
            SetAnchored(modal, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject titleBar = HudUiFactory.CreatePanel("EquipmentDismantleTitleBar", modal.transform, new Color(0.25f, 0.37f, 0.63f, 1f));
            SetAnchored(titleBar, new Vector2(0.08f, 1f), new Vector2(0.92f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 72f), new Vector2(0f, 34f));
            Text titleText = HudUiFactory.CreateText("EquipmentDismantleTitle", titleBar.transform, 34, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(titleText.gameObject);
            titleText.text = "장비 분해";

            GameObject filterRow = HudUiFactory.CreatePanel("EquipmentDismantleFilters", modal.transform, new Color(0.30f, 0.38f, 0.54f, 1f));
            SetAnchored(filterRow, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 66f), new Vector2(0f, -72f));
            HorizontalLayoutGroup filterLayout = filterRow.AddComponent<HorizontalLayoutGroup>();
            filterLayout.padding = new RectOffset(12, 12, 6, 6);
            filterLayout.spacing = 8;
            filterLayout.childControlWidth = true;
            filterLayout.childControlHeight = true;
            filterLayout.childForceExpandWidth = true;
            filterLayout.childForceExpandHeight = true;
            foreach (EquipmentSlot slot in EquipmentUiText.FilterSlots)
            {
                Button button = HudUiFactory.CreateButton(EquipmentUiText.BuildFilterButtonLabel(slot, args.SelectedEquipmentSlots), filterRow.transform, 21, new Color(0.24f, 0.30f, 0.42f, 1f));
                button.onClick.AddListener(() => args.OnToggleEquipmentFilter?.Invoke(slot));
                args.DismantleFilterButtons[slot] = button;
            }

            refs.EquipmentDismantleSummaryText = CreateTextAnchored("EquipmentDismantleSummary", modal.transform, 23, TextAnchor.MiddleLeft, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 42f), new Vector2(0f, -146f));
            GameObject scrollObject = HudUiFactory.CreatePanel("EquipmentDismantleScroll", modal.transform, new Color(0.12f, 0.16f, 0.24f, 1f));
            SetAnchored(scrollObject, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetOffsets(scrollObject, new Vector2(0f, 166f), new Vector2(0f, -194f));
            ScrollRect scrollRect = ConfigureVerticalScroll(scrollObject, 34f);
            RectTransform gridRect = CreateGridViewport(scrollObject.transform, "EquipmentDismantle", new Vector2(810f, 0f), new Vector2(12f, -12f), new Vector2(146f, 126f), new Vector2(16f, 16f), 5, out Transform gridTransform);
            scrollRect.content = gridRect;
            refs.DismantleGridTransform = gridTransform;

            refs.EquipmentDismantleEmptyText = CreateTextAnchored("EquipmentDismantleEmpty", modal.transform, 27, TextAnchor.MiddleCenter, new Vector2(0.04f, 0.36f), new Vector2(0.96f, 0.50f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            refs.EquipmentDismantleEmptyText.text = "분해할 장비가 없습니다.";
            GameObject row = CreateActionRow("EquipmentDismantleActions", modal.transform, new Vector2(0.15f, 0f), new Vector2(0.85f, 0f), new Vector2(0f, 82f), new Vector2(0f, 42f), 24);
            refs.EquipmentDismantleButton = HudUiFactory.CreateButton("선택 분해", row.transform, 28, new Color(0.54f, 0.76f, 0.96f, 1f));
            refs.EquipmentBulkDismantleButton = HudUiFactory.CreateButton("일괄 분해", row.transform, 28, new Color(0.54f, 0.76f, 0.96f, 1f));
            refs.EquipmentDismantleButton.onClick.AddListener(() => args.OnDismantleSelectedEquipment?.Invoke());
            refs.EquipmentBulkDismantleButton.onClick.AddListener(() => args.OnOpenEquipmentBulkDismantlePrompt?.Invoke());
            refs.EquipmentDismantleNoticeText = CreateTextAnchored("EquipmentDismantleNotice", modal.transform, 22, TextAnchor.MiddleCenter, new Vector2(0.05f, 0f), new Vector2(0.95f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(0f, 126f));
            refs.EquipmentDismantleNoticeText.color = new Color(1f, 0.62f, 0.34f, 1f);
            Button closeButton = HudUiFactory.CreateButton("X", refs.EquipmentDismantlePopup.transform, 40, new Color(0.20f, 0.28f, 0.43f, 1f));
            SetAnchored(closeButton.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(112f, 86f), new Vector2(0f, 14f));
            closeButton.onClick.AddListener(() => args.OnCloseEquipmentDismantlePopup?.Invoke());
            BuildEquipmentBulkDismantlePrompt(args, refs);
            refs.EquipmentDismantlePopup.SetActive(false);
        }

        private static void BuildEquipmentBulkDismantlePrompt(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.EquipmentBulkDismantlePrompt = HudUiFactory.CreatePanel("EquipmentBulkDismantlePrompt", refs.EquipmentDismantlePopup.transform, new Color(0.01f, 0.015f, 0.025f, 0.62f));
            HudUiFactory.StretchToParent(refs.EquipmentBulkDismantlePrompt);
            GameObject modal = HudUiFactory.CreatePanel("EquipmentBulkDismantleModal", refs.EquipmentBulkDismantlePrompt.transform, new Color(0.39f, 0.48f, 0.66f, 1f));
            SetAnchored(modal, new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            GameObject titleBar = HudUiFactory.CreatePanel("EquipmentBulkDismantleTitleBar", modal.transform, new Color(0.25f, 0.37f, 0.63f, 1f));
            SetAnchored(titleBar, new Vector2(0.08f, 1f), new Vector2(0.92f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 68f), new Vector2(0f, 30f));
            Text title = HudUiFactory.CreateText("EquipmentBulkDismantleTitle", titleBar.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(title.gameObject);
            title.text = "장비 일괄 분해";
            Text category = CreateTextAnchored("EquipmentBulkDismantleCategory", modal.transform, 24, TextAnchor.MiddleCenter, new Vector2(0.30f, 1f), new Vector2(0.70f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 48f), new Vector2(0f, -52f));
            category.text = "품질";
            refs.EquipmentBulkDismantleInfoText = CreateTextAnchored("EquipmentBulkDismantleInfo", modal.transform, 30, TextAnchor.MiddleCenter, new Vector2(0.08f, 1f), new Vector2(0.92f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 150f), new Vector2(0f, -112f));
            Button leftButton = HudUiFactory.CreateButton("<", modal.transform, 42, new Color(0.20f, 0.28f, 0.43f, 1f));
            Button rightButton = HudUiFactory.CreateButton(">", modal.transform, 42, new Color(0.20f, 0.28f, 0.43f, 1f));
            SetAnchored(leftButton.gameObject, new Vector2(0.30f, 0.42f), new Vector2(0.30f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(82f, 82f), Vector2.zero);
            SetAnchored(rightButton.gameObject, new Vector2(0.70f, 0.42f), new Vector2(0.70f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(82f, 82f), Vector2.zero);
            leftButton.onClick.AddListener(() => args.OnChangeBulkDismantleRarity?.Invoke(-1));
            rightButton.onClick.AddListener(() => args.OnChangeBulkDismantleRarity?.Invoke(1));
            refs.EquipmentBulkDismantleRarityText = CreateTextAnchored("EquipmentBulkDismantleRarity", modal.transform, 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(260f, 70f), Vector2.zero);
            Text protectText = CreateTextAnchored("EquipmentBulkDismantleProtect", modal.transform, 25, TextAnchor.MiddleCenter, new Vector2(0.18f, 0f), new Vector2(0.82f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(0f, 140f));
            protectText.text = "[x] 장착 장비 제외";
            Button confirmButton = HudUiFactory.CreateButton("일괄 분해", modal.transform, 30, new Color(0.54f, 0.76f, 0.96f, 1f));
            SetAnchored(confirmButton.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(300f, 82f), new Vector2(0f, 56f));
            confirmButton.onClick.AddListener(() => args.OnConfirmBulkDismantleEquipment?.Invoke());
            refs.EquipmentBulkDismantleNoticeText = CreateTextAnchored("EquipmentBulkDismantleNotice", modal.transform, 22, TextAnchor.MiddleCenter, new Vector2(0.05f, 0f), new Vector2(0.95f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(0f, 18f));
            refs.EquipmentBulkDismantleNoticeText.color = new Color(1f, 0.62f, 0.34f, 1f);
            Button closeButton = HudUiFactory.CreateButton("X", refs.EquipmentBulkDismantlePrompt.transform, 40, new Color(0.20f, 0.28f, 0.43f, 1f));
            SetAnchored(closeButton.gameObject, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(112f, 86f), Vector2.zero);
            closeButton.onClick.AddListener(() => args.OnCloseEquipmentBulkDismantlePrompt?.Invoke());
            refs.EquipmentBulkDismantlePrompt.SetActive(false);
        }
    }
}
