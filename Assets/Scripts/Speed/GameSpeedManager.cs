using System;
using UnityEngine;

public sealed class GameSpeedManager : MonoBehaviour
{
    public const int NormalSpeed = 1;
    public const int FreeSpeed = 2;
    public const int PremiumSpeed = 4;

    private SaveManager saveManager;

    public event Action Changed;

    public int CurrentMultiplier { get; private set; } = NormalSpeed;
    public bool HasFourTimesSpeedEntitlement { get; private set; }

    public void Initialize(SaveManager save)
    {
        saveManager = save;
        CurrentMultiplier = (int)saveManager.LoadLong(SaveKeys.CombatSpeedMultiplier, NormalSpeed);
        HasFourTimesSpeedEntitlement = saveManager.LoadBool(SaveKeys.HasFourTimesSpeedEntitlement, false);

        if (!CanUseSpeed(CurrentMultiplier))
        {
            CurrentMultiplier = NormalSpeed;
        }

        Save();
        NotifyChanged();
    }

    public bool TrySelectSpeed(int multiplier)
    {
        if (!CanUseSpeed(multiplier))
        {
            return false;
        }

        CurrentMultiplier = multiplier;
        Save();
        NotifyChanged();
        return true;
    }

    public bool CanUseSpeed(int multiplier)
    {
        if (multiplier == NormalSpeed || multiplier == FreeSpeed)
        {
            return true;
        }

        return multiplier == PremiumSpeed && HasFourTimesSpeedEntitlement;
    }

    public void DebugSetFourTimesEntitlement(bool enabled)
    {
        HasFourTimesSpeedEntitlement = enabled;
        if (!CanUseSpeed(CurrentMultiplier))
        {
            CurrentMultiplier = NormalSpeed;
        }

        Save();
        NotifyChanged();
    }

    private void Save()
    {
        saveManager.SaveLong(SaveKeys.CombatSpeedMultiplier, CurrentMultiplier);
        saveManager.SaveBool(SaveKeys.HasFourTimesSpeedEntitlement, HasFourTimesSpeedEntitlement);
        saveManager.Flush();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
