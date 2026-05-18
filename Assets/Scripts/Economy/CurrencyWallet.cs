using System;
using UnityEngine;

public sealed class CurrencyWallet : MonoBehaviour
{
    private SaveManager saveManager;

    public event Action Changed;

    public long Gold { get; private set; }
    public long Ruby { get; private set; }
    public long HeroExpItem { get; private set; }
    public long EquipmentExpItem { get; private set; }
    public long HeroSummonTicket { get; private set; }
    public long EquipmentSummonTicket { get; private set; }

    public void Initialize(SaveManager save)
    {
        saveManager = save;
        Gold = saveManager.LoadLong(SaveKeys.Gold, 0);
        Ruby = saveManager.LoadLong(SaveKeys.Ruby, 675);
        HeroExpItem = saveManager.LoadLong(SaveKeys.HeroExpItem, 120);
        EquipmentExpItem = saveManager.LoadLong(SaveKeys.EquipmentExpItem, 120);
        HeroSummonTicket = saveManager.LoadLong(SaveKeys.HeroSummonTicket, 10);
        EquipmentSummonTicket = saveManager.LoadLong(SaveKeys.EquipmentSummonTicket, 10);
        NotifyChanged();
    }

    public void AddGold(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
        Save();
        NotifyChanged();
    }

    public void AddHeroExpItem(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        HeroExpItem += amount;
        Save();
        NotifyChanged();
    }

    public void AddRuby(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Ruby += amount;
        Save();
        NotifyChanged();
    }

    public void AddHeroSummonTicket(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        HeroSummonTicket += amount;
        Save();
        NotifyChanged();
    }

    public void AddEquipmentExpItem(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        EquipmentExpItem += amount;
        Save();
        NotifyChanged();
    }

    public void AddEquipmentSummonTicket(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        EquipmentSummonTicket += amount;
        Save();
        NotifyChanged();
    }

    public bool SpendGold(long amount)
    {
        if (amount <= 0)
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

    public bool SpendHeroExpItem(long amount)
    {
        if (amount <= 0)
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

    public bool SpendEquipmentExpItem(long amount)
    {
        if (amount <= 0)
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
        saveManager.SaveLong(SaveKeys.Gold, Gold);
        saveManager.SaveLong(SaveKeys.Ruby, Ruby);
        saveManager.SaveLong(SaveKeys.HeroExpItem, HeroExpItem);
        saveManager.SaveLong(SaveKeys.EquipmentExpItem, EquipmentExpItem);
        saveManager.SaveLong(SaveKeys.HeroSummonTicket, HeroSummonTicket);
        saveManager.SaveLong(SaveKeys.EquipmentSummonTicket, EquipmentSummonTicket);
        saveManager.Flush();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
