using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.UI.Hero.Transcend
{
    public sealed class HeroTranscendActionResult
    {
        public bool StopAutoRoll;
        public bool OpenConfirmPrompt;
        public bool ExecuteRoll;
        public bool AutoRoll;
        public bool HasStopOnlySs;
        public bool StopOnlySs;
        public bool HasSelectedSlotIndex;
        public int SelectedSlotIndex;
        public string Message;
    }

    public static class HeroTranscendActionService
    {
        public static bool[] BuildLockedSlots(HeroState hero)
        {
            var lockedSlots = new bool[HeroDefinition.MaxTranscendSlots];
            if (hero == null)
            {
                return lockedSlots;
            }

            for (int i = 0; i < lockedSlots.Length; i++)
            {
                lockedSlots[i] = IsSlotLocked(hero.Definition.Id, i);
            }

            return lockedSlots;
        }

        public static HeroTranscendActionResult ToggleStopMode(bool currentStopOnlySs)
        {
            bool nextStopOnlySs = !currentStopOnlySs;
            PlayerPrefs.SetInt(SaveKeys.HeroTranscendStopOnlySs, nextStopOnlySs ? 1 : 0);
            PlayerPrefs.Save();
            return new HeroTranscendActionResult
            {
                HasStopOnlySs = true,
                StopOnlySs = nextStopOnlySs
            };
        }

        public static HeroTranscendActionResult BuildRollRequest(
            HeroState hero,
            bool[] lockedSlots,
            bool autoRoll,
            bool autoRolling)
        {
            if (autoRoll && autoRolling)
            {
                return new HeroTranscendActionResult
                {
                    StopAutoRoll = true,
                    Message = "자동 변경을 중지했습니다."
                };
            }

            if (hero == null)
            {
                return new HeroTranscendActionResult();
            }

            if (!hero.IsOwned)
            {
                return new HeroTranscendActionResult
                {
                    Message = "아직 획득하지 않은 영웅입니다."
                };
            }

            if (HeroTranscendRollService.ShouldAskBeforeRoll(hero, lockedSlots))
            {
                return new HeroTranscendActionResult
                {
                    OpenConfirmPrompt = true,
                    AutoRoll = autoRoll
                };
            }

            return new HeroTranscendActionResult
            {
                ExecuteRoll = true,
                AutoRoll = autoRoll
            };
        }

        public static HeroTranscendActionResult ToggleSlotLock(HeroState hero, int slotIndex)
        {
            if (hero == null)
            {
                return new HeroTranscendActionResult();
            }

            if (!hero.IsTranscendSlotUnlocked(slotIndex))
            {
                return new HeroTranscendActionResult
                {
                    Message = HeroDefinition.GetTranscendRequiredStars(slotIndex) + "성부터 해금할 수 있습니다."
                };
            }

            bool locked = IsSlotLocked(hero.Definition.Id, slotIndex);
            PlayerPrefs.SetInt(SaveKeys.HeroTranscendLocked(hero.Definition.Id, slotIndex), locked ? 0 : 1);
            PlayerPrefs.Save();
            return new HeroTranscendActionResult
            {
                HasSelectedSlotIndex = true,
                SelectedSlotIndex = slotIndex
            };
        }

        private static bool IsSlotLocked(string heroId, int slotIndex)
        {
            return !string.IsNullOrEmpty(heroId)
                && slotIndex >= 0
                && slotIndex < HeroDefinition.MaxTranscendSlots
                && PlayerPrefs.GetInt(SaveKeys.HeroTranscendLocked(heroId, slotIndex), 0) == 1;
        }
    }
}
