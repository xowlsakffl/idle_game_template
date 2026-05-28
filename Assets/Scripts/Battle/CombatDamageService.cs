using System;
using System.Collections.Generic;
using IdleGame.Data;
using IdleGame.Economy;

namespace IdleGame.Battle
{
    internal static class CombatDamageService
    {
        public static GameNumber CalculateHeroDamage(
            HeroState hero,
            AbilityManager abilityManager,
            double heroOwnedAttackMultiplier,
            double attackTalentMultiplier,
            double criticalDamageTalentMultiplier,
            double finalDamageTalentMultiplier,
            double totemAttackMultiplier,
            double runeAttackMultiplier,
            double totemCriticalChanceBonusPercent,
            double runeCriticalChanceBonusPercent,
            double runeFinalDamageMultiplier,
            Func<double> random01,
            out bool isCritical)
        {
            isCritical = false;
            if (hero == null || abilityManager == null)
            {
                return GameNumber.One;
            }

            double damage = (hero.AttackPower + abilityManager.AttackPowerBonus)
                * heroOwnedAttackMultiplier
                * attackTalentMultiplier
                * totemAttackMultiplier
                * runeAttackMultiplier;
            double criticalChance = Math.Min(0.75d, abilityManager.CriticalChance + (totemCriticalChanceBonusPercent + runeCriticalChanceBonusPercent) / 100d);
            isCritical = (random01?.Invoke() ?? 1d) < criticalChance;
            if (isCritical)
            {
                damage *= abilityManager.CriticalDamageMultiplier * criticalDamageTalentMultiplier;
                if ((random01?.Invoke() ?? 1d) < abilityManager.DoubleCriticalChance)
                {
                    damage *= abilityManager.DoubleCriticalBonusMultiplier;
                }
            }

            damage *= abilityManager.FinalDamageMultiplier * finalDamageTalentMultiplier * runeFinalDamageMultiplier;
            return NormalizeDamage(damage);
        }

        public static double GetPartyAttackPower(
            IReadOnlyList<HeroState> deployedHeroes,
            AbilityManager abilityManager,
            double heroOwnedAttackMultiplier,
            double attackTalentMultiplier,
            Func<HeroState, double> totemAttackMultiplier,
            Func<HeroState, double> runeAttackMultiplier)
        {
            if (deployedHeroes == null || deployedHeroes.Count <= 0 || abilityManager == null)
            {
                return 0d;
            }

            double total = 0d;
            foreach (HeroState hero in deployedHeroes)
            {
                total += (hero.AttackPower + abilityManager.AttackPowerBonus)
                    * heroOwnedAttackMultiplier
                    * attackTalentMultiplier
                    * (totemAttackMultiplier?.Invoke(hero) ?? 1d)
                    * (runeAttackMultiplier?.Invoke(hero) ?? 1d);
            }

            return Math.Max(1d, total);
        }

        public static GameNumber CalculateSkillDamage(
            double partyAttackPower,
            CombatSkillState skill,
            AbilityManager abilityManager,
            double finalDamageTalentMultiplier,
            double skillDamageTalentMultiplier,
            double totemSkillDamageMultiplier,
            double runeSkillDamageMultiplier)
        {
            if (skill == null || abilityManager == null)
            {
                return GameNumber.One;
            }

            return NormalizeDamage(partyAttackPower
                * skill.Definition.PartyAttackMultiplier
                * abilityManager.FinalDamageMultiplier
                * finalDamageTalentMultiplier
                * skillDamageTalentMultiplier
                * totemSkillDamageMultiplier
                * runeSkillDamageMultiplier);
        }

        public static GameNumber CalculatePetDamage(
            PetState pet,
            AbilityManager abilityManager,
            double finalDamageTalentMultiplier)
        {
            if (pet == null || abilityManager == null)
            {
                return GameNumber.One;
            }

            return NormalizeDamage(pet.Definition.AttackPower
                * abilityManager.FinalDamageMultiplier
                * finalDamageTalentMultiplier);
        }

        public static double GetHeroOwnedAttackBonusPercent(HeroState hero)
        {
            if (hero == null || !hero.IsOwned)
            {
                return 0d;
            }

            return 0.5d + Math.Max(0, hero.Stars) * 0.08d;
        }

        public static double GetHeroOwnedAttackBonusPercent(IEnumerable<HeroState> heroes)
        {
            if (heroes == null)
            {
                return 0d;
            }

            double total = 0d;
            foreach (HeroState hero in heroes)
            {
                total += GetHeroOwnedAttackBonusPercent(hero);
            }

            return total;
        }

        public static double GetHeroOwnedAttackMultiplier(IEnumerable<HeroState> heroes)
        {
            return 1d + GetHeroOwnedAttackBonusPercent(heroes) / 100d;
        }

        public static GameNumber NormalizeDamage(double damage)
        {
            if (double.IsNaN(damage) || damage <= 1d)
            {
                return GameNumber.One;
            }

            if (double.IsInfinity(damage))
            {
                return GameNumber.FromDouble(double.MaxValue / 1024d);
            }

            return GameData.ClampNumber(GameNumber.Floor(GameNumber.Max(GameNumber.One, damage)));
        }

        public static GameNumber NormalizeDamage(GameNumber damage)
        {
            return GameData.ClampNumber(GameNumber.Floor(GameNumber.Max(GameNumber.One, damage)));
        }
    }
}
