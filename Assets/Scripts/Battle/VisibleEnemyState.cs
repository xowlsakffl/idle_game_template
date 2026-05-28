using IdleGame.Data;
using UnityEngine;

namespace IdleGame.Battle
{
    internal sealed class VisibleEnemyState
    {
        public VisibleEnemyState(
            int spawnSequence,
            GameNumber maxHp,
            int displayNumber,
            float spawnGraceSeconds,
            Vector2 spawnPosition,
            float attackIntervalSeconds)
        {
            SpawnSequence = spawnSequence;
            MaxHp = maxHp;
            Hp = maxHp;
            DisplayNumber = displayNumber;
            SpawnGraceRemaining = Mathf.Max(0f, spawnGraceSeconds);
            Position = spawnPosition;
            SpawnPosition = spawnPosition;
            AttackCooldown = attackIntervalSeconds * (0.35f + 0.05f * (Mathf.Abs(spawnSequence) % 5));
        }

        public int SpawnSequence { get; }
        public GameNumber MaxHp { get; }
        public int DisplayNumber { get; }
        public GameNumber Hp { get; set; }
        public float SpawnGraceRemaining { get; set; }
        public float AttackCooldown { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 SpawnPosition { get; }
        public string TargetHeroId { get; set; } = string.Empty;
        public bool IsAttackable => SpawnGraceRemaining <= 0f && Hp > GameNumber.Zero;
    }
}
