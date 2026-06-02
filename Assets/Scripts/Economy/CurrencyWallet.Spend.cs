using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed partial class CurrencyWallet
    {
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
    }
}
