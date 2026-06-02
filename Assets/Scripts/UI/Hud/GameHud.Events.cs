using IdleGame.Battle;

namespace IdleGame.UI.Hud
{
    public sealed partial class GameHud
    {
        private void SubscribeEvents()
        {
            progressManager.Changed += OnProgressChanged;
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
            QueueHudRefresh(HudBattleDirtyFlagResolver.Resolve(flags));
        }

        private void OnGachaChanged()
        {
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
