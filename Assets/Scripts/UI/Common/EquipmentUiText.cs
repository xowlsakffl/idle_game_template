using System.Collections.Generic;
using IdleGame.Data;

namespace IdleGame.UI.Common
{
    public static class EquipmentUiText
    {
        public static readonly EquipmentSlot[] FilterSlots =
        {
            EquipmentSlot.Weapon,
            EquipmentSlot.Hat,
            EquipmentSlot.Armor,
            EquipmentSlot.Accessory,
            EquipmentSlot.Potion
        };

        public static string GetSlotLabel(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    return "무기";
                case EquipmentSlot.Hat:
                    return "모자";
                case EquipmentSlot.Armor:
                    return "갑옷";
                case EquipmentSlot.Accessory:
                    return "장신구";
                case EquipmentSlot.Potion:
                    return "포션";
                default:
                    return "미정";
            }
        }

        public static string BuildFilterButtonLabel(EquipmentSlot slot, ICollection<EquipmentSlot> selectedSlots)
        {
            bool selected = selectedSlots != null && selectedSlots.Contains(slot);
            return (selected ? "[x] " : "[ ] ") + GetSlotLabel(slot);
        }

        public static string BuildFilterSummaryLabel(ICollection<EquipmentSlot> selectedSlots)
        {
            if (selectedSlots == null || selectedSlots.Count <= 0)
            {
                return "없음";
            }

            if (selectedSlots.Count >= FilterSlots.Length)
            {
                return "전체";
            }

            string label = string.Empty;
            foreach (EquipmentSlot slot in FilterSlots)
            {
                if (!selectedSlots.Contains(slot))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(label))
                {
                    label += ", ";
                }

                label += GetSlotLabel(slot);
            }

            return label;
        }

        public static string BuildInventoryCardText(
            EquipmentDefinition equipment,
            EquipmentState state,
            string equippedHeroLabel,
            int copyNumber)
        {
            int level = state != null ? state.Level : 1;
            int maxLevel = state != null ? state.MaxLevel : equipment.GetMaxLevel(0);
            int stars = state != null ? state.Stars : 0;
            int attack = state != null ? state.AttackBonus : equipment.GetAttackBonus(1, 0);
            int hp = state != null ? state.HpBonus : equipment.GetHpBonus(1, 0);
            bool equipped = !string.IsNullOrEmpty(equippedHeroLabel);
            string status = equipped
                ? "<color=#FFD84D>장착중 " + equippedHeroLabel + "</color>"
                : equipment.SlotLabel;
            string copyLabel = equipped ? "장착본" : "개별 #" + copyNumber;

            return status
                + "\n" + equipment.RarityLabel + " " + equipment.DisplayName
                + "\nLv." + level + "/" + maxLevel + "  " + stars + "성"
                + "\nATK+" + NumberFormatter.Format(attack) + " HP+" + NumberFormatter.Format(hp)
                + "\n" + copyLabel;
        }

        public static string BuildDismantleCardText(EquipmentState state, int copyNumber, int reward)
        {
            return "Lv." + state.Level
                + "\n" + state.Definition.RarityLabel + " " + state.Definition.SlotLabel
                + "\n" + state.Definition.DisplayName
                + "\n분해 #" + copyNumber
                + "\n+" + NumberFormatter.Format(reward);
        }

        public static string BuildDetailEffectText(EquipmentState state)
        {
            string slotEffect;
            switch (state.Definition.Slot)
            {
                case EquipmentSlot.Weapon:
                    slotEffect = "공격력 +" + NumberFormatter.Format(state.AttackBonus);
                    break;
                case EquipmentSlot.Hat:
                    slotEffect = "체력 +" + NumberFormatter.Format(state.HpBonus);
                    break;
                case EquipmentSlot.Armor:
                    slotEffect = "받는 피해 감소 +" + (1 + state.Stars) + "%";
                    break;
                case EquipmentSlot.Accessory:
                    slotEffect = "치명타 데미지 +" + (5 + state.Stars * 2) + "%";
                    break;
                case EquipmentSlot.Potion:
                    slotEffect = "전투 회복량 +" + (3 + state.Stars) + "%";
                    break;
                default:
                    slotEffect = "기본 능력 강화";
                    break;
            }

            return "<color=#80FF5C>" + state.Definition.RarityLabel + " 세트</color>"
                + "\n" + state.Definition.DisplayName
                + "\n" + state.Definition.SlotLabel + " 숙련"
                + "\n\n<color=#80FF5C>3세트 효과</color>"
                + "\n최종 데미지 증가 +" + (10 + state.Stars * 2) + "%"
                + "\n\n<color=#80FF5C>5세트 효과</color>"
                + "\n" + slotEffect
                + "\n레벨 " + state.Level + " 기준 능력치 적용";
        }
    }
}
