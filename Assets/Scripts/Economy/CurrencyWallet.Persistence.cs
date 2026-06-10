using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class CurrencyWallet
    {
        private void Save()
        {
            saveManager.SaveGameNumber(SaveKeys.Gold, Gold);
            saveManager.SaveLong(SaveKeys.Ruby, Ruby);
            saveManager.SaveGameNumber(SaveKeys.HeroExpItem, HeroExpItem);
            saveManager.SaveGameNumber(SaveKeys.EquipmentExpItem, EquipmentExpItem);
            saveManager.SaveLong(SaveKeys.TotemEssence, TotemEssence);
            saveManager.SaveLong(SaveKeys.Wood, Wood);
            saveManager.SaveLong(SaveKeys.Brick, Brick);
            saveManager.SaveLong(SaveKeys.Iron, Iron);
            saveManager.SaveLong(SaveKeys.HeroTranscendStone, HeroTranscendStone);
            saveManager.SaveLong(SaveKeys.HeroSummonTicket, HeroSummonTicket);
            saveManager.SaveLong(SaveKeys.EquipmentSummonTicket, EquipmentSummonTicket);
            saveManager.SaveLong(SaveKeys.DungeonTicket, DungeonTicket);
            saveManager.Flush();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
