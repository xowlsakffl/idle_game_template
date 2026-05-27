using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Hero.TotemRune
{
    public sealed class TotemRuneCardViewState
    {
        public string Text;
        public Color ButtonColor;
        public bool ActionVisible;
        public bool ActionInteractable;
        public string ActionText;
        public Color ActionColor;
    }

    public sealed class TotemRuneActionButtonViewState
    {
        public bool Visible = true;
        public bool Interactable;
        public string Text;
        public Color Color;
    }

    public static class TotemRunePanelStateBuilder
    {
        private static readonly Color LockedCardColor = new Color(0.20f, 0.22f, 0.26f, 1f);
        private static readonly Color EquippedButtonColor = new Color(0.42f, 0.54f, 0.82f, 1f);
        private static readonly Color EquipButtonColor = new Color(0.54f, 0.76f, 0.96f, 1f);
        private static readonly Color AddActionColor = new Color(0.88f, 0.72f, 0.20f, 1f);
        private static readonly Color LockedActionColor = new Color(0.30f, 0.30f, 0.32f, 1f);
        private static readonly Color MaxButtonColor = new Color(0.34f, 0.36f, 0.40f, 1f);
        private static readonly Color PromoteButtonColor = new Color(0.92f, 0.58f, 0.18f, 1f);
        private static readonly Color BlockedPromoteButtonColor = new Color(0.36f, 0.30f, 0.22f, 1f);
        private static readonly Color CanLevelButtonColor = new Color(0.54f, 0.78f, 0.22f, 1f);
        private static readonly Color CannotLevelButtonColor = new Color(0.35f, 0.36f, 0.34f, 1f);

        public static TotemState ResolveSelectedTotem(
            string selectedTotemId,
            IReadOnlyList<TotemDefinition> totems,
            Func<string, TotemState> getTotemState,
            out string resolvedTotemId)
        {
            TotemState selectedState = !string.IsNullOrEmpty(selectedTotemId) && getTotemState != null
                ? getTotemState(selectedTotemId)
                : null;

            if (selectedState == null && totems != null && totems.Count > 0 && getTotemState != null)
            {
                selectedState = getTotemState(totems[0].Id);
            }

            resolvedTotemId = selectedState != null ? selectedState.Definition.Id : string.Empty;
            return selectedState;
        }

        public static TotemRuneCardViewState BuildTotemCardState(TotemDefinition totem, TotemState state, bool selected)
        {
            bool unlocked = state != null && state.Unlocked;
            Color baseColor = unlocked ? TotemRuneUiText.GetTotemColor(totem) : LockedCardColor;
            return new TotemRuneCardViewState
            {
                Text = TotemRuneUiText.BuildTotemCardText(totem, state, unlocked),
                ButtonColor = selected ? Color.Lerp(baseColor, Color.white, 0.35f) : baseColor,
                ActionVisible = false,
                ActionInteractable = false,
                ActionText = string.Empty,
                ActionColor = LockedActionColor
            };
        }

        public static TotemRuneActionButtonViewState BuildTotemLevelUpButtonState(
            TotemState state,
            long totemEssence,
            bool tierReady,
            Func<long, string> formatCount)
        {
            if (state == null)
            {
                return new TotemRuneActionButtonViewState
                {
                    Interactable = false,
                    Text = "강화",
                    Color = CannotLevelButtonColor
                };
            }

            bool canLevel = state.Unlocked && !state.IsMaxed && totemEssence >= state.LevelUpCost;
            bool canPromote = tierReady && totemEssence >= state.PromoteCost;
            return new TotemRuneActionButtonViewState
            {
                Interactable = state.Unlocked,
                Text = TotemRuneUiText.BuildTotemLevelUpButtonText(state, tierReady, formatCount),
                Color = state.CanPromote
                    ? canPromote ? PromoteButtonColor : BlockedPromoteButtonColor
                    : state.IsMaxed ? MaxButtonColor
                    : canLevel ? CanLevelButtonColor : CannotLevelButtonColor
            };
        }

        public static RuneState ResolveSelectedRune(
            string selectedRuneId,
            IReadOnlyList<RuneDefinition> runes,
            Func<string, RuneState> getRuneState,
            out string resolvedRuneId)
        {
            RuneState selectedState = !string.IsNullOrEmpty(selectedRuneId) && getRuneState != null
                ? getRuneState(selectedRuneId)
                : null;

            if (selectedState == null && runes != null && runes.Count > 0 && getRuneState != null)
            {
                selectedState = getRuneState(runes[0].Id);
            }

            resolvedRuneId = selectedState != null ? selectedState.Definition.Id : string.Empty;
            return selectedState;
        }

        public static TotemRuneCardViewState BuildRuneCardState(RuneDefinition rune, RuneState state, bool selected, bool equipped)
        {
            bool unlocked = state != null && state.Unlocked;
            Color baseColor = unlocked ? TotemRuneUiText.GetRuneColor(rune) : LockedCardColor;
            return new TotemRuneCardViewState
            {
                Text = TotemRuneUiText.BuildRuneCardText(rune, state, equipped, unlocked),
                ButtonColor = selected ? Color.Lerp(baseColor, Color.white, 0.35f) : baseColor,
                ActionVisible = true,
                ActionInteractable = unlocked,
                ActionText = equipped ? "-" : "+",
                ActionColor = equipped ? EquippedButtonColor : unlocked ? AddActionColor : LockedActionColor
            };
        }

        public static TotemRuneActionButtonViewState BuildRuneEquipButtonState(bool equipped)
        {
            return new TotemRuneActionButtonViewState
            {
                Interactable = true,
                Text = TotemRuneUiText.BuildRuneEquipButtonText(equipped),
                Color = equipped ? EquippedButtonColor : EquipButtonColor
            };
        }

        public static TotemRuneActionButtonViewState BuildRuneLevelUpButtonState(RuneState state)
        {
            if (state == null)
            {
                return new TotemRuneActionButtonViewState
                {
                    Interactable = false,
                    Text = "합성",
                    Color = CannotLevelButtonColor
                };
            }

            return new TotemRuneActionButtonViewState
            {
                Interactable = true,
                Text = TotemRuneUiText.BuildRuneLevelUpButtonText(state),
                Color = state.IsMaxed ? MaxButtonColor
                    : state.CanPromote ? PromoteButtonColor : CannotLevelButtonColor
            };
        }
    }
}
