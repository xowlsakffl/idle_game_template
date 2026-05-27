using UnityEngine.UI;
using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.UI.Hero;
using IdleGame.UI.Hero.TotemRune;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void RefreshHeroTotemPanel()
        {
            TotemPanelPresenterResult result = TotemRunePanelPresenter.RefreshTotem(new TotemPanelPresenterArgs
            {
                BattleManager = battleManager,
                ProgressManager = progressManager,
                Wallet = wallet,
                SelectedTotemId = totemRuneState.SelectedTotemId,
                SummaryText = heroHud.TotemSummaryText,
                DetailText = heroHud.TotemDetailText,
                EquipButton = heroHud.TotemEquipButton,
                LevelUpButton = heroHud.TotemLevelUpButton,
                ButtonTexts = heroHud.TotemButtonTexts,
                Buttons = heroHud.TotemButtons,
                ActionButtons = heroHud.TotemActionButtons,
                FormatCountNumber = FormatCountNumber
            });
            totemRuneState.SetResolvedTotem(result.SelectedTotemId);
        }

        private void RefreshHeroRunePanel()
        {
            RunePanelPresenterResult result = TotemRunePanelPresenter.RefreshRune(new RunePanelPresenterArgs
            {
                BattleManager = battleManager,
                SelectedHeroPreset = heroFormationState.SelectedPreset,
                SelectedRuneSlot = totemRuneState.SelectedRuneSlot,
                SelectedRuneId = totemRuneState.SelectedRuneId,
                SummaryText = heroHud.RuneSummaryText,
                DetailText = heroHud.RuneDetailText,
                EquipButton = heroHud.RuneEquipButton,
                LevelUpButton = heroHud.RuneLevelUpButton,
                ButtonTexts = heroHud.RuneButtonTexts,
                Buttons = heroHud.RuneButtons,
                ActionButtons = heroHud.RuneActionButtons
            });
            totemRuneState.SetResolvedRune(result.SelectedRuneSlot, result.SelectedRuneId);
        }


        private void SelectRune(string runeId)
        {
            ApplyTotemRuneUiAction(totemRuneState.SelectRune(runeId));
        }

        private void StartPendingRuneEquip(string runeId)
        {
            ApplyTotemRuneUiAction(totemRuneState.StartPendingRuneEquip(battleManager, heroFormationState.SelectedPreset, runeId));
        }

        private void HandleFormationRuneSlotClick(int slot)
        {
            ApplyTotemRuneUiAction(totemRuneState.HandleRuneSlotClick(battleManager, heroFormationState.SelectedPreset, slot));
        }

        private void TryEquipPendingRuneInSlot(int slot)
        {
            ApplyTotemRuneUiAction(totemRuneState.TryEquipPendingRuneInSlot(battleManager, heroFormationState.SelectedPreset, slot));
        }

        private void RemoveRuneFromFormationSlot(int slot)
        {
            ApplyTotemRuneUiAction(totemRuneState.RemoveRuneFromSlot(battleManager, heroFormationState.SelectedPreset, slot));
        }

        private void EquipSelectedRune()
        {
            ApplyTotemRuneUiAction(totemRuneState.EquipSelectedRune(battleManager, heroFormationState.SelectedPreset));
        }

        private void LevelUpSelectedRune()
        {
            ApplyTotemRuneUiAction(totemRuneState.PromoteSelectedRune(battleManager));
        }

        private bool CanLevelUpSelectedRune()
        {
            return totemRuneState.CanPromoteSelectedRune(battleManager);
        }


        private void SelectTotem(string totemId)
        {
            ApplyTotemRuneUiAction(totemRuneState.SelectTotem(totemId));
        }

        private void EquipSelectedTotem()
        {
            ApplyTotemRuneUiAction(totemRuneState.EquipSelectedTotem());
        }

        private void RefreshPendingRuneSlotGlow()
        {
            if (battleManager == null
                || string.IsNullOrEmpty(totemRuneState.PendingRuneEquipId)
                || activeTab != HudTab.Hero
                || activeHeroPageTab != HeroPageTab.Formation)
            {
                return;
            }

            RuneState state = battleManager.GetRuneState(totemRuneState.PendingRuneEquipId);
            if (state == null)
            {
                return;
            }

            float glow = 0.35f + Mathf.PingPong(Time.unscaledTime * 2.4f, 1f) * 0.45f;
            Color glowColor = Color.Lerp(TotemRuneUiText.GetRuneColor(state.Definition), Color.white, glow);
            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                if (!battleManager.IsRuneSlotUnlocked(slot))
                {
                    continue;
                }

                if (heroHud.FormationRuneSlotButtons.TryGetValue(slot, out Button button) && button != null)
                {
                    SetButtonColor(button, glowColor);
                }
            }
        }

        private void LevelUpSelectedTotem()
        {
            ApplyTotemRuneUiAction(totemRuneState.LevelSelectedTotem(battleManager, wallet));
        }

        private bool CanLevelUpSelectedTotem()
        {
            return totemRuneState.CanLevelSelectedTotem(battleManager, wallet);
        }


        private void ApplyTotemRuneUiAction(TotemRuneUiActionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.SwitchToHeroFormation)
            {
                activeTab = HudTab.Hero;
                contentPanelOpen = true;
                activeHeroPageTab = HeroPageTab.Formation;
            }

            ShowActionResult(result.ActionResult);

            if (!string.IsNullOrEmpty(result.NoticeMessage))
            {
                ShowGrowthNotice(result.NoticeMessage);
            }

            if (result.ShouldRefreshTotemPanel)
            {
                RefreshHeroTotemPanel();
            }

            if (result.ShouldRefreshRunePanel)
            {
                RefreshHeroRunePanel();
            }

            if (result.ShouldUpdateView)
            {
                UpdateView();
            }
        }

        private void ShowActionResult(TotemRuneActionResult result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }
        }

    }
}
