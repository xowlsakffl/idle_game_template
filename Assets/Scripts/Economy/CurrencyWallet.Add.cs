using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class CurrencyWallet
    {
        public void AddGold(GameNumber amount)
        {
            if (amount <= GameNumber.Zero)
            {
                return;
            }

            Gold = GameData.ClampNumber(Gold + amount);
            Save();
            NotifyChanged();
        }

        public void AddHeroExpItem(GameNumber amount)
        {
            if (amount <= GameNumber.Zero)
            {
                return;
            }

            HeroExpItem = GameData.ClampNumber(HeroExpItem + amount);
            Save();
            NotifyChanged();
        }

        public void AddRuby(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Ruby = GameData.ClampCount(Ruby + amount);
            Save();
            NotifyChanged();
        }

        public void AddHeroSummonTicket(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            HeroSummonTicket = GameData.ClampCount(HeroSummonTicket + amount);
            Save();
            NotifyChanged();
        }

        public void AddEquipmentExpItem(GameNumber amount)
        {
            if (amount <= GameNumber.Zero)
            {
                return;
            }

            EquipmentExpItem = GameData.ClampNumber(EquipmentExpItem + amount);
            Save();
            NotifyChanged();
        }

        public void AddHeroTranscendStone(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            HeroTranscendStone = GameData.ClampCount(HeroTranscendStone + amount);
            Save();
            NotifyChanged();
        }

        public void AddTotemEssence(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            TotemEssence = GameData.ClampCount(TotemEssence + amount);
            Save();
            NotifyChanged();
        }

        public void AddWood(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Wood = GameData.ClampCount(Wood + amount);
            Save();
            NotifyChanged();
        }

        public void AddBrick(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Brick = GameData.ClampCount(Brick + amount);
            Save();
            NotifyChanged();
        }

        public void AddIron(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Iron = GameData.ClampCount(Iron + amount);
            Save();
            NotifyChanged();
        }

        public void AddFacilityMaterials(long wood, long brick, long iron)
        {
            bool changed = false;
            if (wood > 0)
            {
                Wood = GameData.ClampCount(Wood + wood);
                changed = true;
            }

            if (brick > 0)
            {
                Brick = GameData.ClampCount(Brick + brick);
                changed = true;
            }

            if (iron > 0)
            {
                Iron = GameData.ClampCount(Iron + iron);
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            Save();
            NotifyChanged();
        }

        public void AddEquipmentSummonTicket(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EquipmentSummonTicket = GameData.ClampCount(EquipmentSummonTicket + amount);
            Save();
            NotifyChanged();
        }

        public void AddDungeonTicket(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            DungeonTicket = GameData.ClampCount(DungeonTicket + amount);
            Save();
            NotifyChanged();
        }
    }
}
