using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IdleGame.Data;
using IdleGame.UI.Battle;
using IdleGame.UI.Common;
using IdleGame.UI.Debugging;
using IdleGame.UI.Growth;
using IdleGame.UI.Header;
using IdleGame.UI.Hero.Formation;
using IdleGame.UI.Navigation;
using IdleGame.UI.Summon;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void RefreshVisibleContentPanel(HudPanelRefreshState panelState)
        {
            if (panelState == null)
            {
                return;
            }

            if (panelState.GrowthPanelOpen)
            {
                RefreshGrowthStatusPanel(panelState);
                RefreshGrowthPanel(panelState);
                return;
            }

            if (panelState.HeroPanelOpen)
            {
                RefreshHeroPanel(panelState);
                return;
            }

            if (panelState.FortressPanelOpen)
            {
                RefreshFortressPanelIfNeeded(panelState);
                return;
            }

            if (panelState.SummonPanelOpen)
            {
                RefreshSummonPanel(panelState);
                return;
            }

            if (panelState.SupportPanelOpen)
            {
                RefreshSupportPanel(panelState);
                return;
            }

            if (panelState.StagePanelOpen)
            {
                RefreshStagePanel(panelState);
                return;
            }

            if (panelState.DebugPanelOpen)
            {
                RefreshDebugPanel(panelState);
            }
        }

        private void RefreshClosedPanelAttention(HudPanelRefreshState panelState)
        {
            if (panelState == null)
            {
                return;
            }

            if (!panelState.GrowthPanelOpen && panelState.RefreshGrowthAttention)
            {
                RefreshGrowthPanel(panelState);
            }

            if (!panelState.HeroPanelOpen && panelState.RefreshHeroAttention)
            {
                RefreshHeroPanel(panelState);
            }
        }

        private void RefreshHeaderPanel(HudPanelRefreshState panelState, StageDefinition stage)
        {
            if (!panelState.RefreshHeader)
            {
                return;
            }

            HeaderHudPresenter.Refresh(new HeaderHudPresenterArgs
            {
                Wallet = wallet,
                AccountProgressManager = accountProgressManager,
                BattleManager = battleManager,
                Stage = stage,
                ResourceText = resourceText,
                RubyResourceText = rubyResourceText,
                StageText = battleHud.StageText,
                ModeText = battleHud.ModeText,
                AccountLevelText = accountLevelText,
                FieldStagePillText = fieldStagePillText,
                AccountExpFill = accountExpFill,
                FormatGameNumber = value => FormatShortNumber(value),
                FormatDoubleNumber = value => FormatShortNumber(value),
                FormatCountNumber = FormatCountNumber
            });
        }

        private void RefreshBattlePanel(HudPanelRefreshState panelState, StageDefinition stage)
        {
            if (!panelState.RefreshBattlePanel)
            {
                return;
            }

            BattleProgressPresenter.Refresh(new BattleProgressPresenterArgs
            {
                BattleManager = battleManager,
                Stage = stage,
                TargetText = battleHud.TargetText,
                ProgressFill = battleHud.HpFill,
                ProgressValueText = battleHud.HpText,
                ProgressText = battleHud.ProgressText,
                GuideQuestText = guideQuestText
            });

            RefreshDamageMeter();

            BattleLogPresenter.Refresh(new BattleLogPresenterArgs
            {
                BattleManager = battleManager,
                SupportText = battleHud.SupportText,
                LogText = battleHud.LogText,
                RewardText = battleHud.RewardText
            });

            RefreshBattlefieldVisuals();

            BattleControlPresenter.Refresh(new BattleControlPresenterArgs
            {
                BattleManager = battleManager,
                SpeedManager = speedManager,
                SkillAutoButton = battleHud.SkillAutoButton,
                FeverAutoButton = battleHud.FeverAutoButton,
                SpeedCycleButton = battleHud.SpeedCycleButton
            });
        }

        private void RefreshSummonPanel(HudPanelRefreshState panelState)
        {
            if (!panelState.RefreshSummonPanel)
            {
                return;
            }

            SummonPanelPresenter.Refresh(new SummonPanelPresenterArgs
            {
                GachaManager = gachaManager,
                EquipmentInventory = equipmentInventory,
                ResultText = gachaText,
                RefreshPanel = true
            });
        }

        private void RefreshGrowthStatusPanel(HudPanelRefreshState panelState)
        {
            if (!panelState.GrowthPanelOpen)
            {
                return;
            }

            HudStatusPresenter.Refresh(new HudStatusPresenterArgs
            {
                BattleManager = battleManager,
                TotalCombatPowerText = totalCombatPowerText,
                GrowthNoticeText = growthNoticeText,
                GrowthNoticeMessage = runtimeTickState.NoticeMessage,
                GrowthNoticeUntil = runtimeTickState.NoticeUntil,
                CurrentTime = Time.unscaledTime,
                FormatShortNumber = value => FormatShortNumber(value)
            });
        }

        private void RefreshGrowthPanel(HudPanelRefreshState panelState)
        {
            if (!panelState.RefreshGrowthAttention)
            {
                return;
            }

            cachedGrowthAttention = GrowthPanelPresenter.Refresh(new GrowthPanelPresenterArgs
            {
                AbilityManager = abilityManager,
                Wallet = wallet,
                SelectedGrowthLevelStep = selectedGrowthLevelStep,
                RefreshPanel = panelState.RefreshGrowthPanel,
                GrowthStepButtons = growthStepButtons,
                AbilityButtonTexts = abilityButtonTexts,
                AbilityCostBadgeTexts = abilityCostBadgeTexts,
                AbilityNotificationDots = abilityNotificationDots,
                FormatShortNumber = FormatShortNumber
            });
        }

        private void RefreshHeroPanel(HudPanelRefreshState panelState)
        {
            if (panelState.RefreshHeroPanel)
            {
                RefreshHeroFormationPanel();
            }

            if (!panelState.RefreshHeroAttention)
            {
                return;
            }

            cachedHeroAttention = HeroRosterPresenter.Refresh(new HeroRosterPresenterArgs
            {
                BattleManager = battleManager,
                Wallet = wallet,
                RefreshPanel = panelState.RefreshHeroPanel,
                SelectedHeroForPlacement = heroFormationState.SelectedHeroForPlacement,
                HeroRosterButtons = heroHud.RosterButtons,
                HeroButtonTexts = heroHud.HeroButtonTexts,
                HeroRosterActionButtons = heroHud.RosterActionButtons,
                HeroRosterDeployedOverlays = heroHud.RosterDeployedOverlays,
                HeroNotificationDots = heroHud.NotificationDots,
                CachedCardStates = heroHud.RosterCardStates,
                IsHeroInFormation = IsHeroInEditingFormation,
                FormatShortNumber = FormatShortNumber,
                GetShortHeroLabel = GetShortHeroLabel
            });
        }

        private void RefreshSupportPanel(HudPanelRefreshState panelState)
        {
            if (!panelState.RefreshSupportPanel)
            {
                return;
            }

            SecondaryPanelView.ApplySupportPanelState(
                supportSummaryText,
                battleManager.Skills,
                battleManager.Pets,
                battleManager.PartyAttackPower,
                battleManager.PetGoldBonusPercent,
                value => FormatShortNumber(value),
                heroHud.SkillStatusTexts,
                heroHud.PetStatusTexts);
        }

        private void RefreshStagePanel(HudPanelRefreshState panelState)
        {
            if (!panelState.RefreshStagePanel)
            {
                return;
            }

            foreach (KeyValuePair<string, Button> pair in stageButtons)
            {
                bool unlocked = GameData.IsStageUnlocked(pair.Key, progressManager.HighestStageId);
                pair.Value.interactable = unlocked;
                Text text = pair.Value.GetComponentInChildren<Text>(true);
                if (text != null)
                {
                    text.text = pair.Key == progressManager.CurrentStageId ? "[" + pair.Key + "]" : pair.Key;
                }
            }
        }

    }
}
