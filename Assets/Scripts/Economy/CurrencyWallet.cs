using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed class CurrencyWallet : MonoBehaviour
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

        public bool SpendGold(GameNumber amount)
        {
            if (amount <= GameNumber.Zero)
            {
                return true;
            }

            if (Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            Save();
            NotifyChanged();
            return true;
        }

        public bool SpendHeroExpItem(GameNumber amount)
        {
            if (amount <= GameNumber.Zero)
            {
                return true;
            }

            if (HeroExpItem < amount)
            {
                return false;
            }

            HeroExpItem -= amount;
            Save();
            NotifyChanged();
            return true;
        }

        public bool SpendHeroSummonCost(int count, int rubyPerMissingTicket)
        {
            if (!CanSpendSummonCost(count, rubyPerMissingTicket, HeroSummonTicket, out long ticketUse, out long rubyCost))
            {
                return false;
            }

            HeroSummonTicket -= ticketUse;
            Ruby -= rubyCost;
            Save();
            NotifyChanged();
            return true;
        }

        public bool SpendEquipmentExpItem(GameNumber amount)
        {
            if (amount <= GameNumber.Zero)
            {
                return true;
            }

            if (EquipmentExpItem < amount)
            {
                return false;
            }

            EquipmentExpItem -= amount;
            Save();
            NotifyChanged();
            return true;
        }

        public bool SpendHeroTranscendStone(long amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (HeroTranscendStone < amount)
            {
                return false;
            }

            HeroTranscendStone -= amount;
            Save();
            NotifyChanged();
            return true;
        }

        public bool SpendTotemEssence(long amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (TotemEssence < amount)
            {
                return false;
            }

            TotemEssence -= amount;
            Save();
            NotifyChanged();
            return true;
        }

        public bool SpendFacilityMaterials(FacilityUpgradeCost cost)
        {
            if (cost.IsFree)
            {
                return true;
            }

            if (Wood < cost.Wood || Brick < cost.Brick || Iron < cost.Iron)
            {
                return false;
            }

            Wood -= cost.Wood;
            Brick -= cost.Brick;
            Iron -= cost.Iron;
            Save();
            NotifyChanged();
            return true;
        }

        public bool SpendEquipmentSummonCost(int count, int rubyPerMissingTicket)
        {
            if (!CanSpendSummonCost(count, rubyPerMissingTicket, EquipmentSummonTicket, out long ticketUse, out long rubyCost))
            {
                return false;
            }

            EquipmentSummonTicket -= ticketUse;
            Ruby -= rubyCost;
            Save();
            NotifyChanged();
            return true;
        }

        private bool CanSpendSummonCost(int count, int rubyPerMissingTicket, long ticketBalance, out long ticketUse, out long rubyCost)
        {
            ticketUse = 0;
            rubyCost = 0;
            if (count <= 0)
            {
                return true;
            }

            ticketUse = Math.Min(ticketBalance, count);
            long missingTickets = count - ticketUse;
            rubyCost = missingTickets * Math.Max(0, rubyPerMissingTicket);

            if (Ruby < rubyCost)
            {
                return false;
            }

            return true;
        }

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
            saveManager.Flush();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }

    }
}
