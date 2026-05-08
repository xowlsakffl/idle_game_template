using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AbilityManager : MonoBehaviour
{
    private readonly Dictionary<AbilityKind, AbilityState> statesByKind = new Dictionary<AbilityKind, AbilityState>();
    private readonly List<AbilityState> states = new List<AbilityState>();
    private CurrencyWallet wallet;
    private SaveManager saveManager;

    public event Action Changed;

    public IReadOnlyList<AbilityState> States => states;

    public int AttackPowerBonus => GetRawValue(AbilityKind.AttackPower);
    public float CriticalChance => Mathf.Clamp01(GetRawValue(AbilityKind.CriticalChance) / 1000f);
    public float CriticalDamageMultiplier => Mathf.Max(1f, GetRawValue(AbilityKind.CriticalDamage) / 100f);

    public void Initialize(CurrencyWallet currency, SaveManager save)
    {
        wallet = currency;
        saveManager = save;
        states.Clear();
        statesByKind.Clear();

        foreach (AbilityDefinition definition in GameData.Abilities)
        {
            int level = PlayerPrefs.GetInt(SaveKeys.AbilityLevel(definition.Kind), 0);
            var state = new AbilityState(definition, level);
            states.Add(state);
            statesByKind[definition.Kind] = state;
        }

        NotifyChanged();
    }

    public bool TryLevelUp(AbilityKind kind)
    {
        if (!statesByKind.TryGetValue(kind, out AbilityState state) || state.IsMaxed)
        {
            return false;
        }

        long cost = state.LevelUpCost;
        if (!wallet.SpendGold(cost))
        {
            return false;
        }

        state.Level += 1;
        PlayerPrefs.SetInt(SaveKeys.AbilityLevel(kind), state.Level);
        saveManager.Flush();
        NotifyChanged();
        return true;
    }

    public string GetDisplayValue(AbilityState state)
    {
        switch (state.Definition.Kind)
        {
            case AbilityKind.AttackPower:
                return "+" + state.RawValue;
            case AbilityKind.CriticalChance:
                return (state.RawValue / 10f).ToString("0.0") + "%";
            case AbilityKind.CriticalDamage:
                return state.RawValue + "%";
            default:
                return state.RawValue.ToString();
        }
    }

    private int GetRawValue(AbilityKind kind)
    {
        return statesByKind.TryGetValue(kind, out AbilityState state) ? state.RawValue : 0;
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
