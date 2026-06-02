using System;
using System.Collections.Generic;
using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal static partial class CombatMovementService
    {
        public static Vector2 GetHeroBattleSlotPosition(HeroState hero, int heroIndex)
        {
            if (IsFortressProtectedHero(hero))
            {
                switch (heroIndex % GameData.MaxPartyHeroes)
                {
                    case 0:
                        return new Vector2(-0.36f, -0.08f);
                    case 1:
                        return new Vector2(0.36f, -0.08f);
                    case 2:
                        return new Vector2(-0.34f, 0.42f);
                    case 3:
                        return new Vector2(0.34f, 0.42f);
                    case 4:
                        return new Vector2(0f, -0.52f);
                    case 5:
                        return new Vector2(-0.58f, 0.18f);
                    case 6:
                        return new Vector2(0.58f, 0.18f);
                    default:
                        return new Vector2(0f, 0.70f);
                }
            }

            switch (heroIndex % GameData.MaxPartyHeroes)
            {
                case 0:
                    return new Vector2(-0.92f, -0.58f);
                case 1:
                    return new Vector2(0.92f, -0.58f);
                case 2:
                    return new Vector2(-1.42f, 0.12f);
                case 3:
                    return new Vector2(1.42f, 0.12f);
                case 4:
                    return new Vector2(0f, -1.18f);
                case 5:
                    return new Vector2(-1.82f, -0.44f);
                case 6:
                    return new Vector2(1.82f, -0.44f);
                default:
                    return new Vector2(0f, 1.10f);
            }
        }

        public static bool IsFortressProtectedHero(HeroState hero)
        {
            return hero != null
                && (hero.Definition.Trait == HeroTrait.Ranged || hero.Definition.Trait == HeroTrait.Support);
        }

        public static float GetHeroAttackRange(HeroState hero)
        {
            switch (hero.Definition.Trait)
            {
                case HeroTrait.Melee:
                    return 0.82f;
                case HeroTrait.Ranged:
                    return 4.35f;
                case HeroTrait.Support:
                    return 3.65f;
                case HeroTrait.Defense:
                    return 0.92f;
                default:
                    return 1.25f;
            }
        }

        public static float GetEnemyMoveSpeed(VisibleEnemyState enemy)
        {
            return 1.15f + (Mathf.Abs(enemy.SpawnSequence) % 4) * 0.09f;
        }
    }
}
