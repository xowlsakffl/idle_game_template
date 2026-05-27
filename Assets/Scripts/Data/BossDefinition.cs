using System;

namespace IdleGame.Data
{
    [Serializable]
    public sealed class BossDefinition
    {
        public BossDefinition(string id, string displayName, int baseHp, float timeLimitSeconds, int clearGold)
        {
            Id = id;
            DisplayName = displayName;
            BaseHp = baseHp;
            TimeLimitSeconds = timeLimitSeconds;
            ClearGold = clearGold;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int BaseHp { get; }
        public float TimeLimitSeconds { get; }
        public int ClearGold { get; }
    }
}
