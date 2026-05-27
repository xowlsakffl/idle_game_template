using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public sealed class HeroDetailEquipmentActionResult
    {
        public bool Success;
        public bool NeedsSelection;
        public bool SelectSlotFilter;
        public bool ClearSelectedEquipment;
        public bool ClearSelectedEquipmentDetail;
        public bool ClearSlotSelection;
        public bool CloseEquipmentDetailPopup;
        public bool CloseBulkDismantlePrompt;
        public string Message;
    }

    public static class HeroDetailEquipmentActionService
    {
        public static HeroDetailEquipmentActionResult ToggleDismantleSelection(
            string equipmentCopyKey,
            ISet<string> selectedKeys,
            EquipmentInventory equipmentInventory)
        {
            if (selectedKeys == null || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            string equipmentId = HeroDetailEquipmentListBuilder.GetEquipmentIdFromCopyKey(equipmentCopyKey);
            EquipmentState state = equipmentInventory.GetState(equipmentId);
            if (state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult();
            }

            if (equipmentInventory.GetAvailableCount(equipmentId) <= 0)
            {
                selectedKeys.Remove(equipmentCopyKey);
                return new HeroDetailEquipmentActionResult
                {
                    Message = "장착 중인 장비는 분해할 수 없습니다."
                };
            }

            if (selectedKeys.Contains(equipmentCopyKey))
            {
                selectedKeys.Remove(equipmentCopyKey);
            }
            else
            {
                selectedKeys.Add(equipmentCopyKey);
            }

            return new HeroDetailEquipmentActionResult
            {
                Success = true
            };
        }

        public static HeroDetailEquipmentActionResult ToggleHeroEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            string equipmentId,
            bool slotSelectionActive,
            EquipmentSlot selectedSlot)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            EquipmentState state = equipmentInventory.GetState(equipmentId);
            if (state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult();
            }

            if (equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, equipmentId))
            {
                bool unequipped = equipmentInventory.UnequipEquipment(hero.Definition.Id, equipmentId);
                return new HeroDetailEquipmentActionResult
                {
                    Success = unequipped,
                    ClearSelectedEquipment = true,
                    ClearSlotSelection = true
                };
            }

            if (!slotSelectionActive)
            {
                return new HeroDetailEquipmentActionResult
                {
                    NeedsSelection = true
                };
            }

            if (state.Definition.Slot != selectedSlot)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = EquipmentUiText.GetSlotLabel(selectedSlot) + " 칸에는 "
                        + EquipmentUiText.GetSlotLabel(state.Definition.Slot) + " 장비를 장착할 수 없습니다."
                };
            }

            return EquipToHero(hero, equipmentInventory, state);
        }

        public static HeroDetailEquipmentActionResult PlaceSelectedEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            string equipmentId,
            EquipmentSlot slot)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            EquipmentState state = equipmentInventory.GetState(equipmentId);
            if (state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult
                {
                    ClearSelectedEquipment = true
                };
            }

            if (state.Definition.Slot != slot)
            {
                return new HeroDetailEquipmentActionResult
                {
                    SelectSlotFilter = true
                };
            }

            return EquipToHero(hero, equipmentInventory, state);
        }

        public static HeroDetailEquipmentActionResult RemoveSlotEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            EquipmentSlot slot)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            bool removed = equipmentInventory.Unequip(hero.Definition.Id, slot);
            return new HeroDetailEquipmentActionResult
            {
                Success = removed,
                ClearSelectedEquipment = true,
                ClearSlotSelection = true
            };
        }

        public static HeroDetailEquipmentActionResult UnequipAll(HeroState hero, EquipmentInventory equipmentInventory)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            int unequippedCount = equipmentInventory.UnequipAll(hero.Definition.Id);
            return new HeroDetailEquipmentActionResult
            {
                Success = unequippedCount > 0,
                ClearSelectedEquipment = true,
                ClearSelectedEquipmentDetail = true,
                ClearSlotSelection = true,
                CloseEquipmentDetailPopup = true,
                Message = unequippedCount > 0
                    ? "장비 " + unequippedCount + "개를 해제했습니다."
                    : "해제할 장비가 없습니다."
            };
        }

        public static HeroDetailEquipmentActionResult AutoEquip(HeroState hero, EquipmentInventory equipmentInventory)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            int equippedCount = equipmentInventory.EquipBestAvailable(hero.Definition.Id);
            return new HeroDetailEquipmentActionResult
            {
                Success = equippedCount > 0,
                ClearSelectedEquipment = true,
                ClearSelectedEquipmentDetail = true,
                ClearSlotSelection = true,
                CloseEquipmentDetailPopup = true,
                Message = equippedCount > 0
                    ? "강한 장비 " + equippedCount + "개를 자동 장착했습니다."
                    : "장착할 장비가 없습니다."
            };
        }

        public static HeroDetailEquipmentActionResult ToggleDetailEquipment(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            string equipmentId)
        {
            if (hero == null || !hero.IsOwned || equipmentInventory == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            EquipmentState state = equipmentInventory.GetState(equipmentId);
            if (state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult();
            }

            if (equipmentInventory.IsEquipmentEquippedToHero(hero.Definition.Id, state.Definition.Id))
            {
                bool unequipped = equipmentInventory.UnequipEquipment(hero.Definition.Id, state.Definition.Id);
                return new HeroDetailEquipmentActionResult
                {
                    Success = unequipped,
                    ClearSelectedEquipment = true,
                    ClearSlotSelection = true
                };
            }

            HeroDetailEquipmentActionResult result = EquipToHero(hero, equipmentInventory, state);
            if (result.Success)
            {
                result.ClearSelectedEquipmentDetail = true;
                result.CloseEquipmentDetailPopup = true;
            }

            return result;
        }

        public static HeroDetailEquipmentActionResult LevelUp(
            EquipmentInventory equipmentInventory,
            CurrencyWallet wallet,
            string equipmentId)
        {
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
            if (state == null || !state.IsOwned || wallet == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            if (state.Level >= state.MaxLevel)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = state.IsMaxStars ? "이미 최대 레벨입니다." : "승급 후 레벨업할 수 있습니다."
                };
            }

            int cost = state.LevelUpCost;
            if (wallet.EquipmentExpItem < cost || !wallet.SpendEquipmentExpItem(cost))
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "장비 레벨업 책이 부족합니다."
                };
            }

            if (!equipmentInventory.TryLevelUpEquipment(state.Definition.Id))
            {
                wallet.AddEquipmentExpItem(cost);
                return new HeroDetailEquipmentActionResult
                {
                    Message = "장비 레벨업에 실패했습니다."
                };
            }

            return new HeroDetailEquipmentActionResult
            {
                Success = true
            };
        }

        public static bool CanLevelUp(EquipmentInventory equipmentInventory, CurrencyWallet wallet, string equipmentId)
        {
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
            return state != null
                && state.IsOwned
                && wallet != null
                && state.Level < state.MaxLevel
                && wallet.EquipmentExpItem >= state.LevelUpCost;
        }

        public static HeroDetailEquipmentActionResult StarUp(
            EquipmentInventory equipmentInventory,
            string equipmentId)
        {
            EquipmentState state = equipmentInventory != null ? equipmentInventory.GetState(equipmentId) : null;
            if (state == null || !state.IsOwned)
            {
                return new HeroDetailEquipmentActionResult();
            }

            if (state.IsMaxStars)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "이미 최대 성급입니다."
                };
            }

            if (equipmentInventory.GetStarUpMaterialCount(state.Definition.Id) < state.StarUpCost)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "승급 재료 장비가 부족합니다."
                };
            }

            bool starred = equipmentInventory.TryStarUpEquipment(state.Definition.Id);
            return new HeroDetailEquipmentActionResult
            {
                Success = starred,
                Message = starred ? string.Empty : "장비 승급에 실패했습니다."
            };
        }

        public static HeroDetailEquipmentActionResult DismantleSelected(
            EquipmentInventory equipmentInventory,
            CurrencyWallet wallet,
            ISet<string> selectedKeys,
            ICollection<EquipmentSlot> selectedSlots)
        {
            if (selectedKeys == null || selectedKeys.Count <= 0 || equipmentInventory == null || wallet == null)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "분해할 장비를 선택하세요."
                };
            }

            HeroDetailEquipmentListBuilder.PruneInvalidDismantleSelections(
                selectedKeys,
                equipmentInventory,
                selectedSlots);
            if (selectedKeys.Count <= 0)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "분해할 수 있는 선택 장비가 없습니다."
                };
            }

            int dismantledCount = 0;
            int totalReward = 0;
            var keys = new List<string>(selectedKeys);
            foreach (string equipmentCopyKey in keys)
            {
                string equipmentId = HeroDetailEquipmentListBuilder.GetEquipmentIdFromCopyKey(equipmentCopyKey);
                if (!equipmentInventory.TryDismantleEquipment(equipmentId, out int reward))
                {
                    selectedKeys.Remove(equipmentCopyKey);
                    continue;
                }

                dismantledCount += 1;
                totalReward += reward;
                selectedKeys.Remove(equipmentCopyKey);
            }

            if (dismantledCount <= 0)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "장비 분해에 실패했습니다."
                };
            }

            wallet.AddEquipmentExpItem(totalReward);
            return new HeroDetailEquipmentActionResult
            {
                Success = true,
                Message = "장비 " + dismantledCount + "개 분해: 장비책+" + NumberFormatter.Format(totalReward)
            };
        }

        public static HeroDetailEquipmentActionResult BulkDismantle(
            EquipmentInventory equipmentInventory,
            CurrencyWallet wallet,
            HeroRarity selectedRarity,
            ISet<string> selectedKeys)
        {
            if (equipmentInventory == null || wallet == null)
            {
                return new HeroDetailEquipmentActionResult();
            }

            int dismantledCount = equipmentInventory.DismantleByRarity(selectedRarity, null, out int reward);
            if (dismantledCount <= 0)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "분해할 장비가 없습니다."
                };
            }

            wallet.AddEquipmentExpItem(reward);
            selectedKeys?.Clear();
            return new HeroDetailEquipmentActionResult
            {
                Success = true,
                CloseBulkDismantlePrompt = true,
                Message = "장비 " + dismantledCount + "개 분해: 장비책+" + NumberFormatter.Format(reward)
            };
        }

        private static HeroDetailEquipmentActionResult EquipToHero(
            HeroState hero,
            EquipmentInventory equipmentInventory,
            EquipmentState state)
        {
            if (equipmentInventory.GetAvailableCount(state.Definition.Id) <= 0)
            {
                return new HeroDetailEquipmentActionResult
                {
                    Message = "장착 가능한 장비 수량이 부족합니다."
                };
            }

            bool equipped = equipmentInventory.Equip(hero.Definition.Id, state.Definition.Id);
            return new HeroDetailEquipmentActionResult
            {
                Success = equipped,
                ClearSelectedEquipment = equipped,
                ClearSlotSelection = equipped,
                Message = equipped ? string.Empty : "장착 가능한 장비 수량이 부족합니다."
            };
        }
    }
}
