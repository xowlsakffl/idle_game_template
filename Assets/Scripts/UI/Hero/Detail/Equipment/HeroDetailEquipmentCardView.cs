using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public sealed class HeroDetailEquipmentInventoryCardBuildArgs
    {
        public EquipmentDefinition Equipment;
        public string CardKey;
        public Transform Parent;
        public IDictionary<string, Button> CardButtons;
        public IDictionary<string, Text> CardTexts;
        public IDictionary<string, Button> ActionButtons;
        public Func<string, Transform, int, Color, Button> CreateButton;
        public Func<string, Transform, Color, Button> CreateCornerActionButton;
        public Action<string> OpenDetail;
        public Action<string> ToggleEquipment;
    }

    public sealed class HeroDetailEquipmentInventoryCardViewState
    {
        public EquipmentDefinition Equipment;
        public EquipmentState State;
        public string EquippedHeroLabel;
        public int CopyNumber;
        public bool Equipped;
        public bool EquippedToCurrentHero;
        public bool Selected;
    }

    public sealed class HeroDetailEquipmentDismantleCardBuildArgs
    {
        public EquipmentDefinition Equipment;
        public string CardKey;
        public Transform Parent;
        public IDictionary<string, Button> CardButtons;
        public IDictionary<string, Text> CardTexts;
        public Func<string, Transform, int, Color, Button> CreateButton;
        public Action<string> SelectCard;
    }

    public sealed class HeroDetailEquipmentDismantleCardViewState
    {
        public EquipmentDefinition Equipment;
        public EquipmentState State;
        public int CopyNumber;
        public int Reward;
        public bool Selected;
    }

    public static class HeroDetailEquipmentCardView
    {
        public static void HideCards(IDictionary<string, Button> cardButtons)
        {
            if (cardButtons == null)
            {
                return;
            }

            foreach (KeyValuePair<string, Button> pair in cardButtons)
            {
                if (pair.Value != null)
                {
                    pair.Value.gameObject.SetActive(false);
                }
            }
        }

        public static Button GetOrCreateInventoryCard(HeroDetailEquipmentInventoryCardBuildArgs args)
        {
            if (args == null
                || args.Equipment == null
                || string.IsNullOrEmpty(args.CardKey)
                || args.CardButtons == null)
            {
                return null;
            }

            if (args.CardButtons.TryGetValue(args.CardKey, out Button existingButton))
            {
                return existingButton;
            }

            if (args.Parent == null || args.CreateButton == null)
            {
                return null;
            }

            Button card = args.CreateButton(args.Equipment.DisplayName, args.Parent, 18, HeroUiText.GetRarityColor(args.Equipment.Rarity));
            string equipmentId = args.Equipment.Id;
            card.onClick.AddListener(() => args.OpenDetail?.Invoke(equipmentId));

            Text text = card.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 18;
                args.CardTexts[args.CardKey] = text;
            }

            args.CardButtons[args.CardKey] = card;
            if (args.CreateCornerActionButton != null && args.ActionButtons != null)
            {
                Button actionButton = args.CreateCornerActionButton("+", card.transform, new Color(0.88f, 0.72f, 0.20f, 1f));
                actionButton.onClick.AddListener(() => args.ToggleEquipment?.Invoke(equipmentId));
                args.ActionButtons[args.CardKey] = actionButton;
            }

            return card;
        }

        public static void ApplyInventoryCard(Button cardButton, Text cardText, Button actionButton, HeroDetailEquipmentInventoryCardViewState state)
        {
            if (cardButton == null || state == null || state.Equipment == null || state.State == null)
            {
                return;
            }

            cardButton.gameObject.SetActive(true);
            cardButton.transform.SetAsLastSibling();

            Color color = state.Equipped
                ? state.EquippedToCurrentHero ? new Color(0.13f, 0.15f, 0.18f, 1f) : new Color(0.16f, 0.20f, 0.30f, 1f)
                : state.Selected ? new Color(0.54f, 0.45f, 0.16f, 1f) : HeroUiText.GetRarityColor(state.Equipment.Rarity);
            HudUiFactory.SetButtonColor(cardButton, color);

            if (cardText != null)
            {
                cardText.text = EquipmentUiText.BuildInventoryCardText(
                    state.Equipment,
                    state.State,
                    state.EquippedHeroLabel,
                    state.CopyNumber);
            }

            ApplyInventoryActionButton(actionButton, state);
        }

        public static Button GetOrCreateDismantleCard(HeroDetailEquipmentDismantleCardBuildArgs args)
        {
            if (args == null
                || args.Equipment == null
                || string.IsNullOrEmpty(args.CardKey)
                || args.CardButtons == null)
            {
                return null;
            }

            if (args.CardButtons.TryGetValue(args.CardKey, out Button existingButton))
            {
                return existingButton;
            }

            if (args.Parent == null || args.CreateButton == null)
            {
                return null;
            }

            Button card = args.CreateButton(args.Equipment.DisplayName, args.Parent, 16, HeroUiText.GetRarityColor(args.Equipment.Rarity));
            string equipmentCopyKey = args.CardKey;
            card.onClick.AddListener(() => args.SelectCard?.Invoke(equipmentCopyKey));

            Text text = card.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 16;
                args.CardTexts[args.CardKey] = text;
            }

            args.CardButtons[args.CardKey] = card;
            return card;
        }

        public static void ApplyDismantleCard(Button cardButton, Text cardText, HeroDetailEquipmentDismantleCardViewState state)
        {
            if (cardButton == null || state == null || state.Equipment == null || state.State == null)
            {
                return;
            }

            cardButton.gameObject.SetActive(true);
            cardButton.transform.SetAsLastSibling();
            HudUiFactory.SetButtonColor(
                cardButton,
                state.Selected ? new Color(0.54f, 0.45f, 0.16f, 1f) : HeroUiText.GetRarityColor(state.Equipment.Rarity));

            if (cardText != null)
            {
                cardText.text = EquipmentUiText.BuildDismantleCardText(state.State, state.CopyNumber, state.Reward);
            }
        }

        private static void ApplyInventoryActionButton(Button actionButton, HeroDetailEquipmentInventoryCardViewState state)
        {
            if (actionButton == null)
            {
                return;
            }

            actionButton.gameObject.SetActive(true);
            actionButton.interactable = !state.Equipped || state.EquippedToCurrentHero;

            Text actionText = actionButton.GetComponentInChildren<Text>(true);
            if (actionText != null)
            {
                actionText.text = state.Equipped ? state.EquippedToCurrentHero ? "-" : "사용중" : "+";
            }

            HudUiFactory.SetButtonColor(actionButton, state.Equipped
                ? state.EquippedToCurrentHero ? new Color(0.58f, 0.12f, 0.12f, 1f) : new Color(0.31f, 0.34f, 0.39f, 1f)
                : new Color(0.88f, 0.72f, 0.20f, 1f));
        }
    }
}
