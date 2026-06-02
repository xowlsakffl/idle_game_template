using IdleGame.Data;

namespace IdleGame.Battle
{
    internal readonly struct CombatAttackAction
    {
        public CombatAttackAction(
            bool isValid,
            bool isBossTarget,
            int visibleEnemyIndex,
            GameNumber damage,
            string sourceName,
            bool isCritical,
            string heroId)
        {
            IsValid = isValid;
            IsBossTarget = isBossTarget;
            VisibleEnemyIndex = visibleEnemyIndex;
            Damage = damage;
            SourceName = sourceName ?? string.Empty;
            IsCritical = isCritical;
            HeroId = heroId;
        }

        public bool IsValid { get; }
        public bool IsBossTarget { get; }
        public int VisibleEnemyIndex { get; }
        public GameNumber Damage { get; }
        public string SourceName { get; }
        public bool IsCritical { get; }
        public string HeroId { get; }

        public static CombatAttackAction Invalid { get; } = new CombatAttackAction(
            false,
            false,
            -1,
            GameNumber.Zero,
            string.Empty,
            false,
            null);
    }
}
