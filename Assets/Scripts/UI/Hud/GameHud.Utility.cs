using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Progression;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private HeroState FindHeroState(string heroId)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                return null;
            }

            foreach (HeroState hero in battleManager.Heroes)
            {
                if (hero.Definition.Id == heroId)
                {
                    return hero;
                }
            }

            return null;
        }

        private string GetShortHeroLabel(HeroDefinition hero)
        {
            if (hero.DisplayName.Length <= 2)
            {
                return hero.DisplayName;
            }

            return hero.DisplayName.Substring(hero.DisplayName.Length - 2);
        }

        private bool IsDebugPanelEnabled()
        {
            return Application.isEditor || Debug.isDebugBuild;
        }

        private string GetModeLabel(ProgressMode mode)
        {
            switch (mode)
            {
                case ProgressMode.AutoProgress:
                    return "자동 진행";
                case ProgressMode.RepeatSelected:
                    return "선택 반복";
                case ProgressMode.BossBlocked:
                    return "보스 막힘";
                default:
                    return mode.ToString();
            }
        }

        private string GetHeroPageTabLabel(HeroPageTab tab)
        {
            switch (tab)
            {
                case HeroPageTab.Formation:
                    return "편성";
                case HeroPageTab.Trait:
                    return "특성";
                case HeroPageTab.Statue:
                    return "토템";
                case HeroPageTab.Seal:
                    return "룬";
                case HeroPageTab.Relic:
                    return "시설";
                default:
                    return tab.ToString();
            }
        }

        private string FormatShortNumber(double value)
        {
            return NumberFormatter.Format(value);
        }

        private string FormatShortNumber(GameNumber value)
        {
            return NumberFormatter.Format(value);
        }

        private string FormatCountNumber(long value)
        {
            return GameData.ClampCount(value).ToString("#,0");
        }

        private void ShowDungeonEntryTransition(DungeonKind kind, int level)
        {
            if (dungeonTransitionRoot == null || dungeonTransitionCanvasGroup == null)
            {
                return;
            }

            if (dungeonEntryTransitionCoroutine != null)
            {
                StopCoroutine(dungeonEntryTransitionCoroutine);
                dungeonEntryTransitionCoroutine = null;
            }

            if (dungeonTransitionTitleText != null)
            {
                dungeonTransitionTitleText.text = DungeonProgressManager.GetTitle(kind) + " Lv." + Mathf.Max(1, level);
            }

            if (dungeonTransitionSubtitleText != null)
            {
                dungeonTransitionSubtitleText.text = "던전 입장 중";
            }

            dungeonEntryTransitionCoroutine = StartCoroutine(PlayDungeonEntryTransition());
        }

        private IEnumerator PlayDungeonEntryTransition()
        {
            const float fadeInSeconds = 0.16f;
            const float holdSeconds = 0.38f;
            const float fadeOutSeconds = 0.36f;

            dungeonTransitionRoot.SetActive(true);
            dungeonTransitionCanvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeInSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                dungeonTransitionCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInSeconds);
                yield return null;
            }

            dungeonTransitionCanvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(holdSeconds);

            elapsed = 0f;
            while (elapsed < fadeOutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                dungeonTransitionCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutSeconds);
                yield return null;
            }

            dungeonTransitionCanvasGroup.alpha = 0f;
            dungeonTransitionCanvasGroup.blocksRaycasts = false;
            dungeonTransitionRoot.SetActive(false);
            dungeonEntryTransitionCoroutine = null;
        }

        private void RefreshDungeonClearPopup()
        {
            if (dungeonClearPopupRoot == null)
            {
                return;
            }

            dungeonClearPopupRoot.SetActive(dungeonClearPopupOpen);
            if (!dungeonClearPopupOpen)
            {
                return;
            }

            if (dungeonClearPopupTitleText != null)
            {
                dungeonClearPopupTitleText.text = dungeonClearPopupEndedRepeat
                    ? "연속 도전 종료"
                    : dungeonClearPopupCloseOnNextRun
                        ? "보상 획득"
                    : DungeonProgressManager.GetTitle(dungeonClearPopupKind) + " 클리어";
            }

            if (dungeonClearPopupRewardText != null)
            {
                string reward = string.IsNullOrEmpty(dungeonClearPopupReward)
                    ? "보상 없음"
                    : dungeonClearPopupReward;
                string progressText = dungeonClearPopupKind == DungeonKind.TotemEssence
                    ? "보스 " + Mathf.Max(0, dungeonClearPopupLevel) + " 처치"
                    : "Lv." + Mathf.Max(1, dungeonClearPopupLevel);
                dungeonClearPopupRewardText.text = DungeonProgressManager.GetTitle(dungeonClearPopupKind)
                    + " " + progressText
                    + "\n획득 보상\n" + reward;
            }
        }

        private void OpenDungeonClearPopup(DungeonKind kind, int level, string rewardText, bool keepSelectedLevel, bool endedRepeat, bool closeOnNextRun)
        {
            dungeonClearPopupKind = kind;
            dungeonClearPopupLevel = kind == DungeonKind.TotemEssence
                ? Mathf.Max(0, level)
                : Mathf.Max(1, level);
            dungeonClearPopupReward = rewardText ?? string.Empty;
            dungeonClearPopupKeepSelectedLevel = keepSelectedLevel;
            dungeonClearPopupEndedRepeat = endedRepeat;
            dungeonClearPopupCloseOnNextRun = closeOnNextRun;
            selectedDungeonKind = kind;
            selectedDungeonLevel = dungeonClearPopupLevel;
            dungeonClearPopupOpen = true;
            QueueHudRefresh(HudDirtyFlags.Header | HudDirtyFlags.Stage | HudDirtyFlags.Navigation);
        }

        private void CloseDungeonClearPopup()
        {
            if (dungeonClearPopupCloseOnNextRun)
            {
                HideDungeonClearPopupOnly();
                return;
            }

            HideDungeonClearPopupOnly();
            selectedDungeonLevel = dungeonProgressManager != null
                ? (dungeonClearPopupKeepSelectedLevel
                    ? dungeonProgressManager.ClampSelectableLevel(selectedDungeonKind, dungeonClearPopupLevel)
                    : dungeonProgressManager.GetMaxSelectableLevel(selectedDungeonKind))
                : 1;
            dungeonDetailOpen = true;
            activeTab = HudTab.Stage;
            contentPanelOpen = true;
            heroDetailPanelOpen = false;
            QueueHudRefresh(HudDirtyFlags.All);
            UpdateView();
        }

        private void HideDungeonClearPopupOnly()
        {
            dungeonClearPopupOpen = false;
            dungeonClearPopupCloseOnNextRun = false;
            if (dungeonClearPopupRoot != null)
            {
                dungeonClearPopupRoot.SetActive(false);
            }
        }

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            return HudUiFactory.CreatePanel(name, parent, color);
        }

        private void ConfigureHoldRepeat(Button button, Action action, Func<bool> canRepeat = null)
        {
            if (button == null)
            {
                return;
            }

            HoldRepeatButton repeatButton = button.GetComponent<HoldRepeatButton>();
            if (repeatButton == null)
            {
                repeatButton = button.gameObject.AddComponent<HoldRepeatButton>();
            }

            repeatButton.Configure(action, canRepeat);
        }

        private Button CreateButton(string label, Transform parent, int fontSize, Color color)
        {
            return HudUiFactory.CreateButton(label, parent, fontSize, color);
        }

        private void SetButtonText(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            Text buttonText = button.GetComponentInChildren<Text>(true);
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }

        private void SetButtonColor(Button button, Color color)
        {
            HudUiFactory.SetButtonColor(button, color);
        }

        private Text CreateText(string name, Transform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment)
        {
            return HudUiFactory.CreateText(name, parent, fontSize, fontStyle, alignment);
        }

        private void StretchToParent(GameObject target)
        {
            HudUiFactory.StretchToParent(target);
        }

        private LayoutElement AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
        {
            return HudUiFactory.AddLayoutElement(target, preferredWidth, preferredHeight);
        }
    }
}
