using System;
using UnityEngine;

public sealed class CurrencyWallet : MonoBehaviour
{
    private SaveManager saveManager;

    public event Action Changed;

    public long Gold { get; private set; }
    public long Ruby { get; private set; }
    public long HeroExpItem { get; private set; }
    public long HeroSummonTicket { get; private set; }

    public void Initialize(SaveManager save)
    {
        saveManager = save;
        Gold = saveManager.LoadLong(SaveKeys.Gold, 0);
        Ruby = saveManager.LoadLong(SaveKeys.Ruby, 675);
        HeroExpItem = saveManager.LoadLong(SaveKeys.HeroExpItem, 120);
        HeroSummonTicket = saveManager.LoadLong(SaveKeys.HeroSummonTicket, 10);
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
        if (count <= 0)
        {
            return true;
        }

        long ticketUse = Math.Min(HeroSummonTicket, count);
        long missingTickets = count - ticketUse;
        long rubyCost = missingTickets * Math.Max(0, rubyPerMissingTicket);

        if (Ruby < rubyCost)
        {
            return false;
        }

        HeroSummonTicket -= ticketUse;
        Ruby -= rubyCost;
        Save();
        NotifyChanged();
        return true;
    }

    private void Save()
    {
        saveManager.SaveLong(SaveKeys.Gold, Gold);
        saveManager.SaveLong(SaveKeys.Ruby, Ruby);
        saveManager.SaveLong(SaveKeys.HeroExpItem, HeroExpItem);
        saveManager.SaveLong(SaveKeys.HeroSummonTicket, HeroSummonTicket);
        saveManager.Flush();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
