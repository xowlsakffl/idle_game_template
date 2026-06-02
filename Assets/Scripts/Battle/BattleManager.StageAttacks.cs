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
        private void DealDamage(HeroState hero)
        {
            ApplyAttack(CombatAttackService.BuildHeroAttack(
                hero,
                IsBossFight,
                hero != null ? SelectVisibleEnemyIndexForHero(hero) : -1,
                abilityManager,
                GetHeroOwnedAttackMultiplier(),
                GetTalentMultiplier(TalentEffectKind.AttackPercent),
                GetTalentMultiplier(TalentEffectKind.CriticalDamagePercent),
                GetTalentMultiplier(TalentEffectKind.FinalDamagePercent),
                GetTotemAttackMultiplier(hero),
                GetRuneAttackMultiplier(hero),
                GetTotemCriticalChanceBonus(),
                GetRuneCriticalChanceBonus(),
                GetRuneFinalDamageMultiplier(),
                random.NextDouble));
        }

        private void CastSkill(CombatSkillState skill)
        {
            ApplyAttack(CombatAttackService.BuildSkillAttack(
                skill,
                IsBossFight,
                skill != null ? SelectVisibleEnemyIndexForSkill(skill) : -1,
                GetPartyAttackPower(),
                abilityManager,
                GetTalentMultiplier(TalentEffectKind.FinalDamagePercent),
                GetTalentMultiplier(TalentEffectKind.SkillDamagePercent),
                GetTotemSkillDamageMultiplier(),
                GetRuneSkillDamageMultiplier()));
        }

        private void AttackWithPet(PetState pet)
        {
            ApplyAttack(CombatAttackService.BuildPetAttack(
                pet,
                IsBossFight,
                pet != null ? SelectVisibleEnemyIndexForPet(pet) : -1,
                abilityManager,
                GetTalentMultiplier(TalentEffectKind.FinalDamagePercent)));
        }

        private void ApplyAttack(CombatAttackAction attack)
        {
            if (!attack.IsValid)
            {
                return;
            }

            if (attack.IsBossTarget)
            {
                ApplyDamage(attack.Damage, attack.SourceName, attack.IsCritical, attack.HeroId);
                return;
            }

            ApplyDamageToVisibleEnemy(
                attack.VisibleEnemyIndex,
                attack.Damage,
                attack.SourceName,
                attack.IsCritical,
                attack.HeroId);
        }

        private int SelectVisibleEnemyIndexForHero(HeroState hero)
        {
            return CombatTargetingService.SelectVisibleEnemyIndexForHero(
                hero,
                visibleEnemies,
                heroTargetSpawnSequences,
                heroRuntimeStates,
                CombatMovementService.GetHeroAttackRange(hero));
        }

        private int SelectVisibleEnemyIndexForSkill(CombatSkillState skill)
        {
            int skillIndex = skills.IndexOf(skill);
            return SelectVisibleEnemyIndexForLockedSource(
                skill.Definition.Id,
                skillTargetSpawnSequences,
                Mathf.Max(0, skillIndex));
        }

        private int SelectVisibleEnemyIndexForPet(PetState pet)
        {
            int petIndex = pets.IndexOf(pet);
            return SelectVisibleEnemyIndexForLockedSource(
                pet.Definition.Id,
                petTargetSpawnSequences,
                Mathf.Max(0, petIndex));
        }

        private int SelectVisibleEnemyIndexForLockedSource(
            string sourceId,
            Dictionary<string, int> targetLocks,
            int preferredOffset)
        {
            return CombatTargetingService.SelectVisibleEnemyIndexForLockedSource(
                sourceId,
                visibleEnemies,
                targetLocks,
                preferredOffset);
        }

        private int FindFirstAttackableVisibleEnemyIndex()
        {
            return CombatTargetingService.FindFirstAttackableVisibleEnemyIndex(visibleEnemies);
        }
    }
}
