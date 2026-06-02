using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.UI.Common;

namespace IdleGame.UI.Facility
{
    public static partial class FacilityView
    {
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
