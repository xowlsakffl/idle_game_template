using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class CurrencyWallet : MonoBehaviour
    {
        private SaveManager saveManager;

        public event Action Changed;

        public GameNumber Gold { get; private set; }
        public long Ruby { get; private set; }
        public GameNumber HeroExpItem { get; private set; }
        public GameNumber EquipmentExpItem { get; private set; }
        public long TotemEssence { get; private set; }
        public long Wood { get; private set; }
        public long Brick { get; private set; }
        public long Iron { get; private set; }
        public long HeroTranscendStone { get; private set; }
        public long HeroSummonTicket { get; private set; }
        public long EquipmentSummonTicket { get; private set; }

        public void Initialize(SaveManager save)
        {
            saveManager = save;
            Gold = GameData.ClampNumber(saveManager.LoadGameNumber(SaveKeys.Gold, 120));
            Ruby = GameData.ClampCount(saveManager.LoadLong(SaveKeys.Ruby, 100));
            HeroExpItem = GameData.ClampNumber(saveManager.LoadGameNumber(SaveKeys.HeroExpItem, 30));
            EquipmentExpItem = GameData.ClampNumber(saveManager.LoadGameNumber(SaveKeys.EquipmentExpItem, 30));
            TotemEssence = GameData.ClampCount(saveManager.LoadLong(SaveKeys.TotemEssence, 120));
            Wood = GameData.ClampCount(saveManager.LoadLong(SaveKeys.Wood, 80));
            Brick = GameData.ClampCount(saveManager.LoadLong(SaveKeys.Brick, 0));
            Iron = GameData.ClampCount(saveManager.LoadLong(SaveKeys.Iron, 0));
            HeroTranscendStone = GameData.ClampCount(saveManager.LoadLong(SaveKeys.HeroTranscendStone, 0));
            HeroSummonTicket = GameData.ClampCount(saveManager.LoadLong(SaveKeys.HeroSummonTicket, 3));
            EquipmentSummonTicket = GameData.ClampCount(saveManager.LoadLong(SaveKeys.EquipmentSummonTicket, 3));
            Save();
            NotifyChanged();
        }




    }
}
