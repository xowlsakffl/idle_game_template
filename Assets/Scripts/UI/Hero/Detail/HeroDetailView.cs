using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;
using IdleGame.UI.Hero.Transcend;

namespace IdleGame.UI.Hero.Detail
{
    public sealed class HeroDetailViewRefs
    {
        public GameObject Panel;
        public GameObject StatsPanel;
        public GameObject ActionRow;
        public GameObject EquipmentContent;
        public GameObject TranscendContent;
        public GameObject EquipmentDetailPopup;
        public GameObject EquipmentDismantlePopup;
        public GameObject EquipmentBulkDismantlePrompt;
        public GameObject TranscendConfirmPrompt;
        public Transform EquipmentGridTransform;
        public Transform DismantleGridTransform;
        public Text TitleText;
        public Text TraitText;
        public Text StarsText;
        public Text CharacterText;
        public Text LevelText;
        public Text PowerText;
        public Text ExpBookText;
        public Text SkillText;
        public Text StatsText;
        public Text StarEffectsText;
        public Text OwnedEffectText;
        public Text NoticeText;
        public Text EquipmentSummaryText;
        public Text EquipmentEmptyText;
        public Text TranscendText;
        public Text TranscendNoticeText;
        public Text TranscendConfirmMessageText;
        public Text EquipmentDetailIconText;
        public Text EquipmentDetailTitleText;
        public Text EquipmentDetailMetaText;
        public Text EquipmentDetailStatsText;
        public Text EquipmentDetailSetText;
        public Text EquipmentDetailBookText;
        public Text EquipmentDetailNoticeText;
        public Text EquipmentDismantleSummaryText;
        public Text EquipmentDismantleEmptyText;
        public Text EquipmentDismantleNoticeText;
        public Text EquipmentBulkDismantleRarityText;
        public Text EquipmentBulkDismantleInfoText;
        public Text EquipmentBulkDismantleNoticeText;
        public Button ExcludeButton;
        public Button LevelUpButton;
        public Button StarUpButton;
        public Button TranscendChangeButton;
        public Button TranscendAutoButton;
        public Button TranscendStopButton;
        public Button EquipmentDetailEquipButton;
        public Button EquipmentDetailLevelUpButton;
        public Button EquipmentDetailStarUpButton;
        public Button EquipmentDismantleButton;
        public Button EquipmentBulkDismantleButton;
    }

    public sealed class HeroDetailViewBuildArgs
    {
        public Transform Parent;
        public Action OnToggleFormation;
        public Action OnLevelUpHero;
        public Func<bool> CanLevelUpHero;
        public Action OnStarUpHero;
        public Action<EquipmentSlot> OnPlaceEquipmentSlot;
        public Action<EquipmentSlot> OnRemoveEquipmentSlot;
        public Action<EquipmentSlot> OnToggleEquipmentFilter;
        public Action OnOpenEquipmentDismantle;
        public Action OnUnequipAllEquipment;
        public Action OnAutoEquipEquipment;
        public Action<int> OnSelectTranscendSlot;
        public Action<int> OnToggleTranscendSlotLock;
        public Action OnToggleTranscendStopMode;
        public Action OnRollTranscendManual;
        public Action OnAutoRollTranscend;
        public Action<HeroDetailTab> OnSelectTab;
        public Action OnConfirmTranscendRollPrompt;
        public Action OnCancelTranscendRollPrompt;
        public Action OnToggleSelectedEquipmentDetailEquip;
        public Action OnLevelUpSelectedEquipmentDetail;
        public Func<bool> CanLevelUpSelectedEquipmentDetail;
        public Action OnStarUpSelectedEquipmentDetail;
        public Action OnCloseEquipmentDetailPopup;
        public Action OnDismantleSelectedEquipment;
        public Action OnOpenEquipmentBulkDismantlePrompt;
        public Action OnCloseEquipmentDismantlePopup;
        public Action<int> OnChangeBulkDismantleRarity;
        public Action OnConfirmBulkDismantleEquipment;
        public Action OnCloseEquipmentBulkDismantlePrompt;
        public IDictionary<HeroDetailTab, Button> TabButtons;
        public IDictionary<EquipmentSlot, Button> EquipmentSlotButtons;
        public IDictionary<EquipmentSlot, Text> EquipmentSlotTexts;
        public IDictionary<EquipmentSlot, Button> EquipmentSlotRemoveButtons;
        public IDictionary<EquipmentSlot, Button> EquipmentFilterButtons;
        public IDictionary<EquipmentSlot, Button> DismantleFilterButtons;
        public ICollection<EquipmentSlot> SelectedEquipmentSlots;
        public IList<Button> TranscendSlotButtons;
        public IList<Text> TranscendSlotTexts;
        public IList<Button> TranscendLockButtons;
    }

