using System;
using UnityEngine;

public sealed class CurrencyWallet : MonoBehaviour
{
    private SaveManager saveManager;

    public event Action Changed;

    public long Gold { get; private set; }

    public void Initialize(SaveManager save)
    {
        saveManager = save;
        Gold = saveManager.LoadLong(SaveKeys.Gold, 0);
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

    private void Save()
    {
        saveManager.SaveLong(SaveKeys.Gold, Gold);
        saveManager.Flush();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
