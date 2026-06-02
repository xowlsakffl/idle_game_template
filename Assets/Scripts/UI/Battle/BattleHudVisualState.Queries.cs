using System.Collections.Generic;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Battle
{
    public sealed partial class BattleHudVisualState
    {
        public Vector2 GetNearestHeroPosition(Vector2 fromPosition)
        {
            if (activeHeroPositions.Count <= 0)
            {
                return Vector2.zero;
            }

            Vector2 nearest = activeHeroPositions[0];
            float nearestDistance = (fromPosition - nearest).sqrMagnitude;
            for (int i = 1; i < activeHeroPositions.Count; i++)
            {
                float distance = (fromPosition - activeHeroPositions[i]).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = activeHeroPositions[i];
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public Vector2 GetNearestEnemyPosition(Vector2 fromPosition)
        {
            if (activeEnemyPositions.Count <= 0)
            {
                return fromPosition;
            }

            Vector2 nearest = activeEnemyPositions[0];
            float nearestDistance = (fromPosition - nearest).sqrMagnitude;
            for (int i = 1; i < activeEnemyPositions.Count; i++)
            {
                float distance = (fromPosition - activeEnemyPositions[i]).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = activeEnemyPositions[i];
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public Vector2 GetHeroLockedEnemyPosition(int targetIndex, Vector2 fallbackFromPosition)
        {
            if (targetIndex >= 0
                && targetIndex < activeEnemyPositionsByIndex.Count
                && targetIndex < activeEnemyPositionStates.Count
                && activeEnemyPositionStates[targetIndex])
            {
                return activeEnemyPositionsByIndex[targetIndex];
            }

            return GetNearestEnemyPosition(fallbackFromPosition);
        }

        public void EnsureEnemyCapacity(int index)
        {
            while (displayedEnemyPositions.Count <= index)
            {
                displayedEnemyPositions.Add(Vector2.zero);
            }

            while (displayedEnemyActiveStates.Count <= index)
            {
                displayedEnemyActiveStates.Add(false);
            }

            while (displayedEnemySpawnSequences.Count <= index)
            {
                displayedEnemySpawnSequences.Add(-1);
            }

            while (displayedEnemyDeathDelays.Count <= index)
            {
                displayedEnemyDeathDelays.Add(0f);
            }

            while (displayedEnemyDeathPositions.Count <= index)
            {
                displayedEnemyDeathPositions.Add(Vector2.zero);
            }

            while (activeEnemyPositionsByIndex.Count <= index)
            {
                activeEnemyPositionsByIndex.Add(Vector2.zero);
            }

            while (activeEnemyPositionStates.Count <= index)
            {
                activeEnemyPositionStates.Add(false);
            }
        }

        private void ResetActiveEnemyPositions()
        {
            for (int i = 0; i < activeEnemyPositionStates.Count; i++)
            {
                activeEnemyPositionStates[i] = false;
            }
        }
    }
}
