using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;
using System.Text;

namespace IdleGame.Economy
{
    public sealed partial class EquipmentInventory
    {
        public string GetOwnedSummary(int maxLines)
        {
            int lines = 0;
            var builder = new StringBuilder();
            foreach (EquipmentState state in states)
            {
                if (!state.IsOwned)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(state.Definition.SlotLabel)
                    .Append(" ")
                    .Append(state.Definition.RarityLabel)
                    .Append(" ")
                    .Append(state.Definition.DisplayName)
                    .Append(" Lv.")
                    .Append(state.Level)
                    .Append("/")
                    .Append(state.MaxLevel)
                    .Append(" ")
                    .Append(state.Stars)
                    .Append("성")
                    .Append(" ATK+")
                    .Append(state.AttackBonus)
                    .Append(" HP+")
                    .Append(state.HpBonus)
                    .Append(" x")
                    .Append(state.Count);

                lines += 1;
                if (lines >= maxLines)
                {
                    break;
                }
            }

            return builder.Length > 0 ? builder.ToString() : "보유 장비 없음";
        }
    }
}
