using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using IdleGame.Data;

namespace IdleGame.UI.Battle
{
    public sealed partial class BattleHudVisualState
    {
        private readonly float enemyDeathVisualSeconds;
        private readonly List<Vector2> activeHeroPositions = new List<Vector2>();
        private readonly List<Vector2> activeEnemyPositions = new List<Vector2>();
        private readonly List<Vector2> activeEnemyPositionsByIndex = new List<Vector2>();
        private readonly List<bool> activeEnemyPositionStates = new List<bool>();
        private readonly Dictionary<string, Vector2> heroBasePositions = new Dictionary<string, Vector2>();
        private readonly Dictionary<string, Vector2> displayedHeroPositions = new Dictionary<string, Vector2>();
        private readonly List<Vector2> displayedEnemyPositions = new List<Vector2>();
        private readonly List<bool> displayedEnemyActiveStates = new List<bool>();
        private readonly List<int> displayedEnemySpawnSequences = new List<int>();
        private readonly List<float> displayedEnemyDeathDelays = new List<float>();
        private readonly List<Vector2> displayedEnemyDeathPositions = new List<Vector2>();

        public BattleHudVisualState(float enemyDeathVisualSeconds)
        {
            this.enemyDeathVisualSeconds = Mathf.Max(0.01f, enemyDeathVisualSeconds);
        }

        public void ResetAll()
        {
            activeHeroPositions.Clear();
            activeEnemyPositions.Clear();
            activeEnemyPositionsByIndex.Clear();
            activeEnemyPositionStates.Clear();
            heroBasePositions.Clear();
            displayedHeroPositions.Clear();
            displayedEnemyPositions.Clear();
            displayedEnemyActiveStates.Clear();
            displayedEnemySpawnSequences.Clear();
            displayedEnemyDeathDelays.Clear();
            displayedEnemyDeathPositions.Clear();
        }

        public void PrepareFrame()
        {
            activeHeroPositions.Clear();
            activeEnemyPositions.Clear();
            ResetActiveEnemyPositions();
            heroBasePositions.Clear();
        }

        public void ClearActiveHeroPositions()
        {
            activeHeroPositions.Clear();
        }

        public void AddActiveHeroPosition(Vector2 position)
        {
            activeHeroPositions.Add(position);
        }

        public void AddActiveEnemyPosition(Vector2 position)
        {
            activeEnemyPositions.Add(position);
        }

        public void SetHeroBasePosition(string heroId, Vector2 position)
        {
            if (!string.IsNullOrEmpty(heroId))
            {
                heroBasePositions[heroId] = position;
            }
        }

        public bool TryGetHeroBasePosition(string heroId, out Vector2 position)
        {
            return heroBasePositions.TryGetValue(heroId, out position);
        }

        public Vector2 GetDisplayedHeroPositionOrDefault(string heroId, Vector2 fallback)
        {
            return displayedHeroPositions.TryGetValue(heroId, out Vector2 position) ? position : fallback;
        }

        public Vector2 SmoothHeroPosition(HeroState hero, Vector2 targetPosition, float visualDeltaTime, bool isAttackSource, float flashRatio)
        {
            string heroId = hero.Definition.Id;
            if (!displayedHeroPositions.TryGetValue(heroId, out Vector2 currentPosition))
            {
                displayedHeroPositions[heroId] = targetPosition;
                return targetPosition;
            }

            float move = Mathf.Max(0.1f, hero.MoveSpeed);
            float pixelsPerSecond = 82f + move * 54f;
            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, pixelsPerSecond * visualDeltaTime);
            displayedHeroPositions[heroId] = nextPosition;
            return nextPosition;
        }

        public Vector2 SmoothEnemyPosition(int index, Vector2 targetPosition, Vector2 spawnPosition, float visualDeltaTime, bool isBoss)
        {
            EnsureEnemyCapacity(index);

            Vector2 currentPosition = displayedEnemyActiveStates[index]
                ? displayedEnemyPositions[index]
                : spawnPosition;

            displayedEnemyActiveStates[index] = true;
            float pixelsPerSecond = isBoss ? 135f : 150f + index % 4 * 22f;
            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, pixelsPerSecond * visualDeltaTime);
            displayedEnemyPositions[index] = nextPosition;
            return nextPosition;
        }

        public Vector2 GetDisplayedEnemyPositionOrSpawn(int index, Vector2 spawnPosition)
        {
            EnsureEnemyCapacity(index);
            return displayedEnemyActiveStates[index] ? displayedEnemyPositions[index] : spawnPosition;
        }

        public void ResetEnemyIfSpawnChanged(int index, int spawnSequence)
        {
            EnsureEnemyCapacity(index);
            if (displayedEnemySpawnSequences[index] == spawnSequence)
            {
                return;
            }

            if (displayedEnemySpawnSequences[index] >= 0 && displayedEnemyActiveStates[index])
            {
                displayedEnemyDeathDelays[index] = enemyDeathVisualSeconds;
                displayedEnemyDeathPositions[index] = displayedEnemyPositions[index];
            }

            displayedEnemySpawnSequences[index] = spawnSequence;
            displayedEnemyActiveStates[index] = false;
        }

        public bool TryRenderEnemyDeath(
            int index,
            RectTransform enemyRect,
            Image image,
            Text text,
            float visualDeltaTime,
            out Vector2 deathPosition)
        {
            EnsureEnemyCapacity(index);
            deathPosition = displayedEnemyDeathPositions[index];
            if (displayedEnemyDeathDelays[index] <= 0f)
            {
                return false;
            }

            displayedEnemyDeathDelays[index] = Mathf.Max(0f, displayedEnemyDeathDelays[index] - visualDeltaTime);
            float progress = 1f - displayedEnemyDeathDelays[index] / enemyDeathVisualSeconds;
            float alpha = 1f - progress;
            deathPosition = displayedEnemyDeathPositions[index] + Vector2.up * (24f * progress);
            enemyRect.localScale = Vector3.one * Mathf.Lerp(0.9f, 0.18f, progress);
            image.color = new Color(1f, 0.18f, 0.12f, Mathf.Clamp01(alpha));
            text.text = "KO";
            text.color = new Color(1f, 0.95f, 0.7f, Mathf.Clamp01(alpha));
            return true;
        }

        public void SetEnemyInactive(int index)
        {
            EnsureEnemyCapacity(index);
            displayedEnemyActiveStates[index] = false;
            displayedEnemySpawnSequences[index] = -1;
            displayedEnemyDeathDelays[index] = 0f;
        }

        public void ResetDisplayedEnemies()
        {
            for (int i = 0; i < displayedEnemyActiveStates.Count; i++)
            {
                displayedEnemyActiveStates[i] = false;
            }

            for (int i = 0; i < displayedEnemySpawnSequences.Count; i++)
            {
                displayedEnemySpawnSequences[i] = -1;
            }

            for (int i = 0; i < displayedEnemyDeathDelays.Count; i++)
            {
                displayedEnemyDeathDelays[i] = 0f;
            }

            ResetActiveEnemyPositions();
        }

        public void SetActiveEnemyPosition(int index, Vector2 position)
        {
            EnsureEnemyCapacity(index);
            activeEnemyPositionsByIndex[index] = position;
            activeEnemyPositionStates[index] = true;
        }

        public void ClearActiveEnemyPosition(int index)
        {
            EnsureEnemyCapacity(index);
            activeEnemyPositionStates[index] = false;
        }

    }
}
