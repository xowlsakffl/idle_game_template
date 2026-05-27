using UnityEngine;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Save;

namespace IdleGame.UI.Hero.Transcend
{
    public sealed class HeroTranscendUiState
    {
        public int SelectedSlotIndex { get; private set; }
        public bool StopOnlySs { get; private set; } = true;
        public bool PendingAutoRoll { get; private set; }
        public bool AutoRolling { get; private set; }

        public void ResetRuntime()
        {
            SelectedSlotIndex = 0;
            StopOnlySs = PlayerPrefs.GetInt(SaveKeys.HeroTranscendStopOnlySs, 1) == 1;
            PendingAutoRoll = false;
            AutoRolling = false;
        }

        public void SelectSlot(int slotIndex)
        {
            SelectedSlotIndex = Mathf.Clamp(slotIndex, 0, HeroDefinition.MaxTranscendSlots - 1);
        }

        public bool[] BuildLockedSlots(HeroState hero)
        {
            return HeroTranscendActionService.BuildLockedSlots(hero);
        }

        public HeroTranscendActionResult ToggleStopMode()
        {
            return HeroTranscendActionService.ToggleStopMode(StopOnlySs);
        }

        public HeroTranscendActionResult BuildRollRequest(HeroState hero, bool autoRoll)
        {
            return HeroTranscendActionService.BuildRollRequest(
                hero,
                BuildLockedSlots(hero),
                autoRoll,
                AutoRolling);
        }

        public HeroTranscendActionResult ToggleSlotLock(HeroState hero, int slotIndex)
        {
            return HeroTranscendActionService.ToggleSlotLock(hero, slotIndex);
        }

        public HeroTranscendRollResult RollBatch(
            HeroState hero,
            CurrencyWallet wallet,
            BattleManager battleManager)
        {
            return HeroTranscendRollService.TryRollBatch(
                hero,
                BuildLockedSlots(hero),
                wallet,
                battleManager);
        }

        public bool ShouldStopAuto(HeroTranscendOptionDefinition option)
        {
            return HeroTranscendRules.ShouldStopAuto(option, StopOnlySs);
        }

        public void OpenConfirmPrompt(bool autoRoll)
        {
            PendingAutoRoll = autoRoll;
        }

        public bool ConsumeConfirmPromptAutoRoll()
        {
            bool autoRoll = PendingAutoRoll;
            CloseConfirmPrompt();
            return autoRoll;
        }

        public void CloseConfirmPrompt()
        {
            PendingAutoRoll = false;
        }

        public void MarkAutoRollStarted()
        {
            AutoRolling = true;
        }

        public void MarkAutoRollStopped()
        {
            PendingAutoRoll = false;
            AutoRolling = false;
        }

        public void ApplyActionResult(HeroTranscendActionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.HasStopOnlySs)
            {
                StopOnlySs = result.StopOnlySs;
            }

            if (result.HasSelectedSlotIndex)
            {
                SelectSlot(result.SelectedSlotIndex);
            }
        }
    }
}
