using IdleGame.Battle;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.UI.Hero.TotemRune
{
    public sealed class TotemRuneActionResult
    {
        public bool Success;
        public string Message;
    }

    public static class TotemRuneActionService
    {
        public static int GetEquippedRuneSlot(BattleManager battleManager, int preset, string runeId)
        {
            if (battleManager == null || string.IsNullOrEmpty(runeId))
            {
                return 0;
            }

            for (int slot = 1; slot <= GameData.MaxRuneSlots; slot++)
            {
                if (battleManager.GetEquippedRuneId(preset, slot) == runeId)
                {
                    return slot;
                }
            }

            return 0;
        }

        public static bool IsRuneEquipped(BattleManager battleManager, int preset, string runeId)
        {
            return GetEquippedRuneSlot(battleManager, preset, runeId) > 0;
        }

        public static TotemRuneActionResult TryStartRuneEquip(BattleManager battleManager, string runeId)
        {
            if (battleManager == null || string.IsNullOrEmpty(runeId))
            {
                return new TotemRuneActionResult();
            }

            RuneState state = battleManager.GetRuneState(runeId);
            if (state == null || !state.Unlocked)
            {
                return new TotemRuneActionResult
                {
                    Message = "장착할 수 없는 룬입니다."
                };
            }

            return new TotemRuneActionResult
            {
                Success = true,
                Message = state.Definition.DisplayName + "을 장착할 룬 슬롯을 선택하세요."
            };
        }

        public static TotemRuneActionResult TryClearEquippedRune(BattleManager battleManager, int preset, string runeId)
        {
            int equippedSlot = GetEquippedRuneSlot(battleManager, preset, runeId);
            if (equippedSlot <= 0)
            {
                return new TotemRuneActionResult();
            }

            bool cleared = battleManager.ClearRuneForPreset(preset, equippedSlot);
            return new TotemRuneActionResult
            {
                Success = cleared,
                Message = cleared ? equippedSlot + "번 룬을 해제했습니다." : "룬을 해제할 수 없습니다."
            };
        }

        public static TotemRuneActionResult TryEquipRuneInSlot(BattleManager battleManager, int preset, int slot, string runeId)
        {
            if (battleManager == null || string.IsNullOrEmpty(runeId))
            {
                return new TotemRuneActionResult();
            }

            if (!battleManager.IsRuneSlotUnlocked(slot))
            {
                return new TotemRuneActionResult
                {
                    Message = slot + "번 룬 슬롯은 계정 Lv." + battleManager.GetRuneSlotUnlockLevel(slot) + "에 해금됩니다."
                };
            }

            bool equipped = battleManager.SetRuneForPreset(preset, slot, runeId);
            RuneState state = battleManager.GetRuneState(runeId);
            return new TotemRuneActionResult
            {
                Success = equipped,
                Message = equipped
                    ? (state != null ? state.Definition.DisplayName : "룬") + "을 " + slot + "번 슬롯에 장착했습니다."
                    : "룬을 장착할 수 없습니다."
            };
        }

        public static TotemRuneActionResult TryRemoveRuneFromSlot(BattleManager battleManager, int preset, int slot)
        {
            if (battleManager == null)
            {
                return new TotemRuneActionResult();
            }

            string equippedRuneId = battleManager.GetEquippedRuneId(preset, slot);
            if (string.IsNullOrEmpty(equippedRuneId))
            {
                return new TotemRuneActionResult();
            }

            RuneState state = battleManager.GetRuneState(equippedRuneId);
            bool removed = battleManager.ClearRuneForPreset(preset, slot);
            return new TotemRuneActionResult
            {
                Success = removed,
                Message = removed ? (state != null ? state.Definition.DisplayName : "룬") + "을 해제했습니다." : "룬을 해제할 수 없습니다."
            };
        }

        public static TotemRuneActionResult TryPromoteRune(BattleManager battleManager, string runeId)
        {
            if (battleManager == null || string.IsNullOrEmpty(runeId))
            {
                return new TotemRuneActionResult();
            }

            RuneState state = battleManager.GetRuneState(runeId);
            if (state == null)
            {
                return new TotemRuneActionResult();
            }

            if (state.IsMaxed)
            {
                return new TotemRuneActionResult
                {
                    Message = "이미 최고 등급입니다."
                };
            }

            if (!state.CanPromote)
            {
                return new TotemRuneActionResult
                {
                    Message = "같은 등급 룬이 부족합니다. " + state.FormatCurrentSynthesisProgress()
                };
            }

            bool promoted = battleManager.TryPromoteRune(runeId);
            return new TotemRuneActionResult
            {
                Success = promoted,
                Message = promoted ? state.Definition.DisplayName + " 합성 완료" : "룬 합성에 실패했습니다."
            };
        }

        public static bool CanPromoteRune(BattleManager battleManager, string runeId)
        {
            if (battleManager == null || string.IsNullOrEmpty(runeId))
            {
                return false;
            }

            RuneState state = battleManager.GetRuneState(runeId);
            return state != null && state.CanPromote;
        }

        public static TotemRuneActionResult TryLevelOrPromoteTotem(
            BattleManager battleManager,
            CurrencyWallet wallet,
            string totemId)
        {
            if (battleManager == null || string.IsNullOrEmpty(totemId))
            {
                return new TotemRuneActionResult();
            }

            TotemState state = battleManager.GetTotemState(totemId);
            if (state == null)
            {
                return new TotemRuneActionResult();
            }

            if (!state.Unlocked)
            {
                return new TotemRuneActionResult
                {
                    Message = "보유하지 않은 토템입니다."
                };
            }

            if (state.IsMaxed)
            {
                return TryPromoteTotem(battleManager, wallet, state);
            }

            if (wallet == null || wallet.TotemEssence < state.LevelUpCost)
            {
                return new TotemRuneActionResult
                {
                    Message = "토템 정수가 부족합니다."
                };
            }

            bool leveled = battleManager.TryLevelUpTotem(totemId);
            return new TotemRuneActionResult
            {
                Success = leveled,
                Message = leveled ? string.Empty : "토템 강화에 실패했습니다."
            };
        }

        public static bool CanLevelOrPromoteTotem(BattleManager battleManager, CurrencyWallet wallet, string totemId)
        {
            if (battleManager == null || wallet == null || string.IsNullOrEmpty(totemId))
            {
                return false;
            }

            TotemState state = battleManager.GetTotemState(totemId);
            return state != null
                && state.Unlocked
                && ((!state.IsMaxed && wallet.TotemEssence >= state.LevelUpCost)
                    || (state.CanPromote && battleManager.CanPromoteTotemTier(totemId) && wallet.TotemEssence >= state.PromoteCost));
        }

        private static TotemRuneActionResult TryPromoteTotem(BattleManager battleManager, CurrencyWallet wallet, TotemState state)
        {
            if (!state.CanPromote)
            {
                return new TotemRuneActionResult
                {
                    Message = "이미 최대 등급입니다."
                };
            }

            if (!battleManager.CanPromoteTotemTier(state.Definition.Id))
            {
                return new TotemRuneActionResult
                {
                    Message = "같은 등급 토템 6개를 모두 Lv." + TotemDefinition.MaxLevel + "까지 강화해야 합니다."
                };
            }

            if (wallet == null || wallet.TotemEssence < state.PromoteCost)
            {
                return new TotemRuneActionResult
                {
                    Message = "토템 정수가 부족합니다."
                };
            }

            bool promoted = battleManager.TryPromoteTotem(state.Definition.Id);
            return new TotemRuneActionResult
            {
                Success = promoted,
                Message = promoted ? string.Empty : "토템 진화에 실패했습니다."
            };
        }
    }
}
