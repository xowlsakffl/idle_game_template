using IdleGame.Battle;
using UnityEngine;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void SubscribeEvents()
        {
            progressManager.Changed += OnProgressChanged;
            dungeonProgressManager.Changed += OnDungeonChanged;
            wallet.Changed += OnWalletChanged;
            accountProgressManager.Changed += OnAccountProgressChanged;
            abilityManager.Changed += OnAbilityChanged;
            speedManager.Changed += OnSpeedChanged;
            battleManager.ChangedWithFlags += OnBattleChanged;
            gachaManager.Changed += OnGachaChanged;
            equipmentInventory.Changed += OnEquipmentInventoryChanged;
        }

        private void UnsubscribeEvents()
        {
            if (progressManager != null)
            {
                progressManager.Changed -= OnProgressChanged;
            }

            if (dungeonProgressManager != null)
            {
                dungeonProgressManager.Changed -= OnDungeonChanged;
            }

            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }

            if (accountProgressManager != null)
            {
                accountProgressManager.Changed -= OnAccountProgressChanged;
            }

            if (abilityManager != null)
            {
                abilityManager.Changed -= OnAbilityChanged;
            }

            if (speedManager != null)
            {
                speedManager.Changed -= OnSpeedChanged;
            }

            if (battleManager != null)
            {
                battleManager.ChangedWithFlags -= OnBattleChanged;
            }

            if (gachaManager != null)
            {
                gachaManager.Changed -= OnGachaChanged;
            }

            if (equipmentInventory != null)
            {
                equipmentInventory.Changed -= OnEquipmentInventoryChanged;
            }
        }

        private void OnProgressChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Header | HudDirtyFlags.Battle | HudDirtyFlags.Stage | HudDirtyFlags.Navigation);
        }

        private void OnDungeonChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Stage | HudDirtyFlags.Navigation | HudDirtyFlags.Debug);
        }

        private void OnWalletChanged()
        {
            HudWalletSnapshot currentSnapshot = HudWalletSnapshot.Capture(wallet);
            HudDirtyFlags flags = HudWalletDirtyFlagResolver.Resolve(lastWalletSnapshot, currentSnapshot);
            lastWalletSnapshot = currentSnapshot;
            QueueHudRefresh(flags);
        }

        private void OnAccountProgressChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Header
                | HudDirtyFlags.Hero
                | HudDirtyFlags.HeroDetail
                | HudDirtyFlags.Facility
                | HudDirtyFlags.Debug
                | HudDirtyFlags.Navigation);
        }

        private void OnAbilityChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Battle | HudDirtyFlags.Growth | HudDirtyFlags.Navigation);
        }

        private void OnSpeedChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Battle | HudDirtyFlags.Debug);
        }

        private void OnBattleChanged(BattleChangeFlags flags)
        {
            if (battleManager != null
                && dungeonClearPopupOpen
                && dungeonClearPopupCloseOnNextRun
                && battleManager.IsDungeonRunActive)
            {
                HideDungeonClearPopupOnly();
            }

            if (battleManager != null && battleManager.DungeonClearResultSequence != observedDungeonClearResultSequence)
            {
                observedDungeonClearResultSequence = battleManager.DungeonClearResultSequence;
                if (observedDungeonClearResultSequence > 0)
                {
                    OpenDungeonClearPopup(
                        battleManager.LastDungeonClearKind,
                        battleManager.LastDungeonClearLevel,
                        battleManager.LastDungeonClearRewardText,
                        false,
                        battleManager.LastDungeonClearEndedRepeat,
                        battleManager.LastDungeonClearContinuesRepeat);
                    flags |= BattleChangeFlags.BattleLog;
                }
            }

            QueueHudRefresh(HudBattleDirtyFlagResolver.Resolve(flags));
        }

        private void OnGachaChanged()
        {
            if (gachaManager != null && gachaManager.ResultSequence != observedGachaResultSequence)
            {
                observedGachaResultSequence = gachaManager.ResultSequence;
                summonResultPopupOpen = gachaManager.LastOutcomes.Count > 0;
            }

            QueueHudRefresh(HudDirtyFlags.Summon | HudDirtyFlags.Hero | HudDirtyFlags.HeroDetail | HudDirtyFlags.Navigation);
        }

        private void OnEquipmentInventoryChanged()
        {
            QueueHudRefresh(HudDirtyFlags.Summon | HudDirtyFlags.Hero | HudDirtyFlags.HeroDetail | HudDirtyFlags.Navigation);
        }

        private void QueueHudRefresh(HudDirtyFlags flags)
        {
            if (flags == HudDirtyFlags.None)
            {
                return;
            }

            dirtyHudFlags |= flags;
            hudRefreshQueued = true;
        }
    }
}
