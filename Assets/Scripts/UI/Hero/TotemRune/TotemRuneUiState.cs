using IdleGame.Battle;
using IdleGame.Economy;

namespace IdleGame.UI.Hero.TotemRune
{
    public sealed class TotemRuneUiActionResult
    {
        public TotemRuneActionResult ActionResult;
        public string NoticeMessage;
        public bool ShouldUpdateView;
        public bool ShouldRefreshRunePanel;
        public bool ShouldRefreshTotemPanel;
        public bool SwitchToHeroFormation;
    }

    public sealed class TotemRuneUiState
    {
        private const string DefaultTotemId = "TOTEM_COMBAT";
        private const string DefaultRuneId = "RUNE_STRIKE";

        public string SelectedTotemId { get; private set; } = DefaultTotemId;
        public string PendingTotemEquipId { get; private set; } = string.Empty;
        public string SelectedRuneId { get; private set; } = DefaultRuneId;
        public string PendingRuneEquipId { get; private set; } = string.Empty;
        public int SelectedRuneSlot { get; private set; } = 1;

        public void ResetRuntime()
        {
            SelectedTotemId = DefaultTotemId;
            PendingTotemEquipId = string.Empty;
            SelectedRuneId = DefaultRuneId;
            PendingRuneEquipId = string.Empty;
            SelectedRuneSlot = 1;
        }

        public void SetResolvedTotem(string totemId)
        {
            if (!string.IsNullOrEmpty(totemId))
            {
                SelectedTotemId = totemId;
            }
        }

        public void SetResolvedRune(int slot, string runeId)
        {
            SelectedRuneSlot = slot;
            if (!string.IsNullOrEmpty(runeId))
            {
                SelectedRuneId = runeId;
            }
        }

        public TotemRuneUiActionResult SelectRune(string runeId)
        {
            SelectedRuneId = runeId;
            PendingRuneEquipId = string.Empty;
            PendingTotemEquipId = string.Empty;
            return new TotemRuneUiActionResult
            {
                ShouldRefreshRunePanel = true
            };
        }

        public TotemRuneUiActionResult StartPendingRuneEquip(BattleManager battleManager, int preset, string runeId)
        {
            if (battleManager == null || string.IsNullOrEmpty(runeId))
            {
                return new TotemRuneUiActionResult();
            }

            SelectedRuneId = runeId;
            PendingTotemEquipId = string.Empty;

            int equippedSlot = TotemRuneActionService.GetEquippedRuneSlot(battleManager, preset, runeId);
            if (equippedSlot > 0)
            {
                PendingRuneEquipId = string.Empty;
                return BuildUpdateResult(TotemRuneActionService.TryClearEquippedRune(battleManager, preset, runeId));
            }

            TotemRuneActionResult result = TotemRuneActionService.TryStartRuneEquip(battleManager, runeId);
            if (!result.Success)
            {
                PendingRuneEquipId = string.Empty;
                return BuildUpdateResult(result);
            }

            PendingRuneEquipId = runeId;
            return new TotemRuneUiActionResult
            {
                ActionResult = result,
                ShouldUpdateView = true,
                SwitchToHeroFormation = true
            };
        }

        public TotemRuneUiActionResult HandleRuneSlotClick(BattleManager battleManager, int preset, int slot)
        {
            if (string.IsNullOrEmpty(PendingRuneEquipId))
            {
                return new TotemRuneUiActionResult();
            }

            SelectedRuneSlot = slot;
            return TryEquipPendingRuneInSlot(battleManager, preset, slot);
        }

        public TotemRuneUiActionResult TryEquipPendingRuneInSlot(BattleManager battleManager, int preset, int slot)
        {
            if (battleManager == null || string.IsNullOrEmpty(PendingRuneEquipId))
            {
                return new TotemRuneUiActionResult();
            }

            SelectedRuneSlot = slot;
            SelectedRuneId = PendingRuneEquipId;
            TotemRuneActionResult result = TotemRuneActionService.TryEquipRuneInSlot(
                battleManager,
                preset,
                slot,
                PendingRuneEquipId);
            if (result.Success)
            {
                PendingRuneEquipId = string.Empty;
            }

            return BuildUpdateResult(result);
        }

        public TotemRuneUiActionResult RemoveRuneFromSlot(BattleManager battleManager, int preset, int slot)
        {
            if (battleManager == null)
            {
                return new TotemRuneUiActionResult();
            }

            PendingRuneEquipId = string.Empty;
            return BuildUpdateResult(TotemRuneActionService.TryRemoveRuneFromSlot(battleManager, preset, slot));
        }

        public TotemRuneUiActionResult EquipSelectedRune(BattleManager battleManager, int preset)
        {
            if (battleManager == null || string.IsNullOrEmpty(SelectedRuneId))
            {
                return new TotemRuneUiActionResult();
            }

            return StartPendingRuneEquip(battleManager, preset, SelectedRuneId);
        }

        public TotemRuneUiActionResult PromoteSelectedRune(BattleManager battleManager)
        {
            if (battleManager == null || string.IsNullOrEmpty(SelectedRuneId))
            {
                return new TotemRuneUiActionResult();
            }

            return BuildUpdateResult(TotemRuneActionService.TryPromoteRune(battleManager, SelectedRuneId));
        }

        public bool CanPromoteSelectedRune(BattleManager battleManager)
        {
            return TotemRuneActionService.CanPromoteRune(battleManager, SelectedRuneId);
        }

        public TotemRuneUiActionResult SelectTotem(string totemId)
        {
            SelectedTotemId = totemId;
            PendingTotemEquipId = string.Empty;
            return new TotemRuneUiActionResult
            {
                ShouldRefreshTotemPanel = true
            };
        }

        public TotemRuneUiActionResult EquipSelectedTotem()
        {
            PendingTotemEquipId = string.Empty;
            return new TotemRuneUiActionResult
            {
                NoticeMessage = TotemRuneActionService.GetTotemGlobalEquipMessage()
            };
        }

        public TotemRuneUiActionResult HandleTotemSlotClick()
        {
            return new TotemRuneUiActionResult
            {
                NoticeMessage = TotemRuneActionService.GetTotemSlotDisabledMessage()
            };
        }

        public TotemRuneUiActionResult TryEquipPendingTotem()
        {
            PendingTotemEquipId = string.Empty;
            return new TotemRuneUiActionResult
            {
                NoticeMessage = TotemRuneActionService.GetTotemGlobalEquipMessage()
            };
        }

        public TotemRuneUiActionResult RemoveTotem()
        {
            PendingTotemEquipId = string.Empty;
            return new TotemRuneUiActionResult
            {
                NoticeMessage = TotemRuneActionService.GetTotemRemoveDisabledMessage()
            };
        }

        public TotemRuneUiActionResult LevelSelectedTotem(BattleManager battleManager, CurrencyWallet wallet)
        {
            if (battleManager == null || string.IsNullOrEmpty(SelectedTotemId))
            {
                return new TotemRuneUiActionResult();
            }

            return BuildUpdateResult(TotemRuneActionService.TryLevelOrPromoteTotem(battleManager, wallet, SelectedTotemId));
        }

        public bool CanLevelSelectedTotem(BattleManager battleManager, CurrencyWallet wallet)
        {
            return TotemRuneActionService.CanLevelOrPromoteTotem(battleManager, wallet, SelectedTotemId);
        }

        private static TotemRuneUiActionResult BuildUpdateResult(TotemRuneActionResult result)
        {
            return new TotemRuneUiActionResult
            {
                ActionResult = result,
                ShouldUpdateView = true
            };
        }
    }
}
