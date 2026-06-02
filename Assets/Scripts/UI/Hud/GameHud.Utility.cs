using System;
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Progression;
using IdleGame.UI.Common;
using IdleGame.UI.Hero;

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
