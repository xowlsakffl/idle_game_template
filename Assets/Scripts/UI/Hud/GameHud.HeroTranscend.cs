using System.Collections;
using UnityEngine;
using IdleGame.Data;
using IdleGame.UI.Hero.Transcend;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void ToggleHeroTranscendStopMode()
        {
            ApplyHeroTranscendActionResult(heroTranscendState.ToggleStopMode());
            UpdateView();
        }

        private void RollSelectedHeroTranscendManual()
        {
            RequestHeroTranscendRoll(false);
        }

        private void AutoRollSelectedHeroTranscend()
        {
            RequestHeroTranscendRoll(true);
        }

        private void RequestHeroTranscendRoll(bool autoRoll)
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyHeroTranscendActionResult(heroTranscendState.BuildRollRequest(hero, autoRoll));
            UpdateView();
        }

        private void ExecuteHeroTranscendRoll(bool autoRoll)
        {
            if (autoRoll)
            {
                StartHeroTranscendAutoRoll();
                return;
            }

            RollSelectedHeroTranscendBatch(true);
            UpdateView();
        }

        private void OpenHeroTranscendConfirmPrompt(bool autoRoll)
        {
            heroTranscendState.OpenConfirmPrompt(autoRoll);
            if (heroHud.TranscendConfirmMessageText != null)
            {
                heroHud.TranscendConfirmMessageText.text = "변경 대상에 SS 옵션이 있습니다.\n계속 변경하시겠습니까?";
            }

            if (heroHud.TranscendConfirmPrompt != null)
            {
                heroHud.TranscendConfirmPrompt.SetActive(true);
            }
        }

        private void ConfirmHeroTranscendRollPrompt()
        {
            bool autoRoll = heroTranscendState.ConsumeConfirmPromptAutoRoll();
            CloseHeroTranscendConfirmPrompt();
            ExecuteHeroTranscendRoll(autoRoll);
        }

        private void CancelHeroTranscendRollPrompt()
        {
            CloseHeroTranscendConfirmPrompt();
        }

        private void CloseHeroTranscendConfirmPrompt()
        {
            heroTranscendState.CloseConfirmPrompt();
            if (heroHud.TranscendConfirmPrompt != null)
            {
                heroHud.TranscendConfirmPrompt.SetActive(false);
            }
        }

        private void StartHeroTranscendAutoRoll()
        {
            if (heroTranscendAutoRollCoroutine != null)
            {
                return;
            }

            heroTranscendState.MarkAutoRollStarted();
            heroTranscendAutoRollCoroutine = StartCoroutine(RunHeroTranscendAutoRoll());
            UpdateView();
        }

        private void StopHeroTranscendAutoRoll()
        {
            if (heroTranscendAutoRollCoroutine != null)
            {
                StopCoroutine(heroTranscendAutoRollCoroutine);
                heroTranscendAutoRollCoroutine = null;
            }

            heroTranscendState.MarkAutoRollStopped();
        }

        private IEnumerator RunHeroTranscendAutoRoll()
        {
            int rolls = 0;
            HeroTranscendOptionDefinition lastOption = null;
            while (true)
            {
                HeroTranscendRollResult result = RollSelectedHeroTranscendBatch(false);
                if (!result.Success)
                {
                    break;
                }

                rolls += 1;
                lastOption = result.BestOption;
                if (heroTranscendState.ShouldStopAuto(lastOption))
                {
                    break;
                }

                UpdateView();
                yield return new WaitForSecondsRealtime(HeroTranscendAutoRollIntervalSeconds);
            }

            heroTranscendState.MarkAutoRollStopped();
            heroTranscendAutoRollCoroutine = null;

            ShowGrowthNotice(HeroTranscendRollService.BuildAutoRollFinishedMessage(rolls, lastOption));

            UpdateView();
        }

        private HeroTranscendRollResult RollSelectedHeroTranscendBatch(bool showNotice)
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            HeroTranscendRollResult result = heroTranscendState.RollBatch(
                hero,
                wallet,
                battleManager);
            if (showNotice && !string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }

            return result;
        }

        private void ToggleHeroTranscendSlotLock(int slotIndex)
        {
            HeroState hero = FindHeroState(selectedHeroDetailId);
            ApplyHeroTranscendActionResult(heroTranscendState.ToggleSlotLock(hero, slotIndex));
            UpdateView();
        }

        private void ApplyHeroTranscendActionResult(HeroTranscendActionResult result)
        {
            if (result == null)
            {
                return;
            }

            heroTranscendState.ApplyActionResult(result);

            if (result.StopAutoRoll)
            {
                StopHeroTranscendAutoRoll();
            }

            if (result.OpenConfirmPrompt)
            {
                OpenHeroTranscendConfirmPrompt(result.AutoRoll);
            }

            if (result.ExecuteRoll)
            {
                ExecuteHeroTranscendRoll(result.AutoRoll);
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                ShowGrowthNotice(result.Message);
            }
        }

    }
}
