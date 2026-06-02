using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.UI.Common;

namespace IdleGame.UI.Hero.Detail.Equipment
{
    public static partial class HeroDetailEquipmentActionService
    {
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
    }
}
