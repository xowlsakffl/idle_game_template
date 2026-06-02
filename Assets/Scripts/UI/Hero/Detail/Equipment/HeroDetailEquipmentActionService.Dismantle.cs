using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public static partial class HeroDetailEquipmentActionService
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
    }
}
