using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;
using IdleGame.UI.Hero.Transcend;

namespace IdleGame.UI.Hero.Detail
{
    public static partial class HeroDetailView
    {
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
    }
}
