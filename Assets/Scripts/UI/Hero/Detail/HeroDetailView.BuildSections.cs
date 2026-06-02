using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail
{
    public static partial class HeroDetailView
    {
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
            HudUiFactory.ConfigureHoldRepeat(refs.LevelUpButton, args.OnLevelUpHero, args.CanLevelUpHero);
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
                Button lockButton = HudUiFactory.CreateCornerActionButton("잠", slotButton.transform, 18, new Color(0.20f, 0.25f, 0.36f, 1f), 34f, 2f);
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

            Button removeButton = HudUiFactory.CreateCornerActionButton("-", slot.transform, 18, new Color(0.58f, 0.12f, 0.12f, 1f), 34f, 2f);
            removeButton.onClick.AddListener(() => args.OnRemoveEquipmentSlot?.Invoke(equipmentSlot));
            args.EquipmentSlotRemoveButtons[equipmentSlot] = removeButton;
        }

        private static void CreateTabButton(HeroDetailViewBuildArgs args, Transform parent, HeroDetailTab tab, string label)
        {
            Button button = HudUiFactory.CreateButton(label, parent, 25, new Color(0.20f, 0.27f, 0.42f, 1f));
            button.onClick.AddListener(() => args.OnSelectTab?.Invoke(tab));
            args.TabButtons[tab] = button;
        }
    }
}