    public sealed class HeroDetailButtonViewState
    {
        public bool Interactable;
        public string Text;
        public Color Color;
    }

    public sealed class HeroDetailActionViewState
    {
        public HeroDetailButtonViewState Formation;
        public HeroDetailButtonViewState LevelUp;
        public HeroDetailButtonViewState StarUp;
    }

    public sealed class HeroDetailBasicViewState
    {
        public string TitleText;
        public string TraitText;
        public string StarsText;
        public string CharacterText;
        public Color CharacterColor;
        public string LevelText;
        public string PowerText;
        public string ResourceText;
        public string SkillText;
        public string StatsText;
        public string StarEffectsText;
        public string OwnedEffectText;
        public string NoticeText;
    }

    public sealed class HeroDetailEquipmentSlotViewState
    {
        public EquipmentSlot Slot;
        public string Text;
        public Color TextColor;
        public Color ButtonColor;
        public bool RemoveVisible;
    }

    public sealed class HeroDetailEquipmentDetailViewState
    {
        public string IconText;
        public Color IconColor;
        public string MetaText;
        public string TitleText;
        public string StatsText;
        public string SetText;
        public string BookText;
        public string NoticeText;
        public HeroDetailButtonViewState EquipButton;
        public HeroDetailButtonViewState LevelUpButton;
        public HeroDetailButtonViewState StarUpButton;
    }

    public sealed class HeroDetailEquipmentDismantleViewState
    {
        public string SummaryText;
        public bool EmptyVisible;
        public string NoticeText;
        public HeroDetailButtonViewState DismantleButton;
    }

    public sealed class HeroDetailEquipmentBulkDismantleViewState
    {
        public string InfoText;
        public string RarityText;
        public Color RarityColor;
        public string NoticeText;
    }

    public static class HeroDetailView
    {
        public static HeroDetailViewRefs Build(HeroDetailViewBuildArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args.Parent == null)
            {
                throw new ArgumentException("Hero detail parent is required.", nameof(args));
            }

            var refs = new HeroDetailViewRefs();
            refs.Panel = HudUiFactory.CreatePanel("HeroDetailPanel", args.Parent, new Color(0.04f, 0.06f, 0.12f, 0.96f));
            LayoutElement overlayLayout = refs.Panel.AddComponent<LayoutElement>();
            overlayLayout.ignoreLayout = true;
            HudUiFactory.StretchToParent(refs.Panel);
            refs.Panel.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 130f);

            BuildHeader(args, refs);
            BuildEquipmentSlots(args, refs);
            BuildSummaryAndBasicInfo(args, refs);
            BuildHeroActionButtons(args, refs);
            BuildEquipmentContent(args, refs);
            BuildTranscendContent(args, refs);
            BuildBottomTabs(args, refs);
            BuildTranscendConfirmPrompt(args, refs);
            BuildEquipmentDetailPopup(args, refs);
            BuildEquipmentDismantlePopup(args, refs);

