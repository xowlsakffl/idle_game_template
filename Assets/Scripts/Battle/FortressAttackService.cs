using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static class FortressAttackService
    {
        private const string FortressSourceName = "요새";

        public static CombatAttackAction TickFortressAttack(
            GameNumber fortressHp,
            bool hasAttackableTarget,
            bool isBossFight,
            IReadOnlyList<VisibleEnemyState> visibleEnemies,
            ref float attackCooldown,
            int attackSequence,
            float deltaTime,
            float attackInterval,
            float attackRange,
            GameNumber attackPower,
            float retryCooldown)
        {
            if (!CombatTickService.TryTickFortressAttack(
                    fortressHp,
                    hasAttackableTarget,
                    isBossFight,
                    ref attackCooldown,
                    deltaTime,
                    attackInterval,
                    () => CombatTargetingService.FindNearestAttackableEnemyInRange(
                        visibleEnemies,
                        GetAttackOrigin(attackSequence),
                        attackRange),
                    retryCooldown,
                    out int targetIndex))
            {
                return CombatAttackAction.Invalid;
            }

            return new CombatAttackAction(
                true,
                isBossFight,
                targetIndex,
                attackPower,
                FortressSourceName,
                false,
                null);
        }

        private static Vector2 GetAttackOrigin(int attackSequence)
        {
            return attackSequence % 2 == 0
                ? new Vector2(-0.72f, 0.62f)
                : new Vector2(0.72f, 0.62f);
        }
    }
}
