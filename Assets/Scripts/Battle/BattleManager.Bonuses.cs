using System.Collections.Generic;
using System;
using UnityEngine;
using IdleGame.Data;
using IdleGame.Economy;
using IdleGame.Progression;
using IdleGame.Save;
using IdleGame.Speed;

namespace IdleGame.Battle
{
    public sealed partial class BattleManager
    {
        private double GetPartyAttackPower()
        {
            return CombatDamageService.GetPartyAttackPower(
                deployedHeroes,
                abilityManager,
                GetHeroOwnedAttackMultiplier(),
                GetTalentMultiplier(TalentEffectKind.AttackPercent),
                GetTotemAttackMultiplier,
                GetRuneAttackMultiplier);
        }

        public double GetHeroOwnedAttackBonusPercent(HeroState hero)
        {
            return CombatDamageService.GetHeroOwnedAttackBonusPercent(hero);
        }

        private double GetHeroOwnedAttackBonusPercent()
        {
            return CombatDamageService.GetHeroOwnedAttackBonusPercent(heroes);
        }

        private double GetHeroOwnedAttackMultiplier()
        {
            return CombatDamageService.GetHeroOwnedAttackMultiplier(heroes);
        }

        private double GetBossGoldMultiplier()
        {
            return GetTalentMultiplier(TalentEffectKind.GoldGainPercent)
                * GetTotemGoldMultiplier()
                * GetRuneGoldMultiplier();
        }

        private double GetEnemyGoldMultiplier()
        {
            return CombatRewardService.GetPetGoldBonusMultiplier(pets)
                * GetTalentMultiplier(TalentEffectKind.GoldGainPercent)
                * GetTotemGoldMultiplier()
                * GetRuneGoldMultiplier();
        }

        private double GetEnemyHeroExpMultiplier()
        {
            return GetTalentMultiplier(TalentEffectKind.HeroExpGainPercent)
                * GetTotemHeroExpMultiplier()
                * GetRuneHeroExpMultiplier();
        }

        private double GetAccountExperienceMultiplier()
        {
            return GetTalentMultiplier(TalentEffectKind.AccountExpGainPercent)
                * GetTotemAccountExpMultiplier()
                * GetRuneAccountExpMultiplier();
        }

        private string BuildSupportStatusText()
        {
            return "Skill Auto " + (skillAutoEnabled ? "ON" : "OFF")
                + "    Fever Auto " + (feverAutoEnabled ? "ON" : "OFF")
                + "    Field " + VisibleEnemyCount;
        }

        private float GetHeroMoveSpeed(HeroState hero)
        {
            return (0.95f + Mathf.Max(0.1f, hero.MoveSpeed) * 0.34f)
                * (float)GetTalentMultiplier(TalentEffectKind.MoveSpeedPercent)
                * (float)GetTotemMoveSpeedMultiplier()
                * (float)GetRuneMoveSpeedMultiplier();
        }

        private float CalculateHeroBattleMaxHp(HeroState hero)
        {
            double hp = hero != null ? hero.MaxHp : 1d;
            if (abilityManager != null)
            {
                hp += abilityManager.MaxHpBonus;
            }

            hp *= GetTalentMultiplier(TalentEffectKind.HpPercent) * GetTotemHpMultiplier(hero) * GetRuneHpMultiplier();
            return Mathf.Max(1f, (float)Math.Min(float.MaxValue, hp));
        }

        private double GetTalentMultiplier(TalentEffectKind kind)
        {
            return accountProgressManager != null ? accountProgressManager.GetMultiplier(kind) : 1d;
        }

        private double GetDamageTakenMultiplier()
        {
            return (accountProgressManager != null ? accountProgressManager.DamageTakenMultiplier : 1d)
                * GetTotemDamageTakenMultiplier()
                * GetRuneDamageTakenMultiplier();
        }

        private double GetTotemAttackMultiplier(HeroState hero)
        {
            return FormationBonusService.GetTotemAttackMultiplier(totems, deployedHeroes, IsBossFight, hero);
        }

        private double GetTotemHpMultiplier(HeroState hero)
        {
            return FormationBonusService.GetTotemHpMultiplier(totems, deployedHeroes, hero);
        }

        private double GetTotemGoldMultiplier()
        {
            return FormationBonusService.GetTotemGoldMultiplier(totems);
        }

        private double GetTotemHeroExpMultiplier()
        {
            return FormationBonusService.GetTotemHeroExpMultiplier(totems);
        }

        private double GetTotemAccountExpMultiplier()
        {
            return FormationBonusService.GetTotemAccountExpMultiplier(totems);
        }

        private double GetTotemAttackSpeedMultiplier(HeroState hero)
        {
            return FormationBonusService.GetTotemAttackSpeedMultiplier(totems, hero);
        }

        private double GetTotemMoveSpeedMultiplier()
        {
            return FormationBonusService.GetTotemMoveSpeedMultiplier(totems);
        }

        private double GetTotemSkillDamageMultiplier()
        {
            return FormationBonusService.GetTotemSkillDamageMultiplier(totems);
        }

        private double GetTotemSkillCooldownMultiplier()
        {
            return FormationBonusService.GetTotemSkillCooldownMultiplier(totems);
        }

        private double GetTotemCriticalChanceBonus()
        {
            return FormationBonusService.GetTotemCriticalChanceBonus(totems);
        }

        private double GetTotemDamageTakenMultiplier()
        {
            return FormationBonusService.GetTotemDamageTakenMultiplier(totems);
        }

        private IReadOnlyList<RuneState> GetActiveUsableRunes()
        {
            FormationLoadoutService.FillActiveUsableRunes(
                saveManager,
                runesById,
                GetCurrentAccountLevel(),
                activeHeroPreset,
                activeUsableRunes);
            return activeUsableRunes;
        }

        private double GetRuneAttackMultiplier(HeroState hero)
        {
            return FormationBonusService.GetRuneAttackMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneFinalDamageMultiplier()
        {
            return FormationBonusService.GetRuneFinalDamageMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneHpMultiplier()
        {
            return FormationBonusService.GetRuneHpMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneGoldMultiplier()
        {
            return FormationBonusService.GetRuneGoldMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneHeroExpMultiplier()
        {
            return FormationBonusService.GetRuneHeroExpMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneAccountExpMultiplier()
        {
            return FormationBonusService.GetRuneAccountExpMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneAttackSpeedMultiplier(HeroState hero)
        {
            return FormationBonusService.GetRuneAttackSpeedMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneMoveSpeedMultiplier()
        {
            return FormationBonusService.GetRuneMoveSpeedMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneSkillDamageMultiplier()
        {
            return FormationBonusService.GetRuneSkillDamageMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneSkillCooldownMultiplier()
        {
            return FormationBonusService.GetRuneSkillCooldownMultiplier(GetActiveUsableRunes());
        }

        private double GetRuneCriticalChanceBonus()
        {
            return FormationBonusService.GetRuneCriticalChanceBonus(GetActiveUsableRunes());
        }

        private double GetRuneDamageTakenMultiplier()
        {
            return FormationBonusService.GetRuneDamageTakenMultiplier(GetActiveUsableRunes());
        }
    }
}
