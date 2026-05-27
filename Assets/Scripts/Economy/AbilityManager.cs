using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Save;

namespace IdleGame.Economy
{
    public sealed class AbilityManager : MonoBehaviour
    {
        private readonly Dictionary<AbilityKind, AbilityState> statesByKind = new Dictionary<AbilityKind, AbilityState>();
        private readonly List<AbilityState> states = new List<AbilityState>();
        private CurrencyWallet wallet;
        private SaveManager saveManager;

        public event Action Changed;

        public IReadOnlyList<AbilityState> States => states;

        public double AttackPowerBonus => GetValue(AbilityKind.AttackPower);
        public double MaxHpBonus => GetValue(AbilityKind.MaxHp);
        public float CriticalChance => Mathf.Clamp((float)(GetValue(AbilityKind.CriticalChance) / 100d), 0f, 0.5f);
        public double CriticalDamageMultiplier => Math.Max(1d, 1d + GetValue(AbilityKind.CriticalDamage) / 100d);
        public float DoubleCriticalChance => Mathf.Clamp((float)(GetValue(AbilityKind.DoubleCriticalChance) / 100d), 0f, 0.5f);
        public double DoubleCriticalBonusMultiplier => Math.Max(1d, 1d + GetValue(AbilityKind.DoubleCriticalBonusDamage) / 100d);
        public double FinalDamageMultiplier => Math.Max(1d, 1d + GetValue(AbilityKind.FinalDamage) / 100d);

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
            return TryLevelUp(kind, 1);
        }

        public bool TryLevelUp(AbilityKind kind, int requestedLevels)
        {
            if (!statesByKind.TryGetValue(kind, out AbilityState state) || state.IsMaxed)
            {
                return false;
            }

            int levels = GetCappedLevelCount(state, requestedLevels);
            if (levels <= 0)
            {
                return false;
            }

            long cost = GetLevelUpCost(state, levels);
            if (cost <= 0 || wallet.Gold < cost)
            {
                return false;
            }

            if (!wallet.SpendGold(cost))
            {
                return false;
            }

            state.Level += levels;
            PlayerPrefs.SetInt(SaveKeys.AbilityLevel(kind), state.Level);
            saveManager.Flush();
            NotifyChanged();
            return true;
        }

        public string GetDisplayValue(AbilityState state)
        {
            string prefix = state.Definition.DisplayKind == AbilityDisplayKind.Flat ? "+" : string.Empty;
            return prefix + state.Definition.FormatValue(state.Level);
        }

        public long GetLevelUpCost(AbilityState state, int requestedLevels)
        {
            if (state == null || requestedLevels <= 0 || state.IsMaxed)
            {
                return 0;
            }

            int cappedLevels = GetCappedLevelCount(state, requestedLevels);
            long total = 0;
            for (int i = 0; i < cappedLevels; i++)
            {
                long cost = state.Definition.GetLevelUpCost(state.Level + i);
                if (cost <= 0)
                {
                    continue;
                }

                if (long.MaxValue - total < cost)
                {
                    return long.MaxValue;
                }

                total += cost;
            }

            return total;
        }

        public int GetCappedLevelCount(AbilityState state, int requestedLevels)
        {
            if (state == null || requestedLevels <= 0 || state.IsMaxed)
            {
                return 0;
            }

            if (state.Definition.MaxLevel <= 0)
            {
                return requestedLevels;
            }

            return Mathf.Clamp(requestedLevels, 0, state.Definition.MaxLevel - state.Level);
        }

        public int GetPurchasableLevelCount(AbilityState state, int requestedLevels, long availableGold)
        {
            int cappedLevels = GetCappedLevelCount(state, requestedLevels);
            long total = 0;
            int purchasable = 0;
            for (int i = 0; i < cappedLevels; i++)
            {
                long cost = state.Definition.GetLevelUpCost(state.Level + i);
                if (cost <= 0 || total > availableGold - cost)
                {
                    break;
                }

                total += cost;
                purchasable += 1;
            }

            return purchasable;
        }

        public double GetTotalCombatPower(IReadOnlyList<HeroState> heroes)
        {
            int heroCount = heroes == null ? 0 : heroes.Count;
            double attackScore = heroCount <= 0 ? AttackPowerBonus : 0d;
            double hpScore = heroCount <= 0 ? MaxHpBonus : 0d;
            double mobilityScore = 0d;
            if (heroes != null)
            {
                foreach (HeroState hero in heroes)
                {
                    attackScore += (hero.AttackPower + AttackPowerBonus) * hero.AttackSpeed;
                    hpScore += hero.MaxHp + MaxHpBonus;
                    mobilityScore += hero.MoveSpeed * 10d;
                }
            }

            double criticalExpected = 1d + CriticalChance * (CriticalDamageMultiplier - 1d);
            double doubleCriticalExpected = 1d + DoubleCriticalChance * (DoubleCriticalBonusMultiplier - 1d);
            double power = attackScore * criticalExpected * doubleCriticalExpected * FinalDamageMultiplier
                + hpScore * 0.03d
                + mobilityScore;
            return GameData.ClampCombatPower(power);
        }

        private double GetValue(AbilityKind kind)
        {
            return statesByKind.TryGetValue(kind, out AbilityState state) ? state.Value : 0d;
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
