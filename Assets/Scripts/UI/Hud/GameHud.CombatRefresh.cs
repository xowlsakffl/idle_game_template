using UnityEngine;
using IdleGame.UI.Battle;
using IdleGame.UI.Fortress;
using IdleGame.UI.Navigation;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void RefreshFortressPanel()
        {
            if (battleManager == null || fortressViewRefs == null)
            {
                return;
            }

            FortressPanelView.ApplyState(
                fortressViewRefs,
                FortressPanelStateBuilder.Build(
                    battleManager,
                    value => FormatShortNumber(value),
                    value => FormatShortNumber(value)));
        }

        private void RefreshDamageMeter()
        {
            battleHud.DamageMeterRowStates = DamageMeterStateBuilder.BuildRows(
                battleManager.DeployedHeroes,
                battleManager.GetHeroDamageDone,
                battleManager.GetMaxHeroDamageDone(),
                battleHud.DamageMeterRows.Count,
                GetShortHeroLabel,
                value => FormatShortNumber(value),
                battleHud.DamageMeterHeroScratch,
                battleHud.DamageMeterRowStates);

            DamageMeterView.Apply(
                battleHud.DamageMeterText,
                battleHud.DamageMeterRowStates,
                battleHud.DamageMeterRows,
                battleHud.DamageMeterFills,
                battleHud.DamageMeterRowTexts);
        }

        private void RefreshBattlefieldVisuals()
        {
            battleHud.Renderer.Refresh(new BattleHudRenderArgs
            {
                ProgressManager = progressManager,
                BattleManager = battleManager,
                BattlefieldWorldView = battlefieldWorldView,
                VisualState = battleHud.VisualState,
                BattlefieldRect = battleHud.BattlefieldRect,
                BattlefieldWorldImage = battleHud.BattlefieldWorldImage,
                DamagePopupText = battleHud.DamagePopupText,
                CenterSpawnText = battleHud.CenterSpawnText,
                HeroBattleImages = battleHud.HeroImages,
                HeroBattleTexts = battleHud.HeroTexts,
                HeroBattleRects = battleHud.HeroRects,
                EnemyBattleImages = battleHud.EnemyImages,
                EnemyBattleTexts = battleHud.EnemyTexts,
                EnemyBattleRects = battleHud.EnemyRects,
                EnemyHpBarObjects = battleHud.EnemyHpBars,
                EnemyHpFillImages = battleHud.EnemyHpFills,
                HitFlashRemaining = runtimeTickState.HitFlashRemaining,
                HeroAttackFlashRemaining = runtimeTickState.HeroAttackFlashRemaining,
                Time = Time.time,
                DeltaTime = Time.deltaTime
            });
        }

        private bool IsBattlePanelVisible()
        {
            return !contentPanelOpen || activeTab == HudTab.Growth;
        }
    }
}
