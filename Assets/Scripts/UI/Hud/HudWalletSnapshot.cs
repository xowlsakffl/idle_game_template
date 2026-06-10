using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.UI.Hud
{
    public readonly struct HudWalletSnapshot
    {
        public readonly bool IsValid;
        public readonly GameNumber Gold;
        public readonly long Ruby;
        public readonly GameNumber HeroExpItem;
        public readonly GameNumber EquipmentExpItem;
        public readonly long TotemEssence;
        public readonly long Wood;
        public readonly long Brick;
        public readonly long Iron;
        public readonly long HeroTranscendStone;
        public readonly long HeroSummonTicket;
        public readonly long EquipmentSummonTicket;
        public readonly long DungeonTicket;

        private HudWalletSnapshot(CurrencyWallet wallet)
        {
            IsValid = wallet != null;
            Gold = wallet != null ? wallet.Gold : GameNumber.Zero;
            Ruby = wallet != null ? wallet.Ruby : 0;
            HeroExpItem = wallet != null ? wallet.HeroExpItem : GameNumber.Zero;
            EquipmentExpItem = wallet != null ? wallet.EquipmentExpItem : GameNumber.Zero;
            TotemEssence = wallet != null ? wallet.TotemEssence : 0;
            Wood = wallet != null ? wallet.Wood : 0;
            Brick = wallet != null ? wallet.Brick : 0;
            Iron = wallet != null ? wallet.Iron : 0;
            HeroTranscendStone = wallet != null ? wallet.HeroTranscendStone : 0;
            HeroSummonTicket = wallet != null ? wallet.HeroSummonTicket : 0;
            EquipmentSummonTicket = wallet != null ? wallet.EquipmentSummonTicket : 0;
            DungeonTicket = wallet != null ? wallet.DungeonTicket : 0;
        }

        public static HudWalletSnapshot Capture(CurrencyWallet wallet)
        {
            return new HudWalletSnapshot(wallet);
        }
    }
}
