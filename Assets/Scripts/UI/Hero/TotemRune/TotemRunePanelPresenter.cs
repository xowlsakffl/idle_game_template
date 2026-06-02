using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.TotemRune
{
    public sealed class TotemPanelPresenterArgs
    {
        public BattleManager BattleManager;
        public StageProgressManager ProgressManager;
        public CurrencyWallet Wallet;
        public string SelectedTotemId;
        public Text SummaryText;
        public Text DetailText;
        public Button LevelUpButton;
        public Dictionary<string, Text> ButtonTexts;
        public Dictionary<string, Button> Buttons;
        public Dictionary<string, TotemRuneCardViewState> CachedCardStates;
        public Func<long, string> FormatCountNumber;
    }

    public sealed class TotemPanelPresenterResult
    {
        public string SelectedTotemId;
    }

    public sealed class RunePanelPresenterArgs
    {
        public BattleManager BattleManager;
        public int SelectedHeroPreset;
        public int SelectedRuneSlot;
        public string SelectedRuneId;
        public Text SummaryText;
        public Text DetailText;
        public Button EquipButton;
        public Button LevelUpButton;
        public Dictionary<string, Text> ButtonTexts;
        public Dictionary<string, Button> Buttons;
        public Dictionary<string, Button> ActionButtons;
        public Dictionary<string, TotemRuneCardViewState> CachedCardStates;
    }

    public sealed class RunePanelPresenterResult
    {
        public int SelectedRuneSlot;
        public string SelectedRuneId;
    }

    public static class TotemRunePanelPresenter
    {
        public static TotemPanelPresenterResult RefreshTotem(TotemPanelPresenterArgs args)
        {
            var result = new TotemPanelPresenterResult
            {
                SelectedTotemId = args != null ? args.SelectedTotemId : string.Empty
            };
            if (args == null || args.BattleManager == null || args.Wallet == null)
            {
                return result;
            }

            TotemState selectedState = TotemRunePanelStateBuilder.ResolveSelectedTotem(
                args.SelectedTotemId,
                GameData.Totems,
                args.BattleManager.GetTotemState,
                out string resolvedTotemId);
            result.SelectedTotemId = resolvedTotemId;

            ApplyTotemSummary(args);
            ApplyTotemCards(args, selectedState);
            ApplyTotemDetail(args, selectedState);
            ApplyTotemButtons(args, selectedState);

            return result;
        }

        public static RunePanelPresenterResult RefreshRune(RunePanelPresenterArgs args)
        {
            var result = new RunePanelPresenterResult
            {
                SelectedRuneSlot = args != null ? args.SelectedRuneSlot : 1,
                SelectedRuneId = args != null ? args.SelectedRuneId : string.Empty
            };
            if (args == null || args.BattleManager == null)
            {
                return result;
            }

            result.SelectedRuneSlot = Mathf.Clamp(args.SelectedRuneSlot, 1, GameData.MaxRuneSlots);
            RuneState selectedState = TotemRunePanelStateBuilder.ResolveSelectedRune(
                args.SelectedRuneId,
                GameData.Runes,
                args.BattleManager.GetRuneState,
                out string resolvedRuneId);
            result.SelectedRuneId = resolvedRuneId;

            ApplyRuneSummary(args, selectedState);
            ApplyRuneCards(args, selectedState);
            ApplyRuneDetail(args, selectedState);
            ApplyRuneButtons(args, selectedState);

            return result;
        }

        private static void ApplyTotemSummary(TotemPanelPresenterArgs args)
        {
            if (args.SummaryText != null)
            {
                args.SummaryText.text = TotemRuneUiText.BuildTotemSummary(args.Wallet.TotemEssence, args.FormatCountNumber);
            }
        }

        private static void ApplyTotemCards(TotemPanelPresenterArgs args, TotemState selectedState)
        {
            foreach (TotemDefinition totem in GameData.Totems)
            {
                TotemState state = args.BattleManager.GetTotemState(totem.Id);
                bool selected = selectedState != null && selectedState.Definition.Id == totem.Id;
                TotemRuneCardViewState cardState = TotemRunePanelStateBuilder.BuildTotemCardState(totem, state, selected);
                if (IsCachedCardCurrent(args.CachedCardStates, totem.Id, cardState))
                {
                    continue;
                }

                if (args.ButtonTexts != null && args.ButtonTexts.TryGetValue(totem.Id, out Text text) && text != null)
                {
                    text.text = cardState.Text;
                }

                if (args.Buttons != null && args.Buttons.TryGetValue(totem.Id, out Button button) && button != null)
                {
                    HudUiFactory.SetButtonColor(button, cardState.ButtonColor);
                }

                CacheCardState(args.CachedCardStates, totem.Id, cardState);
            }
        }

        private static void ApplyTotemDetail(TotemPanelPresenterArgs args, TotemState selectedState)
        {
            if (args.DetailText == null)
            {
                return;
            }

            bool isBoss = args.ProgressManager != null && args.ProgressManager.CurrentStage.Type == StageType.Boss;
            args.DetailText.text = selectedState != null
                ? TotemRuneUiText.BuildTotemDetailText(selectedState, args.BattleManager.DeployedHeroes, isBoss)
                : string.Empty;
        }

        private static void ApplyTotemButtons(TotemPanelPresenterArgs args, TotemState selectedState)
        {
            if (args.LevelUpButton == null)
            {
                return;
            }

            bool tierReady = selectedState != null
                && selectedState.Unlocked
                && selectedState.CanPromote
                && args.BattleManager.CanPromoteTotemTier(selectedState.Definition.Id);
            TotemRuneActionButtonViewState buttonState = TotemRunePanelStateBuilder.BuildTotemLevelUpButtonState(
                selectedState,
                args.Wallet.TotemEssence,
                tierReady,
                args.FormatCountNumber);
            args.LevelUpButton.interactable = buttonState.Interactable;
            HudUiFactory.SetButtonText(args.LevelUpButton, buttonState.Text);
            HudUiFactory.SetButtonColor(args.LevelUpButton, buttonState.Color);
        }

        private static void ApplyRuneSummary(RunePanelPresenterArgs args, RuneState selectedState)
        {
            if (args.SummaryText != null)
            {
                args.SummaryText.text = TotemRuneUiText.BuildRuneSummary(args.SelectedHeroPreset, selectedState);
            }
        }

        private static void ApplyRuneCards(RunePanelPresenterArgs args, RuneState selectedState)
        {
            foreach (RuneDefinition rune in GameData.Runes)
            {
                RuneState state = args.BattleManager.GetRuneState(rune.Id);
                bool selected = selectedState != null && selectedState.Definition.Id == rune.Id;
                bool equipped = TotemRuneActionService.IsRuneEquipped(args.BattleManager, args.SelectedHeroPreset, rune.Id);
                TotemRuneCardViewState cardState = TotemRunePanelStateBuilder.BuildRuneCardState(rune, state, selected, equipped);
                if (IsCachedCardCurrent(args.CachedCardStates, rune.Id, cardState))
                {
                    continue;
                }

                if (args.ButtonTexts != null && args.ButtonTexts.TryGetValue(rune.Id, out Text text) && text != null)
                {
                    text.text = cardState.Text;
                }

                if (args.Buttons != null && args.Buttons.TryGetValue(rune.Id, out Button button) && button != null)
                {
                    HudUiFactory.SetButtonColor(button, cardState.ButtonColor);
                }

                if (args.ActionButtons != null
                    && args.ActionButtons.TryGetValue(rune.Id, out Button actionButton)
                    && actionButton != null)
                {
                    actionButton.gameObject.SetActive(cardState.ActionVisible);
                    actionButton.interactable = cardState.ActionInteractable;
                    HudUiFactory.SetButtonText(actionButton, cardState.ActionText);
                    HudUiFactory.SetButtonColor(actionButton, cardState.ActionColor);
                }

                CacheCardState(args.CachedCardStates, rune.Id, cardState);
            }
        }

        private static void ApplyRuneDetail(RunePanelPresenterArgs args, RuneState selectedState)
        {
            if (args.DetailText != null)
            {
                args.DetailText.text = selectedState != null ? TotemRuneUiText.BuildRuneDetailText(selectedState) : string.Empty;
            }
        }

        private static void ApplyRuneButtons(RunePanelPresenterArgs args, RuneState selectedState)
        {
            if (args.EquipButton != null)
            {
                bool equipped = selectedState != null
                    && TotemRuneActionService.GetEquippedRuneSlot(
                        args.BattleManager,
                        args.SelectedHeroPreset,
                        selectedState.Definition.Id) > 0;
                TotemRuneActionButtonViewState buttonState = TotemRunePanelStateBuilder.BuildRuneEquipButtonState(equipped);
                args.EquipButton.interactable = selectedState != null && selectedState.Unlocked;
                HudUiFactory.SetButtonText(args.EquipButton, buttonState.Text);
                HudUiFactory.SetButtonColor(args.EquipButton, buttonState.Color);
            }

            if (args.LevelUpButton != null)
            {
                TotemRuneActionButtonViewState buttonState = TotemRunePanelStateBuilder.BuildRuneLevelUpButtonState(selectedState);
                args.LevelUpButton.interactable = buttonState.Interactable;
                HudUiFactory.SetButtonText(args.LevelUpButton, buttonState.Text);
                HudUiFactory.SetButtonColor(args.LevelUpButton, buttonState.Color);
            }
        }

        private static bool IsCachedCardCurrent(
            Dictionary<string, TotemRuneCardViewState> cachedStates,
            string id,
            TotemRuneCardViewState state)
        {
            return cachedStates != null
                && cachedStates.TryGetValue(id, out TotemRuneCardViewState cachedState)
                && state != null
                && state.IsSameAs(cachedState);
        }

        private static void CacheCardState(
            Dictionary<string, TotemRuneCardViewState> cachedStates,
            string id,
            TotemRuneCardViewState state)
        {
            if (cachedStates != null && !string.IsNullOrEmpty(id) && state != null)
            {
                cachedStates[id] = state;
            }
        }
    }
}
