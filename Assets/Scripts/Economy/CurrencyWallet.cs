using System;
using UnityEngine;

public sealed class CurrencyWallet : MonoBehaviour
{
    private SaveManager saveManager;

    public event Action Changed;

    public GameNumber Gold { get; private set; }
    public long Ruby { get; private set; }
    public GameNumber HeroExpItem { get; private set; }
    public GameNumber EquipmentExpItem { get; private set; }
    public long HeroTranscendStone { get; private set; }
    public long HeroSummonTicket { get; private set; }
    public long EquipmentSummonTicket { get; private set; }

    public void Initialize(SaveManager save)
    {
        saveManager = save;
        Gold = saveManager.LoadGameNumber(SaveKeys.Gold, GameNumber.Zero);
        Ruby = saveManager.LoadLong(SaveKeys.Ruby, 150);
        HeroExpItem = saveManager.LoadGameNumber(SaveKeys.HeroExpItem, 80);
        EquipmentExpItem = saveManager.LoadGameNumber(SaveKeys.EquipmentExpItem, 80);
        HeroTranscendStone = saveManager.LoadLong(SaveKeys.HeroTranscendStone, 0);
        HeroSummonTicket = saveManager.LoadLong(SaveKeys.HeroSummonTicket, 3);
        EquipmentSummonTicket = saveManager.LoadLong(SaveKeys.EquipmentSummonTicket, 3);
        NotifyChanged();
    }

    public void AddGold(GameNumber amount)
    {
        if (amount <= GameNumber.Zero)
        {
            return;
        }

        Gold += amount;
        Save();
        NotifyChanged();
    }

    public void AddHeroExpItem(GameNumber amount)
    {
        if (amount <= GameNumber.Zero)
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

    public void AddEquipmentExpItem(GameNumber amount)
    {
        if (amount <= GameNumber.Zero)
        {
            return;
        }

        EquipmentExpItem += amount;
        Save();
        NotifyChanged();
    }

    public void AddHeroTranscendStone(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        HeroTranscendStone += amount;
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
