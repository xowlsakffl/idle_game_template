using UnityEngine;
using IdleGame.Battle;

namespace IdleGame.UI.Hud
{
    public sealed class HudRuntimeTickState
    {
        private const float FlashDurationSeconds = 0.28f;

        private int observedHitSequence = -1;
        private int observedHeroAttackBatchSequence = -1;

        public float HitFlashRemaining { get; private set; }
        public float HeroAttackFlashRemaining { get; private set; }
        public string NoticeMessage { get; private set; } = string.Empty;
        public float NoticeUntil { get; private set; }

        public void ResetRuntime()
        {
            observedHitSequence = -1;
            observedHeroAttackBatchSequence = -1;
            HitFlashRemaining = 0f;
            HeroAttackFlashRemaining = 0f;
            NoticeMessage = string.Empty;
            NoticeUntil = 0f;
        }

        public bool Tick(BattleManager battleManager, float deltaTime, float unscaledTime)
        {
            TickBattleFlashes(battleManager, deltaTime);
            return TickNoticeExpiry(unscaledTime);
        }

        public void ShowNotice(string message, float currentTime, float durationSeconds)
        {
            NoticeMessage = message ?? string.Empty;
            NoticeUntil = currentTime + Mathf.Max(0f, durationSeconds);
        }

        public string GetActiveNotice(float currentTime)
        {
            return currentTime < NoticeUntil ? NoticeMessage : string.Empty;
        }

        private void TickBattleFlashes(BattleManager battleManager, float deltaTime)
        {
            if (battleManager == null)
            {
                HitFlashRemaining = ReduceTimer(HitFlashRemaining, deltaTime);
                HeroAttackFlashRemaining = ReduceTimer(HeroAttackFlashRemaining, deltaTime);
                return;
            }

            if (observedHitSequence != battleManager.HitSequence)
            {
                observedHitSequence = battleManager.HitSequence;
                HitFlashRemaining = FlashDurationSeconds;
            }

            if (observedHeroAttackBatchSequence != battleManager.HeroAttackBatchSequence)
            {
                observedHeroAttackBatchSequence = battleManager.HeroAttackBatchSequence;
                HeroAttackFlashRemaining = battleManager.HeroAttackBatchSequence > 0 ? FlashDurationSeconds : 0f;
            }

            HitFlashRemaining = ReduceTimer(HitFlashRemaining, deltaTime);
            HeroAttackFlashRemaining = ReduceTimer(HeroAttackFlashRemaining, deltaTime);
        }

        private bool TickNoticeExpiry(float unscaledTime)
        {
            if (string.IsNullOrEmpty(NoticeMessage) || unscaledTime < NoticeUntil)
            {
                return false;
            }

            NoticeMessage = string.Empty;
            return true;
        }

        private static float ReduceTimer(float value, float deltaTime)
        {
            return value > 0f ? Mathf.Max(0f, value - deltaTime) : 0f;
        }
    }
}