            refs.Panel.SetActive(false);
            return refs;
        }

        public static void ApplyActionButtons(HeroDetailViewRefs refs, HeroDetailActionViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            ApplyButton(refs.ExcludeButton, state.Formation);
            ApplyButton(refs.LevelUpButton, state.LevelUp);
            ApplyButton(refs.StarUpButton, state.StarUp);
        }

        public static void ApplyBasicInfo(HeroDetailViewRefs refs, HeroDetailBasicViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            SetText(refs.TitleText, state.TitleText);
            SetText(refs.TraitText, state.TraitText);
            SetText(refs.StarsText, state.StarsText);
            SetText(refs.CharacterText, state.CharacterText);
            SetTextColor(refs.CharacterText, state.CharacterColor);
            SetText(refs.LevelText, state.LevelText);
            SetText(refs.PowerText, state.PowerText);
            SetText(refs.ExpBookText, state.ResourceText);
            SetText(refs.SkillText, state.SkillText);
            SetText(refs.StatsText, state.StatsText);
            SetText(refs.StarEffectsText, state.StarEffectsText);
            SetText(refs.OwnedEffectText, state.OwnedEffectText);
            SetText(refs.NoticeText, state.NoticeText);
        }

        public static void ApplyEquipmentSlotState(
            IDictionary<EquipmentSlot, Button> slotButtons,
            IDictionary<EquipmentSlot, Text> slotTexts,
            IDictionary<EquipmentSlot, Button> removeButtons,
            HeroDetailEquipmentSlotViewState state)
        {
            if (state == null)
            {
                return;
            }

            if (slotTexts != null && slotTexts.TryGetValue(state.Slot, out Text slotText) && slotText != null)
            {
                slotText.text = state.Text;
                slotText.color = state.TextColor;
            }

            if (slotButtons != null && slotButtons.TryGetValue(state.Slot, out Button slotButton))
            {
                HudUiFactory.SetButtonColor(slotButton, state.ButtonColor);
            }

            if (removeButtons != null && removeButtons.TryGetValue(state.Slot, out Button removeButton) && removeButton != null)
            {
                removeButton.gameObject.SetActive(state.RemoveVisible);
            }
        }

        public static void ApplyTabState(HeroDetailViewRefs refs, IDictionary<HeroDetailTab, Button> tabButtons, HeroDetailTab activeTab)
        {
            if (refs == null)
            {
                return;
            }

            bool basicInfoActive = activeTab == HeroDetailTab.BasicInfo;
            bool equipmentActive = activeTab == HeroDetailTab.Equipment;
            bool transcendActive = activeTab == HeroDetailTab.Transcend;

            SetActive(refs.SkillText, basicInfoActive);
            if (refs.StatsPanel != null)
            {
                refs.StatsPanel.SetActive(basicInfoActive);
            }

            SetActive(refs.OwnedEffectText, basicInfoActive);
            SetActive(refs.NoticeText, basicInfoActive || transcendActive);
            if (refs.ActionRow != null)
            {
                refs.ActionRow.SetActive(basicInfoActive);
            }

            if (refs.EquipmentContent != null)
            {
                refs.EquipmentContent.SetActive(equipmentActive);
            }

            if (refs.TranscendContent != null)
            {
                refs.TranscendContent.SetActive(transcendActive);
            }

            if (tabButtons == null)
            {
                return;
            }

            foreach (KeyValuePair<HeroDetailTab, Button> pair in tabButtons)
            {
                bool selected = pair.Key == activeTab;
                HudUiFactory.SetButtonColor(pair.Value, selected ? new Color(0.56f, 0.68f, 0.94f, 1f) : new Color(0.20f, 0.27f, 0.42f, 1f));
            }
        }

        public static void ApplyEquipmentFilterButtons(IDictionary<EquipmentSlot, Button> buttons, ICollection<EquipmentSlot> selectedSlots)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (KeyValuePair<EquipmentSlot, Button> pair in buttons)
            {
                bool selected = selectedSlots != null && selectedSlots.Contains(pair.Key);
                HudUiFactory.SetButtonText(pair.Value, EquipmentUiText.BuildFilterButtonLabel(pair.Key, selectedSlots));
                HudUiFactory.SetButtonColor(pair.Value, selected ? new Color(0.46f, 0.62f, 0.30f, 1f) : new Color(0.24f, 0.30f, 0.42f, 1f));
            }
        }

        public static void ApplyEquipmentDetailPopup(HeroDetailViewRefs refs, HeroDetailEquipmentDetailViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            SetText(refs.EquipmentDetailIconText, state.IconText);
            SetTextColor(refs.EquipmentDetailIconText, state.IconColor);
            SetText(refs.EquipmentDetailMetaText, state.MetaText);
            SetText(refs.EquipmentDetailTitleText, state.TitleText);
            SetText(refs.EquipmentDetailStatsText, state.StatsText);
            SetText(refs.EquipmentDetailSetText, state.SetText);
            SetText(refs.EquipmentDetailBookText, state.BookText);
            SetText(refs.EquipmentDetailNoticeText, state.NoticeText);
            ApplyButton(refs.EquipmentDetailEquipButton, state.EquipButton);
            ApplyButton(refs.EquipmentDetailLevelUpButton, state.LevelUpButton);
            ApplyButton(refs.EquipmentDetailStarUpButton, state.StarUpButton);
        }

        public static void ApplyEquipmentDismantlePopup(HeroDetailViewRefs refs, HeroDetailEquipmentDismantleViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            SetText(refs.EquipmentDismantleSummaryText, state.SummaryText);
            SetText(refs.EquipmentDismantleNoticeText, state.NoticeText);
            if (refs.EquipmentDismantleEmptyText != null)
            {
                refs.EquipmentDismantleEmptyText.gameObject.SetActive(state.EmptyVisible);
            }

            ApplyButton(refs.EquipmentDismantleButton, state.DismantleButton);
        }

        public static void ApplyEquipmentBulkDismantlePrompt(HeroDetailViewRefs refs, HeroDetailEquipmentBulkDismantleViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            SetText(refs.EquipmentBulkDismantleInfoText, state.InfoText);
            SetText(refs.EquipmentBulkDismantleRarityText, state.RarityText);
            SetTextColor(refs.EquipmentBulkDismantleRarityText, state.RarityColor);
            SetText(refs.EquipmentBulkDismantleNoticeText, state.NoticeText);
        }

        public static void ApplyTranscendContent(
            HeroDetailViewRefs refs,
            IList<Text> slotTexts,
            IList<Button> slotButtons,
            IList<Button> lockButtons,
            HeroDetailTranscendViewState state)
        {
            if (refs == null || state == null)
            {
                return;
            }

            SetText(refs.TranscendText, state.SummaryText);
            foreach (HeroDetailTranscendSlotViewState slotState in state.Slots)
            {
                int index = slotState.SlotIndex;
                if (slotTexts != null && index >= 0 && index < slotTexts.Count && slotTexts[index] != null)
                {
                    slotTexts[index].text = slotState.Text;
                }

                if (slotButtons != null && index >= 0 && index < slotButtons.Count && slotButtons[index] != null)
                {
                    HudUiFactory.SetButtonColor(slotButtons[index], slotState.ButtonColor);
                }

                if (lockButtons != null && index >= 0 && index < lockButtons.Count && lockButtons[index] != null)
                {
                    Button lockButton = lockButtons[index];
                    lockButton.gameObject.SetActive(slotState.LockVisible);
                    HudUiFactory.SetButtonText(lockButton, slotState.LockText);
                    HudUiFactory.SetButtonColor(lockButton, slotState.LockColor);
                }
            }

            ApplyButton(refs.TranscendStopButton, state.StopButton);
            ApplyButton(refs.TranscendChangeButton, state.ChangeButton);
            ApplyButton(refs.TranscendAutoButton, state.AutoButton);
        }

        private static void BuildHeader(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            GameObject header = HudUiFactory.CreatePanel("HeroDetailHeader", refs.Panel.transform, new Color(0.24f, 0.36f, 0.62f, 1f));
            SetAnchored(header, new Vector2(0.03f, 1f), new Vector2(0.97f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 74f), new Vector2(0f, -28f));

            refs.TitleText = HudUiFactory.CreateText("HeroDetailTitle", header.transform, 32, FontStyle.Bold, TextAnchor.MiddleCenter);
            HudUiFactory.StretchToParent(refs.TitleText.gameObject);

            refs.TraitText = CreateTextAnchored("HeroDetailTrait", refs.Panel.transform, 25, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, 96f), new Vector2(30f, -132f));
            refs.StarsText = CreateTextAnchored("HeroDetailStars", refs.Panel.transform, 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 52f), new Vector2(0f, -150f));
            refs.CharacterText = CreateTextAnchored("HeroDetailCharacter", refs.Panel.transform, 52, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 360f), new Vector2(0f, -222f));
        }

        private static void BuildEquipmentSlots(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            CreateEquipmentSlot(args, refs, EquipmentSlot.Weapon, new Vector2(170f, -300f));
            CreateEquipmentSlot(args, refs, EquipmentSlot.Armor, new Vector2(170f, -424f));
            CreateEquipmentSlot(args, refs, EquipmentSlot.Potion, new Vector2(170f, -548f));
            CreateEquipmentSlot(args, refs, EquipmentSlot.Hat, new Vector2(910f, -300f));
            CreateEquipmentSlot(args, refs, EquipmentSlot.Accessory, new Vector2(910f, -424f));
        }

        private static void BuildSummaryAndBasicInfo(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.LevelText = CreateTextAnchored("HeroDetailLevel", refs.Panel.transform, 27, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 52f), new Vector2(0f, -650f));

            GameObject summaryBar = HudUiFactory.CreatePanel("HeroDetailSummaryBar", refs.Panel.transform, new Color(0.39f, 0.50f, 0.67f, 1f));
            SetAnchored(summaryBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 82f), new Vector2(0f, -712f));
            refs.PowerText = CreateTextAnchored("HeroDetailPower", summaryBar.transform, 32, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0.55f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetOffsets(refs.PowerText.gameObject, new Vector2(28f, 0f), Vector2.zero);
            refs.ExpBookText = CreateTextAnchored("HeroDetailExpBook", summaryBar.transform, 28, TextAnchor.MiddleRight, new Vector2(0.55f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetOffsets(refs.ExpBookText.gameObject, Vector2.zero, new Vector2(-28f, 0f));

            refs.SkillText = CreateTextAnchored("HeroDetailSkill", refs.Panel.transform, 25, TextAnchor.UpperLeft, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 136f), new Vector2(0f, -820f));

            refs.StatsPanel = HudUiFactory.CreatePanel("HeroDetailStatsPanel", refs.Panel.transform, new Color(0.22f, 0.31f, 0.48f, 1f));
            SetAnchored(refs.StatsPanel, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 230f), new Vector2(0f, -972f));
            refs.StatsText = HudUiFactory.CreateText("HeroDetailStats", refs.StatsPanel.transform, 26, FontStyle.Bold, TextAnchor.UpperLeft);
            SetStretchOffsets(refs.StatsText.gameObject, new Vector2(24f, 22f), new Vector2(-24f, -20f));
            refs.StarEffectsText = HudUiFactory.CreateText("HeroDetailStarEffects", refs.StatsPanel.transform, 23, FontStyle.Bold, TextAnchor.LowerLeft);
            SetAnchored(refs.StarEffectsText.gameObject, Vector2.zero, new Vector2(1f, 0.48f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetOffsets(refs.StarEffectsText.gameObject, new Vector2(24f, 14f), new Vector2(-24f, -6f));

            refs.OwnedEffectText = CreateTextAnchored("HeroDetailOwnedEffect", refs.Panel.transform, 26, TextAnchor.MiddleLeft, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 54f), new Vector2(0f, -1222f));
            refs.NoticeText = CreateTextAnchored("HeroDetailNotice", refs.Panel.transform, 23, TextAnchor.MiddleCenter, new Vector2(0.05f, 0f), new Vector2(0.95f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(0f, 194f));
            refs.NoticeText.color = new Color(1f, 0.58f, 0.34f, 1f);
        }

        private static void BuildHeroActionButtons(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.ActionRow = HudUiFactory.CreatePanel("HeroDetailActionButtons", refs.Panel.transform, new Color(0.15f, 0.20f, 0.31f, 1f));
            SetAnchored(refs.ActionRow, new Vector2(0.06f, 0f), new Vector2(0.94f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 78f), new Vector2(0f, 112f));
            HorizontalLayoutGroup layout = refs.ActionRow.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 16;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            refs.ExcludeButton = HudUiFactory.CreateButton("제외", refs.ActionRow.transform, 27, new Color(0.54f, 0.76f, 0.96f, 1f));
            refs.LevelUpButton = HudUiFactory.CreateButton("레벨업", refs.ActionRow.transform, 23, new Color(0.34f, 0.36f, 0.34f, 1f));
            refs.StarUpButton = HudUiFactory.CreateButton("승급", refs.ActionRow.transform, 23, new Color(0.34f, 0.36f, 0.34f, 1f));
            refs.ExcludeButton.onClick.AddListener(() => args.OnToggleFormation?.Invoke());
            ConfigureHoldRepeat(refs.LevelUpButton, args.OnLevelUpHero, args.CanLevelUpHero);
            refs.StarUpButton.onClick.AddListener(() => args.OnStarUpHero?.Invoke());
        }

        private static void BuildEquipmentContent(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.EquipmentContent = HudUiFactory.CreatePanel("HeroDetailEquipmentContent", refs.Panel.transform, new Color(0.18f, 0.24f, 0.34f, 0.96f));
            SetAnchored(refs.EquipmentContent, new Vector2(0.03f, 1f), new Vector2(0.97f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 720f), new Vector2(0f, -805f));

            GameObject filterRow = new GameObject("HeroDetailEquipmentFilters", typeof(RectTransform));
            filterRow.transform.SetParent(refs.EquipmentContent.transform, false);
            SetAnchored(filterRow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 58f), new Vector2(0f, -12f));
            HorizontalLayoutGroup filterLayout = filterRow.AddComponent<HorizontalLayoutGroup>();
            filterLayout.padding = new RectOffset(10, 10, 0, 0);
            filterLayout.spacing = 8;
            filterLayout.childControlWidth = true;
            filterLayout.childControlHeight = true;
            filterLayout.childForceExpandWidth = true;
            filterLayout.childForceExpandHeight = true;

            foreach (EquipmentSlot slot in EquipmentUiText.FilterSlots)
            {
                Button button = HudUiFactory.CreateButton(EquipmentUiText.BuildFilterButtonLabel(slot, args.SelectedEquipmentSlots), filterRow.transform, 22, new Color(0.24f, 0.30f, 0.42f, 1f));
                button.onClick.AddListener(() => args.OnToggleEquipmentFilter?.Invoke(slot));
                args.EquipmentFilterButtons[slot] = button;
            }

            refs.EquipmentSummaryText = CreateTextAnchored("HeroDetailEquipmentSummary", refs.EquipmentContent.transform, 24, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-28f, 42f), new Vector2(0f, -82f));
            GameObject scrollObject = HudUiFactory.CreatePanel("HeroDetailEquipmentScroll", refs.EquipmentContent.transform, new Color(0.13f, 0.17f, 0.25f, 1f));
            SetAnchored(scrollObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-18f, 438f), new Vector2(0f, -130f));
            ScrollRect scrollRect = ConfigureVerticalScroll(scrollObject, 34f);
            RectTransform gridRect = CreateGridViewport(scrollObject.transform, "HeroDetailEquipment", new Vector2(950f, 0f), new Vector2(10f, -10f), new Vector2(176f, 128f), new Vector2(10f, 10f), 5, out Transform gridTransform);
            scrollRect.content = gridRect;
            refs.EquipmentGridTransform = gridTransform;

            refs.EquipmentEmptyText = CreateTextAnchored("HeroDetailEquipmentEmpty", refs.EquipmentContent.transform, 28, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 180f), new Vector2(0f, -260f));

            GameObject actionRow = new GameObject("HeroDetailEquipmentActions", typeof(RectTransform));
            actionRow.transform.SetParent(refs.EquipmentContent.transform, false);
            SetAnchored(actionRow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-34f, 72f), new Vector2(0f, 24f));
            HorizontalLayoutGroup actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 14;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = true;
            actionLayout.childForceExpandHeight = true;

            Button dismantleButton = HudUiFactory.CreateButton("장비 분해", actionRow.transform, 24, new Color(0.20f, 0.26f, 0.38f, 1f));
            Button bulkUnequipButton = HudUiFactory.CreateButton("일괄 해제", actionRow.transform, 24, new Color(0.44f, 0.58f, 0.76f, 1f));
            Button bulkEquipButton = HudUiFactory.CreateButton("일괄 장착", actionRow.transform, 24, new Color(0.54f, 0.78f, 0.22f, 1f));
            dismantleButton.onClick.AddListener(() => args.OnOpenEquipmentDismantle?.Invoke());
            bulkUnequipButton.onClick.AddListener(() => args.OnUnequipAllEquipment?.Invoke());
            bulkEquipButton.onClick.AddListener(() => args.OnAutoEquipEquipment?.Invoke());
            refs.EquipmentContent.SetActive(false);
        }

        private static void BuildTranscendContent(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            refs.TranscendContent = HudUiFactory.CreatePanel("HeroDetailTranscendContent", refs.Panel.transform, new Color(0.18f, 0.23f, 0.34f, 0.96f));
            SetAnchored(refs.TranscendContent, new Vector2(0.03f, 1f), new Vector2(0.97f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 720f), new Vector2(0f, -805f));

            refs.TranscendText = CreateTextAnchored("HeroDetailTranscendText", refs.TranscendContent.transform, 25, TextAnchor.MiddleLeft, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 58f), new Vector2(0f, -18f));
            refs.TranscendText.text = "초월은 성급에 따라 슬롯이 열립니다.";

            for (int i = 0; i < HeroDefinition.MaxTranscendSlots; i++)
            {
                int slotIndex = i;
                Button slotButton = HudUiFactory.CreateButton(string.Empty, refs.TranscendContent.transform, 24, new Color(0.22f, 0.29f, 0.43f, 1f));
                SetAnchored(slotButton.gameObject, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 78f), new Vector2(0f, -82f - i * 84f));
                slotButton.onClick.AddListener(() => args.OnSelectTranscendSlot?.Invoke(slotIndex));
                Text slotText = slotButton.GetComponentInChildren<Text>(true);
                if (slotText != null)
                {
                    slotText.alignment = TextAnchor.MiddleLeft;
                    SetOffsets(slotText.gameObject, new Vector2(22f, 6f), new Vector2(-22f, -6f));
                    args.TranscendSlotTexts.Add(slotText);
                }

                args.TranscendSlotButtons.Add(slotButton);
                Button lockButton = CreateCornerActionButton("잠", slotButton.transform, new Color(0.20f, 0.25f, 0.36f, 1f));
                lockButton.onClick.AddListener(() => args.OnToggleTranscendSlotLock?.Invoke(slotIndex));
                args.TranscendLockButtons.Add(lockButton);
            }

            refs.TranscendStopButton = HudUiFactory.CreateButton("[x] 자동 변경시 SS만 정지", refs.TranscendContent.transform, 25, new Color(0.26f, 0.32f, 0.43f, 1f));
            SetAnchored(refs.TranscendStopButton.gameObject, new Vector2(0.18f, 0f), new Vector2(0.82f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 56f), new Vector2(0f, 122f));
            refs.TranscendStopButton.onClick.AddListener(() => args.OnToggleTranscendStopMode?.Invoke());

            GameObject actionRow = CreateActionRow("HeroDetailTranscendActions", refs.TranscendContent.transform, new Vector2(0.04f, 0f), new Vector2(0.96f, 0f), new Vector2(0f, 82f), new Vector2(0f, 28f), 18);
            refs.TranscendChangeButton = HudUiFactory.CreateButton("변경\n10", actionRow.transform, 25, new Color(0.28f, 0.72f, 0.92f, 1f));
            refs.TranscendAutoButton = HudUiFactory.CreateButton("자동 변경", actionRow.transform, 25, new Color(0.70f, 0.24f, 0.82f, 1f));
            refs.TranscendChangeButton.onClick.AddListener(() => args.OnRollTranscendManual?.Invoke());
            refs.TranscendAutoButton.onClick.AddListener(() => args.OnAutoRollTranscend?.Invoke());

            refs.TranscendNoticeText = CreateTextAnchored("HeroDetailTranscendNotice", refs.TranscendContent.transform, 22, TextAnchor.MiddleCenter, new Vector2(0.04f, 0f), new Vector2(0.96f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(0f, 92f));
            refs.TranscendNoticeText.color = new Color(1f, 0.64f, 0.34f, 1f);
            refs.TranscendContent.SetActive(false);
        }

        private static void BuildBottomTabs(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs)
        {
            GameObject tabRow = HudUiFactory.CreatePanel("HeroDetailBottomTabs", refs.Panel.transform, new Color(0.12f, 0.18f, 0.30f, 1f));
            SetAnchored(tabRow, new Vector2(0.03f, 0f), new Vector2(0.97f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 82f), new Vector2(0f, 20f));
            HorizontalLayoutGroup tabLayout = tabRow.AddComponent<HorizontalLayoutGroup>();
            tabLayout.padding = new RectOffset(8, 8, 8, 8);
            tabLayout.spacing = 10;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            CreateTabButton(args, tabRow.transform, HeroDetailTab.BasicInfo, "기본 정보");
            CreateTabButton(args, tabRow.transform, HeroDetailTab.Equipment, "장비");
            CreateTabButton(args, tabRow.transform, HeroDetailTab.Transcend, "초월");
        }

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
            ConfigureHoldRepeat(refs.EquipmentDetailLevelUpButton, args.OnLevelUpSelectedEquipmentDetail, args.CanLevelUpSelectedEquipmentDetail);
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

        private static void CreateEquipmentSlot(HeroDetailViewBuildArgs args, HeroDetailViewRefs refs, EquipmentSlot equipmentSlot, Vector2 anchoredPosition)
        {
            string label = EquipmentUiText.GetSlotLabel(equipmentSlot);
            Button slot = HudUiFactory.CreateButton("+\n" + label, refs.Panel.transform, 22, new Color(0.28f, 0.18f, 0.29f, 0.88f));
            SetAnchored(slot.gameObject, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(126f, 96f), anchoredPosition);
            slot.onClick.AddListener(() => args.OnPlaceEquipmentSlot?.Invoke(equipmentSlot));
            args.EquipmentSlotButtons[equipmentSlot] = slot;
            Text text = slot.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                args.EquipmentSlotTexts[equipmentSlot] = text;
            }

            Button removeButton = CreateCornerActionButton("-", slot.transform, new Color(0.58f, 0.12f, 0.12f, 1f));
            removeButton.onClick.AddListener(() => args.OnRemoveEquipmentSlot?.Invoke(equipmentSlot));
            args.EquipmentSlotRemoveButtons[equipmentSlot] = removeButton;
        }

        private static void CreateTabButton(HeroDetailViewBuildArgs args, Transform parent, HeroDetailTab tab, string label)
        {
            Button button = HudUiFactory.CreateButton(label, parent, 25, new Color(0.20f, 0.27f, 0.42f, 1f));
            button.onClick.AddListener(() => args.OnSelectTab?.Invoke(tab));
            args.TabButtons[tab] = button;
        }

        private static ScrollRect ConfigureVerticalScroll(GameObject scrollObject, float sensitivity)
        {
            ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = sensitivity;
            return scrollRect;
        }

        private static RectTransform CreateGridViewport(
            Transform parent,
            string name,
            Vector2 contentSize,
            Vector2 contentPosition,
            Vector2 cellSize,
            Vector2 spacing,
            int columns,
            out Transform gridTransform)
        {
            GameObject viewport = HudUiFactory.CreatePanel(name + "Viewport", parent, new Color(0f, 0f, 0f, 0f));
            HudUiFactory.StretchToParent(viewport);
            viewport.AddComponent<RectMask2D>();
            Image viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.raycastTarget = true;
            }

            GameObject gridObject = new GameObject(name + "Grid", typeof(RectTransform));
            gridObject.transform.SetParent(viewport.transform, false);
            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0f, 1f);
            gridRect.anchorMax = new Vector2(0f, 1f);
            gridRect.pivot = new Vector2(0f, 1f);
            gridRect.sizeDelta = contentSize;
            gridRect.anchoredPosition = contentPosition;
            GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = spacing;
            grid.padding = new RectOffset(0, 0, 0, Mathf.RoundToInt(spacing.y));
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            ContentSizeFitter fitter = gridObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            parent.GetComponent<ScrollRect>().viewport = viewport.GetComponent<RectTransform>();
            gridTransform = gridObject.transform;
            return gridRect;
        }

        private static GameObject CreateActionRow(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position, float spacing)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            SetAnchored(row, anchorMin, anchorMax, new Vector2(0.5f, 0f), size, position);
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return row;
        }

        private static Button CreateCornerActionButton(string label, Transform parent, Color color)
        {
            Button button = HudUiFactory.CreateButton(label, parent, 18, color);
            SetAnchored(button.gameObject, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(34f, 34f), new Vector2(-2f, -2f));
            return button;
        }

        private static Text CreateTextAnchored(
            string name,
            Transform parent,
            int fontSize,
            TextAnchor anchor,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 position)
        {
            Text text = HudUiFactory.CreateText(name, parent, fontSize, FontStyle.Bold, anchor);
            SetAnchored(text.gameObject, anchorMin, anchorMax, pivot, size, position);
            return text;
        }

        private static void ConfigureHoldRepeat(Button button, Action action, Func<bool> canRepeat = null)
        {
            if (button == null)
            {
                return;
            }

            HoldRepeatButton repeat = button.GetComponent<HoldRepeatButton>();
            if (repeat == null)
            {
                repeat = button.gameObject.AddComponent<HoldRepeatButton>();
            }

            repeat.Configure(action, canRepeat);
        }

        private static void ApplyButton(Button button, HeroDetailButtonViewState state)
        {
            if (button == null || state == null)
            {
                return;
            }

            button.interactable = state.Interactable;
            HudUiFactory.SetButtonText(button, state.Text);
            HudUiFactory.SetButtonColor(button, state.Color);
        }

        private static void SetActive(Text text, bool active)
        {
            if (text != null)
            {
                text.gameObject.SetActive(active);
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetTextColor(Text text, Color color)
        {
            if (text != null)
            {
                text.color = color;
            }
        }

        private static void SetStretchOffsets(GameObject target, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetOffsets(GameObject target, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetAnchored(GameObject target, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
